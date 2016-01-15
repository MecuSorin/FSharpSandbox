
#r """C:\Users\sorin.mecu\Documents\Visual Studio 2015\Projects\FSharpSandbox\WindowsServicesList\packages\FSharp.Data.2.2.5\lib\net40\FSharp.Data.dll"""

open FSharp.Data
open System.Diagnostics
open System.Text.RegularExpressions
open System.Collections.Generic
open System.Xml.Serialization
open System.IO
open System

type ServiceDesc = { Name: string; DisplayName: string; Default: string; Description: string; DependOn: string[]; UsedBy: string[]; mutable Rank: int option }

[<CLIMutable>]
type ServiceDescription = {Name: string; PrettyName: string; Default: string; Description: string; DependOn: string[]; UsedBy: string[]; Rank: int }

[<Literal>]
let SourceListPage = """http://www.blackviper.com/service-configurations/black-vipers-windows-7-service-pack-1-service-configurations""" 

[<Literal>]
let ServiceDescriptionPageSample = """http://www.blackviper.com/windows-services/tcpip-netbios-helper"""

type ServicesPage = HtmlProvider<SourceListPage>
type ServiceDescriptionPage = HtmlProvider<ServiceDescriptionPageSample>

let obtainServiceDescription (fromLink : string) = 
    if String.IsNullOrEmpty fromLink then String.Empty
    else
        ServiceDescriptionPage.Load(fromLink).Html.Descendants( fun x -> (x.HasId("Windows-8") || x.HasId("Windows-7"))  && x.HasName("div")) 
        |> Seq.head
        |> fun i-> i.Descendants("p")
        |> Seq.head 
        |> fun p-> p.InnerText()

let runAndGetOutput commandAndServiceName = 
    let p = new Process()
    // Redirect the output stream of the child process.
    p.StartInfo.UseShellExecute <- false
    p.StartInfo.RedirectStandardOutput <- true
    p.StartInfo.FileName <- "sc.exe"
    p.StartInfo.Arguments <- commandAndServiceName
    p.Start() |> ignore
    let output = p.StandardOutput.ReadToEnd()
    p.WaitForExit()
    output

let getNameAndDependencies serviceName =
    let extractNameAndDependencies serviceInfo = 
        let name = Regex.Match(serviceInfo, """SERVICE_NAME:\s+(\w+)""").Groups.[1].Value
        let m = Regex.Match(serviceInfo, """DEPENDENCIES\s+(.*)\b\s+SERVICE_START_NAME""", RegexOptions.Singleline)
        if m.Success then
            let listOfServices = m.Groups.[1].Value
            let serviceDeps = Regex.Matches(listOfServices, """:\s+(\w+)""", RegexOptions.Singleline)
            if serviceDeps.Count <> 0 then 
                let matches = 
                    [|0..(serviceDeps.Count-1)|]
                    |> Array.map (fun v-> serviceDeps.[v].Groups.[1].Value)
                name, Some matches
            else name, None
        else name, None
    "qc " + serviceName |> runAndGetOutput |> extractNameAndDependencies

let getDependents serviceName =
    let extractDependents serviceInfo =
        let m = Regex.Matches(serviceInfo, """SERVICE_NAME:\s+(\w+)""")
        if m.Count <> 0 then
            let matches = 
                [|0..(m.Count-1)|]
                |> Array.map (fun v-> m.[v].Groups.[1].Value) 
            Some matches
        else None
    "EnumDepend " + serviceName + " 80000" |> runAndGetOutput |> extractDependents

let flattenOption op = match op with |None -> [||] |Some t-> t

let toString aa = 
    match aa with
    |Some vv -> vv |> Array.sort |> String.concat ", "
    |_ -> "---"



let servicesPage = ServicesPage.Load(SourceListPage).Tables.``Service Default Registry Entries``
let links = 
    servicesPage.Html.Descendants["a"] 
    |> Seq.choose (fun x->
        x.TryGetAttribute("href")
        |> Option.map (fun a -> a.Value()))
    |> Seq.toList

let servicesDictionary = new Dictionary<string, ServiceDesc>()

let allServices =
    servicesPage.Rows
    |> Array.sortBy(fun r-> r.``Service Name (Registry)``)
    |> Array.mapi (fun i row->
        printfn "%s" row.``Service Name (Registry)``
        let name,dependencies = getNameAndDependencies row.``Service Name (Registry)``
        let dependants = getDependents row.``Service Name (Registry)``
        let serviceDescription = obtainServiceDescription links.[i]
        let service = {
            Name = (if String.IsNullOrEmpty name then row.``Service Name (Registry)`` else name);
            DisplayName = row.``Display Name``;
            Default = row.``DEFAULT Professional``;
            Description = serviceDescription;
            DependOn = flattenOption dependencies;
            UsedBy = flattenOption dependants;
            Rank = None
        }
        servicesDictionary.[service.Name.ToLower()] <- service
        service)

let doRankEvaluation () =
    let rec evaluateRank (serviceKey:string) =
        match  servicesDictionary.TryGetValue(serviceKey.ToLower()) with
        | (true, service) ->
            let rank = 
                match service.Rank with
                |None ->
                    match service.DependOn with
                    | [||] -> 1
                    | _ ->
                        service.DependOn
                        |> Array.map evaluateRank
                        |> Array.max
                        |> fun i -> 1+i
                |Some v -> v
            service.Rank <- Some rank  
            rank     
        | _ ->
            printfn "Not found service: %s" serviceKey
            1000
    for key in servicesDictionary.Keys do
        evaluateRank key |> ignore

let save () =
    let sw = new StreamWriter("""C:\Work\services.xml""")
    let serializer = new XmlSerializer(typeof<ServiceDescription[]>)
    let toPrint = 
        allServices
        |> Array.map (fun s-> { Name = s.Name; PrettyName = s.DisplayName; Default = s.Default; Rank = (match s.Rank with |Some t-> t |_ ->2000); DependOn = s.DependOn; UsedBy = s.UsedBy; Description = s.Description})
        |> Array.sortBy (fun s-> s.Rank)
    serializer.Serialize(sw, toPrint)
    sw.Close()

doRankEvaluation()
save()
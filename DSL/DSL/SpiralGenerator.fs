namespace DSL

module Sample =
    let path = "pseudoFractal.html"
    let svgLine (x1,y1,x2,y2) color= sprintf """<line x1="%.1f" y1="%.1f" x2="%.1f" y2="%.1f" stroke="%s" />""" x1 y1 x2 y2 color
    let CANVAS_HALF_SIZE = 1000

    let inTemplate content = 
        sprintf """
    <html>
    <body>
        <h1>F# snail!</h1>
        <svg width="%d" height="%d">
            %s
        </svg>
    </body>
    </html>""" (2*CANVAS_HALF_SIZE) (2*CANVAS_HALF_SIZE) content

    type POINT = { X: float; Y:float }

    let alongate (mobiles: Mobil list) =
        let dx = mobiles.[1].X - mobiles.[0].X
        let dy = mobiles.[1].Y - mobiles.[0].Y
        {X = mobiles.[1].X + dx; Y= mobiles.[1].Y + dy }
    
    // use this to generate a html by providing a list of commands
    let runFractal commands startingMobil = 
        let run =
            startingMobil
            |> cmd.execute commands
            |> List.windowed 3
            |> List.map (fun  tL -> 
                let main = svgLine (tL.[0].X, tL.[0].Y, tL.[1].X, tL.[1].Y) "green"
                let out = tL |> alongate
                let second = svgLine (tL.[1].X, tL.[1].Y, out.X, out.Y) "red"
                [main; second])
            |> List.concat
            |> String.concat "\n"
        let commandsHTML = inTemplate run
        // opens the browser with the generated html
        do System.IO.File.WriteAllText(path, commandsHTML)
        System.Diagnostics.Process.Start(path) |> ignore 
        true

    // sample 
    let GenerateSample _ = 
        let startingMobile = { Angle =0.; Momentum =1.; LastMove=1.; X= (float)CANVAS_HALF_SIZE; Y= (float)CANVAS_HALF_SIZE }
        let fractal = [ FORWARD 2.; MOMENTUM 1.05; REPEAT(220, [TURN 35.; GO])]
        startingMobile |> runFractal fractal |> ignore
    
    let Generate (text:string) = 
        let startingMobile = { Angle =0.; Momentum =1.; LastMove=1.; X= (float)CANVAS_HALF_SIZE; Y= (float)CANVAS_HALF_SIZE }
        let generateFractal cmds = 
            runFractal cmds startingMobile |> ignore
            true
        let showError msg =
            printfn "Failure: %s" msg
            false
        parser.Parse text generateFractal showError
    
    // sample 
    let Test _ = 
        Generate """[ FORWARD 2; MOMENTUM 1.05; REPEAT(220, [TURN 325; GO])]"""

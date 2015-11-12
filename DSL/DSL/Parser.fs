namespace DSL
open FParsec

module parser =
    // direct implementation of grammar
    let ForwardParser = spaces >>. skipStringCI "forward" >>. spaces >>. pfloat |>> FORWARD
    let GoParser = spaces >>. skipStringCI "go" |>> fun _ -> GO
    let TurnParser = spaces >>. skipStringCI "turn" >>. spaces >>. pfloat |>> TURN
    let MomentumParser = spaces >>. skipStringCI "momentum" >>. spaces >>. pfloat |>> MOMENTUM

    //helper for integers bigger than 0
    let ppositive (failText:string) :Parser<int32,'u> = 
        pint32 
        >>= fun nr -> if nr > 0 then (preturn nr) else fail failText

    // because the RepeatParser is recursive we must treat it with respect
    let genericCommandParserDummy, updatableGenericCommandParser = createParserForwardedToRef<COMMAND, unit>()

    let RepeatParser = 
        (spaces >>. skipStringCI "repeat" >>. spaces >>. skipString "(" >>. (ppositive "Only positive number of repeats is accepted")) .>>. 
            (spaces >>. skipString "," >>. spaces >>. between (pstring "[") (pstring "]")
                (spaces >>. sepBy (genericCommandParserDummy .>> spaces) (pstring ";" >>. spaces))) .>> skipString ")" |>> REPEAT
    // once we have now the RepeatParser we can update the genericCommandParser
    do updatableGenericCommandParser := choice[ForwardParser; TurnParser; GoParser; MomentumParser; RepeatParser]

    // helpers for lists of commands
    let parserBetweenBrackets = between (spaces >>. pstring "[" .>> spaces) (spaces >>. pstring "]" .>> spaces)
    let parserListSeparatedBySemicolon listOfParsers= spaces >>. sepBy (listOfParsers .>> spaces) (pstring ";" >>. spaces)
    let parserList =  parserBetweenBrackets << parserListSeparatedBySemicolon

    // the usable string parser lies below RIP :P
    let COMMANDParser = spaces >>. (parserList genericCommandParserDummy) .>> spaces .>> eof

    let Parse (text:string) (onSuccess:COMMAND list -> bool) (onFail:string -> bool) =
        match run COMMANDParser text with
        | Success(result, _, _) -> onSuccess result     // printfn "Success: %A" result
        | Failure(errorMsg, _, _) -> onFail errorMsg    // printfn "Failure: %s" errorMsg
namespace DSL

type COMMAND =
    | FORWARD of float              // moves in current direction the distance provided
    | GO                            // moves in current direction the last distance pondered with the last MOMENTUM applied
    | TURN of float                 // turns from current position number of degrees
    | MOMENTUM of float             // updates the momentum
    | REPEAT of int * COMMAND list  // repeat a sequence of commands

module cmd =
    let rec execute (program: COMMAND list) (turtle: Mobil) = 
        match  program with
        | [] -> [turtle]
        | head::tail ->
            match head with
            | FORWARD(distance) ->
                let nextTurtle = turtle |> mobil.forward distance
                nextTurtle::( nextTurtle |> execute tail)
            | GO ->
                let nextTurtle = turtle |> mobil.go
                nextTurtle::( nextTurtle |> execute tail)
            | TURN(angle) ->
                let nextTurtle = turtle |> mobil.turn angle
                nextTurtle::( nextTurtle |> execute tail)
            | MOMENTUM(ponder) ->
                let nextTurtle = turtle |> mobil.momentum ponder
                nextTurtle |> execute tail
            | REPEAT(times,procedure) ->
                if (times<1) then failwith "Invalid number of times to repeat"
                let commands = System.Linq.Enumerable.Repeat(procedure, times) |> Seq.toList
                let allCommands = 
                    commands @ [tail] 
                    |> List.concat
                turtle |> execute allCommands
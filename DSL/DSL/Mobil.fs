namespace DSL

type Mobil = { Angle:float; Momentum:float; LastMove:float; X:float; Y:float }

module mobil =
    let toRadians angle = angle * 2.0 * System.Math.PI / 360.0
    let forward distance (mobil: Mobil) =
        let degrees =  mobil.Angle |> toRadians
        { mobil with
            X = mobil.X + distance * cos (degrees)
            Y = mobil.Y + distance * sin (degrees)
            LastMove = distance }

    let go (mobil:Mobil) = mobil |> forward (mobil.LastMove*mobil.Momentum)
    let turn angle (mobil: Mobil) =
        { mobil with
            Angle = (mobil.Angle + angle  ) % 360.0 }
    let momentum ponder (mobil: Mobil) =
        { mobil with
            Momentum = ponder }
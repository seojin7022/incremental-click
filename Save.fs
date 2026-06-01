module IncrementalClick.Save

open System.IO
open System.Text.Json
open IncrementalClick.Types

[<CLIMutable>]
type SaveData = {
    Points: float
    UnlockedIds: string[]
    CameraX: float32
    CameraY: float32
    Zoom: float32
}

let private path = "save.json"

let save (state: GameState) (camX: float32) (camY: float32) (zoom: float32) =
    let data = {
        Points = state.Points
        UnlockedIds = state.UnlockedIds |> Set.toArray
        CameraX = camX
        CameraY = camY
        Zoom = zoom
    }
    let opts = JsonSerializerOptions(WriteIndented = true)
    let json = JsonSerializer.Serialize(data, opts)
    File.WriteAllText(path, json)

let tryLoad () : SaveData option =
    if File.Exists path then
        try
            let json = File.ReadAllText path
            let data = JsonSerializer.Deserialize<SaveData>(json)
            Some data
        with _ -> None
    else
        None

module IncrementalClick.Program
open IncrementalClick
open IncrementalClick.Constants
open IncrementalClick.Types
open IncrementalClick.Tree
open System
open System.Numerics
open Raylib_cs

let formatNum (v: float) =
    let av = if v < 0.0 then -v else v
    if av >= 1e12 then sprintf "%.2fT" (v / 1e12)
    elif av >= 1e9 then sprintf "%.2fB" (v / 1e9)
    elif av >= 1e6 then sprintf "%.2fM" (v / 1e6)
    elif av >= 1e3 then sprintf "%.2fK" (v / 1e3)
    elif av >= 10.0 then sprintf "%.0f" v
    else sprintf "%.1f" v

[<EntryPoint>]
let main _ = 
    Render.init ()
    let state = GameState ()
    let mutable camTarget = Vector2(400f, 0f)
    let mutable camZoom = 0.7f

    let mutable camera =
        Camera2D(
            Vector2(float32 (TreeX + (WIDTH - TreeX) / 2), float32 (TreeY + (HEIGHT - TreeY) / 2)), 
            camTarget, 
            0f, 
            camZoom
        )

    let btnRect = Rectangle(60f, 180f, 400f, 400f)
    let mutable hoveredNode: Node option = None
    let mutable dragging = false

    while not (Raylib.WindowShouldClose ()) do
        let dt = Raylib.GetFrameTime ()
        
        if state.Passive > 0.0 then
            state.Points <- state.Points + state.Passive * float dt

        Raylib.BeginDrawing()
        Raylib.ClearBackground(Color(20, 22, 32, 255))

        Raylib.DrawRectangle(0, 0, WIDTH, TreeY, Color(12, 14, 22, 255))
        Raylib.DrawText(sprintf "Points: %s" (formatNum state.Points), 20, 16, 30, Color.White)
        Raylib.DrawText(
            sprintf "Click Power: %s" (formatNum state.ClickPower),
            440, 8, 18, Color.Yellow)
        Raylib.DrawText(
            sprintf "Passive: %s /s" (formatNum state.Passive),
            440, 32, 18, Color.SkyBlue)
        Raylib.DrawText(
            sprintf "Unlocks: %d / %d" state.UnlockedIds.Count (List.length allNodes),
            760, 20, 18, Color.LightGray)
        Raylib.DrawLine(0, TreeY, WIDTH, TreeY, Color.Gray)

        Raylib.EndDrawing ()
    
    Raylib.CloseWindow ()
    0
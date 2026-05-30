module IncrementalClick.Render

open Raylib_cs
open IncrementalClick.Constants

let init () = 
    Raylib.InitWindow (WIDTH, HEIGHT, TITLE)
    Raylib.SetTargetFPS FPS
module IncrementalClick.Render

open System
open System.IO
open Raylib_cs
open IncrementalClick.Constants

let inline private rb (x: CBool) : bool = CBool.op_Implicit x

type UiFont = {
    Font: Font
    ShouldUnload: bool
}

let init () = 
    Raylib.InitWindow (WIDTH, HEIGHT, TITLE)
    Raylib.SetTargetFPS FPS

let loadUiFont () =
    let fontPathCandidates = [
        Path.Combine(AppContext.BaseDirectory, "asset", "font", "NotoSans-Regular.ttf")
        Path.Combine(Directory.GetCurrentDirectory(), "asset", "font", "NotoSans-Regular.ttf")
    ]

    match fontPathCandidates |> List.tryFind File.Exists with
    | Some path ->
        let font = Raylib.LoadFontEx(path, 64, [||], 0)
        if rb (Raylib.IsFontValid font) then
            Raylib.SetTextureFilter(font.Texture, TextureFilter.Bilinear)
            { Font = font; ShouldUnload = true }
        else
            failwithf "Failed to load UI font: %s" path
    | None ->
        failwith "Missing UI font: asset/font/NotoSans-Regular.ttf"

let unloadUiFont (font: UiFont) =
    if font.ShouldUnload then
        Raylib.UnloadFont font.Font

module IncrementalClick.Program
open IncrementalClick
open IncrementalClick.Constants
open IncrementalClick.Types
open IncrementalClick.Tree
open IncrementalClick.Save
open System
open System.Numerics
open Raylib_cs

let inline rb (x: CBool) : bool = CBool.op_Implicit x

let recompute (state: GameState) =
    let mutable cAdd = 1.0
    let mutable cMul = 1.0
    let mutable pAdd = 0.0
    let mutable pMul = 1.0
    for n in allNodes do
        if state.UnlockedIds.Contains n.Id then
            cAdd <- cAdd + n.Effect.AddClick
            cMul <- cMul * n.Effect.MulClick
            pAdd <- pAdd + n.Effect.AddPassive
            pMul <- pMul * n.Effect.MulPassive
    state.ClickPower <- cAdd * cMul
    state.Passive <- pAdd * pMul

let isAvailable (state: GameState) (n: Node) =
    not (state.UnlockedIds.Contains n.Id)
    && n.Prereqs |> List.forall state.UnlockedIds.Contains

let isVisible (state: GameState) (n: Node) =
    state.UnlockedIds.Contains n.Id
    || n.Prereqs.IsEmpty
    || n.Prereqs |> List.exists state.UnlockedIds.Contains

let formatNum (v: float) =
    let av = if v < 0.0 then -v else v
    if av >= 1e12 then sprintf "%.2fT" (v / 1e12)
    elif av >= 1e9 then sprintf "%.2fB" (v / 1e9)
    elif av >= 1e6 then sprintf "%.2fM" (v / 1e6)
    elif av >= 1e3 then sprintf "%.2fK" (v / 1e3)
    elif av >= 10.0 then sprintf "%.0f" v
    else sprintf "%.1f" v

let effectDescription (e: Effect) =
    [
        if e.AddClick <> 0.0 then sprintf "+%g click power" e.AddClick
        if e.AddPassive <> 0.0 then sprintf "+%g passive/s" e.AddPassive
        if e.MulClick <> 1.0 then sprintf "x%g click power" e.MulClick
        if e.MulPassive <> 1.0 then sprintf "x%g passive" e.MulPassive
    ]
    |> String.concat ", "

let nodeUnderMouse (state: GameState) (world: Vector2) : Node option =
    allNodes
    |> List.tryFind (fun n ->
        if isVisible state n then
            let dx = world.X - n.X
            let dy = world.Y - n.Y
            dx * dx + dy * dy <= NODE_R * NODE_R
        else
            false)

let textSpacing (size: float32) =
    max 1f (size * 0.05f)

let measureText (font: Font) (text: string) (size: float32) =
    Raylib.MeasureTextEx(font, text, size, textSpacing size).X

let drawText (font: Font) (text: string) (x: float32) (y: float32) (size: float32) (color: Color) =
    Raylib.DrawTextEx(font, text, Vector2(x, y), size, textSpacing size, color)

let drawCenteredText (font: Font) (text: string) (centerX: float32) (y: float32) (size: float32) (color: Color) =
    let width = measureText font text size
    drawText font text (centerX - width / 2f) y size color

let ellipsizeText (font: Font) (text: string) (size: float32) (maxWidth: float32) =
    if measureText font text size <= maxWidth then
        text
    else
        let ellipsis = "..."
        let mutable endIndex = text.Length
        while endIndex > 0
              && measureText font (text.Substring(0, endIndex) + ellipsis) size > maxWidth do
            endIndex <- endIndex - 1
        if endIndex <= 0 then ellipsis else text.Substring(0, endIndex) + ellipsis

let fitOneLine (font: Font) (text: string) (maxWidth: float32) (preferredSize: float32) (minSize: float32) =
    let mutable size = preferredSize
    while size > minSize && measureText font text size > maxWidth do
        size <- size - 1f
    ellipsizeText font text size maxWidth, size

let drawNodeLabel (font: Font) (state: GameState) (n: Node) =
    let unlocked = state.UnlockedIds.Contains n.Id
    let available = isAvailable state n
    let canAfford = available && state.Points >= n.Cost
    let maxLabelWidth = 118f
    let name, nameSize = fitOneLine font n.Name maxLabelWidth 13f 9f
    let cost = if unlocked then "" else sprintf "%s pts" (formatNum n.Cost)
    let costSize = 10f
    let nameWidth = measureText font name nameSize
    let costWidth = if unlocked then 0f else measureText font cost costSize
    let labelWidth = max 72f (min 132f (max nameWidth costWidth + 14f))
    let labelHeight = if unlocked then 23f else 38f
    let labelX = n.X - labelWidth / 2f
    let labelY = n.Y + NODE_R + 6f
    let fill =
        if unlocked then Color(22, 70, 42, 235)
        elif canAfford then Color(90, 74, 26, 235)
        elif available then Color(62, 53, 34, 235)
        else Color(38, 40, 54, 235)
    let outline =
        if unlocked then Color(96, 220, 132, 230)
        elif canAfford then Color(255, 224, 120, 230)
        else Color(96, 98, 116, 220)

    Raylib.DrawRectangleRounded(Rectangle(labelX, labelY, labelWidth, labelHeight), 0.25f, 8, fill)
    Raylib.DrawRectangleRoundedLinesEx(Rectangle(labelX, labelY, labelWidth, labelHeight), 0.25f, 8, 1.2f, outline)
    drawCenteredText font name n.X (labelY + 4f) nameSize Color.White
    if not unlocked then
        drawCenteredText font cost n.X (labelY + 21f) costSize (Color(218, 222, 232, 255))

[<EntryPoint>]
let main _ = 
    Render.init ()
    let uiFont = Render.loadUiFont ()
    let state = GameState ()
    let mutable camTarget = Vector2(400f, 0f)
    let mutable camZoom = 0.7f

    match Save.tryLoad () with
    | Some d ->
        state.Points <- d.Points
        state.UnlockedIds <-
            if isNull d.UnlockedIds then Set.empty else Set.ofArray d.UnlockedIds
        if d.Zoom > 0f then camZoom <- d.Zoom
        camTarget <- Vector2(d.CameraX, d.CameraY)
        recompute state
    | None -> ()

    let mutable camera =
        Camera2D(
            Vector2(float32 (TREE_X + (WIDTH - TREE_X) / 2), float32 (TREE_Y + (HEIGHT - TREE_Y) / 2)), 
            camTarget, 
            0f, 
            camZoom
        )

    let btnRect = Rectangle(60f, 180f, 400f, 400f)
    let mutable hoveredNode: Node option = None
    let mutable dragging = false

    while not (rb (Raylib.WindowShouldClose ())) do
        let dt = Raylib.GetFrameTime ()
        
        if state.Passive > 0.0 then
            state.Points <- state.Points + state.Passive * float dt


        let mouse = Raylib.GetMousePosition()
        let mouseInTree =
            mouse.X >= float32 TREE_X
            && mouse.Y >= float32 TREE_Y
            && mouse.X < float32 WIDTH
            && mouse.Y < float32 HEIGHT

        // ---- input ----
        // wheel zoom (zoom around the cursor)
        if mouseInTree then
            let wheel = Raylib.GetMouseWheelMove()
            if wheel <> 0f then
                let worldBefore = Raylib.GetScreenToWorld2D(mouse, camera)
                camZoom <- max 0.25f (min 3.0f (camZoom + wheel * 0.1f))
                camera.Zoom <- camZoom
                let worldAfter = Raylib.GetScreenToWorld2D(mouse, camera)
                camTarget <- camTarget + (worldBefore - worldAfter)
                camera.Target <- camTarget

        // right/middle drag pan
        if mouseInTree
           && (rb (Raylib.IsMouseButtonDown MouseButton.Right)
               || rb (Raylib.IsMouseButtonDown MouseButton.Middle)) then
            let d = Raylib.GetMouseDelta()
            camTarget <- camTarget - (d / camZoom)
            camera.Target <- camTarget

        // left mouse: click button, purchase node, or start tree pan
        if rb (Raylib.IsMouseButtonPressed MouseButton.Left) then
            if rb (Raylib.CheckCollisionPointRec(mouse, btnRect)) then
                state.Points <- state.Points + state.ClickPower
            elif mouseInTree then
                let world = Raylib.GetScreenToWorld2D(mouse, camera)
                match nodeUnderMouse state world with
                | Some n when isAvailable state n && state.Points >= n.Cost ->
                    state.Points <- state.Points - n.Cost
                    state.UnlockedIds <- state.UnlockedIds.Add n.Id
                    recompute state
                | Some _ -> ()
                | None -> dragging <- true

        if rb (Raylib.IsMouseButtonReleased MouseButton.Left) then
            dragging <- false

        if dragging && rb (Raylib.IsMouseButtonDown MouseButton.Left) then
            let d = Raylib.GetMouseDelta()
            camTarget <- camTarget - (d / camZoom)
            camera.Target <- camTarget

        // hover (each frame, for tooltip)
        hoveredNode <-
            if mouseInTree then
                let world = Raylib.GetScreenToWorld2D(mouse, camera)
                nodeUnderMouse state world
            else
                None

        Raylib.BeginDrawing()
        Raylib.ClearBackground(Color(20, 22, 32, 255))

        Raylib.DrawRectangle(0, 0, WIDTH, TREE_Y, Color(12, 14, 22, 255))
        drawText uiFont.Font (sprintf "Points: %s" (formatNum state.Points)) 20f 13f 30f Color.White
        drawText uiFont.Font (sprintf "Click Power: %s" (formatNum state.ClickPower)) 440f 7f 18f Color.Yellow
        drawText uiFont.Font (sprintf "Passive: %s /s" (formatNum state.Passive)) 440f 32f 18f Color.SkyBlue
        drawText uiFont.Font (sprintf "Unlocks: %d / %d" state.UnlockedIds.Count (List.length allNodes)) 760f 20f 18f Color.LightGray
        Raylib.DrawLine(0, TREE_Y, WIDTH, TREE_Y, Color.Gray)

        // left click button
        let hoverBtn = rb (Raylib.CheckCollisionPointRec(mouse, btnRect))
        let downBtn = hoverBtn && rb (Raylib.IsMouseButtonDown MouseButton.Left)
        let btnColor =
            if downBtn then Color(170, 55, 65, 255)
            elif hoverBtn then Color(230, 110, 120, 255)
            else Color(200, 80, 90, 255)
        Raylib.DrawRectangleRec(btnRect, btnColor)
        Raylib.DrawRectangleLinesEx(btnRect, 4f, Color.White)
        let btnText = "CLICK!"
        let btnTextW = measureText uiFont.Font btnText 60f
        drawText uiFont.Font btnText (btnRect.X + (btnRect.Width - btnTextW) / 2f) (btnRect.Y + 158f) 60f Color.White
        let perClick = sprintf "+%s per click" (formatNum state.ClickPower)
        let perW = measureText uiFont.Font perClick 22f
        drawText uiFont.Font perClick (btnRect.X + (btnRect.Width - perW) / 2f) (btnRect.Y + 240f) 22f Color.White

        // tree panel background
        Raylib.DrawRectangle(TREE_X, TREE_Y, TREE_W, TREE_H, Color(28, 30, 42, 255))

        Raylib.BeginScissorMode(TREE_X, TREE_Y, TREE_W, TREE_H)
        Raylib.BeginMode2D(camera)

        // edges
        for n in allNodes do
            if isVisible state n then
                for p in n.Prereqs do
                    match nodeById.TryFind p with
                    | Some parent when isVisible state parent ->
                        let bothOwned =
                            state.UnlockedIds.Contains parent.Id
                            && state.UnlockedIds.Contains n.Id
                        let parentOwned = state.UnlockedIds.Contains parent.Id
                        let color =
                            if bothOwned then Color(120, 220, 140, 220)
                            elif parentOwned then Color(220, 200, 110, 200)
                            else Color(90, 90, 110, 180)
                        Raylib.DrawLineEx(
                            Vector2(parent.X, parent.Y),
                            Vector2(n.X, n.Y),
                            3f,
                            color)
                    | _ -> ()

        // nodes
        for n in allNodes do
            if isVisible state n then
                let unlocked = state.UnlockedIds.Contains n.Id
                let available = isAvailable state n
                let canAfford = available && state.Points >= n.Cost
                let baseColor =
                    if unlocked then Color(70, 200, 110, 255)
                    elif canAfford then Color(240, 210, 100, 255)
                    elif available then Color(150, 130, 80, 255)
                    else Color(80, 80, 95, 255)
                Raylib.DrawCircleV(Vector2(n.X, n.Y), NODE_R, baseColor)
                let ring =
                    if unlocked then Color(40, 120, 60, 255)
                    elif canAfford then Color(255, 240, 160, 255)
                    else Color(40, 40, 55, 255)
                Raylib.DrawCircleLinesV(Vector2(n.X, n.Y), NODE_R, ring)
                drawNodeLabel uiFont.Font state n

        Raylib.EndMode2D()
        Raylib.EndScissorMode()

        // border between panels
        Raylib.DrawLine(TREE_X, TREE_Y, TREE_X, HEIGHT, Color.Gray)

        // tooltip
        match hoveredNode with
        | Some n ->
            let statusLine =
                if state.UnlockedIds.Contains n.Id then "[OWNED]"
                elif isAvailable state n then
                    if state.Points >= n.Cost then "[Click to buy]"
                    else "[Need more points]"
                else "[Locked]"
            let lines = [
                n.Name
                sprintf "Cost: %s" (formatNum n.Cost)
                effectDescription n.Effect
                statusLine
            ]
            let pad = 8
            let lineH = 18
            let fontSize = 14
            let maxW =
                lines
                |> List.map (fun l -> measureText uiFont.Font l (float32 fontSize))
                |> List.max
            let w = int maxW + pad * 2 + 2
            let h = (List.length lines) * lineH + pad * 2
            let tx0 = int mouse.X + 18
            let ty0 = int mouse.Y + 18
            let tx = if tx0 + w > WIDTH then int mouse.X - w - 18 else tx0
            let ty = if ty0 + h > HEIGHT then int mouse.Y - h - 18 else ty0
            Raylib.DrawRectangle(tx, ty, w, h, Color(0, 0, 0, 220))
            Raylib.DrawRectangleLines(tx, ty, w, h, Color.White)
            lines
            |> List.iteri (fun i l ->
                drawText uiFont.Font l (float32 (tx + pad)) (float32 (ty + pad + i * lineH)) (float32 fontSize) Color.White)
        | None -> ()

        // help
        drawText uiFont.Font "Drag (left/right) to pan  -  Scroll to zoom  -  Left-click node to buy" (float32 (TREE_X + 10)) (float32 (HEIGHT - 23)) 14f Color.LightGray

        Raylib.EndDrawing ()
    
    Save.save state camTarget.X camTarget.Y camZoom
    Render.unloadUiFont uiFont
    Raylib.CloseWindow ()
    0

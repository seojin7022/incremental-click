module IncrementalClick.Tree
open IncrementalClick.Types

let allNodes : Node list = [
    { Id = "start"; Name = "Awaken"; Cost = 10.0;
      Prereqs = []; Effect = Effect.addClick 1.0;
      X = 0f; Y = 0f }

    // Click branch (up-right)
    { Id = "click1"; Name = "Click II"; Cost = 50.0;
      Prereqs = ["start"]; Effect = Effect.addClick 2.0;
      X = 200f; Y = -130f }
    { Id = "click2"; Name = "Click III"; Cost = 200.0;
      Prereqs = ["click1"]; Effect = Effect.addClick 3.0;
      X = 380f; Y = -260f }
    { Id = "click3"; Name = "Click IV"; Cost = 800.0;
      Prereqs = ["click2"]; Effect = Effect.addClick 5.0;
      X = 560f; Y = -390f }
    { Id = "click4"; Name = "Click x2"; Cost = 3000.0;
      Prereqs = ["click3"]; Effect = Effect.mulClick 2.0;
      X = 740f; Y = -520f }
    { Id = "click5"; Name = "Click +20"; Cost = 15000.0;
      Prereqs = ["click4"]; Effect = Effect.addClick 20.0;
      X = 920f; Y = -650f }
    { Id = "click6"; Name = "Click x3"; Cost = 80000.0;
      Prereqs = ["click5"]; Effect = Effect.mulClick 3.0;
      X = 1100f; Y = -780f }

    // Passive branch (down-right)
    { Id = "auto1"; Name = "Auto-Clicker"; Cost = 25.0;
      Prereqs = ["start"]; Effect = Effect.addPassive 1.0;
      X = 200f; Y = 130f }
    { Id = "auto2"; Name = "Factory"; Cost = 150.0;
      Prereqs = ["auto1"]; Effect = Effect.addPassive 3.0;
      X = 380f; Y = 260f }
    { Id = "auto3"; Name = "Mega Factory"; Cost = 700.0;
      Prereqs = ["auto2"]; Effect = Effect.addPassive 8.0;
      X = 560f; Y = 390f }
    { Id = "auto4"; Name = "Passive x2"; Cost = 2500.0;
      Prereqs = ["auto3"]; Effect = Effect.mulPassive 2.0;
      X = 740f; Y = 520f }
    { Id = "auto5"; Name = "Quantum"; Cost = 12000.0;
      Prereqs = ["auto4"]; Effect = Effect.addPassive 50.0;
      X = 920f; Y = 650f }
    { Id = "auto6"; Name = "Galaxy"; Cost = 60000.0;
      Prereqs = ["auto5"]; Effect = Effect.addPassive 200.0;
      X = 1100f; Y = 780f }
    { Id = "auto7"; Name = "Passive x3"; Cost = 300000.0;
      Prereqs = ["auto6"]; Effect = Effect.mulPassive 3.0;
      X = 1280f; Y = 910f }

    // Synergy branch (middle)
    { Id = "syn1"; Name = "Combo"; Cost = 1000.0;
      Prereqs = ["click1"; "auto1"]; Effect = Effect.combo 5.0 5.0;
      X = 420f; Y = 0f }
    { Id = "syn2"; Name = "Harmony"; Cost = 8000.0;
      Prereqs = ["syn1"]; Effect = Effect.mulBoth 1.5;
      X = 620f; Y = 0f }
    { Id = "syn3"; Name = "Resonance"; Cost = 50000.0;
      Prereqs = ["syn2"]; Effect = Effect.combo 50.0 50.0;
      X = 820f; Y = 0f }

    // Endgame
    { Id = "pres1"; Name = "Ascend Click"; Cost = 500000.0;
      Prereqs = ["click6"; "syn3"]; Effect = Effect.mulClick 3.0;
      X = 1280f; Y = -220f }
    { Id = "pres2"; Name = "Ascend Flow"; Cost = 500000.0;
      Prereqs = ["auto7"; "syn3"]; Effect = Effect.mulPassive 3.0;
      X = 1280f; Y = 220f }
    { Id = "pres3"; Name = "Singularity"; Cost = 5000000.0;
      Prereqs = ["pres1"; "pres2"]; Effect = Effect.mulBoth 5.0;
      X = 1480f; Y = 0f }
    { Id = "pres4"; Name = "Big Bang"; Cost = 50000000.0;
      Prereqs = ["pres3"]; Effect = Effect.mulBoth 10.0;
      X = 1680f; Y = 0f }
]

let nodeById : Map<string, Node> =
    allNodes |> List.map (fun n -> n.Id, n) |> Map.ofList
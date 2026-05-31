module IncrementalClick.Tree
open IncrementalClick.Types

let allNodes: Node list = [
    { Id = "start"; Name = "Awaken"; Cost = 10.0; 
    Prereqs = []; Effect = Effect.addClick 1.0; X = 0f; Y = 0f}
]
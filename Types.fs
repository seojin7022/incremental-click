module IncrementalClick.Types

type Effect = {
    AddClick: float
    AddPassive: float
    MulClick: float
    MulPassive: float
}

module Effect =
    let empty = {AddClick = 0.0; AddPassive = 0.0; MulClick = 0.0; MulPassive = 0.0}
    
    let addClick v = {empty with AddClick=v}
    let addPassive v = {empty with AddPassive=v}
    let mulClick v = {empty with MulClick=v}
    let mulPassive v = {empty with MulPassive=v}
    let combo c p = {empty with AddClick=c; AddPassive=p}
    let mulBoth v = {empty with MulClick=v; MulPassive=v}

type Node = {
    Id: string
    Name: string
    Cost: float
    Prereqs: string list
    Effect: Effect
    X: float32
    Y: float32
}

type GameState = 
    member _.Points = 0.0
    member _.ClickPower = 1.0
    member _.Passive = 0.0
    member _.UnlockedIds = Set.empty
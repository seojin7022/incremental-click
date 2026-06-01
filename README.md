# Incremental Click

Incremental Click is a 2D incremental clicker game written in F# with .NET 10 and Raylib. The player earns points by clicking a main button and by unlocking passive production upgrades, then spends points on a branching upgrade tree.

## Requirements

- .NET 10 SDK
- A desktop environment that can open a Raylib window
- Internet access on the first run so `dotnet` can restore the `Raylib-cs` NuGet package

## Running the Game

```sh
dotnet restore
dotnet run --project incremental-click.fsproj
```

The game writes progress to `save.json` in the project directory when the window closes and loads that file on startup. To start over on macOS/Linux, run:

```sh
rm -f save.json
```

On Windows PowerShell, use:

```powershell
Remove-Item save.json -ErrorAction SilentlyContinue
```

## Controls

- Left-click the `CLICK!` button to gain points equal to the current click power.
- Left-click an affordable highlighted upgrade node to buy it.
- Drag the upgrade tree with the left mouse button on empty space, or with the right/middle mouse button.
- Scroll the mouse wheel over the tree to zoom in or out.
- Close the window to save progress.

## Gameplay

- The top bar shows current points, click power, passive points per second, and unlocked node count.
- The left panel contains the main click button.
- The right panel contains the upgrade tree.
- Purchased nodes are green, affordable nodes are highlighted, available but unaffordable nodes are muted, and locked visible nodes are gray.
- The tree contains 21 upgrade nodes across click-power, passive-production, synergy, and endgame branches.
- The practical completion goal is to unlock all 21 nodes.

## Proposal Compliance

| Submitted requirement | Final implementation |
| --- | --- |
| Raylib 2D window with point display, click button, and upgrade tree | Implemented |
| Points earned by clicking with current click power, starting at 1 | Implemented |
| 60 FPS game loop with passive points-per-second updates | Implemented |
| Upgrade tree represented as nodes with IDs, costs, prerequisites, effects, and unlock state | Implemented |
| Nodes are purchasable only when prerequisites are unlocked and the player has enough points | Implemented |
| Purchasing deducts cost, unlocks the node, applies its effect, and reveals connected progress | Implemented |
| Tree panning and scroll-wheel zoom | Implemented |
| Save on close and load on startup using a local JSON file | Implemented |
| At least 20 upgrade nodes across multiple branches | Implemented with 21 nodes |

## Requirement Changes

No intentional gameplay requirement changes were made from the submitted requirements document. The final upgrade tree uses concrete node names and numeric balance values in code, but no proposed feature was removed.

## LLM Usage

If an LLM was used while developing this project, replace this template before final submission:

- LLM/system used:
- What I used it for:
- Manual changes or reprompts needed because the LLM did not understand the first prompt:
- Main point that the LLM was not able to do correctly:

# Incremental Click

Incremental Click is a 2D incremental clicker game written in F# with .NET 10 and Raylib. The player earns points by clicking a main button, gains passive points from unlocked upgrades, and spends points on a branching upgrade tree.

The submitted proposal requirements describe a Raylib window with a points display, a central click button, a pannable and zoomable upgrade tree, passive point generation, JSON saving/loading, and at least 20 upgrade nodes. The current implementation follows those requirements and contains 21 upgrade nodes.

## Getting Started

### Prerequisites

- .NET 10 SDK, including the F# compiler.
- A desktop environment that can open a Raylib window.
- Internet access for the first build/run so NuGet can restore `Raylib-cs`.

No separate Raylib installation is required. The project references `Raylib-cs` in `incremental-click.fsproj`, and NuGet restores the managed package and native Raylib binaries automatically.

The game also includes its UI font in `asset/font/NotoSans-Regular.ttf`, so all platforms use the same bundled font instead of depending on system fonts.

### Run

From the project directory:

```sh
dotnet run --project incremental-click.fsproj
```

The first run may take longer because NuGet restores dependencies. Later runs should start quickly.

### Build Only

```sh
dotnet build
```

### Reset Save Data

The game stores progress in `save.json` in the project directory. To start over, delete `save.json` before launching the game again.

## How to Play

### Goal

There is no fail state and no forced victory screen. The practical completion goal is to unlock all 21 upgrade nodes. The top bar shows progress as `Unlocks: current / 21`.

### Screen Layout

- The top bar shows points, click power, passive points per second, and unlocked-node count.
- The left side contains the large `CLICK!` button.
- The right side contains the upgrade tree.
- Hovering over a visible node shows a tooltip with the node name, cost, effect, and purchase status.

### Controls

| Action                               | What it does                                                              |
| ------------------------------------ | ------------------------------------------------------------------------- |
| Left-click `CLICK!`                  | Adds points equal to the current click power.                             |
| Left-click an affordable node        | Purchases the node, deducts its cost, unlocks it, and applies its effect. |
| Left-drag empty tree space           | Pans the upgrade tree.                                                    |
| Right-drag or middle-drag tree space | Also pans the upgrade tree.                                               |
| Scroll over the tree                 | Zooms the upgrade tree in or out.                                         |
| Close the window                     | Saves progress to `save.json`.                                            |

### Node Colors

| Node state                   | Visual behavior                     |
| ---------------------------- | ----------------------------------- |
| Purchased                    | Green node and green label.         |
| Available and affordable     | Bright yellow node/label highlight. |
| Available but not affordable | Muted brown/yellow label.           |
| Locked but visible           | Gray node/label.                    |

The node label is drawn below the circular node so long names and costs do not overflow out of the node. If a name is still too long, it is shortened with `...` for readability; the full name remains visible in the tooltip.

### Number Formatting

Large point and cost values are abbreviated for readability:

| Range     | Display suffix | Example                         |
| --------- | -------------- | ------------------------------- |
| Thousands | `K`            | `1500` becomes `1.50K`          |
| Millions  | `M`            | `1200000` becomes `1.20M`       |
| Billions  | `B`            | `2500000000` becomes `2.50B`    |
| Trillions | `T`            | `1000000000000` becomes `1.00T` |

This formatting is presentation-only. It does not change actual point values, costs, passive generation, or upgrade effects.

## Upgrade Tree

The tree has four broad sections:

- Click branch: increases or multiplies click power.
- Passive branch: adds or multiplies passive points per second.
- Synergy branch: requires progress in both click and passive branches.
- Endgame branch: combines late click/passive/synergy requirements into large multipliers.

### Upgrade Nodes

| ID       | Name         |       Cost | Prerequisite(s)   | Effect                         |
| -------- | ------------ | ---------: | ----------------- | ------------------------------ |
| `start`  | Awaken       |         10 | none              | +1 click power                 |
| `click1` | Click II     |         50 | `start`           | +2 click power                 |
| `click2` | Click III    |        200 | `click1`          | +3 click power                 |
| `click3` | Click IV     |        800 | `click2`          | +5 click power                 |
| `click4` | Click x2     |      3,000 | `click3`          | ×2 click power                 |
| `click5` | Click +20    |     15,000 | `click4`          | +20 click power                |
| `click6` | Click x3     |     80,000 | `click5`          | ×3 click power                 |
| `auto1`  | Auto-Clicker |         25 | `start`           | +1 passive/s                   |
| `auto2`  | Factory      |        150 | `auto1`           | +3 passive/s                   |
| `auto3`  | Mega Factory |        700 | `auto2`           | +8 passive/s                   |
| `auto4`  | Passive x2   |      2,500 | `auto3`           | ×2 passive                     |
| `auto5`  | Quantum      |     12,000 | `auto4`           | +50 passive/s                  |
| `auto6`  | Galaxy       |     60,000 | `auto5`           | +200 passive/s                 |
| `auto7`  | Passive x3   |    300,000 | `auto6`           | ×3 passive                     |
| `syn1`   | Combo        |      1,000 | `click1`, `auto1` | +5 click power, +5 passive/s   |
| `syn2`   | Harmony      |      8,000 | `syn1`            | ×1.5 click power and passive   |
| `syn3`   | Resonance    |     50,000 | `syn2`            | +50 click power, +50 passive/s |
| `pres1`  | Ascend Click |    500,000 | `click6`, `syn3`  | ×3 click power                 |
| `pres2`  | Ascend Flow  |    500,000 | `auto7`, `syn3`   | ×3 passive                     |
| `pres3`  | Singularity  |  5,000,000 | `pres1`, `pres2`  | ×5 click power and passive     |
| `pres4`  | Big Bang     | 50,000,000 | `pres3`           | ×10 click power and passive    |

## Save and Load Behavior

- On startup, the game checks for `save.json`.
- If `save.json` exists and is valid, points, unlocked node IDs, camera position, and zoom are restored.
- If `save.json` does not exist, the game starts from a fresh state with 0 points, 1 click power, 0 passive generation, and no unlocked nodes.
- When the window closes normally, the game writes the current state back to `save.json`.
- `save.json` is ignored by Git because it is local player data, not source code.

## Project Structure

```text
incremental-click/
├── incremental-click.fsproj   # .NET 10 / F# project file and Raylib-cs dependency
├── Constants.fs               # Window, tree panel, and node-size constants
├── Types.fs                   # Effect, node, and game-state types
├── Tree.fs                    # All 21 upgrade-node definitions
├── Render.fs                  # Raylib initialization and bundled font loading
├── Save.fs                    # JSON save/load logic
├── Program.fs                 # Main game loop, input handling, rendering, and upgrades
├── asset/
│   └── font/
│       ├── NotoSans-Regular.ttf
│       └── OFL.txt
├── .gitignore
└── README.md
```

`bin/` and `obj/` are generated by .NET builds and are already ignored by `.gitignore`. They are not required for a clean checkout because `dotnet build` and `dotnet run` regenerate them.

## Proposal Compliance

| Submitted requirement                                                                                          | Final implementation                                                                                                           |
| -------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| Raylib 2D window with a points display, large click button, and upgrade tree                                   | Implemented. The top bar shows points and stats, the left side has `CLICK!`, and the right side shows the tree.                |
| Clicking the main button adds points equal to current click power, starting at 1                               | Implemented. Click power starts at 1 and is recomputed from unlocked upgrades.                                                 |
| Continuous 60 FPS game loop updates passive points-per-second                                                  | Implemented. `Render.init` sets 60 FPS and the main loop adds `Passive * dt`.                                                  |
| Upgrade tree is represented as nodes with unique IDs, costs, prerequisites, effects, and locked/unlocked state | Implemented in `Tree.fs` and `GameState.UnlockedIds`.                                                                          |
| A node is purchasable only if prerequisites are unlocked and the player has enough points                      | Implemented by the availability and purchase checks in `Program.fs`.                                                           |
| Locked, available, and purchased nodes use distinct visual states                                              | Implemented with gray, muted/highlighted, and green node/label colors.                                                         |
| Purchasing a node deducts cost, unlocks it, applies its effect, and reveals newly connected progress           | Implemented. Effects are recomputed after every purchase and child/progress nodes become visible as prerequisites are reached. |
| Player can pan the tree by dragging and zoom using the scroll wheel                                            | Implemented. Empty-space left-drag, right-drag, middle-drag, and scroll zoom are supported.                                    |
| Save progress to local JSON on close and load it on startup                                                    | Implemented with `save.json`.                                                                                                  |
| Tree contains at least 20 upgrade nodes across multiple branches                                               | Implemented with 21 nodes across click, passive, synergy, and endgame branches.                                                |

## Changes from the Proposal

No proposed gameplay feature was removed. The final implementation follows the submitted requirements.

The following details are clarifications or presentation improvements, not requirement-breaking changes:

| Item                                                                   | Final behavior                                                                                 | Reason                                                               |
| ---------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------- | -------------------------------------------------------------------- |
| Concrete upgrade names, costs, coordinates, and numeric balance values | The proposal described the tree structurally; the final code supplies exact data for 21 nodes. | Required to make the game playable and testable.                     |
| `K`/`M`/`B`/`T` number formatting                                      | Large values are abbreviated in the UI.                                                        | Readability only; actual numeric values and mechanics are unchanged. |

### Requirements Planned and Implemented

All behaviors listed in the submitted requirements are present in the final game: clicking for points, passive generation, prerequisite-checked node purchasing, visible node states, branch reveal behavior, tree panning, scroll zoom, save/load, and at least 20 upgrade nodes.

## Use of LLM

I used OpenAI Codex, an LLM-based coding assistant, while preparing the final submission. I am responsible for the final README and implementation, and I reviewed the generated changes before keeping them.

### What I Used the LLM For

- Reading the course project specification and my requirements document.
- Drafting and revising this README so reviewers can run and evaluate the game from the repository alone.
- Checking whether the implementation appears to satisfy the grading policy.
- Reviewing whether generated build folders such as `bin/` and `obj/` should be ignored before deployment.
- Improving UI readability by moving node text into bounded labels and using a bundled font.
- Updating the README to explain presentation-only details such as `K`/`M`/`B`/`T` number formatting.

### Prompts Used

- "Refer to the files above and write README.md. Also check whether my game violates any grading policy. For the LLM Usage section, write only a template. Also consider whether the `bin` and `obj` folders should be added to `.gitignore` when deploying."
- "Rewrite it. Also fill in all the other items, not just the prompt."
- "I did not write in the requirements that points are formatted as K, M, B, and T. Reflect this in README.md. There is also a bug where text on nodes sometimes goes outside the node and becomes hard to see. Fix that too. The font looks bad, so fix that as well."
- "Write the README in much more detail like the example, so there is no grading penalty. Also, do not separate README instructions by operating system; mention only one common behavior so there is no need to list commands for each operating system. Also, do not use a default font; put a font inside `asset/font` and use that consistently."

### Manual Changes or Reprompts Needed

- The first README draft left the LLM usage section as only a template, so I reprompted the LLM to fill in the actual items.
- I reviewed and adjusted the wording around requirement changes so UI-only changes, such as number formatting and font rendering, are clearly described as presentation details rather than gameplay changes.
- I rejected system-font fallback behavior and required a bundled font in `asset/font` so the visual result is consistent.
- I checked the build after code changes because UI code can compile incorrectly if Raylib-cs function calls or F# argument syntax are wrong.

### Main Point the LLM Could Not Fully Verify

The LLM could inspect source code and run `dotnet build`, but it could not fully judge the graphical play experience like a human reviewer. In particular, final confirmation of text readability, mouse feel, and whether the tree view is comfortable to pan/zoom still requires launching the game and inspecting it visually.

# PYFGG - 2D Metroidvania Narrative Game
## Overview
### PYFGG is a 2D Metroidvania-styled, narrative-focused game.
- The project emphasizes **fighting, progression and storytelling.**
- This repository highlights the core gameplay systems, particularly the dynamic state machine called **GameActionSystem** used for player and npc actions.
- While the game itself is larger, only scripts and essential systems are included here for portfolio purposes.

## Key Feature - GameActionSystem
- A modular state machine for managing gameplay actions.
- Currently implemeneted for the player, but designed to work for NPCs as well.
- Makes it easy to add new actions without changing core logic.
- Example actions in repository: Jump, Dash.

## How To Use GameActionSystem
Please refer to GameActionSystem's [README](https://github.com/theHardInGame/PYFGG/blob/main/Assets/_PYFGGMain/Code/Scripts/Core/GameActionSystem/README.md)

## Testing/Running Environment Setup
(This is only for editor Play Mode)
1. Load Bootstrap.unity (scene) intro heirarchy. Make sure it is the only scene in Heirarchy before entering Play Mode.
2. If is missing, in Assets/_Bootstrap/BootConfig.asset, reassign InputManager.asset prefab.
3. Make sure it looks likes this:
<img width="597" height="476" alt="image" src="https://github.com/user-attachments/assets/13ec3680-326c-42a7-aae2-b2513ac593a1" />  
(Make sure both scenes are in Scene List of your editors Build Profiles)  
4. Press Play.  
Controls:  
W, A, S, D for movement.  
Space for jump.  
Left Shift for sprint.  

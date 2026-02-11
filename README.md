# PYFGG - 2D Metroidvania Narrative Game
## Overview
### PYFGG is a 2D Metroidvania-styled, narrative-focused game.
- The project emphasizes **fighting, progression and storytelling.**
- This repository highlights the core gameplay systems, particularly the dynamic state machine called **GameActionSystem** used for player and npc actions.
- While the game itself is larger, only scripts and essential systems are included here for portfolio purposes.

## Key Feature - GameActionSystem
- A modular state machine for managing gameplay actions.
- Currently implemented for the player, but designed to work for NPCs as well.
- Makes it easy to add new actions without modifying core logic.
- Example actions in repository: Jump, Dash.

![Demo](GameActSysDemo.gif)

## How To Use GameActionSystem
Please refer to GameActionSystem's [README](https://github.com/theHardInGame/PYFGG/blob/main/Assets/_PYFGGMain/Code/Scripts/Core/GameActionSystem/README.md)

## Testing/Running Environment Setup
(This is only for editor Play Mode)
1. Load Bootstrap.unity (scene) intro hierarchy. Make sure it is the only scene in hierarchy before entering Play Mode.
2. If is missing, in Assets/_Bootstrap/BootConfig.asset, reassign InputManager.asset prefab.
3. It should looks like this:  
   ![Boostrap Scene Setup](https://github.com/user-attachments/assets/13ec3680-326c-42a7-aae2-b2513ac593a1)  
   (Make sure both scenes are in your Editors Build Settings Scene List)  
4. Press Play.

Controls:  
W, A, S, D -> Move  
Space -> Jump.  
Left Shift -> Dash & (hold)Sprint.  


**Unity version used:** Unity 6000.3.1f1

## Move, Aim, Roll
- Player can walk around in all directions using WASD
- Player can aim the weapon using the mouse
- Player can attack by left-clicking and is unable to attack again until the attack cooldown has run out
- Player can dodge roll by pressing the space bar and cannot roll again until the cooldown has run out
## HP, Mana, Melee
- Pressing 1 and 2 (skill keys) temporarily decrease and increase the player's health to show the system is working
- Same for the 3 and 4 (skill keys) on the mana bar to show independence
- Damage is dealt to enemies within the weapon's hitbox
## Ranged
- Player can left-click to attack with ranged weapons and cannot attack again until the cooldown has run out
- Projectiles will travel in the direction of the player's mouse when they initiated the attack
- Enemies that collide with the arrows take damage
## Code
### Player Controller
- This class will be split into a general agent script to share code between enemies, NPCs, and the player + a player input script
- Handles player input, movement, sprite animations
### Weapon Parent Controller
- Handles weapon aiming, attacking, collisions, animations
### Resource System
- General class for anything that has a maximum that we want to add and subtract from
- Currently being used for player health and mana, and enemy health
### Stat Block
- Scriptable object that contains data for an entity
- Makes general stats across players, NPCs, and enemies uniform, easy to create, and easy to reference
### Weapon Data
- Scriptable object that contains data for weapon types
- Makes weapons with different play styles easy to create
- Can easily make scriptable objects using a custom submenu in the "Add Asset" menu
### Object Pooler
- Keeps a pool of instantiated objects that can be reused to avoid memory leaks and improve runtime (instead of deleting and instantiating objects every use)
- Automatically adds more objects to the pool if the current scene requires more
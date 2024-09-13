# Casino Knight

[![Discord](https://img.shields.io/discord/879125729936298015.svg?logo=discord&logoColor=white&logoWidth=20&labelColor=7289DA&label=Discord&color=17cf48)](https://discord.gg/F6Y5TeFQ8j) ![OS](https://img.shields.io/badge/os-windows-blue) [![License](https://img.shields.io/badge/license-MIT-green)](./LICENSE)


Lets go gambling!
This mod adds in a new casino building with two slot machines in Dirtmouth where our addicted hero can waste all his geo.
Will you win it big?

This is a follow up intermediate mod to my first beginner mod [Fart Knight](https://github.com/AbsoluteStratos/FartKnight).
I had this idea randomly and decided to wastefully sink in several weekends working out how to get my vision into the game and I'm pretty happy with the result.
Similar to my previous mod, I want to document this the learnings I had creating what I consider an intermediate mod for Hollow Knight.
Many of the features of this mod can be quickly extended to create entirely new levels, change the look of existing scenes and more.

This mod has the following features:

- A simple mod menu created using [Satchel BetterMenus](https://prashantmohta.github.io/ModdingDocs/Satchel/BetterMenus/better-menus.html)
- Modifying an existing scene by loading in a asset bundle exported from Unity
- Creating a new scene loaded from an asset bundle exported from Unity
- Creating a new gate from an existing scene to a new scene, including a door way
- Combining custom assets with pre-loaded in-game assets
- Bootstrapping pre-loaded game asset FSM graphs to create custom behavior
- Ingrating complex sprite animations using unity animator controllers and scripting 

## Documentation

Some rough notes taken documenting various items:

- [Adding a new object into a existing scene from Unity](./docs/modify_scene_unity.md)
- [Adding a new scene from Unity](./docs/modify_scene_unity.md)

## Code Walkthrough

Coming soon, maybe

## Repository Layout

```
CasinoKnight
├── bin                     # Compiled project files
├── etc                     # Miscellaneous stuff
├── src                     # Source folder
│   ├── Resources           # Packed asset bundles
│   ├── ModClass.cs         # Core mod class for hooking on Modding API
│   ├── ModMenu.cs          # Building function for Custom Mod Menu
│   ├── GlobalSettings.cs   # Data-structure for global state / settings
│   ├── Log.cs              # Simple logging utils
│   ├── CasinoExterior.cs   # Casino exterior loading and logic
│   ├── CasinoInterior.cs   # Casino interior loading and logic 
│   ├── SlotHandler.cs      # Slot machine logic and behavior
│   └── CasinoKnight.csproj # C# project file
└── FartKnight.sln          # Visual Studio solution file
```

## Resources

- [Hollow Knight Scene Names](https://drive.google.com/drive/folders/1VwVbCjU8uPV4V3cDu_Tr1TgEs01hMSFr)
- [Hollow Knight Sprite Database](https://drive.google.com/drive/folders/1lx02_w9TFTYdR3aggI1gbXcLr69roaNV)
- [OG NewScene Docs](https://radiance.synthagen.net/apidocs/_images/NewScene.html)
- [Unity 2020.2.2f1](https://unity.com/releases/editor/archive)
- [HKWorldEdit2](https://github.com/nesrak1/HKWorldEdit2)
- [Unity Asset Bundler Browser](https://github.com/Unity-Technologies/AssetBundles-Browser)
- [PlayMaker FSM Viewer Avalonia](https://github.com/nesrak1/FSMViewAvalonia)

Not used but cool:

- [Unity Asset Ripper](https://github.com/AssetRipper/AssetRipper)


## Dependencies

- [Satchel](https://github.com/PrashantMohta/Satchel/)

## Support

For issues / bugs, I probably won't fix them but feel free to open an issue.
The modding discord has a lot of very helpful and active devs there which can also answer various questions but don't bug them about this mod.

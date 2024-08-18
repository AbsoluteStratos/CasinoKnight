# Hollow Knight Treasure Hunt

Example mod that demos setting up a set of collectables around the hollow knight map and triggering an even when all are collected.

## Install

Clone mod from Github Repo:
```
git clone git@github.com:AbsoluteStratos/HollowKnightTreasureHunt.git
```

Update the Hollow Knight refs in the [HollowKnightTreasureHunt.csproj](./src/HollowKnightTreasureHunt.csproj) to point to your Hollow Knight game:
```
<!-- Change this to the path of your modded HK installation -->
<HollowKnightRefs>C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight\hollow_knight_Data\Managed</HollowKnightRefs>
<!-- Change this to the path where you want the ready-to-upload exports to be -->
<ExportDir>C:\Users\$(USERNAME)\Documents\Hollow Knight Mods\StudyKnight\bin\</ExportDir>
```

Open this repository using VSCode and do "Build > BuildStudyKnight" or Ctrl-B. This should build binaries in the `bin` folder and also place the compiled dll in your local hollow knight mods folder.

Check Luma Fly to see if its installed.

## Dependencies

- [FrogCore](https://github.com/RedFrog6002/FrogCore/)
- [Satchel](https://github.com/PrashantMohta/Satchel/) for mod menus

## Features

- 
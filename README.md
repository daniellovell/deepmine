# deepmine
[![Discord](https://img.shields.io/discord/799012229524488223?color=blue&label=Discord&logo=Discord&style=flat-square)]() [![GitHub Release](https://img.shields.io/github/v/release/daniellovell/deepmine?style=flat-square)]() [![Commits Since](https://img.shields.io/github/commits-since/daniellovell/deepmine/latest?color=yellow&style=flat-square)]() [![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](http://makeapullrequest.com) 

![image](https://user-images.githubusercontent.com/6250953/104089769-5b5d2d00-5226-11eb-9b6e-2ce87961ffb0.png)

The goal of the mod is to enhance sub-surface gameplay and make it worthwhile to build structures and explore deep below the surface. Personally I find the surface (placer) mining mechanic of the game to be the weakest gameplay loop in an otherwise amazing game. I'm striving to make mining in Stationeers a more enjoyable experience :-)

I want to emphasize this mod is very very raw. An alpha version of sorts. I'm hoping to get some feedback from the community on balancing and requested features!

### Features

Completed features
 - :heavy_check_mark: Lowered bedrock depth - default 4x deeper than vanilla (configurable)
 - :heavy_check_mark: No more lava/bedrock texture at unrealistic depths
 - :heavy_check_mark: Increasing ore yields the deeper you go
 - :heavy_check_mark: Faster drilling speeds
 - :heavy_check_mark: Doubled GPR (ground penetrating radar) scan radius (configurable)

Planned features
 - :small_orange_diamond: Increasing vein size/frequency at deeper depths (conversely, sparse veins at the surface)
 - :small_orange_diamond: More mining drill tiers. Including tools with larger mining radius.
 - :small_orange_diamond: Threats and dangers below the surface. Gas pockets, and various pressure/temperature

### Compatibility

#### With existing saves
 - As of version 0.1, this mod is compatible with existing saves and will not corrupt them when you install or uninstall the mod

#### With other mods
 - Not sure about this one, but I'd be hesitant to run this mod with any other terrain generation/ore rebalancing mods.
 
### Installation

 1. Install BepInEx to your Stationeers folder in Program Files (x86) https://github.com/BepInEx/BepInEx/releases
 2. Run Stationeers once to complete the BepInEx installation
 3. Install ``DeepMineMod.dll`` to the Stationeers/BepInEx/plugins folder
 4. Run Stationeers once to generate the config file
 5. Change the config to your liking in /Stationeers/BepInEx/config/com.dl.deepmine.cfg

### Contributions

 - I'm reaching out to any modders who are interested in the project. If you have any experience with BepInex + Harmony patching, and especially use of Harmony Transpiler, please DM me :-)

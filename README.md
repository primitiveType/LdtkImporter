# Godot LDtk C# Importer

Godot 4 C# [LDtk](https://ldtk.io/) Import Plugin

![](https://img.shields.io/badge/Godot%20.NET-4.2%2B-%20?logo=godotengine&color=%23478CBF) ![](https://img.shields.io/badge/LDtk%201.4.1-%20?color=%23FFCC00)

> ⚠️ This plugin is still in development, many features are still being adjusted. Do not use this plugin in production environments! Please make sure to back up your project files before using it!

# 📖 Installation

1. Use Godot 4.2+ with C#
2. Place the `addons\LdtkImporter` directory inside the project’s `addons` folder
3. Enable the plugin via `Project > Project Settings > Plugins`
4. `.ldtk` files can now be recognized by Godot, automatically imported, and corresponding `.tscn` scene resources and `.tres` Tileset resources will be generated

# ✨ Features

## 🌏 World

- [x] After importing `.ldtk` files, a `.tscn` scene with the same name will be generated, containing all `Level` nodes ⬇️

![](img/World.png)


## 🏔️ Level

- [x] LDTK `Level` is imported as a `Level` scene with the same name ⬇️
- [x] `Level` scenes are exported independently as `.tscn` files

![](img/Level.png)


## 📄 Layer
- [x] `AutoLayer` and `IntGrid` Layers are generated as Godot TileMaps, `Entity` Layers are generated as Godot Node2D
- [x] Support generating `IntGrid` as child nodes ⬇️

![](img/LayerIntGrid.gif)

- [x] Support stacking multiple tiles under the same `Layer`


## 🧱 Tilesets
- [x] For each LDTK Tileset, generate a Godot Tileset in [TileSetAtlasSource](https://docs.godotengine.org/en/stable/classes/class_tilesetatlassource.html#class-tilesetatlassource) style
- [x] Support [Tile](https://ldtk.io/json/#ldtk-Tile;f) flipping on X/Y axes, and generate corresponding `AlternativeTile`


## 🐸 Entity
- [x] Generate one for each Entity


# 🚩 Import Options
When selecting an `.ldtk` file in Godot’s `FileSystem`, the following import options are available ⬇️

![](img/ImportOptions.png)



* General
    * Prefix: The prefix applied to generated nodes (`Node2D`, `TileMap`, etc.) and metadata names during import (e.g. `LDTK_World_Level_0`)
* World
    * Post Processor: Import post-processor, allows custom handling of imported nodes
    * World Scenes: Names of world scene files to generate
* Tileset
    * Add Tileset Definition to Meta: Store all [LDTK Tileset definition](https://ldtk.io/json/#ldtk-TilesetDefJson) data as metadata in the Godot tileset, under the key `${Prefix}_tilesetDefinition` (e.g. `LDTK_tilesetDefinition`)
    * Resources: Plugin automatically generates configuration for different LDTK Tilesets
* Entity
    * Post Processor: Import post-processor, allows custom handling of imported nodes
    * Add Entity Definition to Meta: Store [LDTK Entity Definition](https://ldtk.io/json/#ldtk-EntityDefJson) data as metadata in imported Entity Scenes and nodes, under the key `${Prefix}_entityDefinition` (e.g. `LDTK_entityDefinition`)
    * Add Entity Instance to Meta: Store [LDTK Entity Instance](https://ldtk.io/json/#ldtk-EntityInstanceJson) data as metadata in imported Entity nodes, under the key `${Prefix}_entityInstance` (e.g. `LDTK_entityInstance`)
    * Scenes: Plugin automatically generates configuration based on the number of Entities in LDTK
* Level
    * Post Processor: Import post-processor, allows custom handling of imported nodes
    * Add Level to Meta: Store [LDTK Level](https://ldtk.io/json/#ldtk-LevelJson) data as metadata in imported Level nodes, under the key `${Prefix}_levelInstance` (e.g. `LDTK_levelInstance`)
    * Add Layer Instance to Meta: Store [LDTK Layer Instance](https://ldtk.io/json/#ldtk-LayerInstanceJson) data as metadata in imported Layer nodes, under the key `${Prefix}_layerInstance` (e.g. `LDTK_layerInstance`)
    * Add Layer Definition to Meta: Store [LDTK Layer Definition](https://ldtk.io/json/#ldtk-LayerDefJson) data as metadata in imported Layer nodes, under the key `${Prefix}_layerDefinition` (e.g. `LDTK_layerDefinition`)
    * Import Int Grid: Whether to import `IntGrid`, see the animation under [Layer](#-layer) for effect
    * Scenes: Plugin automatically generates configuration based on different LDTK Levels

# ❓FAQ
## LDTK supports stacking multiple [tile instances](https://ldtk.io/json/#ldtk-LayerInstanceJson;autoLayerTiles) in the same position within a Layer. How is this handled when imported into Godot?
Godot TileMap supports creating multiple [Layers](https://docs.godotengine.org/en/stable/tutorials/2d/using_tilemaps.html#creating-tilemap-layers).  
During import, the plugin calculates the maximum stack depth for the current Layer and pre-creates those Layers in the TileMap, while also updating each [tile instance](https://ldtk.io/json/#ldtk-Tile) with its correct TileMap Layer index. By the time import runs, the plugin already knows which TileMap Layer each LDTK tile instance belongs to.  
In short: stacking is solved by leveraging Godot TileMap’s multi-Layer support.

## If this plugin is used as a bridge between LDTK and Godot, what should the workflow look like?
The author has been considering this, but has not found a perfect solution yet. In an LDTK-Godot workflow, LDTK serves mainly as a map editor, but not all editing tasks can be done in LDTK. For example, configuring physics collisions or navigation in TileSets, or editing `.tscn` scenes after import.  
This leads to a core conflict: **How to handle re-imports without affecting modifications made in Godot?**

Possible solutions:
1. No re-import support: each import overwrites existing resources (Tileset, Scene)
2. Support re-imports
    1. If resources already exist, modify them based on the original data
    2. Use prefixes to distinguish between `User` nodes and `LDTK` nodes, and between `User` metadata and `LDTK` metadata

Currently, the plugin uses solution **1**. If you have better ideas, feel free to discuss!

## How to extend `Import Post Processors`?
1. Implement [AbstractPostProcessor](addons/LdtkImporter/PostProcessor/AbstractPostProcessor.cs), for example:
    ```csharp
    [Tool] //1. Required
    [GlobalClass] //2. Required
    [PostProcessor(ProcessorType.World)] //3. The processor type, can be combined, e.g. [PostProcessor(ProcessorType.World | ProcessorType.Level)] 
    public partial class MyWorldPostProcessor : AbstractPostProcessor
    {
        public override Node2D PostProcess(LdtkJson ldtkJson, Dictionary options, Node2D baseNode)
        {
            // Process baseNode, can return baseNode, or a completely new Node2D or subclass
            return baseNode;
        }
    }
    ```
2. Create a new resource of type `MyWorldPostProcessor` in `FileSystem`, save with the extension `.tres`. This post-processor will automatically be added to `Import Options`.

# 💣 TODO

- [ ] Runtime
    - [ ] Support runtime dynamic modification of `IntGrid`, with real-time updates and rendering of affected `IntGrid` and `AutoLayer` based on [LDTK Auto-layer rule definition](https://ldtk.io/json/#ldtk-AutoRuleDef)
- [ ] World
    - [x] Support import post-process scripts
    - [x] Support LDTK default `Level` background color
    - [ ] Support LDTK [Multi-worlds](https://github.com/deepnight/ldtk/issues/231)
- [ ] Level
    - [x] Support importing `Level` background color and images
    - [x] Support LDTK Level fields, metadata name: `$"{prefix}_fieldInstances"`
    - [x] Support `Level` import post-process scripts
    - [ ] Add Level configuration option: whether to generate independent Level scenes
- [ ] Entity
    - [ ] Support Entity visual display (`Sprite2D`)
    - [ ] Support `Entity` import post-process scripts
    - [ ] Enum support
- [ ] Documentation
    - [x] Add explanation for extending `Post Processors`

# 🐞 Known Bugs
- [ ] After each re-import, you need to `Reload Current Project` or restart Godot for imported `.tscn` to take effect
- [ ] Every re-exported `.tscn` file changes (mainly due to Level scene IDs changing)
- [x] If `IntGrid` does not contain rules, generated TileMap nodes lack TileSet and tiles are not set correctly
- [x] Official example: Typical_TopDown_example.ldtk — some tiles are missing after import
- [x] No background color is set for Levels after import
- [x] Opening certain imported scenes and Tilesets causes Godot to crash (e.g. Typical_TopDown_example.ldtk)

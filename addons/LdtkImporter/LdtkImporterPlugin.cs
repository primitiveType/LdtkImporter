using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using Environment = System.Environment;
#if TOOLS
namespace LdtkImporter;

[Tool]
[SuppressMessage("ReSharper", "UnusedParameter.Local")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
[SuppressMessage("Performance", "CA1822:将成员标记为 static")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public partial class LdtkImporterPlugin : EditorImportPlugin
{
    public override string _GetImporterName() => "Ldtk Importer";
    public override string _GetVisibleName() => "LDTK World Scene";
    public override string[] _GetRecognizedExtensions() => new[] { "ldtk" };
    public override string _GetResourceType() => "Node2D";
    public override string _GetSaveExtension() => LdtkImporterConstants.SaveExtension;
    public override float _GetPriority() => 0.1f;
    public override int _GetPresetCount() => 0;
    public override string _GetPresetName(int presetIndex) => "";
    public override int _GetImportOrder() => 99;
    public override bool _GetOptionVisibility(string path, StringName optionName, Dictionary options) => true;

    private static readonly object _importLock = new();
    private LdtkJson _ldtkJson;
    private string _ldtkFileName;

    public override Array<Dictionary> _GetImportOptions(string path, int presetIndex)
    {
        _ldtkFileName = path.GetBaseName().GetFile();
        _ldtkJson = LdtkJson.FromPath(path);

        var options = new Array<Dictionary>();

        options.AddRange(GeneralOptions(path, presetIndex));
        options.AddRange(WorldOptions(path, presetIndex));
        options.AddRange(TilesetOptions(path, presetIndex));
        options.AddRange(EntityOptions(path, presetIndex));
        options.AddRange(LevelOptions(path, presetIndex));

        return options;
    }

    private IEnumerable<Dictionary> GeneralOptions(string path, int presetIndex)
    {
        return new Array<Dictionary>
        {
            new()
            {
                { "name", LdtkImporterConstants.OptionGeneralPrefix },
                { "default_value", "LDTK" },
                { "hint_string", "the prefix of all imported node name and meta." }
            },
        };
    }

    private IEnumerable<Dictionary> WorldOptions(string path, int presetIndex)
    {
        var globalizePath = ProjectSettings.GlobalizePath("res://");
        var worldPostProcessorPath = Directory
            .GetFiles(globalizePath, "*.tres", SearchOption.AllDirectories)
            .Select(s => ResourceLoader.Load(ProjectSettings.LocalizePath(s)))
            .OfType<AbstractPostProcessor>()
            .Where(processor => processor.Handle(ProcessorType.World))
            .Select(resource => resource.ResourcePath)
            .ToList();

        return new Array<Dictionary>
        {
            new()
            {
                { "name", LdtkImporterConstants.OptionWorldPostProcessor },
                { "default_value", worldPostProcessorPath.FirstOrDefault("") },
                { "property_hint", (int)PropertyHint.Enum },
                { "hint_string", string.Join(",", worldPostProcessorPath) },
            },
            new()
            {
                { "name", LdtkImporterConstants.OptionWorldWorldScenes },
                {
                    "default_value",
                    $"{path.GetBaseDir().PathJoin(_ldtkFileName).PathJoin(_ldtkFileName)}.{_GetSaveExtension()}"
                },
                { "property_hint", (int)PropertyHint.File },
                { "hint_string", "*.tscn;Godot Scene" }
            },
        };
    }

    private IEnumerable<Dictionary> TilesetOptions(string path, int presetIndex)
    {
        var tilesetBaseDir = path.GetBaseDir().PathJoin($"{_ldtkFileName}/Tileset");
        var options = new Array<Dictionary>
        {
            new()
            {
                { "name", LdtkImporterConstants.OptionTilesetAddMeta },
                { "default_value", true },
                { "hint_string", "If true, will add the original LDtk data as metadata with key:ldtk." }
            },
            new()
            {
                { "name", LdtkImporterConstants.OptionTilesetImportTileCustomData },
                { "default_value", true },
                {
                    "hint_string",
                    "If true, will add custom layer data for each godot tile using Ldtk tile custom data."
                }
            },
        };

        var tilesetMapping = _ldtkJson.Defs.Tilesets
            .OrderBy(definition => definition.Identifier)
            .Select(definition => new Dictionary
                {
                    { "name", $"{LdtkImporterConstants.OptionTilesetResources}/{definition.Identifier}" },
                    { "default_value", $"{tilesetBaseDir.PathJoin(definition.Identifier)}.tres" },
                    { "property_hint", (int)PropertyHint.File },
                    { "hint_string", "*.tres;Tileset Resource" }
                }
            );

        options.AddRange(tilesetMapping);

        return options;
    }

    private IEnumerable<Dictionary> LevelOptions(string path, int presetIndex)
    {
        var globalizePath = ProjectSettings.GlobalizePath("res://");
        var levelPostProcessorPath = Directory
            .GetFiles(globalizePath, "*.tres", SearchOption.AllDirectories)
            .Select(s => ResourceLoader.Load(ProjectSettings.LocalizePath(s)))
            .OfType<AbstractPostProcessor>()
            .Where(processor => processor.Handle(ProcessorType.Level))
            .Select(resource => resource.ResourcePath)
            .ToList();

        var levelBaseDir = path.GetBaseDir().PathJoin($"{_ldtkFileName}/Level");
        var options = new Array<Dictionary>
        {
            new()
            {
                { "name", LdtkImporterConstants.OptionLevelPostProcessor },
                { "default_value", levelPostProcessorPath.FirstOrDefault("") },
                { "property_hint", (int)PropertyHint.Enum },
                { "hint_string", string.Join(",", levelPostProcessorPath) },
            },
            new()
            {
                { "name", LdtkImporterConstants.OptionLevelAddLevelInstanceToMeta },
                { "default_value", true },
                {
                    "hint_string",
                    "If true, will add the original LDtk level data to level node's meta with key:${Prefix}_level"
                }
            },
            new()
            {
                { "name", LdtkImporterConstants.OptionLevelAddLayerInstanceToMeta },
                { "default_value", true },
                {
                    "hint_string",
                    "If true, will add the original LDtk layer instance to layer node's meta with key:${Prefix}_layerInstance"
                }
            },
            new()
            {
                { "name", LdtkImporterConstants.OptionLevelAddLayerDefinitionToMeta },
                { "default_value", true },
                {
                    "hint_string",
                    "If true, will add the original LDtk layer definition to layer node's meta with key:${Prefix}_layerDefinition"
                }
            },
            new()
            {
                { "name", LdtkImporterConstants.OptionLevelImportIntGrid },
                { "default_value", false },
                { "hint_string", "If true, will import IntGrid as a child TileMap node." }
            },
        };

        var levelMapping = _ldtkJson.Levels
            .OrderBy(definition => definition.Identifier)
            .Select(definition => new Dictionary()
                {
                    { "name", $"{LdtkImporterConstants.OptionLevelScenes}/{definition.Identifier}" },
                    { "default_value", $"{levelBaseDir.PathJoin(definition.Identifier)}.tscn" },
                    { "property_hint", (int)PropertyHint.File },
                    { "hint_string", "*.tscn;Godot Scene" }
                }
            );

        options.AddRange(levelMapping);

        return options;
    }

    private IEnumerable<Dictionary> EntityOptions(string path, int presetIndex)
    {
        var globalizePath = ProjectSettings.GlobalizePath("res://");
        var entityPostProcessorPath = Directory
            .GetFiles(globalizePath, "*.tres", SearchOption.AllDirectories)
            .Select(s => ResourceLoader.Load(ProjectSettings.LocalizePath(s)))
            .OfType<AbstractPostProcessor>()
            .Where(processor => processor.Handle(ProcessorType.Entity))
            .Select(resource => resource.ResourcePath)
            .ToList();

        var entityBaseDir = path.GetBaseDir().PathJoin($"{_ldtkFileName}/Entity");
        var options = new Array<Dictionary>
        {
            new()
            {
                { "name", LdtkImporterConstants.OptionEntityPostProcessor },
                { "default_value", entityPostProcessorPath.FirstOrDefault("") },
                { "property_hint", (int)PropertyHint.Enum },
                { "hint_string", string.Join(",", entityPostProcessorPath) },
            },
            new()
            {
                { "name", LdtkImporterConstants.OptionEntityAddDefinition2Meta },
                { "default_value", true },
                {
                    @"hint_string",
                    "If true, will add the original LDtk entity definition to meta with key:${Prefix}_entityDefinition"
                }
            },
            new()
            {
                { "name", LdtkImporterConstants.OptionEntityAddInstance2Meta },
                { "default_value", true },
                {
                    @"hint_string",
                    "If true, will add the original LDtk entity instance to meta with key:${Prefix}_entityInstance"
                }
            },
        };

        var properties = _ldtkJson.Defs.Entities
            .OrderBy(definition => definition.Identifier)
            .Select(definition => new Dictionary
                {
                    { "name", $"{LdtkImporterConstants.OptionEntityScenes}/{definition.Identifier}" },
                    { "default_value", $"{entityBaseDir.PathJoin(definition.Identifier)}.tscn" },
                    { "property_hint", (int)PropertyHint.File },
                    { "hint_string", "*.tscn;Godot Scene" }
                }
            );

        options.AddRange(properties);

        return options;
    }

    public override Error _Import(string sourceFile, string savePath, Dictionary options,
        Array<string> platformVariants, Array<string> genFiles)
    {
        lock (_importLock)
        {
            GD.Print(
                $"Import begin, LDTK file path:{sourceFile}, object:{_importLock.GetHashCode()}  thread:{Environment.CurrentManagedThreadId}, godot main id:{OS.GetMainThreadId()}");

            var error = _ldtkJson.PreImport(_ldtkJson, savePath, options, genFiles);
            if (error != Error.Ok)
            {
                GD.PrintErr($" PreImport failed, error:{error}");
                return error;
            }

            error = _ldtkJson.Import(_ldtkJson, savePath, options, genFiles);
            if (error != Error.Ok)
            {
                GD.PrintErr($" Import failed, error:{error}");
                return error;
            }

            error = _ldtkJson.PostImport(_ldtkJson, savePath, options, genFiles);
            if (error != Error.Ok)
            {
                GD.PrintErr($" PostImport failed, error:{error}");
                return error;
            }

            var worldScenePath = options.GetValueOrDefault<string>(LdtkImporterConstants.OptionWorldWorldScenes);
            DirAccess.CopyAbsolute($"{savePath}.{LdtkImporterConstants.SaveExtension}", worldScenePath);

            genFiles.Add(worldScenePath);

            GD.Print($"Import success thread:{Environment.CurrentManagedThreadId}");

            return Error.Ok;
        }
    }
}
#endif
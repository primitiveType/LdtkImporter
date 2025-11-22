namespace LdtkImporter;

public static class LdtkImporterConstants
{
    public const string OptionGeneral = "General";
    public const string OptionGeneralPrefix = $"{OptionGeneral}/prefix";
    public const string OptionWorld = "World";
    public const string OptionWorldPostProcessor = $"{OptionWorld}/post_processor";
    public const string OptionWorldWorldScenes = $"{OptionWorld}/WorldScenes";
    public const string OptionTileset = "Tileset";
    public const string OptionTilesetAddMeta = $"{OptionTileset}/add_tileset_definition_to_meta";
    public const string OptionTilesetResources = $"{OptionTileset}/Resources";
    public const string OptionTilesetImportTileCustomData = $"{OptionTileset}/import_LDTK_tile_custom_data";
    public const string OptionLevel = "Level";
    public const string OptionLevelPostProcessor = $"{OptionLevel}/post_processor";
    public const string OptionLevelScenes = $"{OptionLevel}/Scenes";
    public const string OptionLevelImportIntGrid = $"{OptionLevel}/import_IntGrid";
    public const string OptionLevelAddLevelInstanceToMeta = $"{OptionLevel}/add_level_instance_to_meta";
    public const string OptionLevelAddLayerDefinitionToMeta = $"{OptionLevel}/add_layer_definition_to_meta";
    public const string OptionLevelAddLayerInstanceToMeta = $"{OptionLevel}/add_layer_instance_to_meta";
    public const string OptionEntity = "Entity";
    public const string OptionEntityPostProcessor = $"{OptionEntity}/post_processor";
    public const string OptionEntityScenes = $"{OptionEntity}/Scenes";
    public const string OptionEntityAddDefinition2Meta = $"{OptionEntity}/add_entity_definition_to_meta";
    public const string OptionEntityAddInstance2Meta = $"{OptionEntity}/add_entity_instance_to_meta";
    public const string SaveExtension = "tscn";
}

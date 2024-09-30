namespace Sqlist.NET.TestResources.Properties;
public static class Consts
{
    public const string ScriptsRscPath = "Resources.Scripts";
    public const string RoadmapRscPath = "Resources.Roadmap";

    public const string TestDatabaseName = "sqlist_net_test";

    private const string ErResources = "Sqlist.NET.TestResources.Resources.";
    private const string ErInvalidPhases = ErResources + "InvalidPhases.";
    
    public const string ErInvalidPhasesInvalidFormat = ErInvalidPhases + "invalid_format.yml";
    public const string ErInvalidPhasesInvalidVersion = ErInvalidPhases + "invalid_version.yml";
    public const string ErInvalidPhasesMissingTitle = ErInvalidPhases + "missing_title.yml";
    public const string ErInvalidPhasesUndefinedGuidelines = ErInvalidPhases + "undefined_guidelines.yml";

    private const string ErMigration = ErResources + "Roadmap.";
    public const string ErMigrationInitial = ErMigration + "v1_initial.yml";
}

﻿namespace Sqlist.NET.TestResources.Properties;
public static class Consts
{
    public const string ScriptsRscPath = "Resources.Scripts";
    public const string RoadmapRscPath = "Resources.Roadmap";

    public const string TestDatabaseName = "sqlist_net_test";

    public const string ER_Resources = "Sqlist.NET.TestResources.Resources.";

    public const string ER_InvalidPhases = ER_Resources + "InvalidPhases.";
    public const string ER_InvalidPhases_InvalidFormat = ER_InvalidPhases + "invalid_format.yml";
    public const string ER_InvalidPhases_InvalidVersion = ER_InvalidPhases + "invalid_version.yml";
    public const string ER_InvalidPhases_MissingTitle = ER_InvalidPhases + "missing_title.yml";
    public const string ER_InvalidPhases_UndefinedGuidelines = ER_InvalidPhases + "undefined_guidelines.yml";

    public const string ER_Migration = ER_Resources + "Roadmap.";
    public const string ER_Migration_Intial = ER_Migration + "v1_initial.yml";
}

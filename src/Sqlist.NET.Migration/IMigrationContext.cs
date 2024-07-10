using Sqlist.NET.Migration.Deserialization;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sqlist.NET.Migration;
public interface IMigrationContext
{
    IList<MigrationPhase> GetMigrationRoadMap();
    Task<MigrationOperationInformation> InitializeAsync(Version? targetVersion = null, Version? currentVersion = null);
    Task MigrateDataAsync();
}

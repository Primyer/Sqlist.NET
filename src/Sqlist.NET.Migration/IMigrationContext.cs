using Sqlist.NET.Migration.Deserialization;
using Sqlist.NET.Migration.Infrastructure;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sqlist.NET.Migration;
public interface IMigrationContext
{
    abstract static IList<MigrationPhase> GetMigrationRoadmap(MigrationAssetInfo assets);
    Task<MigrationOperationInfo> InitializeAsync(Version? targetVersion = null, Version? currentVersion = null, CancellationToken cancellationToken = default);
    Task MigrateDataAsync(CancellationToken cancellationToken = default);
}

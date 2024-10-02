using System;
using System.Collections.Generic;

using Sqlist.NET.Migration.Deserialization;

namespace Sqlist.NET.Migration;

internal interface IRoadmapBuilder
{
    DataTransactionMap Build(ref IEnumerable<MigrationPhase> phases, Version? currentVersion, Version? targetVersion = null);
}
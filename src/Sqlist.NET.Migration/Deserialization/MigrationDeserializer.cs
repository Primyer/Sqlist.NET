using Sqlist.NET.Migration.Properties;

using System;
using System.Collections;
using System.Linq;
using System.Runtime.Serialization;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sqlist.NET.Migration.Deserialization;
public class MigrationDeserializer
{
    private readonly IDeserializer _deserializer;

    /// <summary>
    ///     Initializes a new instance of <see cref="MigrationDeserializer"/> class.
    /// </summary>
    public MigrationDeserializer()
    {
        _deserializer = new DeserializerBuilder()
            .WithNodeDeserializer(new VersionNodeDeserializer())
            .WithNodeDeserializer(new DefinitionNodeDeserializer())
            .WithNodeDeserializer(new ColumnsDefinitionNodeDeserializer())
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
    }

    public MigrationPhase DeserializePhase(string data)
    {
        MigrationPhase phase;

        try
        {
            phase = _deserializer.Deserialize<MigrationPhase>(data);
        }
        catch (Exception ex)
        {
            throw new SerializationException(Resources.InvalidYamlFormat, ex);
        }

        ValidatePhase(phase);
        return phase;
    }

    private static void ValidatePhase(MigrationPhase phase)
    {
        ArgumentNullException.ThrowIfNull(phase);

        if (string.IsNullOrEmpty(phase.Title))
            throw new InvalidOperationException(Resources.PhaseTitleRequired);

        var guidelines = phase.Guidelines;
        if (guidelines is null || AllAreNullOrEmpty(guidelines.Create, guidelines.Update, guidelines.Delete))
        {
            throw new InvalidOperationException(Resources.NoMigrationGuidelinesDefined);
        }
    }

    private static bool AllAreNullOrEmpty(params ICollection[] collections)
    {
        return collections.All(collection => collection is null || collection.Count == 0);
    }
}

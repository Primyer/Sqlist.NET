using Sqlist.NET.Migration.Deserialization;
using Sqlist.NET.TestResources.Properties;
using Sqlist.NET.TestResources.Utilities;

using System.Runtime.Serialization;

namespace Sqlist.NET.Migration.Tests;
public class MigrationDeserializerTests
{
    private readonly MigrationDeserializer _deserializer;

    /// <summary>
    ///     Intializes a new instance of the <see cref="MigrationDeserializerTests"/> class.
    /// </summary>
    public MigrationDeserializerTests()
    {
        _deserializer = new MigrationDeserializer();
    }

    [Fact]
    public void DeserializePhase_InvalidFormat_ShouldFail()
    {
        Assert.Throws<SerializationException>(() =>
        {
            var data = AssemblyUtility.GetEmbeddedResource(Consts.ErInvalidPhasesInvalidFormat);
            _deserializer.DeserializePhase(data);
        });
    }

    [Fact]
    public void DeserializePhase_InvalidVersion_ShouldFail()
    {
        Assert.Throws<SerializationException>(() =>
        {
            var data = AssemblyUtility.GetEmbeddedResource(Consts.ErInvalidPhasesInvalidVersion);
            _deserializer.DeserializePhase(data);
        });
    }

    [Fact]
    public void DeserializePhase_MissingTitle_ShouldFail()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            var data = AssemblyUtility.GetEmbeddedResource(Consts.ErInvalidPhasesMissingTitle);
            _deserializer.DeserializePhase(data);
        });
    }

    [Fact]
    public void DeserializePhase_UndefinedGuidelines_ShouldFail()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            var data = AssemblyUtility.GetEmbeddedResource(Consts.ErInvalidPhasesUndefinedGuidelines);
            _deserializer.DeserializePhase(data);
        });
    }

    [Fact]
    public void DeserializePhase_ValidPhase_ShouldSucceed()
    {
        var data = AssemblyUtility.GetEmbeddedResource(Consts.ErMigrationInitial);

        var phase = _deserializer.DeserializePhase(data);
        var users = phase.Guidelines.Create["Users"];

        Assert.NotNull(phase);
        Assert.NotNull(users.Condition);
        Assert.NotEmpty(users.Condition);

        Assert.NotNull(phase.Title);
        Assert.NotEmpty(phase.Title);

        Assert.NotNull(phase.Guidelines);
        Assert.NotEmpty(phase.Guidelines.Create);
        Assert.NotEmpty(phase.Guidelines.Create.First().Value.Columns);
    }
}
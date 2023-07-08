using Sqlist.NET.Migration.Deserialization;
using Sqlist.NET.Migration.Tests.Properties;

using System.Runtime.Serialization;

namespace Sqlist.NET.Migration.Tests.BasicTests;
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
            var data = GetEmbeddedResource(Consts.ER_InvalidPhases_InvalidFormat);
            _deserializer.DeserializePhase(data);
        });
    }

    [Fact]
    public void DeserializePhase_InvalidVersion_ShouldFail()
    {
        Assert.Throws<SerializationException>(() =>
        {
            var data = GetEmbeddedResource(Consts.ER_InvalidPhases_InvalidVersion);
            _deserializer.DeserializePhase(data);
        });
    }

    [Fact]
    public void DeserializePhase_MissingTitle_ShouldFail()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            var data = GetEmbeddedResource(Consts.ER_InvalidPhases_MissingTitle);
            _deserializer.DeserializePhase(data);
        });
    }

    [Fact]
    public void DeserializePhase_UndefinedGuidelines_ShouldFail()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            var data = GetEmbeddedResource(Consts.ER_InvalidPhases_UndefinedGuidelines);
            _deserializer.DeserializePhase(data);
        });
    }

    [Fact]
    public void DeserializePhase_ValidPhase_ShouldSucceed()
    {
        var data = GetEmbeddedResource(Consts.ER_Migration_Intial);

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

    private static string GetEmbeddedResource(string name)
    {
        using var stream = typeof(MigrationDeserializerTests).Assembly.GetManifestResourceStream(name);
        using var reader = new StreamReader(stream!);

        return reader.ReadToEnd();
    }
}
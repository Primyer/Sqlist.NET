using System;
using System.Collections;
using System.Runtime.Serialization;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sqlist.NET.Migration.Deserialization
{
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
                .WithNodeDeserializer(new DefinitionCollectionNodeDeserializer())
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
                throw new SerializationException("Deserialization failed due to invalid YAML format.\n" + ex.Message);
            }

            ValidatePhase(phase);
            return phase;
        }

        private void ValidatePhase(MigrationPhase phase)
        {
            if (phase is null)
                throw new ArgumentException("Invalid input.");

            if (string.IsNullOrEmpty(phase.Title))
                throw new InvalidOperationException("Phase title is required.");

            var guidelines = phase.Guidelines;
            if (guidelines is null || (IsNullOrEmpty(guidelines.Create) && IsNullOrEmpty(guidelines.Update) && IsNullOrEmpty(guidelines.Delete)))
                throw new InvalidOperationException("Migration phase has no guidelines defined.");
        }

        private bool IsNullOrEmpty(IDictionary dictionary)
        {
            return dictionary is null || dictionary.Count == 0;
        }
    }
}

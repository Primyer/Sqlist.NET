using System;

namespace Sqlist.NET.Migration.Deserialization
{
    public class MigrationPhase
    {
        /// <summary>
        ///     Gets or sets the version of the phase.
        /// </summary>
        public Version Version { get; set; } = new();

        /// <summary>
        ///     Gets or sets the title describing the phase.
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        ///     Gets or sets an optional description of the phase.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        ///     Gets or sets the data-migration guidelines with in the phase.
        /// </summary>
        public PhaseGuidelines Guidelines { get; set; } = new PhaseGuidelines();
    }
}

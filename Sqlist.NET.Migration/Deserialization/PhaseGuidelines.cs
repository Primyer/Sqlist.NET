﻿using System.Collections.Generic;

namespace Sqlist.NET.Migration.Deserialization
{
    public class PhaseGuidelines
    {
        /// <summary>
        ///     Gets or sets the list of data structures to be added to the data map.
        /// </summary>
        public Dictionary<string, ColumnsDefinition> Create { get; set; } = new();

        /// <summary>
        ///     Gets or sets a list of rules for re-processing existing sets of data and re-structure the data map.
        /// </summary>
        public DataTransactionMap Update { get; set; } = new();

        /// <summary>
        ///     Gets or sets a list of data sets to be excluded from the data map.
        /// </summary>
        public Dictionary<string, string[]> Delete { get; set; } = new();
    }
}
using System;
using System.Collections.Generic;

using Sqlist.NET.Data;
using Sqlist.NET.Migration.Exceptions;

namespace Sqlist.NET.Migration;

/// <summary>
/// Represents a delegate that resolves conflicts between data transaction rules.
/// </summary>
/// <param name="table">The name of the table.</param>
/// <param name="column">The name of the column.</param>
/// <param name="existing">The existing data transaction rule.</param>
/// <param name="conflicting">The conflicting data transaction rule.</param>
/// <returns>The resolved data transaction rule.</returns>
public delegate DataTransactionRule ConflictResolver(string table, string column, DataTransactionRule existing, DataTransactionRule conflicting);

/// <summary>
/// Provides methods to merge data transaction maps.
/// </summary>
public static class DataTransactionMapMerger
{
    /// <summary>
    /// Merges multiple <see cref="DataTransactionMap"/> instances into a single map, ensuring no conflicts in transfer definitions.
    /// </summary>
    /// <param name="maps">The collection of maps to merge.</param>
    /// <param name="resolver">An optional conflict resolver to handle rule conflicts.</param>
    /// <returns>The merged <see cref="DataTransactionMap"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="maps"/> is null.</exception>
    public static DataTransactionMap SafeMerge(IEnumerable<DataTransactionMap> maps, ConflictResolver? resolver = null)
    {
        ArgumentNullException.ThrowIfNull(maps);
        
        var result = new DataTransactionMap();

        foreach (var map in maps)
        {
            MergeRules(map, result, resolver);
            
            foreach (var (table, definitions) in map.TransferDefinitions)
            {
                if (!result.TransferDefinitions.TryAdd(table, definitions))
                {
                    throw new InvalidOperationException($"Conflict detected for transfer definition of table '{table}'.");
                }
            }
        }

        return result;
    }
    
    /// <summary>
    /// Merges the source <see cref="DataTransactionMap"/> into the target one, including transfer definitions and resolving conflicts.
    /// </summary>
    /// <param name="source">The source map to merge from.</param>
    /// <param name="target">The target map to merge into.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="target"/> is null.</exception>
    public static void FullMerge(DataTransactionMap source, DataTransactionMap target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        
        MergeTransferDefinitions(source, target);
        MergeRules(source, target, (_, _, _, conflicting) => conflicting);
    }

    /// <summary>
    /// Merges the source <see cref="DataTransactionMap"/> into the target one, handling rule conflicts.
    /// </summary>
    /// <remarks>
    /// This method handles rule conflicts when detected during the merge process by either using the <paramref name="resolver"/>,
    /// if provided, or throwing an <see cref="InvalidOperationException"/> otherwise.
    /// </remarks>
    /// <param name="source">The source <see cref="DataTransactionMap"/> to merge from.</param>
    /// <param name="target">The target <see cref="DataTransactionMap"/> to merge into.</param>
    /// <param name="resolver">An optional <see cref="ConflictResolver"/> to handle conflicts during the merge.</param>
    private static void MergeRules(DataTransactionMap source, DataTransactionMap target, ConflictResolver? resolver)
    {
        try
        {
            // Merge transaction rules
            foreach (var (table, columns) in source)
            {
                if (!target.TryGetValue(table, out var rules))
                {
                    target[table] = rules = new TransactionRuleDictionary { Condition = columns.Condition };
                }

                foreach (var (column, rule) in columns)
                {
                    if (!rules.TryGetValue(column, out var existingRule))
                    {
                        rules[column] = rule;
                    }
                    else if (resolver is not null)
                    {
                        rules[column] = resolver(table, column, existingRule, rule);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Conflict detected for table '{table}', column '{column}'.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new MigrationException("Failed to merge data transaction maps.", ex);
        }
    }
    
    /// <summary>
    /// Merges the transfer definitions from the source <see cref="DataTransactionMap"/> into the target one.
    /// </summary>
    /// <param name="source">The source map to merge from.</param>
    /// <param name="target">The target map to merge into.</param>
    private static void MergeTransferDefinitions(DataTransactionMap source, DataTransactionMap target)
    {
        foreach (var (table, columns) in source)
        {
            if (!target.TransferDefinitions.TryGetValue(table, out var transferDefinition))
                continue;

            foreach (var (column, rule) in columns)
            {
                if (!string.IsNullOrEmpty(rule.ColumnName) && transferDefinition.Columns.ContainsKey(column))
                {
                    transferDefinition.Columns[column] = rule.ColumnName;
                }
            }
        }
    }
}

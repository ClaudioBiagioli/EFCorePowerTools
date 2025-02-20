﻿using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace RevEng.Core.Mermaid
{
    public class DatabaseModelToMermaid
    {
        private readonly DatabaseModel databaseModel;

        public DatabaseModelToMermaid(DatabaseModel databaseModel)
        {
            this.databaseModel = databaseModel;
        }

        public string CreateMermaid()
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine(CultureInfo.InvariantCulture, $"## {databaseModel.DatabaseName}");
            sb.AppendLine();

            sb.AppendLine("```mermaid");
            sb.AppendLine("erDiagram");

            foreach (var table in databaseModel.Tables)
            {
                if (table.ForeignKeys.Count == 0 && table.PrimaryKey == null)
                {
                    continue;
                }

                var formattedTableName = table.Name.Replace(" ", string.Empty, System.StringComparison.OrdinalIgnoreCase);

                sb.AppendLine(CultureInfo.InvariantCulture, $"  {formattedTableName} {{");
                foreach (var column in table.Columns)
                {
                    var pkfk = string.Empty;

                    if (table.PrimaryKey?.Columns.Contains(column) ?? false)
                    {
                        pkfk = "PK";
                    }

                    if (table.ForeignKeys.Any(c => c.Columns.Contains(column)))
                    {
                        pkfk = string.IsNullOrEmpty(pkfk) ? "FK" : "PK,FK";
                    }

                    var nullable = column.IsNullable ? "(NULL)" : string.Empty;
                    sb.AppendLine(CultureInfo.InvariantCulture, $"    {column.Name} {column.StoreType?.Replace(", ", "-", System.StringComparison.OrdinalIgnoreCase)}{nullable} {pkfk}");
                }

                sb.AppendLine("  }");

                foreach (var foreignKey in table.ForeignKeys)
                {
                    var relationship = "}o--|";

                    var dependentIndexes = foreignKey.PrincipalTable.Indexes
                        .Where(i => i.Columns.SequenceEqual(foreignKey.PrincipalColumns));

                    if (dependentIndexes.Any(i => i.IsUnique))
                    {
                        relationship = "|o--|";
                    }

                    if (foreignKey.PrincipalColumns.Any(c => c.IsNullable))
                    {
                        relationship = "}o--o";
                    }

                    sb.AppendLine(CultureInfo.InvariantCulture, $"  {formattedTableName} {relationship}| {foreignKey.PrincipalTable.Name} : {foreignKey.Name}");
                }
            }

            sb.AppendLine("```");

            return sb.ToString();
        }
    }
}

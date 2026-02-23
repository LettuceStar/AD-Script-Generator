using ClosedXML.Excel;

namespace AdScript.Core.Services.Excel;

public static class HeaderMapBuilder
{
    /// <summary>
    /// Returns a map from canonical key -> column number.
    /// Example: "FirstName" -> 2, "EmployeeId" -> 1
    /// </summary>
    public static Dictionary<string, int> BuildCanonicalHeaderMap(IXLRow headerRow)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var cell in headerRow.CellsUsed())
        {
            var raw = cell.GetString();
            var norm = HeaderAliases.NormalizeHeader(raw);

            if (string.IsNullOrEmpty(norm)) continue;

            foreach (var (canonical, aliases) in HeaderAliases.Aliases)
            {
                foreach (var a in aliases)
                {
                    if (norm == HeaderAliases.NormalizeHeader(a))
                    {
                        // First match wins (do not overwrite if duplicates)
                        if (!map.ContainsKey(canonical))
                            map[canonical] = cell.Address.ColumnNumber;

                        break;
                    }
                }
            }
        }

        return map;
    }
}
using ClosedXML.Excel;

namespace AdScript.Core.Services.Excel;

public static class HeaderRowDetector
{
    /// <summary>
    /// Finds the most likely header row by scanning the first N rows and scoring alias matches.
    /// This handles sheets with logos/titles/summary rows above the table.
    /// </summary>

    // Required canonical keys (internal identifiers)
    private static readonly string[] RequiredHeaders =
    {
        HeaderAliases.ID,
        HeaderAliases.FirstName,
        HeaderAliases.LastName
    };

    public static IXLRow? FindHeaderRow(IXLRange usedRange, int maxScanRows = 50)
    {
        // Scan from the first used row downwards to find the best header candidate
        var firstRowNumber = usedRange.FirstRow().RowNumber();
        var lastRowNumber = Math.Min(usedRange.LastRow().RowNumber(), firstRowNumber + maxScanRows - 1);

        IXLRow? bestRow = null;
        int bestScore = -1;

        for (int r = firstRowNumber; r <= lastRowNumber; r++)
        {
            var row = usedRange.Worksheet.Row(r);
            var map = HeaderMapBuilder.BuildCanonicalHeaderMap(row);

            // Score based on how many required headers are present
            int score = RequiredHeaders.Count(h => map.ContainsKey(h));

            // Prefer higher score; if tie, prefer earlier row
            if (score > bestScore)
            {
                bestScore = score;
                bestRow = row;

                // Early exit if we already have all required headers
                if (bestScore == RequiredHeaders.Length)
                    return bestRow;
            }
        }

        // Return the best match only if it meets the minimum requirement
        return (bestScore == RequiredHeaders.Length) ? bestRow : null;
    }

    public static void ValidateRequiredHeaders(
        Dictionary<string, int> headerMap,
        List<string> errors)
    {
        foreach (var h in RequiredHeaders)
        {
            if (!headerMap.ContainsKey(h))
                errors.Add($"Missing required column: {h}");
        }
    }
}

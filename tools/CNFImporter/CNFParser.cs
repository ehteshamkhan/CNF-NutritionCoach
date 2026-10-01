using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CNFImporter;

public static class CNFParser
{
    private static readonly TimeSpan RegexTimeout =
        TimeSpan.FromSeconds(5);

    private static readonly CultureInfo Invariant =
        CultureInfo.InvariantCulture;

    public static IReadOnlyList<FoodRecord> ReadFoods(
        string csvPath)
    {
        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException(
                "Canonical food CSV was not found.",
                csvPath);
        }

        var records = new List<FoodRecord>();

        using var reader = new StreamReader(
            csvPath,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true);

        string? header = reader.ReadLine();

        if (string.IsNullOrWhiteSpace(header))
        {
            throw new InvalidDataException(
                "Canonical food CSV has no header.");
        }

        var columns = ParseCsvLine(header);

        int foodCodeIndex = FindColumn(
            columns,
            "Food_Code");

        int foodNameIndex = FindColumn(
            columns,
            "Food_Description_EN");

        string? line;

        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var values = ParseCsvLine(line);

            if (values.Count <= foodCodeIndex ||
                values.Count <= foodNameIndex)
            {
                throw new InvalidDataException(
                    "Canonical food CSV row has fewer columns than expected.");
            }

            string codeText =
                values[foodCodeIndex].Trim();

            string name =
                NormalizeFoodName(values[foodNameIndex]);

            if (!int.TryParse(
                    codeText,
                    NumberStyles.Integer,
                    Invariant,
                    out int foodCode))
            {
                throw new InvalidDataException(
                    $"Invalid Food_Code '{codeText}'.");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidDataException(
                    $"Food Code {foodCode} has a blank Food_Description_EN.");
            }

            records.Add(
                new FoodRecord(
                    foodCode,
                    name));
        }

        return records;
    }

    private static IEnumerable<string> SplitRecords(
        string content,
        string recordHeader)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            yield break;
        }

        string normalized = content
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

        string[] parts = Regex.Split(
            normalized,
            "(?m)^[ \t]*" +
            Regex.Escape(recordHeader) +
            @"[ \t]*$");

        foreach (string part in parts)
        {
            string record = part.Trim();

            if (!string.IsNullOrWhiteSpace(record))
            {
                yield return recordHeader +
                    Environment.NewLine +
                    record;
            }
        }
    }
    public static IEnumerable<MeasureRecord> ReadMeasures(
        IEnumerable<string> files)
    {
        foreach (string file in files.OrderBy(x => x))
        {
            string content = File.ReadAllText(
                file,
                Encoding.UTF8);

            foreach (string record in SplitRecords(
                content,
                "CNF MEASURE RECORD"))
            {
                yield return ParseMeasure(
                    record,
                    file);
            }
        }
    }

    public static IEnumerable<NutrientRecord> ReadNutrients(
        IEnumerable<string> files)
    {
        foreach (string file in files.OrderBy(x => x))
        {
            string content = File.ReadAllText(
                file,
                Encoding.UTF8);

            foreach (string record in SplitRecords(
                content,
                "CNF NUTRIENT RECORD"))
            {
                yield return ParseNutrient(
                    record,
                    file);
            }
        }
    }

    public static MeasureRecord ParseMeasure(
        string record,
        string sourceFile)
    {
        int foodCode = ParseRequiredInt(
            GetField(record, "Food Code"),
            "Food Code",
            sourceFile);

        string foodName = NormalizeFoodName(
            RequireField(
                GetField(record, "Food"),
                "Food",
                sourceFile));

        int measureCode = ParseRequiredInt(
            GetField(record, "Measure Code"),
            "Measure Code",
            sourceFile);

        string measure = RequireField(
            GetField(record, "Measure"),
            "Measure",
            sourceFile);

        int measureTypeCode = ParseRequiredInt(
            GetField(record, "Measure Type Code"),
            "Measure Type Code",
            sourceFile);

        string measureType = RequireField(
            GetField(record, "Measure Type"),
            "Measure Type",
            sourceFile);

        string weightText = RequireField(
            GetField(
                record,
                "Measure Weight Conversion (grams)"),
            "Measure Weight Conversion (grams)",
            sourceFile);

        decimal weight = ParseDecimal(
            weightText,
            "Measure Weight Conversion (grams)",
            sourceFile);

        return new MeasureRecord(
            foodCode,
            foodName,
            measureCode,
            measure,
            measureTypeCode,
            measureType,
            weight);
    }

    public static NutrientRecord ParseNutrient(
        string record,
        string sourceFile)
    {
        int foodCode = ParseRequiredInt(
            GetField(record, "Food Code"),
            "Food Code",
            sourceFile);

        string foodName = NormalizeFoodName(
            RequireField(
                GetField(record, "Food"),
                "Food",
                sourceFile));

        int nutrientCode = ParseRequiredInt(
            GetField(record, "Nutrient Code"),
            "Nutrient Code",
            sourceFile);

        string nutrientName = RequireField(
            GetField(record, "Nutrient"),
            "Nutrient",
            sourceFile);

        string nutrientSymbol = RequireField(
            GetField(record, "Nutrient Symbol"),
            "Nutrient Symbol",
            sourceFile);

        string nutrientUnit = RequireField(
            GetField(record, "Nutrient Unit"),
            "Nutrient Unit",
            sourceFile);

        decimal amount = ParseDecimal(
            RequireField(
                GetField(
                    record,
                    "Nutrient Amount (per 100 g)"),
                "Nutrient Amount (per 100 g)",
                sourceFile),
            "Nutrient Amount (per 100 g)",
            sourceFile);

        decimal? standardError =
            ParseNullableDecimal(
                GetField(record, "Standard Error"),
                "Standard Error",
                sourceFile);

        decimal? observations =
            ParseNullableDecimal(
                GetField(record, "Observations"),
                "Observations",
                sourceFile);

        int sourceCode = ParseRequiredInt(
            GetField(
                record,
                "Nutrient Source Code"),
            "Nutrient Source Code",
            sourceFile);

        DateTime lastUpdated =
            ParseDate(
                RequireField(
                    GetField(
                        record,
                        "Nutrient Last Updated Date"),
                    "Nutrient Last Updated Date",
                    sourceFile),
                "Nutrient Last Updated Date",
                sourceFile);

        return new NutrientRecord(
            foodCode,
            foodName,
            nutrientCode,
            nutrientName,
            nutrientSymbol,
            nutrientUnit,
            amount,
            standardError,
            observations,
            sourceCode,
            lastUpdated);
    }

    private static string? GetField(
        string record,
        string fieldName)
    {
        if (string.IsNullOrWhiteSpace(record))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(fieldName))
        {
            throw new ArgumentException(
                "Field name cannot be null, empty, or whitespace.",
                nameof(fieldName));
        }

        string escapedFieldName =
            Regex.Escape(fieldName);

        string pattern =
            $@"(?im)^[ \t]*{escapedFieldName}[ \t]*:[ \t]*([^\r\n]*)";

        Match match = Regex.Match(
            record,
            pattern,
            RegexOptions.IgnoreCase |
            RegexOptions.Multiline,
            RegexTimeout);

        if (!match.Success)
        {
            return null;
        }

        return match.Groups[1].Value.Trim();
    }

    private static string RequireField(
        string? value,
        string fieldName,
        string sourceFile)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidDataException(
                $"Required field '{fieldName}' is blank in '{sourceFile}'.");
        }

        return value.Trim();
    }

    private static int ParseRequiredInt(
        string? value,
        string fieldName,
        string sourceFile)
    {
        string text = RequireField(
            value,
            fieldName,
            sourceFile);

        if (!int.TryParse(
                text,
                NumberStyles.Integer,
                Invariant,
                out int result))
        {
            throw new InvalidDataException(
                $"Field '{fieldName}' has invalid integer '{text}' in '{sourceFile}'.");
        }

        return result;
    }

    private static decimal ParseDecimal(
        string value,
        string fieldName,
        string sourceFile)
    {
        if (!decimal.TryParse(
                value.Trim(),
                NumberStyles.Float,
                Invariant,
                out decimal result))
        {
            throw new InvalidDataException(
                $"Field '{fieldName}' has invalid decimal '{value}' in '{sourceFile}'.");
        }

        return result;
    }

    private static decimal? ParseNullableDecimal(
        string? value,
        string fieldName,
        string sourceFile)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return ParseDecimal(
            value,
            fieldName,
            sourceFile);
    }

    private static DateTime ParseDate(
        string value,
        string fieldName,
        string sourceFile)
    {
        if (!DateTime.TryParseExact(
                value.Trim(),
                "yyyy-MM-dd",
                Invariant,
                DateTimeStyles.None,
                out DateTime result))
        {
            throw new InvalidDataException(
                $"Field '{fieldName}' has invalid date '{value}' in '{sourceFile}'.");
        }

        return result;
    }

    private static string NormalizeFoodName(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return Regex.Replace(
            value.Trim(),
            @"\s+",
            " ");
    }

    private static int FindColumn(
        IReadOnlyList<string> columns,
        string name)
    {
        for (int i = 0; i < columns.Count; i++)
        {
            if (string.Equals(
                    columns[i].Trim(),
                    name,
                    StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        throw new InvalidDataException(
            $"Required CSV column '{name}' was not found.");
    }

    private static List<string> ParseCsvLine(
        string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        bool quoted = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (quoted &&
                    i + 1 < line.Length &&
                    line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    quoted = !quoted;
                }

                continue;
            }

            if (c == ',' && !quoted)
            {
                values.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(c);
        }

        values.Add(current.ToString());

        return values;
    }
}
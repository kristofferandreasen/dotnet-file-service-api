namespace DotNet.FileService.Api.Helpers;

/// <summary>
/// Helper class for parsing tag strings into key-value dictionaries.
/// </summary>
public static class RequestHelper
{
    /// <summary>
    /// Parses a comma-separated list of key=value pairs into a dictionary.
    /// </summary>
    /// <param name="dictionary">The input string containing key=value pairs, e.g. "key1=value1,key2=value2".</param>
    /// <param name="throwOnError">
    /// If true, throws exceptions on invalid input;
    /// if false, returns an empty dictionary on invalid input.
    /// </param>
    /// <returns>A dictionary with keys and values parsed from the input string.</returns>
    public static Dictionary<string, string> ParseDictionary(
        string? dictionary,
        bool throwOnError = true)
    {
        if (string.IsNullOrWhiteSpace(dictionary))
        {
            if (throwOnError)
            {
                throw new ArgumentException("Input string must not be empty.", nameof(dictionary));
            }

            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        var tagFilters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var splitTags = dictionary.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var t in splitTags)
        {
            var parts = t.Split('=', 2);
            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]))
            {
                if (throwOnError)
                {
                    throw new FormatException($"'{t}' is invalid. Input must be in key=value format.");
                }

                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            tagFilters[parts[0].Trim()] = parts[1].Trim();
        }

        return tagFilters;
    }
}

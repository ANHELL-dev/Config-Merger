using System;
using System.Collections.Generic;
using System.Linq;

namespace ConfigMerger.Parsers
{
    public interface IConfigParser
    {
        Dictionary<string, string> ParseConfig(string content);
        string GenerateMergedConfig(Dictionary<string, string> merged, string originalContent);
    }

    public class YamlConfigParser : IConfigParser
    {
        public Dictionary<string, string> ParseConfig(string content)
        {
            var config = new Dictionary<string, string>();
            var lines = content.Split('\n');
            var currentPath = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var trimmedLine = line.Trim();

                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                var indentLevel = GetIndentLevel(line);

                if (trimmedLine.Contains(":"))
                {
                    var parts = trimmedLine.Split(new[] { ':' }, 2);
                    var key = parts[0].Trim();
                    var value = parts.Length > 1 ? parts[1].Trim() : "";

                    UpdateCurrentPath(currentPath, indentLevel, key);

                    if (!string.IsNullOrEmpty(value))
                    {
                        var fullKey = string.Join(".", currentPath);
                        config[fullKey] = ProcessValue(value);
                    }
                    else
                    {
                        var multiLineValue = ProcessMultiLineValue(lines, ref i, indentLevel);
                        if (!string.IsNullOrEmpty(multiLineValue))
                        {
                            var fullKey = string.Join(".", currentPath);
                            config[fullKey] = multiLineValue;
                        }
                    }
                }
                else if (trimmedLine.StartsWith("-"))
                {
                    var arrayValue = trimmedLine.Substring(1).Trim();
                    var arrayIndex = GetArrayIndex(config, currentPath);
                    var fullKey = string.Join(".", currentPath) + $"[{arrayIndex}]";
                    config[fullKey] = ProcessValue(arrayValue);
                }
            }

            return config;
        }

        private int GetIndentLevel(string line)
        {
            int spaces = 0;
            foreach (char c in line)
            {
                if (c == ' ')
                    spaces++;
                else if (c == '\t')
                    spaces += 4;
                else
                    break;
            }
            return spaces / 2;
        }

        private void UpdateCurrentPath(List<string> currentPath, int indentLevel, string key)
        {
            while (currentPath.Count > indentLevel)
            {
                currentPath.RemoveAt(currentPath.Count - 1);
            }

            currentPath.Add(key);
        }

        private string ProcessValue(string value)
        {
            if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                (value.StartsWith("'") && value.EndsWith("'")))
            {
                return value.Substring(1, value.Length - 2);
            }

            return value;
        }

        private string ProcessMultiLineValue(string[] lines, ref int currentIndex, int baseIndentLevel)
        {
            var multiLineValue = "";
            var nextLineIndex = currentIndex + 1;

            while (nextLineIndex < lines.Length)
            {
                var nextLine = lines[nextLineIndex];
                var nextTrimmed = nextLine.Trim();

                if (string.IsNullOrEmpty(nextTrimmed) || nextTrimmed.StartsWith("#"))
                {
                    nextLineIndex++;
                    continue;
                }

                var nextIndentLevel = GetIndentLevel(nextLine);

                if (nextIndentLevel <= baseIndentLevel)
                    break;

                if (!string.IsNullOrEmpty(multiLineValue))
                    multiLineValue += "\n";
                multiLineValue += nextTrimmed;

                nextLineIndex++;
            }

            currentIndex = nextLineIndex - 1;
            return multiLineValue;
        }

        private int GetArrayIndex(Dictionary<string, string> config, List<string> currentPath)
        {
            var basePath = string.Join(".", currentPath);
            return config.Keys.Count(k => k.StartsWith(basePath + "["));
        }

        public string GenerateMergedConfig(Dictionary<string, string> merged, string originalContent)
        {
            var result = new List<string>();
            var processedPaths = new HashSet<string>();

            var groupedKeys = GroupKeysByLevel(merged.Keys);

            foreach (var levelGroup in groupedKeys.OrderBy(g => g.Key))
            {
                foreach (var key in levelGroup.Value.OrderBy(k => k))
                {
                    if (!processedPaths.Contains(key))
                    {
                        GenerateYamlLine(key, merged[key], result, 0);
                        processedPaths.Add(key);
                    }
                }
            }

            return string.Join("\n", result);
        }

        private Dictionary<int, List<string>> GroupKeysByLevel(IEnumerable<string> keys)
        {
            var grouped = new Dictionary<int, List<string>>();

            foreach (var key in keys)
            {
                var level = key.Count(c => c == '.');
                if (!grouped.ContainsKey(level))
                    grouped[level] = new List<string>();
                grouped[level].Add(key);
            }

            return grouped;
        }

        private void GenerateYamlLine(string key, string value, List<string> result, int indentLevel)
        {
            var indent = new string(' ', indentLevel * 2);
            var keyParts = key.Split('.');
            var currentKey = keyParts.Last();

            if (currentKey.Contains("[") && currentKey.Contains("]"))
            {
                result.Add($"{indent}- {value}");
            }
            else
            {
                var formattedValue = FormatYamlValue(value);
                result.Add($"{indent}{currentKey}: {formattedValue}");
            }
        }

        private string FormatYamlValue(string value)
        {
            if (value.Contains(":") || value.Contains("#") || value.Contains("'") ||
                value.Contains("\"") || value.Contains("\n"))
            {
                return $"\"{value.Replace("\"", "\\\"")}\"";
            }

            return value;
        }
    }
}
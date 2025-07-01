using ConfigMerger.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConfigMerger
{
    public static class ConfigParserFactory
    {
        public static IConfigParser GetParser(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            return extension switch
            {
                ".py" => new PythonConfigParser(),
                ".yaml" or ".yml" => new YamlConfigParser(),
                ".json" => new JsonConfigParser(),
                ".xml" => new XmlConfigParser(),
                _ => throw new NotSupportedException($"Формат файла {extension} не поддерживается")
            };
        }

        public static bool IsSupported(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension is ".py" or ".yaml" or ".yml" or ".json" or ".xml";
        }

        public static string GetSupportedFormats()
        {
            return "Python (*.py)|*.py|YAML (*.yaml;*.yml)|*.yaml;*.yml|JSON (*.json)|*.json|XML (*.xml)|*.xml|All supported|*.py;*.yaml;*.yml;*.json;*.xml";
        }
    }

    public class PythonConfigParser : IConfigParser
    {
        public Dictionary<string, string> ParseConfig(string content)
        {
            var config = new Dictionary<string, string>();
            var lines = content.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                if (line.StartsWith("#") || string.IsNullOrEmpty(line)) continue;

                var match = Regex.Match(line, @"^([A-Z_][A-Z0-9_]*)\s*=\s*(.*)$");
                if (match.Success)
                {
                    var key = match.Groups[1].Value;
                    var value = match.Groups[2].Value.Trim();

                    // Обработка многострочных блоков
                    if (value.Contains("{") && !value.Contains("}"))
                    {
                        var fullValue = value;
                        var j = i + 1;
                        var braceCount = value.Count(c => c == '{');

                        while (j < lines.Length && braceCount > 0)
                        {
                            var nextLine = lines[j].Trim();
                            fullValue += "\n" + nextLine;
                            braceCount += nextLine.Count(c => c == '{');
                            braceCount -= nextLine.Count(c => c == '}');
                            j++;
                        }

                        value = fullValue;
                        i = j - 1;
                    }

                    config[key] = value;
                }
            }

            return config;
        }

        public string GenerateMergedConfig(Dictionary<string, string> merged, string originalContent)
        {
            var lines = originalContent.Split('\n').ToList();
            var result = new List<string>();
            var processedKeys = new HashSet<string>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                if (trimmedLine.StartsWith("#") || string.IsNullOrEmpty(trimmedLine))
                {
                    result.Add(line);
                    continue;
                }

                var match = Regex.Match(trimmedLine, @"^([A-Z_][A-Z0-9_]*)\s*=\s*(.*)$");
                if (match.Success)
                {
                    var key = match.Groups[1].Value;
                    if (merged.ContainsKey(key))
                    {
                        result.Add($"{key} = {merged[key]}");
                        processedKeys.Add(key);
                    }
                    else
                    {
                        result.Add(line);
                    }
                }
                else
                {
                    result.Add(line);
                }
            }

            // Добавляем новые ключи
            foreach (var kvp in merged.Where(x => !processedKeys.Contains(x.Key)))
            {
                result.Add($"{kvp.Key} = {kvp.Value}");
            }

            return string.Join("\n", result);
        }
    }
}
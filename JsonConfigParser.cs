using ConfigMerger.Parsers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConfigMerger
{
    public class JsonConfigParser : IConfigParser
    {
        public Dictionary<string, string> ParseConfig(string content)
        {
            var config = new Dictionary<string, string>();

            try
            {
                var jsonObject = JToken.Parse(content);
                ProcessJsonToken(jsonObject, "", config);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Ошибка парсинга JSON: {ex.Message}", ex);
            }

            return config;
        }

        private void ProcessJsonToken(JToken token, string prefix, Dictionary<string, string> config)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    var obj = (JObject)token;
                    foreach (var property in obj.Properties())
                    {
                        string key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                        ProcessJsonToken(property.Value, key, config);
                    }
                    break;

                case JTokenType.Array:
                    var array = (JArray)token;
                    for (int i = 0; i < array.Count; i++)
                    {
                        string key = $"{prefix}[{i}]";
                        ProcessJsonToken(array[i], key, config);
                    }
                    break;

                case JTokenType.String:
                    config[prefix] = token.Value<string>() ?? "";
                    break;

                case JTokenType.Integer:
                case JTokenType.Float:
                    config[prefix] = token.ToString();
                    break;

                case JTokenType.Boolean:
                    config[prefix] = token.Value<bool>().ToString().ToLower();
                    break;

                case JTokenType.Null:
                    config[prefix] = "null";
                    break;

                case JTokenType.Date:
                    config[prefix] = token.Value<DateTime>().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                    break;

                default:
                    config[prefix] = token.ToString();
                    break;
            }
        }

        public string GenerateMergedConfig(Dictionary<string, string> merged, string originalContent)
        {
            try
            {
                var jsonObject = BuildJsonObject(merged);
                return JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка генерации JSON: {ex.Message}", ex);
            }
        }

        private JToken BuildJsonObject(Dictionary<string, string> config)
        {
            var result = new JObject();

            foreach (var kvp in config.OrderBy(x => x.Key))
            {
                SetNestedValue(result, kvp.Key, ParseValue(kvp.Value));
            }

            return result;
        }

        private void SetNestedValue(JObject obj, string path, JToken value)
        {
            var parts = path.Split('.');
            JToken current = obj;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                var part = parts[i];

                if (part.Contains("[") && part.Contains("]"))
                {
                    current = ProcessArrayNavigation(current, part, i == parts.Length - 2 ? parts[i + 1] : null, value);
                    if (i == parts.Length - 2) return;
                }
                else
                {
                    var currentObj = (JObject)current;
                    if (currentObj[part] == null)
                        currentObj[part] = new JObject();
                    current = currentObj[part];
                }
            }

            var lastKey = parts[parts.Length - 1];
            if (lastKey.Contains("[") && lastKey.Contains("]"))
            {
                ProcessArrayAssignment(current, lastKey, value);
            }
            else
            {
                var currentObj = (JObject)current;
                currentObj[lastKey] = value;
            }
        }

        private JToken ProcessArrayNavigation(JToken current, string arrayPart, string nextPart, JToken value)
        {
            var arrayKey = arrayPart.Substring(0, arrayPart.IndexOf('['));
            var arrayIndex = int.Parse(arrayPart.Substring(arrayPart.IndexOf('[') + 1, arrayPart.IndexOf(']') - arrayPart.IndexOf('[') - 1));

            var currentObj = (JObject)current;
            if (currentObj[arrayKey] == null)
                currentObj[arrayKey] = new JArray();

            var array = (JArray)currentObj[arrayKey];

            while (array.Count <= arrayIndex)
                array.Add(new JObject());

            if (nextPart != null && nextPart.Contains("["))
            {
                var nestedKey = nextPart.Substring(0, nextPart.IndexOf('['));
                var nestedIndex = int.Parse(nextPart.Substring(nextPart.IndexOf('[') + 1, nextPart.IndexOf(']') - nextPart.IndexOf('[') - 1));

                var nestedObj = (JObject)array[arrayIndex];
                if (nestedObj[nestedKey] == null)
                    nestedObj[nestedKey] = new JArray();

                var nestedArray = (JArray)nestedObj[nestedKey];
                while (nestedArray.Count <= nestedIndex)
                    nestedArray.Add(JValue.CreateNull());

                nestedArray[nestedIndex] = value;
            }
            else if (nextPart != null)
            {
                var nestedObj = (JObject)array[arrayIndex];
                nestedObj[nextPart] = value;
            }

            return array[arrayIndex];
        }

        private void ProcessArrayAssignment(JToken current, string arrayKey, JToken value)
        {
            var keyName = arrayKey.Substring(0, arrayKey.IndexOf('['));
            var index = int.Parse(arrayKey.Substring(arrayKey.IndexOf('[') + 1, arrayKey.IndexOf(']') - arrayKey.IndexOf('[') - 1));

            var currentObj = (JObject)current;
            if (currentObj[keyName] == null)
                currentObj[keyName] = new JArray();

            var array = (JArray)currentObj[keyName];
            while (array.Count <= index)
                array.Add(JValue.CreateNull());

            array[index] = value;
        }

        private JToken ParseValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return JValue.CreateString("");

            if (int.TryParse(value, out int intValue))
                return new JValue(intValue);

            if (double.TryParse(value, out double doubleValue))
                return new JValue(doubleValue);

            if (bool.TryParse(value, out bool boolValue))
                return new JValue(boolValue);

            if (value.Equals("null", StringComparison.OrdinalIgnoreCase))
                return JValue.CreateNull();

            if (DateTime.TryParse(value, out DateTime dateValue))
                return new JValue(dateValue);

            return new JValue(value);
        }
    }
}
using ConfigMerger.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ConfigMerger
{
    public class XmlConfigParser : IConfigParser
    {
        public Dictionary<string, string> ParseConfig(string content)
        {
            var config = new Dictionary<string, string>();

            try
            {
                var doc = XDocument.Parse(content);
                ProcessXmlElement(doc.Root, "", config);
            }
            catch (XmlException ex)
            {
                throw new InvalidOperationException($"Ошибка парсинга XML: {ex.Message}", ex);
            }

            return config;
        }

        private void ProcessXmlElement(XElement element, string prefix, Dictionary<string, string> config)
        {
            if (element == null) return;

            string currentPath = string.IsNullOrEmpty(prefix) ? element.Name.LocalName : $"{prefix}.{element.Name.LocalName}";

            // Обработка атрибутов
            foreach (var attr in element.Attributes())
            {
                string attrKey = $"{currentPath}@{attr.Name.LocalName}";
                config[attrKey] = attr.Value;
            }

            if (!element.HasElements)
            {
                if (!string.IsNullOrWhiteSpace(element.Value))
                {
                    config[currentPath] = element.Value.Trim();
                }
                else if (!element.Attributes().Any())
                {
                    config[currentPath] = "";
                }
            }
            else
            {
                var childGroups = element.Elements().GroupBy(e => e.Name.LocalName);

                foreach (var group in childGroups)
                {
                    if (group.Count() > 1)
                    {
                        // Массив элементов
                        int index = 0;
                        foreach (var child in group)
                        {
                            string arrayPath = $"{currentPath}.{child.Name.LocalName}[{index}]";
                            ProcessXmlElement(child, arrayPath.Substring(0, arrayPath.LastIndexOf('.')), config);
                            index++;
                        }
                    }
                    else
                    {
                        ProcessXmlElement(group.First(), currentPath, config);
                    }
                }

                // Текст элемента с дочерними элементами
                var directText = element.Nodes().OfType<XText>()
                    .Select(t => t.Value.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(directText))
                {
                    config[$"{currentPath}#text"] = directText;
                }
            }
        }

        public string GenerateMergedConfig(Dictionary<string, string> merged, string originalContent)
        {
            try
            {
                var originalDoc = XDocument.Parse(originalContent);
                string rootElementName = originalDoc.Root?.Name.LocalName ?? "root";

                var newDoc = new XDocument(new XDeclaration("1.0", "utf-8", null));
                var rootElement = new XElement(rootElementName);
                newDoc.Add(rootElement);

                var structuredData = BuildXmlStructure(merged);
                BuildXmlTree(rootElement, structuredData);

                return FormatXml(newDoc);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка генерации XML: {ex.Message}", ex);
            }
        }

        private Dictionary<string, object> BuildXmlStructure(Dictionary<string, string> config)
        {
            var structure = new Dictionary<string, object>();

            foreach (var kvp in config.OrderBy(x => x.Key))
            {
                SetNestedXmlValue(structure, kvp.Key, kvp.Value);
            }

            return structure;
        }

        private void SetNestedXmlValue(Dictionary<string, object> obj, string path, string value)
        {
            var parts = path.Split('.');
            var current = obj;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                var part = parts[i];

                if (part.Contains("[") && part.Contains("]"))
                {
                    current = ProcessXmlArrayNavigation(current, part);
                }
                else
                {
                    if (!current.ContainsKey(part))
                        current[part] = new Dictionary<string, object>();
                    current = (Dictionary<string, object>)current[part];
                }
            }

            var lastPart = parts[parts.Length - 1];
            ProcessLastXmlPart(current, lastPart, value);
        }

        private Dictionary<string, object> ProcessXmlArrayNavigation(Dictionary<string, object> current, string part)
        {
            var elementName = part.Substring(0, part.IndexOf('['));
            var index = int.Parse(part.Substring(part.IndexOf('[') + 1, part.IndexOf(']') - part.IndexOf('[') - 1));

            if (!current.ContainsKey(elementName))
                current[elementName] = new List<Dictionary<string, object>>();

            var list = (List<Dictionary<string, object>>)current[elementName];

            while (list.Count <= index)
                list.Add(new Dictionary<string, object>());

            return list[index];
        }

        private void ProcessLastXmlPart(Dictionary<string, object> current, string lastPart, string value)
        {
            if (lastPart.StartsWith("@"))
            {
                // Атрибут
                var attrName = lastPart.Substring(1);
                if (!current.ContainsKey("@attributes"))
                    current["@attributes"] = new Dictionary<string, string>();
                ((Dictionary<string, string>)current["@attributes"])[attrName] = value;
            }
            else if (lastPart == "#text")
            {
                // Текст элемента
                current["#text"] = value;
            }
            else if (lastPart.Contains("[") && lastPart.Contains("]"))
            {
                // Элемент массива
                var elementName = lastPart.Substring(0, lastPart.IndexOf('['));
                var index = int.Parse(lastPart.Substring(lastPart.IndexOf('[') + 1, lastPart.IndexOf(']') - lastPart.IndexOf('[') - 1));

                if (!current.ContainsKey(elementName))
                    current[elementName] = new List<string>();

                var list = (List<string>)current[elementName];
                while (list.Count <= index)
                    list.Add("");

                list[index] = value;
            }
            else
            {
                current[lastPart] = value;
            }
        }

        private void BuildXmlTree(XElement parent, Dictionary<string, object> structure)
        {
            foreach (var kvp in structure)
            {
                switch (kvp.Key)
                {
                    case "@attributes":
                        var attributes = (Dictionary<string, string>)kvp.Value;
                        foreach (var attr in attributes)
                            parent.SetAttributeValue(attr.Key, attr.Value);
                        break;

                    case "#text":
                        parent.Value = kvp.Value.ToString();
                        break;

                    default:
                        ProcessXmlStructureItem(parent, kvp);
                        break;
                }
            }
        }

        private void ProcessXmlStructureItem(XElement parent, KeyValuePair<string, object> kvp)
        {
            switch (kvp.Value)
            {
                case Dictionary<string, object> dict:
                    var childElement = new XElement(kvp.Key);
                    parent.Add(childElement);
                    BuildXmlTree(childElement, dict);
                    break;

                case List<Dictionary<string, object>> dictList:
                    foreach (var item in dictList)
                    {
                        var arrayElement = new XElement(kvp.Key);
                        parent.Add(arrayElement);
                        BuildXmlTree(arrayElement, item);
                    }
                    break;

                case List<string> stringList:
                    foreach (var item in stringList)
                    {
                        var simpleElement = new XElement(kvp.Key, item);
                        parent.Add(simpleElement);
                    }
                    break;

                default:
                    var valueElement = new XElement(kvp.Key, kvp.Value.ToString());
                    parent.Add(valueElement);
                    break;
            }
        }

        private string FormatXml(XDocument doc)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\n",
                NewLineHandling = NewLineHandling.Replace,
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false
            };

            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb, settings))
            {
                doc.Save(writer);
            }

            return sb.ToString();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ConfigMerger
{
    public partial class MainForm : Form
    {
        private Dictionary<string, string> sourceConfig = new Dictionary<string, string>();
        private Dictionary<string, string> targetConfig = new Dictionary<string, string>();
        private string sourceContent = "";
        private string targetContent = "";
        private string sourceFileName = "";
        private string targetFileName = "";

        public MainForm()
        {
            InitializeComponent();
            SetupStyles();
            SetupDragDrop(); // Включаем drag & drop
            AddSaveButtons(); // Добавляем кнопки сохранения
        }

        private string HtmlEncode(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            return text.Replace("&", "&amp;")
                      .Replace("<", "&lt;")
                      .Replace(">", "&gt;")
                      .Replace("\"", "&quot;")
                      .Replace("'", "&#39;");
        }

        // Добавление кнопок сохранения программно
        private void AddSaveButtons()
        {
            // Находим вкладку с объединенным конфигом
            foreach (TabPage tab in resultsTabControl.TabPages)
            {
                if (tab.Text.Contains("Объединенный конфиг"))
                {
                    // Находим панель внутри вкладки
                    var mergedPanel = tab.Controls[0] as Panel;
                    if (mergedPanel != null)
                    {
                        // Создаем панель для кнопок
                        var buttonPanel = new Panel
                        {
                            Dock = DockStyle.Top,
                            Height = 60,
                            BackColor = Color.FromArgb(248, 249, 250)
                        };

                        // Создаем FlowLayoutPanel для автоматического расположения кнопок
                        var flowPanel = new FlowLayoutPanel
                        {
                            Dock = DockStyle.Fill,
                            FlowDirection = FlowDirection.LeftToRight,
                            WrapContents = true,
                            Padding = new Padding(10, 10, 10, 10)
                        };

                        // Обновляем существующую кнопку "Скопировать"
                        copyBtn.Size = new Size(130, 30);
                        copyBtn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                        copyBtn.Margin = new Padding(0, 0, 10, 0);
                        copyBtn.TextAlign = ContentAlignment.MiddleCenter;

                        // Кнопка "Сохранить как..."
                        var saveAsBtn = new Button
                        {
                            Text = "💾 Сохранить как...",
                            Size = new Size(160, 30),
                            Font = new Font("Segoe UI", 9, FontStyle.Bold),
                            BackColor = Color.FromArgb(0, 123, 255),
                            ForeColor = Color.White,
                            FlatStyle = FlatStyle.Flat,
                            UseVisualStyleBackColor = false,
                            Margin = new Padding(0, 0, 10, 0),
                            TextAlign = ContentAlignment.MiddleCenter
                        };
                        saveAsBtn.FlatAppearance.BorderSize = 0;
                        saveAsBtn.Click += saveResultBtn_Click;

                        // Кнопка "Сохранить как целевой"
                        var saveAsTargetBtn = new Button
                        {
                            Text = "📁 Сохранить как целевой",
                            Size = new Size(200, 30),
                            Font = new Font("Segoe UI", 9, FontStyle.Bold),
                            BackColor = Color.FromArgb(108, 117, 125),
                            ForeColor = Color.White,
                            FlatStyle = FlatStyle.Flat,
                            UseVisualStyleBackColor = false,
                            Margin = new Padding(0, 0, 10, 0),
                            TextAlign = ContentAlignment.MiddleCenter
                        };
                        saveAsTargetBtn.FlatAppearance.BorderSize = 0;
                        saveAsTargetBtn.Click += saveAsTargetBtn_Click;

                        // Кнопка "Быстрое сохранение"
                        var quickSaveBtn = new Button
                        {
                            Text = "⚡ Быстрое сохранение",
                            Size = new Size(180, 30),
                            Font = new Font("Segoe UI", 9, FontStyle.Bold),
                            BackColor = Color.FromArgb(255, 193, 7),
                            ForeColor = Color.Black,
                            FlatStyle = FlatStyle.Flat,
                            UseVisualStyleBackColor = false,
                            Margin = new Padding(0, 0, 10, 0),
                            TextAlign = ContentAlignment.MiddleCenter
                        };
                        quickSaveBtn.FlatAppearance.BorderSize = 0;
                        quickSaveBtn.Click += quickSaveBtn_Click;

                        // Кнопка "Экспорт отчета"
                        var exportReportBtn = new Button
                        {
                            Text = "📊 Экспорт отчета",
                            Size = new Size(150, 30),
                            Font = new Font("Segoe UI", 9, FontStyle.Bold),
                            BackColor = Color.FromArgb(220, 53, 69),
                            ForeColor = Color.White,
                            FlatStyle = FlatStyle.Flat,
                            UseVisualStyleBackColor = false,
                            Margin = new Padding(0, 0, 10, 0),
                            TextAlign = ContentAlignment.MiddleCenter
                        };
                        exportReportBtn.FlatAppearance.BorderSize = 0;
                        exportReportBtn.Click += exportReportBtn_Click;

                        // Добавляем кнопки в FlowLayoutPanel
                        flowPanel.Controls.Add(copyBtn);
                        flowPanel.Controls.Add(saveAsBtn);
                        flowPanel.Controls.Add(saveAsTargetBtn);
                        flowPanel.Controls.Add(quickSaveBtn);
                        flowPanel.Controls.Add(exportReportBtn);

                        // Добавляем FlowLayoutPanel в buttonPanel
                        buttonPanel.Controls.Add(flowPanel);

                        // Добавляем панель кнопок в начало
                        mergedPanel.Controls.Add(buttonPanel);
                        buttonPanel.BringToFront();

                        // Обновляем отступы для текстового поля
                        if (mergedConfigTextBox != null)
                        {
                            mergedConfigTextBox.Location = new Point(10, 70);
                            mergedConfigTextBox.Size = new Size(
                                mergedPanel.Width - 20,
                                mergedPanel.Height - 80
                            );
                            mergedConfigTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                        }

                        break;
                    }
                }
            }
        }

        private void SetupStyles()
        {
            // Style buttons
            StyleButton(sourceSelectBtn, Color.FromArgb(79, 172, 254));
            StyleButton(targetSelectBtn, Color.FromArgb(79, 172, 254));
            StyleButton(compareBtn, Color.FromArgb(79, 172, 254));
            StyleButton(resetBtn, Color.FromArgb(108, 117, 125));
            StyleButton(copyBtn, Color.FromArgb(40, 167, 69));

            // Принудительно устанавливаем выравнивание для основных кнопок
            compareBtn.TextAlign = ContentAlignment.MiddleCenter;
            resetBtn.TextAlign = ContentAlignment.MiddleCenter;
            sourceSelectBtn.TextAlign = ContentAlignment.MiddleCenter;
            targetSelectBtn.TextAlign = ContentAlignment.MiddleCenter;
            copyBtn.TextAlign = ContentAlignment.MiddleCenter;

            // Initial state
            CheckReadyToCompare();
        }

        private void StyleButton(System.Windows.Forms.Button button, Color baseColor)
        {
            if (button == null) return;

            // Устанавливаем стиль после установки цветов
            button.BackColor = baseColor;
            button.ForeColor = Color.White;
            button.UseVisualStyleBackColor = false;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = baseColor;
            button.Cursor = Cursors.Hand;
            button.TextAlign = ContentAlignment.MiddleCenter;

            // Hover effects
            button.MouseEnter += (s, e) =>
            {
                if (button.Enabled)
                {
                    button.BackColor = ControlPaint.Dark(baseColor, 0.1f);
                    button.FlatAppearance.BorderColor = ControlPaint.Dark(baseColor, 0.1f);
                }
            };

            button.MouseLeave += (s, e) =>
            {
                if (button == compareBtn)
                    button.BackColor = button.Enabled ? baseColor : Color.Gray;
                else
                    button.BackColor = baseColor;

                button.FlatAppearance.BorderColor = button.BackColor;
            };
        }

        private void sourceSelectBtn_Click(object sender, EventArgs e)
        {
            SelectFile("source");
        }

        private void targetSelectBtn_Click(object sender, EventArgs e)
        {
            SelectFile("target");
        }

        private void SelectFile(string type)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Python files (*.py)|*.py|Text files (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.Title = type == "source" ? "Выберите исходный конфиг" : "Выберите целевой конфиг";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadConfigFile(dialog.FileName, type);
                }
            }
        }

        private Dictionary<string, string> ParseConfig(string content)
        {
            var config = new Dictionary<string, string>();
            var lines = content.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // Пропускаем комментарии и пустые строки
                if (line.StartsWith("#") || string.IsNullOrEmpty(line)) continue;

                // Ищем присваивания переменных
                var match = Regex.Match(line, @"^([A-Z_][A-Z0-9_]*)\s*=\s*(.*)$");
                if (match.Success)
                {
                    var key = match.Groups[1].Value;
                    var value = match.Groups[2].Value.Trim();

                    // Обрабатываем многострочные значения (словари, списки)
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

                    // ВАЖНО: перезаписываем значение если ключ уже существует
                    // Это обеспечивает использование последнего найденного значения
                    config[key] = value;
                }
            }

            return config;
        }

        private void CheckReadyToCompare()
        {
            compareBtn.Enabled = sourceConfig.Count > 0 && targetConfig.Count > 0;
            compareBtn.BackColor = compareBtn.Enabled ? Color.FromArgb(79, 172, 254) : Color.Gray;
        }

        private async void compareBtn_Click(object sender, EventArgs e)
        {
            progressBar.Visible = true;
            compareBtn.Enabled = false;

            // Simulate processing time
            await System.Threading.Tasks.Task.Delay(1000);

            var comparison = CompareConfigs();
            DisplayResults(comparison);

            progressBar.Visible = false;
            compareBtn.Enabled = true;
        }

        private ComparisonResult CompareConfigs()
        {
            var added = new List<ConfigParameter>();
            var missing = new List<ConfigParameter>();
            var different = new List<ConfigDifference>();
            var merged = new Dictionary<string, string>(targetConfig);

            // Находим добавленные и измененные параметры
            foreach (var kvp in sourceConfig)
            {
                if (!targetConfig.ContainsKey(kvp.Key))
                {
                    added.Add(new ConfigParameter { Key = kvp.Key, Value = kvp.Value });
                }
                else if (sourceConfig[kvp.Key] != targetConfig[kvp.Key])
                {
                    different.Add(new ConfigDifference
                    {
                        Key = kvp.Key,
                        OldValue = targetConfig[kvp.Key],
                        NewValue = sourceConfig[kvp.Key]
                    });
                }

                merged[kvp.Key] = kvp.Value;
            }

            // Находим отсутствующие параметры
            foreach (var kvp in targetConfig)
            {
                if (!sourceConfig.ContainsKey(kvp.Key))
                {
                    missing.Add(new ConfigParameter { Key = kvp.Key, Value = kvp.Value });
                }
            }

            return new ComparisonResult
            {
                Added = added,
                Missing = missing,
                Different = different,
                Merged = merged
            };
        }

        private void DisplayResults(ComparisonResult comparison)
        {
            // Сводка
            summaryLabel.Text = $"📊 Результаты сравнения\n\n" +
                               $"Новых параметров: {comparison.Added.Count}\n" +
                               $"Отсутствующих: {comparison.Missing.Count}\n" +
                               $"Измененных: {comparison.Different.Count}\n" +
                               $"Всего параметров: {comparison.Merged.Count}";

            // Объединенный конфиг
            mergedConfigTextBox.Text = GenerateMergedConfig(comparison.Merged);

            // Новые параметры
            addedListBox.Items.Clear();
            foreach (var param in comparison.Added)
            {
                addedListBox.Items.Add($"{param.Key} = {param.Value}");
            }

            // Отсутствующие параметры
            missingListBox.Items.Clear();
            foreach (var param in comparison.Missing)
            {
                missingListBox.Items.Add($"{param.Key} = {param.Value}");
            }

            // Измененные параметры
            differentListBox.Items.Clear();
            foreach (var diff in comparison.Different)
            {
                differentListBox.Items.Add($"{diff.Key}:");
                differentListBox.Items.Add($"  Было: {diff.OldValue}");
                differentListBox.Items.Add($"  Стало: {diff.NewValue}");
                differentListBox.Items.Add("");
            }

            resultsTabControl.SelectedIndex = 0; // Показываем сводку
        }

        private string GenerateMergedConfig(Dictionary<string, string> merged)
        {
            var lines = targetContent.Split('\n').ToList();
            var result = new List<string>();
            var processedKeys = new HashSet<string>();
            var skipNextLines = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                // Пропускаем строки, если они являются частью многострочного блока
                if (skipNextLines > 0)
                {
                    skipNextLines--;
                    continue;
                }

                var line = lines[i];
                var trimmedLine = line.Trim();

                // Сохраняем комментарии и пустые строки как есть
                if (trimmedLine.StartsWith("#") || string.IsNullOrEmpty(trimmedLine))
                {
                    result.Add(line);
                    continue;
                }

                // Ищем присваивания переменных
                var match = Regex.Match(trimmedLine, @"^([A-Z_][A-Z0-9_]*)\s*=\s*(.*)$");
                if (match.Success)
                {
                    var key = match.Groups[1].Value;
                    var originalValue = match.Groups[2].Value.Trim();

                    // Проверяем, не обработали ли мы уже этот ключ
                    if (!processedKeys.Contains(key))
                    {
                        // Если многострочное значение, подсчитываем количество строк для пропуска
                        if (originalValue.Contains("{") && !originalValue.Contains("}"))
                        {
                            var braceCount = originalValue.Count(c => c == '{');
                            var j = i + 1;

                            while (j < lines.Count && braceCount > 0)
                            {
                                var nextLine = lines[j].Trim();
                                braceCount += nextLine.Count(c => c == '{');
                                braceCount -= nextLine.Count(c => c == '}');
                                j++;
                                skipNextLines++;
                            }
                        }

                        // Используем значение из объединенного словаря, если оно есть
                        if (merged.ContainsKey(key))
                        {
                            result.Add($"{key} = {merged[key]}");
                        }
                        else
                        {
                            result.Add(line);
                        }

                        processedKeys.Add(key);
                    }
                    // Если ключ уже обработан, пропускаем дубликат
                    else
                    {
                        // Пропускаем многострочный блок, если это дубликат
                        if (originalValue.Contains("{") && !originalValue.Contains("}"))
                        {
                            var braceCount = originalValue.Count(c => c == '{');
                            var j = i + 1;

                            while (j < lines.Count && braceCount > 0)
                            {
                                var nextLine = lines[j].Trim();
                                braceCount += nextLine.Count(c => c == '{');
                                braceCount -= nextLine.Count(c => c == '}');
                                j++;
                                skipNextLines++;
                            }
                        }
                        // Пропускаем строку с дубликатом
                        continue;
                    }
                }
                else
                {
                    result.Add(line);
                }
            }

            // Добавляем новые параметры, которых не было в целевом файле
            var newParams = new List<string>();
            foreach (var kvp in merged)
            {
                if (!processedKeys.Contains(kvp.Key))
                {
                    newParams.Add($"{kvp.Key} = {kvp.Value}");
                }
            }

            // Если есть новые параметры, добавляем их в конец с комментарием
            if (newParams.Count > 0)
            {
                result.Add("");
                result.Add("# Новые параметры, добавленные из исходного конфига:");
                result.AddRange(newParams);
            }

            return string.Join("\n", result);
        }

        private void copyBtn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(mergedConfigTextBox.Text))
            {
                Clipboard.SetText(mergedConfigTextBox.Text);

                var originalText = copyBtn.Text;
                copyBtn.Text = "✅ Скопировано!";
                copyBtn.BackColor = Color.FromArgb(25, 135, 84);

                var timer = new Timer { Interval = 2000 };
                timer.Tick += (s, args) =>
                {
                    copyBtn.Text = originalText;
                    copyBtn.BackColor = Color.FromArgb(40, 167, 69);
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            }
        }

        private void resetBtn_Click(object sender, EventArgs e)
        {
            sourceConfig.Clear();
            targetConfig.Clear();
            sourceContent = "";
            targetContent = "";
            sourceFileName = "";
            targetFileName = "";

            sourceStatusLabel.Text = "Файл не выбран";
            sourceStatusLabel.ForeColor = Color.Gray;
            sourceGroupBox.BackColor = SystemColors.Control;

            targetStatusLabel.Text = "Файл не выбран";
            targetStatusLabel.ForeColor = Color.Gray;
            targetGroupBox.BackColor = SystemColors.Control;

            compareBtn.Enabled = false;
            compareBtn.BackColor = Color.Gray;

            mergedConfigTextBox.Text = "";
            addedListBox.Items.Clear();
            missingListBox.Items.Clear();
            differentListBox.Items.Clear();
            summaryLabel.Text = "";
        }

        // Drag & Drop поддержка
        private void SetupDragDrop()
        {
            // Включаем drag & drop для группбоксов
            sourceGroupBox.AllowDrop = true;
            targetGroupBox.AllowDrop = true;

            // События для исходного конфига
            sourceGroupBox.DragEnter += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    e.Effect = DragDropEffects.Copy;
                    sourceGroupBox.BackColor = Color.FromArgb(232, 245, 232);
                }
            };

            sourceGroupBox.DragLeave += (s, e) =>
            {
                sourceGroupBox.BackColor = SystemColors.Control;
            };

            sourceGroupBox.DragDrop += (s, e) =>
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    LoadConfigFile(files[0], "source");
                }
                sourceGroupBox.BackColor = SystemColors.Control;
            };

            // События для целевого конфига
            targetGroupBox.DragEnter += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    e.Effect = DragDropEffects.Copy;
                    targetGroupBox.BackColor = Color.FromArgb(232, 245, 232);
                }
            };

            targetGroupBox.DragLeave += (s, e) =>
            {
                targetGroupBox.BackColor = SystemColors.Control;
            };

            targetGroupBox.DragDrop += (s, e) =>
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    LoadConfigFile(files[0], "target");
                }
                targetGroupBox.BackColor = SystemColors.Control;
            };
        }

        // Загрузка файла (вынесено в отдельный метод)
        private void LoadConfigFile(string filePath, string type)
        {
            try
            {
                var content = File.ReadAllText(filePath);

                // Очищаем дубликаты перед парсингом
                content = CleanDuplicates(content);

                var config = ParseConfig(content);

                if (type == "source")
                {
                    sourceConfig = config;
                    sourceContent = content;
                    sourceFileName = Path.GetFileName(filePath);
                    sourceStatusLabel.Text = $"✅ Загружено: {sourceFileName} ({config.Count} параметров)";
                    sourceStatusLabel.ForeColor = Color.Green;
                    sourceGroupBox.BackColor = Color.FromArgb(232, 245, 232);
                }
                else
                {
                    targetConfig = config;
                    targetContent = content;
                    targetFileName = Path.GetFileName(filePath);
                    targetStatusLabel.Text = $"✅ Загружено: {targetFileName} ({config.Count} параметров)";
                    targetStatusLabel.ForeColor = Color.Green;
                    targetGroupBox.BackColor = Color.FromArgb(232, 245, 232);
                }

                CheckReadyToCompare();
            }
            catch (Exception ex)
            {
                var statusLabel = type == "source" ? sourceStatusLabel : targetStatusLabel;
                var groupBox = type == "source" ? sourceGroupBox : targetGroupBox;

                statusLabel.Text = $"❌ Ошибка: {ex.Message}";
                statusLabel.ForeColor = Color.Red;
                groupBox.BackColor = Color.FromArgb(255, 234, 234);

                MessageBox.Show($"Ошибка загрузки файла:\n{ex.Message}", "Ошибка",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Функция очистки дубликатов
        private string CleanDuplicates(string content)
        {
            var lines = content.Split('\n');
            var result = new List<string>();
            var seenKeys = new HashSet<string>();
            var keyToLastLine = new Dictionary<string, int>();

            // Первый проход - находим все ключи и их последние позиции
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                var match = Regex.Match(line, @"^([A-Z_][A-Z0-9_]*)\s*=\s*(.*)$");

                if (match.Success)
                {
                    var key = match.Groups[1].Value;
                    keyToLastLine[key] = i;
                }
            }

            // Второй проход - добавляем только последние вхождения
            var skipNextLines = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (skipNextLines > 0)
                {
                    skipNextLines--;
                    continue;
                }

                var line = lines[i];
                var trimmedLine = line.Trim();

                // Комментарии и пустые строки добавляем всегда
                if (trimmedLine.StartsWith("#") || string.IsNullOrEmpty(trimmedLine))
                {
                    result.Add(line);
                    continue;
                }

                var match = Regex.Match(trimmedLine, @"^([A-Z_][A-Z0-9_]*)\s*=\s*(.*)$");
                if (match.Success)
                {
                    var key = match.Groups[1].Value;
                    var value = match.Groups[2].Value.Trim();

                    // Добавляем только если это последнее вхождение ключа
                    if (keyToLastLine[key] == i)
                    {
                        result.Add(line);

                        // Обрабатываем многострочные значения
                        if (value.Contains("{") && !value.Contains("}"))
                        {
                            var braceCount = value.Count(c => c == '{');
                            var j = i + 1;

                            while (j < lines.Length && braceCount > 0)
                            {
                                var nextLine = lines[j].Trim();
                                result.Add(lines[j]);
                                braceCount += nextLine.Count(c => c == '{');
                                braceCount -= nextLine.Count(c => c == '}');
                                j++;
                                skipNextLines++;
                            }
                        }
                    }
                    else
                    {
                        // Пропускаем дубликат и его многострочное содержимое
                        if (value.Contains("{") && !value.Contains("}"))
                        {
                            var braceCount = value.Count(c => c == '{');
                            var j = i + 1;

                            while (j < lines.Length && braceCount > 0)
                            {
                                var nextLine = lines[j].Trim();
                                braceCount += nextLine.Count(c => c == '{');
                                braceCount -= nextLine.Count(c => c == '}');
                                j++;
                                skipNextLines++;
                            }
                        }
                    }
                }
                else
                {
                    result.Add(line);
                }
            }

            return string.Join("\n", result);
        }

        // Сохранение результата в файл (произвольное имя)
        private void saveResultBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(mergedConfigTextBox.Text))
            {
                MessageBox.Show("Нет данных для сохранения!\nСначала выполните сравнение файлов.", "Предупреждение",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Python files (*.py)|*.py|Text files (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.DefaultExt = "py";
                dialog.FileName = "merged_config.py";
                dialog.Title = "Сохранить объединенный конфиг";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(dialog.FileName, mergedConfigTextBox.Text);
                        MessageBox.Show($"Файл успешно сохранен:\n{dialog.FileName}", "Успех",
                                       MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка сохранения файла:\n{ex.Message}", "Ошибка",
                                       MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Сохранение с именем целевого файла
        private void saveAsTargetBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(mergedConfigTextBox.Text))
            {
                MessageBox.Show("Нет данных для сохранения!\nСначала выполните сравнение файлов.", "Предупреждение",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(targetFileName))
            {
                MessageBox.Show("Целевой файл не загружен!\nСначала загрузите целевой конфиг.", "Предупреждение",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Python files (*.py)|*.py|Text files (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.DefaultExt = Path.GetExtension(targetFileName);
                dialog.FileName = targetFileName; // Используем имя целевого файла
                dialog.Title = $"Сохранить как '{targetFileName}'";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(dialog.FileName, mergedConfigTextBox.Text);

                        var result = MessageBox.Show(
                            $"Файл успешно сохранен:\n{dialog.FileName}\n\n" +
                            "Хотите открыть папку с файлом?",
                            "Успех",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            // Открываем папку с файлом
                            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{dialog.FileName}\"");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка сохранения файла:\n{ex.Message}", "Ошибка",
                                       MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Быстрое сохранение (замена оригинального целевого файла)
        private void quickSaveBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(mergedConfigTextBox.Text))
            {
                MessageBox.Show("Нет данных для сохранения!\nСначала выполните сравнение файлов.", "Предупреждение",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(targetFileName))
            {
                MessageBox.Show("Целевой файл не загружен!\nСначала загрузите целевой конфиг.", "Предупреждение",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите заменить файл '{targetFileName}'?\n\n" +
                "⚠️ ВНИМАНИЕ: Оригинальный файл будет перезаписан!\n" +
                "Рекомендуется создать резервную копию.",
                "Подтверждение замены",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                // Создаем резервную копию
                var backupPath = CreateBackup();

                try
                {
                    // Определяем папку целевого файла
                    var targetDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    var targetPath = Path.Combine(targetDir, targetFileName);

                    File.WriteAllText(targetPath, mergedConfigTextBox.Text);

                    var message = $"Файл успешно сохранен:\n{targetPath}";
                    if (!string.IsNullOrEmpty(backupPath))
                    {
                        message += $"\n\nРезервная копия создана:\n{backupPath}";
                    }

                    var openResult = MessageBox.Show(
                        message + "\n\nХотите открыть папку с файлом?",
                        "Успех",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (openResult == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{targetPath}\"");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения файла:\n{ex.Message}", "Ошибка",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Создание резервной копии
        private string CreateBackup()
        {
            try
            {
                if (string.IsNullOrEmpty(targetContent) || string.IsNullOrEmpty(targetFileName))
                    return null;

                var backupDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ConfigMerger_Backups");
                Directory.CreateDirectory(backupDir);

                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var backupFileName = $"{Path.GetFileNameWithoutExtension(targetFileName)}_backup_{timestamp}{Path.GetExtension(targetFileName)}";
                var backupPath = Path.Combine(backupDir, backupFileName);

                File.WriteAllText(backupPath, targetContent);
                return backupPath;
            }
            catch
            {
                return null;
            }
        }

        // Экспорт отчета в разные форматы
        private void exportReportBtn_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "HTML Report (*.html)|*.html|Text Report (*.txt)|*.txt|CSV Report (*.csv)|*.csv",
                DefaultExt = "html",
                FileName = "config_comparison_report"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string content = "";
                    var ext = Path.GetExtension(dialog.FileName).ToLower();

                    switch (ext)
                    {
                        case ".html":
                            content = GenerateHtmlReport();
                            break;
                        case ".txt":
                            content = GenerateTextReport();
                            break;
                        case ".csv":
                            content = GenerateCsvReport();
                            break;
                    }

                    File.WriteAllText(dialog.FileName, content);
                    MessageBox.Show($"Отчет сохранен: {dialog.FileName}", "Успех",
                                   MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения отчета:\n{ex.Message}", "Ошибка",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Генерация HTML отчета
        private string GenerateHtmlReport()
        {
            var html = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Отчет сравнения конфигов</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; background: #f8f9fa; }
        .container { max-width: 1200px; margin: 0 auto; background: white; padding: 20px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        .header { background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%); color: white; padding: 30px; border-radius: 10px; text-align: center; margin-bottom: 30px; }
        .header h1 { margin: 0; font-size: 2em; }
        .stats { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 20px; margin-bottom: 30px; }
        .stat-card { background: #f8f9fa; padding: 20px; border-radius: 10px; text-align: center; border-left: 5px solid #4facfe; }
        .stat-number { font-size: 2em; font-weight: bold; color: #4facfe; }
        .stat-label { color: #666; margin-top: 5px; }
        .section { margin: 30px 0; }
        .section-title { font-size: 1.5em; font-weight: bold; margin-bottom: 15px; padding-bottom: 10px; border-bottom: 2px solid #eee; }
        .added { background: #d4edda; border-left: 5px solid #28a745; padding: 15px; border-radius: 5px; }
        .missing { background: #f8d7da; border-left: 5px solid #dc3545; padding: 15px; border-radius: 5px; }
        .different { background: #fff3cd; border-left: 5px solid #ffc107; padding: 15px; border-radius: 5px; }
        .config-line { font-family: 'Courier New', monospace; padding: 8px; margin: 5px 0; background: rgba(0,0,0,0.05); border-radius: 3px; word-break: break-all; }
        .file-info { background: #e9ecef; padding: 15px; border-radius: 5px; margin-bottom: 20px; }
        .diff-item { margin-bottom: 15px; }
        .old-value { color: #dc3545; }
        .new-value { color: #28a745; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔧 Отчет сравнения конфиг-файлов</h1>
            <p>Дата создания: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + @"</p>
        </div>";

            // Информация о файлах
            html += "<div class='file-info'>";
            html += "<h3>📁 Информация о файлах</h3>";
            if (!string.IsNullOrEmpty(sourceFileName))
                html += $"<p><strong>Исходный файл:</strong> {sourceFileName}</p>";
            if (!string.IsNullOrEmpty(targetFileName))
                html += $"<p><strong>Целевой файл:</strong> {targetFileName}</p>";
            html += "</div>";

            // Статистика
            html += "<div class='stats'>";
            html += $"<div class='stat-card'><div class='stat-number'>{addedListBox.Items.Count}</div><div class='stat-label'>Новых параметров</div></div>";
            html += $"<div class='stat-card'><div class='stat-number'>{missingListBox.Items.Count}</div><div class='stat-label'>Отсутствующих</div></div>";
            html += $"<div class='stat-card'><div class='stat-number'>{differentListBox.Items.Count / 4}</div><div class='stat-label'>Измененных</div></div>";
            html += $"<div class='stat-card'><div class='stat-number'>{sourceConfig.Count + targetConfig.Count - addedListBox.Items.Count}</div><div class='stat-label'>Всего параметров</div></div>";
            html += "</div>";

            // Новые параметры
            if (addedListBox.Items.Count > 0)
            {
                html += "<div class='section'>";
                html += "<div class='section-title'>➕ Новые параметры</div>";
                html += "<div class='added'>";
                foreach (var item in addedListBox.Items)
                {
                    html += $"<div class='config-line'>{HtmlEncode(item.ToString())}</div>";
                }
                html += "</div></div>";
            }

            // Отсутствующие параметры
            if (missingListBox.Items.Count > 0)
            {
                html += "<div class='section'>";
                html += "<div class='section-title'>❌ Отсутствующие параметры</div>";
                html += "<div class='missing'>";
                foreach (var item in missingListBox.Items)
                {
                    html += $"<div class='config-line'>{HtmlEncode(item.ToString())}</div>";
                }
                html += "</div></div>";
            }

            // Измененные параметры
            if (differentListBox.Items.Count > 0)
            {
                html += "<div class='section'>";
                html += "<div class='section-title'>🔄 Измененные параметры</div>";
                html += "<div class='different'>";

                string currentParam = "";
                string oldValue = "";

                foreach (var item in differentListBox.Items)
                {
                    var itemStr = item.ToString();
                    if (!itemStr.StartsWith("  ") && !string.IsNullOrEmpty(itemStr))
                    {
                        if (!string.IsNullOrEmpty(currentParam))
                        {
                            html += "</div>";
                        }
                        currentParam = itemStr.TrimEnd(':');
                        html += $"<div class='diff-item'><strong>{HtmlEncode(currentParam)}:</strong><br>";
                    }
                    else if (itemStr.StartsWith("  Было:"))
                    {
                        oldValue = itemStr.Substring(7).Trim();
                        html += $"<span class='old-value'>Было: {HtmlEncode(oldValue)}</span><br>";
                    }
                    else if (itemStr.StartsWith("  Стало:"))
                    {
                        var newValue = itemStr.Substring(8).Trim();
                        html += $"<span class='new-value'>Стало: {HtmlEncode(newValue)}</span>";    
                    }
                }

                if (!string.IsNullOrEmpty(currentParam))
                {
                    html += "</div>";
                }

                html += "</div></div>";
            }

            html += @"
        <div style='margin-top: 30px; text-align: center; color: #666; font-size: 0.9em;'>
            <p>Отчет сгенерирован с помощью Config Merger</p>
            <p>by ANHELL</p>
        </div>
    </div>
</body>
</html>";
            return html;
        }

        // Генерация текстового отчета
        private string GenerateTextReport()
        {
            var report = "=== ОТЧЕТ СРАВНЕНИЯ КОНФИГ-ФАЙЛОВ ===\n";
            report += $"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm:ss}\n\n";

            // Информация о файлах
            if (!string.IsNullOrEmpty(sourceFileName))
                report += $"Исходный файл: {sourceFileName}\n";
            if (!string.IsNullOrEmpty(targetFileName))
                report += $"Целевой файл: {targetFileName}\n\n";

            report += "СТАТИСТИКА:\n";
            report += $"- Новых параметров: {addedListBox.Items.Count}\n";
            report += $"- Отсутствующих параметров: {missingListBox.Items.Count}\n";
            report += $"- Измененных параметров: {differentListBox.Items.Count / 4}\n";
            report += $"- Всего параметров: {sourceConfig.Count + targetConfig.Count - addedListBox.Items.Count}\n\n";

            if (addedListBox.Items.Count > 0)
            {
                report += "НОВЫЕ ПАРАМЕТРЫ:\n";
                foreach (var item in addedListBox.Items)
                {
                    report += $"+ {item}\n";
                }
                report += "\n";
            }

            if (missingListBox.Items.Count > 0)
            {
                report += "ОТСУТСТВУЮЩИЕ ПАРАМЕТРЫ:\n";
                foreach (var item in missingListBox.Items)
                {
                    report += $"- {item}\n";
                }
                report += "\n";
            }

            if (differentListBox.Items.Count > 0)
            {
                report += "ИЗМЕНЕННЫЕ ПАРАМЕТРЫ:\n";
                foreach (var item in differentListBox.Items)
                {
                    report += $"  {item}\n";
                }
                report += "\n";
            }

            return report;
        }

        // Генерация CSV отчета
        private string GenerateCsvReport()
        {
            var csv = "Тип изменения,Параметр,Старое значение,Новое значение,Файл\n";

            // Новые параметры
            foreach (var item in addedListBox.Items)
            {
                var parts = item.ToString().Split('=');
                if (parts.Length >= 2)
                {
                    var param = parts[0].Trim();
                    var value = string.Join("=", parts.Skip(1)).Trim();
                    csv += $"\"Новый\",\"{param}\",\"\",\"{value}\",\"{sourceFileName}\"\n";
                }
            }

            // Отсутствующие параметры
            foreach (var item in missingListBox.Items)
            {
                var parts = item.ToString().Split('=');
                if (parts.Length >= 2)
                {
                    var param = parts[0].Trim();
                    var value = string.Join("=", parts.Skip(1)).Trim();
                    csv += $"\"Отсутствующий\",\"{param}\",\"{value}\",\"\",\"{targetFileName}\"\n";
                }
            }

            // Измененные параметры
            string currentParam = "";
            string oldValue = "";

            foreach (var item in differentListBox.Items)
            {
                var itemStr = item.ToString();
                if (!itemStr.StartsWith("  ") && !string.IsNullOrEmpty(itemStr))
                {
                    currentParam = itemStr.TrimEnd(':');
                }
                else if (itemStr.StartsWith("  Было:"))
                {
                    oldValue = itemStr.Substring(7).Trim();
                }
                else if (itemStr.StartsWith("  Стало:"))
                {
                    var newValue = itemStr.Substring(8).Trim();
                    csv += $"\"Измененный\",\"{currentParam}\",\"{oldValue}\",\"{newValue}\",\"Оба файла\"\n";
                }
            }

            return csv;
        }
    }

    // Helper classes
    public class ConfigParameter
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class ConfigDifference
    {
        public string Key { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }

    public class ComparisonResult
    {
        public List<ConfigParameter> Added { get; set; }
        public List<ConfigParameter> Missing { get; set; }
        public List<ConfigDifference> Different { get; set; }
        public Dictionary<string, string> Merged { get; set; }
    }

    // Класс настроек
    public class AppSettings
    {
        public bool DarkTheme { get; set; }
        public bool AutoBackup { get; set; }
        public string BackupPath { get; set; }
        public bool ShowLineNumbers { get; set; }
    }
}
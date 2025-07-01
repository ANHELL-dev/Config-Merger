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
        // Основные данные
        private Dictionary<string, string> sourceConfig = new Dictionary<string, string>();
        private Dictionary<string, string> targetConfig = new Dictionary<string, string>();
        private string sourceContent = "";
        private string targetContent = "";
        private string sourceFileName = "";
        private string targetFileName = "";

        public MainForm()
        {
            InitializeComponent();

            ThemeManager.LoadThemeSettings();
            SetupInitialState();
            SetupDragDrop();
            AddSaveButtons();

            ThemeManager.ThemeChanged += OnThemeChanged;
            CreateThemeToggleButton();
            ApplyCurrentTheme();
        }

        #region Инициализация и настройка UI

        private void SetupInitialState()
        {
            ApplyCurrentTheme();
            CheckReadyToCompare();
        }

        private void CreateThemeToggleButton()
        {
            var themeToggleBtn = new Button
            {
                Name = "themeToggleBtn",
                Text = ThemeManager.CurrentTheme == AppTheme.Light ? "🌙 Темная тема" : "☀️ Светлая тема",
                Size = new Size(130, 35),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                UseVisualStyleBackColor = false,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Location = new Point(headerPanel.Width - 150, 22),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            themeToggleBtn.FlatAppearance.BorderSize = 0;

            themeToggleBtn.Click += (s, e) =>
            {
                ThemeManager.ToggleTheme();
                ThemeManager.SaveThemeSettings();
                themeToggleBtn.Text = ThemeManager.CurrentTheme == AppTheme.Light ? "🌙 Темная тема" : "☀️ Светлая тема";
                ThemeManager.StyleButton(themeToggleBtn, ThemeManager.ButtonType.Secondary);
            };

            headerPanel.Controls.Add(themeToggleBtn);
            ThemeManager.StyleButton(themeToggleBtn, ThemeManager.ButtonType.Secondary);
            headerPanel.Invalidate();
        }

        private void SetupDragDrop()
        {
            sourceGroupBox.AllowDrop = true;
            targetGroupBox.AllowDrop = true;

            // События для исходного файла
            sourceGroupBox.DragEnter += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    e.Effect = DragDropEffects.Copy;
                    sourceGroupBox.BackColor = ThemeManager.GetSuccessBackground();
                }
            };

            sourceGroupBox.DragLeave += (s, e) =>
            {
                sourceGroupBox.BackColor = sourceConfig.Count > 0
                    ? ThemeManager.GetSuccessBackground()
                    : ThemeManager.GetBackground();
            };

            sourceGroupBox.DragDrop += (s, e) =>
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                    LoadConfigFile(files[0], "source");
            };

            // События для целевого файла
            targetGroupBox.DragEnter += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    e.Effect = DragDropEffects.Copy;
                    targetGroupBox.BackColor = ThemeManager.GetSuccessBackground();
                }
            };

            targetGroupBox.DragLeave += (s, e) =>
            {
                targetGroupBox.BackColor = targetConfig.Count > 0
                    ? ThemeManager.GetSuccessBackground()
                    : ThemeManager.GetBackground();
            };

            targetGroupBox.DragDrop += (s, e) =>
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                    LoadConfigFile(files[0], "target");
            };
        }

        #endregion

        #region Обработка темы

        private void OnThemeChanged(AppTheme newTheme)
        {
            ApplyCurrentTheme();

            var themeToggleBtn = headerPanel.Controls.OfType<Button>()
                .FirstOrDefault(b => b.Name == "themeToggleBtn");

            if (themeToggleBtn != null)
            {
                themeToggleBtn.Text = newTheme == AppTheme.Light ? "🌙 Темная тема" : "☀️ Светлая тема";
                ThemeManager.StyleButton(themeToggleBtn, ThemeManager.ButtonType.Secondary);
            }

            resultsTabControl.Invalidate();
        }

        private void ApplyCurrentTheme()
        {
            ThemeManager.ApplyTheme(this);
            StyleAllButtons();
            UpdateElementsForTheme();
        }

        private void StyleAllButtons()
        {
            ThemeManager.StyleButton(sourceSelectBtn, ThemeManager.ButtonType.Primary);
            ThemeManager.StyleButton(targetSelectBtn, ThemeManager.ButtonType.Primary);
            ThemeManager.StyleButton(compareBtn, ThemeManager.ButtonType.Primary);
            ThemeManager.StyleButton(resetBtn, ThemeManager.ButtonType.Secondary);
            ThemeManager.StyleButton(copyBtn, ThemeManager.ButtonType.Success);

            // Кнопки в результатах
            StyleResultsButtons();
            CheckReadyToCompare();
        }

        private void StyleResultsButtons()
        {
            foreach (Control control in resultsTabControl.Controls)
            {
                if (control is TabPage tabPage)
                {
                    StyleButtonsInContainer(tabPage);
                }
            }
        }

        private void StyleButtonsInContainer(Control container)
        {
            foreach (Control control in container.Controls)
            {
                if (control is Button button)
                {
                    var buttonType = GetButtonTypeFromText(button.Text);
                    ThemeManager.StyleButton(button, buttonType);
                }
                else if (control.HasChildren)
                {
                    StyleButtonsInContainer(control);
                }
            }
        }

        private ThemeManager.ButtonType GetButtonTypeFromText(string buttonText)
        {
            if (buttonText.Contains("Скопировать")) return ThemeManager.ButtonType.Success;
            if (buttonText.Contains("Быстрое")) return ThemeManager.ButtonType.Warning;
            if (buttonText.Contains("Экспорт")) return ThemeManager.ButtonType.Danger;
            if (buttonText.Contains("целевой")) return ThemeManager.ButtonType.Secondary;
            return ThemeManager.ButtonType.Primary;
        }

        private void UpdateElementsForTheme()
        {
            UpdateStatusLabels();
            UpdateTabControlBackground();
            UpdateHeaderElements();
            UpdateSummaryLabel();
        }

        private void UpdateStatusLabels()
        {
            if (sourceConfig.Count > 0)
            {
                sourceStatusLabel.ForeColor = ThemeManager.GetSuccess();
                sourceGroupBox.BackColor = ThemeManager.GetSuccessBackground();
            }
            else
            {
                sourceStatusLabel.ForeColor = ThemeManager.GetTextSecondary();
                sourceGroupBox.BackColor = ThemeManager.GetBackground();
            }

            if (targetConfig.Count > 0)
            {
                targetStatusLabel.ForeColor = ThemeManager.GetSuccess();
                targetGroupBox.BackColor = ThemeManager.GetSuccessBackground();
            }
            else
            {
                targetStatusLabel.ForeColor = ThemeManager.GetTextSecondary();
                targetGroupBox.BackColor = ThemeManager.GetBackground();
            }
        }

        private void UpdateTabControlBackground()
        {
            resultsTabControl.BackColor = ThemeManager.GetBackground();
            resultsTabControl.Invalidate();

            foreach (TabPage tab in resultsTabControl.TabPages)
            {
                tab.BackColor = ThemeManager.GetBackground();
                tab.ForeColor = ThemeManager.GetText();
            }
        }

        private void UpdateHeaderElements()
        {
            headerPanel.BackColor = ThemeManager.GetHeader();
            titleLabel.ForeColor = ThemeManager.GetHeaderText();
            titleLabel.BackColor = Color.Transparent;
            subtitleLabel.ForeColor = ThemeManager.GetHeaderText();
            subtitleLabel.BackColor = Color.Transparent;
        }

        private void UpdateSummaryLabel()
        {
            summaryLabel.BackColor = ThemeManager.CurrentTheme == AppTheme.Light
                ? Color.FromArgb(102, 126, 234)
                : Color.FromArgb(52, 76, 134);
            summaryLabel.ForeColor = Color.White;
            progressBar.BackColor = ThemeManager.GetInputBackground();
        }

        #endregion

        #region Загрузка и обработка файлов

        private void sourceSelectBtn_Click(object sender, EventArgs e) => SelectFile("source");
        private void targetSelectBtn_Click(object sender, EventArgs e) => SelectFile("target");

        private void SelectFile(string type)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = ConfigParserFactory.GetSupportedFormats();
                dialog.Title = type == "source" ? "Выберите исходный конфиг" : "Выберите целевой конфиг";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadConfigFile(dialog.FileName, type);
                }
            }
        }

        private void LoadConfigFile(string filePath, string type)
        {
            try
            {
                if (!ConfigParserFactory.IsSupported(filePath))
                {
                    MessageBox.Show("Формат файла не поддерживается!\nПоддерживаемые форматы: Python, YAML, JSON, XML",
                                   "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var content = File.ReadAllText(filePath);

                // Очистка дубликатов для Python файлов
                if (Path.GetExtension(filePath).ToLowerInvariant() == ".py")
                {
                    content = CleanDuplicates(content);
                }

                var parser = ConfigParserFactory.GetParser(filePath);
                var config = parser.ParseConfig(content);

                UpdateConfigData(type, config, content, Path.GetFileName(filePath));
                CheckReadyToCompare();
            }
            catch (Exception ex)
            {
                HandleFileLoadError(type, ex.Message);
            }
        }

        private void UpdateConfigData(string type, Dictionary<string, string> config, string content, string fileName)
        {
            if (type == "source")
            {
                sourceConfig = config;
                sourceContent = content;
                sourceFileName = fileName;
                sourceStatusLabel.Text = $"✅ Загружено: {fileName} ({config.Count} параметров)";
                sourceStatusLabel.ForeColor = ThemeManager.GetSuccess();
                sourceGroupBox.BackColor = ThemeManager.GetSuccessBackground();
            }
            else
            {
                targetConfig = config;
                targetContent = content;
                targetFileName = fileName;
                targetStatusLabel.Text = $"✅ Загружено: {fileName} ({config.Count} параметров)";
                targetStatusLabel.ForeColor = ThemeManager.GetSuccess();
                targetGroupBox.BackColor = ThemeManager.GetSuccessBackground();
            }
        }

        private void HandleFileLoadError(string type, string errorMessage)
        {
            var statusLabel = type == "source" ? sourceStatusLabel : targetStatusLabel;
            var groupBox = type == "source" ? sourceGroupBox : targetGroupBox;

            statusLabel.Text = $"❌ Ошибка: {errorMessage}";
            statusLabel.ForeColor = ThemeManager.GetDanger();
            groupBox.BackColor = ThemeManager.GetDangerBackground();

            MessageBox.Show($"Ошибка загрузки файла:\n{errorMessage}", "Ошибка",
                           MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private string CleanDuplicates(string content)
        {
            var lines = content.Split('\n');
            var result = new List<string>();
            var keyToLastLine = new Dictionary<string, int>();

            // Находим последние позиции ключей
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

            // Добавляем только последние вхождения
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

                    if (keyToLastLine[key] == i)
                    {
                        result.Add(line);

                        // Обработка многострочных значений
                        if (value.Contains("{") && !value.Contains("}"))
                        {
                            skipNextLines = ProcessMultilineBlock(lines, i, result);
                        }
                    }
                    else
                    {
                        // Пропускаем дубликат
                        if (value.Contains("{") && !value.Contains("}"))
                        {
                            skipNextLines = SkipMultilineBlock(lines, i);
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

        private int ProcessMultilineBlock(string[] lines, int startIndex, List<string> result)
        {
            var value = lines[startIndex].Split('=')[1].Trim();
            var braceCount = value.Count(c => c == '{');
            var skipCount = 0;
            var j = startIndex + 1;

            while (j < lines.Length && braceCount > 0)
            {
                var nextLine = lines[j].Trim();
                result.Add(lines[j]);
                braceCount += nextLine.Count(c => c == '{');
                braceCount -= nextLine.Count(c => c == '}');
                j++;
                skipCount++;
            }

            return skipCount;
        }

        private int SkipMultilineBlock(string[] lines, int startIndex)
        {
            var value = lines[startIndex].Split('=')[1].Trim();
            var braceCount = value.Count(c => c == '{');
            var skipCount = 0;
            var j = startIndex + 1;

            while (j < lines.Length && braceCount > 0)
            {
                var nextLine = lines[j].Trim();
                braceCount += nextLine.Count(c => c == '{');
                braceCount -= nextLine.Count(c => c == '}');
                j++;
                skipCount++;
            }

            return skipCount;
        }

        #endregion

        #region Сравнение и результаты

        private void CheckReadyToCompare()
        {
            compareBtn.Enabled = sourceConfig.Count > 0 && targetConfig.Count > 0;

            if (compareBtn.Enabled)
            {
                ThemeManager.StyleButton(compareBtn, ThemeManager.ButtonType.Primary);
            }
            else
            {
                compareBtn.BackColor = Color.Gray;
                compareBtn.ForeColor = Color.White;
            }
        }

        private async void compareBtn_Click(object sender, EventArgs e)
        {
            progressBar.Visible = true;
            compareBtn.Enabled = false;

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

            // Новые и измененные параметры
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

            // Отсутствующие параметры
            foreach (var kvp in targetConfig.Where(kvp => !sourceConfig.ContainsKey(kvp.Key)))
            {
                missing.Add(new ConfigParameter { Key = kvp.Key, Value = kvp.Value });
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
            UpdateSummary(comparison);
            UpdateMergedConfig(comparison.Merged);
            UpdateParameterLists(comparison);

            resultsTabControl.SelectedIndex = 0;
        }

        private void UpdateSummary(ComparisonResult comparison)
        {
            summaryLabel.Text = $"📊 Результаты сравнения\n\n" +
                               $"Новых параметров: {comparison.Added.Count}\n" +
                               $"Отсутствующих: {comparison.Missing.Count}\n" +
                               $"Измененных: {comparison.Different.Count}\n" +
                               $"Всего параметров: {comparison.Merged.Count}";
        }

        private void UpdateMergedConfig(Dictionary<string, string> merged)
        {
            mergedConfigTextBox.Text = GenerateMergedConfig(merged);
        }

        private void UpdateParameterLists(ComparisonResult comparison)
        {
            addedListBox.Items.Clear();
            foreach (var param in comparison.Added)
            {
                addedListBox.Items.Add($"{param.Key} = {param.Value}");
            }

            missingListBox.Items.Clear();
            foreach (var param in comparison.Missing)
            {
                missingListBox.Items.Add($"{param.Key} = {param.Value}");
            }

            differentListBox.Items.Clear();
            foreach (var diff in comparison.Different)
            {
                differentListBox.Items.Add($"{diff.Key}:");
                differentListBox.Items.Add($"  Было: {diff.OldValue}");
                differentListBox.Items.Add($"  Стало: {diff.NewValue}");
                differentListBox.Items.Add("");
            }
        }

        private string GenerateMergedConfig(Dictionary<string, string> merged)
        {
            try
            {
                if (!string.IsNullOrEmpty(targetFileName))
                {
                    var parser = ConfigParserFactory.GetParser(targetFileName);
                    return parser.GenerateMergedConfig(merged, targetContent);
                }

                var pythonParser = new PythonConfigParser();
                return pythonParser.GenerateMergedConfig(merged, targetContent);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации конфига: {ex.Message}", "Ошибка",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }
        }

        #endregion

        #region Действия с результатами

        private void copyBtn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(mergedConfigTextBox.Text))
            {
                Clipboard.SetText(mergedConfigTextBox.Text);
                ShowCopyFeedback();
            }
        }

        private void ShowCopyFeedback()
        {
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

        private void resetBtn_Click(object sender, EventArgs e)
        {
            ClearAllData();
            UpdateUIAfterReset();
        }

        private void ClearAllData()
        {
            sourceConfig.Clear();
            targetConfig.Clear();
            sourceContent = "";
            targetContent = "";
            sourceFileName = "";
            targetFileName = "";
        }

        private void UpdateUIAfterReset()
        {
            sourceStatusLabel.Text = "Файл не выбран";
            sourceStatusLabel.ForeColor = ThemeManager.GetTextSecondary();
            sourceGroupBox.BackColor = ThemeManager.GetBackground();

            targetStatusLabel.Text = "Файл не выбран";
            targetStatusLabel.ForeColor = ThemeManager.GetTextSecondary();
            targetGroupBox.BackColor = ThemeManager.GetBackground();

            compareBtn.Enabled = false;
            compareBtn.BackColor = Color.Gray;

            mergedConfigTextBox.Text = "";
            addedListBox.Items.Clear();
            missingListBox.Items.Clear();
            differentListBox.Items.Clear();
            summaryLabel.Text = "";
        }

        #endregion

        #region Сохранение и экспорт

        private void AddSaveButtons()
        {
            foreach (TabPage tab in resultsTabControl.TabPages)
            {
                if (tab.Text.Contains("Объединенный конфиг"))
                {
                    var mergedPanel = tab.Controls[0] as Panel;
                    if (mergedPanel != null)
                    {
                        CreateSaveButtonsPanel(mergedPanel);
                        break;
                    }
                }
            }
        }

        private void CreateSaveButtonsPanel(Panel mergedPanel)
        {
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            var flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(10, 10, 10, 10)
            };

            // Перемещаем существующую кнопку копирования
            copyBtn.Size = new Size(130, 30);
            copyBtn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            copyBtn.Margin = new Padding(0, 0, 10, 0);
            copyBtn.TextAlign = ContentAlignment.MiddleCenter;

            var saveButtons = CreateSaveButtons();

            flowPanel.Controls.Add(copyBtn);
            foreach (var button in saveButtons)
            {
                flowPanel.Controls.Add(button);
            }

            buttonPanel.Controls.Add(flowPanel);
            mergedPanel.Controls.Add(buttonPanel);
            buttonPanel.BringToFront();

            // Обновляем позицию текстового поля
            if (mergedConfigTextBox != null)
            {
                mergedConfigTextBox.Location = new Point(10, 70);
                mergedConfigTextBox.Size = new Size(mergedPanel.Width - 20, mergedPanel.Height - 80);
                mergedConfigTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            }
        }

        private List<Button> CreateSaveButtons()
        {
            var buttons = new List<Button>();

            var saveAsBtn = CreateSaveButton("💾 Сохранить как...", new Size(160, 30), Color.FromArgb(0, 123, 255), saveResultBtn_Click);
            var saveAsTargetBtn = CreateSaveButton("📁 Сохранить как целевой", new Size(200, 30), Color.FromArgb(108, 117, 125), saveAsTargetBtn_Click);
            var quickSaveBtn = CreateSaveButton("⚡ Быстрое сохранение", new Size(180, 30), Color.FromArgb(255, 193, 7), quickSaveBtn_Click);
            var exportReportBtn = CreateSaveButton("📊 Экспорт отчета", new Size(150, 30), Color.FromArgb(220, 53, 69), exportReportBtn_Click);

            quickSaveBtn.ForeColor = Color.Black; // Особый цвет для желтой кнопки

            buttons.AddRange(new[] { saveAsBtn, saveAsTargetBtn, quickSaveBtn, exportReportBtn });
            return buttons;
        }

        private Button CreateSaveButton(string text, Size size, Color backColor, EventHandler clickHandler)
        {
            var button = new Button
            {
                Text = text,
                Size = size,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                Margin = new Padding(0, 0, 10, 0),
                TextAlign = ContentAlignment.MiddleCenter
            };

            button.FlatAppearance.BorderSize = 0;
            button.Click += clickHandler;

            return button;
        }

        private void saveResultBtn_Click(object sender, EventArgs e)
        {
            if (!ValidateConfigForSaving()) return;

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Python files (*.py)|*.py|Text files (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.DefaultExt = "py";
                dialog.FileName = "merged_config.py";
                dialog.Title = "Сохранить объединенный конфиг";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    SaveToFile(dialog.FileName);
                }
            }
        }

        private void saveAsTargetBtn_Click(object sender, EventArgs e)
        {
            if (!ValidateConfigForSaving() || !ValidateTargetFile()) return;

            using (var dialog = new SaveFileDialog())
            {
                SetupSaveDialogForTarget(dialog);

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    SaveToFileWithPrompt(dialog.FileName);
                }
            }
        }

        private void quickSaveBtn_Click(object sender, EventArgs e)
        {
            if (!ValidateConfigForSaving() || !ValidateTargetFile()) return;

            if (ConfirmQuickSave())
            {
                PerformQuickSave();
            }
        }

        private bool ValidateConfigForSaving()
        {
            if (string.IsNullOrEmpty(mergedConfigTextBox.Text))
            {
                MessageBox.Show("Нет данных для сохранения!\nСначала выполните сравнение файлов.", "Предупреждение",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private bool ValidateTargetFile()
        {
            if (string.IsNullOrEmpty(targetFileName))
            {
                MessageBox.Show("Целевой файл не загружен!\nСначала загрузите целевой конфиг.", "Предупреждение",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void SetupSaveDialogForTarget(SaveFileDialog dialog)
        {
            var extension = Path.GetExtension(targetFileName).ToLowerInvariant();
            dialog.Filter = extension switch
            {
                ".py" => "Python files (*.py)|*.py|All files (*.*)|*.*",
                ".yaml" or ".yml" => "YAML files (*.yaml)|*.yaml|YAML files (*.yml)|*.yml|All files (*.*)|*.*",
                ".json" => "JSON files (*.json)|*.json|All files (*.*)|*.*",
                ".xml" => "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                _ => "All files (*.*)|*.*"
            };

            dialog.DefaultExt = extension;
            dialog.FileName = targetFileName;
            dialog.Title = $"Сохранить как '{targetFileName}'";
        }

        private bool ConfirmQuickSave()
        {
            var result = MessageBox.Show(
                $"Вы уверены, что хотите заменить файл '{targetFileName}'?\n\n" +
                "⚠️ ВНИМАНИЕ: Оригинальный файл будет перезаписан!\n" +
                "Рекомендуется создать резервную копию.",
                "Подтверждение замены",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            return result == DialogResult.Yes;
        }

        private void PerformQuickSave()
        {
            var backupPath = CreateBackup();

            try
            {
                var targetDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var targetPath = Path.Combine(targetDir, targetFileName);

                File.WriteAllText(targetPath, mergedConfigTextBox.Text);

                ShowQuickSaveResult(targetPath, backupPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения файла:\n{ex.Message}", "Ошибка",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowQuickSaveResult(string targetPath, string backupPath)
        {
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

        private void SaveToFile(string fileName)
        {
            try
            {
                File.WriteAllText(fileName, mergedConfigTextBox.Text);
                MessageBox.Show($"Файл успешно сохранен:\n{fileName}", "Успех",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения файла:\n{ex.Message}", "Ошибка",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveToFileWithPrompt(string fileName)
        {
            try
            {
                File.WriteAllText(fileName, mergedConfigTextBox.Text);

                var result = MessageBox.Show(
                    $"Файл успешно сохранен:\n{fileName}\n\n" +
                    "Хотите открыть папку с файлом?",
                    "Успех",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{fileName}\"");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения файла:\n{ex.Message}", "Ошибка",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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

        #endregion

        #region Экспорт отчетов

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
                ExportReport(dialog.FileName);
            }
        }

        private void ExportReport(string fileName)
        {
            try
            {
                var ext = Path.GetExtension(fileName).ToLower();
                var content = ext switch
                {
                    ".html" => GenerateHtmlReport(),
                    ".txt" => GenerateTextReport(),
                    ".csv" => GenerateCsvReport(),
                    _ => GenerateTextReport()
                };

                File.WriteAllText(fileName, content);
                MessageBox.Show($"Отчет сохранен: {fileName}", "Успех",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения отчета:\n{ex.Message}", "Ошибка",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GenerateHtmlReport()
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Отчет сравнения конфигов</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; background: #f8f9fa; }}
        .container {{ max-width: 1200px; margin: 0 auto; background: white; padding: 20px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%); color: white; padding: 30px; border-radius: 10px; text-align: center; margin-bottom: 30px; }}
        .header h1 {{ margin: 0; font-size: 2em; }}
        .stats {{ display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 20px; margin-bottom: 30px; }}
        .stat-card {{ background: #f8f9fa; padding: 20px; border-radius: 10px; text-align: center; border-left: 5px solid #4facfe; }}
        .stat-number {{ font-size: 2em; font-weight: bold; color: #4facfe; }}
        .stat-label {{ color: #666; margin-top: 5px; }}
        .section {{ margin: 30px 0; }}
        .section-title {{ font-size: 1.5em; font-weight: bold; margin-bottom: 15px; padding-bottom: 10px; border-bottom: 2px solid #eee; }}
        .added {{ background: #d4edda; border-left: 5px solid #28a745; padding: 15px; border-radius: 5px; }}
        .missing {{ background: #f8d7da; border-left: 5px solid #dc3545; padding: 15px; border-radius: 5px; }}
        .different {{ background: #fff3cd; border-left: 5px solid #ffc107; padding: 15px; border-radius: 5px; }}
        .config-line {{ font-family: 'Courier New', monospace; padding: 8px; margin: 5px 0; background: rgba(0,0,0,0.05); border-radius: 3px; word-break: break-all; }}
        .file-info {{ background: #e9ecef; padding: 15px; border-radius: 5px; margin-bottom: 20px; }}
        .diff-item {{ margin-bottom: 15px; }}
        .old-value {{ color: #dc3545; }}
        .new-value {{ color: #28a745; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔧 Отчет сравнения конфиг-файлов</h1>
            <p>Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm:ss}</p>
        </div>";

            html += BuildFileInfoSection();
            html += BuildStatsSection();
            html += BuildParameterSections();

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

        private string BuildFileInfoSection()
        {
            var html = "<div class='file-info'><h3>📁 Информация о файлах</h3>";

            if (!string.IsNullOrEmpty(sourceFileName))
                html += $"<p><strong>Исходный файл:</strong> {sourceFileName}</p>";
            if (!string.IsNullOrEmpty(targetFileName))
                html += $"<p><strong>Целевой файл:</strong> {targetFileName}</p>";

            html += "</div>";
            return html;
        }

        private string BuildStatsSection()
        {
            var html = "<div class='stats'>";
            html += $"<div class='stat-card'><div class='stat-number'>{addedListBox.Items.Count}</div><div class='stat-label'>Новых параметров</div></div>";
            html += $"<div class='stat-card'><div class='stat-number'>{missingListBox.Items.Count}</div><div class='stat-label'>Отсутствующих</div></div>";
            html += $"<div class='stat-card'><div class='stat-number'>{differentListBox.Items.Count / 4}</div><div class='stat-label'>Измененных</div></div>";
            html += $"<div class='stat-card'><div class='stat-number'>{sourceConfig.Count + targetConfig.Count - addedListBox.Items.Count}</div><div class='stat-label'>Всего параметров</div></div>";
            html += "</div>";
            return html;
        }

        private string BuildParameterSections()
        {
            var html = "";

            // Новые параметры
            if (addedListBox.Items.Count > 0)
            {
                html += "<div class='section'><div class='section-title'>➕ Новые параметры</div><div class='added'>";
                foreach (var item in addedListBox.Items)
                {
                    html += $"<div class='config-line'>{HtmlEncode(item.ToString())}</div>";
                }
                html += "</div></div>";
            }

            // Отсутствующие параметры
            if (missingListBox.Items.Count > 0)
            {
                html += "<div class='section'><div class='section-title'>❌ Отсутствующие параметры</div><div class='missing'>";
                foreach (var item in missingListBox.Items)
                {
                    html += $"<div class='config-line'>{HtmlEncode(item.ToString())}</div>";
                }
                html += "</div></div>";
            }

            // Измененные параметры
            if (differentListBox.Items.Count > 0)
            {
                html += "<div class='section'><div class='section-title'>🔄 Измененные параметры</div><div class='different'>";
                html += BuildDifferentParametersHtml();
                html += "</div></div>";
            }

            return html;
        }

        private string BuildDifferentParametersHtml()
        {
            var html = "";
            string currentParam = "";

            foreach (var item in differentListBox.Items)
            {
                var itemStr = item.ToString();
                if (!itemStr.StartsWith("  ") && !string.IsNullOrEmpty(itemStr))
                {
                    if (!string.IsNullOrEmpty(currentParam))
                        html += "</div>";

                    currentParam = itemStr.TrimEnd(':');
                    html += $"<div class='diff-item'><strong>{HtmlEncode(currentParam)}:</strong><br>";
                }
                else if (itemStr.StartsWith("  Было:"))
                {
                    var oldValue = itemStr.Substring(7).Trim();
                    html += $"<span class='old-value'>Было: {HtmlEncode(oldValue)}</span><br>";
                }
                else if (itemStr.StartsWith("  Стало:"))
                {
                    var newValue = itemStr.Substring(8).Trim();
                    html += $"<span class='new-value'>Стало: {HtmlEncode(newValue)}</span>";
                }
            }

            if (!string.IsNullOrEmpty(currentParam))
                html += "</div>";

            return html;
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

        private string GenerateTextReport()
        {
            var report = "=== ОТЧЕТ СРАВНЕНИЯ КОНФИГ-ФАЙЛОВ ===\n";
            report += $"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm:ss}\n\n";

            if (!string.IsNullOrEmpty(sourceFileName))
                report += $"Исходный файл: {sourceFileName}\n";
            if (!string.IsNullOrEmpty(targetFileName))
                report += $"Целевой файл: {targetFileName}\n\n";

            report += "СТАТИСТИКА:\n";
            report += $"- Новых параметров: {addedListBox.Items.Count}\n";
            report += $"- Отсутствующих параметров: {missingListBox.Items.Count}\n";
            report += $"- Измененных параметров: {differentListBox.Items.Count / 4}\n";
            report += $"- Всего параметров: {sourceConfig.Count + targetConfig.Count - addedListBox.Items.Count}\n\n";

            report += BuildTextParameterSections();
            return report;
        }

        private string BuildTextParameterSections()
        {
            var report = "";

            if (addedListBox.Items.Count > 0)
            {
                report += "НОВЫЕ ПАРАМЕТРЫ:\n";
                foreach (var item in addedListBox.Items)
                    report += $"+ {item}\n";
                report += "\n";
            }

            if (missingListBox.Items.Count > 0)
            {
                report += "ОТСУТСТВУЮЩИЕ ПАРАМЕТРЫ:\n";
                foreach (var item in missingListBox.Items)
                    report += $"- {item}\n";
                report += "\n";
            }

            if (differentListBox.Items.Count > 0)
            {
                report += "ИЗМЕНЕННЫЕ ПАРАМЕТРЫ:\n";
                foreach (var item in differentListBox.Items)
                    report += $"  {item}\n";
                report += "\n";
            }

            return report;
        }

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

            csv += BuildCsvDifferentParameters();
            return csv;
        }

        private string BuildCsvDifferentParameters()
        {
            var csv = "";
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

        #endregion

        #region Очистка ресурсов

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThemeManager.ThemeChanged -= OnThemeChanged;
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    #region Вспомогательные классы

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

    public class AppSettings
    {
        public bool DarkTheme { get; set; }
        public bool AutoBackup { get; set; }
        public string BackupPath { get; set; }
        public bool ShowLineNumbers { get; set; }
    }

    #endregion
}
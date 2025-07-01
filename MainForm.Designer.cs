using System;
using System.Drawing;
using System.Windows.Forms;

namespace ConfigMerger
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel headerPanel;
        private Label titleLabel;
        private Label subtitleLabel;
        private GroupBox sourceGroupBox;
        private GroupBox targetGroupBox;
        private Button sourceSelectBtn;
        private Button targetSelectBtn;
        private Label sourceStatusLabel;
        private Label targetStatusLabel;
        private Button compareBtn;
        private Button resetBtn;
        private TabControl resultsTabControl;
        private RichTextBox mergedConfigTextBox;
        private ListBox addedListBox;
        private ListBox missingListBox;
        private ListBox differentListBox;
        private Label summaryLabel;
        private Button copyBtn;
        private ProgressBar progressBar;


        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Настройки формы
            this.Text = "Config Merger - Сравнение конфиг-файлов";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.AutoScaleMode = AutoScaleMode.Font;

            CreateHeaderPanel();
            CreateUploadPanel();
            CreateActionPanel();
            CreateResultsSection();

            // Добавляем контролы на форму
            this.Controls.Add(resultsTabControl);
            this.Controls.Add(CreateActionPanel());
            this.Controls.Add(CreateUploadPanel());
            this.Controls.Add(headerPanel);

            this.ResumeLayout(false);
        }

        private void CreateHeaderPanel()
        {
            headerPanel = new Panel
            {
                Name = "headerPanel",
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(79, 172, 254)
            };

            titleLabel = new Label
            {
                Text = "🔧 Сравнение конфиг-файлов",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };

            subtitleLabel = new Label
            {
                Text = "Загрузите два конфиг-файла для сравнения и объединения настроек",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(20, 45),
                AutoSize = true
            };

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(subtitleLabel);
        }

        private Panel CreateUploadPanel()
        {
            var uploadPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 150,
                Padding = new Padding(20, 20, 20, 10)
            };

            CreateSourceGroupBox();
            CreateTargetGroupBox();

            uploadPanel.Controls.Add(sourceGroupBox);
            uploadPanel.Controls.Add(targetGroupBox);

            return uploadPanel;
        }

        private void CreateSourceGroupBox()
        {
            sourceGroupBox = new GroupBox
            {
                Text = "📄 Исходный конфиг (с данными)",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(350, 120),
                Location = new Point(20, 10),
                ForeColor = Color.FromArgb(50, 50, 50)
            };

            sourceSelectBtn = new Button
            {
                Text = "Выбрать файл",
                Size = new Size(120, 35),
                Location = new Point(20, 30),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                UseVisualStyleBackColor = false,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            sourceSelectBtn.Click += sourceSelectBtn_Click;

            sourceStatusLabel = new Label
            {
                Text = "Файл не выбран",
                Location = new Point(20, 75),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray
            };

            sourceGroupBox.Controls.Add(sourceSelectBtn);
            sourceGroupBox.Controls.Add(sourceStatusLabel);
        }

        private void CreateTargetGroupBox()
        {
            targetGroupBox = new GroupBox
            {
                Text = "📋 Целевой конфиг (пустой)",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(350, 120),
                Location = new Point(390, 10),
                ForeColor = Color.FromArgb(50, 50, 50)
            };

            targetSelectBtn = new Button
            {
                Text = "Выбрать файл",
                Size = new Size(120, 35),
                Location = new Point(20, 30),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                UseVisualStyleBackColor = false,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            targetSelectBtn.Click += targetSelectBtn_Click;

            targetStatusLabel = new Label
            {
                Text = "Файл не выбран",
                Location = new Point(20, 75),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray
            };

            targetGroupBox.Controls.Add(targetSelectBtn);
            targetGroupBox.Controls.Add(targetStatusLabel);
        }

        private Panel CreateActionPanel()
        {
            var actionPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(20, 10, 20, 10)
            };

            compareBtn = new Button
            {
                Text = "🔍 Сравнить файлы",
                Size = new Size(150, 30),
                Location = new Point(400, 10),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Enabled = false,
                UseVisualStyleBackColor = false,
                FlatStyle = FlatStyle.Standard,
                TextAlign = ContentAlignment.MiddleCenter
            };
            compareBtn.Click += compareBtn_Click;

            resetBtn = new Button
            {
                Text = "🔄 Сбросить",
                Size = new Size(150, 30),
                Location = new Point(570, 10),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                UseVisualStyleBackColor = false,
                FlatStyle = FlatStyle.Standard,
                TextAlign = ContentAlignment.MiddleCenter
            };
            resetBtn.Click += resetBtn_Click;

            progressBar = new ProgressBar
            {
                Location = new Point(20, 25),
                Size = new Size(300, 10),
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Visible = false
            };

            actionPanel.Controls.Add(compareBtn);
            actionPanel.Controls.Add(resetBtn);
            actionPanel.Controls.Add(progressBar);

            return actionPanel;
        }

        private void CreateResultsSection()
        {
            resultsTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9),
                Padding = new Point(20, 10)
            };

            CreateSummaryTab();
            CreateMergedConfigTab();
            CreateAddedTab();
            CreateMissingTab();
            CreateDifferentTab();
        }

        private void CreateSummaryTab()
        {
            var summaryTab = new TabPage("📊 Сводка");
            summaryLabel = new Label
            {
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 11),
                Padding = new Padding(20),
                Height = 100,
                BackColor = Color.FromArgb(102, 126, 234),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
            summaryTab.Controls.Add(summaryLabel);
            resultsTabControl.TabPages.Add(summaryTab);
        }

        private void CreateMergedConfigTab()
        {
            var mergedTab = new TabPage("📝 Объединенный конфиг");
            var mergedPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            copyBtn = new Button
            {
                Text = "📋 Скопировать",
                Size = new Size(120, 30),
                Location = new Point(10, 10),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            copyBtn.Click += copyBtn_Click;

            mergedConfigTextBox = new RichTextBox
            {
                Location = new Point(10, 50),
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(248, 249, 250),
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Both,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            mergedPanel.Controls.Add(mergedConfigTextBox);
            mergedTab.Controls.Add(mergedPanel);
            resultsTabControl.TabPages.Add(mergedTab);
        }

        private void CreateAddedTab()
        {
            var addedTab = new TabPage("➕ Новые параметры");
            addedListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(232, 245, 232),
                BorderStyle = BorderStyle.None
            };
            addedTab.Controls.Add(addedListBox);
            resultsTabControl.TabPages.Add(addedTab);
        }

        private void CreateMissingTab()
        {
            var missingTab = new TabPage("❌ Отсутствующие");
            missingListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(255, 234, 234),
                BorderStyle = BorderStyle.None
            };
            missingTab.Controls.Add(missingListBox);
            resultsTabControl.TabPages.Add(missingTab);
        }

        private void CreateDifferentTab()
        {
            var differentTab = new TabPage("🔄 Измененные");
            differentListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(255, 243, 205),
                BorderStyle = BorderStyle.None
            };
            differentTab.Controls.Add(differentListBox);
            resultsTabControl.TabPages.Add(differentTab);
        }
    }
}
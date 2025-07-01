using System;
using System.Drawing;
using System.Windows.Forms;

namespace ConfigMerger
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // UI Components
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

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form settings
            this.Text = "Config Merger - Сравнение конфиг-файлов";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.AutoScaleMode = AutoScaleMode.Font;

            // Header Panel
            this.headerPanel = new Panel();
            this.headerPanel.Dock = DockStyle.Top;
            this.headerPanel.Height = 80;
            this.headerPanel.BackColor = Color.FromArgb(79, 172, 254);

            this.titleLabel = new Label();
            this.titleLabel.Text = "🔧 Сравнение конфиг-файлов";
            this.titleLabel.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            this.titleLabel.ForeColor = Color.White;
            this.titleLabel.Location = new Point(20, 15);
            this.titleLabel.AutoSize = true;

            this.subtitleLabel = new Label();
            this.subtitleLabel.Text = "Загрузите два конфиг-файла для сравнения и объединения настроек";
            this.subtitleLabel.Font = new Font("Segoe UI", 10);
            this.subtitleLabel.ForeColor = Color.White;
            this.subtitleLabel.Location = new Point(20, 45);
            this.subtitleLabel.AutoSize = true;

            this.headerPanel.Controls.Add(this.titleLabel);
            this.headerPanel.Controls.Add(this.subtitleLabel);

            // File upload section
            var uploadPanel = new Panel();
            uploadPanel.Dock = DockStyle.Top;
            uploadPanel.Height = 150;
            uploadPanel.Padding = new Padding(20, 20, 20, 10);

            this.sourceGroupBox = new GroupBox();
            this.sourceGroupBox.Text = "📄 Исходный конфиг (с данными)";
            this.sourceGroupBox.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.sourceGroupBox.Size = new Size(350, 120);
            this.sourceGroupBox.Location = new Point(20, 10);
            this.sourceGroupBox.ForeColor = Color.FromArgb(50, 50, 50);

            this.targetGroupBox = new GroupBox();
            this.targetGroupBox.Text = "📋 Целевой конфиг (пустой)";
            this.targetGroupBox.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.targetGroupBox.Size = new Size(350, 120);
            this.targetGroupBox.Location = new Point(390, 10);
            this.targetGroupBox.ForeColor = Color.FromArgb(50, 50, 50);

            this.sourceSelectBtn = new Button();
            this.sourceSelectBtn.Text = "Выбрать файл";
            this.sourceSelectBtn.Size = new Size(120, 35);
            this.sourceSelectBtn.Location = new Point(20, 30);
            this.sourceSelectBtn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            this.sourceSelectBtn.UseVisualStyleBackColor = false;
            this.sourceSelectBtn.FlatStyle = FlatStyle.Flat;
            this.sourceSelectBtn.TextAlign = ContentAlignment.MiddleCenter;
            this.sourceSelectBtn.Click += new EventHandler(this.sourceSelectBtn_Click);

            this.targetSelectBtn = new Button();
            this.targetSelectBtn.Text = "Выбрать файл";
            this.targetSelectBtn.Size = new Size(120, 35);
            this.targetSelectBtn.Location = new Point(20, 30);
            this.targetSelectBtn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            this.targetSelectBtn.UseVisualStyleBackColor = false;
            this.targetSelectBtn.FlatStyle = FlatStyle.Flat;
            this.targetSelectBtn.TextAlign = ContentAlignment.MiddleCenter;
            this.targetSelectBtn.Click += new EventHandler(this.targetSelectBtn_Click);

            this.sourceStatusLabel = new Label();
            this.sourceStatusLabel.Text = "Файл не выбран";
            this.sourceStatusLabel.Location = new Point(20, 75);
            this.sourceStatusLabel.Size = new Size(300, 20);
            this.sourceStatusLabel.Font = new Font("Segoe UI", 8);
            this.sourceStatusLabel.ForeColor = Color.Gray;

            this.targetStatusLabel = new Label();
            this.targetStatusLabel.Text = "Файл не выбран";
            this.targetStatusLabel.Location = new Point(20, 75);
            this.targetStatusLabel.Size = new Size(300, 20);
            this.targetStatusLabel.Font = new Font("Segoe UI", 8);
            this.targetStatusLabel.ForeColor = Color.Gray;

            this.sourceGroupBox.Controls.Add(this.sourceSelectBtn);
            this.sourceGroupBox.Controls.Add(this.sourceStatusLabel);
            this.targetGroupBox.Controls.Add(this.targetSelectBtn);
            this.targetGroupBox.Controls.Add(this.targetStatusLabel);

            uploadPanel.Controls.Add(this.sourceGroupBox);
            uploadPanel.Controls.Add(this.targetGroupBox);

            // Action buttons panel
            var actionPanel = new Panel();
            actionPanel.Dock = DockStyle.Top;
            actionPanel.Height = 60;
            actionPanel.Padding = new Padding(20, 10, 20, 10);

            this.compareBtn = new Button();
            this.compareBtn.Text = "🔍 Сравнить файлы";
            this.compareBtn.Size = new Size(150, 30);
            this.compareBtn.Location = new Point(400, 10);
            this.compareBtn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.compareBtn.Enabled = false;
            this.compareBtn.UseVisualStyleBackColor = false;
            this.compareBtn.FlatStyle = FlatStyle.Standard;
            this.compareBtn.TextAlign = ContentAlignment.MiddleCenter;
            this.compareBtn.Click += new EventHandler(this.compareBtn_Click);

            this.resetBtn = new Button();
            this.resetBtn.Text = "🔄 Сбросить";
            this.resetBtn.Size = new Size(150, 30);
            this.resetBtn.Location = new Point(570, 10);
            this.resetBtn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.resetBtn.UseVisualStyleBackColor = false;
            this.resetBtn.FlatStyle = FlatStyle.Standard;
            this.resetBtn.TextAlign = ContentAlignment.MiddleCenter;
            this.resetBtn.Click += new EventHandler(this.resetBtn_Click);

            this.progressBar = new ProgressBar();
            this.progressBar.Location = new Point(20, 25);
            this.progressBar.Size = new Size(300, 10);
            this.progressBar.Style = ProgressBarStyle.Marquee;
            this.progressBar.MarqueeAnimationSpeed = 30;
            this.progressBar.Visible = false;

            actionPanel.Controls.Add(this.compareBtn);
            actionPanel.Controls.Add(this.resetBtn);
            actionPanel.Controls.Add(this.progressBar);

            // Results section
            this.resultsTabControl = new TabControl();
            this.resultsTabControl.Dock = DockStyle.Fill;
            this.resultsTabControl.Font = new Font("Segoe UI", 9);
            this.resultsTabControl.Padding = new Point(20, 10);

            // Summary tab
            var summaryTab = new TabPage("📊 Сводка");
            this.summaryLabel = new Label();
            this.summaryLabel.Dock = DockStyle.Top;
            this.summaryLabel.Font = new Font("Segoe UI", 11);
            this.summaryLabel.Padding = new Padding(20);
            this.summaryLabel.Height = 100;
            this.summaryLabel.BackColor = Color.FromArgb(102, 126, 234);
            this.summaryLabel.ForeColor = Color.White;
            this.summaryLabel.TextAlign = ContentAlignment.MiddleCenter;
            summaryTab.Controls.Add(this.summaryLabel);

            // Merged config tab
            var mergedTab = new TabPage("📝 Объединенный конфиг");
            var mergedPanel = new Panel();
            mergedPanel.Dock = DockStyle.Fill;
            mergedPanel.Padding = new Padding(10);

            this.copyBtn = new Button();
            this.copyBtn.Text = "📋 Скопировать";
            this.copyBtn.Size = new Size(120, 30);
            this.copyBtn.Location = new Point(10, 10);
            this.copyBtn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            this.copyBtn.BackColor = Color.FromArgb(40, 167, 69);
            this.copyBtn.ForeColor = Color.White;
            this.copyBtn.FlatStyle = FlatStyle.Flat;
            this.copyBtn.TextAlign = ContentAlignment.MiddleCenter;
            this.copyBtn.Click += new EventHandler(this.copyBtn_Click);

            this.mergedConfigTextBox = new RichTextBox();
            this.mergedConfigTextBox.Location = new Point(10, 50);
            this.mergedConfigTextBox.Size = new Size(mergedPanel.Width - 20, mergedPanel.Height - 60);
            this.mergedConfigTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.mergedConfigTextBox.Font = new Font("Consolas", 9);
            this.mergedConfigTextBox.BackColor = Color.FromArgb(248, 249, 250);
            this.mergedConfigTextBox.ReadOnly = true;
            this.mergedConfigTextBox.ScrollBars = RichTextBoxScrollBars.Both;

            mergedPanel.Controls.Add(this.mergedConfigTextBox);
            mergedTab.Controls.Add(mergedPanel);

            // Added parameters tab
            var addedTab = new TabPage("➕ Новые параметры");
            this.addedListBox = new ListBox();
            this.addedListBox.Dock = DockStyle.Fill;
            this.addedListBox.Font = new Font("Consolas", 9);
            this.addedListBox.BackColor = Color.FromArgb(232, 245, 232);
            this.addedListBox.BorderStyle = BorderStyle.None;
            addedTab.Controls.Add(this.addedListBox);

            // Missing parameters tab
            var missingTab = new TabPage("❌ Отсутствующие");
            this.missingListBox = new ListBox();
            this.missingListBox.Dock = DockStyle.Fill;
            this.missingListBox.Font = new Font("Consolas", 9);
            this.missingListBox.BackColor = Color.FromArgb(255, 234, 234);
            this.missingListBox.BorderStyle = BorderStyle.None;
            missingTab.Controls.Add(this.missingListBox);

            // Different parameters tab
            var differentTab = new TabPage("🔄 Измененные");
            this.differentListBox = new ListBox();
            this.differentListBox.Dock = DockStyle.Fill;
            this.differentListBox.Font = new Font("Consolas", 9);
            this.differentListBox.BackColor = Color.FromArgb(255, 243, 205);
            this.differentListBox.BorderStyle = BorderStyle.None;
            differentTab.Controls.Add(this.differentListBox);

            this.resultsTabControl.TabPages.Add(summaryTab);
            this.resultsTabControl.TabPages.Add(mergedTab);
            this.resultsTabControl.TabPages.Add(addedTab);
            this.resultsTabControl.TabPages.Add(missingTab);
            this.resultsTabControl.TabPages.Add(differentTab);

            // Add all controls to form
            this.Controls.Add(this.resultsTabControl);
            this.Controls.Add(actionPanel);
            this.Controls.Add(uploadPanel);
            this.Controls.Add(this.headerPanel);

            this.ResumeLayout(false);
        }

        #endregion
    }
}
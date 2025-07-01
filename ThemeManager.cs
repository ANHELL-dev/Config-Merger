using System;
using System.Drawing;
using System.Windows.Forms;

namespace ConfigMerger
{
    public enum AppTheme { Light, Dark }

    public static class ThemeManager
    {
        private static AppTheme currentTheme = AppTheme.Light;

        public static AppTheme CurrentTheme
        {
            get => currentTheme;
            set
            {
                currentTheme = value;
                ThemeChanged?.Invoke(value);
            }
        }

        public static event Action<AppTheme> ThemeChanged;

        public static class LightTheme
        {
            public static readonly Color Background = Color.FromArgb(240, 240, 240);
            public static readonly Color Surface = Color.White;
            public static readonly Color Primary = Color.FromArgb(79, 172, 254);
            public static readonly Color Secondary = Color.FromArgb(108, 117, 125);
            public static readonly Color Success = Color.FromArgb(40, 167, 69);
            public static readonly Color Warning = Color.FromArgb(255, 193, 7);
            public static readonly Color Danger = Color.FromArgb(220, 53, 69);
            public static readonly Color Text = Color.FromArgb(33, 37, 41);
            public static readonly Color TextSecondary = Color.FromArgb(108, 117, 125);
            public static readonly Color Border = Color.FromArgb(222, 226, 230);
            public static readonly Color Header = Color.FromArgb(79, 172, 254);
            public static readonly Color HeaderText = Color.White;
            public static readonly Color InputBackground = Color.FromArgb(248, 249, 250);
            public static readonly Color SuccessBackground = Color.FromArgb(232, 245, 232);
            public static readonly Color DangerBackground = Color.FromArgb(255, 234, 234);
            public static readonly Color WarningBackground = Color.FromArgb(255, 243, 205);
        }

        public static class DarkTheme
        {
            public static readonly Color Background = Color.FromArgb(33, 37, 41);
            public static readonly Color Surface = Color.FromArgb(52, 58, 64);
            public static readonly Color Primary = Color.FromArgb(13, 110, 253);
            public static readonly Color Secondary = Color.FromArgb(108, 117, 125);
            public static readonly Color Success = Color.FromArgb(25, 135, 84);
            public static readonly Color Warning = Color.FromArgb(255, 193, 7);
            public static readonly Color Danger = Color.FromArgb(220, 53, 69);
            public static readonly Color Text = Color.FromArgb(248, 249, 250);
            public static readonly Color TextSecondary = Color.FromArgb(173, 181, 189);
            public static readonly Color Border = Color.FromArgb(73, 80, 87);
            public static readonly Color Header = Color.FromArgb(20, 30, 40);
            public static readonly Color HeaderText = Color.FromArgb(248, 249, 250);
            public static readonly Color InputBackground = Color.FromArgb(73, 80, 87);
            public static readonly Color SuccessBackground = Color.FromArgb(20, 50, 30);
            public static readonly Color DangerBackground = Color.FromArgb(60, 20, 30);
            public static readonly Color WarningBackground = Color.FromArgb(60, 50, 20);
        }

        // Методы доступа к цветам - сокращены для читаемости
        public static Color GetBackground() => CurrentTheme == AppTheme.Light ? LightTheme.Background : DarkTheme.Background;
        public static Color GetSurface() => CurrentTheme == AppTheme.Light ? LightTheme.Surface : DarkTheme.Surface;
        public static Color GetPrimary() => CurrentTheme == AppTheme.Light ? LightTheme.Primary : DarkTheme.Primary;
        public static Color GetSecondary() => CurrentTheme == AppTheme.Light ? LightTheme.Secondary : DarkTheme.Secondary;
        public static Color GetSuccess() => CurrentTheme == AppTheme.Light ? LightTheme.Success : DarkTheme.Success;
        public static Color GetWarning() => CurrentTheme == AppTheme.Light ? LightTheme.Warning : DarkTheme.Warning;
        public static Color GetDanger() => CurrentTheme == AppTheme.Light ? LightTheme.Danger : DarkTheme.Danger;
        public static Color GetText() => CurrentTheme == AppTheme.Light ? LightTheme.Text : DarkTheme.Text;
        public static Color GetTextSecondary() => CurrentTheme == AppTheme.Light ? LightTheme.TextSecondary : DarkTheme.TextSecondary;
        public static Color GetBorder() => CurrentTheme == AppTheme.Light ? LightTheme.Border : DarkTheme.Border;
        public static Color GetHeader() => CurrentTheme == AppTheme.Light ? LightTheme.Header : DarkTheme.Header;
        public static Color GetHeaderText() => CurrentTheme == AppTheme.Light ? LightTheme.HeaderText : DarkTheme.HeaderText;
        public static Color GetInputBackground() => CurrentTheme == AppTheme.Light ? LightTheme.InputBackground : DarkTheme.InputBackground;
        public static Color GetSuccessBackground() => CurrentTheme == AppTheme.Light ? LightTheme.SuccessBackground : DarkTheme.SuccessBackground;
        public static Color GetDangerBackground() => CurrentTheme == AppTheme.Light ? LightTheme.DangerBackground : DarkTheme.DangerBackground;
        public static Color GetWarningBackground() => CurrentTheme == AppTheme.Light ? LightTheme.WarningBackground : DarkTheme.WarningBackground;

        public static void ApplyTheme(Form form)
        {
            form.BackColor = GetBackground();
            form.ForeColor = GetText();
            ApplyThemeToControl(form);
        }

        private static void ApplyThemeToControl(Control control)
        {
            foreach (Control child in control.Controls)
            {
                ApplyThemeToSingleControl(child);
                ApplyThemeToControl(child);
            }
        }

        private static void ApplyThemeToSingleControl(Control control)
        {
            if (!(control is Button || control is TabControl || control is TabPage))
            {
                control.BackColor = GetBackground();
                control.ForeColor = GetText();
            }

            if (control is Panel panel)
            {
                if (panel.Name?.Contains("header") == true)
                {
                    panel.BackColor = GetHeader();
                    panel.ForeColor = GetHeaderText();
                }
                else
                {
                    panel.BackColor = GetBackground();
                }
            }
            else if (control is GroupBox)
            {
                control.BackColor = GetBackground();
                control.ForeColor = GetText();
            }
            else if (control is TextBox || control is RichTextBox)
            {
                control.BackColor = GetInputBackground();
                control.ForeColor = GetText();
            }
            else if (control is ListBox listBox)
            {
                ApplyListBoxTheme(listBox);
            }
            else if (control is TabControl tabControl)
            {
                ApplyTabControlTheme(tabControl);
            }
            else if (control is TabPage tabPage)
            {
                ApplyTabPageTheme(tabPage);
            }
            else if (control is Label label)
            {
                ApplyLabelTheme(label);
            }
            else if (control is ProgressBar)
            {
                control.BackColor = GetInputBackground();
            }
        }

        private static void ApplyListBoxTheme(ListBox listBox)
        {
            if (listBox.Parent is TabPage parentTabPage)
            {
                if (parentTabPage.Text.Contains("Новые"))
                    listBox.BackColor = GetSuccessBackground();
                else if (parentTabPage.Text.Contains("Отсутствующие"))
                    listBox.BackColor = GetDangerBackground();
                else if (parentTabPage.Text.Contains("Измененные"))
                    listBox.BackColor = GetWarningBackground();
                else
                    listBox.BackColor = GetInputBackground();
            }
            else
            {
                listBox.BackColor = GetInputBackground();
            }
            listBox.ForeColor = GetText();
        }

        private static void ApplyTabControlTheme(TabControl tabControl)
        {
            tabControl.BackColor = GetBackground();
            tabControl.ForeColor = GetText();

            foreach (TabPage tabPage in tabControl.TabPages)
            {
                ApplyTabPageTheme(tabPage);
            }
        }

        private static void ApplyTabPageTheme(TabPage tabPage)
        {
            if (tabPage.Text.Contains("Новые"))
                tabPage.BackColor = GetSuccessBackground();
            else if (tabPage.Text.Contains("Отсутствующие"))
                tabPage.BackColor = GetDangerBackground();
            else if (tabPage.Text.Contains("Измененные"))
                tabPage.BackColor = GetWarningBackground();
            else
                tabPage.BackColor = GetBackground();

            tabPage.ForeColor = GetText();
        }

        private static void ApplyLabelTheme(Label label)
        {
            if (label.Parent?.Name?.Contains("header") == true)
            {
                label.ForeColor = GetHeaderText();
                label.BackColor = Color.Transparent;
            }
            else
            {
                label.ForeColor = GetText();
                label.BackColor = Color.Transparent;
            }
        }

        public enum ButtonType { Primary, Secondary, Success, Warning, Danger }

        public static void StyleButton(Button button, ButtonType type = ButtonType.Primary)
        {
            var (baseColor, textColor) = GetButtonColors(type);

            button.BackColor = baseColor;
            button.ForeColor = textColor;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.UseVisualStyleBackColor = false;
            button.Cursor = Cursors.Hand;
            button.TextAlign = ContentAlignment.MiddleCenter;

            SetupButtonHoverEffects(button, baseColor);
        }

        private static (Color baseColor, Color textColor) GetButtonColors(ButtonType type)
        {
            return type switch
            {
                ButtonType.Secondary => (GetSecondary(), Color.White),
                ButtonType.Success => (GetSuccess(), Color.White),
                ButtonType.Warning => (GetWarning(), Color.Black),
                ButtonType.Danger => (GetDanger(), Color.White),
                _ => (GetPrimary(), Color.White)
            };
        }

        private static void SetupButtonHoverEffects(Button button, Color baseColor)
        {
            button.MouseEnter += (s, e) =>
            {
                if (button.Enabled)
                    button.BackColor = ControlPaint.Dark(baseColor, 0.1f);
            };

            button.MouseLeave += (s, e) =>
            {
                button.BackColor = button.Enabled ? baseColor : Color.Gray;
            };

            if (!button.Enabled)
                button.BackColor = Color.Gray;
        }

        public static void ToggleTheme()
        {
            CurrentTheme = CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light;
        }

        public static void SaveThemeSettings()
        {
            try
            {
                Properties.Settings.Default.DarkTheme = CurrentTheme == AppTheme.Dark;
                Properties.Settings.Default.Save();
            }
            catch
            {
                // Игнорируем ошибки сохранения настроек
            }
        }

        public static void LoadThemeSettings()
        {
            try
            {
                CurrentTheme = Properties.Settings.Default.DarkTheme ? AppTheme.Dark : AppTheme.Light;
            }
            catch
            {
                CurrentTheme = AppTheme.Light;
            }
        }
    }

    // Кастомный TabControl с темной темой
    public class ThemedTabControl : TabControl
    {
        public ThemedTabControl()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var backColor = ThemeManager.GetBackground();
            using (var brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }

            for (int i = 0; i < TabCount; i++)
            {
                DrawTab(e.Graphics, i);
            }
        }

        private void DrawTab(Graphics g, int index)
        {
            var tabRect = GetTabRect(index);
            var isSelected = (index == SelectedIndex);

            var tabBackColor = isSelected ? ThemeManager.GetPrimary() : ThemeManager.GetBackground();
            using (var brush = new SolidBrush(tabBackColor))
            {
                g.FillRectangle(brush, tabRect);
            }

            if (!isSelected)
            {
                using (var pen = new Pen(ThemeManager.GetBorder()))
                {
                    g.DrawRectangle(pen, tabRect);
                }
            }

            var textColor = isSelected ? Color.White : ThemeManager.GetText();
            TextRenderer.DrawText(g, TabPages[index].Text, Font, tabRect, textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            Invalidate();
        }
    }
}
using System.Drawing;
using System.Windows.Forms;

namespace Cotillon.Desktop.WinForms.Helpers;

public static class AppTheme
{
    public static class Colors
    {
        public static readonly Color Background = Color.FromArgb(244, 246, 251);
        public static readonly Color Surface = Color.White;
        public static readonly Color Border = Color.FromArgb(217, 225, 236);

        public static readonly Color Primary = Color.FromArgb(37, 99, 235);      // azul
        public static readonly Color Secondary = Color.FromArgb(124, 58, 237);   // violeta
        public static readonly Color Accent = Color.FromArgb(236, 72, 153);      // rosa
        public static readonly Color Warning = Color.FromArgb(245, 158, 11);     // amarillo
        public static readonly Color Success = Color.FromArgb(16, 185, 129);     // verde
        public static readonly Color Danger = Color.FromArgb(239, 68, 68);       // rojo

        public static readonly Color TextPrimary = Color.FromArgb(31, 41, 55);
        public static readonly Color TextSecondary = Color.FromArgb(107, 114, 128);

        public static readonly Color GridHeader = Color.FromArgb(37, 99, 235);
        public static readonly Color GridHeaderText = Color.White;
        public static readonly Color GridSelection = Color.FromArgb(219, 234, 254);
    }

    public static Font TitleFont => new("Segoe UI", 20F, FontStyle.Bold);
    public static Font SubtitleFont => new("Segoe UI", 11F, FontStyle.Regular);
    public static Font SectionTitleFont => new("Segoe UI", 14F, FontStyle.Bold);
    public static Font ButtonFont => new("Segoe UI", 10F, FontStyle.Bold);
    public static Font LabelFont => new("Segoe UI", 10F, FontStyle.Regular);
    public static Font ValueFont => new("Segoe UI", 24F, FontStyle.Bold);

    public static void ApplyFormStyle(Form form)
    {
        form.BackColor = Colors.Background;
        form.Font = LabelFont;
        form.ForeColor = Colors.TextPrimary;
    }

    public static void ApplyPrimaryButton(Button button)
    {
        button.BackColor = Colors.Primary;
        button.ForeColor = Color.White;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Font = ButtonFont;
        button.Cursor = Cursors.Hand;
    }

    public static void ApplySecondaryButton(Button button)
    {
        button.BackColor = Colors.Surface;
        button.ForeColor = Colors.TextPrimary;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Colors.Border;
        button.FlatAppearance.BorderSize = 1;
        button.Font = ButtonFont;
        button.Cursor = Cursors.Hand;
    }

    public static void ApplyDangerButton(Button button)
    {
        button.BackColor = Colors.Danger;
        button.ForeColor = Color.White;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Font = ButtonFont;
        button.Cursor = Cursors.Hand;
    }

    public static void ApplyPanelCard(Control control)
    {
        control.BackColor = Colors.Surface;
        control.ForeColor = Colors.TextPrimary;
    }

    public static void ApplyGroupBox(GroupBox groupBox)
    {
        groupBox.BackColor = Colors.Surface;
        groupBox.ForeColor = Colors.TextPrimary;
        groupBox.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
    }

    public static void ApplyGrid(DataGridView dgv)
    {
        dgv.BackgroundColor = Colors.Surface;
        dgv.BorderStyle = BorderStyle.FixedSingle;
        dgv.EnableHeadersVisualStyles = false;

        dgv.ColumnHeadersDefaultCellStyle.BackColor = Colors.GridHeader;
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Colors.GridHeaderText;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

        dgv.DefaultCellStyle.BackColor = Colors.Surface;
        dgv.DefaultCellStyle.ForeColor = Colors.TextPrimary;
        dgv.DefaultCellStyle.SelectionBackColor = Colors.GridSelection;
        dgv.DefaultCellStyle.SelectionForeColor = Colors.TextPrimary;

        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        dgv.RowHeadersVisible = false;
        dgv.GridColor = Colors.Border;
    }

    public static void ApplyTextBox(TextBox textBox)
    {
        textBox.BackColor = Color.White;
        textBox.ForeColor = Colors.TextPrimary;
        textBox.BorderStyle = BorderStyle.FixedSingle;
    }

    public static void ApplyComboBox(ComboBox comboBox)
    {
        comboBox.BackColor = Color.White;
        comboBox.ForeColor = Colors.TextPrimary;
        comboBox.FlatStyle = FlatStyle.Flat;
    }

    public static void ApplyCheckBox(CheckBox checkBox)
    {
        checkBox.ForeColor = Colors.TextPrimary;
        checkBox.BackColor = Color.Transparent;
    }

    public static void ApplyTitle(Label label)
    {
        label.Font = TitleFont;
        label.ForeColor = Colors.TextPrimary;
    }

    public static void ApplySubtitle(Label label)
    {
        label.Font = SubtitleFont;
        label.ForeColor = Colors.TextSecondary;
    }

    public static void ApplyKpiValue(Label label)
    {
        label.Font = ValueFont;
        label.ForeColor = Colors.TextPrimary;
    }
}
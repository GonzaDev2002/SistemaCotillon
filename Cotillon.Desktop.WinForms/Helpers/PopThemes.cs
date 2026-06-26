using System.Drawing.Drawing2D;

namespace Cotillon.Desktop.WinForms.Helpers;

public static class PopTheme
{
    public static class Colors
    {
        public static Color Fondo = Color.FromArgb(247, 248, 252);
        public static Color Card = Color.White;
        public static Color Principal = Color.FromArgb(255, 77, 109);
        public static Color Secundario = Color.FromArgb(79, 70, 229);
        public static Color Amarillo = Color.FromArgb(245, 158, 11);
        public static Color Verde = Color.FromArgb(16, 185, 129);
        public static Color Rojo = Color.FromArgb(239, 68, 68);
        public static Color Texto = Color.FromArgb(30, 41, 59);
        public static Color TextoSuave = Color.FromArgb(100, 116, 139);
        public static Color Borde = Color.FromArgb(226, 232, 240);
    }

    public static void ApplyForm(Form form)
    {
        form.BackColor = Colors.Fondo;
        form.Font = new Font("Segoe UI", 10F);
    }

    public static void ApplyTitle(Label label)
    {
        label.ForeColor = Colors.Texto;
        label.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
    }

    public static void ApplySubtitle(Label label)
    {
        label.ForeColor = Colors.TextoSuave;
        label.Font = new Font("Segoe UI", 11F);
    }

    public static void ApplyCard(Control control)
    {
        control.BackColor = Colors.Card;
        control.ForeColor = Colors.Texto;
    }

    public static void ApplyGroupBox(GroupBox groupBox)
    {
        groupBox.BackColor = Colors.Card;
        groupBox.ForeColor = Colors.Texto;
        groupBox.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
    }

    public static void ApplyPrimaryButton(Button btn)
    {
        btn.BackColor = Colors.Principal;
        btn.ForeColor = Color.White;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btn.Cursor = Cursors.Hand;
    }

    public static void ApplySecondaryButton(Button btn)
    {
        btn.BackColor = Color.White;
        btn.ForeColor = Colors.Secundario;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderColor = Colors.Secundario;
        btn.FlatAppearance.BorderSize = 1;
        btn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btn.Cursor = Cursors.Hand;
    }

    public static void ApplyDangerButton(Button btn)
    {
        btn.BackColor = Colors.Rojo;
        btn.ForeColor = Color.White;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btn.Cursor = Cursors.Hand;
    }

    public static void ApplySuccessButton(Button btn)
    {
        btn.BackColor = Colors.Verde;
        btn.ForeColor = Color.White;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btn.Cursor = Cursors.Hand;
    }

    public static void ApplyInput(TextBox txt)
    {
        txt.BorderStyle = BorderStyle.FixedSingle;
        txt.Font = new Font("Segoe UI", 10F);
        txt.BackColor = Color.White;
        txt.ForeColor = Colors.Texto;
    }

    public static void ApplyCombo(ComboBox cmb)
    {
        cmb.Font = new Font("Segoe UI", 10F);
        cmb.BackColor = Color.White;
        cmb.ForeColor = Colors.Texto;
        cmb.DropDownStyle = ComboBoxStyle.DropDownList;
    }

    public static void ApplyGrid(DataGridView dgv)
    {
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.RowHeadersVisible = false;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.ReadOnly = true;
        dgv.MultiSelect = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.BackgroundColor = Color.White;
        dgv.BorderStyle = BorderStyle.FixedSingle;
        dgv.EnableHeadersVisualStyles = false;

        dgv.ColumnHeadersDefaultCellStyle.BackColor = Colors.Secundario;
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

        dgv.DefaultCellStyle.BackColor = Color.White;
        dgv.DefaultCellStyle.ForeColor = Colors.Texto;
        dgv.DefaultCellStyle.SelectionBackColor = Colors.Principal;
        dgv.DefaultCellStyle.SelectionForeColor = Color.White;
        dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9F);

        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 255);
        dgv.RowTemplate.Height = 28;
    }

    public static void ApplyListBox(ListBox list)
    {
        list.BackColor = Color.White;
        list.ForeColor = Colors.Texto;
        list.Font = new Font("Segoe UI", 10F);
        list.BorderStyle = BorderStyle.FixedSingle;
    }

    public static void Round(Control control, int radius = 14)
    {
        Rectangle bounds = new Rectangle(0, 0, control.Width, control.Height);
        int diameter = radius * 2;

        GraphicsPath path = new GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        control.Region = new Region(path);
    }
}
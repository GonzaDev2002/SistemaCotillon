namespace Cotillon.Desktop.WinForms.Forms;

partial class FrmUsuarios
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlHeader;
    private Label lblTitulo;
    private Label lblSubtitulo;

    private GroupBox grpDatos;
    private GroupBox grpListado;

    private Label lblUsuario;
    private TextBox txtUsuario;
    private Label lblNombreCompleto;
    private TextBox txtNombreCompleto;
    private Label lblPin;
    private TextBox txtPin;
    private Label lblRol;
    private ComboBox cmbRol;
    private CheckBox chkActivo;

    private Button btnGuardar;
    private Button btnDarBaja;
    private Button btnReactivar;
    private Button btnLimpiar;
    private Button btnCerrar;

    private CheckBox chkVerInactivos;
    private DataGridView dgvUsuarios;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        pnlHeader = new Panel();
        lblTitulo = new Label();
        lblSubtitulo = new Label();
        grpDatos = new GroupBox();
        lblUsuario = new Label();
        txtUsuario = new TextBox();
        lblNombreCompleto = new Label();
        txtNombreCompleto = new TextBox();
        lblPin = new Label();
        txtPin = new TextBox();
        lblRol = new Label();
        cmbRol = new ComboBox();
        chkActivo = new CheckBox();
        btnGuardar = new Button();
        btnDarBaja = new Button();
        btnReactivar = new Button();
        btnLimpiar = new Button();
        btnCerrar = new Button();
        grpListado = new GroupBox();
        chkVerInactivos = new CheckBox();
        dgvUsuarios = new DataGridView();

        pnlHeader.SuspendLayout();
        grpDatos.SuspendLayout();
        grpListado.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvUsuarios).BeginInit();
        SuspendLayout();

        // pnlHeader
        pnlHeader.BackColor = Color.FromArgb(255, 77, 109);
        pnlHeader.Controls.Add(lblTitulo);
        pnlHeader.Controls.Add(lblSubtitulo);
        pnlHeader.Location = new Point(20, 20);
        pnlHeader.Name = "pnlHeader";
        pnlHeader.Size = new Size(900, 88);
        pnlHeader.TabIndex = 0;

        lblTitulo.AutoSize = true;
        lblTitulo.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        lblTitulo.ForeColor = Color.White;
        lblTitulo.Location = new Point(25, 10);
        lblTitulo.Name = "lblTitulo";
        lblTitulo.Size = new Size(211, 41);
        lblTitulo.TabIndex = 0;
        lblTitulo.Text = "👤 Usuarios";

        lblSubtitulo.AutoSize = true;
        lblSubtitulo.Font = new Font("Segoe UI", 11F);
        lblSubtitulo.ForeColor = Color.White;
        lblSubtitulo.Location = new Point(28, 55);
        lblSubtitulo.Name = "lblSubtitulo";
        lblSubtitulo.Size = new Size(402, 20);
        lblSubtitulo.TabIndex = 1;
        lblSubtitulo.Text = "Administración de usuarios, roles y accesos del sistema";

        // grpDatos
        grpDatos.BackColor = Color.White;
        grpDatos.Controls.Add(lblUsuario);
        grpDatos.Controls.Add(txtUsuario);
        grpDatos.Controls.Add(lblNombreCompleto);
        grpDatos.Controls.Add(txtNombreCompleto);
        grpDatos.Controls.Add(lblPin);
        grpDatos.Controls.Add(txtPin);
        grpDatos.Controls.Add(lblRol);
        grpDatos.Controls.Add(cmbRol);
        grpDatos.Controls.Add(chkActivo);
        grpDatos.Controls.Add(btnGuardar);
        grpDatos.Controls.Add(btnDarBaja);
        grpDatos.Controls.Add(btnReactivar);
        grpDatos.Controls.Add(btnLimpiar);
        grpDatos.Controls.Add(btnCerrar);
        grpDatos.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpDatos.Location = new Point(20, 125);
        grpDatos.Name = "grpDatos";
        grpDatos.Size = new Size(900, 200);
        grpDatos.TabIndex = 1;
        grpDatos.TabStop = false;
        grpDatos.Text = "Datos del usuario";

        lblUsuario.AutoSize = true;
        lblUsuario.Location = new Point(25, 38);
        lblUsuario.Name = "lblUsuario";
        lblUsuario.Size = new Size(62, 19);
        lblUsuario.TabIndex = 0;
        lblUsuario.Text = "Usuario:";

        txtUsuario.Font = new Font("Segoe UI", 10F);
        txtUsuario.Location = new Point(150, 34);
        txtUsuario.Name = "txtUsuario";
        txtUsuario.Size = new Size(250, 25);
        txtUsuario.TabIndex = 1;

        lblNombreCompleto.AutoSize = true;
        lblNombreCompleto.Location = new Point(440, 38);
        lblNombreCompleto.Name = "lblNombreCompleto";
        lblNombreCompleto.Size = new Size(137, 19);
        lblNombreCompleto.TabIndex = 2;
        lblNombreCompleto.Text = "Nombre completo:";

        txtNombreCompleto.Font = new Font("Segoe UI", 10F);
        txtNombreCompleto.Location = new Point(590, 34);
        txtNombreCompleto.Name = "txtNombreCompleto";
        txtNombreCompleto.Size = new Size(270, 25);
        txtNombreCompleto.TabIndex = 3;

        lblPin.AutoSize = true;
        lblPin.Location = new Point(25, 84);
        lblPin.Name = "lblPin";
        lblPin.Size = new Size(35, 19);
        lblPin.TabIndex = 4;
        lblPin.Text = "PIN:";

        txtPin.Font = new Font("Segoe UI", 10F);
        txtPin.Location = new Point(150, 80);
        txtPin.Name = "txtPin";
        txtPin.Size = new Size(250, 25);
        txtPin.TabIndex = 5;

        lblRol.AutoSize = true;
        lblRol.Location = new Point(440, 84);
        lblRol.Name = "lblRol";
        lblRol.Size = new Size(32, 19);
        lblRol.TabIndex = 6;
        lblRol.Text = "Rol:";

        cmbRol.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbRol.Font = new Font("Segoe UI", 10F);
        cmbRol.Location = new Point(590, 80);
        cmbRol.Name = "cmbRol";
        cmbRol.Size = new Size(270, 25);
        cmbRol.TabIndex = 7;

        chkActivo.AutoSize = true;
        chkActivo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        chkActivo.Location = new Point(150, 120);
        chkActivo.Name = "chkActivo";
        chkActivo.Size = new Size(69, 23);
        chkActivo.TabIndex = 8;
        chkActivo.Text = "Activo";

        btnGuardar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnGuardar.Location = new Point(150, 152);
        btnGuardar.Name = "btnGuardar";
        btnGuardar.Size = new Size(125, 34);
        btnGuardar.TabIndex = 9;
        btnGuardar.Text = "Guardar";
        btnGuardar.UseVisualStyleBackColor = false;
        btnGuardar.Click += btnGuardar_Click;

        btnDarBaja.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnDarBaja.Location = new Point(290, 152);
        btnDarBaja.Name = "btnDarBaja";
        btnDarBaja.Size = new Size(125, 34);
        btnDarBaja.TabIndex = 10;
        btnDarBaja.Text = "Dar baja";
        btnDarBaja.UseVisualStyleBackColor = false;
        btnDarBaja.Click += btnDarBaja_Click;

        btnReactivar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnReactivar.Location = new Point(430, 152);
        btnReactivar.Name = "btnReactivar";
        btnReactivar.Size = new Size(125, 34);
        btnReactivar.TabIndex = 11;
        btnReactivar.Text = "Reactivar";
        btnReactivar.UseVisualStyleBackColor = false;
        btnReactivar.Click += btnReactivar_Click;

        btnLimpiar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnLimpiar.Location = new Point(570, 152);
        btnLimpiar.Name = "btnLimpiar";
        btnLimpiar.Size = new Size(125, 34);
        btnLimpiar.TabIndex = 12;
        btnLimpiar.Text = "Limpiar";
        btnLimpiar.UseVisualStyleBackColor = false;
        btnLimpiar.Click += btnLimpiar_Click;

        btnCerrar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnCerrar.Location = new Point(710, 152);
        btnCerrar.Name = "btnCerrar";
        btnCerrar.Size = new Size(125, 34);
        btnCerrar.TabIndex = 13;
        btnCerrar.Text = "Cerrar";
        btnCerrar.UseVisualStyleBackColor = false;
        btnCerrar.Click += btnCerrar_Click;

        // grpListado
        grpListado.BackColor = Color.White;
        grpListado.Controls.Add(chkVerInactivos);
        grpListado.Controls.Add(dgvUsuarios);
        grpListado.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpListado.Location = new Point(20, 345);
        grpListado.Name = "grpListado";
        grpListado.Size = new Size(900, 365);
        grpListado.TabIndex = 2;
        grpListado.TabStop = false;
        grpListado.Text = "Usuarios registrados";

        chkVerInactivos.AutoSize = true;
        chkVerInactivos.Checked = true;
        chkVerInactivos.CheckState = CheckState.Checked;
        chkVerInactivos.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        chkVerInactivos.Location = new Point(18, 28);
        chkVerInactivos.Name = "chkVerInactivos";
        chkVerInactivos.Size = new Size(112, 23);
        chkVerInactivos.TabIndex = 0;
        chkVerInactivos.Text = "Ver inactivos";
        chkVerInactivos.CheckedChanged += chkVerInactivos_CheckedChanged;

        dgvUsuarios.AllowUserToAddRows = false;
        dgvUsuarios.AllowUserToDeleteRows = false;
        dgvUsuarios.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvUsuarios.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvUsuarios.Location = new Point(18, 62);
        dgvUsuarios.Name = "dgvUsuarios";
        dgvUsuarios.ReadOnly = true;
        dgvUsuarios.RowHeadersVisible = false;
        dgvUsuarios.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvUsuarios.Size = new Size(864, 280);
        dgvUsuarios.TabIndex = 1;
        dgvUsuarios.CellClick += dgvUsuarios_CellClick;

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(250, 248, 242);
        BackgroundImage = Properties.Resources.fondoForms;
        BackgroundImageLayout = ImageLayout.Stretch;
        ClientSize = new Size(945, 735);
        Controls.Add(pnlHeader);
        Controls.Add(grpDatos);
        Controls.Add(grpListado);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "FrmUsuarios";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "POP Cotillón - Usuarios";
        Load += FrmUsuarios_Load;

        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        grpDatos.ResumeLayout(false);
        grpDatos.PerformLayout();
        grpListado.ResumeLayout(false);
        grpListado.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvUsuarios).EndInit();
        ResumeLayout(false);
    }
}

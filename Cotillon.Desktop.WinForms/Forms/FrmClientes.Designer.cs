namespace Cotillon.Desktop.WinForms.Forms;

partial class FrmClientes
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlHeader;
    private Label lblTitulo;
    private Label lblSubtitulo;

    private GroupBox grpDatos;
    private GroupBox grpListado;

    private Label lblNombre;
    private TextBox txtNombre;
    private Label lblDocumento;
    private TextBox txtDocumento;
    private Label lblTelefono;
    private TextBox txtTelefono;
    private Label lblEmail;
    private TextBox txtEmail;
    private Label lblDireccion;
    private TextBox txtDireccion;
    private Label lblObservaciones;
    private TextBox txtObservaciones;

    private Button btnGuardar;
    private Button btnEliminar;
    private Button btnLimpiar;
    private Button btnCerrar;

    private DataGridView dgvClientes;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        pnlHeader = new Panel();
        lblTitulo = new Label();
        lblSubtitulo = new Label();
        grpDatos = new GroupBox();
        lblNombre = new Label();
        txtNombre = new TextBox();
        lblDocumento = new Label();
        txtDocumento = new TextBox();
        lblTelefono = new Label();
        txtTelefono = new TextBox();
        lblEmail = new Label();
        txtEmail = new TextBox();
        lblDireccion = new Label();
        txtDireccion = new TextBox();
        lblObservaciones = new Label();
        txtObservaciones = new TextBox();
        btnGuardar = new Button();
        btnEliminar = new Button();
        btnLimpiar = new Button();
        btnCerrar = new Button();
        grpListado = new GroupBox();
        dgvClientes = new DataGridView();

        pnlHeader.SuspendLayout();
        grpDatos.SuspendLayout();
        grpListado.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvClientes).BeginInit();
        SuspendLayout();

        // pnlHeader
        pnlHeader.BackColor = Color.FromArgb(255, 77, 109);
        pnlHeader.BorderStyle = BorderStyle.None;
        pnlHeader.Controls.Add(lblTitulo);
        pnlHeader.Controls.Add(lblSubtitulo);
        pnlHeader.Location = new Point(20, 18);
        pnlHeader.Name = "pnlHeader";
        pnlHeader.Size = new Size(940, 90);
        pnlHeader.TabIndex = 0;

        // lblTitulo
        lblTitulo.AutoSize = true;
        lblTitulo.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        lblTitulo.ForeColor = Color.White;
        lblTitulo.Location = new Point(25, 12);
        lblTitulo.Name = "lblTitulo";
        lblTitulo.Size = new Size(263, 41);
        lblTitulo.TabIndex = 0;
        lblTitulo.Text = "👥 Clientes";

        // lblSubtitulo
        lblSubtitulo.AutoSize = true;
        lblSubtitulo.Font = new Font("Segoe UI", 11F);
        lblSubtitulo.ForeColor = Color.White;
        lblSubtitulo.Location = new Point(28, 58);
        lblSubtitulo.Name = "lblSubtitulo";
        lblSubtitulo.Size = new Size(392, 20);
        lblSubtitulo.TabIndex = 1;
        lblSubtitulo.Text = "Alta, modificación y consulta de clientes del negocio";

        // grpDatos
        grpDatos.BackColor = Color.White;
        grpDatos.Controls.Add(lblNombre);
        grpDatos.Controls.Add(txtNombre);
        grpDatos.Controls.Add(lblDocumento);
        grpDatos.Controls.Add(txtDocumento);
        grpDatos.Controls.Add(lblTelefono);
        grpDatos.Controls.Add(txtTelefono);
        grpDatos.Controls.Add(lblEmail);
        grpDatos.Controls.Add(txtEmail);
        grpDatos.Controls.Add(lblDireccion);
        grpDatos.Controls.Add(txtDireccion);
        grpDatos.Controls.Add(lblObservaciones);
        grpDatos.Controls.Add(txtObservaciones);
        grpDatos.Controls.Add(btnGuardar);
        grpDatos.Controls.Add(btnEliminar);
        grpDatos.Controls.Add(btnLimpiar);
        grpDatos.Controls.Add(btnCerrar);
        grpDatos.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpDatos.Location = new Point(20, 125);
        grpDatos.Name = "grpDatos";
        grpDatos.Size = new Size(940, 245);
        grpDatos.TabIndex = 1;
        grpDatos.TabStop = false;
        grpDatos.Text = "Datos del cliente";

        // lblNombre
        lblNombre.AutoSize = true;
        lblNombre.Location = new Point(25, 38);
        lblNombre.Name = "lblNombre";
        lblNombre.Size = new Size(67, 19);
        lblNombre.TabIndex = 0;
        lblNombre.Text = "Nombre:";

        // txtNombre
        txtNombre.Location = new Point(120, 35);
        txtNombre.Name = "txtNombre";
        txtNombre.Size = new Size(300, 25);
        txtNombre.TabIndex = 1;

        // lblDocumento
        lblDocumento.AutoSize = true;
        lblDocumento.Location = new Point(475, 38);
        lblDocumento.Name = "lblDocumento";
        lblDocumento.Size = new Size(91, 19);
        lblDocumento.TabIndex = 2;
        lblDocumento.Text = "Documento:";

        // txtDocumento
        txtDocumento.Location = new Point(590, 35);
        txtDocumento.Name = "txtDocumento";
        txtDocumento.Size = new Size(300, 25);
        txtDocumento.TabIndex = 3;

        // lblTelefono
        lblTelefono.AutoSize = true;
        lblTelefono.Location = new Point(25, 82);
        lblTelefono.Name = "lblTelefono";
        lblTelefono.Size = new Size(72, 19);
        lblTelefono.TabIndex = 4;
        lblTelefono.Text = "Teléfono:";

        // txtTelefono
        txtTelefono.Location = new Point(120, 79);
        txtTelefono.Name = "txtTelefono";
        txtTelefono.Size = new Size(300, 25);
        txtTelefono.TabIndex = 5;

        // lblEmail
        lblEmail.AutoSize = true;
        lblEmail.Location = new Point(475, 82);
        lblEmail.Name = "lblEmail";
        lblEmail.Size = new Size(52, 19);
        lblEmail.TabIndex = 6;
        lblEmail.Text = "Email:";

        // txtEmail
        txtEmail.Location = new Point(590, 79);
        txtEmail.Name = "txtEmail";
        txtEmail.Size = new Size(300, 25);
        txtEmail.TabIndex = 7;

        // lblDireccion
        lblDireccion.AutoSize = true;
        lblDireccion.Location = new Point(25, 126);
        lblDireccion.Name = "lblDireccion";
        lblDireccion.Size = new Size(79, 19);
        lblDireccion.TabIndex = 8;
        lblDireccion.Text = "Dirección:";

        // txtDireccion
        txtDireccion.Location = new Point(120, 123);
        txtDireccion.Name = "txtDireccion";
        txtDireccion.Size = new Size(300, 25);
        txtDireccion.TabIndex = 9;

        // lblObservaciones
        lblObservaciones.AutoSize = true;
        lblObservaciones.Location = new Point(475, 126);
        lblObservaciones.Name = "lblObservaciones";
        lblObservaciones.Size = new Size(111, 19);
        lblObservaciones.TabIndex = 10;
        lblObservaciones.Text = "Observaciones:";

        // txtObservaciones
        txtObservaciones.Location = new Point(590, 123);
        txtObservaciones.Multiline = true;
        txtObservaciones.Name = "txtObservaciones";
        txtObservaciones.Size = new Size(300, 60);
        txtObservaciones.TabIndex = 11;

        // btnGuardar
        btnGuardar.Location = new Point(120, 190);
        btnGuardar.Name = "btnGuardar";
        btnGuardar.Size = new Size(160, 36);
        btnGuardar.TabIndex = 12;
        btnGuardar.Text = "Guardar";
        btnGuardar.UseVisualStyleBackColor = false;
        btnGuardar.Click += btnGuardar_Click;

        // btnEliminar
        btnEliminar.Location = new Point(300, 190);
        btnEliminar.Name = "btnEliminar";
        btnEliminar.Size = new Size(160, 36);
        btnEliminar.TabIndex = 13;
        btnEliminar.Text = "Eliminar";
        btnEliminar.UseVisualStyleBackColor = false;
        btnEliminar.Click += btnEliminar_Click;

        // btnLimpiar
        btnLimpiar.Location = new Point(480, 190);
        btnLimpiar.Name = "btnLimpiar";
        btnLimpiar.Size = new Size(160, 36);
        btnLimpiar.TabIndex = 14;
        btnLimpiar.Text = "Limpiar";
        btnLimpiar.UseVisualStyleBackColor = false;
        btnLimpiar.Click += btnLimpiar_Click;

        // btnCerrar
        btnCerrar.Location = new Point(660, 190);
        btnCerrar.Name = "btnCerrar";
        btnCerrar.Size = new Size(160, 36);
        btnCerrar.TabIndex = 15;
        btnCerrar.Text = "Cerrar";
        btnCerrar.UseVisualStyleBackColor = false;
        btnCerrar.Click += btnCerrar_Click;

        // grpListado
        grpListado.BackColor = Color.White;
        grpListado.Controls.Add(dgvClientes);
        grpListado.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpListado.Location = new Point(20, 390);
        grpListado.Name = "grpListado";
        grpListado.Size = new Size(940, 320);
        grpListado.TabIndex = 2;
        grpListado.TabStop = false;
        grpListado.Text = "Clientes registrados";

        // dgvClientes
        dgvClientes.AllowUserToAddRows = false;
        dgvClientes.AllowUserToDeleteRows = false;
        dgvClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvClientes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvClientes.Dock = DockStyle.Fill;
        dgvClientes.Location = new Point(3, 21);
        dgvClientes.Name = "dgvClientes";
        dgvClientes.ReadOnly = true;
        dgvClientes.RowHeadersVisible = false;
        dgvClientes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvClientes.Size = new Size(934, 296);
        dgvClientes.TabIndex = 0;
        dgvClientes.CellClick += dgvClientes_CellClick;

        // FrmClientes
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(250, 248, 242);
        BackgroundImage = Properties.Resources.fondoForms;
        BackgroundImageLayout = ImageLayout.Stretch;
        ClientSize = new Size(980, 735);
        Controls.Add(pnlHeader);
        Controls.Add(grpDatos);
        Controls.Add(grpListado);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "FrmClientes";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "POP Cotillón - Clientes";
        Load += FrmClientes_Load;

        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        grpDatos.ResumeLayout(false);
        grpDatos.PerformLayout();
        grpListado.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvClientes).EndInit();
        ResumeLayout(false);
    }
}

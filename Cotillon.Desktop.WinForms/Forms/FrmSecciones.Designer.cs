namespace Cotillon.Desktop.WinForms.Forms;

partial class FrmSecciones
{
    private System.ComponentModel.IContainer components = null;

    private Label lblTitulo;
    private Label lblNombreSeccion;
    private TextBox txtNombreSeccion;
    private CheckBox chkActivo;
    private Button btnGuardar;
    private Button btnActualizar;
    private Button btnLimpiarCampos;
    private DataGridView dgvSecciones;
    private DataGridViewTextBoxColumn colSeccionId;
    private DataGridViewTextBoxColumn colNombreSeccion;
    private DataGridViewCheckBoxColumn colActivo;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        lblTitulo = new Label();
        lblNombreSeccion = new Label();
        txtNombreSeccion = new TextBox();
        chkActivo = new CheckBox();
        btnGuardar = new Button();
        btnActualizar = new Button();
        btnLimpiarCampos = new Button();
        dgvSecciones = new DataGridView();
        colSeccionId = new DataGridViewTextBoxColumn();
        colNombreSeccion = new DataGridViewTextBoxColumn();
        colActivo = new DataGridViewCheckBoxColumn();
        ((System.ComponentModel.ISupportInitialize)dgvSecciones).BeginInit();
        SuspendLayout();
        // 
        // lblTitulo
        // 
        lblTitulo.AutoSize = true;
        lblTitulo.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitulo.Location = new Point(25, 20);
        lblTitulo.Name = "lblTitulo";
        lblTitulo.Size = new Size(113, 30);
        lblTitulo.TabIndex = 0;
        lblTitulo.Text = "Secciones";
        // 
        // lblNombreSeccion
        // 
        lblNombreSeccion.AutoSize = true;
        lblNombreSeccion.Location = new Point(25, 75);
        lblNombreSeccion.Name = "lblNombreSeccion";
        lblNombreSeccion.Size = new Size(110, 15);
        lblNombreSeccion.TabIndex = 1;
        lblNombreSeccion.Text = "Nombre de sección";
        // 
        // txtNombreSeccion
        // 
        txtNombreSeccion.Location = new Point(25, 95);
        txtNombreSeccion.Name = "txtNombreSeccion";
        txtNombreSeccion.Size = new Size(280, 23);
        txtNombreSeccion.TabIndex = 2;
        // 
        // chkActivo
        // 
        chkActivo.AutoSize = true;
        chkActivo.Location = new Point(25, 135);
        chkActivo.Name = "chkActivo";
        chkActivo.Size = new Size(60, 19);
        chkActivo.TabIndex = 3;
        chkActivo.Text = "Activo";
        chkActivo.UseVisualStyleBackColor = true;
        // 
        // btnGuardar
        // 
        btnGuardar.Location = new Point(25, 170);
        btnGuardar.Name = "btnGuardar";
        btnGuardar.Size = new Size(120, 35);
        btnGuardar.TabIndex = 4;
        btnGuardar.Text = "Agregar";
        btnGuardar.UseVisualStyleBackColor = true;
        btnGuardar.Click += btnGuardar_Click;
        // 
        // btnActualizar
        // 
        btnActualizar.Location = new Point(155, 170);
        btnActualizar.Name = "btnActualizar";
        btnActualizar.Size = new Size(120, 35);
        btnActualizar.TabIndex = 5;
        btnActualizar.Text = "Actualizar";
        btnActualizar.UseVisualStyleBackColor = true;
        btnActualizar.Click += btnActualizar_Click;
        // 
        // btnLimpiarCampos
        // 
        btnLimpiarCampos.Location = new Point(285, 170);
        btnLimpiarCampos.Name = "btnLimpiarCampos";
        btnLimpiarCampos.Size = new Size(120, 35);
        btnLimpiarCampos.TabIndex = 6;
        btnLimpiarCampos.Text = "Limpiar Campos";
        btnLimpiarCampos.UseVisualStyleBackColor = true;
        btnLimpiarCampos.Click += btnNuevo_Click;
        // 
        // dgvSecciones
        // 
        dgvSecciones.AllowUserToAddRows = false;
        dgvSecciones.AllowUserToDeleteRows = false;
        dgvSecciones.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvSecciones.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvSecciones.Columns.AddRange(new DataGridViewColumn[] { colSeccionId, colNombreSeccion, colActivo });
        dgvSecciones.Location = new Point(25, 230);
        dgvSecciones.MultiSelect = false;
        dgvSecciones.Name = "dgvSecciones";
        dgvSecciones.ReadOnly = true;
        dgvSecciones.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvSecciones.Size = new Size(540, 260);
        dgvSecciones.TabIndex = 7;
        dgvSecciones.CellClick += dgvSecciones_CellClick;
        // 
        // colSeccionId
        // 
        colSeccionId.DataPropertyName = "SeccionId";
        colSeccionId.HeaderText = "ID";
        colSeccionId.Name = "colSeccionId";
        colSeccionId.ReadOnly = true;
        colSeccionId.Visible = false;
        // 
        // colNombreSeccion
        // 
        colNombreSeccion.DataPropertyName = "NombreSeccion";
        colNombreSeccion.HeaderText = "Nombre";
        colNombreSeccion.Name = "colNombreSeccion";
        colNombreSeccion.ReadOnly = true;
        // 
        // colActivo
        // 
        colActivo.DataPropertyName = "Activo";
        colActivo.HeaderText = "Activo";
        colActivo.Name = "colActivo";
        colActivo.ReadOnly = true;
        // 
        // FrmSecciones
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(600, 520);
        Controls.Add(dgvSecciones);
        Controls.Add(btnLimpiarCampos);
        Controls.Add(btnActualizar);
        Controls.Add(btnGuardar);
        Controls.Add(chkActivo);
        Controls.Add(txtNombreSeccion);
        Controls.Add(lblNombreSeccion);
        Controls.Add(lblTitulo);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "FrmSecciones";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Gestión de Secciones";
        Load += FrmSecciones_Load;
        ((System.ComponentModel.ISupportInitialize)dgvSecciones).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
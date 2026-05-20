namespace Cotillon.Desktop.WinForms.Forms;

partial class FrmCategorias
{
    private System.ComponentModel.IContainer components = null;

    private Label lblTitulo;
    private Label lblSeccion;
    private ComboBox cmbSeccion;
    private Button btnBuscar;
    private Label lblNombreCategoria;
    private TextBox txtNombreCategoria;
    private CheckBox chkActivo;
    private Button btnGuardar;
    private Button btnActualizar;
    private Button btnLimpiar;
    private DataGridView dgvCategorias;
    private DataGridViewTextBoxColumn colCategoriaId;
    private DataGridViewTextBoxColumn colSeccionId;
    private DataGridViewTextBoxColumn colNombreCategoria;
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
        lblSeccion = new Label();
        cmbSeccion = new ComboBox();
        btnBuscar = new Button();
        lblNombreCategoria = new Label();
        txtNombreCategoria = new TextBox();
        chkActivo = new CheckBox();
        btnGuardar = new Button();
        btnActualizar = new Button();
        btnLimpiar = new Button();
        dgvCategorias = new DataGridView();
        colCategoriaId = new DataGridViewTextBoxColumn();
        colSeccionId = new DataGridViewTextBoxColumn();
        colNombreCategoria = new DataGridViewTextBoxColumn();
        colActivo = new DataGridViewCheckBoxColumn();
        ((System.ComponentModel.ISupportInitialize)dgvCategorias).BeginInit();
        SuspendLayout();

        // lblTitulo
        lblTitulo.AutoSize = true;
        lblTitulo.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitulo.Location = new Point(25, 20);
        lblTitulo.Name = "lblTitulo";
        lblTitulo.Size = new Size(122, 30);
        lblTitulo.TabIndex = 0;
        lblTitulo.Text = "Categorías";

        // lblSeccion
        lblSeccion.AutoSize = true;
        lblSeccion.Location = new Point(25, 75);
        lblSeccion.Name = "lblSeccion";
        lblSeccion.Size = new Size(48, 15);
        lblSeccion.TabIndex = 1;
        lblSeccion.Text = "Sección";

        // cmbSeccion
        cmbSeccion.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbSeccion.FormattingEnabled = true;
        cmbSeccion.Location = new Point(25, 95);
        cmbSeccion.Name = "cmbSeccion";
        cmbSeccion.Size = new Size(280, 23);
        cmbSeccion.TabIndex = 2;

        // btnBuscar
        btnBuscar.Location = new Point(320, 93);
        btnBuscar.Name = "btnBuscar";
        btnBuscar.Size = new Size(120, 28);
        btnBuscar.TabIndex = 3;
        btnBuscar.Text = "Buscar";
        btnBuscar.UseVisualStyleBackColor = true;
        btnBuscar.Click += btnBuscar_Click;

        // lblNombreCategoria
        lblNombreCategoria.AutoSize = true;
        lblNombreCategoria.Location = new Point(25, 145);
        lblNombreCategoria.Name = "lblNombreCategoria";
        lblNombreCategoria.Size = new Size(123, 15);
        lblNombreCategoria.TabIndex = 4;
        lblNombreCategoria.Text = "Nombre de categoría";

        // txtNombreCategoria
        txtNombreCategoria.Location = new Point(25, 165);
        txtNombreCategoria.Name = "txtNombreCategoria";
        txtNombreCategoria.Size = new Size(280, 23);
        txtNombreCategoria.TabIndex = 5;

        // chkActivo
        chkActivo.AutoSize = true;
        chkActivo.Location = new Point(25, 205);
        chkActivo.Name = "chkActivo";
        chkActivo.Size = new Size(59, 19);
        chkActivo.TabIndex = 6;
        chkActivo.Text = "Activo";
        chkActivo.UseVisualStyleBackColor = true;

        // btnGuardar
        btnGuardar.Location = new Point(25, 240);
        btnGuardar.Name = "btnGuardar";
        btnGuardar.Size = new Size(120, 35);
        btnGuardar.TabIndex = 7;
        btnGuardar.Text = "Agregar";
        btnGuardar.UseVisualStyleBackColor = true;
        btnGuardar.Click += btnGuardar_Click;

        // btnActualizar
        btnActualizar.Location = new Point(155, 240);
        btnActualizar.Name = "btnActualizar";
        btnActualizar.Size = new Size(120, 35);
        btnActualizar.TabIndex = 8;
        btnActualizar.Text = "Actualizar";
        btnActualizar.UseVisualStyleBackColor = true;
        btnActualizar.Click += btnActualizar_Click;

        // btnLimpiar
        btnLimpiar.Location = new Point(285, 240);
        btnLimpiar.Name = "btnLimpiar";
        btnLimpiar.Size = new Size(120, 35);
        btnLimpiar.TabIndex = 9;
        btnLimpiar.Text = "Limpiar";
        btnLimpiar.UseVisualStyleBackColor = true;
        btnLimpiar.Click += btnLimpiar_Click;

        // dgvCategorias
        dgvCategorias.AllowUserToAddRows = false;
        dgvCategorias.AllowUserToDeleteRows = false;
        dgvCategorias.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvCategorias.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvCategorias.Columns.AddRange(new DataGridViewColumn[] { colCategoriaId, colSeccionId, colNombreCategoria, colActivo });
        dgvCategorias.Location = new Point(25, 300);
        dgvCategorias.MultiSelect = false;
        dgvCategorias.Name = "dgvCategorias";
        dgvCategorias.ReadOnly = true;
        dgvCategorias.RowHeadersVisible = false;
        dgvCategorias.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvCategorias.Size = new Size(620, 260);
        dgvCategorias.TabIndex = 10;
        dgvCategorias.CellClick += dgvCategorias_CellClick;

        // colCategoriaId
        colCategoriaId.DataPropertyName = "CategoriaId";
        colCategoriaId.HeaderText = "CategoriaId";
        colCategoriaId.Name = "colCategoriaId";
        colCategoriaId.ReadOnly = true;
        colCategoriaId.Visible = false;

        // colSeccionId
        colSeccionId.DataPropertyName = "SeccionId";
        colSeccionId.HeaderText = "SeccionId";
        colSeccionId.Name = "colSeccionId";
        colSeccionId.ReadOnly = true;
        colSeccionId.Visible = false;

        // colNombreCategoria
        colNombreCategoria.DataPropertyName = "NombreCategoria";
        colNombreCategoria.HeaderText = "Nombre";
        colNombreCategoria.Name = "colNombreCategoria";
        colNombreCategoria.ReadOnly = true;

        // colActivo
        colActivo.DataPropertyName = "Activo";
        colActivo.HeaderText = "Activo";
        colActivo.Name = "colActivo";
        colActivo.ReadOnly = true;

        // FrmCategorias
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(680, 590);
        Controls.Add(dgvCategorias);
        Controls.Add(btnLimpiar);
        Controls.Add(btnActualizar);
        Controls.Add(btnGuardar);
        Controls.Add(chkActivo);
        Controls.Add(txtNombreCategoria);
        Controls.Add(lblNombreCategoria);
        Controls.Add(btnBuscar);
        Controls.Add(cmbSeccion);
        Controls.Add(lblSeccion);
        Controls.Add(lblTitulo);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "FrmCategorias";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Gestión de Categorías";
        Load += FrmCategorias_Load;
        ((System.ComponentModel.ISupportInitialize)dgvCategorias).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
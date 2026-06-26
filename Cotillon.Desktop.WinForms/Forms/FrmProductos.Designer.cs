namespace Cotillon.Desktop.WinForms.Forms;

partial class FrmProductos
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlHeader;
    private Label lblTitulo;
    private Label lblSubtitulo;

    private GroupBox grpDatos;
    private GroupBox grpListado;

    private Label lblSeccion;
    private ComboBox cmbSeccion;
    private Label lblCategoria;
    private ComboBox cmbCategoria;
    private Label lblNombre;
    private TextBox txtNombre;
    private Label lblPrecio;
    private TextBox txtPrecio;
    private Label lblStock;
    private TextBox txtStock;

    private Button btnGuardar;
    private Button btnRefrescar;
    private Button btnEliminar;
    private Button btnLimpiar;
    private CheckBox chkVerEliminados;

    private DataGridView dgvProductos;
    private DataGridViewTextBoxColumn colProductoId;
    private DataGridViewTextBoxColumn colNombreProducto;
    private DataGridViewTextBoxColumn colNombreSeccion;
    private DataGridViewTextBoxColumn colNombreCategoria;
    private DataGridViewTextBoxColumn colPrecio;
    private DataGridViewTextBoxColumn colStock;
    private DataGridViewCheckBoxColumn colActivo;

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
        lblSeccion = new Label();
        cmbSeccion = new ComboBox();
        lblCategoria = new Label();
        cmbCategoria = new ComboBox();
        lblNombre = new Label();
        txtNombre = new TextBox();
        lblPrecio = new Label();
        txtPrecio = new TextBox();
        lblStock = new Label();
        txtStock = new TextBox();
        btnGuardar = new Button();
        btnRefrescar = new Button();
        btnEliminar = new Button();
        btnLimpiar = new Button();
        chkVerEliminados = new CheckBox();

        grpListado = new GroupBox();
        dgvProductos = new DataGridView();
        colProductoId = new DataGridViewTextBoxColumn();
        colNombreProducto = new DataGridViewTextBoxColumn();
        colNombreSeccion = new DataGridViewTextBoxColumn();
        colNombreCategoria = new DataGridViewTextBoxColumn();
        colPrecio = new DataGridViewTextBoxColumn();
        colStock = new DataGridViewTextBoxColumn();
        colActivo = new DataGridViewCheckBoxColumn();

        pnlHeader.SuspendLayout();
        grpDatos.SuspendLayout();
        grpListado.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvProductos).BeginInit();
        SuspendLayout();

        pnlHeader.BackColor = Color.FromArgb(255, 77, 109);
        pnlHeader.Controls.Add(lblTitulo);
        pnlHeader.Controls.Add(lblSubtitulo);
        pnlHeader.Location = new Point(20, 18);
        pnlHeader.Name = "pnlHeader";
        pnlHeader.Size = new Size(940, 90);
        pnlHeader.TabIndex = 0;

        lblTitulo.AutoSize = true;
        lblTitulo.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        lblTitulo.ForeColor = Color.White;
        lblTitulo.Location = new Point(25, 12);
        lblTitulo.Name = "lblTitulo";
        lblTitulo.Size = new Size(255, 41);
        lblTitulo.TabIndex = 0;
        lblTitulo.Text = "📦 Productos";

        lblSubtitulo.AutoSize = true;
        lblSubtitulo.Font = new Font("Segoe UI", 11F);
        lblSubtitulo.ForeColor = Color.White;
        lblSubtitulo.Location = new Point(28, 58);
        lblSubtitulo.Name = "lblSubtitulo";
        lblSubtitulo.Size = new Size(402, 20);
        lblSubtitulo.TabIndex = 1;
        lblSubtitulo.Text = "Alta, consulta y control de stock del catálogo";

        grpDatos.BackColor = Color.White;
        grpDatos.Controls.Add(lblSeccion);
        grpDatos.Controls.Add(cmbSeccion);
        grpDatos.Controls.Add(lblCategoria);
        grpDatos.Controls.Add(cmbCategoria);
        grpDatos.Controls.Add(lblNombre);
        grpDatos.Controls.Add(txtNombre);
        grpDatos.Controls.Add(lblPrecio);
        grpDatos.Controls.Add(txtPrecio);
        grpDatos.Controls.Add(lblStock);
        grpDatos.Controls.Add(txtStock);
        grpDatos.Controls.Add(btnGuardar);
        grpDatos.Controls.Add(btnRefrescar);
        grpDatos.Controls.Add(btnEliminar);
        grpDatos.Controls.Add(btnLimpiar);
        grpDatos.Controls.Add(chkVerEliminados);
        grpDatos.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpDatos.Location = new Point(20, 125);
        grpDatos.Name = "grpDatos";
        grpDatos.Size = new Size(940, 205);
        grpDatos.TabIndex = 1;
        grpDatos.TabStop = false;
        grpDatos.Text = "Datos del producto";

        lblSeccion.AutoSize = true;
        lblSeccion.Location = new Point(25, 35);
        lblSeccion.Name = "lblSeccion";
        lblSeccion.Size = new Size(61, 19);
        lblSeccion.TabIndex = 0;
        lblSeccion.Text = "Sección";

        cmbSeccion.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbSeccion.FormattingEnabled = true;
        cmbSeccion.Location = new Point(25, 58);
        cmbSeccion.Name = "cmbSeccion";
        cmbSeccion.Size = new Size(260, 25);
        cmbSeccion.TabIndex = 1;
        cmbSeccion.SelectionChangeCommitted += cmbSeccion_SelectionChangeCommitted;

        lblCategoria.AutoSize = true;
        lblCategoria.Location = new Point(315, 35);
        lblCategoria.Name = "lblCategoria";
        lblCategoria.Size = new Size(76, 19);
        lblCategoria.TabIndex = 2;
        lblCategoria.Text = "Categoría";

        cmbCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbCategoria.FormattingEnabled = true;
        cmbCategoria.Location = new Point(315, 58);
        cmbCategoria.Name = "cmbCategoria";
        cmbCategoria.Size = new Size(260, 25);
        cmbCategoria.TabIndex = 3;

        lblNombre.AutoSize = true;
        lblNombre.Location = new Point(605, 35);
        lblNombre.Name = "lblNombre";
        lblNombre.Size = new Size(65, 19);
        lblNombre.TabIndex = 4;
        lblNombre.Text = "Nombre";

        txtNombre.Location = new Point(605, 58);
        txtNombre.Name = "txtNombre";
        txtNombre.Size = new Size(285, 25);
        txtNombre.TabIndex = 5;

        lblPrecio.AutoSize = true;
        lblPrecio.Location = new Point(25, 105);
        lblPrecio.Name = "lblPrecio";
        lblPrecio.Size = new Size(51, 19);
        lblPrecio.TabIndex = 6;
        lblPrecio.Text = "Precio";

        txtPrecio.Location = new Point(25, 128);
        txtPrecio.Name = "txtPrecio";
        txtPrecio.Size = new Size(150, 25);
        txtPrecio.TabIndex = 7;

        lblStock.AutoSize = true;
        lblStock.Location = new Point(205, 105);
        lblStock.Name = "lblStock";
        lblStock.Size = new Size(45, 19);
        lblStock.TabIndex = 8;
        lblStock.Text = "Stock";

        txtStock.Location = new Point(205, 128);
        txtStock.Name = "txtStock";
        txtStock.Size = new Size(150, 25);
        txtStock.TabIndex = 9;

        btnGuardar.Location = new Point(390, 122);
        btnGuardar.Name = "btnGuardar";
        btnGuardar.Size = new Size(135, 36);
        btnGuardar.TabIndex = 10;
        btnGuardar.Text = "Guardar";
        btnGuardar.UseVisualStyleBackColor = false;
        btnGuardar.Click += btnGuardar_Click;

        btnRefrescar.Location = new Point(540, 122);
        btnRefrescar.Name = "btnRefrescar";
        btnRefrescar.Size = new Size(115, 36);
        btnRefrescar.TabIndex = 11;
        btnRefrescar.Text = "Refrescar";
        btnRefrescar.UseVisualStyleBackColor = false;
        btnRefrescar.Click += btnRefrescar_Click;

        btnEliminar.Location = new Point(670, 122);
        btnEliminar.Name = "btnEliminar";
        btnEliminar.Size = new Size(115, 36);
        btnEliminar.TabIndex = 12;
        btnEliminar.Text = "Eliminar";
        btnEliminar.UseVisualStyleBackColor = false;
        btnEliminar.Click += btnEliminar_Click;

        btnLimpiar.Location = new Point(800, 122);
        btnLimpiar.Name = "btnLimpiar";
        btnLimpiar.Size = new Size(90, 36);
        btnLimpiar.TabIndex = 13;
        btnLimpiar.Text = "Limpiar";
        btnLimpiar.UseVisualStyleBackColor = false;
        btnLimpiar.Click += btnLimpiar_Click;

        chkVerEliminados.AutoSize = true;
        chkVerEliminados.Location = new Point(25, 170);
        chkVerEliminados.Name = "chkVerEliminados";
        chkVerEliminados.Size = new Size(128, 23);
        chkVerEliminados.TabIndex = 14;
        chkVerEliminados.Text = "Ver eliminados";
        chkVerEliminados.UseVisualStyleBackColor = true;
        chkVerEliminados.CheckedChanged += chkVerEliminados_CheckedChanged;

        grpListado.BackColor = Color.White;
        grpListado.Controls.Add(dgvProductos);
        grpListado.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpListado.Location = new Point(20, 350);
        grpListado.Name = "grpListado";
        grpListado.Size = new Size(940, 365);
        grpListado.TabIndex = 2;
        grpListado.TabStop = false;
        grpListado.Text = "Productos registrados";

        dgvProductos.AllowUserToAddRows = false;
        dgvProductos.AllowUserToDeleteRows = false;
        dgvProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvProductos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvProductos.Columns.AddRange(new DataGridViewColumn[] { colProductoId, colNombreProducto, colNombreSeccion, colNombreCategoria, colPrecio, colStock, colActivo });
        dgvProductos.Dock = DockStyle.Fill;
        dgvProductos.Location = new Point(3, 21);
        dgvProductos.MultiSelect = false;
        dgvProductos.Name = "dgvProductos";
        dgvProductos.ReadOnly = true;
        dgvProductos.RowHeadersVisible = false;
        dgvProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvProductos.Size = new Size(934, 341);
        dgvProductos.TabIndex = 0;
        dgvProductos.CellClick += dgvProductos_CellClick;

        colProductoId.DataPropertyName = "ProductoId";
        colProductoId.HeaderText = "ProductoId";
        colProductoId.Name = "colProductoId";
        colProductoId.ReadOnly = true;
        colProductoId.Visible = false;

        colNombreProducto.DataPropertyName = "NombreProducto";
        colNombreProducto.HeaderText = "Producto";
        colNombreProducto.Name = "colNombreProducto";
        colNombreProducto.ReadOnly = true;
        colNombreProducto.FillWeight = 180;

        colNombreSeccion.DataPropertyName = "NombreSeccion";
        colNombreSeccion.HeaderText = "Sección";
        colNombreSeccion.Name = "colNombreSeccion";
        colNombreSeccion.ReadOnly = true;
        colNombreSeccion.FillWeight = 130;

        colNombreCategoria.DataPropertyName = "NombreCategoria";
        colNombreCategoria.HeaderText = "Categoría";
        colNombreCategoria.Name = "colNombreCategoria";
        colNombreCategoria.ReadOnly = true;
        colNombreCategoria.FillWeight = 130;

        colPrecio.DataPropertyName = "Precio";
        colPrecio.HeaderText = "Precio";
        colPrecio.Name = "colPrecio";
        colPrecio.ReadOnly = true;
        colPrecio.DefaultCellStyle.Format = "C2";
        colPrecio.FillWeight = 90;

        colStock.DataPropertyName = "Stock";
        colStock.HeaderText = "Stock";
        colStock.Name = "colStock";
        colStock.ReadOnly = true;
        colStock.FillWeight = 80;

        colActivo.DataPropertyName = "Activo";
        colActivo.HeaderText = "Activo";
        colActivo.Name = "colActivo";
        colActivo.ReadOnly = true;
        colActivo.FillWeight = 70;

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
        Name = "FrmProductos";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "POP Cotillón - Productos";
        Load += FrmProductos_Load;

        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        grpDatos.ResumeLayout(false);
        grpDatos.PerformLayout();
        grpListado.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvProductos).EndInit();
        ResumeLayout(false);
    }
}

namespace Cotillon.Desktop.WinForms.Forms;

partial class FrmProductos
{
    private System.ComponentModel.IContainer components = null;

    private Label lblTitulo;
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
    private DataGridView dgvProductos;
    private DataGridViewTextBoxColumn colProductoId;
    private DataGridViewTextBoxColumn colNombreProducto;
    private DataGridViewTextBoxColumn colNombreSeccion;
    private DataGridViewTextBoxColumn colNombreCategoria;
    private DataGridViewTextBoxColumn colPrecio;
    private DataGridViewTextBoxColumn colStock;
    private DataGridViewCheckBoxColumn colActivo;
    private Button btnEliminar;
    private CheckBox chkVerEliminados;

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
        dgvProductos = new DataGridView();
        colProductoId = new DataGridViewTextBoxColumn();
        colNombreProducto = new DataGridViewTextBoxColumn();
        colNombreSeccion = new DataGridViewTextBoxColumn();
        colNombreCategoria = new DataGridViewTextBoxColumn();
        colPrecio = new DataGridViewTextBoxColumn();
        colStock = new DataGridViewTextBoxColumn();
        colActivo = new DataGridViewCheckBoxColumn();
        btnEliminar = new Button();
        chkVerEliminados = new CheckBox();
        ((System.ComponentModel.ISupportInitialize)dgvProductos).BeginInit();
        SuspendLayout();

        // lblTitulo
        lblTitulo.AutoSize = true;
        lblTitulo.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitulo.Location = new Point(25, 20);
        lblTitulo.Name = "lblTitulo";
        lblTitulo.Size = new Size(116, 30);
        lblTitulo.TabIndex = 0;
        lblTitulo.Text = "Productos";

        // lblSeccion
        lblSeccion.AutoSize = true;
        lblSeccion.Location = new Point(25, 70);
        lblSeccion.Name = "lblSeccion";
        lblSeccion.Size = new Size(48, 15);
        lblSeccion.TabIndex = 1;
        lblSeccion.Text = "Sección";

        // cmbSeccion
        cmbSeccion.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbSeccion.FormattingEnabled = true;
        cmbSeccion.Location = new Point(25, 90);
        cmbSeccion.Name = "cmbSeccion";
        cmbSeccion.Size = new Size(220, 23);
        cmbSeccion.TabIndex = 2;
        cmbSeccion.SelectionChangeCommitted += cmbSeccion_SelectionChangeCommitted;

        // lblCategoria
        lblCategoria.AutoSize = true;
        lblCategoria.Location = new Point(270, 70);
        lblCategoria.Name = "lblCategoria";
        lblCategoria.Size = new Size(58, 15);
        lblCategoria.TabIndex = 3;
        lblCategoria.Text = "Categoría";

        // cmbCategoria
        cmbCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbCategoria.FormattingEnabled = true;
        cmbCategoria.Location = new Point(270, 90);
        cmbCategoria.Name = "cmbCategoria";
        cmbCategoria.Size = new Size(220, 23);
        cmbCategoria.TabIndex = 4;

        // lblNombre
        lblNombre.AutoSize = true;
        lblNombre.Location = new Point(25, 130);
        lblNombre.Name = "lblNombre";
        lblNombre.Size = new Size(54, 15);
        lblNombre.TabIndex = 5;
        lblNombre.Text = "Nombre";

        // txtNombre
        txtNombre.Location = new Point(25, 150);
        txtNombre.Name = "txtNombre";
        txtNombre.Size = new Size(220, 23);
        txtNombre.TabIndex = 6;

        // lblPrecio
        lblPrecio.AutoSize = true;
        lblPrecio.Location = new Point(270, 130);
        lblPrecio.Name = "lblPrecio";
        lblPrecio.Size = new Size(40, 15);
        lblPrecio.TabIndex = 7;
        lblPrecio.Text = "Precio";

        // txtPrecio
        txtPrecio.Location = new Point(270, 150);
        txtPrecio.Name = "txtPrecio";
        txtPrecio.Size = new Size(100, 23);
        txtPrecio.TabIndex = 8;

        // lblStock
        lblStock.AutoSize = true;
        lblStock.Location = new Point(390, 130);
        lblStock.Name = "lblStock";
        lblStock.Size = new Size(36, 15);
        lblStock.TabIndex = 9;
        lblStock.Text = "Stock";

        // txtStock
        txtStock.Location = new Point(390, 150);
        txtStock.Name = "txtStock";
        txtStock.Size = new Size(100, 23);
        txtStock.TabIndex = 10;

        // btnGuardar
        btnGuardar.Location = new Point(25, 195);
        btnGuardar.Name = "btnGuardar";
        btnGuardar.Size = new Size(220, 35);
        btnGuardar.TabIndex = 11;
        btnGuardar.Text = "Agregar producto";
        btnGuardar.UseVisualStyleBackColor = true;
        btnGuardar.Click += btnGuardar_Click;

        // btnEliminar
        btnEliminar.Location = new Point(515, 195);
        btnEliminar.Name = "btnEliminar";
        btnEliminar.Size = new Size(220, 35);
        btnEliminar.TabIndex = 13;
        btnEliminar.Text = "Eliminar producto";
        btnEliminar.UseVisualStyleBackColor = true;
        btnEliminar.Click += btnEliminar_Click;

        // chkVerEliminados
        chkVerEliminados.AutoSize = true;
        chkVerEliminados.Location = new Point(760, 203);
        chkVerEliminados.Name = "chkVerEliminados";
        chkVerEliminados.Size = new Size(102, 19);
        chkVerEliminados.TabIndex = 14;
        chkVerEliminados.Text = "Ver eliminados";
        chkVerEliminados.UseVisualStyleBackColor = true;
        chkVerEliminados.CheckedChanged += chkVerEliminados_CheckedChanged;

        // btnRefrescar
        btnRefrescar.Location = new Point(270, 195);
        btnRefrescar.Name = "btnRefrescar";
        btnRefrescar.Size = new Size(220, 35);
        btnRefrescar.TabIndex = 12;
        btnRefrescar.Text = "Refrescar grilla";
        btnRefrescar.UseVisualStyleBackColor = true;
        btnRefrescar.Click += btnRefrescar_Click;

        // dgvProductos
        dgvProductos.AllowUserToAddRows = false;
        dgvProductos.AllowUserToDeleteRows = false;
        dgvProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvProductos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvProductos.Columns.AddRange(new DataGridViewColumn[] {
            colProductoId,
            colNombreProducto,
            colNombreSeccion,
            colNombreCategoria,
            colPrecio,
            colStock,
            colActivo
        });
        dgvProductos.Location = new Point(25, 255);
        dgvProductos.MultiSelect = false;
        dgvProductos.Name = "dgvProductos";
        dgvProductos.ReadOnly = true;
        dgvProductos.RowHeadersVisible = false;
        dgvProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvProductos.Size = new Size(900, 320);
        dgvProductos.TabIndex = 13;
        dgvProductos.CellClick += dgvProductos_CellClick;

        // colProductoId
        colProductoId.DataPropertyName = "ProductoId";
        colProductoId.HeaderText = "ProductoId";
        colProductoId.Name = "colProductoId";
        colProductoId.ReadOnly = true;
        colProductoId.Visible = false;

        // colNombreProducto
        colNombreProducto.DataPropertyName = "NombreProducto";
        colNombreProducto.HeaderText = "Producto";
        colNombreProducto.Name = "colNombreProducto";
        colNombreProducto.ReadOnly = true;

        // colNombreSeccion
        colNombreSeccion.DataPropertyName = "NombreSeccion";
        colNombreSeccion.HeaderText = "Sección";
        colNombreSeccion.Name = "colNombreSeccion";
        colNombreSeccion.ReadOnly = true;

        // colNombreCategoria
        colNombreCategoria.DataPropertyName = "NombreCategoria";
        colNombreCategoria.HeaderText = "Categoría";
        colNombreCategoria.Name = "colNombreCategoria";
        colNombreCategoria.ReadOnly = true;

        // colPrecio
        colPrecio.DataPropertyName = "Precio";
        colPrecio.HeaderText = "Precio";
        colPrecio.Name = "colPrecio";
        colPrecio.ReadOnly = true;

        // colStock
        colStock.DataPropertyName = "Stock";
        colStock.HeaderText = "Stock";
        colStock.Name = "colStock";
        colStock.ReadOnly = true;

        // colActivo
        colActivo.DataPropertyName = "Activo";
        colActivo.HeaderText = "Activo";
        colActivo.Name = "colActivo";
        colActivo.ReadOnly = true;

        // FrmProductos
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(960, 600);
        Controls.Add(dgvProductos);
        Controls.Add(btnRefrescar);
        Controls.Add(btnGuardar);
        Controls.Add(txtStock);
        Controls.Add(lblStock);
        Controls.Add(txtPrecio);
        Controls.Add(lblPrecio);
        Controls.Add(txtNombre);
        Controls.Add(lblNombre);
        Controls.Add(cmbCategoria);
        Controls.Add(lblCategoria);
        Controls.Add(cmbSeccion);
        Controls.Add(lblSeccion);
        Controls.Add(lblTitulo);
        Controls.Add(btnEliminar);
        Controls.Add(chkVerEliminados);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "FrmProductos";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Gestión de Productos";
        Load += FrmProductos_Load;
        ((System.ComponentModel.ISupportInitialize)dgvProductos).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
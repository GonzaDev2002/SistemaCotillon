namespace Cotillon.Desktop.WinForms.Forms;

partial class FrmCompras
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlHeader;
    private Label lblTitulo;
    private Label lblSubtitulo;

    private GroupBox grpProveedor;
    private Label lblProveedor;
    private ComboBox cmbProveedores;
    private Button btnNuevoProveedor;
    private Label lblNumeroComprobante;
    private TextBox txtNumeroComprobante;
    private Label lblObservaciones;
    private TextBox txtObservaciones;

    private GroupBox grpProductos;
    private Label lblBuscarProducto;
    private TextBox txtBuscarProducto;
    private ListBox lstProductos;

    private Panel pnlOperacion;
    private Label lblCantidad;
    private TextBox txtCantidad;
    private Label lblCostoUnitario;
    private TextBox txtCostoUnitario;
    private Label lblSubtotal;
    private Button btnAgregarProducto;
    private Button btnQuitarProducto;

    private GroupBox grpDetalle;
    private DataGridView dgvDetalle;

    private GroupBox grpCompras;
    private DataGridView dgvCompras;

    private Panel pnlTotal;
    private Label lblTotal;
    private Button btnRegistrarCompra;
    private Button btnCerrar;

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
        grpProveedor = new GroupBox();
        lblProveedor = new Label();
        cmbProveedores = new ComboBox();
        btnNuevoProveedor = new Button();
        lblNumeroComprobante = new Label();
        txtNumeroComprobante = new TextBox();
        lblObservaciones = new Label();
        txtObservaciones = new TextBox();
        grpProductos = new GroupBox();
        lblBuscarProducto = new Label();
        txtBuscarProducto = new TextBox();
        lstProductos = new ListBox();
        pnlOperacion = new Panel();
        lblCantidad = new Label();
        txtCantidad = new TextBox();
        lblCostoUnitario = new Label();
        txtCostoUnitario = new TextBox();
        lblSubtotal = new Label();
        btnAgregarProducto = new Button();
        btnQuitarProducto = new Button();
        grpDetalle = new GroupBox();
        dgvDetalle = new DataGridView();
        grpCompras = new GroupBox();
        dgvCompras = new DataGridView();
        pnlTotal = new Panel();
        lblTotal = new Label();
        btnRegistrarCompra = new Button();
        btnCerrar = new Button();
        pnlHeader.SuspendLayout();
        grpProveedor.SuspendLayout();
        grpProductos.SuspendLayout();
        pnlOperacion.SuspendLayout();
        grpDetalle.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvDetalle).BeginInit();
        grpCompras.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvCompras).BeginInit();
        pnlTotal.SuspendLayout();
        SuspendLayout();
        // 
        // pnlHeader
        // 
        pnlHeader.BackColor = Color.FromArgb(255, 77, 109);
        pnlHeader.Controls.Add(lblTitulo);
        pnlHeader.Controls.Add(lblSubtitulo);
        pnlHeader.Location = new Point(20, 18);
        pnlHeader.Name = "pnlHeader";
        pnlHeader.Size = new Size(1370, 90);
        pnlHeader.TabIndex = 0;
        // 
        // lblTitulo
        // 
        lblTitulo.AutoSize = true;
        lblTitulo.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        lblTitulo.ForeColor = Color.White;
        lblTitulo.Location = new Point(25, 12);
        lblTitulo.Name = "lblTitulo";
        lblTitulo.Size = new Size(240, 41);
        lblTitulo.TabIndex = 0;
        lblTitulo.Text = "🛍️ Compras";
        // 
        // lblSubtitulo
        // 
        lblSubtitulo.AutoSize = true;
        lblSubtitulo.Font = new Font("Segoe UI", 11F);
        lblSubtitulo.ForeColor = Color.White;
        lblSubtitulo.Location = new Point(28, 58);
        lblSubtitulo.Name = "lblSubtitulo";
        lblSubtitulo.Size = new Size(462, 20);
        lblSubtitulo.TabIndex = 1;
        lblSubtitulo.Text = "Ingreso de mercadería, proveedores y actualización de stock";
        // 
        // grpProveedor
        // 
        grpProveedor.BackColor = Color.White;
        grpProveedor.Controls.Add(lblProveedor);
        grpProveedor.Controls.Add(cmbProveedores);
        grpProveedor.Controls.Add(btnNuevoProveedor);
        grpProveedor.Controls.Add(lblNumeroComprobante);
        grpProveedor.Controls.Add(txtNumeroComprobante);
        grpProveedor.Controls.Add(lblObservaciones);
        grpProveedor.Controls.Add(txtObservaciones);
        grpProveedor.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpProveedor.Location = new Point(20, 120);
        grpProveedor.Name = "grpProveedor";
        grpProveedor.Size = new Size(900, 105);
        grpProveedor.TabIndex = 1;
        grpProveedor.TabStop = false;
        grpProveedor.Text = "Datos de la compra";
        // 
        // lblProveedor
        // 
        lblProveedor.AutoSize = true;
        lblProveedor.Location = new Point(18, 33);
        lblProveedor.Name = "lblProveedor";
        lblProveedor.Size = new Size(80, 19);
        lblProveedor.TabIndex = 0;
        lblProveedor.Text = "Proveedor:";
        // 
        // cmbProveedores
        // 
        cmbProveedores.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbProveedores.Font = new Font("Segoe UI", 10F);
        cmbProveedores.Location = new Point(110, 29);
        cmbProveedores.Name = "cmbProveedores";
        cmbProveedores.Size = new Size(260, 25);
        cmbProveedores.TabIndex = 1;
        // 
        // btnNuevoProveedor
        // 
        btnNuevoProveedor.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnNuevoProveedor.Location = new Point(390, 27);
        btnNuevoProveedor.Name = "btnNuevoProveedor";
        btnNuevoProveedor.Size = new Size(160, 32);
        btnNuevoProveedor.TabIndex = 2;
        btnNuevoProveedor.Text = "Nuevo proveedor";
        btnNuevoProveedor.UseVisualStyleBackColor = false;
        btnNuevoProveedor.Click += btnNuevoProveedor_Click;
        // 
        // lblNumeroComprobante
        // 
        lblNumeroComprobante.AutoSize = true;
        lblNumeroComprobante.Location = new Point(580, 33);
        lblNumeroComprobante.Name = "lblNumeroComprobante";
        lblNumeroComprobante.Size = new Size(103, 19);
        lblNumeroComprobante.TabIndex = 3;
        lblNumeroComprobante.Text = "Comprobante:";
        // 
        // txtNumeroComprobante
        // 
        txtNumeroComprobante.Font = new Font("Segoe UI", 10F);
        txtNumeroComprobante.Location = new Point(700, 29);
        txtNumeroComprobante.Name = "txtNumeroComprobante";
        txtNumeroComprobante.Size = new Size(165, 25);
        txtNumeroComprobante.TabIndex = 4;
        // 
        // lblObservaciones
        // 
        lblObservaciones.AutoSize = true;
        lblObservaciones.Location = new Point(18, 70);
        lblObservaciones.Name = "lblObservaciones";
        lblObservaciones.Size = new Size(111, 19);
        lblObservaciones.TabIndex = 5;
        lblObservaciones.Text = "Observaciones:";
        // 
        // txtObservaciones
        // 
        txtObservaciones.Font = new Font("Segoe UI", 10F);
        txtObservaciones.Location = new Point(140, 67);
        txtObservaciones.Name = "txtObservaciones";
        txtObservaciones.Size = new Size(725, 25);
        txtObservaciones.TabIndex = 6;
        // 
        // grpProductos
        // 
        grpProductos.BackColor = Color.White;
        grpProductos.Controls.Add(lblBuscarProducto);
        grpProductos.Controls.Add(txtBuscarProducto);
        grpProductos.Controls.Add(lstProductos);
        grpProductos.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpProductos.Location = new Point(20, 240);
        grpProductos.Name = "grpProductos";
        grpProductos.Size = new Size(350, 400);
        grpProductos.TabIndex = 2;
        grpProductos.TabStop = false;
        grpProductos.Text = "Buscar producto";
        // 
        // lblBuscarProducto
        // 
        lblBuscarProducto.AutoSize = true;
        lblBuscarProducto.Location = new Point(18, 32);
        lblBuscarProducto.Name = "lblBuscarProducto";
        lblBuscarProducto.Size = new Size(76, 19);
        lblBuscarProducto.TabIndex = 0;
        lblBuscarProducto.Text = "Producto:";
        // 
        // txtBuscarProducto
        // 
        txtBuscarProducto.Font = new Font("Segoe UI", 10F);
        txtBuscarProducto.Location = new Point(18, 58);
        txtBuscarProducto.Name = "txtBuscarProducto";
        txtBuscarProducto.Size = new Size(310, 25);
        txtBuscarProducto.TabIndex = 1;
        txtBuscarProducto.TextChanged += txtBuscarProducto_TextChanged;
        // 
        // lstProductos
        // 
        lstProductos.Font = new Font("Segoe UI", 10F);
        lstProductos.ItemHeight = 17;
        lstProductos.Location = new Point(18, 100);
        lstProductos.Name = "lstProductos";
        lstProductos.Size = new Size(310, 276);
        lstProductos.TabIndex = 2;
        lstProductos.SelectedIndexChanged += lstProductos_SelectedIndexChanged;
        // 
        // pnlOperacion
        // 
        pnlOperacion.BackColor = Color.White;
        pnlOperacion.BorderStyle = BorderStyle.FixedSingle;
        pnlOperacion.Controls.Add(lblCantidad);
        pnlOperacion.Controls.Add(txtCantidad);
        pnlOperacion.Controls.Add(lblCostoUnitario);
        pnlOperacion.Controls.Add(txtCostoUnitario);
        pnlOperacion.Controls.Add(lblSubtotal);
        pnlOperacion.Controls.Add(btnAgregarProducto);
        pnlOperacion.Controls.Add(btnQuitarProducto);
        pnlOperacion.Location = new Point(390, 240);
        pnlOperacion.Name = "pnlOperacion";
        pnlOperacion.Size = new Size(540, 120);
        pnlOperacion.TabIndex = 3;
        // 
        // lblCantidad
        // 
        lblCantidad.AutoSize = true;
        lblCantidad.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblCantidad.Location = new Point(20, 22);
        lblCantidad.Name = "lblCantidad";
        lblCantidad.Size = new Size(74, 19);
        lblCantidad.TabIndex = 0;
        lblCantidad.Text = "Cantidad:";
        // 
        // txtCantidad
        // 
        txtCantidad.Font = new Font("Segoe UI", 10F);
        txtCantidad.Location = new Point(100, 19);
        txtCantidad.Name = "txtCantidad";
        txtCantidad.Size = new Size(90, 25);
        txtCantidad.TabIndex = 1;
        txtCantidad.Text = "1";
        txtCantidad.TextChanged += txtCantidad_TextChanged;
        // 
        // lblCostoUnitario
        // 
        lblCostoUnitario.AutoSize = true;
        lblCostoUnitario.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblCostoUnitario.Location = new Point(220, 22);
        lblCostoUnitario.Name = "lblCostoUnitario";
        lblCostoUnitario.Size = new Size(105, 19);
        lblCostoUnitario.TabIndex = 2;
        lblCostoUnitario.Text = "Costo unitario:";
        // 
        // txtCostoUnitario
        // 
        txtCostoUnitario.Font = new Font("Segoe UI", 10F);
        txtCostoUnitario.Location = new Point(335, 19);
        txtCostoUnitario.Name = "txtCostoUnitario";
        txtCostoUnitario.Size = new Size(125, 25);
        txtCostoUnitario.TabIndex = 3;
        txtCostoUnitario.TextChanged += txtCostoUnitario_TextChanged;
        // 
        // lblSubtotal
        // 
        lblSubtotal.AutoSize = true;
        lblSubtotal.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblSubtotal.ForeColor = Color.FromArgb(79, 70, 229);
        lblSubtotal.Location = new Point(20, 56);
        lblSubtotal.Name = "lblSubtotal";
        lblSubtotal.Size = new Size(109, 19);
        lblSubtotal.TabIndex = 4;
        lblSubtotal.Text = "Subtotal: $0,00";
        // 
        // btnAgregarProducto
        // 
        btnAgregarProducto.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnAgregarProducto.Location = new Point(20, 82);
        btnAgregarProducto.Name = "btnAgregarProducto";
        btnAgregarProducto.Size = new Size(170, 30);
        btnAgregarProducto.TabIndex = 5;
        btnAgregarProducto.Text = "Agregar producto";
        btnAgregarProducto.UseVisualStyleBackColor = false;
        btnAgregarProducto.Click += btnAgregarProducto_Click;
        // 
        // btnQuitarProducto
        // 
        btnQuitarProducto.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnQuitarProducto.Location = new Point(210, 82);
        btnQuitarProducto.Name = "btnQuitarProducto";
        btnQuitarProducto.Size = new Size(170, 30);
        btnQuitarProducto.TabIndex = 6;
        btnQuitarProducto.Text = "Quitar producto";
        btnQuitarProducto.UseVisualStyleBackColor = false;
        btnQuitarProducto.Click += btnQuitarProducto_Click;
        // 
        // grpDetalle
        // 
        grpDetalle.BackColor = Color.White;
        grpDetalle.Controls.Add(dgvDetalle);
        grpDetalle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpDetalle.Location = new Point(390, 375);
        grpDetalle.Name = "grpDetalle";
        grpDetalle.Size = new Size(540, 265);
        grpDetalle.TabIndex = 4;
        grpDetalle.TabStop = false;
        grpDetalle.Text = "Detalle de compra";
        // 
        // dgvDetalle
        // 
        dgvDetalle.Dock = DockStyle.Fill;
        dgvDetalle.Location = new Point(3, 21);
        dgvDetalle.Name = "dgvDetalle";
        dgvDetalle.Size = new Size(534, 241);
        dgvDetalle.TabIndex = 0;
        // 
        // grpCompras
        // 
        grpCompras.BackColor = Color.White;
        grpCompras.Controls.Add(dgvCompras);
        grpCompras.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpCompras.Location = new Point(950, 120);
        grpCompras.Name = "grpCompras";
        grpCompras.Size = new Size(440, 520);
        grpCompras.TabIndex = 5;
        grpCompras.TabStop = false;
        grpCompras.Text = "Compras registradas";
        // 
        // dgvCompras
        // 
        dgvCompras.Dock = DockStyle.Fill;
        dgvCompras.Location = new Point(3, 21);
        dgvCompras.Name = "dgvCompras";
        dgvCompras.Size = new Size(434, 496);
        dgvCompras.TabIndex = 0;
        // 
        // pnlTotal
        // 
        pnlTotal.BackColor = Color.White;
        pnlTotal.BorderStyle = BorderStyle.FixedSingle;
        pnlTotal.Controls.Add(lblTotal);
        pnlTotal.Controls.Add(btnRegistrarCompra);
        pnlTotal.Controls.Add(btnCerrar);
        pnlTotal.Location = new Point(20, 655);
        pnlTotal.Name = "pnlTotal";
        pnlTotal.Size = new Size(1370, 85);
        pnlTotal.TabIndex = 6;
        // 
        // lblTotal
        // 
        lblTotal.AutoSize = true;
        lblTotal.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
        lblTotal.ForeColor = Color.FromArgb(255, 77, 109);
        lblTotal.Location = new Point(25, 18);
        lblTotal.Name = "lblTotal";
        lblTotal.Size = new Size(188, 45);
        lblTotal.TabIndex = 0;
        lblTotal.Text = "Total: $0,00";
        // 
        // btnRegistrarCompra
        // 
        btnRegistrarCompra.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnRegistrarCompra.Location = new Point(980, 22);
        btnRegistrarCompra.Name = "btnRegistrarCompra";
        btnRegistrarCompra.Size = new Size(170, 40);
        btnRegistrarCompra.TabIndex = 1;
        btnRegistrarCompra.Text = "Registrar compra";
        btnRegistrarCompra.UseVisualStyleBackColor = false;
        btnRegistrarCompra.Click += btnRegistrarCompra_Click;
        // 
        // btnCerrar
        // 
        btnCerrar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnCerrar.Location = new Point(1170, 22);
        btnCerrar.Name = "btnCerrar";
        btnCerrar.Size = new Size(150, 40);
        btnCerrar.TabIndex = 2;
        btnCerrar.Text = "Cerrar";
        btnCerrar.UseVisualStyleBackColor = false;
        btnCerrar.Click += btnCerrar_Click;
        // 
        // FrmCompras
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(250, 248, 242);
        BackgroundImage = Properties.Resources.fondoForms;
        BackgroundImageLayout = ImageLayout.Stretch;
        ClientSize = new Size(1415, 760);
        Controls.Add(pnlHeader);
        Controls.Add(grpProveedor);
        Controls.Add(grpProductos);
        Controls.Add(pnlOperacion);
        Controls.Add(grpDetalle);
        Controls.Add(grpCompras);
        Controls.Add(pnlTotal);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "FrmCompras";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "POP Cotillón - Compras";
        Load += FrmCompras_Load;
        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        grpProveedor.ResumeLayout(false);
        grpProveedor.PerformLayout();
        grpProductos.ResumeLayout(false);
        grpProductos.PerformLayout();
        pnlOperacion.ResumeLayout(false);
        pnlOperacion.PerformLayout();
        grpDetalle.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvDetalle).EndInit();
        grpCompras.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvCompras).EndInit();
        pnlTotal.ResumeLayout(false);
        pnlTotal.PerformLayout();
        ResumeLayout(false);
    }
}

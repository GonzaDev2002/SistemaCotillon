namespace Cotillon.Desktop.WinForms.Forms;

partial class FrmVentas
{
    private System.ComponentModel.IContainer components = null;

    private Label lblCliente;
    private ComboBox cmbClientes;
    private CheckBox chkVentaSinCliente;
    private Button btnNuevoCliente;

    private GroupBox grpPago;
    private Button btnEfectivo;
    private Button btnTransferencia;
    private Button btnTarjeta;

    private GroupBox grpProductos;
    private Label lblBuscarProducto;
    private TextBox txtBuscarProducto;
    private ListBox lstProductos;

    private Label lblCantidad;
    private TextBox txtCantidad;
    private Label lblPrecio;
    private TextBox txtPrecio;
    private Label lblSubtotal;

    private Button btnAgregarProducto;
    private Button btnQuitarProducto;
    private Button btnRegistrarVenta;
    private Button btnCerrar;

    private GroupBox grpDetalle;
    private DataGridView dgvDetalle;
    private DataGridViewTextBoxColumn colDetalleProducto;
    private DataGridViewTextBoxColumn colDetalleCantidad;
    private DataGridViewTextBoxColumn colDetallePrecio;
    private DataGridViewTextBoxColumn colDetalleSubtotal;

    private Label lblTotal;

    private GroupBox grpVentas;
    private DataGridView dgvVentas;
    private DataGridViewTextBoxColumn colVentaFecha;
    private DataGridViewTextBoxColumn colVentaCliente;
    private DataGridViewTextBoxColumn colVentaUsuario;
    private DataGridViewTextBoxColumn colVentaTotal;
    private DataGridViewTextBoxColumn colVentaEstado;

    private Panel pnlCliente;
    private Panel pnlOperacion;
    private Panel pnlTotal;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        lblCliente = new Label();
        cmbClientes = new ComboBox();
        chkVentaSinCliente = new CheckBox();
        btnNuevoCliente = new Button();
        grpPago = new GroupBox();
        btnEfectivo = new Button();
        btnTransferencia = new Button();
        btnTarjeta = new Button();
        grpProductos = new GroupBox();
        lblBuscarProducto = new Label();
        txtBuscarProducto = new TextBox();
        lstProductos = new ListBox();
        lblCantidad = new Label();
        txtCantidad = new TextBox();
        lblPrecio = new Label();
        txtPrecio = new TextBox();
        lblSubtotal = new Label();
        btnAgregarProducto = new Button();
        btnQuitarProducto = new Button();
        btnRegistrarVenta = new Button();
        btnCerrar = new Button();
        grpDetalle = new GroupBox();
        dgvDetalle = new DataGridView();
        grpVentas = new GroupBox();
        dgvVentas = new DataGridView();
        lblTotal = new Label();
        pnlCliente = new Panel();
        pnlOperacion = new Panel();
        pnlTotal = new Panel();
        grpPago.SuspendLayout();
        grpProductos.SuspendLayout();
        grpDetalle.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvDetalle).BeginInit();
        grpVentas.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvVentas).BeginInit();
        pnlCliente.SuspendLayout();
        pnlOperacion.SuspendLayout();
        pnlTotal.SuspendLayout();
        SuspendLayout();
        // 
        // pnlCliente
        // 
        pnlCliente.BackColor = Color.White;
        pnlCliente.BorderStyle = BorderStyle.FixedSingle;
        pnlCliente.Controls.Add(lblCliente);
        pnlCliente.Controls.Add(cmbClientes);
        pnlCliente.Controls.Add(chkVentaSinCliente);
        pnlCliente.Controls.Add(btnNuevoCliente);
        pnlCliente.Controls.Add(grpPago);
        pnlCliente.Location = new Point(20, 18);
        pnlCliente.Name = "pnlCliente";
        pnlCliente.Size = new Size(1370, 82);
        pnlCliente.TabIndex = 0;
        // 
        // lblCliente
        // 
        lblCliente.AutoSize = true;
        lblCliente.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblCliente.Location = new Point(20, 18);
        lblCliente.Name = "lblCliente";
        lblCliente.Size = new Size(59, 19);
        lblCliente.TabIndex = 0;
        lblCliente.Text = "Cliente:";
        // 
        // cmbClientes
        // 
        cmbClientes.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbClientes.Font = new Font("Segoe UI", 10F);
        cmbClientes.Location = new Point(90, 14);
        cmbClientes.Name = "cmbClientes";
        cmbClientes.Size = new Size(340, 25);
        cmbClientes.TabIndex = 1;
        // 
        // chkVentaSinCliente
        // 
        chkVentaSinCliente.AutoSize = true;
        chkVentaSinCliente.Checked = true;
        chkVentaSinCliente.CheckState = CheckState.Checked;
        chkVentaSinCliente.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        chkVentaSinCliente.Location = new Point(90, 47);
        chkVentaSinCliente.Name = "chkVentaSinCliente";
        chkVentaSinCliente.Size = new Size(122, 21);
        chkVentaSinCliente.TabIndex = 2;
        chkVentaSinCliente.Text = "Venta sin cliente";
        chkVentaSinCliente.CheckedChanged += chkVentaSinCliente_CheckedChanged;
        // 
        // btnNuevoCliente
        // 
        btnNuevoCliente.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnNuevoCliente.Location = new Point(450, 13);
        btnNuevoCliente.Name = "btnNuevoCliente";
        btnNuevoCliente.Size = new Size(145, 32);
        btnNuevoCliente.TabIndex = 3;
        btnNuevoCliente.Text = "Nuevo cliente";
        btnNuevoCliente.UseVisualStyleBackColor = false;
        btnNuevoCliente.Click += btnNuevoCliente_Click;
        // 
        // grpPago
        // 
        grpPago.Controls.Add(btnEfectivo);
        grpPago.Controls.Add(btnTransferencia);
        grpPago.Controls.Add(btnTarjeta);
        grpPago.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpPago.Location = new Point(640, 8);
        grpPago.Name = "grpPago";
        grpPago.Size = new Size(430, 60);
        grpPago.TabIndex = 4;
        grpPago.TabStop = false;
        grpPago.Text = "Medio de pago";
        // 
        // btnEfectivo
        // 
        btnEfectivo.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnEfectivo.Location = new Point(15, 22);
        btnEfectivo.Name = "btnEfectivo";
        btnEfectivo.Size = new Size(120, 28);
        btnEfectivo.TabIndex = 0;
        btnEfectivo.Text = "Efectivo";
        btnEfectivo.Click += btnEfectivo_Click;
        // 
        // btnTransferencia
        // 
        btnTransferencia.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnTransferencia.Location = new Point(150, 22);
        btnTransferencia.Name = "btnTransferencia";
        btnTransferencia.Size = new Size(135, 28);
        btnTransferencia.TabIndex = 1;
        btnTransferencia.Text = "Transferencia";
        btnTransferencia.Click += btnTransferencia_Click;
        // 
        // btnTarjeta
        // 
        btnTarjeta.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnTarjeta.Location = new Point(300, 22);
        btnTarjeta.Name = "btnTarjeta";
        btnTarjeta.Size = new Size(110, 28);
        btnTarjeta.TabIndex = 2;
        btnTarjeta.Text = "Tarjeta";
        btnTarjeta.Click += btnTarjeta_Click;
        // 
        // grpProductos
        // 
        grpProductos.BackColor = Color.White;
        grpProductos.Controls.Add(lblBuscarProducto);
        grpProductos.Controls.Add(txtBuscarProducto);
        grpProductos.Controls.Add(lstProductos);
        grpProductos.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpProductos.Location = new Point(20, 115);
        grpProductos.Name = "grpProductos";
        grpProductos.Size = new Size(350, 400);
        grpProductos.TabIndex = 1;
        grpProductos.TabStop = false;
        grpProductos.Text = "Buscar producto";
        // 
        // lblBuscarProducto
        // 
        lblBuscarProducto.AutoSize = true;
        lblBuscarProducto.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
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
        pnlOperacion.Controls.Add(lblPrecio);
        pnlOperacion.Controls.Add(txtPrecio);
        pnlOperacion.Controls.Add(lblSubtotal);
        pnlOperacion.Controls.Add(btnAgregarProducto);
        pnlOperacion.Controls.Add(btnQuitarProducto);
        pnlOperacion.Location = new Point(390, 115);
        pnlOperacion.Name = "pnlOperacion";
        pnlOperacion.Size = new Size(540, 120);
        pnlOperacion.TabIndex = 2;
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
        // lblPrecio
        // 
        lblPrecio.AutoSize = true;
        lblPrecio.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblPrecio.Location = new Point(220, 22);
        lblPrecio.Name = "lblPrecio";
        lblPrecio.Size = new Size(54, 19);
        lblPrecio.TabIndex = 2;
        lblPrecio.Text = "Precio:";
        // 
        // txtPrecio
        // 
        txtPrecio.Font = new Font("Segoe UI", 10F);
        txtPrecio.Location = new Point(280, 19);
        txtPrecio.Name = "txtPrecio";
        txtPrecio.ReadOnly = true;
        txtPrecio.Size = new Size(125, 25);
        txtPrecio.TabIndex = 3;
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
        btnQuitarProducto.Click += btnQuitarProducto_Click;
        // 
        // grpDetalle
        // 
        grpDetalle.BackColor = Color.White;
        grpDetalle.Controls.Add(dgvDetalle);
        grpDetalle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpDetalle.Location = new Point(390, 250);
        grpDetalle.Name = "grpDetalle";
        grpDetalle.Size = new Size(540, 265);
        grpDetalle.TabIndex = 3;
        grpDetalle.TabStop = false;
        grpDetalle.Text = "Detalle de venta";
        // 
        // dgvDetalle
        // 
        dgvDetalle.AllowUserToAddRows = false;
        dgvDetalle.AllowUserToDeleteRows = false;
        dgvDetalle.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvDetalle.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvDetalle.Dock = DockStyle.Fill;
        dgvDetalle.Location = new Point(3, 21);
        dgvDetalle.Name = "dgvDetalle";
        dgvDetalle.ReadOnly = true;
        dgvDetalle.RowHeadersVisible = false;
        dgvDetalle.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvDetalle.Size = new Size(534, 241);
        dgvDetalle.TabIndex = 0;
        // 
        // grpVentas
        // 
        grpVentas.BackColor = Color.White;
        grpVentas.Controls.Add(dgvVentas);
        grpVentas.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpVentas.Location = new Point(950, 115);
        grpVentas.Name = "grpVentas";
        grpVentas.Size = new Size(440, 400);
        grpVentas.TabIndex = 4;
        grpVentas.TabStop = false;
        grpVentas.Text = "Ventas registradas";
        // 
        // dgvVentas
        // 
        dgvVentas.AllowUserToAddRows = false;
        dgvVentas.AllowUserToDeleteRows = false;
        dgvVentas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvVentas.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvVentas.Dock = DockStyle.Fill;
        dgvVentas.Location = new Point(3, 21);
        dgvVentas.Name = "dgvVentas";
        dgvVentas.ReadOnly = true;
        dgvVentas.RowHeadersVisible = false;
        dgvVentas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvVentas.Size = new Size(434, 376);
        dgvVentas.TabIndex = 0;
        // 
        // pnlTotal
        // 
        pnlTotal.BackColor = Color.White;
        pnlTotal.BorderStyle = BorderStyle.FixedSingle;
        pnlTotal.Controls.Add(lblTotal);
        pnlTotal.Controls.Add(btnRegistrarVenta);
        pnlTotal.Controls.Add(btnCerrar);
        pnlTotal.Location = new Point(20, 530);
        pnlTotal.Name = "pnlTotal";
        pnlTotal.Size = new Size(1370, 85);
        pnlTotal.TabIndex = 5;
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
        // btnRegistrarVenta
        // 
        btnRegistrarVenta.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnRegistrarVenta.Location = new Point(980, 22);
        btnRegistrarVenta.Name = "btnRegistrarVenta";
        btnRegistrarVenta.Size = new Size(170, 40);
        btnRegistrarVenta.TabIndex = 1;
        btnRegistrarVenta.Text = "Registrar venta";
        btnRegistrarVenta.Click += btnRegistrarVenta_Click;
        // 
        // btnCerrar
        // 
        btnCerrar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnCerrar.Location = new Point(1170, 22);
        btnCerrar.Name = "btnCerrar";
        btnCerrar.Size = new Size(150, 40);
        btnCerrar.TabIndex = 2;
        btnCerrar.Text = "Cerrar";
        btnCerrar.Click += btnCerrar_Click;
        // 
        // FrmVentas
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackgroundImage = Properties.Resources.fondoForms;
        BackgroundImageLayout = ImageLayout.Stretch;
        ClientSize = new Size(1415, 635);
        Controls.Add(pnlCliente);
        Controls.Add(grpProductos);
        Controls.Add(pnlOperacion);
        Controls.Add(grpDetalle);
        Controls.Add(grpVentas);
        Controls.Add(pnlTotal);
        Name = "FrmVentas";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "POP Cotillón - Ventas";
        Load += FrmVentas_Load;
        grpPago.ResumeLayout(false);
        grpProductos.ResumeLayout(false);
        grpProductos.PerformLayout();
        grpDetalle.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvDetalle).EndInit();
        grpVentas.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvVentas).EndInit();
        pnlCliente.ResumeLayout(false);
        pnlCliente.PerformLayout();
        pnlOperacion.ResumeLayout(false);
        pnlOperacion.PerformLayout();
        pnlTotal.ResumeLayout(false);
        pnlTotal.PerformLayout();
        ResumeLayout(false);
    }
}

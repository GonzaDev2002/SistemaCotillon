namespace Cotillon.Desktop.WinForms.Forms;

partial class FrmMain
{
    private System.ComponentModel.IContainer components = null;

    private MenuStrip menuPrincipal;
    private ToolStripMenuItem sistemaToolStripMenuItem;
    private ToolStripMenuItem salirToolStripMenuItem;
    private ToolStripMenuItem catalogoToolStripMenuItem;
    private ToolStripMenuItem seccionesToolStripMenuItem;
    private ToolStripMenuItem categoriasToolStripMenuItem;
    private ToolStripMenuItem productosToolStripMenuItem;
    private ToolStripMenuItem operacionesToolStripMenuItem;
    private ToolStripMenuItem ventasToolStripMenuItem;
    private ToolStripMenuItem comprasToolStripMenuItem;
    private ToolStripMenuItem gestionToolStripMenuItem;
    private ToolStripMenuItem clientesToolStripMenuItem;
    private ToolStripMenuItem controlToolStripMenuItem;
    private ToolStripMenuItem auditoriaToolStripMenuItem;
    private ToolStripMenuItem reportesToolStripMenuItem;

    private Panel pnlHeader;
    private Label lblTitulo;
    private Label lblUsuario;
    private Label lblBienvenida;
    private Button btnRecargarDashboard;

    private Panel pnlKpi1;
    private Panel pnlKpi2;
    private Panel pnlKpi3;
    private Panel pnlKpi4;

    private Label lblKpiTotalProductosTitulo;
    private Label lblKpiTotalProductosValor;
    private Label lblKpiActivosTitulo;
    private Label lblKpiActivosValor;
    private Label lblKpiInactivosTitulo;
    private Label lblKpiInactivosValor;
    private Label lblKpiBajoStockTitulo;
    private Label lblKpiBajoStockValor;

    private GroupBox grpBajoStock;
    private GroupBox grpUltimosProductos;
    private GroupBox grpAccesosRapidos;

    private DataGridView dgvBajoStock;
    private DataGridView dgvUltimosProductos;

    private DataGridViewTextBoxColumn colBsProducto;
    private DataGridViewTextBoxColumn colBsCategoria;
    private DataGridViewTextBoxColumn colBsStock;
    private DataGridViewTextBoxColumn colBsStockMinimo;

    private DataGridViewTextBoxColumn colUpProducto;
    private DataGridViewTextBoxColumn colUpCategoria;
    private DataGridViewTextBoxColumn colUpFechaAlta;
    private DataGridViewTextBoxColumn colUpPrecio;

    private Button btnAccesoProductos;
    private Button btnAccesoCategorias;
    private Button btnAccesoSecciones;
    private Button btnAccesoReportes;

    private Panel pnlDerecho;
    private GroupBox grpAlertas;
    private GroupBox grpResumenRapido;
    private ListBox lstAlertas;
    private Label lblResumen1;
    private Label lblResumen2;
    private Label lblResumen3;
    private Label lblResumen4;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
        DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();

        menuPrincipal = new MenuStrip();
        sistemaToolStripMenuItem = new ToolStripMenuItem();
        salirToolStripMenuItem = new ToolStripMenuItem();
        catalogoToolStripMenuItem = new ToolStripMenuItem();
        seccionesToolStripMenuItem = new ToolStripMenuItem();
        categoriasToolStripMenuItem = new ToolStripMenuItem();
        productosToolStripMenuItem = new ToolStripMenuItem();
        operacionesToolStripMenuItem = new ToolStripMenuItem();
        ventasToolStripMenuItem = new ToolStripMenuItem();
        comprasToolStripMenuItem = new ToolStripMenuItem();
        gestionToolStripMenuItem = new ToolStripMenuItem();
        clientesToolStripMenuItem = new ToolStripMenuItem();
        controlToolStripMenuItem = new ToolStripMenuItem();
        auditoriaToolStripMenuItem = new ToolStripMenuItem();
        reportesToolStripMenuItem = new ToolStripMenuItem();

        pnlHeader = new Panel();
        lblTitulo = new Label();
        lblBienvenida = new Label();
        lblUsuario = new Label();
        btnRecargarDashboard = new Button();

        pnlKpi1 = new Panel();
        lblKpiTotalProductosTitulo = new Label();
        lblKpiTotalProductosValor = new Label();

        pnlKpi2 = new Panel();
        lblKpiActivosTitulo = new Label();
        lblKpiActivosValor = new Label();

        pnlKpi3 = new Panel();
        lblKpiInactivosTitulo = new Label();
        lblKpiInactivosValor = new Label();

        pnlKpi4 = new Panel();
        lblKpiBajoStockTitulo = new Label();
        lblKpiBajoStockValor = new Label();

        grpBajoStock = new GroupBox();
        dgvBajoStock = new DataGridView();
        colBsProducto = new DataGridViewTextBoxColumn();
        colBsCategoria = new DataGridViewTextBoxColumn();
        colBsStock = new DataGridViewTextBoxColumn();
        colBsStockMinimo = new DataGridViewTextBoxColumn();

        grpUltimosProductos = new GroupBox();
        dgvUltimosProductos = new DataGridView();
        colUpProducto = new DataGridViewTextBoxColumn();
        colUpCategoria = new DataGridViewTextBoxColumn();
        colUpFechaAlta = new DataGridViewTextBoxColumn();
        colUpPrecio = new DataGridViewTextBoxColumn();

        grpAccesosRapidos = new GroupBox();
        btnAccesoProductos = new Button();
        btnAccesoCategorias = new Button();
        btnAccesoSecciones = new Button();
        btnAccesoReportes = new Button();

        pnlDerecho = new Panel();
        grpAlertas = new GroupBox();
        lstAlertas = new ListBox();
        grpResumenRapido = new GroupBox();
        lblResumen1 = new Label();
        lblResumen2 = new Label();
        lblResumen3 = new Label();
        lblResumen4 = new Label();

        menuPrincipal.SuspendLayout();
        pnlHeader.SuspendLayout();
        pnlKpi1.SuspendLayout();
        pnlKpi2.SuspendLayout();
        pnlKpi3.SuspendLayout();
        pnlKpi4.SuspendLayout();
        grpBajoStock.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvBajoStock).BeginInit();
        grpUltimosProductos.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvUltimosProductos).BeginInit();
        grpAccesosRapidos.SuspendLayout();
        pnlDerecho.SuspendLayout();
        grpAlertas.SuspendLayout();
        grpResumenRapido.SuspendLayout();
        SuspendLayout();

        // menuPrincipal
        menuPrincipal.Items.AddRange(new ToolStripItem[]
        {
            sistemaToolStripMenuItem,
            catalogoToolStripMenuItem,
            operacionesToolStripMenuItem,
            gestionToolStripMenuItem,
            controlToolStripMenuItem,
            reportesToolStripMenuItem
        });
        menuPrincipal.Location = new Point(0, 0);
        menuPrincipal.Name = "menuPrincipal";
        menuPrincipal.Size = new Size(1440, 24);
        menuPrincipal.TabIndex = 0;

        // sistema
        sistemaToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { salirToolStripMenuItem });
        sistemaToolStripMenuItem.Name = "sistemaToolStripMenuItem";
        sistemaToolStripMenuItem.Size = new Size(60, 20);
        sistemaToolStripMenuItem.Text = "Sistema";

        salirToolStripMenuItem.Name = "salirToolStripMenuItem";
        salirToolStripMenuItem.Size = new Size(96, 22);
        salirToolStripMenuItem.Text = "Salir";
        salirToolStripMenuItem.Click += salirToolStripMenuItem_Click;

        // catalogo
        catalogoToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
        {
            seccionesToolStripMenuItem,
            categoriasToolStripMenuItem,
            productosToolStripMenuItem
        });
        catalogoToolStripMenuItem.Name = "catalogoToolStripMenuItem";
        catalogoToolStripMenuItem.Size = new Size(67, 20);
        catalogoToolStripMenuItem.Text = "Catálogo";

        seccionesToolStripMenuItem.Name = "seccionesToolStripMenuItem";
        seccionesToolStripMenuItem.Size = new Size(130, 22);
        seccionesToolStripMenuItem.Text = "Secciones";
        seccionesToolStripMenuItem.Click += seccionesToolStripMenuItem_Click;

        categoriasToolStripMenuItem.Name = "categoriasToolStripMenuItem";
        categoriasToolStripMenuItem.Size = new Size(130, 22);
        categoriasToolStripMenuItem.Text = "Categorías";
        categoriasToolStripMenuItem.Click += categoriasToolStripMenuItem_Click;

        productosToolStripMenuItem.Name = "productosToolStripMenuItem";
        productosToolStripMenuItem.Size = new Size(130, 22);
        productosToolStripMenuItem.Text = "Productos";
        productosToolStripMenuItem.Click += productosToolStripMenuItem_Click;

        // operaciones
        operacionesToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
        {
            ventasToolStripMenuItem,
            comprasToolStripMenuItem
        });
        operacionesToolStripMenuItem.Name = "operacionesToolStripMenuItem";
        operacionesToolStripMenuItem.Size = new Size(85, 20);
        operacionesToolStripMenuItem.Text = "Operaciones";

        ventasToolStripMenuItem.Name = "ventasToolStripMenuItem";
        ventasToolStripMenuItem.Size = new Size(122, 22);
        ventasToolStripMenuItem.Text = "Ventas";
        ventasToolStripMenuItem.Click += ventasToolStripMenuItem_Click;

        comprasToolStripMenuItem.Name = "comprasToolStripMenuItem";
        comprasToolStripMenuItem.Size = new Size(122, 22);
        comprasToolStripMenuItem.Text = "Compras";
        comprasToolStripMenuItem.Click += comprasToolStripMenuItem_Click;

        // gestion
        gestionToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { clientesToolStripMenuItem });
        gestionToolStripMenuItem.Name = "gestionToolStripMenuItem";
        gestionToolStripMenuItem.Size = new Size(59, 20);
        gestionToolStripMenuItem.Text = "Gestión";

        clientesToolStripMenuItem.Name = "clientesToolStripMenuItem";
        clientesToolStripMenuItem.Size = new Size(116, 22);
        clientesToolStripMenuItem.Text = "Clientes";
        clientesToolStripMenuItem.Click += clientesToolStripMenuItem_Click;

        // control
        controlToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { auditoriaToolStripMenuItem });
        controlToolStripMenuItem.Name = "controlToolStripMenuItem";
        controlToolStripMenuItem.Size = new Size(59, 20);
        controlToolStripMenuItem.Text = "Control";

        auditoriaToolStripMenuItem.Name = "auditoriaToolStripMenuItem";
        auditoriaToolStripMenuItem.Size = new Size(123, 22);
        auditoriaToolStripMenuItem.Text = "Auditoría";
        auditoriaToolStripMenuItem.Click += auditoriaToolStripMenuItem_Click;

        // reportes
        reportesToolStripMenuItem.Name = "reportesToolStripMenuItem";
        reportesToolStripMenuItem.Size = new Size(68, 20);
        reportesToolStripMenuItem.Text = "Reportes";
        reportesToolStripMenuItem.Click += reportesToolStripMenuItem_Click;

        // pnlHeader
        pnlHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pnlHeader.BackColor = Color.White;
        pnlHeader.BorderStyle = BorderStyle.FixedSingle;
        pnlHeader.Controls.Add(lblTitulo);
        pnlHeader.Controls.Add(lblBienvenida);
        pnlHeader.Controls.Add(lblUsuario);
        pnlHeader.Controls.Add(btnRecargarDashboard);
        pnlHeader.Location = new Point(20, 40);
        pnlHeader.Name = "pnlHeader";
        pnlHeader.Size = new Size(980, 90);
        pnlHeader.TabIndex = 1;

        lblTitulo.AutoSize = true;
        lblTitulo.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        lblTitulo.Location = new Point(20, 10);
        lblTitulo.Name = "lblTitulo";
        lblTitulo.Size = new Size(197, 41);
        lblTitulo.TabIndex = 0;
        lblTitulo.Text = "POP Cotillón";

        lblBienvenida.AutoSize = true;
        lblBienvenida.Font = new Font("Segoe UI", 11F);
        lblBienvenida.Location = new Point(24, 55);
        lblBienvenida.Name = "lblBienvenida";
        lblBienvenida.Size = new Size(83, 20);
        lblBienvenida.TabIndex = 1;
        lblBienvenida.Text = "Bienvenido";

        lblUsuario.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblUsuario.AutoSize = true;
        lblUsuario.Font = new Font("Segoe UI", 10F);
        lblUsuario.Location = new Point(690, 20);
        lblUsuario.Name = "lblUsuario";
        lblUsuario.Size = new Size(59, 19);
        lblUsuario.TabIndex = 2;
        lblUsuario.Text = "Usuario:";

        btnRecargarDashboard.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnRecargarDashboard.Location = new Point(760, 48);
        btnRecargarDashboard.Name = "btnRecargarDashboard";
        btnRecargarDashboard.Size = new Size(180, 30);
        btnRecargarDashboard.TabIndex = 3;
        btnRecargarDashboard.Text = "Recargar dashboard";
        btnRecargarDashboard.UseVisualStyleBackColor = true;
        btnRecargarDashboard.Click += btnRecargarDashboard_Click;

        // KPI 1
        pnlKpi1.Controls.Add(lblKpiTotalProductosTitulo);
        pnlKpi1.Controls.Add(lblKpiTotalProductosValor);
        pnlKpi1.Location = new Point(20, 150);
        pnlKpi1.Name = "pnlKpi1";
        pnlKpi1.Size = new Size(230, 110);
        pnlKpi1.TabIndex = 2;

        lblKpiTotalProductosTitulo.AutoSize = true;
        lblKpiTotalProductosTitulo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblKpiTotalProductosTitulo.Location = new Point(20, 18);
        lblKpiTotalProductosTitulo.Name = "lblKpiTotalProductosTitulo";
        lblKpiTotalProductosTitulo.Size = new Size(114, 19);
        lblKpiTotalProductosTitulo.Text = "Total productos";

        lblKpiTotalProductosValor.AutoSize = true;
        lblKpiTotalProductosValor.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
        lblKpiTotalProductosValor.Location = new Point(20, 50);
        lblKpiTotalProductosValor.Name = "lblKpiTotalProductosValor";
        lblKpiTotalProductosValor.Size = new Size(38, 45);
        lblKpiTotalProductosValor.Text = "0";

        // KPI 2
        pnlKpi2.Controls.Add(lblKpiActivosTitulo);
        pnlKpi2.Controls.Add(lblKpiActivosValor);
        pnlKpi2.Location = new Point(270, 150);
        pnlKpi2.Name = "pnlKpi2";
        pnlKpi2.Size = new Size(230, 110);
        pnlKpi2.TabIndex = 3;

        lblKpiActivosTitulo.AutoSize = true;
        lblKpiActivosTitulo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblKpiActivosTitulo.Location = new Point(20, 18);
        lblKpiActivosTitulo.Name = "lblKpiActivosTitulo";
        lblKpiActivosTitulo.Size = new Size(128, 19);
        lblKpiActivosTitulo.Text = "Productos activos";

        lblKpiActivosValor.AutoSize = true;
        lblKpiActivosValor.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
        lblKpiActivosValor.Location = new Point(20, 50);
        lblKpiActivosValor.Name = "lblKpiActivosValor";
        lblKpiActivosValor.Size = new Size(38, 45);
        lblKpiActivosValor.Text = "0";

        // KPI 3
        pnlKpi3.Controls.Add(lblKpiInactivosTitulo);
        pnlKpi3.Controls.Add(lblKpiInactivosValor);
        pnlKpi3.Location = new Point(520, 150);
        pnlKpi3.Name = "pnlKpi3";
        pnlKpi3.Size = new Size(230, 110);
        pnlKpi3.TabIndex = 4;

        lblKpiInactivosTitulo.AutoSize = true;
        lblKpiInactivosTitulo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblKpiInactivosTitulo.Location = new Point(20, 18);
        lblKpiInactivosTitulo.Name = "lblKpiInactivosTitulo";
        lblKpiInactivosTitulo.Size = new Size(140, 19);
        lblKpiInactivosTitulo.Text = "Productos inactivos";

        lblKpiInactivosValor.AutoSize = true;
        lblKpiInactivosValor.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
        lblKpiInactivosValor.Location = new Point(20, 50);
        lblKpiInactivosValor.Name = "lblKpiInactivosValor";
        lblKpiInactivosValor.Size = new Size(38, 45);
        lblKpiInactivosValor.Text = "0";

        // KPI 4
        pnlKpi4.Controls.Add(lblKpiBajoStockTitulo);
        pnlKpi4.Controls.Add(lblKpiBajoStockValor);
        pnlKpi4.Location = new Point(770, 150);
        pnlKpi4.Name = "pnlKpi4";
        pnlKpi4.Size = new Size(230, 110);
        pnlKpi4.TabIndex = 5;

        lblKpiBajoStockTitulo.AutoSize = true;
        lblKpiBajoStockTitulo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblKpiBajoStockTitulo.Location = new Point(20, 18);
        lblKpiBajoStockTitulo.Name = "lblKpiBajoStockTitulo";
        lblKpiBajoStockTitulo.Size = new Size(78, 19);
        lblKpiBajoStockTitulo.Text = "Bajo stock";

        lblKpiBajoStockValor.AutoSize = true;
        lblKpiBajoStockValor.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
        lblKpiBajoStockValor.Location = new Point(20, 50);
        lblKpiBajoStockValor.Name = "lblKpiBajoStockValor";
        lblKpiBajoStockValor.Size = new Size(38, 45);
        lblKpiBajoStockValor.Text = "0";

        // Bajo stock
        grpBajoStock.Controls.Add(dgvBajoStock);
        grpBajoStock.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpBajoStock.Location = new Point(20, 290);
        grpBajoStock.Name = "grpBajoStock";
        grpBajoStock.Size = new Size(490, 330);
        grpBajoStock.TabIndex = 6;
        grpBajoStock.TabStop = false;
        grpBajoStock.Text = "Productos con bajo stock";

        dgvBajoStock.AllowUserToAddRows = false;
        dgvBajoStock.AllowUserToDeleteRows = false;
        dgvBajoStock.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        dgvBajoStock.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        dgvBajoStock.ScrollBars = ScrollBars.Both;
        dgvBajoStock.RowHeadersVisible = false;
        dgvBajoStock.ShowCellToolTips = true;
        dgvBajoStock.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvBajoStock.Columns.AddRange(new DataGridViewColumn[]
        {
            colBsProducto,
            colBsCategoria,
            colBsStock,
            colBsStockMinimo
        });
        dgvBajoStock.Dock = DockStyle.Fill;
        dgvBajoStock.Name = "dgvBajoStock";
        dgvBajoStock.ReadOnly = true;
        dgvBajoStock.RowHeadersVisible = false;
        dgvBajoStock.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        colBsProducto.DataPropertyName = "NombreProducto";
        colBsProducto.HeaderText = "Producto";
        colBsProducto.Name = "colBsProducto";
        colBsProducto.ReadOnly = true;

        colBsCategoria.DataPropertyName = "NombreCategoria";
        colBsCategoria.HeaderText = "Categoría";
        colBsCategoria.Name = "colBsCategoria";
        colBsCategoria.ReadOnly = true;

        colBsStock.DataPropertyName = "Stock";
        colBsStock.HeaderText = "Stock";
        colBsStock.Name = "colBsStock";
        colBsStock.ReadOnly = true;

        colBsStockMinimo.DataPropertyName = "StockMinimo";
        colBsStockMinimo.HeaderText = "Stock mínimo";
        colBsStockMinimo.Name = "colBsStockMinimo";
        colBsStockMinimo.ReadOnly = true;

        // Últimos productos
        grpUltimosProductos.Controls.Add(dgvUltimosProductos);
        grpUltimosProductos.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpUltimosProductos.Location = new Point(530, 290);
        grpUltimosProductos.Name = "grpUltimosProductos";
        grpUltimosProductos.Size = new Size(490, 330);
        grpUltimosProductos.TabIndex = 7;
        grpUltimosProductos.TabStop = false;
        grpUltimosProductos.Text = "Últimos productos agregados";

        dgvUltimosProductos.AllowUserToAddRows = false;
        dgvUltimosProductos.AllowUserToDeleteRows = false;
        dgvUltimosProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        dgvUltimosProductos.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        dgvUltimosProductos.ScrollBars = ScrollBars.Both;
        dgvUltimosProductos.ShowCellToolTips = true;
        dgvUltimosProductos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvUltimosProductos.Columns.AddRange(new DataGridViewColumn[]
        {
    colUpProducto,
    colUpCategoria,
    colUpFechaAlta,
    colUpPrecio
        });
        dgvUltimosProductos.Dock = DockStyle.Fill;
        dgvUltimosProductos.Name = "dgvUltimosProductos";
        dgvUltimosProductos.ReadOnly = true;
        dgvUltimosProductos.RowHeadersVisible = false;
        dgvUltimosProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        colUpProducto.DataPropertyName = "NombreProducto";
        colUpProducto.HeaderText = "Producto";
        colUpProducto.Name = "colUpProducto";
        colUpProducto.Width = 180;
        colUpProducto.ReadOnly = true;

        colUpCategoria.DataPropertyName = "NombreCategoria";
        colUpCategoria.HeaderText = "Categoría";
        colUpCategoria.Name = "colUpCategoria";
        colUpCategoria.Width = 130;
        colUpCategoria.ReadOnly = true;

        colUpFechaAlta.DataPropertyName = "FechaAlta";
        dataGridViewCellStyle1.Format = "dd/MM/yyyy";
        colUpFechaAlta.DefaultCellStyle = dataGridViewCellStyle1;
        colUpFechaAlta.HeaderText = "Fecha alta";
        colUpFechaAlta.Name = "colUpFechaAlta";
        colUpFechaAlta.Width = 75;
        colUpFechaAlta.ReadOnly = true;

        colUpPrecio.DataPropertyName = "Precio";
        dataGridViewCellStyle2.Format = "N2";
        colUpPrecio.DefaultCellStyle = dataGridViewCellStyle2;
        colUpPrecio.HeaderText = "Precio";
        colUpPrecio.Name = "colUpPrecio";
        colUpPrecio.Width = 100;
        colUpPrecio.ReadOnly = true;

        // Accesos rápidos
        grpAccesosRapidos.Controls.Add(btnAccesoProductos);
        grpAccesosRapidos.Controls.Add(btnAccesoCategorias);
        grpAccesosRapidos.Controls.Add(btnAccesoSecciones);
        grpAccesosRapidos.Controls.Add(btnAccesoReportes);
        grpAccesosRapidos.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpAccesosRapidos.Location = new Point(20, 640);
        grpAccesosRapidos.Name = "grpAccesosRapidos";
        grpAccesosRapidos.Size = new Size(1050, 110);
        grpAccesosRapidos.TabIndex = 8;
        grpAccesosRapidos.TabStop = false;
        grpAccesosRapidos.Text = "Accesos rápidos";

        btnAccesoProductos.Location = new Point(30, 40);
        btnAccesoProductos.Name = "btnAccesoProductos";
        btnAccesoProductos.Size = new Size(220, 45);
        btnAccesoProductos.TabIndex = 0;
        btnAccesoProductos.Text = "Productos";
        btnAccesoProductos.Click += btnAccesoProductos_Click;

        btnAccesoCategorias.Location = new Point(280, 40);
        btnAccesoCategorias.Name = "btnAccesoCategorias";
        btnAccesoCategorias.Size = new Size(220, 45);
        btnAccesoCategorias.TabIndex = 1;
        btnAccesoCategorias.Text = "Categorías";
        btnAccesoCategorias.Click += btnAccesoCategorias_Click;

        btnAccesoSecciones.Location = new Point(530, 40);
        btnAccesoSecciones.Name = "btnAccesoSecciones";
        btnAccesoSecciones.Size = new Size(220, 45);
        btnAccesoSecciones.TabIndex = 2;
        btnAccesoSecciones.Text = "Secciones";
        btnAccesoSecciones.Click += btnAccesoSecciones_Click;

        btnAccesoReportes.Location = new Point(780, 40);
        btnAccesoReportes.Name = "btnAccesoReportes";
        btnAccesoReportes.Size = new Size(220, 45);
        btnAccesoReportes.TabIndex = 3;
        btnAccesoReportes.Text = "Reportes";
        btnAccesoReportes.Click += btnAccesoReportes_Click;

        // Panel derecho
        pnlDerecho.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
        pnlDerecho.BackColor = Color.White;
        pnlDerecho.BorderStyle = BorderStyle.FixedSingle;
        pnlDerecho.Controls.Add(grpAlertas);
        pnlDerecho.Controls.Add(grpResumenRapido);
        pnlDerecho.Location = new Point(1020, 40);
        pnlDerecho.Name = "pnlDerecho";
        pnlDerecho.Size = new Size(340, 710);
        pnlDerecho.TabIndex = 9;

        grpAlertas.Controls.Add(lstAlertas);
        grpAlertas.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpAlertas.Location = new Point(15, 15);
        grpAlertas.Name = "grpAlertas";
        grpAlertas.Size = new Size(308, 360);
        grpAlertas.TabIndex = 0;
        grpAlertas.TabStop = false;
        grpAlertas.Text = "Alertas";

        lstAlertas.Dock = DockStyle.Fill;
        lstAlertas.Font = new Font("Segoe UI", 10F);
        lstAlertas.FormattingEnabled = true;
        lstAlertas.ItemHeight = 17;
        lstAlertas.Name = "lstAlertas";
        lstAlertas.TabIndex = 0;

        grpResumenRapido.Controls.Add(lblResumen1);
        grpResumenRapido.Controls.Add(lblResumen2);
        grpResumenRapido.Controls.Add(lblResumen3);
        grpResumenRapido.Controls.Add(lblResumen4);
        grpResumenRapido.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpResumenRapido.Location = new Point(15, 395);
        grpResumenRapido.Name = "grpResumenRapido";
        grpResumenRapido.Size = new Size(308, 230);
        grpResumenRapido.TabIndex = 1;
        grpResumenRapido.TabStop = false;
        grpResumenRapido.Text = "Resumen rápido";

        lblResumen1.AutoSize = true;
        lblResumen1.Font = new Font("Segoe UI", 10F);
        lblResumen1.Location = new Point(18, 35);
        lblResumen1.Name = "lblResumen1";
        lblResumen1.Text = "Activos: 0";

        lblResumen2.AutoSize = true;
        lblResumen2.Font = new Font("Segoe UI", 10F);
        lblResumen2.Location = new Point(18, 70);
        lblResumen2.Name = "lblResumen2";
        lblResumen2.Text = "Inactivos: 0";

        lblResumen3.AutoSize = true;
        lblResumen3.Font = new Font("Segoe UI", 10F);
        lblResumen3.Location = new Point(18, 105);
        lblResumen3.Name = "lblResumen3";
        lblResumen3.Text = "Total: 0";

        lblResumen4.AutoSize = true;
        lblResumen4.Font = new Font("Segoe UI", 10F);
        lblResumen4.Location = new Point(18, 140);
        lblResumen4.Name = "lblResumen4";
        lblResumen4.Text = "Bajo stock: 0";

        // FrmMain
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(245, 247, 250);
        ClientSize = new Size(1440, 795);
        Controls.Add(pnlDerecho);
        Controls.Add(grpAccesosRapidos);
        Controls.Add(grpUltimosProductos);
        Controls.Add(grpBajoStock);
        Controls.Add(pnlKpi4);
        Controls.Add(pnlKpi3);
        Controls.Add(pnlKpi2);
        Controls.Add(pnlKpi1);
        Controls.Add(pnlHeader);
        Controls.Add(menuPrincipal);
        MainMenuStrip = menuPrincipal;
        Name = "FrmMain";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Sistema Cotillón";
        WindowState = FormWindowState.Maximized;
        Load += FrmMain_Load;

        menuPrincipal.ResumeLayout(false);
        menuPrincipal.PerformLayout();
        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        pnlKpi1.ResumeLayout(false);
        pnlKpi1.PerformLayout();
        pnlKpi2.ResumeLayout(false);
        pnlKpi2.PerformLayout();
        pnlKpi3.ResumeLayout(false);
        pnlKpi3.PerformLayout();
        pnlKpi4.ResumeLayout(false);
        pnlKpi4.PerformLayout();
        grpBajoStock.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvBajoStock).EndInit();
        grpUltimosProductos.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvUltimosProductos).EndInit();
        grpAccesosRapidos.ResumeLayout(false);
        pnlDerecho.ResumeLayout(false);
        grpAlertas.ResumeLayout(false);
        grpResumenRapido.ResumeLayout(false);
        grpResumenRapido.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private void ConfigurarKpiPanel(Panel panel, int x, int y)
    {
        panel.BackColor = Color.White;
        panel.Location = new Point(x, y);
        panel.Name = "panel";
        panel.Size = new Size(230, 110);
        panel.BorderStyle = BorderStyle.FixedSingle;
    }

    private void ConfigurarBotonRapido(Button button, string texto, int x)
    {
        button.Location = new Point(x, 35);
        button.Size = new Size(220, 45);
        button.Text = texto;
        button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        button.BackColor = Color.White;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
        button.FlatAppearance.BorderSize = 1;
        button.ForeColor = Color.Black;
        button.UseVisualStyleBackColor = false;
    }
}
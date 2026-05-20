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
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();

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

        pnlDerecho = new Panel();
        grpAlertas = new GroupBox();
        grpResumenRapido = new GroupBox();
        lstAlertas = new ListBox();
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
        menuPrincipal.Items.AddRange(new ToolStripItem[] {
            sistemaToolStripMenuItem,
            catalogoToolStripMenuItem,
            operacionesToolStripMenuItem,
            gestionToolStripMenuItem,
            controlToolStripMenuItem
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
        catalogoToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
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
        operacionesToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
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

        // pnlHeader
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
        pnlHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        lblTitulo.AutoSize = true;
        lblTitulo.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        lblTitulo.Location = new Point(20, 10);
        lblTitulo.Name = "lblTitulo";
        lblTitulo.Size = new Size(197, 41);
        lblTitulo.Text = "POP Cotillón";

        lblBienvenida.AutoSize = true;
        lblBienvenida.Font = new Font("Segoe UI", 11F);
        lblBienvenida.Location = new Point(24, 55);
        lblBienvenida.Name = "lblBienvenida";
        lblBienvenida.Size = new Size(83, 20);
        lblBienvenida.Text = "Bienvenido";

        lblUsuario.AutoSize = true;
        lblUsuario.Font = new Font("Segoe UI", 10F);
        lblUsuario.Location = new Point(690, 20);
        lblUsuario.Name = "lblUsuario";
        lblUsuario.Size = new Size(59, 19);
        lblUsuario.Text = "Usuario:";
        lblUsuario.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        btnRecargarDashboard.Location = new Point(760, 48);
        btnRecargarDashboard.Name = "btnRecargarDashboard";
        btnRecargarDashboard.Size = new Size(180, 30);
        btnRecargarDashboard.TabIndex = 0;
        btnRecargarDashboard.Text = "Recargar dashboard";
        btnRecargarDashboard.UseVisualStyleBackColor = true;
        btnRecargarDashboard.Click += btnRecargarDashboard_Click;
        btnRecargarDashboard.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        // KPI panels
        ConfigurarKpiPanel(pnlKpi4, 20, 150);
        ConfigurarKpiPanel(pnlKpi3, 270, 150);
        ConfigurarKpiPanel(pnlKpi2, 520, 150);
        ConfigurarKpiPanel(pnlKpi1, 770, 150);

        // pnlKpi4 - Bajo stock
        pnlKpi4.Controls.Add(lblKpiBajoStockTitulo);
        pnlKpi4.Controls.Add(lblKpiBajoStockValor);

        lblKpiBajoStockTitulo.AutoSize = true;
        lblKpiBajoStockTitulo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblKpiBajoStockTitulo.Location = new Point(20, 18);
        lblKpiBajoStockTitulo.Text = "Bajo stock";

        lblKpiBajoStockValor.AutoSize = true;
        lblKpiBajoStockValor.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
        lblKpiBajoStockValor.Location = new Point(20, 50);
        lblKpiBajoStockValor.Text = "0";

        // pnlKpi3 - Inactivos
        pnlKpi3.Controls.Add(lblKpiInactivosTitulo);
        pnlKpi3.Controls.Add(lblKpiInactivosValor);

        lblKpiInactivosTitulo.AutoSize = true;
        lblKpiInactivosTitulo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblKpiInactivosTitulo.Location = new Point(20, 18);
        lblKpiInactivosTitulo.Text = "Productos inactivos";

        lblKpiInactivosValor.AutoSize = true;
        lblKpiInactivosValor.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
        lblKpiInactivosValor.Location = new Point(20, 50);
        lblKpiInactivosValor.Text = "0";

        // pnlKpi2 - Activos
        pnlKpi2.Controls.Add(lblKpiActivosTitulo);
        pnlKpi2.Controls.Add(lblKpiActivosValor);

        lblKpiActivosTitulo.AutoSize = true;
        lblKpiActivosTitulo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblKpiActivosTitulo.Location = new Point(20, 18);
        lblKpiActivosTitulo.Text = "Productos activos";

        lblKpiActivosValor.AutoSize = true;
        lblKpiActivosValor.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
        lblKpiActivosValor.Location = new Point(20, 50);
        lblKpiActivosValor.Text = "0";

        // pnlKpi1 - Total
        pnlKpi1.Controls.Add(lblKpiTotalProductosTitulo);
        pnlKpi1.Controls.Add(lblKpiTotalProductosValor);

        lblKpiTotalProductosTitulo.AutoSize = true;
        lblKpiTotalProductosTitulo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblKpiTotalProductosTitulo.Location = new Point(20, 18);
        lblKpiTotalProductosTitulo.Text = "Total productos";

        lblKpiTotalProductosValor.AutoSize = true;
        lblKpiTotalProductosValor.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
        lblKpiTotalProductosValor.Location = new Point(20, 50);
        lblKpiTotalProductosValor.Text = "0";

        // grpBajoStock
        grpBajoStock.Controls.Add(dgvBajoStock);
        grpBajoStock.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpBajoStock.Location = new Point(20, 290);
        grpBajoStock.Name = "grpBajoStock";
        grpBajoStock.Size = new Size(480, 330);
        grpBajoStock.TabIndex = 6;
        grpBajoStock.TabStop = false;
        grpBajoStock.Text = "Productos con bajo stock";

        dgvBajoStock.AllowUserToAddRows = false;
        dgvBajoStock.AllowUserToDeleteRows = false;
        dgvBajoStock.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvBajoStock.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvBajoStock.Columns.AddRange(new DataGridViewColumn[] {
            colBsProducto, colBsCategoria, colBsStock, colBsStockMinimo
        });
        dgvBajoStock.Dock = DockStyle.Fill;
        dgvBajoStock.ReadOnly = true;
        dgvBajoStock.RowHeadersVisible = false;
        dgvBajoStock.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        colBsProducto.DataPropertyName = "NombreProducto";
        colBsProducto.HeaderText = "Producto";
        colBsProducto.ReadOnly = true;

        colBsCategoria.DataPropertyName = "NombreCategoria";
        colBsCategoria.HeaderText = "Categoría";
        colBsCategoria.ReadOnly = true;

        colBsStock.DataPropertyName = "Stock";
        colBsStock.HeaderText = "Stock";
        colBsStock.ReadOnly = true;

        colBsStockMinimo.DataPropertyName = "StockMinimo";
        colBsStockMinimo.HeaderText = "Stock mínimo";
        colBsStockMinimo.ReadOnly = true;

        // grpUltimosProductos
        grpUltimosProductos.Controls.Add(dgvUltimosProductos);
        grpUltimosProductos.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpUltimosProductos.Location = new Point(520, 290);
        grpUltimosProductos.Name = "grpUltimosProductos";
        grpUltimosProductos.Size = new Size(480, 330);
        grpUltimosProductos.TabIndex = 7;
        grpUltimosProductos.TabStop = false;
        grpUltimosProductos.Text = "Últimos productos agregados";

        dgvUltimosProductos.AllowUserToAddRows = false;
        dgvUltimosProductos.AllowUserToDeleteRows = false;
        dgvUltimosProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvUltimosProductos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvUltimosProductos.Columns.AddRange(new DataGridViewColumn[] {
            colUpProducto, colUpCategoria, colUpFechaAlta, colUpPrecio
        });
        dgvUltimosProductos.Dock = DockStyle.Fill;
        dgvUltimosProductos.ReadOnly = true;
        dgvUltimosProductos.RowHeadersVisible = false;
        dgvUltimosProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        colUpProducto.DataPropertyName = "NombreProducto";
        colUpProducto.HeaderText = "Producto";
        colUpProducto.ReadOnly = true;

        colUpCategoria.DataPropertyName = "NombreCategoria";
        colUpCategoria.HeaderText = "Categoría";
        colUpCategoria.ReadOnly = true;

        colUpFechaAlta.DataPropertyName = "FechaAlta";
        dataGridViewCellStyle1.Format = "dd/MM/yyyy HH:mm";
        colUpFechaAlta.DefaultCellStyle = dataGridViewCellStyle1;
        colUpFechaAlta.HeaderText = "Fecha alta";
        colUpFechaAlta.ReadOnly = true;

        colUpPrecio.DataPropertyName = "Precio";
        dataGridViewCellStyle2.Format = "N2";
        colUpPrecio.DefaultCellStyle = dataGridViewCellStyle2;
        colUpPrecio.HeaderText = "Precio";
        colUpPrecio.ReadOnly = true;

        // grpAccesosRapidos
        grpAccesosRapidos.Controls.Add(btnAccesoProductos);
        grpAccesosRapidos.Controls.Add(btnAccesoCategorias);
        grpAccesosRapidos.Controls.Add(btnAccesoSecciones);
        grpAccesosRapidos.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpAccesosRapidos.Location = new Point(20, 640);
        grpAccesosRapidos.Name = "grpAccesosRapidos";
        grpAccesosRapidos.Size = new Size(980, 110);
        grpAccesosRapidos.TabIndex = 8;
        grpAccesosRapidos.TabStop = false;
        grpAccesosRapidos.Text = "Accesos rápidos";

        ConfigurarBotonRapido(btnAccesoProductos, "Productos", 25);
        ConfigurarBotonRapido(btnAccesoCategorias, "Categorías", 270);
        ConfigurarBotonRapido(btnAccesoSecciones, "Secciones", 515);

        btnAccesoProductos.Click += btnAccesoProductos_Click;
        btnAccesoCategorias.Click += btnAccesoCategorias_Click;
        btnAccesoSecciones.Click += btnAccesoSecciones_Click;

        // pnlDerecho
        pnlDerecho.BackColor = Color.White;
        pnlDerecho.BorderStyle = BorderStyle.FixedSingle;
        pnlDerecho.Controls.Add(grpAlertas);
        pnlDerecho.Controls.Add(grpResumenRapido);
        pnlDerecho.Location = new Point(1020, 40);
        pnlDerecho.Name = "pnlDerecho";
        pnlDerecho.Size = new Size(340, 710);
        pnlDerecho.TabIndex = 9;
        pnlDerecho.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;

        // grpAlertas
        grpAlertas.Controls.Add(lstAlertas);
        grpAlertas.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpAlertas.Location = new Point(15, 15);
        grpAlertas.Name = "grpAlertas";
        grpAlertas.Size = new Size(308, 360);
        grpAlertas.TabIndex = 0;
        grpAlertas.TabStop = false;
        grpAlertas.Text = "Alertas";

        // lstAlertas
        lstAlertas.Dock = DockStyle.Fill;
        lstAlertas.Font = new Font("Segoe UI", 10F);
        lstAlertas.FormattingEnabled = true;
        lstAlertas.ItemHeight = 17;
        lstAlertas.Name = "lstAlertas";

        // grpResumenRapido
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
        lblResumen1.Size = new Size(80, 19);
        lblResumen1.Text = "Activos: 0";

        lblResumen2.AutoSize = true;
        lblResumen2.Font = new Font("Segoe UI", 10F);
        lblResumen2.Location = new Point(18, 70);
        lblResumen2.Name = "lblResumen2";
        lblResumen2.Size = new Size(89, 19);
        lblResumen2.Text = "Inactivos: 0";

        lblResumen3.AutoSize = true;
        lblResumen3.Font = new Font("Segoe UI", 10F);
        lblResumen3.Location = new Point(18, 105);
        lblResumen3.Name = "lblResumen3";
        lblResumen3.Size = new Size(61, 19);
        lblResumen3.Text = "Total: 0";

        lblResumen4.AutoSize = true;
        lblResumen4.Font = new Font("Segoe UI", 10F);
        lblResumen4.Location = new Point(18, 140);
        lblResumen4.Name = "lblResumen4";
        lblResumen4.Size = new Size(91, 19);
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
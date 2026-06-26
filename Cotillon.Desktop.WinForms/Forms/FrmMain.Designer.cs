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
    private ToolStripMenuItem cajaToolStripMenuItem;
    private ToolStripMenuItem auditoriaToolStripMenuItem;
    private ToolStripMenuItem usuariosToolStripMenuItem;
    private ToolStripMenuItem reportesToolStripMenuItem;
    private ToolStripMenuItem reportesInformesToolStripMenuItem;
    private ToolStripMenuItem estadisticasToolStripMenuItem;

    private Panel pnlHeader;
    private Label lblTitulo;
    private Label lblBienvenida;
    private Label lblUsuario;
    private Button btnCerrarSesion;
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

    private GroupBox grpPanelTrabajo;
    private Button btnTrabajoVentas;
    private Button btnTrabajoClientes;
    private Button btnTrabajoCaja;
    private Panel pnlContenidoTrabajo;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
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
        cajaToolStripMenuItem = new ToolStripMenuItem();
        auditoriaToolStripMenuItem = new ToolStripMenuItem();
        usuariosToolStripMenuItem = new ToolStripMenuItem();
        reportesToolStripMenuItem = new ToolStripMenuItem();
        reportesInformesToolStripMenuItem = new ToolStripMenuItem();
        estadisticasToolStripMenuItem = new ToolStripMenuItem();

        pnlHeader = new Panel();
        lblTitulo = new Label();
        lblBienvenida = new Label();
        lblUsuario = new Label();
        btnCerrarSesion = new Button();
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

        grpUltimosProductos = new GroupBox();
        dgvUltimosProductos = new DataGridView();

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

        grpPanelTrabajo = new GroupBox();
        btnTrabajoVentas = new Button();
        btnTrabajoClientes = new Button();
        btnTrabajoCaja = new Button();
        pnlContenidoTrabajo = new Panel();

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
        grpPanelTrabajo.SuspendLayout();
        SuspendLayout();

        // MENU
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

        sistemaToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { salirToolStripMenuItem });
        sistemaToolStripMenuItem.Text = "Sistema";

        salirToolStripMenuItem.Text = "Salir";
        salirToolStripMenuItem.Click += salirToolStripMenuItem_Click;

        catalogoToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
        {
            seccionesToolStripMenuItem,
            categoriasToolStripMenuItem,
            productosToolStripMenuItem
        });
        catalogoToolStripMenuItem.Text = "Catálogo";

        seccionesToolStripMenuItem.Text = "Secciones";
        seccionesToolStripMenuItem.Click += seccionesToolStripMenuItem_Click;

        categoriasToolStripMenuItem.Text = "Categorías";
        categoriasToolStripMenuItem.Click += categoriasToolStripMenuItem_Click;

        productosToolStripMenuItem.Text = "Productos";
        productosToolStripMenuItem.Click += productosToolStripMenuItem_Click;

        operacionesToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
        {
            ventasToolStripMenuItem,
            comprasToolStripMenuItem
        });
        operacionesToolStripMenuItem.Text = "Operaciones";

        ventasToolStripMenuItem.Text = "Ventas";
        ventasToolStripMenuItem.Click += ventasToolStripMenuItem_Click;

        comprasToolStripMenuItem.Text = "Compras";
        comprasToolStripMenuItem.Click += comprasToolStripMenuItem_Click;

        gestionToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { clientesToolStripMenuItem });
        gestionToolStripMenuItem.Text = "Gestión";

        clientesToolStripMenuItem.Text = "Clientes";
        clientesToolStripMenuItem.Click += clientesToolStripMenuItem_Click;

        controlToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
        {
            cajaToolStripMenuItem,
            auditoriaToolStripMenuItem,
            usuariosToolStripMenuItem
        });
        controlToolStripMenuItem.Text = "Control";

        cajaToolStripMenuItem.Text = "Caja";
        cajaToolStripMenuItem.Click += cajaToolStripMenuItem_Click;

        auditoriaToolStripMenuItem.Text = "Auditoría";
        auditoriaToolStripMenuItem.Click += auditoriaToolStripMenuItem_Click;

        usuariosToolStripMenuItem.Text = "Usuarios";
        usuariosToolStripMenuItem.Click += usuariosToolStripMenuItem_Click;

        reportesToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
        {
            reportesInformesToolStripMenuItem,
            estadisticasToolStripMenuItem
        });
        reportesToolStripMenuItem.Text = "Reportes";

        reportesInformesToolStripMenuItem.Text = "Reportes e informes";
        reportesInformesToolStripMenuItem.Click += reportesInformesToolStripMenuItem_Click;

        estadisticasToolStripMenuItem.Text = "Estadísticas";
        estadisticasToolStripMenuItem.Click += estadisticasToolStripMenuItem_Click;

        // HEADER
        pnlHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pnlHeader.BackColor = Color.FromArgb(255, 77, 109);
        pnlHeader.BorderStyle = BorderStyle.None;
        pnlHeader.Location = new Point(20, 40);
        pnlHeader.Name = "pnlHeader";
        pnlHeader.Size = new Size(1140, 95);
        pnlHeader.TabIndex = 1;

        lblTitulo.AutoSize = true;
        lblTitulo.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
        lblTitulo.ForeColor = Color.White;
        lblTitulo.Location = new Point(25, 10);
        lblTitulo.Text = "POP Cotillón";

        lblBienvenida.AutoSize = true;
        lblBienvenida.Font = new Font("Segoe UI", 11F);
        lblBienvenida.ForeColor = Color.White;
        lblBienvenida.Location = new Point(28, 58);
        lblBienvenida.Text = "Bienvenido";

        lblUsuario.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblUsuario.AutoSize = true;
        lblUsuario.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblUsuario.ForeColor = Color.White;
        lblUsuario.Location = new Point(790, 20);
        lblUsuario.Text = "Usuario:";

        btnCerrarSesion.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCerrarSesion.Location = new Point(790, 55);
        btnCerrarSesion.Size = new Size(150, 32);
        btnCerrarSesion.Text = "Cerrar sesión";
        btnCerrarSesion.Click += btnCerrarSesion_Click;

        btnRecargarDashboard.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnRecargarDashboard.Location = new Point(955, 55);
        btnRecargarDashboard.Size = new Size(150, 32);
        btnRecargarDashboard.Text = "Recargar";
        btnRecargarDashboard.Click += btnRecargarDashboard_Click;

        pnlHeader.Controls.Add(lblTitulo);
        pnlHeader.Controls.Add(lblBienvenida);
        pnlHeader.Controls.Add(lblUsuario);
        pnlHeader.Controls.Add(btnCerrarSesion);
        pnlHeader.Controls.Add(btnRecargarDashboard);

        // KPIS
        ConfigurarKpiPanel(pnlKpi1, 20, 155);
        ConfigurarKpiPanel(pnlKpi2, 270, 155);
        ConfigurarKpiPanel(pnlKpi3, 520, 155);
        ConfigurarKpiPanel(pnlKpi4, 770, 155);

        ConfigurarKpiLabel(lblKpiTotalProductosTitulo, "Total productos", 20, 18);
        ConfigurarKpiValor(lblKpiTotalProductosValor, "0");
        pnlKpi1.Controls.Add(lblKpiTotalProductosTitulo);
        pnlKpi1.Controls.Add(lblKpiTotalProductosValor);

        ConfigurarKpiLabel(lblKpiActivosTitulo, "Productos activos", 20, 18);
        ConfigurarKpiValor(lblKpiActivosValor, "0");
        pnlKpi2.Controls.Add(lblKpiActivosTitulo);
        pnlKpi2.Controls.Add(lblKpiActivosValor);

        ConfigurarKpiLabel(lblKpiInactivosTitulo, "Productos inactivos", 20, 18);
        ConfigurarKpiValor(lblKpiInactivosValor, "0");
        pnlKpi3.Controls.Add(lblKpiInactivosTitulo);
        pnlKpi3.Controls.Add(lblKpiInactivosValor);

        ConfigurarKpiLabel(lblKpiBajoStockTitulo, "Bajo stock", 20, 18);
        ConfigurarKpiValor(lblKpiBajoStockValor, "0");
        pnlKpi4.Controls.Add(lblKpiBajoStockTitulo);
        pnlKpi4.Controls.Add(lblKpiBajoStockValor);

        // GRILLAS
        grpBajoStock.Text = "Productos con bajo stock";
        grpBajoStock.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpBajoStock.BackColor = Color.White;
        grpBajoStock.Location = new Point(20, 290);
        grpBajoStock.Size = new Size(490, 330);

        dgvBajoStock.Dock = DockStyle.Fill;
        dgvBajoStock.AllowUserToAddRows = false;
        dgvBajoStock.AllowUserToDeleteRows = false;
        dgvBajoStock.ReadOnly = true;
        dgvBajoStock.RowHeadersVisible = false;
        dgvBajoStock.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grpBajoStock.Controls.Add(dgvBajoStock);

        grpUltimosProductos.Text = "Últimos productos agregados";
        grpUltimosProductos.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpUltimosProductos.BackColor = Color.White;
        grpUltimosProductos.Location = new Point(530, 290);
        grpUltimosProductos.Size = new Size(490, 330);

        dgvUltimosProductos.Dock = DockStyle.Fill;
        dgvUltimosProductos.AllowUserToAddRows = false;
        dgvUltimosProductos.AllowUserToDeleteRows = false;
        dgvUltimosProductos.ReadOnly = true;
        dgvUltimosProductos.RowHeadersVisible = false;
        dgvUltimosProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grpUltimosProductos.Controls.Add(dgvUltimosProductos);

        // ACCESOS RAPIDOS
        grpAccesosRapidos.Text = "Accesos rápidos";
        grpAccesosRapidos.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpAccesosRapidos.BackColor = Color.White;
        grpAccesosRapidos.Location = new Point(20, 640);
        grpAccesosRapidos.Size = new Size(1050, 110);

        ConfigurarBotonRapido(btnAccesoProductos, "Productos", 25);
        ConfigurarBotonRapido(btnAccesoCategorias, "Categorías", 280);
        ConfigurarBotonRapido(btnAccesoSecciones, "Secciones", 535);
        ConfigurarBotonRapido(btnAccesoReportes, "Reportes", 790);

        btnAccesoProductos.Click += btnAccesoProductos_Click;
        btnAccesoCategorias.Click += btnAccesoCategorias_Click;
        btnAccesoSecciones.Click += btnAccesoSecciones_Click;
        btnAccesoReportes.Click += btnAccesoReportes_Click;

        grpAccesosRapidos.Controls.Add(btnAccesoProductos);
        grpAccesosRapidos.Controls.Add(btnAccesoCategorias);
        grpAccesosRapidos.Controls.Add(btnAccesoSecciones);
        grpAccesosRapidos.Controls.Add(btnAccesoReportes);

        // PANEL DERECHO
        pnlDerecho.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
        pnlDerecho.BackColor = Color.White;
        pnlDerecho.BorderStyle = BorderStyle.FixedSingle;
        pnlDerecho.Location = new Point(1180, 40);
        pnlDerecho.Size = new Size(340, 710);

        grpAlertas.Text = "Alertas";
        grpAlertas.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpAlertas.Location = new Point(15, 15);
        grpAlertas.Size = new Size(308, 360);

        lstAlertas.Dock = DockStyle.Fill;
        lstAlertas.Font = new Font("Segoe UI", 10F);
        grpAlertas.Controls.Add(lstAlertas);

        grpResumenRapido.Text = "Resumen rápido";
        grpResumenRapido.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpResumenRapido.Location = new Point(15, 395);
        grpResumenRapido.Size = new Size(308, 230);

        ConfigurarResumenLabel(lblResumen1, "Activos: 0", 35);
        ConfigurarResumenLabel(lblResumen2, "Inactivos: 0", 70);
        ConfigurarResumenLabel(lblResumen3, "Total: 0", 105);
        ConfigurarResumenLabel(lblResumen4, "Bajo stock: 0", 140);

        grpResumenRapido.Controls.Add(lblResumen1);
        grpResumenRapido.Controls.Add(lblResumen2);
        grpResumenRapido.Controls.Add(lblResumen3);
        grpResumenRapido.Controls.Add(lblResumen4);

        pnlDerecho.Controls.Add(grpAlertas);
        pnlDerecho.Controls.Add(grpResumenRapido);

        // PANEL TRABAJADOR
        grpPanelTrabajo.Text = "Panel de trabajo";
        grpPanelTrabajo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpPanelTrabajo.BackColor = Color.White;
        grpPanelTrabajo.Location = new Point(20, 170);
        grpPanelTrabajo.Size = new Size(1500, 650);
        grpPanelTrabajo.Visible = false;

        btnTrabajoVentas.Location = new Point(20, 30);
        btnTrabajoVentas.Size = new Size(220, 45);
        btnTrabajoVentas.Text = "Ventas";
        btnTrabajoVentas.Click += btnTrabajoVentas_Click;

        btnTrabajoClientes.Location = new Point(260, 30);
        btnTrabajoClientes.Size = new Size(220, 45);
        btnTrabajoClientes.Text = "Clientes";
        btnTrabajoClientes.Click += btnTrabajoClientes_Click;

        btnTrabajoCaja.Location = new Point(500, 30);
        btnTrabajoCaja.Size = new Size(220, 45);
        btnTrabajoCaja.Text = "Caja";
        btnTrabajoCaja.Click += btnTrabajoCaja_Click;

        pnlContenidoTrabajo.Location = new Point(20, 90);
        pnlContenidoTrabajo.Size = new Size(1460, 535);
        pnlContenidoTrabajo.BorderStyle = BorderStyle.FixedSingle;

        grpPanelTrabajo.Controls.Add(btnTrabajoVentas);
        grpPanelTrabajo.Controls.Add(btnTrabajoClientes);
        grpPanelTrabajo.Controls.Add(btnTrabajoCaja);
        grpPanelTrabajo.Controls.Add(pnlContenidoTrabajo);

        // FORM
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(250, 248, 242);

        BackgroundImage = Properties.Resources.FondoEscritorioApp;
        BackgroundImageLayout = ImageLayout.Stretch;

        ClientSize = new Size(1440, 795);

        Controls.Add(pnlDerecho);
        Controls.Add(grpAccesosRapidos);
        Controls.Add(grpUltimosProductos);
        Controls.Add(grpBajoStock);
        Controls.Add(pnlKpi4);
        Controls.Add(pnlKpi3);
        Controls.Add(pnlKpi2);
        Controls.Add(pnlKpi1);
        Controls.Add(grpPanelTrabajo);
        Controls.Add(pnlHeader);
        Controls.Add(menuPrincipal);

        MainMenuStrip = menuPrincipal;
        Name = "FrmMain";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Sistema POP Cotillón";
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
        grpPanelTrabajo.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    private void ConfigurarKpiPanel(Panel panel, int x, int y)
    {
        panel.BackColor = Color.White;
        panel.Location = new Point(x, y);
        panel.Size = new Size(230, 110);
        panel.BorderStyle = BorderStyle.FixedSingle;
    }

    private void ConfigurarKpiLabel(Label label, string texto, int x, int y)
    {
        label.AutoSize = true;
        label.Text = texto;
        label.Location = new Point(x, y);
        label.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
    }

    private void ConfigurarKpiValor(Label label, string texto)
    {
        label.AutoSize = true;
        label.Text = texto;
        label.Location = new Point(20, 50);
        label.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
    }

    private void ConfigurarResumenLabel(Label label, string texto, int y)
    {
        label.AutoSize = true;
        label.Text = texto;
        label.Location = new Point(18, y);
        label.Font = new Font("Segoe UI", 10F);
    }

    private void ConfigurarBotonRapido(Button button, string texto, int x)
    {
        button.Location = new Point(x, 40);
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
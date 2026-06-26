using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Desktop.WinForms.Models;
using Cotillon.Desktop.WinForms.Services;

namespace Cotillon.Desktop.WinForms.Forms;

public partial class FrmMain : Form
{
    private readonly UsuarioSesion _usuario;
    private readonly DashboardApiService _dashboardService;
    private readonly CajaApiService _cajaService;

    private int _sesionCajaId = 0;

    public FrmMain(UsuarioSesion usuario, int sesionCajaId)
    {
        InitializeComponent();

        _usuario = usuario;
        _sesionCajaId = sesionCajaId;
        _dashboardService = new DashboardApiService();
        _cajaService = new CajaApiService();

        AplicarPermisosPorRol();
        AplicarDiseñoCotillon();
    }

    private async void FrmMain_Load(object sender, EventArgs e)
    {
        try
        {
            lblUsuario.Text = $"Usuario: {_usuario.NombreCompleto} | Rol: {_usuario.Rol}";
            lblBienvenida.Text = $"Bienvenido, {_usuario.NombreCompleto} · Todo para tus fiestas y eventos 🎉";

            ConfigurarGrillasDashboard();

            await VerificarCajaAbierta();

            if (EsAdmin())
                await CargarDashboard();
            else
            {
                PrepararDashboardTrabajador();
                CargarPanelTrabajador();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Error al cargar el menú principal.\n\n" + ex.Message,
                "Sistema",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private bool EsAdmin()
    {
        return _usuario.Rol.Equals("Admin", StringComparison.OrdinalIgnoreCase);
    }

    private void AplicarPermisosPorRol()
    {
        bool esAdmin = EsAdmin();

        catalogoToolStripMenuItem.Visible = esAdmin;
        comprasToolStripMenuItem.Visible = esAdmin;
        reportesToolStripMenuItem.Visible = esAdmin;
        estadisticasToolStripMenuItem.Visible = esAdmin;
        auditoriaToolStripMenuItem.Visible = esAdmin;
        usuariosToolStripMenuItem.Visible = esAdmin;

        pnlDerecho.Visible = esAdmin;
        grpBajoStock.Visible = esAdmin;
        grpUltimosProductos.Visible = esAdmin;
        grpAccesosRapidos.Visible = esAdmin;

        pnlKpi1.Visible = esAdmin;
        pnlKpi2.Visible = esAdmin;
        pnlKpi3.Visible = esAdmin;
        pnlKpi4.Visible = esAdmin;

        grpPanelTrabajo.Visible = !esAdmin;
    }

    private async Task VerificarCajaAbierta()
    {
        try
        {
            _sesionCajaId = await _cajaService.ObtenerOCrearCajaAsync(_usuario.UsuarioId);

            if (_sesionCajaId <= 0)
            {
                MessageBox.Show(
                    "No se pudo abrir la caja del usuario actual.",
                    "Caja",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Error al verificar la caja.\n\n" + ex.Message,
                "Caja",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ConfigurarGrillasDashboard()
    {
        ConfigurarGrillaBajoStock();
        ConfigurarGrillaUltimosProductos();
    }

    private void ConfigurarGrillaBajoStock()
    {
        dgvBajoStock.Parent = grpBajoStock;
        dgvBajoStock.MaximumSize = Size.Empty;
        dgvBajoStock.MinimumSize = Size.Empty;
        dgvBajoStock.Dock = DockStyle.Fill;

        dgvBajoStock.Columns.Clear();
        dgvBajoStock.AutoGenerateColumns = false;

        dgvBajoStock.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Producto",
            DataPropertyName = "NombreProducto",
            FillWeight = 160
        });

        dgvBajoStock.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Categoría",
            DataPropertyName = "NombreCategoria",
            FillWeight = 120
        });

        dgvBajoStock.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Stock",
            DataPropertyName = "Stock",
            FillWeight = 70
        });

        dgvBajoStock.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Stock mínimo",
            DataPropertyName = "StockMinimo",
            FillWeight = 100
        });

        AplicarEstiloGrid(dgvBajoStock);
    }

    private void ConfigurarGrillaUltimosProductos()
    {
        dgvUltimosProductos.Parent = grpUltimosProductos;
        dgvUltimosProductos.MaximumSize = Size.Empty;
        dgvUltimosProductos.MinimumSize = Size.Empty;
        dgvUltimosProductos.Dock = DockStyle.Fill;

        dgvUltimosProductos.Columns.Clear();
        dgvUltimosProductos.AutoGenerateColumns = false;

        dgvUltimosProductos.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Producto",
            DataPropertyName = "NombreProducto",
            FillWeight = 160
        });

        dgvUltimosProductos.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Categoría",
            DataPropertyName = "NombreCategoria",
            FillWeight = 120
        });

        dgvUltimosProductos.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Fecha alta",
            DataPropertyName = "FechaAlta",
            FillWeight = 90,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "dd/MM/yyyy"
            }
        });

        dgvUltimosProductos.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Precio",
            DataPropertyName = "Precio",
            FillWeight = 90,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "N2"
            }
        });

        AplicarEstiloGrid(dgvUltimosProductos);
    }

    private void AplicarEstiloGrid(DataGridView dgv)
    {
        dgv.MaximumSize = Size.Empty;
        dgv.MinimumSize = Size.Empty;

        dgv.Dock = DockStyle.Fill;
        dgv.Margin = new Padding(0);

        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.RowHeadersVisible = false;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.ReadOnly = true;
        dgv.MultiSelect = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.ScrollBars = ScrollBars.Both;
        dgv.BackgroundColor = Color.White;
        dgv.BorderStyle = BorderStyle.FixedSingle;
        dgv.EnableHeadersVisualStyles = false;

        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
    }

    private async Task CargarDashboard()
    {
        try
        {
            var resumen = await _dashboardService.ObtenerResumenAsync();
            var bajoStock = await _dashboardService.ObtenerBajoStockAsync();
            var ultimos = await _dashboardService.ObtenerUltimosProductosAsync();

            dgvBajoStock.DataSource = null;
            dgvUltimosProductos.DataSource = null;

            dgvBajoStock.DataSource = bajoStock;
            dgvUltimosProductos.DataSource = ultimos;

            lstAlertas.Items.Clear();

            if (resumen != null)
            {
                lblKpiTotalProductosValor.Text = resumen.TotalProductos.ToString();
                lblKpiActivosValor.Text = resumen.ProductosActivos.ToString();
                lblKpiInactivosValor.Text = resumen.ProductosInactivos.ToString();
                lblKpiBajoStockValor.Text = resumen.ProductosBajoStock.ToString();

                lblResumen1.Text = $"Activos: {resumen.ProductosActivos}";
                lblResumen2.Text = $"Inactivos: {resumen.ProductosInactivos}";
                lblResumen3.Text = $"Total: {resumen.TotalProductos}";
                lblResumen4.Text = $"Bajo stock: {resumen.ProductosBajoStock}";

                if (resumen.ProductosBajoStock > 0)
                    lstAlertas.Items.Add($"Hay {resumen.ProductosBajoStock} productos con bajo stock.");

                if (resumen.ProductosInactivos > 0)
                    lstAlertas.Items.Add($"Hay {resumen.ProductosInactivos} productos inactivos.");
            }

            if (bajoStock.Count == 0)
                lstAlertas.Items.Add("No hay productos con bajo stock.");

            if (ultimos.Count > 0)
                lstAlertas.Items.Add($"Último agregado: {ultimos[0].NombreProducto}");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Error al cargar el dashboard.\n\n" + ex.Message,
                "Dashboard",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void PrepararDashboardTrabajador()
    {
        grpPanelTrabajo.Visible = true;
        grpPanelTrabajo.Location = new Point(20, 170);
        grpPanelTrabajo.Size = new Size(1500, 650);

        btnTrabajoVentas.Location = new Point(20, 30);
        btnTrabajoVentas.Size = new Size(220, 45);

        btnTrabajoClientes.Location = new Point(260, 30);
        btnTrabajoClientes.Size = new Size(220, 45);

        btnTrabajoCaja.Location = new Point(500, 30);
        btnTrabajoCaja.Size = new Size(220, 45);

        pnlContenidoTrabajo.Location = new Point(20, 90);
        pnlContenidoTrabajo.Size = new Size(1460, 535);
    }

    private void CargarPanelTrabajador()
    {
        AbrirFormularioEnPanel(new FrmVentas(_usuario.UsuarioId, _sesionCajaId));
    }

    private void AbrirFormularioEnPanel(Form form)
    {
        try
        {
            pnlContenidoTrabajo.Controls.Clear();

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.AutoScroll = true;

            pnlContenidoTrabajo.Controls.Add(form);

            form.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "No se pudo abrir el formulario.\n\n" + ex.Message,
                "Sistema",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void btnTrabajoVentas_Click(object sender, EventArgs e)
    {
        AbrirFormularioEnPanel(new FrmVentas(_usuario.UsuarioId, _sesionCajaId));
    }

    private void btnTrabajoClientes_Click(object sender, EventArgs e)
    {
        AbrirFormularioEnPanel(new FrmClientes());
    }

    private void btnTrabajoCaja_Click(object sender, EventArgs e)
    {
        AbrirFormularioEnPanel(new FrmCaja(_usuario.UsuarioId));
    }

    private void salirToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Application.Exit();
    }

    private void seccionesToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var frm = new FrmSecciones();
        frm.ShowDialog();
    }

    private void categoriasToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var frm = new FrmCategorias();
        frm.ShowDialog();
    }

    private void productosToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var frm = new FrmProductos();
        frm.ShowDialog();
    }

    private void ventasToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var frm = new FrmVentas(_usuario.UsuarioId, _sesionCajaId);
        frm.ShowDialog();
    }

    private void comprasToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var frm = new FrmCompras(_usuario.UsuarioId);
        frm.ShowDialog();

    }

    private void clientesToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var frm = new FrmClientes();
        frm.ShowDialog();
    }

    private void auditoriaToolStripMenuItem_Click(object sender, EventArgs e)
    {
        MessageBox.Show("Próximo módulo: Auditoría", "Auditoría");
    }

    private void btnAccesoProductos_Click(object sender, EventArgs e)
    {
        using var frm = new FrmProductos();
        frm.ShowDialog();
    }

    private void btnAccesoCategorias_Click(object sender, EventArgs e)
    {
        using var frm = new FrmCategorias();
        frm.ShowDialog();
    }

    private void btnAccesoSecciones_Click(object sender, EventArgs e)
    {
        using var frm = new FrmSecciones();
        frm.ShowDialog();
    }

    private void btnAccesoReportes_Click(object sender, EventArgs e)
    {
        using var frm = new FrmReportes();
        frm.ShowDialog();
    }

    private void reportesInformesToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var frm = new FrmReportes();
        frm.ShowDialog();
    }

    private void estadisticasToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var frm = new FrmEstadisticas();
        frm.ShowDialog();
    }

    private async void cajaToolStripMenuItem_Click(object sender, EventArgs e)
    {
        try
        {
            using var frm = new FrmCaja(_usuario.UsuarioId);
            frm.ShowDialog();

            var caja = await _cajaService.ObtenerCajaAbiertaAsync(_usuario.UsuarioId);
            _sesionCajaId = caja?.SesionCajaId ?? 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Error al abrir Caja.\n\n" + ex.Message,
                "Caja",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void usuariosToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var frm = new FrmUsuarios();
        frm.ShowDialog();
    }

    private void btnCerrarSesion_Click(object sender, EventArgs e)
    {
        Hide();

        using var login = new FrmLogin();
        login.ShowDialog();

        Close();
    }

    private async void btnRecargarDashboard_Click(object sender, EventArgs e)
    {
        try
        {
            if (EsAdmin())
                await CargarDashboard();
            else
                CargarPanelTrabajador();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Error al actualizar el dashboard.\n\n" + ex.Message,
                "Dashboard",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void AplicarTemaVisual()
    {
        PopTheme.ApplyForm(this);

        foreach (Control control in Controls)
            AplicarTemaAControl(control);
    }

    private void AplicarTemaAControl(Control control)
    {
        if (control is GroupBox groupBox)
            PopTheme.ApplyGroupBox(groupBox);

        if (control is Panel panel)
            PopTheme.ApplyCard(panel);

        if (control is TextBox textBox)
            PopTheme.ApplyInput(textBox);

        if (control is ComboBox comboBox)
            PopTheme.ApplyCombo(comboBox);

        if (control is DataGridView dgv)
            PopTheme.ApplyGrid(dgv);

        if (control is ListBox listBox)
            PopTheme.ApplyListBox(listBox);

        foreach (Control child in control.Controls)
            AplicarTemaAControl(child);
    }
    private void AplicarDiseñoCotillon()
    {
        PopTheme.ApplyForm(this);

        Text = "Sistema POP Cotillón";
        BackColor = Color.FromArgb(247, 248, 252);

        menuPrincipal.BackColor = Color.White;
        menuPrincipal.ForeColor = PopTheme.Colors.Texto;
        menuPrincipal.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

        pnlHeader.BackColor = PopTheme.Colors.Principal;
        pnlHeader.BorderStyle = BorderStyle.None;

        lblTitulo.Text = "POP Cotillón";
        lblTitulo.ForeColor = Color.White;
        lblTitulo.Font = new Font("Segoe UI", 24F, FontStyle.Bold);

        lblBienvenida.ForeColor = Color.White;
        lblBienvenida.Font = new Font("Segoe UI", 11F);

        lblUsuario.ForeColor = Color.White;
        lblUsuario.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

        PopTheme.ApplySecondaryButton(btnCerrarSesion);
        PopTheme.ApplyPrimaryButton(btnRecargarDashboard);

        ConfigurarKpiVisual(pnlKpi1, lblKpiTotalProductosTitulo, lblKpiTotalProductosValor, "Total productos", PopTheme.Colors.Secundario);
        ConfigurarKpiVisual(pnlKpi2, lblKpiActivosTitulo, lblKpiActivosValor, "Productos activos", PopTheme.Colors.Verde);
        ConfigurarKpiVisual(pnlKpi3, lblKpiInactivosTitulo, lblKpiInactivosValor, "Productos inactivos", PopTheme.Colors.Rojo);
        ConfigurarKpiVisual(pnlKpi4, lblKpiBajoStockTitulo, lblKpiBajoStockValor, "Bajo stock", PopTheme.Colors.Amarillo);

        PopTheme.ApplyGroupBox(grpBajoStock);
        PopTheme.ApplyGroupBox(grpUltimosProductos);
        PopTheme.ApplyGroupBox(grpAccesosRapidos);
        PopTheme.ApplyGroupBox(grpAlertas);
        PopTheme.ApplyGroupBox(grpResumenRapido);
        PopTheme.ApplyGroupBox(grpPanelTrabajo);

        grpBajoStock.Text = "Productos con bajo stock";
        grpUltimosProductos.Text = "Últimos productos agregados";
        grpAccesosRapidos.Text = "Accesos rápidos";
        grpAlertas.Text = "Alertas";
        grpResumenRapido.Text = "Resumen rápido";
        grpPanelTrabajo.Text = "Panel de trabajo";

        PopTheme.ApplyGrid(dgvBajoStock);
        PopTheme.ApplyGrid(dgvUltimosProductos);

        PopTheme.ApplyPrimaryButton(btnAccesoProductos);
        PopTheme.ApplySecondaryButton(btnAccesoCategorias);
        PopTheme.ApplySecondaryButton(btnAccesoSecciones);
        PopTheme.ApplySecondaryButton(btnAccesoReportes);

        btnAccesoProductos.Text = "Productos";
        btnAccesoCategorias.Text = "Categorías";
        btnAccesoSecciones.Text = "Secciones";
        btnAccesoReportes.Text = "Reportes";

        PopTheme.ApplyPrimaryButton(btnTrabajoVentas);
        PopTheme.ApplySecondaryButton(btnTrabajoClientes);
        PopTheme.ApplySecondaryButton(btnTrabajoCaja);

        btnTrabajoVentas.Text = "Ventas";
        btnTrabajoClientes.Text = "Clientes";
        btnTrabajoCaja.Text = "Caja";

        pnlDerecho.BackColor = Color.White;
        pnlDerecho.BorderStyle = BorderStyle.FixedSingle;

        PopTheme.ApplyListBox(lstAlertas);

        lblResumen1.ForeColor = PopTheme.Colors.Texto;
        lblResumen2.ForeColor = PopTheme.Colors.Texto;
        lblResumen3.ForeColor = PopTheme.Colors.Texto;
        lblResumen4.ForeColor = PopTheme.Colors.Texto;
    }
    private void ConfigurarKpiVisual(Panel panel, Label titulo, Label valor, string textoTitulo, Color colorValor)
    {
        panel.BackColor = Color.White;
        panel.BorderStyle = BorderStyle.None;

        titulo.Text = textoTitulo;
        titulo.ForeColor = PopTheme.Colors.TextoSuave;
        titulo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

        valor.ForeColor = colorValor;
        valor.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
    }

    
}
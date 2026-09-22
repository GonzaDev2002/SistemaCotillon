using Cotillon.Desktop.WinForms.Models;
using Cotillon.Desktop.WinForms.Services;
using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Desktop.WinForms.Forms;

namespace Cotillon.Desktop.WinForms.Forms;

public partial class FrmMain : Form
{
    private readonly UsuarioSesion _usuario;
    private readonly DashboardApiService _dashboardService;

    public FrmMain(UsuarioSesion usuario)
    {
        InitializeComponent();
        _usuario = usuario;
        _dashboardService = new DashboardApiService();
        AplicarTema();
        ConfigurarGrillasDashboard();
    }

    private void ConfigurarGrillasDashboard()
    {
        dgvBajoStock.Parent = grpBajoStock;
        dgvBajoStock.Dock = DockStyle.Fill;
        dgvBajoStock.AutoGenerateColumns = false;
        dgvBajoStock.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvBajoStock.RowHeadersVisible = false;
        dgvBajoStock.ScrollBars = ScrollBars.Both;
        dgvBajoStock.MaximumSize = Size.Empty;
        dgvBajoStock.MinimumSize = Size.Empty;

        dgvUltimosProductos.Parent = grpUltimosProductos;
        dgvUltimosProductos.Dock = DockStyle.Fill;
        dgvUltimosProductos.AutoGenerateColumns = false;
        dgvUltimosProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvUltimosProductos.RowHeadersVisible = false;
        dgvUltimosProductos.ScrollBars = ScrollBars.Both;
        dgvUltimosProductos.MaximumSize = Size.Empty;
        dgvUltimosProductos.MinimumSize = Size.Empty;

        grpBajoStock.PerformLayout();
        grpUltimosProductos.PerformLayout();
        dgvBajoStock.Refresh();
        dgvUltimosProductos.Refresh();
    }

    private async void FrmMain_Load(object sender, EventArgs e)
    {
        lblUsuario.Text = $"Usuario: {_usuario.NombreCompleto} | Rol: {_usuario.Rol}";
        lblBienvenida.Text = $"Bienvenido, {_usuario.NombreCompleto}";
        await CargarDashboard();
    }

    private async Task CargarDashboard()
    {
        var resumen = await _dashboardService.ObtenerResumenAsync();
        var bajoStock = await _dashboardService.ObtenerBajoStockAsync();
        var ultimos = await _dashboardService.ObtenerUltimosProductosAsync();

        dgvBajoStock.AutoGenerateColumns = false;
        dgvUltimosProductos.AutoGenerateColumns = false;

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
        MessageBox.Show("Próximo módulo: Ventas", "Ventas");
    }

    private void comprasToolStripMenuItem_Click(object sender, EventArgs e)
    {
        MessageBox.Show("Próximo módulo: Compras", "Compras");
    }

    private void clientesToolStripMenuItem_Click(object sender, EventArgs e)
    {
        MessageBox.Show("Próximo módulo: Clientes", "Clientes");
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
    private void reportesToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var frm = new FrmReportes();
        frm.ShowDialog();
    }
    private void btnAccesoReportes_Click(object sender, EventArgs e)
    {
        FrmReportes frm = new FrmReportes();
        frm.ShowDialog();
    }

    private async void btnRecargarDashboard_Click(object sender, EventArgs e)
    {
        await CargarDashboard();
    }
    private void AplicarTema()
    {
        AppTheme.ApplyFormStyle(this);

        pnlHeader.BackColor = AppTheme.Colors.Surface;
        pnlDerecho.BackColor = AppTheme.Colors.Surface;

        AppTheme.ApplyTitle(lblTitulo);
        AppTheme.ApplySubtitle(lblBienvenida);
        lblUsuario.ForeColor = AppTheme.Colors.TextPrimary;

        AppTheme.ApplyPrimaryButton(btnRecargarDashboard);

        AppTheme.ApplyPanelCard(pnlKpi1);
        AppTheme.ApplyPanelCard(pnlKpi2);
        AppTheme.ApplyPanelCard(pnlKpi3);
        AppTheme.ApplyPanelCard(pnlKpi4);

        lblKpiTotalProductosTitulo.ForeColor = AppTheme.Colors.TextSecondary;
        lblKpiActivosTitulo.ForeColor = AppTheme.Colors.TextSecondary;
        lblKpiInactivosTitulo.ForeColor = AppTheme.Colors.TextSecondary;
        lblKpiBajoStockTitulo.ForeColor = AppTheme.Colors.TextSecondary;

        lblKpiTotalProductosValor.ForeColor = AppTheme.Colors.Primary;
        lblKpiActivosValor.ForeColor = AppTheme.Colors.Success;
        lblKpiInactivosValor.ForeColor = AppTheme.Colors.Danger;
        lblKpiBajoStockValor.ForeColor = AppTheme.Colors.Warning;

        AppTheme.ApplyGroupBox(grpBajoStock);
        AppTheme.ApplyGroupBox(grpUltimosProductos);
        AppTheme.ApplyGroupBox(grpAccesosRapidos);
        AppTheme.ApplyGroupBox(grpAlertas);
        AppTheme.ApplyGroupBox(grpResumenRapido);

        //AppTheme.ApplyGrid(dgvBajoStock);
        //AppTheme.ApplyGrid(dgvUltimosProductos);

        AppTheme.ApplyPrimaryButton(btnAccesoProductos);
        AppTheme.ApplySecondaryButton(btnAccesoCategorias);
        AppTheme.ApplySecondaryButton(btnAccesoSecciones);

        lstAlertas.BackColor = Color.White;
        lstAlertas.ForeColor = AppTheme.Colors.TextPrimary;
        lstAlertas.BorderStyle = BorderStyle.None;

        lblResumen1.ForeColor = AppTheme.Colors.TextPrimary;
        lblResumen2.ForeColor = AppTheme.Colors.TextPrimary;
        lblResumen3.ForeColor = AppTheme.Colors.TextPrimary;
        lblResumen4.ForeColor = AppTheme.Colors.TextPrimary;
    }
}
using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Desktop.WinForms.Services;
using WinChart = System.Windows.Forms.DataVisualization.Charting;

namespace Cotillon.Desktop.WinForms.Forms;

public partial class FrmEstadisticas : Form
{
    private readonly EstadisticasApiService _estadisticasService = new();

    public FrmEstadisticas()
    {
        InitializeComponent();

        try
        {
            AplicarDiseñoEstadisticas();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al inicializar estadísticas:\n" + ex.Message,
                "Estadísticas", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AplicarDiseñoEstadisticas()
    {
        PopTheme.ApplyForm(this);

        Text = "POP Cotillón - Estadísticas";
        BackColor = Color.FromArgb(250, 248, 242);
        BackgroundImage = Properties.Resources.fondoForms;
        BackgroundImageLayout = ImageLayout.Stretch;

        pnlHeader.BackColor = PopTheme.Colors.Principal;

        lblTitulo.ForeColor = Color.White;
        lblTitulo.Font = new Font("Segoe UI", 22F, FontStyle.Bold);

        lblSubtitulo.ForeColor = Color.White;
        lblSubtitulo.Font = new Font("Segoe UI", 11F);

        PopTheme.ApplyGroupBox(grpFiltros);
        PopTheme.ApplyGroupBox(grpGrafico);

        foreach (Control control in grpFiltros.Controls)
        {
            if (control is Label lbl)
            {
                lbl.ForeColor = PopTheme.Colors.Texto;
                lbl.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            }

            if (control is ComboBox cmb)
                PopTheme.ApplyCombo(cmb);
        }

        PopTheme.ApplyPrimaryButton(btnGenerar);
        PopTheme.ApplySecondaryButton(btnCerrar);

        lblResumen.ForeColor = PopTheme.Colors.Secundario;
        lblResumen.Font = new Font("Segoe UI", 11F, FontStyle.Bold);

        chartEstadisticas.BackColor = Color.White;
        chartEstadisticas.BorderlineColor = PopTheme.Colors.Borde;
        chartEstadisticas.BorderlineDashStyle = WinChart.ChartDashStyle.Solid;
    }

    private void FrmEstadisticas_Load(object sender, EventArgs e)
    {
        try
        {
            cmbTipoGrafico.Items.Clear();
            cmbTipoGrafico.Items.Add("Ventas por día");
            cmbTipoGrafico.Items.Add("Productos más vendidos");
            cmbTipoGrafico.Items.Add("Ventas por medio de pago");
            cmbTipoGrafico.Items.Add("Productos con bajo stock");

            cmbTipoGrafico.SelectedIndex = 0;
            dtpDesde.Value = DateTime.Today.AddDays(-30);
            dtpHasta.Value = DateTime.Today;

            ConfigurarGrafico();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar estadísticas:\n" + ex.Message,
                "Estadísticas", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ConfigurarGrafico()
    {
        try
        {
            chartEstadisticas.Series.Clear();
            chartEstadisticas.ChartAreas.Clear();
            chartEstadisticas.Titles.Clear();
            chartEstadisticas.Legends.Clear();

            WinChart.ChartArea area = new WinChart.ChartArea("AreaPrincipal");
            area.BackColor = Color.White;
            area.AxisX.Interval = 1;
            area.AxisX.LabelStyle.Angle = -30;
            area.AxisY.Title = "Valores";
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(230, 230, 230);
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(230, 230, 230);

            chartEstadisticas.ChartAreas.Add(area);

            WinChart.Legend legend = new WinChart.Legend("Leyenda");
            chartEstadisticas.Legends.Add(legend);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al configurar gráfico:\n" + ex.Message,
                "Estadísticas", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnGenerar_Click(object sender, EventArgs e)
    {
        try
        {
            if (dtpDesde.Value.Date > dtpHasta.Value.Date)
            {
                MessageBox.Show("La fecha desde no puede ser mayor a la fecha hasta.");
                return;
            }

            string tipo = cmbTipoGrafico.SelectedItem?.ToString() ?? "";

            if (tipo == "Ventas por día")
                await CargarVentasPorDia();
            else if (tipo == "Productos más vendidos")
                await CargarProductosMasVendidos();
            else if (tipo == "Ventas por medio de pago")
                await CargarVentasPorMedioPago();
            else if (tipo == "Productos con bajo stock")
                await CargarProductosBajoStock();
            else
                MessageBox.Show("Seleccione un tipo de gráfico válido.");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al generar estadística:\n" + ex.Message,
                "Estadísticas", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task CargarVentasPorDia()
    {
        try
        {
            var datos = await _estadisticasService.VentasPorDiaAsync(
                dtpDesde.Value.Date,
                dtpHasta.Value.Date);

            ConfigurarGrafico();
            chartEstadisticas.Titles.Add("Ventas por día");

            WinChart.Series serie = new WinChart.Series("Total vendido")
            {
                ChartType = WinChart.SeriesChartType.Column,
                IsValueShownAsLabel = true
            };

            foreach (var item in datos)
                serie.Points.AddXY(item.Fecha.ToString("dd/MM"), item.Total);

            chartEstadisticas.Series.Add(serie);
            lblResumen.Text = $"Días con ventas: {datos.Count} | Total: {datos.Sum(x => x.Total):C2}";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar ventas por día:\n" + ex.Message,
                "Estadísticas", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task CargarProductosMasVendidos()
    {
        try
        {
            var datos = await _estadisticasService.ProductosMasVendidosAsync(
                dtpDesde.Value.Date,
                dtpHasta.Value.Date);

            ConfigurarGrafico();
            chartEstadisticas.Titles.Add("Productos más vendidos");

            WinChart.Series serie = new WinChart.Series("Cantidad vendida")
            {
                ChartType = WinChart.SeriesChartType.Bar,
                IsValueShownAsLabel = true
            };

            foreach (var item in datos)
                serie.Points.AddXY(item.Producto, item.Cantidad);

            chartEstadisticas.Series.Add(serie);
            lblResumen.Text = $"Productos analizados: {datos.Count}";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar productos más vendidos:\n" + ex.Message,
                "Estadísticas", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task CargarVentasPorMedioPago()
    {
        try
        {
            var datos = await _estadisticasService.VentasPorMedioPagoAsync(
                dtpDesde.Value.Date,
                dtpHasta.Value.Date);

            ConfigurarGrafico();
            chartEstadisticas.Titles.Add("Ventas por medio de pago");

            WinChart.Series serie = new WinChart.Series("Medios de pago")
            {
                ChartType = WinChart.SeriesChartType.Pie,
                IsValueShownAsLabel = true
            };

            foreach (var item in datos)
                serie.Points.AddXY(item.MedioPago, item.Total);

            chartEstadisticas.Series.Add(serie);
            lblResumen.Text = $"Medios de pago: {datos.Count} | Total: {datos.Sum(x => x.Total):C2}";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar ventas por medio de pago:\n" + ex.Message,
                "Estadísticas", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task CargarProductosBajoStock()
    {
        try
        {
            var datos = await _estadisticasService.ProductosBajoStockAsync();

            ConfigurarGrafico();
            chartEstadisticas.Titles.Add("Productos con bajo stock");

            WinChart.Series serie = new WinChart.Series("Stock actual")
            {
                ChartType = WinChart.SeriesChartType.Column,
                IsValueShownAsLabel = true
            };

            foreach (var item in datos)
                serie.Points.AddXY(item.Producto, item.Stock);

            chartEstadisticas.Series.Add(serie);
            lblResumen.Text = $"Productos con bajo stock: {datos.Count}";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar productos con bajo stock:\n" + ex.Message,
                "Estadísticas", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnCerrar_Click(object sender, EventArgs e)
    {
        try
        {
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cerrar formulario:\n" + ex.Message,
                "Estadísticas", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
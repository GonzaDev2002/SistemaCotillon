using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Desktop.WinForms.Services;
using System.Drawing.Printing;
using System.Text;

namespace Cotillon.Desktop.WinForms.Forms;

public partial class FrmReportes : Form
{
    private readonly ReportesApiService _reportesApiService = new();
    private string _tituloReporte = "Reporte";
    private int _filaImpresion = 0;

    public FrmReportes()
    {
        InitializeComponent();

        try
        {
            AplicarDiseñoReportes();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al inicializar reportes:\n" + ex.Message,
                "Reportes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AplicarDiseñoReportes()
    {
        PopTheme.ApplyForm(this);

        Text = "POP Cotillón - Reportes";
        BackColor = Color.FromArgb(250, 248, 242);
        BackgroundImage = Properties.Resources.fondoForms;
        BackgroundImageLayout = ImageLayout.Stretch;

        pnlHeader.BackColor = PopTheme.Colors.Principal;
        lblTitulo.ForeColor = Color.White;
        lblTitulo.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        lblSubtitulo.ForeColor = Color.White;
        lblSubtitulo.Font = new Font("Segoe UI", 11F);

        PopTheme.ApplyGroupBox(grpFiltros);
        PopTheme.ApplyGroupBox(grpResultado);

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

        PopTheme.ApplyGrid(dgvReportes);

        PopTheme.ApplyPrimaryButton(btnGenerar);
        PopTheme.ApplySuccessButton(btnExportar);
        PopTheme.ApplyPrimaryButton(btnVistaPrevia);
        PopTheme.ApplySecondaryButton(btnCerrar);

        lblResultado.ForeColor = PopTheme.Colors.Secundario;
        lblResultado.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
    }

    private void FrmReportes_Load(object sender, EventArgs e)
    {
        try
        {
            cmbTipoReporte.Items.Clear();
            cmbTipoReporte.Items.Add("Productos");
            cmbTipoReporte.Items.Add("Productos bajo stock");
            cmbTipoReporte.Items.Add("Ventas por rango");
            cmbTipoReporte.SelectedIndex = 0;

            dtpDesde.Value = DateTime.Today.AddDays(-30);
            dtpHasta.Value = DateTime.Today;

            ConfigurarGrilla();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar formulario de reportes:\n" + ex.Message,
                "Reportes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ConfigurarGrilla()
    {
        try
        {
            dgvReportes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvReportes.ReadOnly = true;
            dgvReportes.AllowUserToAddRows = false;
            dgvReportes.AllowUserToDeleteRows = false;
            dgvReportes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReportes.MultiSelect = false;
            dgvReportes.RowHeadersVisible = false;
            dgvReportes.BackgroundColor = Color.White;
            dgvReportes.BorderStyle = BorderStyle.FixedSingle;
            dgvReportes.EnableHeadersVisualStyles = false;

            dgvReportes.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(79, 70, 229);
            dgvReportes.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvReportes.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            dgvReportes.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvReportes.DefaultCellStyle.SelectionBackColor = PopTheme.Colors.Principal;
            dgvReportes.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvReportes.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 252);

            dgvReportes.RowTemplate.Height = 28;
            dgvReportes.ColumnHeadersHeight = 32;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al configurar grilla:\n" + ex.Message,
                "Reportes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnGenerar_Click(object sender, EventArgs e)
    {
        try
        {
            if (cmbTipoReporte.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un tipo de reporte.");
                return;
            }

            string tipo = cmbTipoReporte.SelectedItem.ToString()!;

            dgvReportes.DataSource = null;

            if (tipo == "Productos")
            {
                var datos = await _reportesApiService.ListarProductosAsync();
                dgvReportes.DataSource = datos;
                _tituloReporte = "Reporte de productos";
                lblResultado.Text = $"Total de productos: {datos.Count}";
            }
            else if (tipo == "Productos bajo stock")
            {
                var datos = await _reportesApiService.ListarProductosBajoStockAsync();
                dgvReportes.DataSource = datos;
                _tituloReporte = "Reporte de productos bajo stock";
                lblResultado.Text = $"Productos bajo stock: {datos.Count}";
            }
            else if (tipo == "Ventas por rango")
            {
                if (dtpDesde.Value.Date > dtpHasta.Value.Date)
                {
                    MessageBox.Show("La fecha desde no puede ser mayor a la fecha hasta.");
                    return;
                }

                var datos = await _reportesApiService.ListarVentasPorRangoAsync(
                    dtpDesde.Value.Date,
                    dtpHasta.Value.Date);

                dgvReportes.DataSource = datos;
                _tituloReporte = "Reporte de ventas por rango";

                decimal total = datos.Sum(x => x.TotalVenta);
                lblResultado.Text = $"Ventas: {datos.Count} | Total vendido: {total:C2}";
            }

            FormatearGrilla();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al generar reporte:\n" + ex.Message,
                "Reportes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void FormatearGrilla()
    {
        try
        {
            if (dgvReportes.Columns.Contains("ProductoId"))
                dgvReportes.Columns["ProductoId"].Visible = false;

            if (dgvReportes.Columns.Contains("VentaId"))
                dgvReportes.Columns["VentaId"].Visible = false;

            if (dgvReportes.Columns.Contains("Precio"))
                dgvReportes.Columns["Precio"].DefaultCellStyle.Format = "C2";

            if (dgvReportes.Columns.Contains("TotalVenta"))
                dgvReportes.Columns["TotalVenta"].DefaultCellStyle.Format = "C2";

            if (dgvReportes.Columns.Contains("FechaVenta"))
                dgvReportes.Columns["FechaVenta"].DefaultCellStyle.Format = "dd/MM/yyyy";

            ConfigurarGrilla();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al formatear grilla:\n" + ex.Message,
                "Reportes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnExportar_Click(object sender, EventArgs e)
    {
        try
        {
            if (dgvReportes.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.");
                return;
            }

            SaveFileDialog dialogo = new SaveFileDialog
            {
                Title = "Exportar reporte",
                Filter = "Archivo CSV (*.csv)|*.csv",
                FileName = $"{_tituloReporte.Replace(" ", "_")}.csv"
            };

            if (dialogo.ShowDialog() == DialogResult.OK)
            {
                ExportarCsv(dialogo.FileName);
                MessageBox.Show("Reporte exportado correctamente.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al exportar reporte:\n" + ex.Message,
                "Reportes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportarCsv(string ruta)
    {
        try
        {
            StringBuilder sb = new StringBuilder();

            var columnasVisibles = dgvReportes.Columns
                .Cast<DataGridViewColumn>()
                .Where(c => c.Visible)
                .ToList();

            sb.AppendLine(string.Join(";", columnasVisibles.Select(c => c.HeaderText)));

            foreach (DataGridViewRow fila in dgvReportes.Rows)
            {
                if (!fila.IsNewRow)
                {
                    List<string> valores = new();

                    foreach (var columna in columnasVisibles)
                    {
                        string valor = fila.Cells[columna.Name].Value?.ToString() ?? "";
                        valor = valor.Replace(";", ",");
                        valores.Add(valor);
                    }

                    sb.AppendLine(string.Join(";", valores));
                }
            }

            File.WriteAllText(ruta, sb.ToString(), Encoding.UTF8);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al generar CSV:\n" + ex.Message,
                "Reportes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnVistaPrevia_Click(object sender, EventArgs e)
    {
        try
        {
            if (dgvReportes.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para imprimir.");
                return;
            }

            _filaImpresion = 0;

            PrintDocument documento = new PrintDocument();
            documento.PrintPage += Documento_PrintPage;

            PrintPreviewDialog vistaPrevia = new PrintPreviewDialog
            {
                Document = documento,
                Width = 1000,
                Height = 700
            };

            vistaPrevia.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al mostrar vista previa:\n" + ex.Message,
                "Reportes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Documento_PrintPage(object sender, PrintPageEventArgs e)
    {
        try
        {
            Font fuenteTitulo = new Font("Arial", 16, FontStyle.Bold);
            Font fuenteTexto = new Font("Arial", 9);
            Brush pincel = Brushes.Black;

            int x = 50;
            int y = 50;
            int altoLinea = 22;

            e.Graphics.DrawString(_tituloReporte, fuenteTitulo, pincel, x, y);
            y += 35;

            e.Graphics.DrawString(
                "Fecha de emisión: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                fuenteTexto,
                pincel,
                x,
                y);

            y += 30;

            foreach (DataGridViewColumn columna in dgvReportes.Columns)
            {
                if (columna.Visible)
                {
                    e.Graphics.DrawString(columna.HeaderText, fuenteTexto, pincel, x, y);
                    x += 120;
                }
            }

            y += altoLinea;
            x = 50;

            while (_filaImpresion < dgvReportes.Rows.Count)
            {
                DataGridViewRow fila = dgvReportes.Rows[_filaImpresion];

                if (!fila.IsNewRow)
                {
                    foreach (DataGridViewColumn columna in dgvReportes.Columns)
                    {
                        if (columna.Visible)
                        {
                            string valor = fila.Cells[columna.Name].Value?.ToString() ?? "";

                            if (valor.Length > 18)
                                valor = valor.Substring(0, 18);

                            e.Graphics.DrawString(valor, fuenteTexto, pincel, x, y);
                            x += 120;
                        }
                    }

                    y += altoLinea;
                    x = 50;
                }

                _filaImpresion++;

                if (y > e.MarginBounds.Bottom - 40)
                {
                    e.HasMorePages = true;
                    return;
                }
            }

            e.HasMorePages = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al imprimir reporte:\n" + ex.Message,
                "Reportes", MessageBoxButtons.OK, MessageBoxIcon.Error);

            e.HasMorePages = false;
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
                "Reportes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
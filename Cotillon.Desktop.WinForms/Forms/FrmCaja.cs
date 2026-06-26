using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Desktop.WinForms.Services;
using Cotillon.Shared.DTOs.Caja;

namespace Cotillon.Desktop.WinForms.Forms;

public partial class FrmCaja : Form
{
    private readonly CajaApiService _cajaService = new();
    private readonly int _usuarioId;
    private SesionCajaDto? _cajaActual;

    public FrmCaja(int usuarioId)
    {
        InitializeComponent();
        _usuarioId = usuarioId;

        try
        {
            AplicarDiseñoCaja();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al aplicar diseño de caja:\n" + ex.Message,
                "Caja", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AplicarDiseñoCaja()
    {
        PopTheme.ApplyForm(this);

        Text = "POP Cotillón - Caja";
        BackColor = Color.FromArgb(250, 248, 242);
        BackgroundImage = Properties.Resources.fondoForms;
        BackgroundImageLayout = ImageLayout.Stretch;

        pnlHeader.BackColor = PopTheme.Colors.Principal;

        lblTitulo.ForeColor = Color.White;
        lblTitulo.Font = new Font("Segoe UI", 22F, FontStyle.Bold);

        lblSubtitulo.ForeColor = Color.White;
        lblSubtitulo.Font = new Font("Segoe UI", 11F);

        PopTheme.ApplyGroupBox(grpCaja);

        foreach (Control control in grpCaja.Controls)
        {
            if (control is Label lbl)
            {
                lbl.ForeColor = PopTheme.Colors.Texto;
                lbl.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            }

            if (control is TextBox txt)
                PopTheme.ApplyInput(txt);
        }

        lblDiferencia.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        lblDiferencia.ForeColor = PopTheme.Colors.Secundario;

        PopTheme.ApplySuccessButton(btnAbrirCaja);
        PopTheme.ApplyDangerButton(btnCerrarCaja);
        PopTheme.ApplySecondaryButton(btnCerrar);
    }

    private async void FrmCaja_Load(object sender, EventArgs e)
    {
        try
        {
            await CargarCaja();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar caja:\n" + ex.Message,
                "Caja", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task CargarCaja()
    {
        try
        {
            _cajaActual = await _cajaService.ObtenerCajaAbiertaAsync(_usuarioId);

            if (_cajaActual == null)
            {
                lblEstado.Text = "Estado: Sin caja abierta";
                lblFechaApertura.Text = "Fecha apertura: -";
                lblMontoInicial.Text = "Monto inicial: $0,00";
                lblMontoEsperado.Text = "Monto esperado: $0,00";
                lblDiferencia.Text = "Diferencia: $0,00";

                txtMontoContado.Text = "";
                txtObservaciones.Text = "";

                btnCerrarCaja.Enabled = false;
                btnAbrirCaja.Enabled = true;
                return;
            }

            lblEstado.Text = "Estado: Caja abierta";
            lblFechaApertura.Text = $"Fecha apertura: {_cajaActual.FechaApertura:dd/MM/yyyy HH:mm}";
            lblMontoInicial.Text = $"Monto inicial: {_cajaActual.MontoInicial:C2}";
            lblMontoEsperado.Text = $"Monto esperado: {_cajaActual.MontoEsperado:C2}";
            lblDiferencia.Text = "Diferencia: $0,00";

            btnCerrarCaja.Enabled = true;
            btnAbrirCaja.Enabled = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al consultar la caja:\n" + ex.Message,
                "Caja", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnAbrirCaja_Click(object sender, EventArgs e)
    {
        try
        {
            btnAbrirCaja.Enabled = false;

            int sesionCajaId = await _cajaService.AbrirCajaAsync(_usuarioId, 0);

            if (sesionCajaId > 0)
            {
                MessageBox.Show("Caja abierta correctamente.");
                await CargarCaja();
            }
            else
            {
                MessageBox.Show("No se pudo abrir la caja.");
                btnAbrirCaja.Enabled = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al abrir caja:\n" + ex.Message,
                "Caja", MessageBoxButtons.OK, MessageBoxIcon.Error);

            btnAbrirCaja.Enabled = true;
        }
    }

    private async void btnCerrarCaja_Click(object sender, EventArgs e)
    {
        try
        {
            if (_cajaActual == null)
            {
                MessageBox.Show("No hay caja abierta.");
                return;
            }

            if (!decimal.TryParse(txtMontoContado.Text, out decimal montoContado))
            {
                MessageBox.Show("Ingrese un monto contado válido.");
                txtMontoContado.Focus();
                return;
            }

            DialogResult confirmar = MessageBox.Show(
                "¿Desea cerrar la caja actual?",
                "Cerrar caja",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmar != DialogResult.Yes)
                return;

            btnCerrarCaja.Enabled = false;

            bool ok = await _cajaService.CerrarCajaAsync(
                _cajaActual.SesionCajaId,
                montoContado,
                txtObservaciones.Text.Trim());

            if (ok)
            {
                MessageBox.Show("Caja cerrada correctamente.");
                Close();
            }
            else
            {
                MessageBox.Show("No se pudo cerrar la caja.");
                btnCerrarCaja.Enabled = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cerrar caja:\n" + ex.Message,
                "Caja", MessageBoxButtons.OK, MessageBoxIcon.Error);

            btnCerrarCaja.Enabled = true;
        }
    }

    private void txtMontoContado_TextChanged(object sender, EventArgs e)
    {
        try
        {
            if (_cajaActual == null)
                return;

            if (!decimal.TryParse(txtMontoContado.Text, out decimal montoContado))
            {
                lblDiferencia.Text = "Diferencia: $0,00";
                lblDiferencia.ForeColor = PopTheme.Colors.Secundario;
                return;
            }

            decimal diferencia = montoContado - _cajaActual.MontoEsperado;

            lblDiferencia.Text = $"Diferencia: {diferencia:C2}";

            if (diferencia == 0)
                lblDiferencia.ForeColor = PopTheme.Colors.Verde;
            else if (diferencia < 0)
                lblDiferencia.ForeColor = PopTheme.Colors.Rojo;
            else
                lblDiferencia.ForeColor = PopTheme.Colors.Amarillo;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al calcular diferencia:\n" + ex.Message,
                "Caja", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                "Caja", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
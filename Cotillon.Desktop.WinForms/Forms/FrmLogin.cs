using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Desktop.WinForms.Services;

namespace Cotillon.Desktop.WinForms.Forms;

public partial class FrmLogin : Form
{
    private readonly AuthApiService _authApiService;

    public FrmLogin()
    {
        InitializeComponent();

        _authApiService = new AuthApiService();

        AplicarDiseñoCotillon();
    }

    private void AplicarDiseñoCotillon()
    {
        PopTheme.ApplyForm(this);

        Text = "POP Cotillón - Iniciar sesión";

        pnlContenedor.BackColor = Color.White;
        pnlContenedor.BorderStyle = BorderStyle.None;

        lblTitulo.Text = "🎈 POP Cotillón";
        lblTitulo.ForeColor = PopTheme.Colors.Principal;
        lblTitulo.Font = new Font("Segoe UI", 24F, FontStyle.Bold);

        lblSubtitulo.Text = "Ingresá al sistema de gestión";
        lblSubtitulo.ForeColor = PopTheme.Colors.TextoSuave;
        lblSubtitulo.Font = new Font("Segoe UI", 10F);

        lblUsuario.ForeColor = PopTheme.Colors.Texto;
        lblUsuario.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

        lblPin.ForeColor = PopTheme.Colors.Texto;
        lblPin.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

        PopTheme.ApplyInput(txtUsuario);
        PopTheme.ApplyInput(txtPin);

        PopTheme.ApplyPrimaryButton(btnIngresar);
        PopTheme.ApplySecondaryButton(btnSalir);

        btnIngresar.Text = "Ingresar";
        btnSalir.Text = "Salir";

        lblFooter.Text = "Sistema administrativo POP Cotillón";
        lblFooter.ForeColor = PopTheme.Colors.TextoSuave;
        lblFooter.Font = new Font("Segoe UI", 8.5F);
    }

    private async void btnIngresar_Click(object sender, EventArgs e)
    {
        btnIngresar.Enabled = false;

        try
        {
            var username = txtUsuario.Text.Trim();
            var pin = txtPin.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Ingresá el usuario.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsuario.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(pin))
            {
                MessageBox.Show("Ingresá el PIN.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPin.Focus();
                return;
            }

            var result = await _authApiService.LoginAsync(username, pin);

            if (result == null)
            {
                MessageBox.Show("No se obtuvo respuesta del servidor.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPin.SelectAll();
                txtPin.Focus();
                return;
            }

            if (!result.Success || result.Data is null)
            {
                string mensaje = string.IsNullOrWhiteSpace(result.Message)
                    ? "Usuario o PIN incorrectos."
                    : result.Message;

                MessageBox.Show(mensaje, "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPin.SelectAll();
                txtPin.Focus();
                return;
            }

            var usuario = result.Data;

            CajaApiService cajaApi = new();
            int sesionCajaId = await cajaApi.ObtenerOCrearCajaAsync(usuario.UsuarioId);

            FrmMain frm = new FrmMain(usuario, sesionCajaId);
            frm.Show();

            Hide();
        }
        catch (Exception ex)
        {
            string mensajeError = string.IsNullOrWhiteSpace(ex.Message)
                ? "Ocurrió un error inesperado al iniciar sesión."
                : ex.Message;

            MessageBox.Show($"Error inesperado: {mensajeError}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnIngresar.Enabled = true;
        }
    }

    private void btnSalir_Click(object sender, EventArgs e)
    {
        Application.Exit();
    }

    private void txtPin_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            btnIngresar.PerformClick();
            e.SuppressKeyPress = true;
        }
    }

    private void FrmLogin_Load(object sender, EventArgs e)
    {
        txtUsuario.Focus();
    }
}
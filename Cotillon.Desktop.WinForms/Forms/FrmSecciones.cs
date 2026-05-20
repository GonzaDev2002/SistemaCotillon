using Cotillon.Desktop.WinForms.Services;
using Cotillon.Shared.DTOs;
using Cotillon.Desktop.WinForms.Helpers;

namespace Cotillon.Desktop.WinForms.Forms;

public partial class FrmSecciones : Form
{
    private readonly SeccionesApiService _service;
    private int _seccionIdSeleccionada = 0;

    public FrmSecciones()
    {
        InitializeComponent();
        _service = new SeccionesApiService();
        AplicarTema();
    }

    private async void FrmSecciones_Load(object sender, EventArgs e)
    {
        await CargarGrilla();
        LimpiarFormulario();
    }

    private async Task CargarGrilla()
    {
        var lista = await _service.ObtenerTodasAsync();

        dgvSecciones.AutoGenerateColumns = false;
        dgvSecciones.DataSource = null;
        dgvSecciones.DataSource = lista;
    }

    private async void btnGuardar_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtNombreSeccion.Text))
        {
            MessageBox.Show("Ingresá el nombre de la sección.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtNombreSeccion.Focus();
            return;
        }

        var dto = new SeccionDto
        {
            NombreSeccion = txtNombreSeccion.Text.Trim(),
            Activo = chkActivo.Checked
        };

        btnGuardar.Enabled = false;

        try
        {
            var result = await _service.GuardarAsync(dto);

            if (result.Success)
            {
                MessageBox.Show("Sección agregada con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await CargarGrilla();
                LimpiarFormulario();
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            btnGuardar.Enabled = true;
        }
    }

    private async void btnActualizar_Click(object sender, EventArgs e)
    {
        if (_seccionIdSeleccionada <= 0)
        {
            MessageBox.Show("Seleccioná una sección de la grilla para modificar.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(txtNombreSeccion.Text))
        {
            MessageBox.Show("Ingresá el nombre de la sección.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtNombreSeccion.Focus();
            return;
        }

        var dto = new SeccionDto
        {
            SeccionId = _seccionIdSeleccionada,
            NombreSeccion = txtNombreSeccion.Text.Trim(),
            Activo = chkActivo.Checked
        };

        btnActualizar.Enabled = false;

        try
        {
            var result = await _service.ActualizarAsync(dto);

            if (result.Success)
            {
                MessageBox.Show("Sección actualizada con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await CargarGrilla();
                LimpiarFormulario();
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            btnActualizar.Enabled = true;
        }
    }

    private void btnNuevo_Click(object sender, EventArgs e)
    {
        LimpiarFormulario();
    }

    private void dgvSecciones_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
            return;

        var fila = dgvSecciones.Rows[e.RowIndex];

        _seccionIdSeleccionada = Convert.ToInt32(fila.Cells["colSeccionId"].Value);
        txtNombreSeccion.Text = fila.Cells["colNombreSeccion"].Value?.ToString() ?? string.Empty;
        chkActivo.Checked = Convert.ToBoolean(fila.Cells["colActivo"].Value);
    }

    private void LimpiarFormulario()
    {
        _seccionIdSeleccionada = 0;
        txtNombreSeccion.Clear();
        chkActivo.Checked = true;
        txtNombreSeccion.Focus();
        dgvSecciones.ClearSelection();
    }
    private void AplicarTema()
    {
        Helpers.AppTheme.ApplyFormStyle(this);

        lblTitulo.ForeColor = Helpers.AppTheme.Colors.Primary;
        lblNombreSeccion.ForeColor = Helpers.AppTheme.Colors.TextPrimary;

        Helpers.AppTheme.ApplyTextBox(txtNombreSeccion);
        Helpers.AppTheme.ApplyCheckBox(chkActivo);

        Helpers.AppTheme.ApplyPrimaryButton(btnGuardar);
        Helpers.AppTheme.ApplySecondaryButton(btnActualizar);
        Helpers.AppTheme.ApplySecondaryButton(btnLimpiarCampos);

        Helpers.AppTheme.ApplyGrid(dgvSecciones);
    }
}
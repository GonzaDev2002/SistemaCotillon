using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Desktop.WinForms.Services;
using Cotillon.Shared.DTOs;

namespace Cotillon.Desktop.WinForms.Forms;

public partial class FrmSecciones : Form
{
    private readonly SeccionesApiService _service;
    private int _seccionIdSeleccionada = 0;

    public FrmSecciones()
    {
        InitializeComponent();

        _service = new SeccionesApiService();

        try
        {
            AplicarTemaVisual();

            PopTheme.ApplySuccessButton(btnGuardar);
            PopTheme.ApplyPrimaryButton(btnActualizar);
            PopTheme.ApplySecondaryButton(btnLimpiarCampos);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al inicializar secciones:\n" + ex.Message,
                "Secciones", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void FrmSecciones_Load(object sender, EventArgs e)
    {
        try
        {
            await CargarGrilla();
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar secciones:\n" + ex.Message,
                "Secciones", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task CargarGrilla()
    {
        try
        {
            var lista = await _service.ObtenerTodasAsync();

            dgvSecciones.AutoGenerateColumns = false;
            dgvSecciones.DataSource = null;
            dgvSecciones.DataSource = lista;
            dgvSecciones.ClearSelection();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al listar secciones:\n" + ex.Message,
                "Secciones", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnGuardar_Click(object sender, EventArgs e)
    {
        try
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
        catch (Exception ex)
        {
            MessageBox.Show("Error al guardar sección:\n" + ex.Message,
                "Secciones", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnGuardar.Enabled = true;
        }
    }

    private async void btnActualizar_Click(object sender, EventArgs e)
    {
        try
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
        catch (Exception ex)
        {
            MessageBox.Show("Error al actualizar sección:\n" + ex.Message,
                "Secciones", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnActualizar.Enabled = true;
        }
    }

    private void btnNuevo_Click(object sender, EventArgs e)
    {
        try
        {
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al limpiar formulario:\n" + ex.Message,
                "Secciones", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void dgvSecciones_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        try
        {
            if (e.RowIndex < 0)
                return;

            var fila = dgvSecciones.Rows[e.RowIndex];

            if (fila.Cells["colSeccionId"].Value == null)
                return;

            _seccionIdSeleccionada = Convert.ToInt32(fila.Cells["colSeccionId"].Value);
            txtNombreSeccion.Text = fila.Cells["colNombreSeccion"].Value?.ToString() ?? string.Empty;
            chkActivo.Checked = Convert.ToBoolean(fila.Cells["colActivo"].Value);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al seleccionar sección:\n" + ex.Message,
                "Secciones", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LimpiarFormulario()
    {
        try
        {
            _seccionIdSeleccionada = 0;
            txtNombreSeccion.Clear();
            chkActivo.Checked = true;

            dgvSecciones.ClearSelection();
            txtNombreSeccion.Focus();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al limpiar sección:\n" + ex.Message,
                "Secciones", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
}
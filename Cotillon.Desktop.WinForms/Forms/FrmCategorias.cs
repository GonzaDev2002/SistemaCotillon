using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Desktop.WinForms.Services;
using Cotillon.Shared.DTOs;

namespace Cotillon.Desktop.WinForms.Forms;

public partial class FrmCategorias : Form
{
    private readonly CategoriasApiService _categoriasService;
    private readonly SeccionesApiService _seccionesService;
    private int _categoriaIdSeleccionada = 0;

    public FrmCategorias()
    {
        InitializeComponent();

        _categoriasService = new CategoriasApiService();
        _seccionesService = new SeccionesApiService();

        try
        {
            AplicarTemaVisual();

            PopTheme.ApplySuccessButton(btnGuardar);
            PopTheme.ApplyPrimaryButton(btnActualizar);
            PopTheme.ApplySecondaryButton(btnBuscar);
            PopTheme.ApplySecondaryButton(btnLimpiar);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al inicializar categorías:\n" + ex.Message,
                "Categorías", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void FrmCategorias_Load(object sender, EventArgs e)
    {
        try
        {
            await CargarSecciones();
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar el formulario:\n" + ex.Message,
                "Categorías", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task CargarSecciones()
    {
        try
        {
            var lista = await _seccionesService.ObtenerTodasAsync();

            cmbSeccion.DataSource = null;
            cmbSeccion.DisplayMember = "NombreSeccion";
            cmbSeccion.ValueMember = "SeccionId";
            cmbSeccion.DataSource = lista;
            cmbSeccion.SelectedIndex = -1;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar secciones:\n" + ex.Message,
                "Categorías", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task BuscarCategoriasPorSeccion()
    {
        try
        {
            if (cmbSeccion.SelectedValue == null)
            {
                dgvCategorias.DataSource = null;
                return;
            }

            if (!int.TryParse(cmbSeccion.SelectedValue.ToString(), out int seccionId))
            {
                dgvCategorias.DataSource = null;
                return;
            }

            var lista = await _categoriasService.GetPorSeccionAsync(seccionId);

            dgvCategorias.AutoGenerateColumns = false;
            dgvCategorias.DataSource = null;
            dgvCategorias.DataSource = lista;
            dgvCategorias.ClearSelection();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al buscar categorías:\n" + ex.Message,
                "Categorías", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnBuscar_Click(object sender, EventArgs e)
    {
        try
        {
            if (cmbSeccion.SelectedValue == null)
            {
                MessageBox.Show("Seleccioná una sección para buscar.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbSeccion.Focus();
                return;
            }

            await BuscarCategoriasPorSeccion();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al buscar:\n" + ex.Message,
                "Categorías", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnGuardar_Click(object sender, EventArgs e)
    {
        try
        {
            if (cmbSeccion.SelectedValue == null)
            {
                MessageBox.Show("Seleccioná una sección.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbSeccion.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNombreCategoria.Text))
            {
                MessageBox.Show("Ingresá el nombre de la categoría.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombreCategoria.Focus();
                return;
            }

            var dto = new CategoriaDto
            {
                SeccionId = Convert.ToInt32(cmbSeccion.SelectedValue),
                NombreCategoria = txtNombreCategoria.Text.Trim(),
                Activo = chkActivo.Checked
            };

            btnGuardar.Enabled = false;

            var result = await _categoriasService.GuardarAsync(dto);

            if (result.Success)
            {
                MessageBox.Show("Categoría agregada con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await BuscarCategoriasPorSeccion();
                LimpiarFormulario(false);
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al guardar categoría:\n" + ex.Message,
                "Categorías", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (_categoriaIdSeleccionada <= 0)
            {
                MessageBox.Show("Seleccioná una categoría de la grilla para modificar.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbSeccion.SelectedValue == null)
            {
                MessageBox.Show("Seleccioná una sección.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbSeccion.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNombreCategoria.Text))
            {
                MessageBox.Show("Ingresá el nombre de la categoría.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombreCategoria.Focus();
                return;
            }

            var dto = new CategoriaDto
            {
                CategoriaId = _categoriaIdSeleccionada,
                SeccionId = Convert.ToInt32(cmbSeccion.SelectedValue),
                NombreCategoria = txtNombreCategoria.Text.Trim(),
                Activo = chkActivo.Checked
            };

            btnActualizar.Enabled = false;

            var result = await _categoriasService.ActualizarAsync(dto);

            if (result.Success)
            {
                MessageBox.Show("Categoría actualizada con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await BuscarCategoriasPorSeccion();
                LimpiarFormulario(false);
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al actualizar categoría:\n" + ex.Message,
                "Categorías", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnActualizar.Enabled = true;
        }
    }

    private void btnLimpiar_Click(object sender, EventArgs e)
    {
        try
        {
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al limpiar:\n" + ex.Message,
                "Categorías", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void dgvCategorias_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        try
        {
            if (e.RowIndex < 0)
                return;

            var fila = dgvCategorias.Rows[e.RowIndex];

            _categoriaIdSeleccionada = Convert.ToInt32(fila.Cells["colCategoriaId"].Value);
            txtNombreCategoria.Text = fila.Cells["colNombreCategoria"].Value?.ToString() ?? string.Empty;
            chkActivo.Checked = Convert.ToBoolean(fila.Cells["colActivo"].Value);

            int seccionId = Convert.ToInt32(fila.Cells["colSeccionId"].Value);
            cmbSeccion.SelectedValue = seccionId;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al seleccionar categoría:\n" + ex.Message,
                "Categorías", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LimpiarFormulario(bool limpiarSeccion = true)
    {
        try
        {
            _categoriaIdSeleccionada = 0;
            txtNombreCategoria.Clear();
            chkActivo.Checked = true;

            if (limpiarSeccion)
                cmbSeccion.SelectedIndex = -1;

            dgvCategorias.ClearSelection();
            txtNombreCategoria.Focus();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al limpiar formulario:\n" + ex.Message,
                "Categorías", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
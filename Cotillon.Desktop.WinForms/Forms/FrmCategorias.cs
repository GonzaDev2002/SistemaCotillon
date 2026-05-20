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
        AplicarTema();
    }

    private async void FrmCategorias_Load(object sender, EventArgs e)
    {
        await CargarSecciones();
        LimpiarFormulario();
    }

    private async Task CargarSecciones()
    {
        var lista = await _seccionesService.ObtenerTodasAsync();

        cmbSeccion.DataSource = null;
        cmbSeccion.DisplayMember = "NombreSeccion";
        cmbSeccion.ValueMember = "SeccionId";
        cmbSeccion.DataSource = lista;
        cmbSeccion.SelectedIndex = -1;
    }

    private async Task BuscarCategoriasPorSeccion()
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
    }

    private async void btnBuscar_Click(object sender, EventArgs e)
    {
        if (cmbSeccion.SelectedValue == null)
        {
            MessageBox.Show("Seleccioná una sección para buscar.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            cmbSeccion.Focus();
            return;
        }

        await BuscarCategoriasPorSeccion();
    }

    private async void btnGuardar_Click(object sender, EventArgs e)
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

        try
        {
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
        finally
        {
            btnGuardar.Enabled = true;
        }
    }

    private async void btnActualizar_Click(object sender, EventArgs e)
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

        try
        {
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
        finally
        {
            btnActualizar.Enabled = true;
        }
    }

    private void btnLimpiar_Click(object sender, EventArgs e)
    {
        LimpiarFormulario();
    }

    private void dgvCategorias_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
            return;

        var fila = dgvCategorias.Rows[e.RowIndex];

        _categoriaIdSeleccionada = Convert.ToInt32(fila.Cells["colCategoriaId"].Value);
        txtNombreCategoria.Text = fila.Cells["colNombreCategoria"].Value?.ToString() ?? string.Empty;
        chkActivo.Checked = Convert.ToBoolean(fila.Cells["colActivo"].Value);

        var seccionId = Convert.ToInt32(fila.Cells["colSeccionId"].Value);
        cmbSeccion.SelectedValue = seccionId;
    }

    private void LimpiarFormulario(bool limpiarSeccion = true)
    {
        _categoriaIdSeleccionada = 0;
        txtNombreCategoria.Clear();
        chkActivo.Checked = true;

        if (limpiarSeccion)
            cmbSeccion.SelectedIndex = -1;

        txtNombreCategoria.Focus();
        dgvCategorias.ClearSelection();
    }
    private void AplicarTema()
    {
        Helpers.AppTheme.ApplyFormStyle(this);

        lblTitulo.ForeColor = Helpers.AppTheme.Colors.Secondary;

        Helpers.AppTheme.ApplyComboBox(cmbSeccion);
        Helpers.AppTheme.ApplyTextBox(txtNombreCategoria);
        Helpers.AppTheme.ApplyCheckBox(chkActivo);

        Helpers.AppTheme.ApplySecondaryButton(btnBuscar);
        Helpers.AppTheme.ApplyPrimaryButton(btnGuardar);
        Helpers.AppTheme.ApplySecondaryButton(btnActualizar);
        Helpers.AppTheme.ApplySecondaryButton(btnLimpiar);

        Helpers.AppTheme.ApplyGrid(dgvCategorias);
    }
}
using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Desktop.WinForms.Services;
using Cotillon.Shared.DTOs;

namespace Cotillon.Desktop.WinForms.Forms;

public partial class FrmProductos : Form
{
    private readonly SeccionesApiService _seccionesService;
    private readonly CategoriasApiService _categoriasService;
    private readonly ProductosApiService _productosService;

    private int _productoIdSeleccionado = 0;

    public FrmProductos()
    {
        InitializeComponent();
        _seccionesService = new SeccionesApiService();
        _categoriasService = new CategoriasApiService();
        _productosService = new ProductosApiService();
        AplicarTema();
    }

    private async void FrmProductos_Load(object sender, EventArgs e)
    {
        await CargarSecciones();
        await CargarGrillaProductos();
    }
    private async void chkVerEliminados_CheckedChanged(object sender, EventArgs e)
    {
        await CargarGrillaProductos();
    }

    private async Task CargarSecciones()
    {
        var lista = await _seccionesService.ObtenerTodasAsync();

        cmbSeccion.DataSource = null;
        cmbSeccion.DisplayMember = "NombreSeccion";
        cmbSeccion.ValueMember = "SeccionId";
        cmbSeccion.DataSource = lista;
        cmbSeccion.SelectedIndex = -1;

        cmbCategoria.DataSource = null;
        cmbCategoria.Items.Clear();
        cmbCategoria.Text = string.Empty;
    }

    private async void cmbSeccion_SelectionChangeCommitted(object sender, EventArgs e)
    {
        if (cmbSeccion.SelectedValue == null)
            return;

        if (!int.TryParse(cmbSeccion.SelectedValue.ToString(), out int seccionId))
            return;

        var categorias = await _categoriasService.GetPorSeccionAsync(seccionId);

        cmbCategoria.DataSource = null;
        cmbCategoria.DisplayMember = "NombreCategoria";
        cmbCategoria.ValueMember = "CategoriaId";
        cmbCategoria.DataSource = categorias;
        cmbCategoria.SelectedIndex = -1;
    }

    private async Task CargarGrillaProductos()
    {
        var lista = await _productosService.ObtenerTodosAsync(chkVerEliminados.Checked);

        dgvProductos.AutoGenerateColumns = false;
        dgvProductos.DataSource = null;
        dgvProductos.DataSource = lista;
        dgvProductos.ClearSelection();
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

            if (cmbCategoria.SelectedValue == null)
            {
                MessageBox.Show("Seleccioná una categoría.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCategoria.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Ingresá el nombre del producto.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return;
            }

            if (!decimal.TryParse(txtPrecio.Text.Trim(), out decimal precio))
            {
                MessageBox.Show("Ingresá un precio válido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrecio.Focus();
                return;
            }

            if (!int.TryParse(txtStock.Text.Trim(), out int stock))
            {
                MessageBox.Show("Ingresá un stock válido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStock.Focus();
                return;
            }

            var dto = new ProductoDto
            {
                CategoriaId = Convert.ToInt32(cmbCategoria.SelectedValue),
                CodigoProducto = "",
                NombreProducto = txtNombre.Text.Trim(),
                Descripcion = "",
                Precio = precio,
                Stock = stock,
                StockMinimo = 0,
                CodigoBarras = "",
                Marca = "",
                Costo = 0,
                ImagenUrl = "",
                UnidadMedida = "",
                PermiteVentaSinStock = false
            };

            btnGuardar.Enabled = false;

            var resultado = await _productosService.GuardarAsync(dto);

            if (resultado.Success)
            {
                MessageBox.Show("Producto agregado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarFormulario();
                await CargarGrillaProductos();
            }
            else
            {
                MessageBox.Show(resultado.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ocurrió un error al agregar el producto: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnGuardar.Enabled = true;
        }
    }

    private async void btnRefrescar_Click(object sender, EventArgs e)
    {
        await CargarGrillaProductos();
    }

    private async void btnEliminar_Click(object sender, EventArgs e)
    {
        if (_productoIdSeleccionado <= 0)
        {
            MessageBox.Show("Seleccioná un producto de la grilla.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirmar = MessageBox.Show(
            "¿Querés eliminar este producto?",
            "Confirmar eliminación",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirmar != DialogResult.Yes)
            return;

        var resultado = await _productosService.EliminarAsync(_productoIdSeleccionado);

        if (resultado.Success)
        {
            MessageBox.Show("Producto eliminado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
            await CargarGrillaProductos();
        }
        else
        {
            MessageBox.Show(resultado.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void dgvProductos_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
            return;

        var fila = dgvProductos.Rows[e.RowIndex];
        _productoIdSeleccionado = Convert.ToInt32(fila.Cells["colProductoId"].Value);
    }

    private void LimpiarFormulario()
    {
        _productoIdSeleccionado = 0;
        cmbSeccion.SelectedIndex = -1;
        cmbCategoria.DataSource = null;
        cmbCategoria.Items.Clear();
        cmbCategoria.Text = string.Empty;
        txtNombre.Clear();
        txtPrecio.Clear();
        txtStock.Clear();
        cmbSeccion.Focus();
        dgvProductos.ClearSelection();
    }
    private void AplicarTema()
    {
        Helpers.AppTheme.ApplyFormStyle(this);

        lblTitulo.ForeColor = Helpers.AppTheme.Colors.Secondary;

        Helpers.AppTheme.ApplyComboBox(cmbSeccion);
        Helpers.AppTheme.ApplyTextBox(txtNombre);
        Helpers.AppTheme.ApplyCheckBox(chkVerEliminados);

        Helpers.AppTheme.ApplySecondaryButton(btnRefrescar);
        Helpers.AppTheme.ApplyPrimaryButton(btnGuardar);
        Helpers.AppTheme.ApplySecondaryButton(btnRefrescar);
        Helpers.AppTheme.ApplySecondaryButton(btnEliminar);

        Helpers.AppTheme.ApplyGrid(dgvProductos);
    }
}
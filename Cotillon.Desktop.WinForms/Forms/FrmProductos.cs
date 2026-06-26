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

        try
        {
            AplicarDiseñoProductos();
            ConfigurarGrillaProductos();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al inicializar productos:\n" + ex.Message,
                "Productos", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AplicarDiseñoProductos()
    {
        PopTheme.ApplyForm(this);

        Text = "POP Cotillón - Productos";
        BackColor = Color.FromArgb(250, 248, 242);
        BackgroundImage = Properties.Resources.fondoForms;
        BackgroundImageLayout = ImageLayout.Stretch;

        pnlHeader.BackColor = PopTheme.Colors.Principal;
        lblTitulo.ForeColor = Color.White;
        lblTitulo.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        lblSubtitulo.ForeColor = Color.White;
        lblSubtitulo.Font = new Font("Segoe UI", 11F);

        PopTheme.ApplyGroupBox(grpDatos);
        PopTheme.ApplyGroupBox(grpListado);

        foreach (Control control in grpDatos.Controls)
        {
            if (control is Label lbl)
            {
                lbl.ForeColor = PopTheme.Colors.Texto;
                lbl.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            }

            if (control is TextBox txt)
                PopTheme.ApplyInput(txt);

            if (control is ComboBox cmb)
                PopTheme.ApplyCombo(cmb);
        }

        PopTheme.ApplyGrid(dgvProductos);

        PopTheme.ApplySuccessButton(btnGuardar);
        PopTheme.ApplyPrimaryButton(btnRefrescar);
        PopTheme.ApplyDangerButton(btnEliminar);
        PopTheme.ApplySecondaryButton(btnLimpiar);

        btnGuardar.Text = "Guardar producto";
        btnRefrescar.Text = "Refrescar";
        btnEliminar.Text = "Eliminar";
        btnLimpiar.Text = "Limpiar";

        chkVerEliminados.ForeColor = PopTheme.Colors.Texto;
        chkVerEliminados.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
    }

    private async void FrmProductos_Load(object sender, EventArgs e)
    {
        try
        {
            ConfigurarGrillaProductos();

            await CargarSecciones();
            await CargarGrillaProductos();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar productos:\n" + ex.Message,
                "Productos", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void chkVerEliminados_CheckedChanged(object sender, EventArgs e)
    {
        try
        {
            await CargarGrillaProductos();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al actualizar listado:\n" + ex.Message,
                "Productos", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            cmbCategoria.DataSource = null;
            cmbCategoria.Items.Clear();
            cmbCategoria.Text = string.Empty;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar secciones:\n" + ex.Message,
                "Productos", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void cmbSeccion_SelectionChangeCommitted(object sender, EventArgs e)
    {
        try
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
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar categorías:\n" + ex.Message,
                "Productos", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ConfigurarGrillaProductos()
    {
        try
        {
            dgvProductos.AutoGenerateColumns = false;
            dgvProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProductos.RowHeadersVisible = false;
            dgvProductos.AllowUserToAddRows = false;
            dgvProductos.AllowUserToDeleteRows = false;
            dgvProductos.ReadOnly = true;
            dgvProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProductos.MultiSelect = false;
            dgvProductos.ScrollBars = ScrollBars.Both;
            dgvProductos.BackgroundColor = Color.White;
            dgvProductos.BorderStyle = BorderStyle.FixedSingle;
            dgvProductos.EnableHeadersVisualStyles = false;

            dgvProductos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(79, 70, 229);
            dgvProductos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProductos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            dgvProductos.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvProductos.DefaultCellStyle.SelectionBackColor = PopTheme.Colors.Principal;
            dgvProductos.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvProductos.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 252);

            dgvProductos.RowTemplate.Height = 28;
            dgvProductos.ColumnHeadersHeight = 32;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al configurar grilla de productos:\n" + ex.Message,
                "Productos", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task CargarGrillaProductos()
    {
        try
        {
            var lista = await _productosService.ObtenerTodosAsync(chkVerEliminados.Checked);

            dgvProductos.DataSource = null;
            dgvProductos.DataSource = lista;
            dgvProductos.ClearSelection();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar grilla de productos:\n" + ex.Message,
                "Productos", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            MessageBox.Show("Ocurrió un error al agregar el producto:\n" + ex.Message,
                "Productos", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnGuardar.Enabled = true;
        }
    }

    private async void btnRefrescar_Click(object sender, EventArgs e)
    {
        try
        {
            await CargarGrillaProductos();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al refrescar productos:\n" + ex.Message,
                "Productos", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnEliminar_Click(object sender, EventArgs e)
    {
        try
        {
            if (_productoIdSeleccionado <= 0)
            {
                MessageBox.Show("Seleccioná un producto de la grilla.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirmar = MessageBox.Show(
                "¿Querés eliminar este producto?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmar != DialogResult.Yes)
                return;

            btnEliminar.Enabled = false;

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
        catch (Exception ex)
        {
            MessageBox.Show("Error al eliminar producto:\n" + ex.Message,
                "Productos", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnEliminar.Enabled = true;
        }
    }

    private void dgvProductos_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        try
        {
            if (e.RowIndex < 0)
                return;

            var fila = dgvProductos.Rows[e.RowIndex];

            if (fila.Cells["colProductoId"].Value == null)
                return;

            _productoIdSeleccionado = Convert.ToInt32(fila.Cells["colProductoId"].Value);

            txtNombre.Text = fila.Cells["colNombreProducto"].Value?.ToString() ?? "";
            txtPrecio.Text = fila.Cells["colPrecio"].Value?.ToString() ?? "";
            txtStock.Text = fila.Cells["colStock"].Value?.ToString() ?? "";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al seleccionar producto:\n" + ex.Message,
                "Productos", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            MessageBox.Show("Error al limpiar formulario:\n" + ex.Message,
                "Productos", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LimpiarFormulario()
    {
        try
        {
            _productoIdSeleccionado = 0;

            cmbSeccion.SelectedIndex = -1;
            cmbCategoria.DataSource = null;
            cmbCategoria.Items.Clear();
            cmbCategoria.Text = string.Empty;

            txtNombre.Clear();
            txtPrecio.Clear();
            txtStock.Clear();

            dgvProductos.ClearSelection();
            cmbSeccion.Focus();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al limpiar producto:\n" + ex.Message,
                "Productos", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
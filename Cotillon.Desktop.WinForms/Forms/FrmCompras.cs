using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Desktop.WinForms.Services;
using Cotillon.Shared.DTOs;
using Cotillon.Shared.DTOs.Compras;
using Cotillon.Shared.DTOs.Proveedores;

namespace Cotillon.Desktop.WinForms.Forms;

public partial class FrmCompras : Form
{
    private readonly ComprasApiService _comprasService = new();
    private readonly ProveedoresApiService _proveedoresService = new();
    private readonly ProductosApiService _productosService = new();

    private readonly int _usuarioId;
    private readonly List<CompraDetalleRequestDto> _detalles = new();
    private List<ProductoListadoDto> _productos = new();
    private ProductoListadoDto? _productoSeleccionado;

    public FrmCompras(int usuarioId)
    {
        InitializeComponent();
        _usuarioId = usuarioId;

        try
        {
            AplicarDiseñoCompras();
            ConfigurarGrillaDetalle();
            ConfigurarGrillaCompras();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al iniciar el formulario de compras:\n" + ex.Message, "Compras", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void FrmCompras_Load(object sender, EventArgs e)
    {
        try
        {
            ConfigurarGrillaDetalle();
            ConfigurarGrillaCompras();

            await CargarProveedores();
            await CargarProductos();
            await CargarCompras();

            txtCantidad.Text = "1";
            txtCostoUnitario.Text = "0";
            txtNumeroComprobante.Text = DateTime.Now.ToString("yyyyMMddHHmmss");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar compras:\n" + ex.Message, "Compras", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AplicarDiseñoCompras()
    {
        PopTheme.ApplyForm(this);

        Text = "POP Cotillón - Compras";
        BackColor = Color.FromArgb(250, 248, 242);
        BackgroundImage = Properties.Resources.fondoForms;
        BackgroundImageLayout = ImageLayout.Stretch;

        pnlHeader.BackColor = PopTheme.Colors.Principal;
        lblTitulo.ForeColor = Color.White;
        lblTitulo.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        lblSubtitulo.ForeColor = Color.White;
        lblSubtitulo.Font = new Font("Segoe UI", 11F);

        PopTheme.ApplyGroupBox(grpProveedor);
        PopTheme.ApplyGroupBox(grpProductos);
        PopTheme.ApplyGroupBox(grpDetalle);
        PopTheme.ApplyGroupBox(grpCompras);

        PopTheme.ApplyCombo(cmbProveedores);
        PopTheme.ApplyInput(txtBuscarProducto);
        PopTheme.ApplyInput(txtCantidad);
        PopTheme.ApplyInput(txtCostoUnitario);
        PopTheme.ApplyInput(txtNumeroComprobante);
        PopTheme.ApplyInput(txtObservaciones);
        PopTheme.ApplyListBox(lstProductos);

        PopTheme.ApplySecondaryButton(btnNuevoProveedor);
        PopTheme.ApplyPrimaryButton(btnAgregarProducto);
        PopTheme.ApplyDangerButton(btnQuitarProducto);
        PopTheme.ApplySuccessButton(btnRegistrarCompra);
        PopTheme.ApplySecondaryButton(btnCerrar);

        lblTotal.ForeColor = PopTheme.Colors.Principal;
        lblTotal.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
        lblSubtotal.ForeColor = PopTheme.Colors.Secundario;
        lblSubtotal.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
    }

    private void ConfigurarGrillaDetalle()
    {
        dgvDetalle.DataSource = null;
        dgvDetalle.Columns.Clear();
        dgvDetalle.AutoGenerateColumns = false;
        dgvDetalle.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        dgvDetalle.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colDetalleProductoId",
            HeaderText = "ProductoId",
            DataPropertyName = "ProductoId",
            Visible = false
        });

        dgvDetalle.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colDetalleProducto",
            HeaderText = "Producto",
            DataPropertyName = "Producto",
            FillWeight = 170
        });

        dgvDetalle.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colDetalleCantidad",
            HeaderText = "Cantidad",
            DataPropertyName = "Cantidad",
            FillWeight = 70
        });

        dgvDetalle.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colDetalleCosto",
            HeaderText = "Costo",
            DataPropertyName = "CostoUnitario",
            DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" },
            FillWeight = 90
        });

        dgvDetalle.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colDetalleSubtotal",
            HeaderText = "Subtotal",
            DataPropertyName = "SubtotalLinea",
            DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" },
            FillWeight = 100
        });

        GridHelper.AplicarEstilo(dgvDetalle);
    }

    private void ConfigurarGrillaCompras()
    {
        dgvCompras.DataSource = null;
        dgvCompras.Columns.Clear();
        dgvCompras.AutoGenerateColumns = false;
        dgvCompras.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        dgvCompras.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colCompraFecha",
            HeaderText = "Fecha",
            DataPropertyName = "FechaCompra",
            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" },
            FillWeight = 125
        });

        dgvCompras.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colCompraProveedor",
            HeaderText = "Proveedor",
            DataPropertyName = "Proveedor",
            FillWeight = 150
        });

        dgvCompras.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colCompraUsuario",
            HeaderText = "Usuario",
            DataPropertyName = "Usuario",
            FillWeight = 120
        });

        dgvCompras.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colCompraTotal",
            HeaderText = "Total",
            DataPropertyName = "TotalCompra",
            DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" },
            FillWeight = 90
        });

        dgvCompras.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colCompraEstado",
            HeaderText = "Estado",
            DataPropertyName = "Estado",
            FillWeight = 95
        });

        GridHelper.AplicarEstilo(dgvCompras);
    }

    private async Task CargarProveedores()
    {
        try
        {
            var proveedores = await _proveedoresService.ListarAsync();
            cmbProveedores.DataSource = null;
            cmbProveedores.DisplayMember = "NombreProveedor";
            cmbProveedores.ValueMember = "ProveedorId";
            cmbProveedores.DataSource = proveedores;
            cmbProveedores.SelectedIndex = proveedores.Count > 0 ? 0 : -1;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar proveedores:\n" + ex.Message, "Compras", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task CargarProductos()
    {
        try
        {
            _productos = await _productosService.ObtenerTodosAsync(false);
            FiltrarProductos();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar productos:\n" + ex.Message, "Compras", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task CargarCompras()
    {
        try
        {
            var compras = await _comprasService.ListarAsync();
            dgvCompras.DataSource = null;
            dgvCompras.DataSource = compras;
            dgvCompras.ClearSelection();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar compras registradas:\n" + ex.Message, "Compras", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void FiltrarProductos()
    {
        try
        {
            string filtro = txtBuscarProducto.Text.Trim().ToLower();
            var filtrados = string.IsNullOrWhiteSpace(filtro)
                ? _productos
                : _productos.Where(x => (x.NombreProducto ?? "").ToLower().Contains(filtro)).ToList();

            lstProductos.DataSource = null;
            lstProductos.DisplayMember = "NombreProducto";
            lstProductos.ValueMember = "ProductoId";
            lstProductos.DataSource = filtrados;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al filtrar productos:\n" + ex.Message, "Compras", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void txtBuscarProducto_TextChanged(object sender, EventArgs e)
    {
        FiltrarProductos();
    }

    private void lstProductos_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            _productoSeleccionado = lstProductos.SelectedItem as ProductoListadoDto;
            if (_productoSeleccionado == null)
                return;

            txtCostoUnitario.Text = _productoSeleccionado.Precio.ToString("N2");
            CalcularSubtotalActual();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al seleccionar producto:\n" + ex.Message, "Compras", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void txtCantidad_TextChanged(object sender, EventArgs e)
    {
        CalcularSubtotalActual();
    }

    private void txtCostoUnitario_TextChanged(object sender, EventArgs e)
    {
        CalcularSubtotalActual();
    }

    private void CalcularSubtotalActual()
    {
        try
        {
            if (!int.TryParse(txtCantidad.Text, out int cantidad))
                cantidad = 0;

            if (!decimal.TryParse(txtCostoUnitario.Text, out decimal costo))
                costo = 0;

            lblSubtotal.Text = $"Subtotal: {cantidad * costo:C2}";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al calcular subtotal:\n" + ex.Message, "Compras", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnAgregarProducto_Click(object sender, EventArgs e)
    {
        try
        {
            if (_productoSeleccionado == null)
            {
                MessageBox.Show("Seleccione un producto.");
                return;
            }

            if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Ingrese una cantidad válida.");
                txtCantidad.Focus();
                return;
            }

            if (!decimal.TryParse(txtCostoUnitario.Text, out decimal costo) || costo <= 0)
            {
                MessageBox.Show("Ingrese un costo unitario válido.");
                txtCostoUnitario.Focus();
                return;
            }

            var existente = _detalles.FirstOrDefault(x => x.ProductoId == _productoSeleccionado.ProductoId);

            if (existente != null)
            {
                existente.Cantidad += cantidad;
                existente.CostoUnitario = costo;
                existente.SubtotalLinea = existente.Cantidad * existente.CostoUnitario;
            }
            else
            {
                _detalles.Add(new CompraDetalleRequestDto
                {
                    ProductoId = _productoSeleccionado.ProductoId,
                    Cantidad = cantidad,
                    CostoUnitario = costo,
                    SubtotalLinea = cantidad * costo
                });
            }

            RefrescarDetalle();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al agregar producto:\n" + ex.Message, "Compras", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnQuitarProducto_Click(object sender, EventArgs e)
    {
        try
        {
            if (dgvDetalle.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un producto del detalle.");
                return;
            }

            object? valorProductoId = dgvDetalle.CurrentRow.Cells["colDetalleProductoId"].Value;
            if (valorProductoId == null)
            {
                MessageBox.Show("No se pudo identificar el producto seleccionado.");
                return;
            }

            int productoId = Convert.ToInt32(valorProductoId);
            var detalle = _detalles.FirstOrDefault(x => x.ProductoId == productoId);

            if (detalle != null)
            {
                _detalles.Remove(detalle);
                RefrescarDetalle();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al quitar producto:\n" + ex.Message, "Compras", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RefrescarDetalle()
    {
        try
        {
            var detalleVisual = _detalles.Select(x =>
            {
                var producto = _productos.FirstOrDefault(p => p.ProductoId == x.ProductoId);
                return new
                {
                    ProductoId = x.ProductoId,
                    Producto = producto?.NombreProducto ?? "",
                    x.Cantidad,
                    x.CostoUnitario,
                    x.SubtotalLinea
                };
            }).ToList();

            dgvDetalle.DataSource = null;
            dgvDetalle.DataSource = detalleVisual;
            dgvDetalle.ClearSelection();

            decimal total = _detalles.Sum(x => x.SubtotalLinea);
            lblTotal.Text = $"Total: {total:C2}";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al actualizar detalle:\n" + ex.Message, "Compras", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnRegistrarCompra_Click(object sender, EventArgs e)
    {
        try
        {
            if (cmbProveedores.SelectedValue == null)
            {
                MessageBox.Show("Seleccione un proveedor.");
                cmbProveedores.Focus();
                return;
            }

            if (_detalles.Count == 0)
            {
                MessageBox.Show("Debe agregar al menos un producto.");
                return;
            }

            CrearCompraRequestDto request = new CrearCompraRequestDto
            {
                ProveedorId = Convert.ToInt32(cmbProveedores.SelectedValue),
                UsuarioId = _usuarioId,
                NumeroComprobante = txtNumeroComprobante.Text.Trim(),
                Subtotal = _detalles.Sum(x => x.SubtotalLinea),
                DescuentoTotal = 0,
                TotalCompra = _detalles.Sum(x => x.SubtotalLinea),
                Estado = "Confirmada",
                Observaciones = txtObservaciones.Text.Trim(),
                Detalles = _detalles
            };

            bool ok = await _comprasService.CrearAsync(request);

            if (ok)
            {
                MessageBox.Show("Compra registrada correctamente.");
                _detalles.Clear();
                RefrescarDetalle();
                txtNumeroComprobante.Text = DateTime.Now.ToString("yyyyMMddHHmmss");
                txtObservaciones.Clear();
                txtCantidad.Text = "1";
                txtCostoUnitario.Text = "0";
                await CargarProductos();
                await CargarCompras();
            }
            else
            {
                MessageBox.Show("No se pudo registrar la compra.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al registrar compra:\n" + ex.Message, "Compras", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnNuevoProveedor_Click(object sender, EventArgs e)
    {
        try
        {
            string nombre = Microsoft.VisualBasic.Interaction.InputBox("Ingrese el nombre del proveedor:", "Nuevo proveedor", "");
            if (string.IsNullOrWhiteSpace(nombre))
                return;

            bool ok = await _proveedoresService.CrearAsync(new ProveedorDto
            {
                NombreProveedor = nombre.Trim(),
                Activo = true
            });

            if (ok)
            {
                MessageBox.Show("Proveedor agregado correctamente.");
                await CargarProveedores();
            }
            else
            {
                MessageBox.Show("No se pudo agregar el proveedor.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al agregar proveedor:\n" + ex.Message, "Compras", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            MessageBox.Show("Error al cerrar el formulario:\n" + ex.Message, "Compras", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

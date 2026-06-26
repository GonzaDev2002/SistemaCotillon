using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Desktop.WinForms.Services;
using Cotillon.Shared.DTOs;
using Cotillon.Shared.DTOs.Clientes;
using Cotillon.Shared.DTOs.Ventas;

namespace Cotillon.Desktop.WinForms.Forms;

public partial class FrmVentas : Form
{
    private readonly VentasApiService _ventasApiService = new();
    private readonly ProductosApiService _productosApiService = new();
    private readonly ClientesApiService _clientesApiService = new();

    private List<ProductoListadoDto> _productos = new();
    private List<ClienteDto> _clientes = new();
    private readonly List<DetalleVentaRequestDto> _detalles = new();

    private int _usuarioId = 1;
    private int _sesionCajaId = 0;
    private int _medioPagoId = 1;
    private int _clienteGenericoId = 0;
    private decimal _precioSeleccionado = 0;

    public FrmVentas()
    {
        InitializeComponent();
        PopTheme.ApplyPrimaryButton(btnAgregarProducto);
        PopTheme.ApplyDangerButton(btnQuitarProducto);
        PopTheme.ApplySuccessButton(btnRegistrarVenta);
        PopTheme.ApplySecondaryButton(btnCerrar);
        PopTheme.ApplySecondaryButton(btnNuevoCliente);

        PopTheme.ApplyPrimaryButton(btnEfectivo);
        PopTheme.ApplySecondaryButton(btnTransferencia);
        PopTheme.ApplySecondaryButton(btnTarjeta);
        AplicarDiseñoVentas();
    }

    public FrmVentas(int usuarioId, int sesionCajaId)
    {
        InitializeComponent();
        _usuarioId = usuarioId;
        _sesionCajaId = sesionCajaId;
        AplicarDiseñoVentas();
        PopTheme.ApplyPrimaryButton(btnAgregarProducto);
        PopTheme.ApplyDangerButton(btnQuitarProducto);
        PopTheme.ApplySuccessButton(btnRegistrarVenta);
        PopTheme.ApplySecondaryButton(btnCerrar);
        PopTheme.ApplySecondaryButton(btnNuevoCliente);

        PopTheme.ApplyPrimaryButton(btnEfectivo);
        PopTheme.ApplySecondaryButton(btnTransferencia);
        PopTheme.ApplySecondaryButton(btnTarjeta);
    }

    private async void FrmVentas_Load(object sender, EventArgs e)
    {
        ConfigurarGrillasVentas();

        await CargarClientes();
        await CargarProductos();
        await CargarVentas();

        SeleccionarMedioPago(1);
        RefrescarDetalle();
    }

    private async Task CargarClientes()
    {
        _clientes = await _clientesApiService.ListarAsync();

        _clienteGenericoId = _clientes
    .FirstOrDefault(x =>
        !string.IsNullOrWhiteSpace(x.NombreCompleto) &&
        x.NombreCompleto.Trim().Equals("Consumidor Final", StringComparison.OrdinalIgnoreCase))
    ?.ClienteId ?? 0;

        if (_clienteGenericoId <= 0)
            _clienteGenericoId = 1;
        var clientesReales = _clientes
            .Where(x => !string.IsNullOrWhiteSpace(x.NombreCompleto))
            .Where(x => !x.NombreCompleto.Trim().Equals("Consumidor Final", StringComparison.OrdinalIgnoreCase))
            .ToList();

        cmbClientes.DataSource = null;
        cmbClientes.DataSource = clientesReales;
        cmbClientes.DisplayMember = "NombreCompleto";
        cmbClientes.ValueMember = "ClienteId";

        chkVentaSinCliente.Checked = true;
        ActualizarEstadoCliente();
    }

    private void chkVentaSinCliente_CheckedChanged(object sender, EventArgs e)
    {
        ActualizarEstadoCliente();
    }

    private void ActualizarEstadoCliente()
    {
        bool ventaSinCliente = chkVentaSinCliente.Checked;

        cmbClientes.Enabled = !ventaSinCliente;
        btnNuevoCliente.Enabled = !ventaSinCliente;

        if (ventaSinCliente)
        {
            cmbClientes.SelectedIndex = -1;
        }
        else
        {
            if (cmbClientes.Items.Count > 0 && cmbClientes.SelectedIndex == -1)
                cmbClientes.SelectedIndex = 0;
        }
    }

    private async Task CargarProductos()
    {
        _productos = await _productosApiService.ObtenerTodosAsync(false);

        lstProductos.DataSource = null;
        lstProductos.DataSource = _productos;
        lstProductos.DisplayMember = "NombreProducto";
        lstProductos.ValueMember = "ProductoId";

        if (_productos.Count > 0)
        {
            lstProductos.SelectedIndex = 0;
            CargarProductoSeleccionado();
        }
    }

    private async Task CargarVentas()
    {
        dgvVentas.DataSource = null;
        dgvVentas.DataSource = await _ventasApiService.ListarVentasAsync();
    }

    private void txtBuscarProducto_TextChanged(object sender, EventArgs e)
    {
        string filtro = txtBuscarProducto.Text.Trim().ToLower();

        var productosFiltrados = _productos
            .Where(p => p.NombreProducto.ToLower().Contains(filtro))
            .ToList();

        lstProductos.DataSource = null;
        lstProductos.DataSource = productosFiltrados;
        lstProductos.DisplayMember = "NombreProducto";
        lstProductos.ValueMember = "ProductoId";
    }

    private void lstProductos_SelectedIndexChanged(object sender, EventArgs e)
    {
        CargarProductoSeleccionado();
    }

    private void CargarProductoSeleccionado()
    {
        if (lstProductos.SelectedItem is ProductoListadoDto producto)
        {
            _precioSeleccionado = producto.Precio;
            txtPrecio.Text = _precioSeleccionado.ToString("N2");
            CalcularSubtotalActual();
        }
    }

    private void txtCantidad_TextChanged(object sender, EventArgs e)
    {
        CalcularSubtotalActual();
    }

    private void CalcularSubtotalActual()
    {
        if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
        {
            lblSubtotal.Text = "Subtotal: $0,00";
            return;
        }

        decimal subtotal = cantidad * _precioSeleccionado;
        lblSubtotal.Text = $"Subtotal: {subtotal:C2}";
    }

    private void SeleccionarMedioPago(int medioPagoId)
    {
        _medioPagoId = medioPagoId;

        PopTheme.ApplySecondaryButton(btnEfectivo);
        PopTheme.ApplySecondaryButton(btnTransferencia);
        PopTheme.ApplySecondaryButton(btnTarjeta);

        if (medioPagoId == 1)
            PopTheme.ApplyPrimaryButton(btnEfectivo);

        if (medioPagoId == 2)
            PopTheme.ApplyPrimaryButton(btnTransferencia);

        if (medioPagoId == 3)
            PopTheme.ApplyPrimaryButton(btnTarjeta);
    }

    private void btnEfectivo_Click(object sender, EventArgs e)
    {
        SeleccionarMedioPago(1);
    }

    private void btnTransferencia_Click(object sender, EventArgs e)
    {
        SeleccionarMedioPago(2);
    }

    private void btnTarjeta_Click(object sender, EventArgs e)
    {
        SeleccionarMedioPago(3);
    }

    private void btnAgregarProducto_Click(object sender, EventArgs e)
    {
        if (lstProductos.SelectedItem is not ProductoListadoDto producto)
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

        if (cantidad > producto.Stock)
        {
            MessageBox.Show("No hay stock suficiente para este producto.");
            return;
        }

        _detalles.Add(new DetalleVentaRequestDto
        {
            ProductoId = producto.ProductoId,
            Cantidad = cantidad,
            PrecioUnitario = producto.Precio
        });

        txtCantidad.Text = "1";
        RefrescarDetalle();
    }

    private void RefrescarDetalle()
    {
        dgvDetalle.DataSource = null;

        var detalleVisual = _detalles.Select(x =>
        {
            ProductoListadoDto? producto = _productos.FirstOrDefault(p => p.ProductoId == x.ProductoId);

            return new
            {
                Producto = producto?.NombreProducto ?? "Producto",
                Cantidad = x.Cantidad,
                PrecioUnitario = x.PrecioUnitario,
                Subtotal = x.Cantidad * x.PrecioUnitario
            };
        }).ToList();

        dgvDetalle.DataSource = detalleVisual;

        decimal total = _detalles.Sum(x => x.Cantidad * x.PrecioUnitario);
        lblTotal.Text = $"Total: {total:C2}";
    }

    private void ConfigurarGrillasVentas()
    {
        dgvDetalle.Columns.Clear();

        dgvDetalle.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Producto", DataPropertyName = "Producto" });
        dgvDetalle.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cantidad", DataPropertyName = "Cantidad" });
        dgvDetalle.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Precio", DataPropertyName = "PrecioUnitario", DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
        dgvDetalle.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Subtotal", DataPropertyName = "Subtotal", DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });

        dgvVentas.Columns.Clear();

        dgvVentas.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fecha", DataPropertyName = "FechaVenta", DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" } });
        dgvVentas.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cliente", DataPropertyName = "Cliente" });
        dgvVentas.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Usuario", DataPropertyName = "Usuario" });
        dgvVentas.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", DataPropertyName = "TotalVenta", DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
        dgvVentas.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Estado", DataPropertyName = "Estado" });

        GridHelper.AplicarEstilo(dgvDetalle);
        GridHelper.AplicarEstilo(dgvVentas);
    }

    private void btnQuitarProducto_Click(object sender, EventArgs e)
    {
        if (dgvDetalle.CurrentRow == null)
            return;

        int index = dgvDetalle.CurrentRow.Index;

        if (index >= 0 && index < _detalles.Count)
        {
            _detalles.RemoveAt(index);
            RefrescarDetalle();
        }
    }

    private async void btnRegistrarVenta_Click(object sender, EventArgs e)
    {
        int? clienteId;

        if (_sesionCajaId <= 0)
        {
            MessageBox.Show("No hay una caja abierta para registrar ventas.");
            return;
        }

        if (chkVentaSinCliente.Checked)
        {
            clienteId = null;
        }
        else
        {
            if (cmbClientes.SelectedValue == null)
            {
                MessageBox.Show("Seleccione un cliente.");
                return;
            }

            clienteId = Convert.ToInt32(cmbClientes.SelectedValue);
        }

        if (_detalles.Count == 0)
        {
            MessageBox.Show("Debe agregar al menos un producto.");
            return;
        }

        CrearVentaRequestDto request = new CrearVentaRequestDto
        {
            SesionCajaId = _sesionCajaId,
            ClienteId = clienteId,
            UsuarioId = _usuarioId,
            MedioPagoId = _medioPagoId,
            TipoComprobante = "Ticket",
            NumeroComprobante = DateTime.Now.ToString("yyyyMMddHHmmss"),
            DescuentoTotal = 0,
            Detalles = _detalles
        };

        bool ok = await _ventasApiService.CrearVentaAsync(request);

        if (ok)
        {
            MessageBox.Show("Venta registrada correctamente.");

            _detalles.Clear();
            RefrescarDetalle();

            txtCantidad.Text = "1";

            await CargarProductos();
            await CargarVentas();
        }
        else
        {
            MessageBox.Show("No se pudo registrar la venta.");
        }
    }

    private async void btnNuevoCliente_Click(object sender, EventArgs e)
    {
        using FrmClientes frm = new FrmClientes();
        frm.ShowDialog();

        await CargarClientes();
    }

    private void btnCerrar_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void AplicarDiseñoVentas()
    {
        PopTheme.ApplyForm(this);

        Text = "POP Cotillón - Ventas";

        BackColor = Color.FromArgb(250, 248, 242);
        BackgroundImage = Properties.Resources.fondoForms;
        BackgroundImageLayout = ImageLayout.Stretch;

        PopTheme.ApplyGroupBox(grpPago);
        PopTheme.ApplyGroupBox(grpProductos);
        PopTheme.ApplyGroupBox(grpDetalle);
        PopTheme.ApplyGroupBox(grpVentas);

        grpPago.Text = "Medio de pago";
        grpProductos.Text = "Buscar producto";
        grpDetalle.Text = "Detalle de venta";
        grpVentas.Text = "Ventas registradas";

        PopTheme.ApplyInput(txtBuscarProducto);
        PopTheme.ApplyInput(txtCantidad);
        PopTheme.ApplyInput(txtPrecio);
        PopTheme.ApplyCombo(cmbClientes);
        PopTheme.ApplyListBox(lstProductos);

        PopTheme.ApplyGrid(dgvDetalle);
        PopTheme.ApplyGrid(dgvVentas);

        PopTheme.ApplySecondaryButton(btnNuevoCliente);
        PopTheme.ApplyPrimaryButton(btnAgregarProducto);
        PopTheme.ApplyDangerButton(btnQuitarProducto);
        PopTheme.ApplySuccessButton(btnRegistrarVenta);
        PopTheme.ApplySecondaryButton(btnCerrar);

        PopTheme.ApplyPrimaryButton(btnEfectivo);
        PopTheme.ApplySecondaryButton(btnTransferencia);
        PopTheme.ApplySecondaryButton(btnTarjeta);

        lblCliente.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblCantidad.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblPrecio.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblSubtotal.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblTotal.Font = new Font("Segoe UI", 20F, FontStyle.Bold);

        lblTotal.ForeColor = PopTheme.Colors.Principal;
        lblSubtotal.ForeColor = PopTheme.Colors.Secundario;

        chkVentaSinCliente.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        chkVentaSinCliente.ForeColor = PopTheme.Colors.Texto;

        txtPrecio.ReadOnly = true;
    }
}
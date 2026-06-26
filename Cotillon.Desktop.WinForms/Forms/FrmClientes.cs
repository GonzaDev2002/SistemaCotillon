using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Desktop.WinForms.Services;
using Cotillon.Shared.DTOs.Clientes;

namespace Cotillon.Desktop.WinForms.Forms;

public partial class FrmClientes : Form
{
    private readonly ClientesApiService _clientesApiService = new();

    private int _clienteIdSeleccionado = 0;

    public FrmClientes()
    {
        InitializeComponent();

        try
        {
            AplicarDiseñoClientes();
            ConfigurarGrillaClientes();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al inicializar clientes:\n" + ex.Message,
                "Clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AplicarDiseñoClientes()
    {
        PopTheme.ApplyForm(this);

        Text = "POP Cotillón - Clientes";
        BackColor = Color.FromArgb(250, 248, 242);
        BackgroundImage = Properties.Resources.fondoForms;
        BackgroundImageLayout = ImageLayout.Stretch;

        PopTheme.ApplyGroupBox(grpDatos);
        PopTheme.ApplyGroupBox(grpListado);

        grpDatos.Text = "Datos del cliente";
        grpListado.Text = "Clientes registrados";

        foreach (Control control in grpDatos.Controls)
        {
            if (control is TextBox txt)
                PopTheme.ApplyInput(txt);

            if (control is Label lbl)
            {
                lbl.ForeColor = PopTheme.Colors.Texto;
                lbl.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            }
        }

        PopTheme.ApplyGrid(dgvClientes);

        PopTheme.ApplySuccessButton(btnGuardar);
        PopTheme.ApplyDangerButton(btnEliminar);
        PopTheme.ApplySecondaryButton(btnLimpiar);
        PopTheme.ApplySecondaryButton(btnCerrar);

        btnGuardar.Text = "Guardar cliente";
        btnEliminar.Text = "Eliminar";
        btnLimpiar.Text = "Limpiar";
        btnCerrar.Text = "Cerrar";
    }

    private async void FrmClientes_Load(object sender, EventArgs e)
    {
        try
        {
            ConfigurarGrillaClientes();
            await CargarClientes();
            Limpiar();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar clientes:\n" + ex.Message,
                "Clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task CargarClientes()
    {
        try
        {
            var lista = await _clientesApiService.ListarAsync();

            dgvClientes.DataSource = null;
            dgvClientes.DataSource = lista;
            dgvClientes.ClearSelection();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al listar clientes:\n" + ex.Message,
                "Clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ConfigurarGrillaClientes()
    {
        try
        {
            dgvClientes.DataSource = null;
            dgvClientes.Columns.Clear();
            dgvClientes.AutoGenerateColumns = false;

            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colNombre",
                HeaderText = "Nombre",
                DataPropertyName = "NombreCompleto",
                FillWeight = 170
            });

            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDocumento",
                HeaderText = "Documento",
                DataPropertyName = "Documento",
                FillWeight = 100
            });

            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTelefono",
                HeaderText = "Teléfono",
                DataPropertyName = "Telefono",
                FillWeight = 110
            });

            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colEmail",
                HeaderText = "Email",
                DataPropertyName = "Email",
                FillWeight = 160
            });

            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDireccion",
                HeaderText = "Dirección",
                DataPropertyName = "Direccion",
                FillWeight = 160
            });

            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colObservaciones",
                HeaderText = "Observaciones",
                DataPropertyName = "Observaciones",
                FillWeight = 180
            });

            dgvClientes.Dock = DockStyle.Fill;
            dgvClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvClientes.RowHeadersVisible = false;
            dgvClientes.AllowUserToAddRows = false;
            dgvClientes.AllowUserToDeleteRows = false;
            dgvClientes.ReadOnly = true;
            dgvClientes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvClientes.MultiSelect = false;
            dgvClientes.ScrollBars = ScrollBars.Both;
            dgvClientes.BackgroundColor = Color.White;
            dgvClientes.BorderStyle = BorderStyle.FixedSingle;
            dgvClientes.EnableHeadersVisualStyles = false;

            dgvClientes.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(79, 70, 229);
            dgvClientes.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvClientes.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvClientes.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvClientes.DefaultCellStyle.SelectionBackColor = PopTheme.Colors.Principal;
            dgvClientes.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvClientes.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 252);
            dgvClientes.RowTemplate.Height = 28;
            dgvClientes.ColumnHeadersHeight = 32;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al configurar grilla de clientes:\n" + ex.Message,
                "Clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Limpiar()
    {
        try
        {
            _clienteIdSeleccionado = 0;

            txtNombre.Clear();
            txtDocumento.Clear();
            txtTelefono.Clear();
            txtEmail.Clear();
            txtDireccion.Clear();
            txtObservaciones.Clear();

            dgvClientes.ClearSelection();
            txtNombre.Focus();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al limpiar formulario:\n" + ex.Message,
                "Clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnGuardar_Click(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Ingrese el nombre del cliente.");
                txtNombre.Focus();
                return;
            }

            ClienteDto cliente = new ClienteDto
            {
                ClienteId = _clienteIdSeleccionado,
                NombreCompleto = txtNombre.Text.Trim(),
                Documento = txtDocumento.Text.Trim(),
                Telefono = txtTelefono.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Direccion = txtDireccion.Text.Trim(),
                Observaciones = txtObservaciones.Text.Trim(),
                Activo = true
            };

            btnGuardar.Enabled = false;

            bool ok = _clienteIdSeleccionado == 0
                ? await _clientesApiService.CrearAsync(cliente)
                : await _clientesApiService.ActualizarAsync(cliente);

            if (ok)
            {
                MessageBox.Show("Cliente guardado correctamente.");
                await CargarClientes();
                Limpiar();
            }
            else
            {
                MessageBox.Show("No se pudo guardar el cliente.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al guardar cliente:\n" + ex.Message,
                "Clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnGuardar.Enabled = true;
        }
    }

    private void dgvClientes_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        try
        {
            if (e.RowIndex < 0)
                return;

            ClienteDto? cliente = dgvClientes.Rows[e.RowIndex].DataBoundItem as ClienteDto;

            if (cliente == null)
                return;

            _clienteIdSeleccionado = cliente.ClienteId;

            txtNombre.Text = cliente.NombreCompleto;
            txtDocumento.Text = cliente.Documento;
            txtTelefono.Text = cliente.Telefono;
            txtEmail.Text = cliente.Email;
            txtDireccion.Text = cliente.Direccion;
            txtObservaciones.Text = cliente.Observaciones;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al seleccionar cliente:\n" + ex.Message,
                "Clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnEliminar_Click(object sender, EventArgs e)
    {
        try
        {
            if (_clienteIdSeleccionado == 0)
            {
                MessageBox.Show("Seleccione un cliente.");
                return;
            }

            DialogResult confirmar = MessageBox.Show(
                "¿Desea eliminar este cliente?",
                "Confirmar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmar != DialogResult.Yes)
                return;

            btnEliminar.Enabled = false;

            bool ok = await _clientesApiService.EliminarAsync(_clienteIdSeleccionado);

            if (ok)
            {
                MessageBox.Show("Cliente eliminado correctamente.");
                await CargarClientes();
                Limpiar();
            }
            else
            {
                MessageBox.Show("No se pudo eliminar el cliente.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al eliminar cliente:\n" + ex.Message,
                "Clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnEliminar.Enabled = true;
        }
    }

    private void btnLimpiar_Click(object sender, EventArgs e)
    {
        Limpiar();
    }

    private void btnCerrar_Click(object sender, EventArgs e)
    {
        Close();
    }
}
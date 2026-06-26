using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Desktop.WinForms.Services;
using Cotillon.Shared.DTOs.Usuarios;

namespace Cotillon.Desktop.WinForms.Forms;

public partial class FrmUsuarios : Form
{
    private readonly UsuariosApiService _usuariosService = new();
    private int _usuarioIdSeleccionado = 0;

    public FrmUsuarios()
    {
        InitializeComponent();

        try
        {
            AplicarTemaVisual();
            ConfigurarGrillaUsuarios();

            PopTheme.ApplySuccessButton(btnGuardar);
            PopTheme.ApplyDangerButton(btnDarBaja);
            PopTheme.ApplyPrimaryButton(btnReactivar);
            PopTheme.ApplySecondaryButton(btnLimpiar);
            PopTheme.ApplySecondaryButton(btnCerrar);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al inicializar el formulario de usuarios:\n" + ex.Message,
                "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void FrmUsuarios_Load(object sender, EventArgs e)
    {
        try
        {
            cmbRol.Items.Clear();
            cmbRol.Items.Add("Admin");
            cmbRol.Items.Add("Trabajador");
            cmbRol.SelectedIndex = 1;

            chkVerInactivos.Checked = true;

            await CargarUsuarios();
            Limpiar();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar usuarios:\n" + ex.Message,
                "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ConfigurarGrillaUsuarios()
    {
        dgvUsuarios.DataSource = null;
        dgvUsuarios.Columns.Clear();

        dgvUsuarios.AutoGenerateColumns = false;
        dgvUsuarios.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvUsuarios.RowHeadersVisible = false;
        dgvUsuarios.AllowUserToAddRows = false;
        dgvUsuarios.AllowUserToDeleteRows = false;
        dgvUsuarios.ReadOnly = true;
        dgvUsuarios.MultiSelect = false;
        dgvUsuarios.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvUsuarios.ScrollBars = ScrollBars.Both;
        dgvUsuarios.BackgroundColor = Color.White;
        dgvUsuarios.BorderStyle = BorderStyle.FixedSingle;
        dgvUsuarios.EnableHeadersVisualStyles = false;

        dgvUsuarios.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(79, 70, 229);
        dgvUsuarios.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvUsuarios.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        dgvUsuarios.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        dgvUsuarios.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
        dgvUsuarios.DefaultCellStyle.SelectionBackColor = PopTheme.Colors.Principal;
        dgvUsuarios.DefaultCellStyle.SelectionForeColor = Color.White;
        dgvUsuarios.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 252);

        dgvUsuarios.RowTemplate.Height = 28;
        dgvUsuarios.ColumnHeadersHeight = 32;

        dgvUsuarios.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colUsuarioId",
            HeaderText = "ID",
            DataPropertyName = "UsuarioId",
            FillWeight = 45
        });

        dgvUsuarios.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colNombreUsuario",
            HeaderText = "Usuario",
            DataPropertyName = "NombreUsuario",
            FillWeight = 110
        });

        dgvUsuarios.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colNombreCompleto",
            HeaderText = "Nombre completo",
            DataPropertyName = "NombreCompleto",
            FillWeight = 180
        });

        dgvUsuarios.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colPin",
            HeaderText = "PIN",
            DataPropertyName = "Pin",
            FillWeight = 70
        });

        dgvUsuarios.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colRol",
            HeaderText = "Rol",
            DataPropertyName = "Rol",
            FillWeight = 100
        });

        dgvUsuarios.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "colActivo",
            HeaderText = "Activo",
            DataPropertyName = "Activo",
            FillWeight = 70
        });

        dgvUsuarios.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colFechaAlta",
            HeaderText = "Fecha alta",
            DataPropertyName = "FechaAlta",
            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" },
            FillWeight = 100
        });

        dgvUsuarios.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colFechaActualizacion",
            HeaderText = "Actualización",
            DataPropertyName = "FechaActualizacion",
            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" },
            FillWeight = 110
        });
    }

    private async Task CargarUsuarios()
    {
        try
        {
            ConfigurarGrillaUsuarios();

            var usuarios = await _usuariosService.ListarAsync(chkVerInactivos.Checked);

            dgvUsuarios.DataSource = null;
            dgvUsuarios.DataSource = usuarios;
            dgvUsuarios.ClearSelection();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al listar usuarios:\n" + ex.Message,
                "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Limpiar()
    {
        try
        {
            _usuarioIdSeleccionado = 0;
            txtUsuario.Clear();
            txtNombreCompleto.Clear();
            txtPin.Clear();

            if (cmbRol.Items.Count > 1)
                cmbRol.SelectedIndex = 1;

            chkActivo.Checked = true;
            dgvUsuarios.ClearSelection();
            txtUsuario.Focus();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al limpiar formulario:\n" + ex.Message,
                "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnGuardar_Click(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text))
            {
                MessageBox.Show("Ingrese el nombre de usuario.");
                txtUsuario.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNombreCompleto.Text))
            {
                MessageBox.Show("Ingrese el nombre completo.");
                txtNombreCompleto.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPin.Text))
            {
                MessageBox.Show("Ingrese el PIN.");
                txtPin.Focus();
                return;
            }

            UsuarioDto usuario = new UsuarioDto
            {
                UsuarioId = _usuarioIdSeleccionado,
                NombreUsuario = txtUsuario.Text.Trim(),
                NombreCompleto = txtNombreCompleto.Text.Trim(),
                Pin = txtPin.Text.Trim(),
                Rol = cmbRol.Text,
                Activo = chkActivo.Checked
            };

            bool ok = _usuarioIdSeleccionado == 0
                ? await _usuariosService.CrearAsync(usuario)
                : await _usuariosService.ActualizarAsync(usuario);

            if (ok)
            {
                MessageBox.Show("Usuario guardado correctamente.");
                await CargarUsuarios();
                Limpiar();
            }
            else
            {
                MessageBox.Show("No se pudo guardar el usuario.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al guardar usuario:\n" + ex.Message,
                "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void dgvUsuarios_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        try
        {
            if (e.RowIndex < 0)
                return;

            if (dgvUsuarios.Rows[e.RowIndex].DataBoundItem is not UsuarioDto usuario)
                return;

            _usuarioIdSeleccionado = usuario.UsuarioId;
            txtUsuario.Text = usuario.NombreUsuario;
            txtNombreCompleto.Text = usuario.NombreCompleto;
            txtPin.Text = usuario.Pin;
            cmbRol.Text = usuario.Rol;
            chkActivo.Checked = usuario.Activo;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al seleccionar usuario:\n" + ex.Message,
                "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnDarBaja_Click(object sender, EventArgs e)
    {
        try
        {
            if (_usuarioIdSeleccionado == 0)
            {
                MessageBox.Show("Seleccione un usuario.");
                return;
            }

            DialogResult confirmar = MessageBox.Show(
                "¿Desea dar de baja este usuario?",
                "Confirmar baja",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmar != DialogResult.Yes)
                return;

            bool ok = await _usuariosService.CambiarEstadoAsync(_usuarioIdSeleccionado, false);

            if (ok)
            {
                MessageBox.Show("Usuario dado de baja.");
                await CargarUsuarios();
                Limpiar();
            }
            else
            {
                MessageBox.Show("No se pudo dar de baja el usuario.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al dar de baja usuario:\n" + ex.Message,
                "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnReactivar_Click(object sender, EventArgs e)
    {
        try
        {
            if (_usuarioIdSeleccionado == 0)
            {
                MessageBox.Show("Seleccione un usuario.");
                return;
            }

            bool ok = await _usuariosService.CambiarEstadoAsync(_usuarioIdSeleccionado, true);

            if (ok)
            {
                MessageBox.Show("Usuario reactivado.");
                await CargarUsuarios();
                Limpiar();
            }
            else
            {
                MessageBox.Show("No se pudo reactivar el usuario.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al reactivar usuario:\n" + ex.Message,
                "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void chkVerInactivos_CheckedChanged(object sender, EventArgs e)
    {
        try
        {
            await CargarUsuarios();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al actualizar listado:\n" + ex.Message,
                "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

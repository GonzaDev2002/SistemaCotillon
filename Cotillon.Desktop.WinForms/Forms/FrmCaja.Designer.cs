namespace Cotillon.Desktop.WinForms.Forms;

partial class FrmCaja
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlHeader;
    private Label lblTitulo;
    private Label lblSubtitulo;

    private GroupBox grpCaja;
    private Label lblEstado;
    private Label lblFechaApertura;
    private Label lblMontoInicial;
    private Label lblMontoEsperado;
    private Label lblMontoContado;
    private TextBox txtMontoContado;
    private Label lblDiferencia;
    private Label lblObservaciones;
    private TextBox txtObservaciones;

    private Button btnAbrirCaja;
    private Button btnCerrarCaja;
    private Button btnCerrar;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        pnlHeader = new Panel();
        lblTitulo = new Label();
        lblSubtitulo = new Label();
        grpCaja = new GroupBox();
        lblEstado = new Label();
        lblFechaApertura = new Label();
        lblMontoInicial = new Label();
        lblMontoEsperado = new Label();
        lblMontoContado = new Label();
        txtMontoContado = new TextBox();
        lblDiferencia = new Label();
        lblObservaciones = new Label();
        txtObservaciones = new TextBox();
        btnAbrirCaja = new Button();
        btnCerrarCaja = new Button();
        btnCerrar = new Button();

        pnlHeader.SuspendLayout();
        grpCaja.SuspendLayout();
        SuspendLayout();

        // pnlHeader
        pnlHeader.BackColor = Color.FromArgb(255, 77, 109);
        pnlHeader.Controls.Add(lblTitulo);
        pnlHeader.Controls.Add(lblSubtitulo);
        pnlHeader.Location = new Point(20, 18);
        pnlHeader.Name = "pnlHeader";
        pnlHeader.Size = new Size(640, 90);
        pnlHeader.TabIndex = 0;

        // lblTitulo
        lblTitulo.AutoSize = true;
        lblTitulo.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        lblTitulo.ForeColor = Color.White;
        lblTitulo.Location = new Point(25, 12);
        lblTitulo.Name = "lblTitulo";
        lblTitulo.Size = new Size(173, 41);
        lblTitulo.TabIndex = 0;
        lblTitulo.Text = "💵 Caja";

        // lblSubtitulo
        lblSubtitulo.AutoSize = true;
        lblSubtitulo.Font = new Font("Segoe UI", 11F);
        lblSubtitulo.ForeColor = Color.White;
        lblSubtitulo.Location = new Point(28, 58);
        lblSubtitulo.Name = "lblSubtitulo";
        lblSubtitulo.Size = new Size(392, 20);
        lblSubtitulo.TabIndex = 1;
        lblSubtitulo.Text = "Apertura, cierre y control diario de caja";

        // grpCaja
        grpCaja.BackColor = Color.White;
        grpCaja.Controls.Add(lblEstado);
        grpCaja.Controls.Add(lblFechaApertura);
        grpCaja.Controls.Add(lblMontoInicial);
        grpCaja.Controls.Add(lblMontoEsperado);
        grpCaja.Controls.Add(lblMontoContado);
        grpCaja.Controls.Add(txtMontoContado);
        grpCaja.Controls.Add(lblDiferencia);
        grpCaja.Controls.Add(lblObservaciones);
        grpCaja.Controls.Add(txtObservaciones);
        grpCaja.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpCaja.Location = new Point(20, 128);
        grpCaja.Name = "grpCaja";
        grpCaja.Size = new Size(640, 345);
        grpCaja.TabIndex = 1;
        grpCaja.TabStop = false;
        grpCaja.Text = "Información de caja";

        // lblEstado
        lblEstado.AutoSize = true;
        lblEstado.Location = new Point(30, 38);
        lblEstado.Name = "lblEstado";
        lblEstado.Size = new Size(58, 19);
        lblEstado.TabIndex = 0;
        lblEstado.Text = "Estado:";

        // lblFechaApertura
        lblFechaApertura.AutoSize = true;
        lblFechaApertura.Location = new Point(30, 75);
        lblFechaApertura.Name = "lblFechaApertura";
        lblFechaApertura.Size = new Size(111, 19);
        lblFechaApertura.TabIndex = 1;
        lblFechaApertura.Text = "Fecha apertura:";

        // lblMontoInicial
        lblMontoInicial.AutoSize = true;
        lblMontoInicial.Location = new Point(30, 112);
        lblMontoInicial.Name = "lblMontoInicial";
        lblMontoInicial.Size = new Size(100, 19);
        lblMontoInicial.TabIndex = 2;
        lblMontoInicial.Text = "Monto inicial:";

        // lblMontoEsperado
        lblMontoEsperado.AutoSize = true;
        lblMontoEsperado.Location = new Point(30, 149);
        lblMontoEsperado.Name = "lblMontoEsperado";
        lblMontoEsperado.Size = new Size(123, 19);
        lblMontoEsperado.TabIndex = 3;
        lblMontoEsperado.Text = "Monto esperado:";

        // lblMontoContado
        lblMontoContado.AutoSize = true;
        lblMontoContado.Location = new Point(30, 190);
        lblMontoContado.Name = "lblMontoContado";
        lblMontoContado.Size = new Size(120, 19);
        lblMontoContado.TabIndex = 4;
        lblMontoContado.Text = "Monto contado:";

        // txtMontoContado
        txtMontoContado.Location = new Point(175, 187);
        txtMontoContado.Name = "txtMontoContado";
        txtMontoContado.Size = new Size(180, 25);
        txtMontoContado.TabIndex = 5;
        txtMontoContado.TextChanged += txtMontoContado_TextChanged;

        // lblDiferencia
        lblDiferencia.AutoSize = true;
        lblDiferencia.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        lblDiferencia.Location = new Point(30, 230);
        lblDiferencia.Name = "lblDiferencia";
        lblDiferencia.Size = new Size(161, 28);
        lblDiferencia.TabIndex = 6;
        lblDiferencia.Text = "Diferencia: $0,00";

        // lblObservaciones
        lblObservaciones.AutoSize = true;
        lblObservaciones.Location = new Point(30, 275);
        lblObservaciones.Name = "lblObservaciones";
        lblObservaciones.Size = new Size(111, 19);
        lblObservaciones.TabIndex = 7;
        lblObservaciones.Text = "Observaciones:";

        // txtObservaciones
        txtObservaciones.Location = new Point(175, 272);
        txtObservaciones.Multiline = true;
        txtObservaciones.Name = "txtObservaciones";
        txtObservaciones.Size = new Size(410, 52);
        txtObservaciones.TabIndex = 8;

        // btnAbrirCaja
        btnAbrirCaja.Location = new Point(135, 495);
        btnAbrirCaja.Name = "btnAbrirCaja";
        btnAbrirCaja.Size = new Size(145, 38);
        btnAbrirCaja.TabIndex = 2;
        btnAbrirCaja.Text = "Abrir caja";
        btnAbrirCaja.UseVisualStyleBackColor = false;
        btnAbrirCaja.Click += btnAbrirCaja_Click;

        // btnCerrarCaja
        btnCerrarCaja.Location = new Point(300, 495);
        btnCerrarCaja.Name = "btnCerrarCaja";
        btnCerrarCaja.Size = new Size(145, 38);
        btnCerrarCaja.TabIndex = 3;
        btnCerrarCaja.Text = "Cerrar caja";
        btnCerrarCaja.UseVisualStyleBackColor = false;
        btnCerrarCaja.Click += btnCerrarCaja_Click;

        // btnCerrar
        btnCerrar.Location = new Point(465, 495);
        btnCerrar.Name = "btnCerrar";
        btnCerrar.Size = new Size(120, 38);
        btnCerrar.TabIndex = 4;
        btnCerrar.Text = "Cerrar";
        btnCerrar.UseVisualStyleBackColor = false;
        btnCerrar.Click += btnCerrar_Click;

        // FrmCaja
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(250, 248, 242);
        BackgroundImage = Properties.Resources.fondoForms;
        BackgroundImageLayout = ImageLayout.Stretch;
        ClientSize = new Size(680, 560);
        Controls.Add(pnlHeader);
        Controls.Add(grpCaja);
        Controls.Add(btnAbrirCaja);
        Controls.Add(btnCerrarCaja);
        Controls.Add(btnCerrar);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "FrmCaja";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "POP Cotillón - Caja";
        Load += FrmCaja_Load;

        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        grpCaja.ResumeLayout(false);
        grpCaja.PerformLayout();
        ResumeLayout(false);
    }
}

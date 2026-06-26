namespace Cotillon.Desktop.WinForms.Forms;

partial class FrmLogin
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlContenedor;
    private Label lblTitulo;
    private Label lblSubtitulo;
    private Label lblUsuario;
    private TextBox txtUsuario;
    private Label lblPin;
    private TextBox txtPin;
    private Button btnIngresar;
    private Button btnSalir;
    private Label lblFooter;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        pnlContenedor = new Panel();
        lblTitulo = new Label();
        lblSubtitulo = new Label();
        lblUsuario = new Label();
        txtUsuario = new TextBox();
        lblPin = new Label();
        txtPin = new TextBox();
        btnIngresar = new Button();
        btnSalir = new Button();
        lblFooter = new Label();

        pnlContenedor.SuspendLayout();
        SuspendLayout();

        // pnlContenedor
        pnlContenedor.BackColor = Color.White;
        pnlContenedor.BorderStyle = BorderStyle.None;
        pnlContenedor.Controls.Add(lblTitulo);
        pnlContenedor.Controls.Add(lblSubtitulo);
        pnlContenedor.Controls.Add(lblUsuario);
        pnlContenedor.Controls.Add(txtUsuario);
        pnlContenedor.Controls.Add(lblPin);
        pnlContenedor.Controls.Add(txtPin);
        pnlContenedor.Controls.Add(btnIngresar);
        pnlContenedor.Controls.Add(btnSalir);
        pnlContenedor.Controls.Add(lblFooter);
        pnlContenedor.Location = new Point(45, 38);
        pnlContenedor.Name = "pnlContenedor";
        pnlContenedor.Size = new Size(390, 355);
        pnlContenedor.TabIndex = 0;

        // lblTitulo
        lblTitulo.AutoSize = true;
        lblTitulo.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
        lblTitulo.Location = new Point(58, 28);
        lblTitulo.Name = "lblTitulo";
        lblTitulo.Size = new Size(230, 45);
        lblTitulo.TabIndex = 0;
        lblTitulo.Text = "POP Cotillón";

        // lblSubtitulo
        lblSubtitulo.AutoSize = true;
        lblSubtitulo.Font = new Font("Segoe UI", 10F);
        lblSubtitulo.Location = new Point(92, 78);
        lblSubtitulo.Name = "lblSubtitulo";
        lblSubtitulo.Size = new Size(197, 19);
        lblSubtitulo.TabIndex = 1;
        lblSubtitulo.Text = "Ingresá al sistema de gestión";

        // lblUsuario
        lblUsuario.AutoSize = true;
        lblUsuario.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblUsuario.Location = new Point(45, 125);
        lblUsuario.Name = "lblUsuario";
        lblUsuario.Size = new Size(59, 19);
        lblUsuario.TabIndex = 2;
        lblUsuario.Text = "Usuario";

        // txtUsuario
        txtUsuario.Font = new Font("Segoe UI", 10F);
        txtUsuario.Location = new Point(45, 148);
        txtUsuario.Name = "txtUsuario";
        txtUsuario.Size = new Size(300, 25);
        txtUsuario.TabIndex = 0;

        // lblPin
        lblPin.AutoSize = true;
        lblPin.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblPin.Location = new Point(45, 190);
        lblPin.Name = "lblPin";
        lblPin.Size = new Size(31, 19);
        lblPin.TabIndex = 4;
        lblPin.Text = "PIN";

        // txtPin
        txtPin.Font = new Font("Segoe UI", 10F);
        txtPin.Location = new Point(45, 213);
        txtPin.Name = "txtPin";
        txtPin.Size = new Size(300, 25);
        txtPin.TabIndex = 1;
        txtPin.UseSystemPasswordChar = true;
        txtPin.KeyDown += txtPin_KeyDown;

        // btnIngresar
        btnIngresar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnIngresar.Location = new Point(45, 265);
        btnIngresar.Name = "btnIngresar";
        btnIngresar.Size = new Size(140, 38);
        btnIngresar.TabIndex = 2;
        btnIngresar.Text = "Ingresar";
        btnIngresar.UseVisualStyleBackColor = false;
        btnIngresar.Click += btnIngresar_Click;

        // btnSalir
        btnSalir.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnSalir.Location = new Point(205, 265);
        btnSalir.Name = "btnSalir";
        btnSalir.Size = new Size(140, 38);
        btnSalir.TabIndex = 3;
        btnSalir.Text = "Salir";
        btnSalir.UseVisualStyleBackColor = false;
        btnSalir.Click += btnSalir_Click;

        // lblFooter
        lblFooter.AutoSize = true;
        lblFooter.Font = new Font("Segoe UI", 8.5F);
        lblFooter.Location = new Point(88, 322);
        lblFooter.Name = "lblFooter";
        lblFooter.Size = new Size(204, 15);
        lblFooter.TabIndex = 8;
        lblFooter.Text = "Sistema administrativo POP Cotillón";

        // FrmLogin
        AcceptButton = btnIngresar;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackgroundImage = Properties.Resources.FondoLogin;
        BackgroundImageLayout = ImageLayout.Stretch;
        ClientSize = new Size(480, 430);
        Controls.Add(pnlContenedor);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "FrmLogin";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "POP Cotillón - Login";
        Load += FrmLogin_Load;

        pnlContenedor.ResumeLayout(false);
        pnlContenedor.PerformLayout();
        ResumeLayout(false);
    }
}
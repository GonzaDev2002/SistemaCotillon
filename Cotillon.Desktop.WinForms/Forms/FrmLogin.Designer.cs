namespace Cotillon.Desktop.WinForms.Forms;

partial class FrmLogin
{
    private System.ComponentModel.IContainer components = null;

    private Label lblTitulo;
    private Label lblUsuario;
    private TextBox txtUsuario;
    private Label lblPin;
    private TextBox txtPin;
    private Button btnIngresar;
    private Button btnSalir;
    private Panel pnlContenedor;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        lblTitulo = new Label();
        lblUsuario = new Label();
        txtUsuario = new TextBox();
        lblPin = new Label();
        txtPin = new TextBox();
        btnIngresar = new Button();
        btnSalir = new Button();
        pnlContenedor = new Panel();
        pnlContenedor.SuspendLayout();
        SuspendLayout();
        // 
        // lblTitulo
        // 
        lblTitulo.AutoSize = true;
        lblTitulo.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitulo.Location = new Point(65, 20);
        lblTitulo.Name = "lblTitulo";
        lblTitulo.Size = new Size(177, 30);
        lblTitulo.TabIndex = 0;
        lblTitulo.Text = "INICIAR SESIÓN";
        // 
        // lblUsuario
        // 
        lblUsuario.AutoSize = true;
        lblUsuario.Location = new Point(32, 80);
        lblUsuario.Name = "lblUsuario";
        lblUsuario.Size = new Size(47, 15);
        lblUsuario.TabIndex = 1;
        lblUsuario.Text = "Usuario";
        // 
        // txtUsuario
        // 
        txtUsuario.Location = new Point(32, 98);
        txtUsuario.Name = "txtUsuario";
        txtUsuario.Size = new Size(260, 23);
        txtUsuario.TabIndex = 0;
        // 
        // lblPin
        // 
        lblPin.AutoSize = true;
        lblPin.Location = new Point(32, 136);
        lblPin.Name = "lblPin";
        lblPin.Size = new Size(26, 15);
        lblPin.TabIndex = 3;
        lblPin.Text = "PIN";
        // 
        // txtPin
        // 
        txtPin.Location = new Point(32, 154);
        txtPin.Name = "txtPin";
        txtPin.Size = new Size(260, 23);
        txtPin.TabIndex = 1;
        txtPin.UseSystemPasswordChar = true;
        txtPin.KeyDown += txtPin_KeyDown;
        // 
        // btnIngresar
        // 
        btnIngresar.Location = new Point(32, 200);
        btnIngresar.Name = "btnIngresar";
        btnIngresar.Size = new Size(110, 32);
        btnIngresar.TabIndex = 2;
        btnIngresar.Text = "Ingresar";
        btnIngresar.UseVisualStyleBackColor = true;
        btnIngresar.Click += btnIngresar_Click;
        // 
        // btnSalir
        // 
        btnSalir.Location = new Point(182, 200);
        btnSalir.Name = "btnSalir";
        btnSalir.Size = new Size(110, 32);
        btnSalir.TabIndex = 3;
        btnSalir.Text = "Salir";
        btnSalir.UseVisualStyleBackColor = true;
        btnSalir.Click += btnSalir_Click;
        // 
        // pnlContenedor
        // 
        pnlContenedor.BorderStyle = BorderStyle.FixedSingle;
        pnlContenedor.Controls.Add(lblTitulo);
        pnlContenedor.Controls.Add(lblUsuario);
        pnlContenedor.Controls.Add(txtUsuario);
        pnlContenedor.Controls.Add(lblPin);
        pnlContenedor.Controls.Add(txtPin);
        pnlContenedor.Controls.Add(btnIngresar);
        pnlContenedor.Controls.Add(btnSalir);
        pnlContenedor.Location = new Point(35, 30);
        pnlContenedor.Name = "pnlContenedor";
        pnlContenedor.Size = new Size(330, 260);
        pnlContenedor.TabIndex = 0;
        // 
        // FrmLogin
        // 
        AcceptButton = btnIngresar;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(400, 320);
        Controls.Add(pnlContenedor);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "FrmLogin";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Login";
        Load += FrmLogin_Load;
        pnlContenedor.ResumeLayout(false);
        pnlContenedor.PerformLayout();
        ResumeLayout(false);
    }
}
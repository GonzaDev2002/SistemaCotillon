namespace Cotillon.Desktop.WinForms.Forms;

partial class FrmReportes
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlHeader;
    private Label lblTitulo;
    private Label lblSubtitulo;

    private GroupBox grpFiltros;
    private Label lblTipoReporte;
    private ComboBox cmbTipoReporte;
    private Label lblDesde;
    private DateTimePicker dtpDesde;
    private Label lblHasta;
    private DateTimePicker dtpHasta;
    private Button btnGenerar;

    private GroupBox grpResultado;
    private DataGridView dgvReportes;
    private Label lblResultado;

    private Button btnExportar;
    private Button btnVistaPrevia;
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
        grpFiltros = new GroupBox();
        lblTipoReporte = new Label();
        cmbTipoReporte = new ComboBox();
        lblDesde = new Label();
        dtpDesde = new DateTimePicker();
        lblHasta = new Label();
        dtpHasta = new DateTimePicker();
        btnGenerar = new Button();
        grpResultado = new GroupBox();
        dgvReportes = new DataGridView();
        lblResultado = new Label();
        btnExportar = new Button();
        btnVistaPrevia = new Button();
        btnCerrar = new Button();

        pnlHeader.SuspendLayout();
        grpFiltros.SuspendLayout();
        grpResultado.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvReportes).BeginInit();
        SuspendLayout();

        // pnlHeader
        pnlHeader.BackColor = Color.FromArgb(255, 77, 109);
        pnlHeader.Controls.Add(lblTitulo);
        pnlHeader.Controls.Add(lblSubtitulo);
        pnlHeader.Location = new Point(20, 18);
        pnlHeader.Name = "pnlHeader";
        pnlHeader.Size = new Size(940, 90);
        pnlHeader.TabIndex = 0;

        // lblTitulo
        lblTitulo.AutoSize = true;
        lblTitulo.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        lblTitulo.ForeColor = Color.White;
        lblTitulo.Location = new Point(25, 12);
        lblTitulo.Name = "lblTitulo";
        lblTitulo.Size = new Size(260, 41);
        lblTitulo.TabIndex = 0;
        lblTitulo.Text = "📄 Reportes";

        // lblSubtitulo
        lblSubtitulo.AutoSize = true;
        lblSubtitulo.Font = new Font("Segoe UI", 11F);
        lblSubtitulo.ForeColor = Color.White;
        lblSubtitulo.Location = new Point(28, 58);
        lblSubtitulo.Name = "lblSubtitulo";
        lblSubtitulo.Size = new Size(462, 20);
        lblSubtitulo.TabIndex = 1;
        lblSubtitulo.Text = "Consultas, exportación CSV y vista previa de información";

        // grpFiltros
        grpFiltros.BackColor = Color.White;
        grpFiltros.Controls.Add(lblTipoReporte);
        grpFiltros.Controls.Add(cmbTipoReporte);
        grpFiltros.Controls.Add(lblDesde);
        grpFiltros.Controls.Add(dtpDesde);
        grpFiltros.Controls.Add(lblHasta);
        grpFiltros.Controls.Add(dtpHasta);
        grpFiltros.Controls.Add(btnGenerar);
        grpFiltros.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpFiltros.Location = new Point(20, 125);
        grpFiltros.Name = "grpFiltros";
        grpFiltros.Size = new Size(940, 105);
        grpFiltros.TabIndex = 1;
        grpFiltros.TabStop = false;
        grpFiltros.Text = "Filtros del reporte";

        // lblTipoReporte
        lblTipoReporte.AutoSize = true;
        lblTipoReporte.Location = new Point(25, 35);
        lblTipoReporte.Name = "lblTipoReporte";
        lblTipoReporte.Size = new Size(114, 19);
        lblTipoReporte.TabIndex = 0;
        lblTipoReporte.Text = "Tipo de reporte:";

        // cmbTipoReporte
        cmbTipoReporte.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbTipoReporte.FormattingEnabled = true;
        cmbTipoReporte.Location = new Point(25, 58);
        cmbTipoReporte.Name = "cmbTipoReporte";
        cmbTipoReporte.Size = new Size(260, 25);
        cmbTipoReporte.TabIndex = 1;

        // lblDesde
        lblDesde.AutoSize = true;
        lblDesde.Location = new Point(320, 35);
        lblDesde.Name = "lblDesde";
        lblDesde.Size = new Size(55, 19);
        lblDesde.TabIndex = 2;
        lblDesde.Text = "Desde:";

        // dtpDesde
        dtpDesde.Format = DateTimePickerFormat.Short;
        dtpDesde.Location = new Point(320, 58);
        dtpDesde.Name = "dtpDesde";
        dtpDesde.Size = new Size(135, 25);
        dtpDesde.TabIndex = 3;

        // lblHasta
        lblHasta.AutoSize = true;
        lblHasta.Location = new Point(485, 35);
        lblHasta.Name = "lblHasta";
        lblHasta.Size = new Size(53, 19);
        lblHasta.TabIndex = 4;
        lblHasta.Text = "Hasta:";

        // dtpHasta
        dtpHasta.Format = DateTimePickerFormat.Short;
        dtpHasta.Location = new Point(485, 58);
        dtpHasta.Name = "dtpHasta";
        dtpHasta.Size = new Size(135, 25);
        dtpHasta.TabIndex = 5;

        // btnGenerar
        btnGenerar.Location = new Point(680, 48);
        btnGenerar.Name = "btnGenerar";
        btnGenerar.Size = new Size(190, 36);
        btnGenerar.TabIndex = 6;
        btnGenerar.Text = "Generar reporte";
        btnGenerar.UseVisualStyleBackColor = false;
        btnGenerar.Click += btnGenerar_Click;

        // grpResultado
        grpResultado.BackColor = Color.White;
        grpResultado.Controls.Add(dgvReportes);
        grpResultado.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpResultado.Location = new Point(20, 250);
        grpResultado.Name = "grpResultado";
        grpResultado.Size = new Size(940, 360);
        grpResultado.TabIndex = 2;
        grpResultado.TabStop = false;
        grpResultado.Text = "Resultado";

        // dgvReportes
        dgvReportes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvReportes.Dock = DockStyle.Fill;
        dgvReportes.Location = new Point(3, 21);
        dgvReportes.Name = "dgvReportes";
        dgvReportes.Size = new Size(934, 336);
        dgvReportes.TabIndex = 0;

        // lblResultado
        lblResultado.AutoSize = true;
        lblResultado.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblResultado.Location = new Point(25, 628);
        lblResultado.Name = "lblResultado";
        lblResultado.Size = new Size(161, 20);
        lblResultado.TabIndex = 3;
        lblResultado.Text = "Sin reporte cargado";

        // btnExportar
        btnExportar.Location = new Point(430, 620);
        btnExportar.Name = "btnExportar";
        btnExportar.Size = new Size(150, 36);
        btnExportar.TabIndex = 4;
        btnExportar.Text = "Exportar CSV";
        btnExportar.UseVisualStyleBackColor = false;
        btnExportar.Click += btnExportar_Click;

        // btnVistaPrevia
        btnVistaPrevia.Location = new Point(600, 620);
        btnVistaPrevia.Name = "btnVistaPrevia";
        btnVistaPrevia.Size = new Size(150, 36);
        btnVistaPrevia.TabIndex = 5;
        btnVistaPrevia.Text = "Vista previa";
        btnVistaPrevia.UseVisualStyleBackColor = false;
        btnVistaPrevia.Click += btnVistaPrevia_Click;

        // btnCerrar
        btnCerrar.Location = new Point(770, 620);
        btnCerrar.Name = "btnCerrar";
        btnCerrar.Size = new Size(150, 36);
        btnCerrar.TabIndex = 6;
        btnCerrar.Text = "Cerrar";
        btnCerrar.UseVisualStyleBackColor = false;
        btnCerrar.Click += btnCerrar_Click;

        // FrmReportes
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(250, 248, 242);
        BackgroundImage = Properties.Resources.fondoForms;
        BackgroundImageLayout = ImageLayout.Stretch;
        ClientSize = new Size(980, 680);
        Controls.Add(pnlHeader);
        Controls.Add(grpFiltros);
        Controls.Add(grpResultado);
        Controls.Add(lblResultado);
        Controls.Add(btnExportar);
        Controls.Add(btnVistaPrevia);
        Controls.Add(btnCerrar);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "FrmReportes";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "POP Cotillón - Reportes";
        Load += FrmReportes_Load;

        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        grpFiltros.ResumeLayout(false);
        grpFiltros.PerformLayout();
        grpResultado.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvReportes).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}

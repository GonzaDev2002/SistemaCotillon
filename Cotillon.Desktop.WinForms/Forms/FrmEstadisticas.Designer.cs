namespace Cotillon.Desktop.WinForms.Forms;

partial class FrmEstadisticas
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlHeader;
    private Label lblTitulo;
    private Label lblSubtitulo;

    private GroupBox grpFiltros;
    private Label lblTipoGrafico;
    private ComboBox cmbTipoGrafico;
    private Label lblDesde;
    private DateTimePicker dtpDesde;
    private Label lblHasta;
    private DateTimePicker dtpHasta;
    private Button btnGenerar;

    private GroupBox grpGrafico;
    private System.Windows.Forms.DataVisualization.Charting.Chart chartEstadisticas;

    private Label lblResumen;
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
        lblTipoGrafico = new Label();
        cmbTipoGrafico = new ComboBox();
        lblDesde = new Label();
        dtpDesde = new DateTimePicker();
        lblHasta = new Label();
        dtpHasta = new DateTimePicker();
        btnGenerar = new Button();
        grpGrafico = new GroupBox();
        chartEstadisticas = new System.Windows.Forms.DataVisualization.Charting.Chart();
        lblResumen = new Label();
        btnCerrar = new Button();

        pnlHeader.SuspendLayout();
        grpFiltros.SuspendLayout();
        grpGrafico.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)chartEstadisticas).BeginInit();
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
        lblTitulo.Size = new Size(300, 41);
        lblTitulo.TabIndex = 0;
        lblTitulo.Text = "📊 Estadísticas";

        // lblSubtitulo
        lblSubtitulo.AutoSize = true;
        lblSubtitulo.Font = new Font("Segoe UI", 11F);
        lblSubtitulo.ForeColor = Color.White;
        lblSubtitulo.Location = new Point(28, 58);
        lblSubtitulo.Name = "lblSubtitulo";
        lblSubtitulo.Size = new Size(418, 20);
        lblSubtitulo.TabIndex = 1;
        lblSubtitulo.Text = "Análisis visual de ventas, stock y medios de pago";

        // grpFiltros
        grpFiltros.BackColor = Color.White;
        grpFiltros.Controls.Add(lblTipoGrafico);
        grpFiltros.Controls.Add(cmbTipoGrafico);
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
        grpFiltros.Text = "Filtros del gráfico";

        // lblTipoGrafico
        lblTipoGrafico.AutoSize = true;
        lblTipoGrafico.Location = new Point(25, 35);
        lblTipoGrafico.Name = "lblTipoGrafico";
        lblTipoGrafico.Size = new Size(112, 19);
        lblTipoGrafico.TabIndex = 0;
        lblTipoGrafico.Text = "Tipo de gráfico:";

        // cmbTipoGrafico
        cmbTipoGrafico.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbTipoGrafico.Location = new Point(25, 58);
        cmbTipoGrafico.Name = "cmbTipoGrafico";
        cmbTipoGrafico.Size = new Size(280, 25);
        cmbTipoGrafico.TabIndex = 1;

        // lblDesde
        lblDesde.AutoSize = true;
        lblDesde.Location = new Point(335, 35);
        lblDesde.Name = "lblDesde";
        lblDesde.Size = new Size(55, 19);
        lblDesde.TabIndex = 2;
        lblDesde.Text = "Desde:";

        // dtpDesde
        dtpDesde.Format = DateTimePickerFormat.Short;
        dtpDesde.Location = new Point(335, 58);
        dtpDesde.Name = "dtpDesde";
        dtpDesde.Size = new Size(135, 25);
        dtpDesde.TabIndex = 3;

        // lblHasta
        lblHasta.AutoSize = true;
        lblHasta.Location = new Point(500, 35);
        lblHasta.Name = "lblHasta";
        lblHasta.Size = new Size(53, 19);
        lblHasta.TabIndex = 4;
        lblHasta.Text = "Hasta:";

        // dtpHasta
        dtpHasta.Format = DateTimePickerFormat.Short;
        dtpHasta.Location = new Point(500, 58);
        dtpHasta.Name = "dtpHasta";
        dtpHasta.Size = new Size(135, 25);
        dtpHasta.TabIndex = 5;

        // btnGenerar
        btnGenerar.Location = new Point(690, 48);
        btnGenerar.Name = "btnGenerar";
        btnGenerar.Size = new Size(190, 36);
        btnGenerar.TabIndex = 6;
        btnGenerar.Text = "Generar gráfico";
        btnGenerar.UseVisualStyleBackColor = false;
        btnGenerar.Click += btnGenerar_Click;

        // grpGrafico
        grpGrafico.BackColor = Color.White;
        grpGrafico.Controls.Add(chartEstadisticas);
        grpGrafico.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpGrafico.Location = new Point(20, 250);
        grpGrafico.Name = "grpGrafico";
        grpGrafico.Size = new Size(940, 390);
        grpGrafico.TabIndex = 2;
        grpGrafico.TabStop = false;
        grpGrafico.Text = "Gráfico";

        // chartEstadisticas
        chartEstadisticas.Dock = DockStyle.Fill;
        chartEstadisticas.Location = new Point(3, 21);
        chartEstadisticas.Name = "chartEstadisticas";
        chartEstadisticas.Size = new Size(934, 366);
        chartEstadisticas.TabIndex = 0;

        // lblResumen
        lblResumen.AutoSize = true;
        lblResumen.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblResumen.Location = new Point(25, 660);
        lblResumen.Name = "lblResumen";
        lblResumen.Size = new Size(156, 20);
        lblResumen.TabIndex = 3;
        lblResumen.Text = "Sin gráfico cargado";

        // btnCerrar
        btnCerrar.Location = new Point(810, 652);
        btnCerrar.Name = "btnCerrar";
        btnCerrar.Size = new Size(150, 36);
        btnCerrar.TabIndex = 4;
        btnCerrar.Text = "Cerrar";
        btnCerrar.UseVisualStyleBackColor = false;
        btnCerrar.Click += btnCerrar_Click;

        // FrmEstadisticas
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(250, 248, 242);
        BackgroundImage = Properties.Resources.fondoForms;
        BackgroundImageLayout = ImageLayout.Stretch;
        ClientSize = new Size(980, 710);
        Controls.Add(pnlHeader);
        Controls.Add(grpFiltros);
        Controls.Add(grpGrafico);
        Controls.Add(lblResumen);
        Controls.Add(btnCerrar);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "FrmEstadisticas";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "POP Cotillón - Estadísticas";
        Load += FrmEstadisticas_Load;

        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        grpFiltros.ResumeLayout(false);
        grpFiltros.PerformLayout();
        grpGrafico.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)chartEstadisticas).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}

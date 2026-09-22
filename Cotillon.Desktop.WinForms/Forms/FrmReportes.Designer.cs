namespace Cotillon.Desktop.WinForms.Forms
{
    partial class FrmReportes
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTipoReporte;
        private ComboBox cmbTipoReporte;
        private Label lblDesde;
        private DateTimePicker dtpDesde;
        private Label lblHasta;
        private DateTimePicker dtpHasta;
        private Button btnGenerar;
        private Button btnExportar;
        private Button btnVistaPrevia;
        private Button btnCerrar;
        private DataGridView dgvReportes;
        private Label lblResultado;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblTipoReporte = new Label();
            cmbTipoReporte = new ComboBox();
            lblDesde = new Label();
            dtpDesde = new DateTimePicker();
            lblHasta = new Label();
            dtpHasta = new DateTimePicker();
            btnGenerar = new Button();
            btnExportar = new Button();
            btnVistaPrevia = new Button();
            btnCerrar = new Button();
            dgvReportes = new DataGridView();
            lblResultado = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvReportes).BeginInit();
            SuspendLayout();
            // 
            // lblTipoReporte
            // 
            lblTipoReporte.AutoSize = true;
            lblTipoReporte.Location = new Point(20, 22);
            lblTipoReporte.Name = "lblTipoReporte";
            lblTipoReporte.Size = new Size(93, 15);
            lblTipoReporte.Text = "Tipo de reporte:";
            // 
            // cmbTipoReporte
            // 
            cmbTipoReporte.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipoReporte.FormattingEnabled = true;
            cmbTipoReporte.Location = new Point(120, 19);
            cmbTipoReporte.Name = "cmbTipoReporte";
            cmbTipoReporte.Size = new Size(220, 23);
            // 
            // lblDesde
            // 
            lblDesde.AutoSize = true;
            lblDesde.Location = new Point(370, 22);
            lblDesde.Name = "lblDesde";
            lblDesde.Size = new Size(43, 15);
            lblDesde.Text = "Desde:";
            // 
            // dtpDesde
            // 
            dtpDesde.Format = DateTimePickerFormat.Short;
            dtpDesde.Location = new Point(420, 19);
            dtpDesde.Name = "dtpDesde";
            dtpDesde.Size = new Size(110, 23);
            // 
            // lblHasta
            // 
            lblHasta.AutoSize = true;
            lblHasta.Location = new Point(550, 22);
            lblHasta.Name = "lblHasta";
            lblHasta.Size = new Size(40, 15);
            lblHasta.Text = "Hasta:";
            // 
            // dtpHasta
            // 
            dtpHasta.Format = DateTimePickerFormat.Short;
            dtpHasta.Location = new Point(600, 19);
            dtpHasta.Name = "dtpHasta";
            dtpHasta.Size = new Size(110, 23);
            // 
            // btnGenerar
            // 
            btnGenerar.Location = new Point(730, 17);
            btnGenerar.Name = "btnGenerar";
            btnGenerar.Size = new Size(120, 28);
            btnGenerar.Text = "Generar";
            btnGenerar.UseVisualStyleBackColor = true;
            btnGenerar.Click += btnGenerar_Click;
            // 
            // btnExportar
            // 
            btnExportar.Location = new Point(20, 430);
            btnExportar.Name = "btnExportar";
            btnExportar.Size = new Size(130, 32);
            btnExportar.Text = "Exportar CSV";
            btnExportar.UseVisualStyleBackColor = true;
            btnExportar.Click += btnExportar_Click;
            // 
            // btnVistaPrevia
            // 
            btnVistaPrevia.Location = new Point(165, 430);
            btnVistaPrevia.Name = "btnVistaPrevia";
            btnVistaPrevia.Size = new Size(130, 32);
            btnVistaPrevia.Text = "Vista previa";
            btnVistaPrevia.UseVisualStyleBackColor = true;
            btnVistaPrevia.Click += btnVistaPrevia_Click;
            // 
            // btnCerrar
            // 
            btnCerrar.Location = new Point(720, 430);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new Size(130, 32);
            btnCerrar.Text = "Cerrar";
            btnCerrar.UseVisualStyleBackColor = true;
            btnCerrar.Click += btnCerrar_Click;
            // 
            // dgvReportes
            // 
            dgvReportes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvReportes.Location = new Point(20, 70);
            dgvReportes.Name = "dgvReportes";
            dgvReportes.Size = new Size(830, 340);
            dgvReportes.TabIndex = 10;
            // 
            // lblResultado
            // 
            lblResultado.AutoSize = true;
            lblResultado.Location = new Point(315, 439);
            lblResultado.Name = "lblResultado";
            lblResultado.Size = new Size(104, 15);
            lblResultado.Text = "Sin reporte cargado";
            // 
            // FrmReportes
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(870, 480);
            Controls.Add(lblResultado);
            Controls.Add(dgvReportes);
            Controls.Add(btnCerrar);
            Controls.Add(btnVistaPrevia);
            Controls.Add(btnExportar);
            Controls.Add(btnGenerar);
            Controls.Add(dtpHasta);
            Controls.Add(lblHasta);
            Controls.Add(dtpDesde);
            Controls.Add(lblDesde);
            Controls.Add(cmbTipoReporte);
            Controls.Add(lblTipoReporte);
            Name = "FrmReportes";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Reportes e Informes";
            Load += FrmReportes_Load;
            ((System.ComponentModel.ISupportInitialize)dgvReportes).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
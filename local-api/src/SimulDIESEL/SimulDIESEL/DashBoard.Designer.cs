namespace SimulDIESEL
{
    partial class DashBoard
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DashBoard));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsLabelSerial = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsLedSerial = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsSeparador1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsLabeLink = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsLedLink = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripConectar = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsLabelSerial,
            this.tsLedSerial,
            this.tsSeparador1,
            this.tsLabeLink,
            this.tsLedLink});
            this.statusStrip1.Location = new System.Drawing.Point(0, 505);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 13, 0);
            this.statusStrip1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.statusStrip1.Size = new System.Drawing.Size(857, 26);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsLabelSerial
            // 
            this.tsLabelSerial.Name = "tsLabelSerial";
            this.tsLabelSerial.Size = new System.Drawing.Size(114, 20);
            this.tsLabelSerial.Text = "Status da Serial:";
            // 
            // tsLedSerial
            // 
            this.tsLedSerial.Image = global::SimulDIESEL.Properties.Resources.LedRedDark_18x18;
            this.tsLedSerial.Name = "tsLedSerial";
            this.tsLedSerial.Size = new System.Drawing.Size(20, 20);
            // 
            // tsSeparador1
            // 
            this.tsSeparador1.Name = "tsSeparador1";
            this.tsSeparador1.Size = new System.Drawing.Size(13, 20);
            this.tsSeparador1.Text = "|";
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(857, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripConectar});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(857, 47);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsLabeLink
            // 
            this.tsLabeLink.Name = "tsLabeLink";
            this.tsLabeLink.Size = new System.Drawing.Size(104, 20);
            this.tsLabeLink.Text = "Status do Link:";
            // 
            // tsLedLink
            // 
            this.tsLedLink.Image = global::SimulDIESEL.Properties.Resources.LedRedDark_18x18;
            this.tsLedLink.Name = "tsLedLink";
            this.tsLedLink.Size = new System.Drawing.Size(20, 20);
            // 
            // toolStripConectar
            // 
            this.toolStripConectar.Image = ((System.Drawing.Image)(resources.GetObject("toolStripConectar.Image")));
            this.toolStripConectar.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripConectar.Name = "toolStripConectar";
            this.toolStripConectar.Size = new System.Drawing.Size(72, 44);
            this.toolStripConectar.Text = "Conectar";
            this.toolStripConectar.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolStripConectar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolStripConectar.Click += new System.EventHandler(this.toolStripConectar_Click);
            // 
            // DashBoard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(857, 531);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "DashBoard";
            this.Text = "SimulDIESEL";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripConectar;
        private System.Windows.Forms.ToolStripStatusLabel tsLabelSerial;
        private System.Windows.Forms.ToolStripStatusLabel tsLedSerial;
        private System.Windows.Forms.ToolStripStatusLabel tsSeparador1;
        private System.Windows.Forms.ToolStripStatusLabel tsLabeLink;
        private System.Windows.Forms.ToolStripStatusLabel tsLedLink;
    }
}


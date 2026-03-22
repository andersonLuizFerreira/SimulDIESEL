namespace SimulDIESEL.UI
{
    partial class frmGSA_UI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._gsaControls = new SimulDIESEL.UI.Controls.GsaControls();
            this.SuspendLayout();
            // 
            // _gsaControls
            // 
            this._gsaControls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(31)))), ((int)(((byte)(35)))));
            this._gsaControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this._gsaControls.Location = new System.Drawing.Point(0, 0);
            this._gsaControls.Name = "_gsaControls";
            this._gsaControls.Size = new System.Drawing.Size(1604, 860);
            this._gsaControls.TabIndex = 0;
            // 
            // frmGSA_UI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(31)))), ((int)(((byte)(35)))));
            this.ClientSize = new System.Drawing.Size(1604, 860);
            this.Controls.Add(this._gsaControls);
            this.Name = "frmGSA_UI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GSA - GERADOR DE SINAIS ANALOGICOS";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.GsaControls _gsaControls;
    }
}

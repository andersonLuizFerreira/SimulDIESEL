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
            this._toolbarPanel = new System.Windows.Forms.Panel();
            this._builtinLedCheckBox = new System.Windows.Forms.CheckBox();
            this._gsaControls = new SimulDIESEL.UI.Controls.GsaControls();
            this._toolbarPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _toolbarPanel
            // 
            this._toolbarPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(31)))), ((int)(((byte)(35)))));
            this._toolbarPanel.Controls.Add(this._builtinLedCheckBox);
            this._toolbarPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._toolbarPanel.Location = new System.Drawing.Point(0, 0);
            this._toolbarPanel.Name = "_toolbarPanel";
            this._toolbarPanel.Padding = new System.Windows.Forms.Padding(18, 10, 18, 10);
            this._toolbarPanel.Size = new System.Drawing.Size(1604, 46);
            this._toolbarPanel.TabIndex = 0;
            // 
            // _builtinLedCheckBox
            // 
            this._builtinLedCheckBox.AutoSize = true;
            this._builtinLedCheckBox.ForeColor = System.Drawing.Color.Gainsboro;
            this._builtinLedCheckBox.Location = new System.Drawing.Point(21, 13);
            this._builtinLedCheckBox.Name = "_builtinLedCheckBox";
            this._builtinLedCheckBox.Size = new System.Drawing.Size(129, 20);
            this._builtinLedCheckBox.TabIndex = 0;
            this._builtinLedCheckBox.Text = "LED_BUILTIN GSA";
            this._builtinLedCheckBox.UseVisualStyleBackColor = true;
            // 
            // _gsaControls
            // 
            this._gsaControls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(31)))), ((int)(((byte)(35)))));
            this._gsaControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this._gsaControls.Location = new System.Drawing.Point(0, 46);
            this._gsaControls.Name = "_gsaControls";
            this._gsaControls.Size = new System.Drawing.Size(1604, 814);
            this._gsaControls.TabIndex = 1;
            // 
            // frmGSA_UI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(31)))), ((int)(((byte)(35)))));
            this.ClientSize = new System.Drawing.Size(1604, 860);
            this.Controls.Add(this._gsaControls);
            this.Controls.Add(this._toolbarPanel);
            this.Name = "frmGSA_UI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GSA - GERADOR DE SINAIS ANALOGICOS";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this._toolbarPanel.ResumeLayout(false);
            this._toolbarPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _toolbarPanel;
        private System.Windows.Forms.CheckBox _builtinLedCheckBox;
        private Controls.GsaControls _gsaControls;
    }
}

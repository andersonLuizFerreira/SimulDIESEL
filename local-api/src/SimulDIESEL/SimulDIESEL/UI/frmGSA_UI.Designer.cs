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
            this._physicalResultLabel = new System.Windows.Forms.Label();
            this._builtinLedCheckBox = new System.Windows.Forms.CheckBox();
            this._gsaControls = new SimulDIESEL.UI.Controls.GsaControls();
            this._toolbarPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _toolbarPanel
            // 
            this._toolbarPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(31)))), ((int)(((byte)(35)))));
            this._toolbarPanel.Controls.Add(this._physicalResultLabel);
            this._toolbarPanel.Controls.Add(this._builtinLedCheckBox);
            this._toolbarPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._toolbarPanel.Location = new System.Drawing.Point(0, 0);
            this._toolbarPanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._toolbarPanel.Name = "_toolbarPanel";
            this._toolbarPanel.Padding = new System.Windows.Forms.Padding(14, 8, 14, 8);
            this._toolbarPanel.Size = new System.Drawing.Size(1203, 47);
            this._toolbarPanel.TabIndex = 0;
            // 
            // _physicalResultLabel
            // 
            this._physicalResultLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._physicalResultLabel.ForeColor = System.Drawing.Color.Silver;
            this._physicalResultLabel.Location = new System.Drawing.Point(140, 11);
            this._physicalResultLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this._physicalResultLabel.Name = "_physicalResultLabel";
            this._physicalResultLabel.Size = new System.Drawing.Size(1048, 26);
            this._physicalResultLabel.TabIndex = 1;
            this._physicalResultLabel.Text = "Resultado físico: aguardando operação da GSA.";
            this._physicalResultLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _builtinLedCheckBox
            // 
            this._builtinLedCheckBox.AutoSize = true;
            this._builtinLedCheckBox.ForeColor = System.Drawing.Color.Gainsboro;
            this._builtinLedCheckBox.Location = new System.Drawing.Point(16, 11);
            this._builtinLedCheckBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._builtinLedCheckBox.Name = "_builtinLedCheckBox";
            this._builtinLedCheckBox.Size = new System.Drawing.Size(120, 17);
            this._builtinLedCheckBox.TabIndex = 0;
            this._builtinLedCheckBox.Text = "LED_BUILTIN GSA";
            this._builtinLedCheckBox.UseVisualStyleBackColor = true;
            // 
            // _gsaControls
            // 
            this._gsaControls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(31)))), ((int)(((byte)(35)))));
            this._gsaControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this._gsaControls.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._gsaControls.ForeColor = System.Drawing.Color.Gainsboro;
            this._gsaControls.Location = new System.Drawing.Point(0, 47);
            this._gsaControls.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._gsaControls.Name = "_gsaControls";
            this._gsaControls.Size = new System.Drawing.Size(1203, 652);
            this._gsaControls.TabIndex = 1;
            this._gsaControls.Click += new System.EventHandler(this._gsaControls_Click);
            // 
            // frmGSA_UI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(31)))), ((int)(((byte)(35)))));
            this.ClientSize = new System.Drawing.Size(1203, 699);
            this.Controls.Add(this._gsaControls);
            this.Controls.Add(this._toolbarPanel);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
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
        private System.Windows.Forms.Label _physicalResultLabel;
        private System.Windows.Forms.CheckBox _builtinLedCheckBox;
        private Controls.GsaControls _gsaControls;
    }
}

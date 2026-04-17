namespace SimulDIESEL.UI
{
    partial class frmUCE_UI
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
            this.grpLed = new System.Windows.Forms.GroupBox();
            this.chkLed = new System.Windows.Forms.CheckBox();
            this.grpCan = new System.Windows.Forms.GroupBox();
            this.lblCanStatus = new System.Windows.Forms.Label();
            this.cmbCanMode = new System.Windows.Forms.ComboBox();
            this.lblCanMode = new System.Windows.Forms.Label();
            this.cmbCanSpeed = new System.Windows.Forms.ComboBox();
            this.lblCanSpeed = new System.Windows.Forms.Label();
            this.chkCanEnabled = new System.Windows.Forms.CheckBox();
            this.grpLed.SuspendLayout();
            this.grpCan.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpLed
            // 
            this.grpLed.Controls.Add(this.chkLed);
            this.grpLed.Location = new System.Drawing.Point(12, 12);
            this.grpLed.Name = "grpLed";
            this.grpLed.Size = new System.Drawing.Size(160, 105);
            this.grpLed.TabIndex = 0;
            this.grpLed.TabStop = false;
            this.grpLed.Text = "LED";
            // 
            // chkLed
            // 
            this.chkLed.AutoSize = true;
            this.chkLed.Location = new System.Drawing.Point(16, 32);
            this.chkLed.Name = "chkLed";
            this.chkLed.Size = new System.Drawing.Size(47, 19);
            this.chkLed.TabIndex = 0;
            this.chkLed.Text = "LED";
            this.chkLed.UseVisualStyleBackColor = true;
            // 
            // grpCan
            // 
            this.grpCan.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpCan.Controls.Add(this.lblCanStatus);
            this.grpCan.Controls.Add(this.cmbCanMode);
            this.grpCan.Controls.Add(this.lblCanMode);
            this.grpCan.Controls.Add(this.cmbCanSpeed);
            this.grpCan.Controls.Add(this.lblCanSpeed);
            this.grpCan.Controls.Add(this.chkCanEnabled);
            this.grpCan.Location = new System.Drawing.Point(178, 12);
            this.grpCan.Name = "grpCan";
            this.grpCan.Size = new System.Drawing.Size(394, 187);
            this.grpCan.TabIndex = 1;
            this.grpCan.TabStop = false;
            this.grpCan.Text = "Porta CAN";
            // 
            // lblCanStatus
            // 
            this.lblCanStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCanStatus.Location = new System.Drawing.Point(16, 132);
            this.lblCanStatus.Name = "lblCanStatus";
            this.lblCanStatus.Size = new System.Drawing.Size(362, 40);
            this.lblCanStatus.TabIndex = 5;
            this.lblCanStatus.Text = "Status CAN: aguardando integração com a UCE.";
            // 
            // cmbCanMode
            // 
            this.cmbCanMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCanMode.FormattingEnabled = true;
            this.cmbCanMode.Items.AddRange(new object[] {
            "Normal",
            "Listen"});
            this.cmbCanMode.Location = new System.Drawing.Point(104, 90);
            this.cmbCanMode.Name = "cmbCanMode";
            this.cmbCanMode.Size = new System.Drawing.Size(154, 23);
            this.cmbCanMode.TabIndex = 4;
            // 
            // lblCanMode
            // 
            this.lblCanMode.AutoSize = true;
            this.lblCanMode.Location = new System.Drawing.Point(16, 93);
            this.lblCanMode.Name = "lblCanMode";
            this.lblCanMode.Size = new System.Drawing.Size(40, 15);
            this.lblCanMode.TabIndex = 3;
            this.lblCanMode.Text = "Modo";
            // 
            // cmbCanSpeed
            // 
            this.cmbCanSpeed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCanSpeed.FormattingEnabled = true;
            this.cmbCanSpeed.Items.AddRange(new object[] {
            "125 kbps",
            "250 kbps",
            "500 kbps",
            "1 Mbps"});
            this.cmbCanSpeed.Location = new System.Drawing.Point(104, 55);
            this.cmbCanSpeed.Name = "cmbCanSpeed";
            this.cmbCanSpeed.Size = new System.Drawing.Size(154, 23);
            this.cmbCanSpeed.TabIndex = 2;
            // 
            // lblCanSpeed
            // 
            this.lblCanSpeed.AutoSize = true;
            this.lblCanSpeed.Location = new System.Drawing.Point(16, 58);
            this.lblCanSpeed.Name = "lblCanSpeed";
            this.lblCanSpeed.Size = new System.Drawing.Size(65, 15);
            this.lblCanSpeed.TabIndex = 1;
            this.lblCanSpeed.Text = "Velocidade";
            // 
            // chkCanEnabled
            // 
            this.chkCanEnabled.AutoSize = true;
            this.chkCanEnabled.Location = new System.Drawing.Point(19, 27);
            this.chkCanEnabled.Name = "chkCanEnabled";
            this.chkCanEnabled.Size = new System.Drawing.Size(85, 19);
            this.chkCanEnabled.TabIndex = 0;
            this.chkCanEnabled.Text = "Porta ativa";
            this.chkCanEnabled.UseVisualStyleBackColor = true;
            // 
            // frmUCE_UI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 211);
            this.Controls.Add(this.grpCan);
            this.Controls.Add(this.grpLed);
            this.MinimumSize = new System.Drawing.Size(600, 250);
            this.Name = "frmUCE_UI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "UCE - Unidade de Comunicacao Externa";
            this.grpLed.ResumeLayout(false);
            this.grpLed.PerformLayout();
            this.grpCan.ResumeLayout(false);
            this.grpCan.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpLed;
        private System.Windows.Forms.CheckBox chkLed;
        private System.Windows.Forms.GroupBox grpCan;
        private System.Windows.Forms.CheckBox chkCanEnabled;
        private System.Windows.Forms.Label lblCanSpeed;
        private System.Windows.Forms.ComboBox cmbCanSpeed;
        private System.Windows.Forms.Label lblCanMode;
        private System.Windows.Forms.ComboBox cmbCanMode;
        private System.Windows.Forms.Label lblCanStatus;
    }
}

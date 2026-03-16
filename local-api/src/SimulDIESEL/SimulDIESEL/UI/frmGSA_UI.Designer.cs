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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.rdoLedOff = new System.Windows.Forms.RadioButton();
            this.rdoLedOn = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rdoLedOn);
            this.groupBox1.Controls.Add(this.rdoLedOff);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(24, 26);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(192, 141);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(161, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Controle do LED da placa";
            // 
            // rdoLedOff
            // 
            this.rdoLedOff.AutoSize = true;
            this.rdoLedOff.Checked = true;
            this.rdoLedOff.Location = new System.Drawing.Point(21, 48);
            this.rdoLedOff.Name = "rdoLedOff";
            this.rdoLedOff.Size = new System.Drawing.Size(91, 20);
            this.rdoLedOff.TabIndex = 1;
            this.rdoLedOff.Text = "Desligado";
            this.rdoLedOff.UseVisualStyleBackColor = true;
            // 
            // rdoLedOn
            // 
            this.rdoLedOn.AutoSize = true;
            this.rdoLedOn.Location = new System.Drawing.Point(21, 91);
            this.rdoLedOn.Name = "rdoLedOn";
            this.rdoLedOn.Size = new System.Drawing.Size(70, 20);
            this.rdoLedOn.TabIndex = 2;
            this.rdoLedOn.Text = "Ligado";
            this.rdoLedOn.UseVisualStyleBackColor = true;
            // 
            // frmGSA_UI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.groupBox1);
            this.Name = "frmGSA_UI";
            this.Text = "GSA - GERADOR DE SINAIS ANALOGICOS";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton rdoLedOn;
        private System.Windows.Forms.RadioButton rdoLedOff;
    }
}
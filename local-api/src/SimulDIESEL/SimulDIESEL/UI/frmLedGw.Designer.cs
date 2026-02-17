using SimulDIESEL.BLL;
using SimulDIESEL.DTL;


namespace SimulDIESEL.UI
{
    partial class frmLedGw
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
            this.btnFechar = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.imgLED = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnTogle = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgLED)).BeginInit();
            this.SuspendLayout();
            // 
            // btnFechar
            // 
            this.btnFechar.Location = new System.Drawing.Point(197, 97);
            this.btnFechar.Name = "btnFechar";
            this.btnFechar.Size = new System.Drawing.Size(75, 23);
            this.btnFechar.TabIndex = 0;
            this.btnFechar.Text = "FECHAR";
            this.btnFechar.UseVisualStyleBackColor = true;
            this.btnFechar.Click += new System.EventHandler(this.btnFechar_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.imgLED);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(260, 79);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // imgLED
            // 
            this.imgLED.Image = global::SimulDIESEL.Properties.Resources.LedRedDark_18x18;
            this.imgLED.Location = new System.Drawing.Point(11, 42);
            this.imgLED.Name = "imgLED";
            this.imgLED.Size = new System.Drawing.Size(20, 20);
            this.imgLED.TabIndex = 0;
            this.imgLED.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Status do LED:";
            // 
            // btnTogle
            // 
            this.btnTogle.Location = new System.Drawing.Point(12, 97);
            this.btnTogle.Name = "btnTogle";
            this.btnTogle.Size = new System.Drawing.Size(75, 23);
            this.btnTogle.TabIndex = 2;
            this.btnTogle.Text = "LIGA";
            this.btnTogle.UseVisualStyleBackColor = true;
            this.btnTogle.Click += new System.EventHandler(this.btnTogle_Click);
            // 
            // frmLedGw
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(286, 133);
            this.Controls.Add(this.btnTogle);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnFechar);
            this.Name = "frmLedGw";
            this.Text = "Teste de LED";
            this.Load += new System.EventHandler(this.frmLedGw_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgLED)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnFechar;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.PictureBox imgLED;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnTogle;
    }
}
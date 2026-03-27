namespace SimulDIESEL.UI
{
    partial class frmBluetoothConnect
    {
        private System.ComponentModel.IContainer components = null;

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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtDeviceName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnBluetoothConnect = new System.Windows.Forms.Button();
            this.cboBluetoothPortas = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtDeviceName);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.btnBluetoothConnect);
            this.groupBox1.Controls.Add(this.cboBluetoothPortas);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(23, 22);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(314, 171);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Bluetooth SPP";
            // 
            // txtDeviceName
            // 
            this.txtDeviceName.Location = new System.Drawing.Point(10, 95);
            this.txtDeviceName.Name = "txtDeviceName";
            this.txtDeviceName.Size = new System.Drawing.Size(293, 20);
            this.txtDeviceName.TabIndex = 2;
            this.txtDeviceName.Text = "SimulDIESEL-BPM";
            this.txtDeviceName.TextChanged += new System.EventHandler(this.txtDeviceName_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 79);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(122, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Nome do dispositivo BT:";
            // 
            // btnBluetoothConnect
            // 
            this.btnBluetoothConnect.Enabled = false;
            this.btnBluetoothConnect.Location = new System.Drawing.Point(10, 129);
            this.btnBluetoothConnect.Name = "btnBluetoothConnect";
            this.btnBluetoothConnect.Size = new System.Drawing.Size(293, 27);
            this.btnBluetoothConnect.TabIndex = 3;
            this.btnBluetoothConnect.Text = "&Conectar";
            this.btnBluetoothConnect.UseVisualStyleBackColor = true;
            this.btnBluetoothConnect.Click += new System.EventHandler(this.btnBluetoothConnect_Click);
            // 
            // cboBluetoothPortas
            // 
            this.cboBluetoothPortas.FormattingEnabled = true;
            this.cboBluetoothPortas.Location = new System.Drawing.Point(10, 41);
            this.cboBluetoothPortas.Name = "cboBluetoothPortas";
            this.cboBluetoothPortas.Size = new System.Drawing.Size(293, 21);
            this.cboBluetoothPortas.TabIndex = 1;
            this.cboBluetoothPortas.SelectedIndexChanged += new System.EventHandler(this.cboBluetoothPortas_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(164, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Porta COM atribuida ao Bluetooth:";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 217);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(360, 22);
            this.statusStrip1.TabIndex = 1;
            // 
            // frmBluetoothConnect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(360, 239);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.groupBox1);
            this.Name = "frmBluetoothConnect";
            this.Text = "Conexao Bluetooth";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtDeviceName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnBluetoothConnect;
        private System.Windows.Forms.ComboBox cboBluetoothPortas;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.StatusStrip statusStrip1;
    }
}

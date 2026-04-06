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
            this.lblHint = new System.Windows.Forms.Label();
            this.lvBluetoothDevices = new System.Windows.Forms.ListView();
            this.colDeviceName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPort = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label2 = new System.Windows.Forms.Label();
            this.btnConectar = new System.Windows.Forms.Button();
            this.btnAtualizar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblHint);
            this.groupBox1.Controls.Add(this.lvBluetoothDevices);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.btnConectar);
            this.groupBox1.Controls.Add(this.btnAtualizar);
            this.groupBox1.Controls.Add(this.btnCancelar);
            this.groupBox1.Location = new System.Drawing.Point(23, 22);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(624, 320);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Dispositivos Bluetooth";
            // 
            // lblHint
            // 
            this.lblHint.AutoSize = true;
            this.lblHint.Location = new System.Drawing.Point(7, 25);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(405, 13);
            this.lblHint.TabIndex = 0;
            this.lblHint.Text = "Somente dispositivos pareados com porta COM SPP no Windows aparecem como conectaveis.";
            // 
            // lvBluetoothDevices
            // 
            this.lvBluetoothDevices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colDeviceName,
            this.colAddress,
            this.colPort,
            this.colStatus});
            this.lvBluetoothDevices.FullRowSelect = true;
            this.lvBluetoothDevices.GridLines = true;
            this.lvBluetoothDevices.HideSelection = false;
            this.lvBluetoothDevices.Location = new System.Drawing.Point(10, 48);
            this.lvBluetoothDevices.MultiSelect = false;
            this.lvBluetoothDevices.Name = "lvBluetoothDevices";
            this.lvBluetoothDevices.Size = new System.Drawing.Size(603, 218);
            this.lvBluetoothDevices.TabIndex = 1;
            this.lvBluetoothDevices.UseCompatibleStateImageBehavior = false;
            this.lvBluetoothDevices.View = System.Windows.Forms.View.Details;
            this.lvBluetoothDevices.SelectedIndexChanged += new System.EventHandler(this.lvBluetoothDevices_SelectedIndexChanged);
            // 
            // colDeviceName
            // 
            this.colDeviceName.Text = "Dispositivo";
            this.colDeviceName.Width = 180;
            // 
            // colAddress
            // 
            this.colAddress.Text = "Endereco";
            this.colAddress.Width = 130;
            // 
            // colPort
            // 
            this.colPort.Text = "Porta";
            this.colPort.Width = 80;
            // 
            // colStatus
            // 
            this.colStatus.Text = "Status";
            this.colStatus.Width = 190;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 279);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(221, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Selecione um dispositivo Bluetooth e conecte.";
            // 
            // btnConectar
            // 
            this.btnConectar.Enabled = false;
            this.btnConectar.Location = new System.Drawing.Point(429, 274);
            this.btnConectar.Name = "btnConectar";
            this.btnConectar.Size = new System.Drawing.Size(89, 27);
            this.btnConectar.TabIndex = 2;
            this.btnConectar.Text = "&Conectar";
            this.btnConectar.UseVisualStyleBackColor = true;
            this.btnConectar.Click += new System.EventHandler(this.btnConectar_Click);
            // 
            // btnAtualizar
            // 
            this.btnAtualizar.Location = new System.Drawing.Point(334, 274);
            this.btnAtualizar.Name = "btnAtualizar";
            this.btnAtualizar.Size = new System.Drawing.Size(89, 27);
            this.btnAtualizar.TabIndex = 4;
            this.btnAtualizar.Text = "&Atualizar";
            this.btnAtualizar.UseVisualStyleBackColor = true;
            this.btnAtualizar.Click += new System.EventHandler(this.btnAtualizar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Location = new System.Drawing.Point(524, 274);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(89, 27);
            this.btnCancelar.TabIndex = 5;
            this.btnCancelar.Text = "Ca&ncelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 345);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(671, 22);
            this.statusStrip1.TabIndex = 1;
            // 
            // frmBluetoothConnect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(671, 367);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.groupBox1);
            this.Name = "frmBluetoothConnect";
            this.Text = "Selecionar dispositivo Bluetooth";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.ListView lvBluetoothDevices;
        private System.Windows.Forms.ColumnHeader colDeviceName;
        private System.Windows.Forms.ColumnHeader colAddress;
        private System.Windows.Forms.ColumnHeader colPort;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnConectar;
        private System.Windows.Forms.Button btnAtualizar;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.StatusStrip statusStrip1;
    }
}

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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.txtD0 = new System.Windows.Forms.TextBox();
            this.txtD1 = new System.Windows.Forms.TextBox();
            this.txtD3 = new System.Windows.Forms.TextBox();
            this.txtD2 = new System.Windows.Forms.TextBox();
            this.txtD5 = new System.Windows.Forms.TextBox();
            this.txtD4 = new System.Windows.Forms.TextBox();
            this.txtD7 = new System.Windows.Forms.TextBox();
            this.txtD6 = new System.Windows.Forms.TextBox();
            this.btnEnable = new System.Windows.Forms.Button();
            this.grpLed.SuspendLayout();
            this.grpCan.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpLed
            // 
            this.grpLed.Controls.Add(this.chkLed);
            this.grpLed.Location = new System.Drawing.Point(14, 13);
            this.grpLed.Name = "grpLed";
            this.grpLed.Size = new System.Drawing.Size(183, 184);
            this.grpLed.TabIndex = 0;
            this.grpLed.TabStop = false;
            this.grpLed.Text = "LED";
            // 
            // chkLed
            // 
            this.chkLed.AutoSize = true;
            this.chkLed.Location = new System.Drawing.Point(18, 34);
            this.chkLed.Name = "chkLed";
            this.chkLed.Size = new System.Drawing.Size(55, 20);
            this.chkLed.TabIndex = 0;
            this.chkLed.Text = "LED";
            this.chkLed.UseVisualStyleBackColor = true;
            // 
            // grpCan
            // 
            this.grpCan.Controls.Add(this.lblCanStatus);
            this.grpCan.Controls.Add(this.cmbCanMode);
            this.grpCan.Controls.Add(this.lblCanMode);
            this.grpCan.Controls.Add(this.cmbCanSpeed);
            this.grpCan.Controls.Add(this.lblCanSpeed);
            this.grpCan.Controls.Add(this.chkCanEnabled);
            this.grpCan.Location = new System.Drawing.Point(203, 13);
            this.grpCan.Name = "grpCan";
            this.grpCan.Size = new System.Drawing.Size(358, 184);
            this.grpCan.TabIndex = 1;
            this.grpCan.TabStop = false;
            this.grpCan.Text = "Porta CAN";
            // 
            // lblCanStatus
            // 
            this.lblCanStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCanStatus.Location = new System.Drawing.Point(18, 141);
            this.lblCanStatus.Name = "lblCanStatus";
            this.lblCanStatus.Size = new System.Drawing.Size(322, 27);
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
            this.cmbCanMode.Location = new System.Drawing.Point(119, 96);
            this.cmbCanMode.Name = "cmbCanMode";
            this.cmbCanMode.Size = new System.Drawing.Size(175, 24);
            this.cmbCanMode.TabIndex = 4;
            // 
            // lblCanMode
            // 
            this.lblCanMode.AutoSize = true;
            this.lblCanMode.Location = new System.Drawing.Point(18, 99);
            this.lblCanMode.Name = "lblCanMode";
            this.lblCanMode.Size = new System.Drawing.Size(42, 16);
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
            this.cmbCanSpeed.Location = new System.Drawing.Point(119, 59);
            this.cmbCanSpeed.Name = "cmbCanSpeed";
            this.cmbCanSpeed.Size = new System.Drawing.Size(175, 24);
            this.cmbCanSpeed.TabIndex = 2;
            // 
            // lblCanSpeed
            // 
            this.lblCanSpeed.AutoSize = true;
            this.lblCanSpeed.Location = new System.Drawing.Point(18, 62);
            this.lblCanSpeed.Name = "lblCanSpeed";
            this.lblCanSpeed.Size = new System.Drawing.Size(77, 16);
            this.lblCanSpeed.TabIndex = 1;
            this.lblCanSpeed.Text = "Velocidade";
            // 
            // chkCanEnabled
            // 
            this.chkCanEnabled.AutoSize = true;
            this.chkCanEnabled.Location = new System.Drawing.Point(22, 29);
            this.chkCanEnabled.Name = "chkCanEnabled";
            this.chkCanEnabled.Size = new System.Drawing.Size(93, 20);
            this.chkCanEnabled.TabIndex = 0;
            this.chkCanEnabled.Text = "Porta ativa";
            this.chkCanEnabled.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Location = new System.Drawing.Point(14, 203);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(547, 362);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Mensagens CAN";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnEnable);
            this.groupBox2.Controls.Add(this.txtD7);
            this.groupBox2.Controls.Add(this.txtD6);
            this.groupBox2.Controls.Add(this.txtD5);
            this.groupBox2.Controls.Add(this.txtD4);
            this.groupBox2.Controls.Add(this.txtD3);
            this.groupBox2.Controls.Add(this.txtD2);
            this.groupBox2.Controls.Add(this.txtD1);
            this.groupBox2.Controls.Add(this.txtD0);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.textBox3);
            this.groupBox2.Controls.Add(this.textBox2);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.comboBox1);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.textBox1);
            this.groupBox2.Location = new System.Drawing.Point(6, 32);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(513, 250);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Enviar";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 68);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 16);
            this.label1.TabIndex = 6;
            this.label1.Text = "ID";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(53, 65);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(427, 22);
            this.textBox1.TabIndex = 5;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "1.0",
            "2.0"});
            this.comboBox1.Location = new System.Drawing.Point(145, 31);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(53, 24);
            this.comboBox1.TabIndex = 7;
            this.comboBox1.Text = "2.0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(125, 16);
            this.label2.TabIndex = 8;
            this.label2.Text = "Tipo de Mensagem";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(221, 34);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(159, 16);
            this.label3.TabIndex = 9;
            this.label3.Text = "Tempo de repetição (ms)";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(386, 34);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(94, 22);
            this.textBox2.TabIndex = 10;
            this.textBox2.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 93);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(33, 16);
            this.label4.TabIndex = 12;
            this.label4.Text = "LEN";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(53, 93);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(427, 22);
            this.textBox3.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(57, 129);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(24, 16);
            this.label5.TabIndex = 13;
            this.label5.Text = "D0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(114, 129);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(24, 16);
            this.label6.TabIndex = 14;
            this.label6.Text = "D1";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(228, 129);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(24, 16);
            this.label7.TabIndex = 16;
            this.label7.Text = "D3";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(171, 129);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(24, 16);
            this.label8.TabIndex = 15;
            this.label8.Text = "D2";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(456, 129);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(24, 16);
            this.label9.TabIndex = 20;
            this.label9.Text = "D7";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(399, 129);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(24, 16);
            this.label10.TabIndex = 19;
            this.label10.Text = "D6";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(342, 129);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(24, 16);
            this.label11.TabIndex = 18;
            this.label11.Text = "D5";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(285, 129);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(24, 16);
            this.label12.TabIndex = 17;
            this.label12.Text = "D4";
            // 
            // txtD0
            // 
            this.txtD0.Location = new System.Drawing.Point(60, 148);
            this.txtD0.Name = "txtD0";
            this.txtD0.Size = new System.Drawing.Size(21, 22);
            this.txtD0.TabIndex = 21;
            // 
            // txtD1
            // 
            this.txtD1.Location = new System.Drawing.Point(117, 148);
            this.txtD1.Name = "txtD1";
            this.txtD1.Size = new System.Drawing.Size(21, 22);
            this.txtD1.TabIndex = 22;
            // 
            // txtD3
            // 
            this.txtD3.Location = new System.Drawing.Point(231, 148);
            this.txtD3.Name = "txtD3";
            this.txtD3.Size = new System.Drawing.Size(21, 22);
            this.txtD3.TabIndex = 24;
            // 
            // txtD2
            // 
            this.txtD2.Location = new System.Drawing.Point(174, 148);
            this.txtD2.Name = "txtD2";
            this.txtD2.Size = new System.Drawing.Size(21, 22);
            this.txtD2.TabIndex = 23;
            // 
            // txtD5
            // 
            this.txtD5.Location = new System.Drawing.Point(345, 148);
            this.txtD5.Name = "txtD5";
            this.txtD5.Size = new System.Drawing.Size(21, 22);
            this.txtD5.TabIndex = 26;
            // 
            // txtD4
            // 
            this.txtD4.Location = new System.Drawing.Point(288, 148);
            this.txtD4.Name = "txtD4";
            this.txtD4.Size = new System.Drawing.Size(21, 22);
            this.txtD4.TabIndex = 25;
            // 
            // txtD7
            // 
            this.txtD7.Location = new System.Drawing.Point(459, 148);
            this.txtD7.Name = "txtD7";
            this.txtD7.Size = new System.Drawing.Size(21, 22);
            this.txtD7.TabIndex = 28;
            // 
            // txtD6
            // 
            this.txtD6.Location = new System.Drawing.Point(402, 148);
            this.txtD6.Name = "txtD6";
            this.txtD6.Size = new System.Drawing.Size(21, 22);
            this.txtD6.TabIndex = 27;
            // 
            // btnEnable
            // 
            this.btnEnable.Location = new System.Drawing.Point(63, 189);
            this.btnEnable.Name = "btnEnable";
            this.btnEnable.Size = new System.Drawing.Size(131, 29);
            this.btnEnable.TabIndex = 29;
            this.btnEnable.Text = "Iniciar";
            this.btnEnable.UseVisualStyleBackColor = true;
            // 
            // frmUCE_UI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(665, 594);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.grpCan);
            this.Controls.Add(this.grpLed);
            this.MinimumSize = new System.Drawing.Size(683, 264);
            this.Name = "frmUCE_UI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "UCE - Unidade de Comunicacao Externa";
            this.grpLed.ResumeLayout(false);
            this.grpLed.PerformLayout();
            this.grpCan.ResumeLayout(false);
            this.grpCan.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
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
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button btnEnable;
        private System.Windows.Forms.TextBox txtD7;
        private System.Windows.Forms.TextBox txtD6;
        private System.Windows.Forms.TextBox txtD5;
        private System.Windows.Forms.TextBox txtD4;
        private System.Windows.Forms.TextBox txtD3;
        private System.Windows.Forms.TextBox txtD2;
        private System.Windows.Forms.TextBox txtD1;
        private System.Windows.Forms.TextBox txtD0;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox3;
    }
}

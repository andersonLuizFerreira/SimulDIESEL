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
            this.tabUCE = new System.Windows.Forms.TabControl();
            this.tabConfig = new System.Windows.Forms.TabPage();
            this.tabDados = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnEnable = new System.Windows.Forms.Button();
            this.txtD7 = new System.Windows.Forms.TextBox();
            this.txtD6 = new System.Windows.Forms.TextBox();
            this.txtD5 = new System.Windows.Forms.TextBox();
            this.txtD4 = new System.Windows.Forms.TextBox();
            this.txtD3 = new System.Windows.Forms.TextBox();
            this.txtD2 = new System.Windows.Forms.TextBox();
            this.txtD1 = new System.Windows.Forms.TextBox();
            this.txtD0 = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtLEN = new System.Windows.Forms.TextBox();
            this.txtTime = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cboCANTYPE = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtID = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lblCanDiagLastError = new System.Windows.Forms.Label();
            this.lblCanDiagCan = new System.Windows.Forms.Label();
            this.lblCanDiagTable = new System.Windows.Forms.Label();
            this.lblCanDiagDispatcher = new System.Windows.Forms.Label();
            this.lblCanDiagSync = new System.Windows.Forms.Label();
            this.lblCanDiagMirror = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dgCanRx = new System.Windows.Forms.DataGridView();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lstMensagens = new System.Windows.Forms.ListBox();
            this.grpLed.SuspendLayout();
            this.grpCan.SuspendLayout();
            this.tabUCE.SuspendLayout();
            this.tabConfig.SuspendLayout();
            this.tabDados.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgCanRx)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            //
            // grpLed
            //
            this.grpLed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.grpLed.Controls.Add(this.chkLed);
            this.grpLed.Location = new System.Drawing.Point(13, 31);
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
            this.grpCan.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpCan.Controls.Add(this.lblCanStatus);
            this.grpCan.Controls.Add(this.cmbCanMode);
            this.grpCan.Controls.Add(this.lblCanMode);
            this.grpCan.Controls.Add(this.cmbCanSpeed);
            this.grpCan.Controls.Add(this.lblCanSpeed);
            this.grpCan.Controls.Add(this.chkCanEnabled);
            this.grpCan.Location = new System.Drawing.Point(202, 31);
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
            "5 kbps",
            "10 kbps",
            "25 kbps",
            "50 kbps",
            "125 kbps",
            "250 kbps",
            "500 kbps",
            "800 kbps",
            "1000 kbps"});
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
            // tabUCE
            //
            this.tabUCE.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabUCE.Controls.Add(this.tabConfig);
            this.tabUCE.Controls.Add(this.tabDados);
            this.tabUCE.Location = new System.Drawing.Point(12, 12);
            this.tabUCE.Name = "tabUCE";
            this.tabUCE.SelectedIndex = 0;
            this.tabUCE.Size = new System.Drawing.Size(838, 570);
            this.tabUCE.TabIndex = 3;
            //
            // tabConfig
            //
            this.tabConfig.AutoScroll = true;
            this.tabConfig.Controls.Add(this.groupBox3);
            this.tabConfig.Controls.Add(this.grpLed);
            this.tabConfig.Controls.Add(this.grpCan);
            this.tabConfig.Location = new System.Drawing.Point(4, 25);
            this.tabConfig.Name = "tabConfig";
            this.tabConfig.Padding = new System.Windows.Forms.Padding(3);
            this.tabConfig.Size = new System.Drawing.Size(830, 541);
            this.tabConfig.TabIndex = 0;
            this.tabConfig.Text = "Configurações";
            this.tabConfig.UseVisualStyleBackColor = true;
            //
            // tabDados
            //
            this.tabDados.AutoScroll = true;
            this.tabDados.Controls.Add(this.groupBox1);
            this.tabDados.Controls.Add(this.groupBox4);
            this.tabDados.Controls.Add(this.groupBox2);
            this.tabDados.Location = new System.Drawing.Point(4, 25);
            this.tabDados.Name = "tabDados";
            this.tabDados.Padding = new System.Windows.Forms.Padding(3);
            this.tabDados.Size = new System.Drawing.Size(830, 541);
            this.tabDados.TabIndex = 1;
            this.tabDados.Text = "Fluxo Dados";
            this.tabDados.UseVisualStyleBackColor = true;
            //
            // groupBox2
            //
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
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
            this.groupBox2.Controls.Add(this.txtLEN);
            this.groupBox2.Controls.Add(this.txtTime);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.cboCANTYPE);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.txtID);
            this.groupBox2.Location = new System.Drawing.Point(6, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(818, 136);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Enviar";
            //
            // btnEnable
            //
            this.btnEnable.Location = new System.Drawing.Point(510, 31);
            this.btnEnable.Name = "btnEnable";
            this.btnEnable.Size = new System.Drawing.Size(131, 29);
            this.btnEnable.TabIndex = 29;
            this.btnEnable.Text = "Iniciar";
            this.btnEnable.UseVisualStyleBackColor = true;
            //
            // txtD7
            //
            this.txtD7.Location = new System.Drawing.Point(580, 92);
            this.txtD7.Name = "txtD7";
            this.txtD7.Size = new System.Drawing.Size(27, 22);
            this.txtD7.TabIndex = 28;
            //
            // txtD6
            //
            this.txtD6.Location = new System.Drawing.Point(523, 92);
            this.txtD6.Name = "txtD6";
            this.txtD6.Size = new System.Drawing.Size(27, 22);
            this.txtD6.TabIndex = 27;
            //
            // txtD5
            //
            this.txtD5.Location = new System.Drawing.Point(466, 92);
            this.txtD5.Name = "txtD5";
            this.txtD5.Size = new System.Drawing.Size(27, 22);
            this.txtD5.TabIndex = 26;
            //
            // txtD4
            //
            this.txtD4.Location = new System.Drawing.Point(409, 92);
            this.txtD4.Name = "txtD4";
            this.txtD4.Size = new System.Drawing.Size(27, 22);
            this.txtD4.TabIndex = 25;
            //
            // txtD3
            //
            this.txtD3.Location = new System.Drawing.Point(352, 92);
            this.txtD3.Name = "txtD3";
            this.txtD3.Size = new System.Drawing.Size(27, 22);
            this.txtD3.TabIndex = 24;
            //
            // txtD2
            //
            this.txtD2.Location = new System.Drawing.Point(295, 92);
            this.txtD2.Name = "txtD2";
            this.txtD2.Size = new System.Drawing.Size(27, 22);
            this.txtD2.TabIndex = 23;
            //
            // txtD1
            //
            this.txtD1.Location = new System.Drawing.Point(238, 92);
            this.txtD1.Name = "txtD1";
            this.txtD1.Size = new System.Drawing.Size(27, 22);
            this.txtD1.TabIndex = 22;
            //
            // txtD0
            //
            this.txtD0.Location = new System.Drawing.Point(181, 92);
            this.txtD0.Name = "txtD0";
            this.txtD0.Size = new System.Drawing.Size(27, 22);
            this.txtD0.TabIndex = 21;
            //
            // label9
            //
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(577, 73);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(24, 16);
            this.label9.TabIndex = 20;
            this.label9.Text = "D7";
            //
            // label10
            //
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(520, 73);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(24, 16);
            this.label10.TabIndex = 19;
            this.label10.Text = "D6";
            //
            // label11
            //
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(463, 73);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(24, 16);
            this.label11.TabIndex = 18;
            this.label11.Text = "D5";
            //
            // label12
            //
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(406, 73);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(24, 16);
            this.label12.TabIndex = 17;
            this.label12.Text = "D4";
            //
            // label7
            //
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(349, 73);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(24, 16);
            this.label7.TabIndex = 16;
            this.label7.Text = "D3";
            //
            // label8
            //
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(292, 73);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(24, 16);
            this.label8.TabIndex = 15;
            this.label8.Text = "D2";
            //
            // label6
            //
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(235, 73);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(24, 16);
            this.label6.TabIndex = 14;
            this.label6.Text = "D1";
            //
            // label5
            //
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(178, 73);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(24, 16);
            this.label5.TabIndex = 13;
            this.label5.Text = "D0";
            //
            // label4
            //
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(119, 73);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(33, 16);
            this.label4.TabIndex = 12;
            this.label4.Text = "LEN";
            //
            // txtLEN
            //
            this.txtLEN.Location = new System.Drawing.Point(122, 92);
            this.txtLEN.Name = "txtLEN";
            this.txtLEN.Size = new System.Drawing.Size(34, 22);
            this.txtLEN.TabIndex = 11;
            //
            // txtTime
            //
            this.txtTime.Location = new System.Drawing.Point(386, 34);
            this.txtTime.Name = "txtTime";
            this.txtTime.Size = new System.Drawing.Size(94, 22);
            this.txtTime.TabIndex = 10;
            this.txtTime.Text = "0";
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
            // label2
            //
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(125, 16);
            this.label2.TabIndex = 8;
            this.label2.Text = "Tipo de Mensagem";
            //
            // cboCANTYPE
            //
            this.cboCANTYPE.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCANTYPE.FormattingEnabled = true;
            this.cboCANTYPE.Items.AddRange(new object[] {
            "CAN 1.0 / STD",
            "CAN 2.0 / EXT"});
            this.cboCANTYPE.Location = new System.Drawing.Point(145, 31);
            this.cboCANTYPE.Name = "cboCANTYPE";
            this.cboCANTYPE.Size = new System.Drawing.Size(96, 24);
            this.cboCANTYPE.TabIndex = 7;
            this.cboCANTYPE.SelectedIndex = 1;
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 73);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 16);
            this.label1.TabIndex = 6;
            this.label1.Text = "ID";
            //
            // txtID
            //
            this.txtID.Location = new System.Drawing.Point(17, 92);
            this.txtID.Name = "txtID";
            this.txtID.Size = new System.Drawing.Size(96, 22);
            this.txtID.TabIndex = 5;
            //
            // groupBox4
            //
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.lblCanDiagLastError);
            this.groupBox4.Controls.Add(this.lblCanDiagCan);
            this.groupBox4.Controls.Add(this.lblCanDiagTable);
            this.groupBox4.Controls.Add(this.lblCanDiagDispatcher);
            this.groupBox4.Controls.Add(this.lblCanDiagSync);
            this.groupBox4.Controls.Add(this.lblCanDiagMirror);
            this.groupBox4.Location = new System.Drawing.Point(6, 148);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(818, 80);
            this.groupBox4.TabIndex = 8;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Diagnóstico CAN RX";
            //
            // lblCanDiagLastError
            //
            this.lblCanDiagLastError.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCanDiagLastError.Location = new System.Drawing.Point(276, 51);
            this.lblCanDiagLastError.Name = "lblCanDiagLastError";
            this.lblCanDiagLastError.Size = new System.Drawing.Size(523, 19);
            this.lblCanDiagLastError.TabIndex = 5;
            this.lblCanDiagLastError.Text = "Último erro: -";
            //
            // lblCanDiagCan
            //
            this.lblCanDiagCan.AutoSize = true;
            this.lblCanDiagCan.Location = new System.Drawing.Point(144, 51);
            this.lblCanDiagCan.Name = "lblCanDiagCan";
            this.lblCanDiagCan.Size = new System.Drawing.Size(92, 16);
            this.lblCanDiagCan.TabIndex = 4;
            this.lblCanDiagCan.Text = "CAN: fechada";
            //
            // lblCanDiagTable
            //
            this.lblCanDiagTable.AutoSize = true;
            this.lblCanDiagTable.Location = new System.Drawing.Point(17, 51);
            this.lblCanDiagTable.Name = "lblCanDiagTable";
            this.lblCanDiagTable.Size = new System.Drawing.Size(99, 16);
            this.lblCanDiagTable.TabIndex = 3;
            this.lblCanDiagTable.Text = "Tabela: OK";
            //
            // lblCanDiagDispatcher
            //
            this.lblCanDiagDispatcher.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCanDiagDispatcher.Location = new System.Drawing.Point(276, 24);
            this.lblCanDiagDispatcher.Name = "lblCanDiagDispatcher";
            this.lblCanDiagDispatcher.Size = new System.Drawing.Size(523, 19);
            this.lblCanDiagDispatcher.TabIndex = 2;
            this.lblCanDiagDispatcher.Text = "Dispatcher FIFO: OK";
            //
            // lblCanDiagSync
            //
            this.lblCanDiagSync.AutoSize = true;
            this.lblCanDiagSync.Location = new System.Drawing.Point(144, 24);
            this.lblCanDiagSync.Name = "lblCanDiagSync";
            this.lblCanDiagSync.Size = new System.Drawing.Size(82, 16);
            this.lblCanDiagSync.TabIndex = 1;
            this.lblCanDiagSync.Text = "Sync: Estável";
            //
            // lblCanDiagMirror
            //
            this.lblCanDiagMirror.AutoSize = true;
            this.lblCanDiagMirror.Location = new System.Drawing.Point(17, 24);
            this.lblCanDiagMirror.Name = "lblCanDiagMirror";
            this.lblCanDiagMirror.Size = new System.Drawing.Size(65, 16);
            this.lblCanDiagMirror.TabIndex = 0;
            this.lblCanDiagMirror.Text = "Mirror: OK";
            //
            // groupBox1
            //
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.dgCanRx);
            this.groupBox1.Location = new System.Drawing.Point(6, 234);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(818, 301);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Recepção";
            //
            // dgCanRx
            //
            this.dgCanRx.AllowUserToAddRows = false;
            this.dgCanRx.AllowUserToDeleteRows = false;
            this.dgCanRx.AllowUserToResizeRows = false;
            this.dgCanRx.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgCanRx.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgCanRx.MultiSelect = false;
            this.dgCanRx.Name = "dgCanRx";
            this.dgCanRx.ReadOnly = true;
            this.dgCanRx.RowHeadersVisible = false;
            this.dgCanRx.RowTemplate.Height = 24;
            this.dgCanRx.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgCanRx.TabIndex = 0;
            //
            // groupBox3
            //
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.lstMensagens);
            this.groupBox3.Location = new System.Drawing.Point(15, 224);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(809, 184);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Mensagens do Driver";
            //
            // lstMensagens
            //
            this.lstMensagens.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstMensagens.FormattingEnabled = true;
            this.lstMensagens.ItemHeight = 16;
            this.lstMensagens.Location = new System.Drawing.Point(9, 23);
            this.lstMensagens.Name = "lstMensagens";
            this.lstMensagens.Size = new System.Drawing.Size(794, 132);
            this.lstMensagens.TabIndex = 0;
            //
            // frmUCE_UI
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(862, 594);
            this.Controls.Add(this.tabUCE);
            this.MinimumSize = new System.Drawing.Size(683, 264);
            this.Name = "frmUCE_UI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "UCE - Unidade de Comunicacao Externa";
            this.grpLed.ResumeLayout(false);
            this.grpLed.PerformLayout();
            this.grpCan.ResumeLayout(false);
            this.grpCan.PerformLayout();
            this.tabUCE.ResumeLayout(false);
            this.tabConfig.ResumeLayout(false);
            this.tabDados.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgCanRx)).EndInit();
            this.groupBox3.ResumeLayout(false);
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
        private System.Windows.Forms.TabControl tabUCE;
        private System.Windows.Forms.TabPage tabConfig;
        private System.Windows.Forms.TabPage tabDados;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView dgCanRx;
        private System.Windows.Forms.GroupBox groupBox2;
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
        private System.Windows.Forms.TextBox txtLEN;
        private System.Windows.Forms.TextBox txtTime;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboCANTYPE;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtID;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label lblCanDiagLastError;
        private System.Windows.Forms.Label lblCanDiagCan;
        private System.Windows.Forms.Label lblCanDiagTable;
        private System.Windows.Forms.Label lblCanDiagDispatcher;
        private System.Windows.Forms.Label lblCanDiagSync;
        private System.Windows.Forms.Label lblCanDiagMirror;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ListBox lstMensagens;
    }
}

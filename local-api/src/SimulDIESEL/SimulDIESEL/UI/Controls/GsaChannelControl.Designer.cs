namespace SimulDIESEL.UI.Controls
{
    partial class GsaChannelControl
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this._headerPanel = new System.Windows.Forms.Panel();
            this._faultPanel = new System.Windows.Forms.Panel();
            this._faultLabel = new System.Windows.Forms.Label();
            this._faultLed = new SimulDIESEL.UI.Controls.SdLedIndicator();
            this._titleLabel = new System.Windows.Forms.Label();
            this._configButton = new System.Windows.Forms.Button();
            this._bodyLayout = new System.Windows.Forms.TableLayoutPanel();
            this._setpointPanel = new System.Windows.Forms.Panel();
            this._rangeLabel = new System.Windows.Forms.Label();
            this._setpointSlider = new SimulDIESEL.UI.Controls.SdVerticalSlider();
            this._setpointHeaderLabel = new System.Windows.Forms.Label();
            this._voltagePanel = new System.Windows.Forms.Panel();
            this._voltageValueLabel = new System.Windows.Forms.Label();
            this._voltageGauge = new SimulDIESEL.UI.Controls.SdVerticalGauge();
            this._voltageHeaderLabel = new System.Windows.Forms.Label();
            this._currentPanel = new System.Windows.Forms.Panel();
            this._currentValueLabel = new System.Windows.Forms.Label();
            this._currentGauge = new SimulDIESEL.UI.Controls.SdVerticalGauge();
            this._currentHeaderLabel = new System.Windows.Forms.Label();
            this._footerPanel = new System.Windows.Forms.Panel();
            this._footerLayout = new System.Windows.Forms.TableLayoutPanel();
            this._setpointLabel = new System.Windows.Forms.Label();
            this._setpointTextBox = new System.Windows.Forms.TextBox();
            this._setpointUnitLabel = new System.Windows.Forms.Label();
            this._enabledCheckBox = new System.Windows.Forms.CheckBox();
            this._headerPanel.SuspendLayout();
            this._faultPanel.SuspendLayout();
            this._bodyLayout.SuspendLayout();
            this._setpointPanel.SuspendLayout();
            this._voltagePanel.SuspendLayout();
            this._currentPanel.SuspendLayout();
            this._footerPanel.SuspendLayout();
            this._footerLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // _headerPanel
            // 
            this._headerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(43)))), ((int)(((byte)(48)))));
            this._headerPanel.Controls.Add(this._faultPanel);
            this._headerPanel.Controls.Add(this._titleLabel);
            this._headerPanel.Controls.Add(this._configButton);
            this._headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._headerPanel.Location = new System.Drawing.Point(10, 10);
            this._headerPanel.Margin = new System.Windows.Forms.Padding(0);
            this._headerPanel.Name = "_headerPanel";
            this._headerPanel.Size = new System.Drawing.Size(236, 64);
            this._headerPanel.TabIndex = 0;
            // 
            // _faultPanel
            // 
            this._faultPanel.BackColor = System.Drawing.Color.Transparent;
            this._faultPanel.Controls.Add(this._faultLabel);
            this._faultPanel.Controls.Add(this._faultLed);
            this._faultPanel.Location = new System.Drawing.Point(8, 33);
            this._faultPanel.Margin = new System.Windows.Forms.Padding(0);
            this._faultPanel.Name = "_faultPanel";
            this._faultPanel.Size = new System.Drawing.Size(110, 22);
            this._faultPanel.TabIndex = 1;
            // 
            // _faultLabel
            // 
            this._faultLabel.AutoSize = true;
            this._faultLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(228)))), ((int)(((byte)(231)))));
            this._faultLabel.Location = new System.Drawing.Point(25, 3);
            this._faultLabel.Margin = new System.Windows.Forms.Padding(0);
            this._faultLabel.Name = "_faultLabel";
            this._faultLabel.Size = new System.Drawing.Size(36, 15);
            this._faultLabel.TabIndex = 1;
            this._faultLabel.Text = "Falha";
            // 
            // _faultLed
            // 
            this._faultLed.IsOn = false;
            this._faultLed.Location = new System.Drawing.Point(2, 2);
            this._faultLed.Margin = new System.Windows.Forms.Padding(0);
            this._faultLed.Name = "_faultLed";
            this._faultLed.Size = new System.Drawing.Size(18, 18);
            this._faultLed.TabIndex = 0;
            // 
            // _titleLabel
            // 
            this._titleLabel.AutoEllipsis = true;
            this._titleLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(240)))), ((int)(((byte)(243)))));
            this._titleLabel.Location = new System.Drawing.Point(8, 8);
            this._titleLabel.Margin = new System.Windows.Forms.Padding(0);
            this._titleLabel.Name = "_titleLabel";
            this._titleLabel.Size = new System.Drawing.Size(164, 22);
            this._titleLabel.TabIndex = 0;
            this._titleLabel.Text = "Canal 1";
            this._titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _configButton
            // 
            this._configButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._configButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(118)))), ((int)(((byte)(126)))));
            this._configButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(58)))), ((int)(((byte)(64)))));
            this._configButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(69)))), ((int)(((byte)(76)))));
            this._configButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._configButton.Location = new System.Drawing.Point(196, 12);
            this._configButton.Margin = new System.Windows.Forms.Padding(0);
            this._configButton.Name = "_configButton";
            this._configButton.Size = new System.Drawing.Size(28, 28);
            this._configButton.TabIndex = 2;
            this._configButton.UseVisualStyleBackColor = true;
            // 
            // _bodyLayout
            // 
            this._bodyLayout.BackColor = System.Drawing.Color.Transparent;
            this._bodyLayout.ColumnCount = 3;
            this._bodyLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 31F));
            this._bodyLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34.5F));
            this._bodyLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34.5F));
            this._bodyLayout.Controls.Add(this._setpointPanel, 0, 0);
            this._bodyLayout.Controls.Add(this._voltagePanel, 1, 0);
            this._bodyLayout.Controls.Add(this._currentPanel, 2, 0);
            this._bodyLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._bodyLayout.Location = new System.Drawing.Point(10, 74);
            this._bodyLayout.Margin = new System.Windows.Forms.Padding(0);
            this._bodyLayout.Name = "_bodyLayout";
            this._bodyLayout.Padding = new System.Windows.Forms.Padding(4, 4, 4, 2);
            this._bodyLayout.RowCount = 1;
            this._bodyLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._bodyLayout.Size = new System.Drawing.Size(236, 230);
            this._bodyLayout.TabIndex = 1;
            // 
            // _setpointPanel
            // 
            this._setpointPanel.BackColor = System.Drawing.Color.Transparent;
            this._setpointPanel.Controls.Add(this._rangeLabel);
            this._setpointPanel.Controls.Add(this._setpointSlider);
            this._setpointPanel.Controls.Add(this._setpointHeaderLabel);
            this._setpointPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._setpointPanel.Location = new System.Drawing.Point(4, 4);
            this._setpointPanel.Margin = new System.Windows.Forms.Padding(0);
            this._setpointPanel.Name = "_setpointPanel";
            this._setpointPanel.Padding = new System.Windows.Forms.Padding(2);
            this._setpointPanel.Size = new System.Drawing.Size(70, 224);
            this._setpointPanel.TabIndex = 0;
            // 
            // _rangeLabel
            // 
            this._rangeLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._rangeLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(159)))), ((int)(((byte)(169)))), ((int)(((byte)(177)))));
            this._rangeLabel.Location = new System.Drawing.Point(2, 202);
            this._rangeLabel.Margin = new System.Windows.Forms.Padding(0);
            this._rangeLabel.Name = "_rangeLabel";
            this._rangeLabel.Size = new System.Drawing.Size(66, 20);
            this._rangeLabel.TabIndex = 2;
            this._rangeLabel.Text = "0-5 V";
            this._rangeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _setpointSlider
            // 
            this._setpointSlider.Dock = System.Windows.Forms.DockStyle.Fill;
            this._setpointSlider.Location = new System.Drawing.Point(2, 20);
            this._setpointSlider.Margin = new System.Windows.Forms.Padding(0);
            this._setpointSlider.Maximum = 5D;
            this._setpointSlider.Name = "_setpointSlider";
            this._setpointSlider.Size = new System.Drawing.Size(66, 202);
            this._setpointSlider.TabIndex = 1;
            this._setpointSlider.Value = 0D;
            // 
            // _setpointHeaderLabel
            // 
            this._setpointHeaderLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this._setpointHeaderLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(180)))), ((int)(((byte)(188)))));
            this._setpointHeaderLabel.Location = new System.Drawing.Point(2, 2);
            this._setpointHeaderLabel.Margin = new System.Windows.Forms.Padding(0);
            this._setpointHeaderLabel.Name = "_setpointHeaderLabel";
            this._setpointHeaderLabel.Size = new System.Drawing.Size(66, 18);
            this._setpointHeaderLabel.TabIndex = 0;
            this._setpointHeaderLabel.Text = "AJUSTE";
            this._setpointHeaderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _voltagePanel
            // 
            this._voltagePanel.BackColor = System.Drawing.Color.Transparent;
            this._voltagePanel.Controls.Add(this._voltageValueLabel);
            this._voltagePanel.Controls.Add(this._voltageGauge);
            this._voltagePanel.Controls.Add(this._voltageHeaderLabel);
            this._voltagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._voltagePanel.Location = new System.Drawing.Point(74, 4);
            this._voltagePanel.Margin = new System.Windows.Forms.Padding(0);
            this._voltagePanel.Name = "_voltagePanel";
            this._voltagePanel.Padding = new System.Windows.Forms.Padding(2);
            this._voltagePanel.Size = new System.Drawing.Size(78, 224);
            this._voltagePanel.TabIndex = 1;
            // 
            // _voltageValueLabel
            // 
            this._voltageValueLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._voltageValueLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._voltageValueLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(235)))), ((int)(((byte)(237)))));
            this._voltageValueLabel.Location = new System.Drawing.Point(2, 202);
            this._voltageValueLabel.Margin = new System.Windows.Forms.Padding(0);
            this._voltageValueLabel.Name = "_voltageValueLabel";
            this._voltageValueLabel.Size = new System.Drawing.Size(74, 20);
            this._voltageValueLabel.TabIndex = 2;
            this._voltageValueLabel.Text = "0,00 V";
            this._voltageValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _voltageGauge
            // 
            this._voltageGauge.AutoColorByThreshold = false;
            this._voltageGauge.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(134)))), ((int)(((byte)(140)))));
            this._voltageGauge.BorderThickness = 2;
            this._voltageGauge.ChannelColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(23)))), ((int)(((byte)(26)))));
            this._voltageGauge.DisplayNumberFormat = "0.0";
            this._voltageGauge.DisplayScaleFactor = 100D;
            this._voltageGauge.Dock = System.Windows.Forms.DockStyle.Fill;
            this._voltageGauge.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(195)))), ((int)(((byte)(155)))));
            this._voltageGauge.GaugeBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this._voltageGauge.Location = new System.Drawing.Point(2, 20);
            this._voltageGauge.Margin = new System.Windows.Forms.Padding(0);
            this._voltageGauge.Maximum = 500;
            this._voltageGauge.Name = "_voltageGauge";
            this._voltageGauge.ShowScaleLabels = true;
            this._voltageGauge.ShowThresholdBands = false;
            this._voltageGauge.ShowTicks = true;
            this._voltageGauge.ShowTitle = false;
            this._voltageGauge.ShowUnitText = false;
            this._voltageGauge.ShowValueText = false;
            this._voltageGauge.Size = new System.Drawing.Size(74, 202);
            this._voltageGauge.TabIndex = 1;
            this._voltageGauge.TickColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(219)))), ((int)(((byte)(170)))));
            this._voltageGauge.WarningThreshold = 500;
            // 
            // _voltageHeaderLabel
            // 
            this._voltageHeaderLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this._voltageHeaderLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(180)))), ((int)(((byte)(188)))));
            this._voltageHeaderLabel.Location = new System.Drawing.Point(2, 2);
            this._voltageHeaderLabel.Margin = new System.Windows.Forms.Padding(0);
            this._voltageHeaderLabel.Name = "_voltageHeaderLabel";
            this._voltageHeaderLabel.Size = new System.Drawing.Size(74, 18);
            this._voltageHeaderLabel.TabIndex = 0;
            this._voltageHeaderLabel.Text = "SAÍDA V";
            this._voltageHeaderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _currentPanel
            // 
            this._currentPanel.BackColor = System.Drawing.Color.Transparent;
            this._currentPanel.Controls.Add(this._currentValueLabel);
            this._currentPanel.Controls.Add(this._currentGauge);
            this._currentPanel.Controls.Add(this._currentHeaderLabel);
            this._currentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._currentPanel.Location = new System.Drawing.Point(152, 4);
            this._currentPanel.Margin = new System.Windows.Forms.Padding(0);
            this._currentPanel.Name = "_currentPanel";
            this._currentPanel.Padding = new System.Windows.Forms.Padding(2);
            this._currentPanel.Size = new System.Drawing.Size(80, 224);
            this._currentPanel.TabIndex = 2;
            // 
            // _currentValueLabel
            // 
            this._currentValueLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._currentValueLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._currentValueLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(235)))), ((int)(((byte)(237)))));
            this._currentValueLabel.Location = new System.Drawing.Point(2, 202);
            this._currentValueLabel.Margin = new System.Windows.Forms.Padding(0);
            this._currentValueLabel.Name = "_currentValueLabel";
            this._currentValueLabel.Size = new System.Drawing.Size(76, 20);
            this._currentValueLabel.TabIndex = 2;
            this._currentValueLabel.Text = "0,00 A";
            this._currentValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _currentGauge
            // 
            this._currentGauge.AutoColorByThreshold = true;
            this._currentGauge.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(134)))), ((int)(((byte)(140)))));
            this._currentGauge.BorderThickness = 2;
            this._currentGauge.ChannelColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(23)))), ((int)(((byte)(26)))));
            this._currentGauge.DisplayNumberFormat = "0.0";
            this._currentGauge.DisplayScaleFactor = 100D;
            this._currentGauge.Dock = System.Windows.Forms.DockStyle.Fill;
            this._currentGauge.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(173)))), ((int)(((byte)(140)))));
            this._currentGauge.GaugeBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this._currentGauge.Location = new System.Drawing.Point(2, 20);
            this._currentGauge.Margin = new System.Windows.Forms.Padding(0);
            this._currentGauge.Maximum = 200;
            this._currentGauge.Name = "_currentGauge";
            this._currentGauge.ShowScaleLabels = true;
            this._currentGauge.ShowThresholdBands = true;
            this._currentGauge.ShowTicks = true;
            this._currentGauge.ShowTitle = false;
            this._currentGauge.ShowUnitText = false;
            this._currentGauge.ShowValueText = false;
            this._currentGauge.Size = new System.Drawing.Size(76, 202);
            this._currentGauge.TabIndex = 1;
            this._currentGauge.WarningThreshold = 150;
            // 
            // _currentHeaderLabel
            // 
            this._currentHeaderLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this._currentHeaderLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(180)))), ((int)(((byte)(188)))));
            this._currentHeaderLabel.Location = new System.Drawing.Point(2, 2);
            this._currentHeaderLabel.Margin = new System.Windows.Forms.Padding(0);
            this._currentHeaderLabel.Name = "_currentHeaderLabel";
            this._currentHeaderLabel.Size = new System.Drawing.Size(76, 18);
            this._currentHeaderLabel.TabIndex = 0;
            this._currentHeaderLabel.Text = "SAÍDA I";
            this._currentHeaderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _footerPanel
            // 
            this._footerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._footerPanel.Controls.Add(this._footerLayout);
            this._footerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._footerPanel.Location = new System.Drawing.Point(10, 304);
            this._footerPanel.Margin = new System.Windows.Forms.Padding(0);
            this._footerPanel.Name = "_footerPanel";
            this._footerPanel.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this._footerPanel.Size = new System.Drawing.Size(236, 66);
            this._footerPanel.TabIndex = 2;
            // 
            // _footerLayout
            // 
            this._footerLayout.ColumnCount = 3;
            this._footerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._footerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._footerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._footerLayout.Controls.Add(this._setpointLabel, 0, 0);
            this._footerLayout.Controls.Add(this._setpointTextBox, 1, 0);
            this._footerLayout.Controls.Add(this._setpointUnitLabel, 2, 0);
            this._footerLayout.Controls.Add(this._enabledCheckBox, 0, 1);
            this._footerLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._footerLayout.Location = new System.Drawing.Point(8, 6);
            this._footerLayout.Margin = new System.Windows.Forms.Padding(0);
            this._footerLayout.Name = "_footerLayout";
            this._footerLayout.RowCount = 2;
            this._footerLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this._footerLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._footerLayout.Size = new System.Drawing.Size(220, 54);
            this._footerLayout.TabIndex = 0;
            // 
            // _setpointLabel
            // 
            this._setpointLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._setpointLabel.AutoSize = true;
            this._setpointLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(217)))), ((int)(((byte)(221)))));
            this._setpointLabel.Location = new System.Drawing.Point(0, 6);
            this._setpointLabel.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this._setpointLabel.Name = "_setpointLabel";
            this._setpointLabel.Size = new System.Drawing.Size(51, 15);
            this._setpointLabel.TabIndex = 0;
            this._setpointLabel.Text = "Setpoint";
            // 
            // _setpointTextBox
            // 
            this._setpointTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._setpointTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(26)))), ((int)(((byte)(29)))));
            this._setpointTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._setpointTextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(240)))), ((int)(((byte)(242)))));
            this._setpointTextBox.Location = new System.Drawing.Point(57, 3);
            this._setpointTextBox.Margin = new System.Windows.Forms.Padding(0);
            this._setpointTextBox.Name = "_setpointTextBox";
            this._setpointTextBox.Size = new System.Drawing.Size(126, 23);
            this._setpointTextBox.TabIndex = 1;
            this._setpointTextBox.Text = "0,00";
            this._setpointTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // _setpointUnitLabel
            // 
            this._setpointUnitLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._setpointUnitLabel.AutoSize = true;
            this._setpointUnitLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(217)))), ((int)(((byte)(221)))));
            this._setpointUnitLabel.Location = new System.Drawing.Point(189, 6);
            this._setpointUnitLabel.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this._setpointUnitLabel.Name = "_setpointUnitLabel";
            this._setpointUnitLabel.Size = new System.Drawing.Size(14, 15);
            this._setpointUnitLabel.TabIndex = 2;
            this._setpointUnitLabel.Text = "V";
            // 
            // _enabledCheckBox
            // 
            this._enabledCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._enabledCheckBox.AutoSize = true;
            this._footerLayout.SetColumnSpan(this._enabledCheckBox, 3);
            this._enabledCheckBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(228)))), ((int)(((byte)(231)))));
            this._enabledCheckBox.Location = new System.Drawing.Point(0, 31);
            this._enabledCheckBox.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this._enabledCheckBox.Name = "_enabledCheckBox";
            this._enabledCheckBox.Size = new System.Drawing.Size(84, 19);
            this._enabledCheckBox.TabIndex = 3;
            this._enabledCheckBox.Text = "Habilitado";
            this._enabledCheckBox.UseVisualStyleBackColor = true;
            // 
            // GsaChannelControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(31)))), ((int)(((byte)(35)))));
            this.Controls.Add(this._bodyLayout);
            this.Controls.Add(this._footerPanel);
            this.Controls.Add(this._headerPanel);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Gainsboro;
            this.Margin = new System.Windows.Forms.Padding(12);
            this.MinimumSize = new System.Drawing.Size(220, 320);
            this.Name = "GsaChannelControl";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Size = new System.Drawing.Size(256, 380);
            this._headerPanel.ResumeLayout(false);
            this._faultPanel.ResumeLayout(false);
            this._faultPanel.PerformLayout();
            this._bodyLayout.ResumeLayout(false);
            this._setpointPanel.ResumeLayout(false);
            this._voltagePanel.ResumeLayout(false);
            this._currentPanel.ResumeLayout(false);
            this._footerPanel.ResumeLayout(false);
            this._footerLayout.ResumeLayout(false);
            this._footerLayout.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _headerPanel;
        private System.Windows.Forms.Panel _faultPanel;
        private System.Windows.Forms.Label _faultLabel;
        private SdLedIndicator _faultLed;
        private System.Windows.Forms.Label _titleLabel;
        private System.Windows.Forms.Button _configButton;
        private System.Windows.Forms.TableLayoutPanel _bodyLayout;
        private System.Windows.Forms.Panel _setpointPanel;
        private System.Windows.Forms.Label _rangeLabel;
        private SdVerticalSlider _setpointSlider;
        private System.Windows.Forms.Label _setpointHeaderLabel;
        private System.Windows.Forms.Panel _voltagePanel;
        private System.Windows.Forms.Label _voltageValueLabel;
        private SdVerticalGauge _voltageGauge;
        private System.Windows.Forms.Label _voltageHeaderLabel;
        private System.Windows.Forms.Panel _currentPanel;
        private System.Windows.Forms.Label _currentValueLabel;
        private SdVerticalGauge _currentGauge;
        private System.Windows.Forms.Label _currentHeaderLabel;
        private System.Windows.Forms.Panel _footerPanel;
        private System.Windows.Forms.TableLayoutPanel _footerLayout;
        private System.Windows.Forms.Label _setpointLabel;
        private System.Windows.Forms.TextBox _setpointTextBox;
        private System.Windows.Forms.Label _setpointUnitLabel;
        private System.Windows.Forms.CheckBox _enabledCheckBox;
    }
}

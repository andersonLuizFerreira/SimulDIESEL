using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace SimulDIESEL.UI.Controls
{
    [DefaultProperty("ChannelTitle")]
    [DefaultEvent("SetpointVoltageChanged")]
    [ToolboxItem(true)]
    public partial class GsaChannelControl : UserControl
    {
        private const double GaugeScaleFactor = 100d;

        private double _voltageRangeMax = 5d;
        private double _setpointVoltage;
        private double _measuredVoltage;
        private double _measuredCurrent;
        private double _currentMax = 2d;
        private double _currentWarningThreshold = 1.4d;
        private double _currentDangerThreshold = 1.75d;
        private string _currentUnitText = "A";
        private bool _industrialThemeEnabled = true;
        private bool _compactMode;
        private bool _denseLayoutActive;
        private int _layoutProfileKey = int.MinValue;
        private bool _suppressSetpointSync;
        private bool _suppressOutputEvent;
        private TableLayoutPanel _headerLayout;
        private TableLayoutPanel _headerInfoLayout;
        private TableLayoutPanel _setpointLayout;
        private TableLayoutPanel _voltageLayout;
        private TableLayoutPanel _currentLayout;

        public GsaChannelControl()
        {
            InitializeComponent();

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            DoubleBuffered = true;

            BuildResponsiveLayout();
            WireEvents();
            ApplyCompactLayout();
            ApplyTheme();
            RefreshAllVisuals();
        }

        [Category("Canal")]
        [DefaultValue("Canal 1")]
        public string ChannelTitle
        {
            get { return _titleLabel.Text; }
            set
            {
                string sanitizedValue = string.IsNullOrWhiteSpace(value) ? "Canal 1" : value.Trim();
                if (string.Equals(_titleLabel.Text, sanitizedValue, StringComparison.Ordinal))
                    return;

                _titleLabel.Text = sanitizedValue;
                Invalidate();
            }
        }

        [Category("Ajuste")]
        [DefaultValue(5d)]
        public double VoltageRangeMax
        {
            get { return _voltageRangeMax; }
            set
            {
                double sanitizedValue = Math.Max(0.1d, value);
                if (AreClose(_voltageRangeMax, sanitizedValue))
                    return;

                _voltageRangeMax = sanitizedValue;
                _setpointSlider.Maximum = sanitizedValue;
                ApplySetpointVoltage(_setpointVoltage, false, true, true);
                UpdateVoltageGaugeScale();
                UpdateVoltageGaugeValue();
                UpdateRangeLabel();
            }
        }

        [Category("Ajuste")]
        [DefaultValue(0d)]
        public double SetpointVoltage
        {
            get { return _setpointVoltage; }
            set { ApplySetpointVoltage(value, false, true, true); }
        }

        [Category("Medicao")]
        [DefaultValue(0d)]
        public double MeasuredVoltage
        {
            get { return _measuredVoltage; }
            set
            {
                if (AreClose(_measuredVoltage, value))
                    return;

                _measuredVoltage = value;
                UpdateVoltageGaugeValue();
            }
        }

        [Category("Medicao")]
        [DefaultValue(0d)]
        public double MeasuredCurrent
        {
            get { return _measuredCurrent; }
            set
            {
                if (AreClose(_measuredCurrent, value))
                    return;

                _measuredCurrent = value;
                UpdateCurrentGaugeValue();
            }
        }

        [Category("Corrente")]
        [DefaultValue(2d)]
        public double CurrentMax
        {
            get { return _currentMax; }
            set
            {
                double sanitizedValue = Math.Max(0.1d, value);
                if (AreClose(_currentMax, sanitizedValue))
                    return;

                _currentMax = sanitizedValue;
                if (_currentWarningThreshold > _currentMax)
                    _currentWarningThreshold = _currentMax;
                if (_currentDangerThreshold > _currentMax)
                    _currentDangerThreshold = _currentMax;

                UpdateCurrentGaugeScale();
                UpdateCurrentGaugeValue();
            }
        }

        [Category("Corrente")]
        [DefaultValue(1.4d)]
        public double CurrentWarningThreshold
        {
            get { return _currentWarningThreshold; }
            set
            {
                double sanitizedValue = Clamp(value, 0d, CurrentMax);
                if (AreClose(_currentWarningThreshold, sanitizedValue))
                    return;

                _currentWarningThreshold = sanitizedValue;
                UpdateCurrentGaugeScale();
            }
        }

        [Category("Corrente")]
        [DefaultValue(1.75d)]
        public double CurrentDangerThreshold
        {
            get { return _currentDangerThreshold; }
            set
            {
                double sanitizedValue = Clamp(value, 0d, CurrentMax);
                if (AreClose(_currentDangerThreshold, sanitizedValue))
                    return;

                _currentDangerThreshold = sanitizedValue;
                UpdateCurrentGaugeScale();
            }
        }

        [Category("Corrente")]
        [DefaultValue("A")]
        public string CurrentUnitText
        {
            get { return _currentUnitText; }
            set
            {
                string sanitizedValue = string.IsNullOrWhiteSpace(value) ? "A" : value.Trim();
                if (string.Equals(_currentUnitText, sanitizedValue, StringComparison.Ordinal))
                    return;

                _currentUnitText = sanitizedValue;
                UpdateCurrentGaugeValue();
            }
        }

        [Category("Estado")]
        [DefaultValue(false)]
        public bool OutputEnabled
        {
            get { return _enabledCheckBox.Checked; }
            set
            {
                if (_enabledCheckBox.Checked == value)
                    return;

                _suppressOutputEvent = true;
                try
                {
                    _enabledCheckBox.Checked = value;
                }
                finally
                {
                    _suppressOutputEvent = false;
                }
            }
        }

        [Category("Estado")]
        [DefaultValue(false)]
        public bool FaultActive
        {
            get { return _faultLed.IsOn; }
            set
            {
                if (_faultLed.IsOn == value)
                    return;

                _faultLed.IsOn = value;
                Invalidate();
            }
        }

        [Category("Aparencia")]
        [DefaultValue(true)]
        public bool IndustrialThemeEnabled
        {
            get { return _industrialThemeEnabled; }
            set
            {
                if (_industrialThemeEnabled == value)
                    return;

                _industrialThemeEnabled = value;
                ApplyTheme();
                Invalidate();
            }
        }

        [Category("Aparencia")]
        [DefaultValue(false)]
        public bool CompactMode
        {
            get { return _compactMode; }
            set
            {
                if (_compactMode == value)
                    return;

                _compactMode = value;
                ApplyCompactLayout();
                Invalidate();
            }
        }

        [Category("Action")]
        public event EventHandler SetpointVoltageChanged;

        [Category("Action")]
        public event EventHandler OutputEnabledChanged;

        [Category("Action")]
        public event EventHandler ConfigButtonClick;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle shadowBounds = Rectangle.Inflate(ClientRectangle, -3, -3);
            shadowBounds.Offset(0, 2);
            Rectangle frameBounds = Rectangle.Inflate(ClientRectangle, -1, -1);

            using (GraphicsPath shadowPath = CreateRoundedPath(shadowBounds, 16))
            using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
            using (GraphicsPath framePath = CreateRoundedPath(frameBounds, 16))
            using (LinearGradientBrush bodyBrush = new LinearGradientBrush(
                frameBounds,
                Color.FromArgb(43, 47, 53),
                Color.FromArgb(24, 27, 31),
                LinearGradientMode.Vertical))
            using (Pen borderPen = new Pen(Color.FromArgb(118, 125, 132), 1.3f))
            using (Pen innerPen = new Pen(Color.FromArgb(62, Color.White), 1f))
            {
                e.Graphics.FillPath(shadowBrush, shadowPath);
                e.Graphics.FillPath(bodyBrush, framePath);
                e.Graphics.DrawPath(borderPen, framePath);

                Rectangle innerBounds = Rectangle.Inflate(frameBounds, -1, -1);
                if (innerBounds.Width > 8 && innerBounds.Height > 8)
                {
                    using (GraphicsPath innerPath = CreateRoundedPath(innerBounds, 15))
                    {
                        e.Graphics.DrawPath(innerPen, innerPath);
                    }
                }
            }

            using (Pen separatorPen = new Pen(Color.FromArgb(52, Color.White), 1f))
            {
                int headerLineY = _headerPanel.Bottom;
                int footerLineY = _footerPanel.Top;
                e.Graphics.DrawLine(separatorPen, Padding.Left, headerLineY, Width - Padding.Right, headerLineY);
                e.Graphics.DrawLine(separatorPen, Padding.Left, footerLineY, Width - Padding.Right, footerLineY);
            }
        }

        private void WireEvents()
        {
            _configButton.Paint += ConfigButton_Paint;
            _configButton.Click += ConfigButton_Click;
            _setpointSlider.ValueChanged += SetpointSlider_ValueChanged;
            _setpointTextBox.TextChanged += SetpointTextBox_TextChanged;
            _setpointTextBox.Leave += SetpointTextBox_Leave;
            _setpointTextBox.KeyDown += SetpointTextBox_KeyDown;
            _enabledCheckBox.CheckedChanged += EnabledCheckBox_CheckedChanged;
            Resize += GsaChannelControl_Resize;
        }

        private void ApplyCompactLayout()
        {
            bool denseLayout = Width <= 138 || Height <= 220;
            bool ultraDenseLayout = Width <= 124 || Height <= 196;
            int layoutProfileKey = (CompactMode ? 1 : 0) | (denseLayout ? 2 : 0) | (ultraDenseLayout ? 4 : 0);
            if (_layoutProfileKey == layoutProfileKey)
                return;

            _layoutProfileKey = layoutProfileKey;
            _denseLayoutActive = denseLayout;

            int headerHeight = ultraDenseLayout ? 42 : denseLayout ? 48 : CompactMode ? 56 : 64;
            int footerHeight = ultraDenseLayout ? 42 : denseLayout ? 50 : CompactMode ? 58 : 66;

            SuspendLayout();
            try
            {
                Padding = ultraDenseLayout ? new Padding(5) : denseLayout ? new Padding(7) : new Padding(10);

                _headerPanel.Height = headerHeight;
                _footerPanel.Height = footerHeight;
                _headerLayout.Padding = ultraDenseLayout ? new Padding(5, 4, 5, 4) : denseLayout ? new Padding(6, 4, 6, 4) : new Padding(8, 6, 8, 6);
                _headerInfoLayout.RowStyles[0].Height = ultraDenseLayout ? 16f : denseLayout ? 19f : 24f;
                _headerInfoLayout.RowStyles[1].Height = ultraDenseLayout ? 14f : denseLayout ? 16f : 22f;

                _configButton.Size = ultraDenseLayout ? new Size(18, 18) : denseLayout ? new Size(22, 22) : CompactMode ? new Size(26, 26) : new Size(28, 28);
                _configButton.Margin = ultraDenseLayout ? new Padding(4, 2, 1, 2) : new Padding(6, 4, 2, 4);

                _titleLabel.Font = ultraDenseLayout
                    ? new Font("Segoe UI Semibold", 7.6f, FontStyle.Bold)
                    : denseLayout
                        ? new Font("Segoe UI Semibold", 8.3f, FontStyle.Bold)
                        : CompactMode
                            ? new Font("Segoe UI Semibold", 9.2f, FontStyle.Bold)
                            : new Font("Segoe UI Semibold", 10f, FontStyle.Bold);

                Font minorBoldFont = ultraDenseLayout
                    ? new Font("Segoe UI Semibold", 7.0f, FontStyle.Bold)
                    : denseLayout
                        ? new Font("Segoe UI Semibold", 7.6f, FontStyle.Bold)
                        : CompactMode
                            ? new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold)
                            : new Font("Segoe UI Semibold", 9f, FontStyle.Bold);

                Font minorRegularFont = ultraDenseLayout
                    ? new Font("Segoe UI", 6.8f, FontStyle.Regular)
                    : denseLayout
                        ? new Font("Segoe UI", 7.2f, FontStyle.Regular)
                        : CompactMode
                            ? new Font("Segoe UI", 8f, FontStyle.Regular)
                            : new Font("Segoe UI", 8.5f, FontStyle.Regular);

                _faultLabel.Font = minorRegularFont;
                _setpointHeaderLabel.Font = minorRegularFont;
                _voltageHeaderLabel.Font = minorRegularFont;
                _currentHeaderLabel.Font = minorRegularFont;
                _rangeLabel.Font = minorRegularFont;
                _setpointLabel.Font = minorRegularFont;
                _setpointUnitLabel.Font = minorRegularFont;
                _enabledCheckBox.Font = minorRegularFont;
                _setpointTextBox.Font = minorRegularFont;

                _faultLed.Size = ultraDenseLayout ? new Size(12, 12) : denseLayout ? new Size(14, 14) : new Size(18, 18);
                _faultLed.Location = new Point(2, ultraDenseLayout ? 1 : 2);
                _faultLabel.Location = new Point(ultraDenseLayout ? 18 : denseLayout ? 20 : 25, ultraDenseLayout ? 0 : 3);

                _voltageValueLabel.Font = minorBoldFont;
                _currentValueLabel.Font = minorBoldFont;

                _bodyLayout.Padding = ultraDenseLayout ? new Padding(1, 2, 1, 1) : denseLayout ? new Padding(2, 2, 2, 1) : CompactMode ? new Padding(2, 3, 2, 2) : new Padding(4, 4, 4, 2);
                _bodyLayout.ColumnStyles[0].Width = denseLayout ? 29f : 31f;
                _bodyLayout.ColumnStyles[1].Width = denseLayout ? 35.5f : 34.5f;
                _bodyLayout.ColumnStyles[2].Width = denseLayout ? 35.5f : 34.5f;

                float instrumentHeaderHeight = ultraDenseLayout ? 12f : denseLayout ? 14f : CompactMode ? 17f : 18f;
                float instrumentFooterHeight = ultraDenseLayout ? 16f : denseLayout ? 18f : Height <= 300 ? 22f : 24f;
                _setpointLayout.RowStyles[0].Height = instrumentHeaderHeight;
                _voltageLayout.RowStyles[0].Height = instrumentHeaderHeight;
                _currentLayout.RowStyles[0].Height = instrumentHeaderHeight;
                _setpointLayout.RowStyles[2].Height = instrumentFooterHeight;
                _voltageLayout.RowStyles[2].Height = instrumentFooterHeight;
                _currentLayout.RowStyles[2].Height = instrumentFooterHeight;

                _footerPanel.Padding = ultraDenseLayout ? new Padding(4, 3, 4, 3) : denseLayout ? new Padding(5, 4, 5, 4) : new Padding(8, 6, 8, 6);
                _footerLayout.RowStyles[0].Height = ultraDenseLayout ? 18f : denseLayout ? 22f : 28f;

                _setpointHeaderLabel.Text = ultraDenseLayout ? "AJ" : denseLayout ? "AJ." : "AJUSTE";
                _voltageHeaderLabel.Text = ultraDenseLayout ? "V" : denseLayout ? "OUT V" : "SAÍDA V";
                _currentHeaderLabel.Text = ultraDenseLayout ? "I" : denseLayout ? "OUT I" : "SAÍDA I";
                _setpointLabel.Text = ultraDenseLayout ? "SP" : denseLayout ? "Set" : "Setpoint";
                _enabledCheckBox.Text = ultraDenseLayout ? "Hab." : denseLayout ? "Habil." : "Habilitado";

                UpdateRangeLabel();
                UpdateVoltageGaugeValue();
                UpdateCurrentGaugeValue();
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        private void ApplyTheme()
        {
            Color frameBackColor = IndustrialThemeEnabled ? Color.FromArgb(27, 31, 35) : Color.FromArgb(236, 239, 242);
            Color headerBackColor = IndustrialThemeEnabled ? Color.FromArgb(39, 43, 48) : Color.FromArgb(249, 250, 251);
            Color footerBackColor = IndustrialThemeEnabled ? Color.FromArgb(33, 37, 41) : Color.FromArgb(243, 245, 247);
            Color primaryText = IndustrialThemeEnabled ? Color.FromArgb(237, 240, 243) : Color.FromArgb(42, 49, 57);
            Color secondaryText = IndustrialThemeEnabled ? Color.FromArgb(171, 180, 188) : Color.FromArgb(88, 97, 106);
            Color editorBackColor = IndustrialThemeEnabled ? Color.FromArgb(23, 26, 29) : Color.White;
            Color editorTextColor = IndustrialThemeEnabled ? Color.FromArgb(238, 240, 242) : Color.FromArgb(38, 44, 50);

            BackColor = frameBackColor;
            _headerPanel.BackColor = headerBackColor;
            _footerPanel.BackColor = footerBackColor;

            _titleLabel.ForeColor = primaryText;
            _faultLabel.ForeColor = primaryText;
            _setpointHeaderLabel.ForeColor = secondaryText;
            _voltageHeaderLabel.ForeColor = secondaryText;
            _currentHeaderLabel.ForeColor = secondaryText;
            _rangeLabel.ForeColor = secondaryText;
            _voltageValueLabel.ForeColor = primaryText;
            _currentValueLabel.ForeColor = primaryText;
            _setpointLabel.ForeColor = primaryText;
            _setpointUnitLabel.ForeColor = primaryText;
            _enabledCheckBox.ForeColor = primaryText;

            _setpointTextBox.BackColor = editorBackColor;
            _setpointTextBox.ForeColor = editorTextColor;

            _configButton.FlatAppearance.BorderColor = IndustrialThemeEnabled
                ? Color.FromArgb(110, 118, 126)
                : Color.FromArgb(181, 188, 196);
            _configButton.FlatAppearance.MouseOverBackColor = IndustrialThemeEnabled
                ? Color.FromArgb(63, 69, 76)
                : Color.FromArgb(228, 233, 238);
            _configButton.FlatAppearance.MouseDownBackColor = IndustrialThemeEnabled
                ? Color.FromArgb(52, 58, 64)
                : Color.FromArgb(218, 224, 230);
            _configButton.ForeColor = primaryText;

            _faultLed.OnColor = Color.FromArgb(241, 72, 72);
            _faultLed.OffColor = IndustrialThemeEnabled ? Color.FromArgb(74, 28, 30) : Color.FromArgb(117, 77, 82);
            _faultLed.BorderColor = IndustrialThemeEnabled ? Color.FromArgb(32, 34, 37) : Color.FromArgb(109, 112, 117);

            _setpointSlider.TrackBackColor = IndustrialThemeEnabled ? Color.FromArgb(56, 61, 66) : Color.FromArgb(214, 220, 226);
            _setpointSlider.SlotColor = IndustrialThemeEnabled ? Color.FromArgb(20, 23, 26) : Color.FromArgb(242, 244, 246);
            _setpointSlider.FillColor = Color.FromArgb(65, 196, 159);
            _setpointSlider.ThumbColor = Color.FromArgb(72, 150, 214);
            _setpointSlider.TickColor = IndustrialThemeEnabled ? Color.FromArgb(154, 164, 172) : Color.FromArgb(136, 145, 153);
            _setpointSlider.BorderColor = IndustrialThemeEnabled ? Color.FromArgb(123, 129, 135) : Color.FromArgb(170, 176, 183);

            ConfigureVoltageGaugeTheme();
            ConfigureCurrentGaugeTheme();
        }

        private void ConfigureVoltageGaugeTheme()
        {
            _voltageGauge.BorderColor = Color.FromArgb(128, 134, 140);
            _voltageGauge.BorderThickness = 2;
            _voltageGauge.ChannelColor = Color.FromArgb(20, 23, 26);
            _voltageGauge.CornerRadius = 12;
            _voltageGauge.DisplayScaleFactor = GaugeScaleFactor;
            _voltageGauge.DisplayNumberFormat = VoltageRangeMax <= 5.05d ? "0.0" : "0";
            _voltageGauge.FillColor = Color.FromArgb(68, 198, 163);
            _voltageGauge.GaugeBackColor = IndustrialThemeEnabled ? Color.FromArgb(47, 51, 56) : Color.FromArgb(224, 228, 232);
            _voltageGauge.MinorTicksPerMajor = 3;
            _voltageGauge.ShowGlassEffect = true;
            _voltageGauge.ShowScaleLabels = true;
            _voltageGauge.ShowThresholdBands = false;
            _voltageGauge.ShowTicks = true;
            _voltageGauge.ShowTitle = false;
            _voltageGauge.ShowUnitText = false;
            _voltageGauge.ShowValueText = false;
            _voltageGauge.TextColor = Color.Gainsboro;
            _voltageGauge.TickColor = Color.FromArgb(110, 219, 170);
            UpdateVoltageGaugeScale();
        }

        private void ConfigureCurrentGaugeTheme()
        {
            _currentGauge.BorderColor = Color.FromArgb(128, 134, 140);
            _currentGauge.BorderThickness = 2;
            _currentGauge.ChannelColor = Color.FromArgb(20, 23, 26);
            _currentGauge.CornerRadius = 12;
            _currentGauge.DangerColor = Color.FromArgb(203, 72, 72);
            _currentGauge.DisplayScaleFactor = GaugeScaleFactor;
            _currentGauge.DisplayNumberFormat = CurrentMax <= 2.5d ? "0.0" : "0";
            _currentGauge.FillColor = Color.FromArgb(56, 173, 141);
            _currentGauge.GaugeBackColor = IndustrialThemeEnabled ? Color.FromArgb(47, 51, 56) : Color.FromArgb(224, 228, 232);
            _currentGauge.MinorTicksPerMajor = 3;
            _currentGauge.ShowGlassEffect = true;
            _currentGauge.ShowScaleLabels = true;
            _currentGauge.ShowThresholdBands = true;
            _currentGauge.ShowTicks = true;
            _currentGauge.ShowTitle = false;
            _currentGauge.ShowUnitText = false;
            _currentGauge.ShowValueText = false;
            _currentGauge.TextColor = Color.Gainsboro;
            _currentGauge.TickColor = IndustrialThemeEnabled ? Color.FromArgb(160, 170, 176) : Color.FromArgb(132, 139, 145);
            _currentGauge.WarningColor = Color.FromArgb(222, 171, 46);
            UpdateCurrentGaugeScale();
        }

        private void RefreshAllVisuals()
        {
            UpdateRangeLabel();
            UpdateSetpointTextBox();
            UpdateVoltageGaugeScale();
            UpdateVoltageGaugeValue();
            UpdateCurrentGaugeScale();
            UpdateCurrentGaugeValue();
        }

        private void BuildResponsiveLayout()
        {
            BuildHeaderLayout();
            _setpointLayout = BuildInstrumentLayout(_setpointPanel, _setpointHeaderLabel, _setpointSlider, _rangeLabel, 24f);
            _voltageLayout = BuildInstrumentLayout(_voltagePanel, _voltageHeaderLabel, _voltageGauge, _voltageValueLabel, 24f);
            _currentLayout = BuildInstrumentLayout(_currentPanel, _currentHeaderLabel, _currentGauge, _currentValueLabel, 24f);
        }

        private void BuildHeaderLayout()
        {
            _headerLayout = CreateLayoutPanel();
            _headerInfoLayout = CreateLayoutPanel();

            _headerPanel.SuspendLayout();
            try
            {
                _headerPanel.Controls.Clear();

                _headerLayout.ColumnCount = 2;
                _headerLayout.RowCount = 1;
                _headerLayout.ColumnStyles.Clear();
                _headerLayout.RowStyles.Clear();
                _headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                _headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                _headerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                _headerLayout.Dock = DockStyle.Fill;
                _headerLayout.Padding = new Padding(8, 6, 8, 6);

                _headerInfoLayout.ColumnCount = 1;
                _headerInfoLayout.RowCount = 2;
                _headerInfoLayout.ColumnStyles.Clear();
                _headerInfoLayout.RowStyles.Clear();
                _headerInfoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                _headerInfoLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
                _headerInfoLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22f));
                _headerInfoLayout.Dock = DockStyle.Fill;
                _headerInfoLayout.Margin = Padding.Empty;

                _titleLabel.Dock = DockStyle.Fill;
                _titleLabel.Margin = Padding.Empty;

                _faultPanel.Dock = DockStyle.Fill;
                _faultPanel.Margin = Padding.Empty;

                _configButton.Anchor = AnchorStyles.None;

                _headerInfoLayout.Controls.Clear();
                _headerInfoLayout.Controls.Add(_titleLabel, 0, 0);
                _headerInfoLayout.Controls.Add(_faultPanel, 0, 1);

                _headerLayout.Controls.Clear();
                _headerLayout.Controls.Add(_headerInfoLayout, 0, 0);
                _headerLayout.Controls.Add(_configButton, 1, 0);

                _headerPanel.Controls.Add(_headerLayout);
            }
            finally
            {
                _headerPanel.ResumeLayout();
            }
        }

        private TableLayoutPanel BuildInstrumentLayout(Panel hostPanel, Control headerControl, Control mainControl, Control footerControl, float footerHeight)
        {
            TableLayoutPanel layout = CreateLayoutPanel();

            hostPanel.SuspendLayout();
            try
            {
                hostPanel.Controls.Clear();

                layout.ColumnCount = 1;
                layout.RowCount = 3;
                layout.ColumnStyles.Clear();
                layout.RowStyles.Clear();
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18f));
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, footerHeight));
                layout.Dock = DockStyle.Fill;
                layout.Margin = Padding.Empty;
                layout.Padding = Padding.Empty;

                headerControl.Dock = DockStyle.Fill;
                headerControl.Margin = new Padding(0, 0, 0, 2);

                mainControl.Dock = DockStyle.Fill;
                mainControl.Margin = new Padding(0, 2, 0, 2);

                footerControl.Dock = DockStyle.Fill;
                footerControl.Margin = new Padding(0, 2, 0, 0);

                layout.Controls.Add(headerControl, 0, 0);
                layout.Controls.Add(mainControl, 0, 1);
                layout.Controls.Add(footerControl, 0, 2);

                hostPanel.Controls.Add(layout);
            }
            finally
            {
                hostPanel.ResumeLayout();
            }

            return layout;
        }

        private static TableLayoutPanel CreateLayoutPanel()
        {
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.BackColor = Color.Transparent;
            layout.Margin = Padding.Empty;
            layout.Padding = Padding.Empty;
            return layout;
        }

        private void UpdateRangeLabel()
        {
            _rangeLabel.Text = string.Format(
                CultureInfo.CurrentCulture,
                _denseLayoutActive ? "0-{0:0.##}V" : "0-{0:0.##} V",
                VoltageRangeMax);
        }

        private void UpdateSetpointTextBox()
        {
            _suppressSetpointSync = true;
            try
            {
                _setpointTextBox.Text = FormatNumber(_setpointVoltage);
            }
            finally
            {
                _suppressSetpointSync = false;
            }
        }

        private void UpdateVoltageGaugeScale()
        {
            _voltageGauge.Maximum = ScaleToGauge(VoltageRangeMax);
            _voltageGauge.WarningThreshold = _voltageGauge.Maximum;
            _voltageGauge.DangerThreshold = _voltageGauge.Maximum;
            _voltageGauge.DisplayNumberFormat = VoltageRangeMax <= 5.05d ? "0.0" : "0";
        }

        private void UpdateCurrentGaugeScale()
        {
            _currentGauge.Maximum = ScaleToGauge(CurrentMax);
            _currentGauge.WarningThreshold = ScaleToGauge(CurrentWarningThreshold);
            _currentGauge.DangerThreshold = ScaleToGauge(CurrentDangerThreshold);
            _currentGauge.DisplayNumberFormat = CurrentMax <= 2.5d ? "0.0" : "0";
        }

        private void UpdateVoltageGaugeValue()
        {
            double gaugeValue = Clamp(_measuredVoltage, 0d, VoltageRangeMax);
            _voltageGauge.Value = ScaleToGauge(gaugeValue);
            _voltageValueLabel.Text = _denseLayoutActive
                ? _measuredVoltage.ToString("0.0", CultureInfo.CurrentCulture)
                : string.Format(CultureInfo.CurrentCulture, "{0} V", FormatNumber(_measuredVoltage));
        }

        private void UpdateCurrentGaugeValue()
        {
            double gaugeValue = Clamp(_measuredCurrent, 0d, CurrentMax);
            _currentGauge.Value = ScaleToGauge(gaugeValue);
            _currentValueLabel.Text = _denseLayoutActive
                ? _measuredCurrent.ToString("0", CultureInfo.CurrentCulture)
                : string.Format(CultureInfo.CurrentCulture, "{0} {1}", FormatNumber(_measuredCurrent), CurrentUnitText);
        }

        private void SetpointSlider_ValueChanged(object sender, EventArgs e)
        {
            if (_suppressSetpointSync)
                return;

            ApplySetpointVoltage(_setpointSlider.Value, true, true, false);
        }

        private void SetpointTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_suppressSetpointSync)
                return;

            double parsedValue;
            if (!TryParseNumber(_setpointTextBox.Text, out parsedValue))
                return;

            ApplySetpointVoltage(parsedValue, true, false, true);
        }

        private void SetpointTextBox_Leave(object sender, EventArgs e)
        {
            NormalizeSetpointText();
        }

        private void SetpointTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            NormalizeSetpointText();
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void EnabledCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_suppressOutputEvent)
                return;

            OnOutputEnabledChanged(EventArgs.Empty);
        }

        private void ConfigButton_Click(object sender, EventArgs e)
        {
            OnConfigButtonClick(e);
        }

        private void ConfigButton_Paint(object sender, PaintEventArgs e)
        {
            Rectangle iconBounds = Rectangle.Inflate(_configButton.ClientRectangle, -7, -7);
            if (iconBounds.Width <= 6 || iconBounds.Height <= 6)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            DrawGearGlyph(e.Graphics, iconBounds, _configButton.ForeColor);
        }

        private void GsaChannelControl_Resize(object sender, EventArgs e)
        {
            ApplyCompactLayout();
        }

        private void NormalizeSetpointText()
        {
            double parsedValue;
            if (TryParseNumber(_setpointTextBox.Text, out parsedValue))
            {
                ApplySetpointVoltage(parsedValue, false, true, true);
                return;
            }

            UpdateSetpointTextBox();
        }

        private void ApplySetpointVoltage(double rawValue, bool raiseEvent, bool updateTextBox, bool updateSlider)
        {
            double clampedValue = Clamp(rawValue, 0d, VoltageRangeMax);
            bool changed = !AreClose(_setpointVoltage, clampedValue);
            _setpointVoltage = clampedValue;

            _suppressSetpointSync = true;
            try
            {
                if (updateSlider && !AreClose(_setpointSlider.Value, clampedValue))
                    _setpointSlider.Value = clampedValue;

                if (updateTextBox)
                    _setpointTextBox.Text = FormatNumber(clampedValue);
            }
            finally
            {
                _suppressSetpointSync = false;
            }

            if (changed && raiseEvent)
                OnSetpointVoltageChanged(EventArgs.Empty);
        }

        private void OnSetpointVoltageChanged(EventArgs e)
        {
            EventHandler handler = SetpointVoltageChanged;
            if (handler != null)
                handler(this, e);
        }

        private void OnOutputEnabledChanged(EventArgs e)
        {
            EventHandler handler = OutputEnabledChanged;
            if (handler != null)
                handler(this, e);
        }

        private void OnConfigButtonClick(EventArgs e)
        {
            EventHandler handler = ConfigButtonClick;
            if (handler != null)
                handler(this, e);
        }

        private bool TryParseNumber(string rawText, out double value)
        {
            string sanitizedText = (rawText ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(sanitizedText))
            {
                value = 0d;
                return false;
            }

            if (double.TryParse(sanitizedText, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                return true;

            if (double.TryParse(sanitizedText, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return true;

            string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            string normalizedText = sanitizedText.Replace(".", decimalSeparator).Replace(",", decimalSeparator);
            return double.TryParse(normalizedText, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
        }

        private string FormatNumber(double value)
        {
            return value.ToString("0.00", CultureInfo.CurrentCulture);
        }

        private static double Clamp(double value, double minimum, double maximum)
        {
            if (value < minimum)
                return minimum;
            if (value > maximum)
                return maximum;
            return value;
        }

        private static bool AreClose(double first, double second)
        {
            return Math.Abs(first - second) < 0.0005d;
        }

        private static int ScaleToGauge(double value)
        {
            return (int)Math.Round(value * GaugeScaleFactor, MidpointRounding.AwayFromZero);
        }

        private static void DrawGearGlyph(Graphics graphics, Rectangle bounds, Color color)
        {
            Rectangle gearBounds = Rectangle.Inflate(bounds, -1, -1);
            Point center = new Point(gearBounds.Left + (gearBounds.Width / 2), gearBounds.Top + (gearBounds.Height / 2));
            int outerRadius = Math.Max(4, Math.Min(gearBounds.Width, gearBounds.Height) / 2);
            int innerRadius = Math.Max(2, outerRadius / 2);

            using (GraphicsPath gearPath = new GraphicsPath())
            using (SolidBrush fillBrush = new SolidBrush(color))
            using (Pen borderPen = new Pen(Color.FromArgb(45, Color.Black), 1f))
            {
                for (int tooth = 0; tooth < 8; tooth++)
                {
                    double angle = tooth * (Math.PI / 4d);
                    double radians = angle - (Math.PI / 2d);
                    int toothWidth = Math.Max(2, outerRadius / 3);
                    int toothHeight = Math.Max(2, outerRadius / 2);
                    Point toothCenter = new Point(
                        center.X + (int)Math.Round(Math.Cos(radians) * (outerRadius - 1)),
                        center.Y + (int)Math.Round(Math.Sin(radians) * (outerRadius - 1)));

                    Rectangle toothBounds = new Rectangle(
                        toothCenter.X - (toothWidth / 2),
                        toothCenter.Y - (toothHeight / 2),
                        toothWidth,
                        toothHeight);

                    using (Matrix rotation = new Matrix())
                    {
                        rotation.RotateAt((float)(angle * (180d / Math.PI)), toothCenter);
                        GraphicsState state = graphics.Save();
                        try
                        {
                            graphics.MultiplyTransform(rotation);
                            graphics.FillRectangle(fillBrush, toothBounds);
                            graphics.DrawRectangle(borderPen, toothBounds);
                        }
                        finally
                        {
                            graphics.Restore(state);
                        }
                    }
                }

                graphics.FillEllipse(fillBrush, center.X - outerRadius + 2, center.Y - outerRadius + 2, (outerRadius - 2) * 2, (outerRadius - 2) * 2);
                graphics.DrawEllipse(borderPen, center.X - outerRadius + 2, center.Y - outerRadius + 2, (outerRadius - 2) * 2, (outerRadius - 2) * 2);

                using (SolidBrush innerBrush = new SolidBrush(Color.FromArgb(35, 38, 42)))
                {
                    graphics.FillEllipse(innerBrush, center.X - innerRadius, center.Y - innerRadius, innerRadius * 2, innerRadius * 2);
                }
            }
        }

        private static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return path;

            int effectiveRadius = Math.Max(0, Math.Min(radius, Math.Min(bounds.Width, bounds.Height) / 2));
            if (effectiveRadius <= 1)
            {
                path.AddRectangle(bounds);
                path.CloseFigure();
                return path;
            }

            int diameter = effectiveRadius * 2;
            Rectangle arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace SimulDIESEL.UI.Controls
{
    [DefaultProperty("Value")]
    [DefaultEvent(null)]
    [ToolboxItem(true)]
    public class SdVerticalGauge : Control
    {
        private const float InternalPadding = 6f;
        private const float MinimumChannelWidth = 12f;
        private const float MaximumChannelWidth = 30f;

        private int _minimum;
        private int _maximum = 100;
        private int _value;

        private Color _gaugeBackColor = Color.FromArgb(44, 48, 52);
        private Color _channelColor = Color.FromArgb(24, 27, 30);
        private Color _fillColor = Color.FromArgb(52, 168, 83);
        private Color _borderColor = Color.FromArgb(120, 127, 134);
        private int _borderThickness = 2;
        private int _cornerRadius = 12;
        private Color _textColor = Color.Gainsboro;

        private bool _showPercentage;
        private bool _showValueText = true;
        private bool _showUnitText = true;
        private string _unitText = string.Empty;
        private string _titleText = string.Empty;
        private bool _showTitle = true;
        private double _displayScaleFactor = 1d;
        private string _displayNumberFormat = "0";

        private bool _showTicks = true;
        private int _majorTickCount = 5;
        private int _minorTicksPerMajor = 4;
        private Color _tickColor = Color.FromArgb(160, 170, 176);
        private bool _showScaleLabels;

        private int _warningThreshold = 70;
        private int _dangerThreshold = 90;
        private Color _warningColor = Color.FromArgb(214, 167, 35);
        private Color _dangerColor = Color.FromArgb(196, 62, 62);
        private bool _autoColorByThreshold = true;
        private bool _showThresholdBands = true;

        private bool _inverted;
        private bool _showGlassEffect = true;

        public SdVerticalGauge()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);

            DoubleBuffered = true;
            base.BackColor = Color.Transparent;
            base.ForeColor = _textColor;
            Size = DefaultSize;
        }

        [Category("Behavior")]
        [DefaultValue(0)]
        public int Minimum
        {
            get { return _minimum; }
            set
            {
                if (_minimum == value)
                    return;

                _minimum = value;
                CoerceCurrentValue();
                Invalidate();
            }
        }

        [Category("Behavior")]
        [DefaultValue(100)]
        public int Maximum
        {
            get { return _maximum; }
            set
            {
                if (_maximum == value)
                    return;

                _maximum = value;
                CoerceCurrentValue();
                Invalidate();
            }
        }

        [Category("Behavior")]
        [DefaultValue(0)]
        public int Value
        {
            get { return _value; }
            set
            {
                int clampedValue = ClampValue(value);
                if (_value == clampedValue)
                    return;

                _value = clampedValue;
                Invalidate();
                OnValueChanged(EventArgs.Empty);
            }
        }

        [Category("Appearance")]
        public Color GaugeBackColor
        {
            get { return _gaugeBackColor; }
            set
            {
                if (_gaugeBackColor == value)
                    return;

                _gaugeBackColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color ChannelColor
        {
            get { return _channelColor; }
            set
            {
                if (_channelColor == value)
                    return;

                _channelColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color FillColor
        {
            get { return _fillColor; }
            set
            {
                if (_fillColor == value)
                    return;

                _fillColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                if (_borderColor == value)
                    return;

                _borderColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(2)]
        public int BorderThickness
        {
            get { return _borderThickness; }
            set
            {
                int sanitizedValue = Math.Max(1, value);
                if (_borderThickness == sanitizedValue)
                    return;

                _borderThickness = sanitizedValue;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(12)]
        public int CornerRadius
        {
            get { return _cornerRadius; }
            set
            {
                int sanitizedValue = Math.Max(0, value);
                if (_cornerRadius == sanitizedValue)
                    return;

                _cornerRadius = sanitizedValue;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color TextColor
        {
            get { return _textColor; }
            set
            {
                if (_textColor == value)
                    return;

                _textColor = value;
                base.ForeColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(false)]
        public bool ShowPercentage
        {
            get { return _showPercentage; }
            set
            {
                if (_showPercentage == value)
                    return;

                _showPercentage = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool ShowValueText
        {
            get { return _showValueText; }
            set
            {
                if (_showValueText == value)
                    return;

                _showValueText = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool ShowUnitText
        {
            get { return _showUnitText; }
            set
            {
                if (_showUnitText == value)
                    return;

                _showUnitText = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        public string UnitText
        {
            get { return _unitText; }
            set
            {
                string sanitizedValue = value ?? string.Empty;
                if (string.Equals(_unitText, sanitizedValue, StringComparison.Ordinal))
                    return;

                _unitText = sanitizedValue;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        public string TitleText
        {
            get { return _titleText; }
            set
            {
                string sanitizedValue = value ?? string.Empty;
                if (string.Equals(_titleText, sanitizedValue, StringComparison.Ordinal))
                    return;

                _titleText = sanitizedValue;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool ShowTitle
        {
            get { return _showTitle; }
            set
            {
                if (_showTitle == value)
                    return;

                _showTitle = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(1d)]
        public double DisplayScaleFactor
        {
            get { return _displayScaleFactor; }
            set
            {
                double sanitizedValue = value <= 0d ? 1d : value;
                if (Math.Abs(_displayScaleFactor - sanitizedValue) < 0.000001d)
                    return;

                _displayScaleFactor = sanitizedValue;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue("0")]
        public string DisplayNumberFormat
        {
            get { return _displayNumberFormat; }
            set
            {
                string sanitizedValue = string.IsNullOrWhiteSpace(value) ? "0" : value;
                if (string.Equals(_displayNumberFormat, sanitizedValue, StringComparison.Ordinal))
                    return;

                _displayNumberFormat = sanitizedValue;
                Invalidate();
            }
        }

        [Category("Scale")]
        [DefaultValue(true)]
        public bool ShowTicks
        {
            get { return _showTicks; }
            set
            {
                if (_showTicks == value)
                    return;

                _showTicks = value;
                Invalidate();
            }
        }

        [Category("Scale")]
        [DefaultValue(5)]
        public int MajorTickCount
        {
            get { return _majorTickCount; }
            set
            {
                int sanitizedValue = Math.Max(2, value);
                if (_majorTickCount == sanitizedValue)
                    return;

                _majorTickCount = sanitizedValue;
                Invalidate();
            }
        }

        [Category("Scale")]
        [DefaultValue(4)]
        public int MinorTicksPerMajor
        {
            get { return _minorTicksPerMajor; }
            set
            {
                int sanitizedValue = Math.Max(0, value);
                if (_minorTicksPerMajor == sanitizedValue)
                    return;

                _minorTicksPerMajor = sanitizedValue;
                Invalidate();
            }
        }

        [Category("Scale")]
        public Color TickColor
        {
            get { return _tickColor; }
            set
            {
                if (_tickColor == value)
                    return;

                _tickColor = value;
                Invalidate();
            }
        }

        [Category("Scale")]
        [DefaultValue(false)]
        public bool ShowScaleLabels
        {
            get { return _showScaleLabels; }
            set
            {
                if (_showScaleLabels == value)
                    return;

                _showScaleLabels = value;
                Invalidate();
            }
        }

        [Category("Alarm")]
        [DefaultValue(70)]
        public int WarningThreshold
        {
            get { return _warningThreshold; }
            set
            {
                if (_warningThreshold == value)
                    return;

                _warningThreshold = value;
                Invalidate();
            }
        }

        [Category("Alarm")]
        [DefaultValue(90)]
        public int DangerThreshold
        {
            get { return _dangerThreshold; }
            set
            {
                if (_dangerThreshold == value)
                    return;

                _dangerThreshold = value;
                Invalidate();
            }
        }

        [Category("Alarm")]
        public Color WarningColor
        {
            get { return _warningColor; }
            set
            {
                if (_warningColor == value)
                    return;

                _warningColor = value;
                Invalidate();
            }
        }

        [Category("Alarm")]
        public Color DangerColor
        {
            get { return _dangerColor; }
            set
            {
                if (_dangerColor == value)
                    return;

                _dangerColor = value;
                Invalidate();
            }
        }

        [Category("Alarm")]
        [DefaultValue(true)]
        public bool AutoColorByThreshold
        {
            get { return _autoColorByThreshold; }
            set
            {
                if (_autoColorByThreshold == value)
                    return;

                _autoColorByThreshold = value;
                Invalidate();
            }
        }

        [Category("Alarm")]
        [DefaultValue(true)]
        public bool ShowThresholdBands
        {
            get { return _showThresholdBands; }
            set
            {
                if (_showThresholdBands == value)
                    return;

                _showThresholdBands = value;
                Invalidate();
            }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        public bool Inverted
        {
            get { return _inverted; }
            set
            {
                if (_inverted == value)
                    return;

                _inverted = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool ShowGlassEffect
        {
            get { return _showGlassEffect; }
            set
            {
                if (_showGlassEffect == value)
                    return;

                _showGlassEffect = value;
                Invalidate();
            }
        }

        [Category("Behavior")]
        public event EventHandler ValueChanged;

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color ForeColor
        {
            get { return base.ForeColor; }
            set { base.ForeColor = value; }
        }

        protected override Size DefaultSize
        {
            get { return new Size(90, 260); }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (base.BackColor == Color.Transparent && Parent != null)
            {
                // Reaproveita o fundo do contêiner para não deixar halo ao redor do corpo arredondado.
                GraphicsState state = pevent.Graphics.Save();
                try
                {
                    pevent.Graphics.TranslateTransform(-Left, -Top);
                    PaintEventArgs parentArgs = new PaintEventArgs(pevent.Graphics, Parent.ClientRectangle);
                    InvokePaintBackground(Parent, parentArgs);
                    InvokePaint(Parent, parentArgs);
                }
                finally
                {
                    pevent.Graphics.Restore(state);
                }

                return;
            }

            base.OnPaintBackground(pevent);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (ClientRectangle.Width < 8 || ClientRectangle.Height < 8)
                return;

            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Rectangle frameBounds = Rectangle.Inflate(ClientRectangle, -1, -1);
            if (frameBounds.Width <= 0 || frameBounds.Height <= 0)
                return;

            GaugeLayout layout = CalculateLayout(frameBounds);
            DrawOuterFrame(graphics, frameBounds);
            DrawChannel(graphics, layout.ChannelRect);
            DrawTicks(graphics, layout);
            DrawFill(graphics, layout.ChannelRect);
            DrawTexts(graphics, layout);
        }

        protected virtual void OnValueChanged(EventArgs e)
        {
            EventHandler handler = ValueChanged;
            if (handler != null)
                handler(this, e);
        }

        private void CoerceCurrentValue()
        {
            int clampedValue = ClampValue(_value);
            if (_value == clampedValue)
                return;

            _value = clampedValue;
            OnValueChanged(EventArgs.Empty);
        }

        private int ClampValue(int rawValue)
        {
            int safeMinimum;
            int safeMaximum;
            GetSafeRange(out safeMinimum, out safeMaximum);

            if (rawValue < safeMinimum)
                return safeMinimum;

            if (rawValue > safeMaximum)
                return safeMaximum;

            return rawValue;
        }

        private void GetSafeRange(out int safeMinimum, out int safeMaximum)
        {
            safeMinimum = _minimum;
            safeMaximum = _maximum > _minimum ? _maximum : _minimum + 1;
        }

        private double GetNormalizedValue(int rawValue)
        {
            int safeMinimum;
            int safeMaximum;
            GetSafeRange(out safeMinimum, out safeMaximum);

            int clampedValue = rawValue;
            if (clampedValue < safeMinimum)
                clampedValue = safeMinimum;
            else if (clampedValue > safeMaximum)
                clampedValue = safeMaximum;

            return (clampedValue - safeMinimum) / (double)(safeMaximum - safeMinimum);
        }

        private Color GetActiveFillColor()
        {
            if (!AutoColorByThreshold)
                return FillColor;

            int lowerThreshold = Math.Min(WarningThreshold, DangerThreshold);
            int upperThreshold = Math.Max(WarningThreshold, DangerThreshold);
            int effectiveValue = ClampValue(Value);

            if (effectiveValue >= upperThreshold)
                return DangerColor;

            if (effectiveValue >= lowerThreshold)
                return WarningColor;

            return FillColor;
        }

        private GaugeLayout CalculateLayout(Rectangle outerBounds)
        {
            // Reserva áreas estáveis para título, escala e rodapé sem deformar o canal principal.
            RectangleF bodyRect = RectangleF.Inflate(outerBounds, -Math.Max(BorderThickness, 1) - 1, -Math.Max(BorderThickness, 1) - 1);
            if (bodyRect.Width <= 0f || bodyRect.Height <= 0f)
                return new GaugeLayout();

            float leftScaleWidth = 0f;
            if (ShowTicks)
                leftScaleWidth += 12f;

            if (ShowTicks && ShowScaleLabels)
                leftScaleWidth += Math.Min(26f, Math.Max(18f, bodyRect.Width * 0.22f));

            float rightTickWidth = ShowTicks ? 8f : 0f;
            float titleHeight = ShouldDrawTitle(bodyRect) ? 22f : 0f;
            float footerHeight = GetFooterHeight(bodyRect);

            float channelTop = bodyRect.Top + InternalPadding + titleHeight;
            float channelBottom = bodyRect.Bottom - InternalPadding - footerHeight;

            if (channelBottom - channelTop < 18f)
            {
                titleHeight = 0f;
                footerHeight = 0f;
                channelTop = bodyRect.Top + InternalPadding;
                channelBottom = bodyRect.Bottom - InternalPadding;
            }

            float availableHeight = Math.Max(18f, channelBottom - channelTop);
            float usableWidth = Math.Max(16f, bodyRect.Width - leftScaleWidth - rightTickWidth - (InternalPadding * 2f));
            float channelWidth = Math.Min(MaximumChannelWidth, usableWidth);
            channelWidth = Math.Max(MinimumChannelWidth, channelWidth);
            channelWidth = Math.Min(channelWidth, Math.Max(MinimumChannelWidth, bodyRect.Width - leftScaleWidth - rightTickWidth - InternalPadding));

            float channelX = bodyRect.Left + leftScaleWidth + ((bodyRect.Width - leftScaleWidth - rightTickWidth - channelWidth) / 2f);
            if (channelX < bodyRect.Left + InternalPadding)
                channelX = bodyRect.Left + InternalPadding;

            float maxChannelRight = bodyRect.Right - rightTickWidth - InternalPadding;
            if (channelX + channelWidth > maxChannelRight)
                channelX = Math.Max(bodyRect.Left + InternalPadding, maxChannelRight - channelWidth);

            RectangleF channelRect = new RectangleF(channelX, channelTop, channelWidth, availableHeight);

            return new GaugeLayout
            {
                BodyRect = bodyRect,
                ChannelRect = channelRect,
                ScaleRect = new RectangleF(bodyRect.Left + InternalPadding, channelRect.Top, Math.Max(0f, channelRect.Left - bodyRect.Left - InternalPadding - 2f), channelRect.Height),
                TitleRect = new RectangleF(bodyRect.Left + InternalPadding, bodyRect.Top + InternalPadding - 1f, bodyRect.Width - (InternalPadding * 2f), titleHeight),
                FooterRect = new RectangleF(bodyRect.Left + InternalPadding, bodyRect.Bottom - InternalPadding - footerHeight, bodyRect.Width - (InternalPadding * 2f), footerHeight)
            };
        }

        private bool ShouldDrawTitle(RectangleF bodyRect)
        {
            return ShowTitle &&
                   !string.IsNullOrWhiteSpace(TitleText) &&
                   bodyRect.Height >= 70f;
        }

        private float GetFooterHeight(RectangleF bodyRect)
        {
            if (bodyRect.Height < 90f)
                return 0f;

            bool hasPrimaryLine = ShowValueText || (ShowUnitText && !string.IsNullOrWhiteSpace(UnitText));
            bool hasSecondaryLine = ShowPercentage;

            int lineCount = 0;
            if (hasPrimaryLine)
                lineCount++;
            if (hasSecondaryLine)
                lineCount++;

            if (lineCount == 0)
                return 0f;

            return (lineCount * 16f) + 6f;
        }

        private void DrawOuterFrame(Graphics graphics, Rectangle bounds)
        {
            using (GraphicsPath framePath = CreateRoundedPath(bounds, CornerRadius))
            using (LinearGradientBrush bodyBrush = new LinearGradientBrush(
                bounds,
                ControlPaint.Light(GaugeBackColor, 0.08f),
                ControlPaint.Dark(GaugeBackColor, 0.20f),
                LinearGradientMode.Vertical))
            using (Pen borderPen = new Pen(BorderColor, BorderThickness))
            using (Pen innerPen = new Pen(Color.FromArgb(70, Color.White), 1f))
            {
                graphics.FillPath(bodyBrush, framePath);

                if (ShowGlassEffect && bounds.Height > 40)
                {
                    GraphicsState state = graphics.Save();
                    try
                    {
                        graphics.SetClip(framePath);
                        Rectangle glossRect = new Rectangle(bounds.Left + 1, bounds.Top + 1, bounds.Width - 2, Math.Max(8, bounds.Height / 3));
                        using (LinearGradientBrush glossBrush = new LinearGradientBrush(
                            glossRect,
                            Color.FromArgb(55, Color.White),
                            Color.FromArgb(0, Color.White),
                            LinearGradientMode.Vertical))
                        {
                            graphics.FillRectangle(glossBrush, glossRect);
                        }
                    }
                    finally
                    {
                        graphics.Restore(state);
                    }
                }

                borderPen.Alignment = PenAlignment.Inset;
                graphics.DrawPath(borderPen, framePath);

                Rectangle innerBounds = Rectangle.Inflate(bounds, -Math.Max(2, BorderThickness), -Math.Max(2, BorderThickness));
                if (innerBounds.Width > 4 && innerBounds.Height > 4)
                {
                    using (GraphicsPath innerPath = CreateRoundedPath(innerBounds, Math.Max(0, CornerRadius - BorderThickness)))
                    {
                        graphics.DrawPath(innerPen, innerPath);
                    }
                }
            }
        }

        private void DrawChannel(Graphics graphics, RectangleF channelRect)
        {
            if (channelRect.Width <= 0f || channelRect.Height <= 0f)
                return;

            Rectangle roundedRect = Rectangle.Round(channelRect);
            int channelRadius = Math.Max(3, CornerRadius - 4);

            using (GraphicsPath outerPath = CreateRoundedPath(roundedRect, channelRadius))
            using (LinearGradientBrush channelBrush = new LinearGradientBrush(
                roundedRect,
                ControlPaint.Light(ChannelColor, 0.05f),
                ControlPaint.Dark(ChannelColor, 0.18f),
                LinearGradientMode.Vertical))
            using (Pen channelPen = new Pen(Color.FromArgb(140, BorderColor), 1f))
            {
                graphics.FillPath(channelBrush, outerPath);

                RectangleF innerRect = RectangleF.Inflate(channelRect, -2f, -2f);
                if (innerRect.Width > 2f && innerRect.Height > 2f)
                {
                    using (GraphicsPath innerPath = CreateRoundedPath(Rectangle.Round(innerRect), Math.Max(2, channelRadius - 2)))
                    {
                        DrawThresholdZones(graphics, innerPath, innerRect);
                    }
                }

                graphics.DrawPath(channelPen, outerPath);
            }
        }

        private void DrawThresholdZones(Graphics graphics, GraphicsPath clipPath, RectangleF innerRect)
        {
            if (!ShowThresholdBands)
                return;

            // As faixas de aviso e perigo ficam abaixo do preenchimento para sugerir a zona operacional.
            GraphicsState state = graphics.Save();
            try
            {
                graphics.SetClip(clipPath);

                int lowerThreshold = Math.Min(WarningThreshold, DangerThreshold);
                int upperThreshold = Math.Max(WarningThreshold, DangerThreshold);

                RectangleF warningBand = GetBandRectangle(innerRect, lowerThreshold, upperThreshold);
                if (!warningBand.IsEmpty)
                {
                    using (SolidBrush warningBrush = new SolidBrush(Color.FromArgb(45, WarningColor)))
                    {
                        graphics.FillRectangle(warningBrush, warningBand);
                    }
                }

                RectangleF dangerBand = GetBandRectangle(innerRect, upperThreshold, Maximum <= Minimum ? Minimum + 1 : Maximum);
                if (!dangerBand.IsEmpty)
                {
                    using (SolidBrush dangerBrush = new SolidBrush(Color.FromArgb(58, DangerColor)))
                    {
                        graphics.FillRectangle(dangerBrush, dangerBand);
                    }
                }

                using (LinearGradientBrush shadowBrush = new LinearGradientBrush(
                    Rectangle.Round(innerRect),
                    Color.FromArgb(40, Color.Black),
                    Color.FromArgb(0, Color.White),
                    LinearGradientMode.Horizontal))
                {
                    graphics.FillRectangle(shadowBrush, innerRect);
                }
            }
            finally
            {
                graphics.Restore(state);
            }
        }

        private RectangleF GetBandRectangle(RectangleF area, int startValue, int endValue)
        {
            if (endValue <= startValue)
                return RectangleF.Empty;

            double startFraction = GetNormalizedValue(startValue);
            double endFraction = GetNormalizedValue(endValue);

            float top;
            float bottom;

            if (!Inverted)
            {
                top = area.Bottom - (float)(endFraction * area.Height);
                bottom = area.Bottom - (float)(startFraction * area.Height);
            }
            else
            {
                top = area.Top + (float)(startFraction * area.Height);
                bottom = area.Top + (float)(endFraction * area.Height);
            }

            if (bottom <= top)
                return RectangleF.Empty;

            return RectangleF.FromLTRB(area.Left, top, area.Right, bottom);
        }

        private void DrawTicks(Graphics graphics, GaugeLayout layout)
        {
            if (!ShowTicks || layout.ChannelRect.Height <= 0f)
                return;

            int visibleMajorTicks = Math.Max(2, MajorTickCount);
            int totalIntervals = visibleMajorTicks - 1;
            RectangleF channelRect = layout.ChannelRect;

            using (Pen majorPen = new Pen(TickColor, 1.2f))
            using (Pen minorPen = new Pen(Color.FromArgb(160, TickColor), 1f))
            using (SolidBrush labelBrush = new SolidBrush(TextColor))
            using (Font labelFont = new Font(Font.FontFamily, Math.Max(6.5f, Font.Size - 1f), FontStyle.Regular))
            using (StringFormat labelFormat = new StringFormat())
            {
                labelFormat.Alignment = StringAlignment.Far;
                labelFormat.LineAlignment = StringAlignment.Center;
                labelFormat.Trimming = StringTrimming.EllipsisCharacter;
                labelFormat.FormatFlags = StringFormatFlags.NoWrap;

                for (int majorIndex = 0; majorIndex < visibleMajorTicks; majorIndex++)
                {
                    double fraction = totalIntervals == 0 ? 0d : majorIndex / (double)totalIntervals;
                    float y = GetPositionFromFraction(channelRect, fraction);

                    graphics.DrawLine(majorPen, channelRect.Left - 8f, y, channelRect.Left - 2f, y);
                    graphics.DrawLine(majorPen, channelRect.Right + 2f, y, channelRect.Right + 5f, y);

                    if (ShowScaleLabels && layout.ScaleRect.Width >= 16f)
                    {
                        int labelValue = GetValueFromFraction(fraction);
                        RectangleF labelRect = new RectangleF(layout.ScaleRect.Left, y - 9f, layout.ScaleRect.Width, 18f);
                        graphics.DrawString(FormatDisplayValue(labelValue), labelFont, labelBrush, labelRect, labelFormat);
                    }
                }

                if (MinorTicksPerMajor <= 0)
                    return;

                for (int intervalIndex = 0; intervalIndex < totalIntervals; intervalIndex++)
                {
                    for (int minorIndex = 1; minorIndex <= MinorTicksPerMajor; minorIndex++)
                    {
                        double fraction = (intervalIndex + (minorIndex / (double)(MinorTicksPerMajor + 1))) / totalIntervals;
                        float y = GetPositionFromFraction(channelRect, fraction);
                        graphics.DrawLine(minorPen, channelRect.Left - 5f, y, channelRect.Left - 2f, y);
                        graphics.DrawLine(minorPen, channelRect.Right + 2f, y, channelRect.Right + 4f, y);
                    }
                }
            }
        }

        private float GetPositionFromFraction(RectangleF channelRect, double fraction)
        {
            if (!Inverted)
                return channelRect.Bottom - (float)(fraction * channelRect.Height);

            return channelRect.Top + (float)(fraction * channelRect.Height);
        }

        private int GetValueFromFraction(double fraction)
        {
            int safeMinimum;
            int safeMaximum;
            GetSafeRange(out safeMinimum, out safeMaximum);

            return safeMinimum + (int)Math.Round((safeMaximum - safeMinimum) * fraction);
        }

        private void DrawFill(Graphics graphics, RectangleF channelRect)
        {
            RectangleF innerRect = RectangleF.Inflate(channelRect, -3f, -3f);
            if (innerRect.Width <= 1f || innerRect.Height <= 1f)
                return;

            double normalizedValue = GetNormalizedValue(Value);
            float fillLength = (float)(normalizedValue * innerRect.Height);
            if (fillLength <= 0.1f)
                return;

            RectangleF fillRect;
            if (!Inverted)
                fillRect = new RectangleF(innerRect.Left, innerRect.Bottom - fillLength, innerRect.Width, fillLength);
            else
                fillRect = new RectangleF(innerRect.Left, innerRect.Top, innerRect.Width, fillLength);

            Color activeColor = GetActiveFillColor();
            Rectangle roundedInner = Rectangle.Round(innerRect);

            using (GraphicsPath clipPath = CreateRoundedPath(roundedInner, Math.Max(2, CornerRadius - 5)))
            {
                // O recorte impede que o preenchimento e os brilhos vazem para fora do canal arredondado.
                GraphicsState state = graphics.Save();
                try
                {
                    graphics.SetClip(clipPath);

                    using (LinearGradientBrush fillBrush = new LinearGradientBrush(
                        Rectangle.Round(fillRect),
                        ControlPaint.Light(activeColor, 0.10f),
                        ControlPaint.Dark(activeColor, 0.10f),
                        LinearGradientMode.Vertical))
                    {
                        graphics.FillRectangle(fillBrush, fillRect);
                    }

                    RectangleF highlightRect = new RectangleF(fillRect.Left + 1f, fillRect.Top, Math.Max(2f, fillRect.Width * 0.24f), fillRect.Height);
                    using (SolidBrush highlightBrush = new SolidBrush(Color.FromArgb(42, Color.White)))
                    {
                        graphics.FillRectangle(highlightBrush, highlightRect);
                    }

                    if (ShowGlassEffect)
                    {
                        float glossHeight = Math.Min(20f, fillRect.Height * 0.25f);
                        RectangleF glossRect = !Inverted
                            ? new RectangleF(fillRect.Left, fillRect.Top, fillRect.Width, glossHeight)
                            : new RectangleF(fillRect.Left, fillRect.Bottom - glossHeight, fillRect.Width, glossHeight);

                        using (LinearGradientBrush glossBrush = new LinearGradientBrush(
                            Rectangle.Round(glossRect),
                            Color.FromArgb(58, Color.White),
                            Color.FromArgb(0, Color.White),
                            LinearGradientMode.Vertical))
                        {
                            graphics.FillRectangle(glossBrush, glossRect);
                        }
                    }

                    using (Pen separatorPen = new Pen(Color.FromArgb(105, Color.White), 1f))
                    {
                        float separatorY = !Inverted ? fillRect.Top : fillRect.Bottom;
                        graphics.DrawLine(separatorPen, innerRect.Left, separatorY, innerRect.Right, separatorY);
                    }
                }
                finally
                {
                    graphics.Restore(state);
                }
            }
        }

        private void DrawTexts(Graphics graphics, GaugeLayout layout)
        {
            using (SolidBrush textBrush = new SolidBrush(TextColor))
            using (StringFormat centerFormat = new StringFormat())
            using (Font titleFont = new Font(Font.FontFamily, Math.Max(7f, Font.Size), FontStyle.Bold))
            using (Font valueFont = new Font(Font.FontFamily, Math.Max(8.5f, Font.Size + 1f), FontStyle.Bold))
            using (Font secondaryFont = new Font(Font.FontFamily, Math.Max(6.5f, Font.Size - 0.5f), FontStyle.Regular))
            {
                centerFormat.Alignment = StringAlignment.Center;
                centerFormat.LineAlignment = StringAlignment.Center;
                centerFormat.Trimming = StringTrimming.EllipsisCharacter;
                centerFormat.FormatFlags = StringFormatFlags.NoWrap;

                if (layout.TitleRect.Height > 0f && ShouldDrawTitle(layout.BodyRect))
                    graphics.DrawString(TitleText, titleFont, textBrush, layout.TitleRect, centerFormat);

                if (layout.FooterRect.Height <= 0f)
                    return;

                string primaryLine = BuildPrimaryFooterText();
                string secondaryLine = ShowPercentage ? BuildPercentageText() : string.Empty;

                bool hasPrimary = !string.IsNullOrWhiteSpace(primaryLine);
                bool hasSecondary = !string.IsNullOrWhiteSpace(secondaryLine);

                if (!hasPrimary && !hasSecondary)
                    return;

                if (hasPrimary && hasSecondary)
                {
                    RectangleF primaryRect = new RectangleF(layout.FooterRect.Left, layout.FooterRect.Top, layout.FooterRect.Width, layout.FooterRect.Height / 2f);
                    RectangleF secondaryRect = new RectangleF(layout.FooterRect.Left, layout.FooterRect.Top + (layout.FooterRect.Height / 2f) - 1f, layout.FooterRect.Width, layout.FooterRect.Height / 2f);
                    graphics.DrawString(primaryLine, valueFont, textBrush, primaryRect, centerFormat);
                    graphics.DrawString(secondaryLine, secondaryFont, textBrush, secondaryRect, centerFormat);
                    return;
                }

                Font singleLineFont = hasPrimary ? valueFont : secondaryFont;
                string text = hasPrimary ? primaryLine : secondaryLine;
                graphics.DrawString(text, singleLineFont, textBrush, layout.FooterRect, centerFormat);
            }
        }

        private string BuildPrimaryFooterText()
        {
            string valueText = ShowValueText ? FormatDisplayValue(ClampValue(Value)) : string.Empty;
            string unitText = ShowUnitText ? UnitText : string.Empty;

            if (!string.IsNullOrWhiteSpace(valueText) && !string.IsNullOrWhiteSpace(unitText))
                return valueText + " " + unitText;

            if (!string.IsNullOrWhiteSpace(valueText))
                return valueText;

            if (!string.IsNullOrWhiteSpace(unitText))
                return unitText;

            return string.Empty;
        }

        private string BuildPercentageText()
        {
            int percent = (int)Math.Round(GetNormalizedValue(Value) * 100d);
            return percent.ToString() + "%";
        }

        private string FormatDisplayValue(int rawValue)
        {
            double scaleFactor = DisplayScaleFactor <= 0d ? 1d : DisplayScaleFactor;
            double displayValue = rawValue / scaleFactor;

            try
            {
                return displayValue.ToString(DisplayNumberFormat, System.Globalization.CultureInfo.CurrentCulture);
            }
            catch (FormatException)
            {
                return displayValue.ToString("0.##", System.Globalization.CultureInfo.CurrentCulture);
            }
        }

        private GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
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

        private struct GaugeLayout
        {
            public RectangleF BodyRect;
            public RectangleF ChannelRect;
            public RectangleF ScaleRect;
            public RectangleF TitleRect;
            public RectangleF FooterRect;
        }
    }
}

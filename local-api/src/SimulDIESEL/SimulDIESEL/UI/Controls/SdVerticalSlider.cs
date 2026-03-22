using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SimulDIESEL.UI.Controls
{
    [DefaultProperty("Value")]
    [DefaultEvent("ValueChanged")]
    [ToolboxItem(true)]
    public class SdVerticalSlider : Control
    {
        private double _minimum;
        private double _maximum = 5d;
        private double _value;
        private int _majorTickCount = 6;
        private Color _trackBackColor = Color.FromArgb(56, 61, 66);
        private Color _slotColor = Color.FromArgb(20, 23, 26);
        private Color _fillColor = Color.FromArgb(49, 174, 138);
        private Color _thumbColor = Color.FromArgb(73, 157, 216);
        private Color _tickColor = Color.FromArgb(154, 164, 172);
        private Color _borderColor = Color.FromArgb(123, 129, 135);
        private bool _dragging;

        public SdVerticalSlider()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.Selectable |
                ControlStyles.SupportsTransparentBackColor,
                true);

            DoubleBuffered = true;
            base.BackColor = Color.Transparent;
            TabStop = true;
            Size = DefaultSize;
        }

        [Category("Behavior")]
        [DefaultValue(0d)]
        public double Minimum
        {
            get { return _minimum; }
            set
            {
                if (Math.Abs(_minimum - value) < 0.000001d)
                    return;

                _minimum = value;
                CoerceValue();
                Invalidate();
            }
        }

        [Category("Behavior")]
        [DefaultValue(5d)]
        public double Maximum
        {
            get { return _maximum; }
            set
            {
                if (Math.Abs(_maximum - value) < 0.000001d)
                    return;

                _maximum = value;
                CoerceValue();
                Invalidate();
            }
        }

        [Category("Behavior")]
        [DefaultValue(0d)]
        public double Value
        {
            get { return _value; }
            set
            {
                double clampedValue = ClampValue(value);
                if (Math.Abs(_value - clampedValue) < 0.000001d)
                    return;

                _value = clampedValue;
                Invalidate();
                OnValueChanged(EventArgs.Empty);
            }
        }

        [Category("Scale")]
        [DefaultValue(6)]
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

        [Category("Appearance")]
        public Color TrackBackColor
        {
            get { return _trackBackColor; }
            set
            {
                if (_trackBackColor == value)
                    return;

                _trackBackColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color SlotColor
        {
            get { return _slotColor; }
            set
            {
                if (_slotColor == value)
                    return;

                _slotColor = value;
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
        public Color ThumbColor
        {
            get { return _thumbColor; }
            set
            {
                if (_thumbColor == value)
                    return;

                _thumbColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
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

        protected override Size DefaultSize
        {
            get { return new Size(48, 222); }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left)
                return;

            Focus();
            _dragging = true;
            Capture = true;
            UpdateValueFromPoint(e.Location);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!_dragging)
                return;

            UpdateValueFromPoint(e.Location);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button != MouseButtons.Left)
                return;

            _dragging = false;
            Capture = false;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            double step = Math.Max((GetSafeMaximum() - Minimum) / 100d, 0.01d);
            if (e.Delta > 0)
                Value += step;
            else if (e.Delta < 0)
                Value -= step;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            Keys key = keyData & Keys.KeyCode;
            return key == Keys.Up || key == Keys.Down || key == Keys.PageUp || key == Keys.PageDown || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            double fineStep = Math.Max((GetSafeMaximum() - Minimum) / 100d, 0.01d);
            double pageStep = Math.Max((GetSafeMaximum() - Minimum) / 10d, 0.05d);

            if (e.KeyCode == Keys.Up)
            {
                Value += fineStep;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                Value -= fineStep;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.PageUp)
            {
                Value += pageStep;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.PageDown)
            {
                Value -= pageStep;
                e.Handled = true;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle frameBounds = Rectangle.Inflate(ClientRectangle, -2, -2);
            if (frameBounds.Width <= 8 || frameBounds.Height <= 8)
                return;

            Rectangle trackBounds = GetTrackBounds(frameBounds);
            if (trackBounds.Width <= 2 || trackBounds.Height <= 10)
                return;

            DrawTicks(e.Graphics, trackBounds);
            DrawTrack(e.Graphics, trackBounds);
            DrawThumb(e.Graphics, trackBounds);

            if (Focused)
                DrawFocusCue(e.Graphics, frameBounds);
        }

        protected virtual void OnValueChanged(EventArgs e)
        {
            EventHandler handler = ValueChanged;
            if (handler != null)
                handler(this, e);
        }

        private void CoerceValue()
        {
            double clampedValue = ClampValue(_value);
            if (Math.Abs(_value - clampedValue) < 0.000001d)
                return;

            _value = clampedValue;
            OnValueChanged(EventArgs.Empty);
        }

        private double ClampValue(double value)
        {
            double safeMaximum = GetSafeMaximum();
            if (value < Minimum)
                return Minimum;
            if (value > safeMaximum)
                return safeMaximum;
            return value;
        }

        private double GetSafeMaximum()
        {
            return Maximum > Minimum ? Maximum : Minimum + 1d;
        }

        private double GetNormalizedValue(double rawValue)
        {
            double safeMaximum = GetSafeMaximum();
            double clampedValue = ClampValue(rawValue);
            return (clampedValue - Minimum) / (safeMaximum - Minimum);
        }

        private void UpdateValueFromPoint(Point point)
        {
            Rectangle trackBounds = GetTrackBounds(Rectangle.Inflate(ClientRectangle, -2, -2));
            if (trackBounds.Width <= 2 || trackBounds.Height <= 10)
                return;

            Rectangle slotRect = GetSlotBounds(trackBounds);
            if (slotRect.Height <= 0)
                return;

            double normalized = 1d - ((point.Y - slotRect.Top) / (double)slotRect.Height);
            if (normalized < 0d)
                normalized = 0d;
            else if (normalized > 1d)
                normalized = 1d;

            Value = Minimum + ((GetSafeMaximum() - Minimum) * normalized);
        }

        private void DrawTicks(Graphics graphics, Rectangle trackBounds)
        {
            int visibleTicks = Math.Max(2, MajorTickCount);
            int totalIntervals = visibleTicks - 1;
            int tickInset = trackBounds.Width <= 10 ? 5 : 9;
            int majorTickLength = trackBounds.Width <= 10 ? 4 : 6;
            int minorTickLength = trackBounds.Width <= 10 ? 2 : 4;
            int leftX = trackBounds.Left - tickInset;

            using (Pen majorPen = new Pen(TickColor, 1f))
            using (Pen minorPen = new Pen(Color.FromArgb(120, TickColor), 1f))
            {
                for (int index = 0; index < visibleTicks; index++)
                {
                    double fraction = totalIntervals == 0 ? 0d : index / (double)totalIntervals;
                    int y = trackBounds.Bottom - (int)Math.Round(trackBounds.Height * fraction);
                    graphics.DrawLine(majorPen, leftX - majorTickLength, y, leftX, y);

                    if (index == visibleTicks - 1)
                        continue;

                    for (int minor = 1; minor <= 3; minor++)
                    {
                        double minorFraction = fraction + ((1d / totalIntervals) * (minor / 4d));
                        int minorY = trackBounds.Bottom - (int)Math.Round(trackBounds.Height * minorFraction);
                        graphics.DrawLine(minorPen, leftX - minorTickLength, minorY, leftX, minorY);
                    }
                }
            }
        }

        private void DrawTrack(Graphics graphics, Rectangle trackBounds)
        {
            if (trackBounds.Width <= 1 || trackBounds.Height <= 1)
                return;

            int outerRadius = trackBounds.Width <= 10 ? 6 : 12;
            using (GraphicsPath trackPath = CreateRoundedPath(trackBounds, outerRadius))
            using (LinearGradientBrush trackBrush = new LinearGradientBrush(
                trackBounds,
                ControlPaint.Light(TrackBackColor, 0.10f),
                ControlPaint.Dark(TrackBackColor, 0.18f),
                LinearGradientMode.Vertical))
            using (Pen borderPen = new Pen(BorderColor, 1.3f))
            using (Pen innerPen = new Pen(Color.FromArgb(70, Color.White), 1f))
            {
                graphics.FillPath(trackBrush, trackPath);
                graphics.DrawPath(borderPen, trackPath);

                Rectangle innerFrame = Rectangle.Inflate(trackBounds, -2, -2);
                if (innerFrame.Width > 4 && innerFrame.Height > 4)
                {
                    using (GraphicsPath innerFramePath = CreateRoundedPath(innerFrame, Math.Max(3, outerRadius - 2)))
                    {
                        graphics.DrawPath(innerPen, innerFramePath);
                    }
                }
            }

            Rectangle slotRect = GetSlotBounds(trackBounds);
            if (slotRect.Width <= 1 || slotRect.Height <= 1)
                return;

            int slotRadius = trackBounds.Width <= 10 ? 4 : 7;
            using (GraphicsPath slotPath = CreateRoundedPath(slotRect, slotRadius))
            using (SolidBrush slotBrush = new SolidBrush(SlotColor))
            using (Pen slotPen = new Pen(Color.FromArgb(52, Color.White), 1f))
            {
                graphics.FillPath(slotBrush, slotPath);
                graphics.DrawPath(slotPen, slotPath);
            }

            Rectangle fillRect = GetFillRectangle(slotRect);
            if (fillRect.Width <= 1 || fillRect.Height <= 0)
                return;

            using (GraphicsPath slotPath = CreateRoundedPath(slotRect, slotRadius))
            {
                GraphicsState state = graphics.Save();
                try
                {
                    graphics.SetClip(slotPath);

                    using (LinearGradientBrush fillBrush = new LinearGradientBrush(
                        fillRect,
                        ControlPaint.Light(FillColor, 0.18f),
                        ControlPaint.Dark(FillColor, 0.12f),
                        LinearGradientMode.Vertical))
                    {
                        graphics.FillRectangle(fillBrush, fillRect);
                    }

                    if (fillRect.Width >= 4)
                    {
                        Rectangle glossRect = new Rectangle(fillRect.Left + 1, fillRect.Top, Math.Max(2, fillRect.Width / 3), fillRect.Height);
                        using (SolidBrush glossBrush = new SolidBrush(Color.FromArgb(36, Color.White)))
                        {
                            graphics.FillRectangle(glossBrush, glossRect);
                        }
                    }
                }
                finally
                {
                    graphics.Restore(state);
                }
            }
        }

        private void DrawThumb(Graphics graphics, Rectangle trackBounds)
        {
            Rectangle slotRect = GetSlotBounds(trackBounds);
            if (slotRect.Width <= 1 || slotRect.Height <= 1)
                return;

            int centerY = GetThumbCenterY(slotRect);
            int sidePadding = trackBounds.Width <= 10 ? 3 : 8;
            int thumbHeight = trackBounds.Width <= 10 ? 12 : 16;
            int thumbWidth = Math.Max(8, Math.Min(Math.Max(8, ClientRectangle.Width - 4), trackBounds.Width + (sidePadding * 2)));
            Rectangle thumbRect = new Rectangle(
                Math.Max(2, trackBounds.Left - sidePadding),
                centerY - (thumbHeight / 2),
                thumbWidth,
                thumbHeight);
            if (thumbRect.Width <= 1 || thumbRect.Height <= 1)
                return;

            using (GraphicsPath thumbPath = CreateRoundedPath(thumbRect, Math.Max(4, thumbHeight / 2)))
            using (LinearGradientBrush thumbBrush = new LinearGradientBrush(
                thumbRect,
                ControlPaint.Light(ThumbColor, 0.15f),
                ControlPaint.Dark(ThumbColor, 0.18f),
                LinearGradientMode.Vertical))
            using (Pen borderPen = new Pen(Color.FromArgb(145, BorderColor), 1.1f))
            using (Pen gripPen = new Pen(Color.FromArgb(105, Color.White), 1f))
            {
                graphics.FillPath(thumbBrush, thumbPath);
                graphics.DrawPath(borderPen, thumbPath);

                int centerX = thumbRect.Left + (thumbRect.Width / 2);
                int gripTop = thumbRect.Top + 3;
                int gripBottom = thumbRect.Bottom - 3;
                if (thumbRect.Width >= 18)
                {
                    graphics.DrawLine(gripPen, centerX - 4, gripTop, centerX - 4, gripBottom);
                    graphics.DrawLine(gripPen, centerX, gripTop, centerX, gripBottom);
                    graphics.DrawLine(gripPen, centerX + 4, gripTop, centerX + 4, gripBottom);
                }
                else if (thumbRect.Width >= 10)
                {
                    graphics.DrawLine(gripPen, centerX - 2, gripTop, centerX - 2, gripBottom);
                    graphics.DrawLine(gripPen, centerX + 2, gripTop, centerX + 2, gripBottom);
                }
            }
        }

        private void DrawFocusCue(Graphics graphics, Rectangle frameBounds)
        {
            Rectangle focusBounds = Rectangle.Inflate(frameBounds, -1, -1);
            using (GraphicsPath focusPath = CreateRoundedPath(focusBounds, 12))
            using (Pen focusPen = new Pen(Color.FromArgb(148, 86, 169, 227), 1f))
            {
                focusPen.DashStyle = DashStyle.Dot;
                graphics.DrawPath(focusPen, focusPath);
            }
        }

        private Rectangle GetFillRectangle(Rectangle slotRect)
        {
            double normalized = GetNormalizedValue(Value);
            int fillHeight = (int)Math.Round(slotRect.Height * normalized);
            return new Rectangle(slotRect.Left, slotRect.Bottom - fillHeight, slotRect.Width, fillHeight);
        }

        private int GetThumbCenterY(Rectangle slotRect)
        {
            double normalized = GetNormalizedValue(Value);
            return slotRect.Bottom - (int)Math.Round(slotRect.Height * normalized);
        }

        private Rectangle GetTrackBounds(Rectangle frameBounds)
        {
            if (frameBounds.Width <= 2 || frameBounds.Height <= 10)
                return Rectangle.Empty;

            int horizontalMargin = frameBounds.Width <= 24 ? 4 : 10;
            int availableWidth = Math.Max(6, frameBounds.Width - horizontalMargin);
            int trackWidth = frameBounds.Width <= 24
                ? Math.Min(12, Math.Max(6, availableWidth / 2))
                : Math.Min(18, Math.Max(8, availableWidth / 2));
            trackWidth = Math.Min(trackWidth, Math.Max(2, frameBounds.Width));

            int x = frameBounds.Left + ((frameBounds.Width - trackWidth) / 2);
            int y = frameBounds.Top + 8;
            int height = Math.Max(20, frameBounds.Height - 16);

            if (y + height > frameBounds.Bottom)
                height = Math.Max(8, frameBounds.Bottom - y);

            return new Rectangle(x, y, trackWidth, height);
        }

        private Rectangle GetSlotBounds(Rectangle trackBounds)
        {
            if (trackBounds.Width <= 2 || trackBounds.Height <= 2)
                return Rectangle.Empty;

            // Mantem um canal interno valido mesmo quando o controle fica muito estreito.
            int horizontalInset = trackBounds.Width <= 10
                ? Math.Max(1, (trackBounds.Width - 4) / 2)
                : Math.Min(6, Math.Max(2, (trackBounds.Width - 4) / 2));
            int verticalInset = trackBounds.Height <= 40
                ? Math.Max(2, (trackBounds.Height - 8) / 4)
                : Math.Min(14, Math.Max(4, (trackBounds.Height - 12) / 2));

            Rectangle slotRect = Rectangle.Inflate(trackBounds, -horizontalInset, -verticalInset);
            if (slotRect.Width <= 1 || slotRect.Height <= 1)
                return Rectangle.Empty;

            return slotRect;
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
    }
}

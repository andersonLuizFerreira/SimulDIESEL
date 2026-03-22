using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SimulDIESEL.UI.Controls
{
    [DefaultProperty("IsOn")]
    [DefaultEvent(null)]
    [ToolboxItem(true)]
    public class SdLedIndicator : Control
    {
        private bool _isOn;
        private Color _onColor = Color.FromArgb(239, 73, 73);
        private Color _offColor = Color.FromArgb(74, 28, 30);
        private Color _borderColor = Color.FromArgb(32, 34, 37);
        private bool _showGloss = true;

        public SdLedIndicator()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.SupportsTransparentBackColor,
                true);

            DoubleBuffered = true;
            base.BackColor = Color.Transparent;
            Size = DefaultSize;
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        public bool IsOn
        {
            get { return _isOn; }
            set
            {
                if (_isOn == value)
                    return;

                _isOn = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color OnColor
        {
            get { return _onColor; }
            set
            {
                if (_onColor == value)
                    return;

                _onColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color OffColor
        {
            get { return _offColor; }
            set
            {
                if (_offColor == value)
                    return;

                _offColor = value;
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
        [DefaultValue(true)]
        public bool ShowGloss
        {
            get { return _showGloss; }
            set
            {
                if (_showGloss == value)
                    return;

                _showGloss = value;
                Invalidate();
            }
        }

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
            get { return new Size(18, 18); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle outerBounds = Rectangle.Inflate(ClientRectangle, -1, -1);
            if (outerBounds.Width <= 2 || outerBounds.Height <= 2)
                return;

            Rectangle ledBounds = Rectangle.Inflate(outerBounds, -2, -2);
            Color activeColor = IsOn ? OnColor : OffColor;

            using (GraphicsPath glowPath = new GraphicsPath())
            using (GraphicsPath ledPath = new GraphicsPath())
            {
                glowPath.AddEllipse(Rectangle.Inflate(ledBounds, 2, 2));
                ledPath.AddEllipse(ledBounds);

                if (IsOn)
                {
                    using (PathGradientBrush glowBrush = new PathGradientBrush(glowPath))
                    {
                        glowBrush.CenterColor = Color.FromArgb(110, activeColor);
                        glowBrush.SurroundColors = new[] { Color.FromArgb(0, activeColor) };
                        e.Graphics.FillPath(glowBrush, glowPath);
                    }
                }

                using (LinearGradientBrush fillBrush = new LinearGradientBrush(
                    ledBounds,
                    ControlPaint.Light(activeColor, 0.22f),
                    ControlPaint.Dark(activeColor, 0.18f),
                    LinearGradientMode.Vertical))
                using (Pen borderPen = new Pen(BorderColor, 1f))
                using (Pen innerPen = new Pen(Color.FromArgb(90, Color.White), 1f))
                {
                    e.Graphics.FillPath(fillBrush, ledPath);
                    e.Graphics.DrawPath(borderPen, ledPath);

                    Rectangle innerBounds = Rectangle.Inflate(ledBounds, -1, -1);
                    if (innerBounds.Width > 4 && innerBounds.Height > 4)
                    {
                        using (GraphicsPath innerPath = new GraphicsPath())
                        {
                            innerPath.AddEllipse(innerBounds);
                            e.Graphics.DrawPath(innerPen, innerPath);
                        }
                    }
                }
            }

            if (!ShowGloss)
                return;

            Rectangle glossBounds = new Rectangle(
                ledBounds.Left + 2,
                ledBounds.Top + 1,
                Math.Max(4, ledBounds.Width - 5),
                Math.Max(3, ledBounds.Height / 2));

            using (GraphicsPath glossPath = new GraphicsPath())
            {
                glossPath.AddEllipse(glossBounds);
                using (LinearGradientBrush glossBrush = new LinearGradientBrush(
                    glossBounds,
                    Color.FromArgb(IsOn ? 108 : 52, Color.White),
                    Color.FromArgb(0, Color.White),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillPath(glossBrush, glossPath);
                }
            }
        }
    }
}

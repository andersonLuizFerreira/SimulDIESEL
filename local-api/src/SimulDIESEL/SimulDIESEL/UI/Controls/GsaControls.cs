using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace SimulDIESEL.UI.Controls
{
    [ToolboxItem(true)]
    [DefaultProperty("TitleText")]
    public class GsaControls : Control
    {
        private const int ChannelsPerRow = 8;
        private const int TotalChannels = 16;

        private readonly ChannelInfo[] _channels = new ChannelInfo[TotalChannels];
        private readonly ChannelHitInfo[] _hitInfos = new ChannelHitInfo[TotalChannels];
        private readonly TextBox _setpointEditor;
        private Bitmap _cacheBitmap;
        private string _titleText = "GSA - Gerador de Sinais Analógicos";
        private string _subtitleText = "Painel fixo de 16 canais com renderização otimizada em um único componente.";
        private int _activeSliderIndex = -1;
        private int _editingChannelIndex = -1;
        private bool _isDraggingSlider;

        public GsaControls()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

            DoubleBuffered = true;
            BackColor = Color.FromArgb(27, 31, 35);
            ForeColor = Color.Gainsboro;
            Font = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
            Size = DefaultSize;
            TabStop = true;

            _setpointEditor = CreateSetpointEditor();
            Controls.Add(_setpointEditor);

            for (int i = 0; i < TotalChannels; i++)
            {
                _channels[i] = new ChannelInfo();
                _channels[i].ChannelTitle = "Canal " + (i + 1).ToString();
                _channels[i].VoltageRangeMax = i < 8 ? 5d : 12d;
                _channels[i].CurrentMax = 200d;
                _channels[i].CurrentUnitText = "mA";
                _hitInfos[i] = new ChannelHitInfo();
            }
        }

        [Category("Appearance")]
        [DefaultValue("GSA - Gerador de Sinais Analógicos")]
        public string TitleText
        {
            get { return _titleText; }
            set
            {
                string text = string.IsNullOrWhiteSpace(value) ? "GSA - Gerador de Sinais Analógicos" : value.Trim();
                if (string.Equals(_titleText, text, StringComparison.Ordinal))
                    return;

                _titleText = text;
                InvalidateCache();
            }
        }

        [Category("Appearance")]
        [DefaultValue("Painel fixo de 16 canais com renderização otimizada em um único componente.")]
        public string SubtitleText
        {
            get { return _subtitleText; }
            set
            {
                string text = value ?? string.Empty;
                if (string.Equals(_subtitleText, text, StringComparison.Ordinal))
                    return;

                _subtitleText = text;
                InvalidateCache();
            }
        }

        [Category("Behavior")]
        public event EventHandler<GsaChannelSetpointChangedEventArgs> SetpointVoltageChanged;

        [Category("Behavior")]
        public event EventHandler<GsaChannelOutputEnabledChangedEventArgs> OutputEnabledChanged;

        [Category("Behavior")]
        public event EventHandler<GsaChannelConfigClickEventArgs> ConfigButtonClick;

        protected override Size DefaultSize
        {
            get { return new Size(1500, 860); }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_cacheBitmap != null)
                {
                    _cacheBitmap.Dispose();
                    _cacheBitmap = null;
                }

                if (_setpointEditor != null)
                    _setpointEditor.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            EnsureCache();
            if (_cacheBitmap != null)
                e.Graphics.DrawImageUnscaled(_cacheBitmap, Point.Empty);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (_editingChannelIndex >= 0)
                PositionSetpointEditor(_editingChannelIndex);

            InvalidateCache();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            HandleMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            HandleMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            HandleMouseUp(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (!_isDraggingSlider)
                Cursor = Cursors.Default;
        }

        public void SetChannelState(int channelNumber, double setpointVoltage, double measuredVoltage, double measuredCurrent, bool outputEnabled, bool faultActive)
        {
            int index = channelNumber - 1;
            if (index < 0 || index >= TotalChannels)
                return;

            ChannelInfo channel = _channels[index];
            channel.SetpointVoltage = Clamp(setpointVoltage, 0d, channel.VoltageRangeMax);
            channel.MeasuredVoltage = Clamp(measuredVoltage, 0d, channel.VoltageRangeMax);
            channel.MeasuredCurrent = Clamp(measuredCurrent, 0d, channel.CurrentMax);
            channel.OutputEnabled = outputEnabled;
            channel.FaultActive = faultActive;
            InvalidateCache();
        }

        private void HandleMouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            Focus();

            int channelIndex = HitTestChannel(e.Location);
            if (channelIndex < 0)
            {
                CommitPendingSetpointEdit();
                return;
            }

            ChannelHitInfo hitInfo = _hitInfos[channelIndex];
            if (hitInfo.ConfigButtonBounds.Contains(e.Location))
            {
                CommitPendingSetpointEdit();
                OnConfigButtonClick(channelIndex);
                return;
            }

            if (hitInfo.CheckboxHitBounds.Contains(e.Location))
            {
                CommitPendingSetpointEdit();
                ToggleOutputEnabled(channelIndex);
                return;
            }

            if (hitInfo.SetpointBoxBounds.Contains(e.Location))
            {
                BeginSetpointEdit(channelIndex);
                return;
            }

            if (hitInfo.SliderHitBounds.Contains(e.Location))
            {
                CommitPendingSetpointEdit();
                BeginSliderDrag(channelIndex, e.Location);
            }
        }

        private void HandleMouseMove(MouseEventArgs e)
        {
            if (_isDraggingSlider && _activeSliderIndex >= 0)
            {
                UpdateSliderFromPoint(_activeSliderIndex, e.Location);
                return;
            }

            UpdateCursor(e.Location);
        }

        private void HandleMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _isDraggingSlider)
                EndSliderDrag();
        }

        private void EnsureCache()
        {
            if (ClientSize.Width <= 1 || ClientSize.Height <= 1)
                return;

            if (_cacheBitmap != null && _cacheBitmap.Size == ClientSize)
                return;

            if (_cacheBitmap != null)
            {
                _cacheBitmap.Dispose();
                _cacheBitmap = null;
            }

            _cacheBitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
            using (Graphics graphics = Graphics.FromImage(_cacheBitmap))
            {
                RenderBoard(graphics, ClientRectangle);
            }
        }

        private void InvalidateCache()
        {
            if (_cacheBitmap != null)
            {
                _cacheBitmap.Dispose();
                _cacheBitmap = null;
            }

            Invalidate();
        }
        private void RenderBoard(Graphics graphics, Rectangle bounds)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.Clear(BackColor);

            for (int i = 0; i < _hitInfos.Length; i++)
                _hitInfos[i].Reset();

            int horizontalPadding = Math.Max(14, bounds.Width / 110);
            int topPadding = Math.Max(18, bounds.Height / 40);
            int titleHeight = 34;
            int subtitleHeight = 22;
            int sectionHeight = 22;
            int rowGap = 18;

            Rectangle titleRect = new Rectangle(horizontalPadding, topPadding, bounds.Width - (horizontalPadding * 2), titleHeight);
            Rectangle subtitleRect = new Rectangle(horizontalPadding, titleRect.Bottom + 2, bounds.Width - (horizontalPadding * 2), subtitleHeight);
            Rectangle fiveVoltLabelRect = new Rectangle(horizontalPadding, subtitleRect.Bottom + 10, bounds.Width - (horizontalPadding * 2), sectionHeight);
            int rowTop = fiveVoltLabelRect.Bottom + 6;
            int rowHeight = Math.Max(180, (bounds.Height - rowTop - rowGap - sectionHeight - topPadding) / 2);
            Rectangle fiveVoltRowRect = new Rectangle(horizontalPadding, rowTop, bounds.Width - (horizontalPadding * 2), rowHeight);
            Rectangle twelveVoltLabelRect = new Rectangle(horizontalPadding, fiveVoltRowRect.Bottom + rowGap, bounds.Width - (horizontalPadding * 2), sectionHeight);
            Rectangle twelveVoltRowRect = new Rectangle(horizontalPadding, twelveVoltLabelRect.Bottom + 6, bounds.Width - (horizontalPadding * 2), rowHeight);

            DrawText(graphics, TitleText, new Font("Segoe UI Semibold", 16f, FontStyle.Bold), Color.FromArgb(236, 239, 241), titleRect, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            DrawText(graphics, SubtitleText, new Font("Segoe UI", 9.2f, FontStyle.Regular), Color.FromArgb(162, 171, 178), subtitleRect, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            DrawText(graphics, "Canais 0-5 V", new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold), Color.FromArgb(96, 215, 173), fiveVoltLabelRect, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            DrawText(graphics, "Canais 0-12 V", new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold), Color.FromArgb(106, 202, 224), twelveVoltLabelRect, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            DrawRow(graphics, fiveVoltRowRect, 0);
            DrawRow(graphics, twelveVoltRowRect, 8);
        }

        private void DrawRow(Graphics graphics, Rectangle rowRect, int startIndex)
        {
            float gap = Math.Max(8f, rowRect.Width / 220f);
            float cellWidth = (rowRect.Width - (gap * (ChannelsPerRow - 1))) / ChannelsPerRow;
            float aspectRatio = 0.62f;

            for (int i = 0; i < ChannelsPerRow; i++)
            {
                RectangleF cellRect = new RectangleF(rowRect.Left + (i * (cellWidth + gap)), rowRect.Top, cellWidth, rowRect.Height);
                float cardWidth = Math.Min(cellRect.Width, (rowRect.Height - 4f) * aspectRatio);
                float cardHeight = Math.Min(rowRect.Height - 4f, cardWidth / aspectRatio);
                RectangleF cardRect = new RectangleF(cellRect.Left + ((cellRect.Width - cardWidth) / 2f), cellRect.Top + ((cellRect.Height - cardHeight) / 2f), cardWidth, cardHeight);
                DrawCard(graphics, Rectangle.Round(cardRect), _channels[startIndex + i], startIndex + i);
            }
        }

        private void DrawCard(Graphics graphics, Rectangle cardRect, ChannelInfo channel, int channelIndex)
        {
            ChannelHitInfo hitInfo = _hitInfos[channelIndex];
            hitInfo.CardBounds = cardRect;

            int radius = Math.Max(10, cardRect.Width / 10);
            using (GraphicsPath cardPath = CreateRoundedPath(cardRect, radius))
            using (LinearGradientBrush bodyBrush = new LinearGradientBrush(cardRect, Color.FromArgb(43, 47, 53), Color.FromArgb(29, 33, 38), LinearGradientMode.Vertical))
            using (Pen borderPen = new Pen(Color.FromArgb(118, 125, 132), 1.3f))
            using (Pen innerPen = new Pen(Color.FromArgb(66, Color.White), 1f))
            {
                graphics.FillPath(bodyBrush, cardPath);
                graphics.DrawPath(borderPen, cardPath);

                Rectangle innerBounds = Rectangle.Inflate(cardRect, -1, -1);
                using (GraphicsPath innerPath = CreateRoundedPath(innerBounds, Math.Max(8, radius - 1)))
                {
                    graphics.DrawPath(innerPen, innerPath);
                }
            }

            int innerPadding = Math.Max(7, cardRect.Width / 18);
            int headerHeight = Math.Max(42, cardRect.Height / 5);
            int footerHeight = Math.Max(48, cardRect.Height / 5);
            Rectangle headerRect = new Rectangle(cardRect.Left + innerPadding, cardRect.Top + innerPadding, cardRect.Width - (innerPadding * 2), headerHeight - innerPadding);
            Rectangle bodyRect = new Rectangle(cardRect.Left + innerPadding, headerRect.Bottom + 4, cardRect.Width - (innerPadding * 2), cardRect.Height - headerHeight - footerHeight - (innerPadding * 2));
            Rectangle footerRect = new Rectangle(cardRect.Left + innerPadding, cardRect.Bottom - footerHeight, cardRect.Width - (innerPadding * 2), footerHeight - innerPadding);

            using (Pen separatorPen = new Pen(Color.FromArgb(52, Color.White), 1f))
            {
                graphics.DrawLine(separatorPen, bodyRect.Left, bodyRect.Top - 2, bodyRect.Right, bodyRect.Top - 2);
            }

            DrawHeader(graphics, headerRect, channel, hitInfo);
            DrawBody(graphics, bodyRect, channel, hitInfo);
            DrawFooter(graphics, footerRect, channel, hitInfo);
        }

        private void DrawHeader(Graphics graphics, Rectangle bounds, ChannelInfo channel, ChannelHitInfo hitInfo)
        {
            Rectangle titleRect = new Rectangle(bounds.Left, bounds.Top, bounds.Width - 24, 18);
            Rectangle gearRect = new Rectangle(bounds.Right - 18, bounds.Top + 1, 18, 18);
            Rectangle faultRect = new Rectangle(bounds.Left, bounds.Top + 20, bounds.Width, 16);
            hitInfo.ConfigButtonBounds = Rectangle.Inflate(gearRect, 3, 3);

            DrawText(graphics, channel.ChannelTitle, new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold), Color.FromArgb(237, 240, 243), titleRect, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            using (SolidBrush gearBrush = new SolidBrush(Color.FromArgb(46, 50, 56)))
            using (Pen gearPen = new Pen(Color.FromArgb(110, 118, 126), 1f))
            {
                graphics.FillRectangle(gearBrush, gearRect);
                graphics.DrawRectangle(gearPen, gearRect);
                DrawGearIcon(graphics, gearRect, Color.FromArgb(238, 240, 242));
            }

            DrawLed(graphics, new Rectangle(faultRect.Left + 2, faultRect.Top + 3, 10, 10), channel.FaultActive);
            DrawText(graphics, "Falha", new Font("Segoe UI", 7.2f, FontStyle.Regular), Color.FromArgb(236, 238, 240), new Rectangle(faultRect.Left + 18, faultRect.Top, 48, 16), TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }

        private void DrawBody(Graphics graphics, Rectangle bounds, ChannelInfo channel, ChannelHitInfo hitInfo)
        {
            int headerHeight = 14;
            int footerHeight = 20;
            int spacing = 6;
            int sliderWidth = Math.Max(22, bounds.Width / 4);
            int gaugeWidth = (bounds.Width - sliderWidth - (spacing * 2)) / 2;

            Rectangle setpointColumn = new Rectangle(bounds.Left, bounds.Top, sliderWidth, bounds.Height);
            Rectangle voltageColumn = new Rectangle(setpointColumn.Right + spacing, bounds.Top, gaugeWidth, bounds.Height);
            Rectangle currentColumn = new Rectangle(voltageColumn.Right + spacing, bounds.Top, gaugeWidth, bounds.Height);

            DrawText(graphics, "AJUSTE", new Font("Segoe UI", 7f, FontStyle.Regular), Color.FromArgb(186, 194, 201), new Rectangle(setpointColumn.Left, setpointColumn.Top, setpointColumn.Width, headerHeight), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            DrawText(graphics, "SAIDA V", new Font("Segoe UI", 7f, FontStyle.Regular), Color.FromArgb(186, 194, 201), new Rectangle(voltageColumn.Left, voltageColumn.Top, voltageColumn.Width, headerHeight), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            DrawText(graphics, "SAIDA I", new Font("Segoe UI", 7f, FontStyle.Regular), Color.FromArgb(186, 194, 201), new Rectangle(currentColumn.Left, currentColumn.Top, currentColumn.Width, headerHeight), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            DrawSlider(graphics, new Rectangle(setpointColumn.Left, setpointColumn.Top + headerHeight, setpointColumn.Width, setpointColumn.Height - headerHeight - footerHeight), channel, hitInfo);
            DrawGauge(graphics, new Rectangle(voltageColumn.Left, voltageColumn.Top + headerHeight, voltageColumn.Width, voltageColumn.Height - headerHeight - footerHeight), channel.MeasuredVoltage, channel.VoltageRangeMax, false, Color.FromArgb(68, 198, 163));
            DrawGauge(graphics, new Rectangle(currentColumn.Left, currentColumn.Top + headerHeight, currentColumn.Width, currentColumn.Height - headerHeight - footerHeight), channel.MeasuredCurrent, channel.CurrentMax, true, Color.FromArgb(58, 173, 141));

            DrawText(graphics, channel.VoltageRangeMax <= 5.01d ? "0-5 V" : "0-12 V", new Font("Segoe UI", 7f, FontStyle.Regular), Color.FromArgb(172, 181, 188), new Rectangle(setpointColumn.Left, bounds.Bottom - footerHeight, setpointColumn.Width, footerHeight), TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
            DrawText(graphics, channel.MeasuredVoltage.ToString("0.00") + " V", new Font("Segoe UI Semibold", 7.3f, FontStyle.Bold), Color.FromArgb(240, 243, 245), new Rectangle(voltageColumn.Left, bounds.Bottom - footerHeight, voltageColumn.Width, footerHeight), TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
            DrawText(graphics, channel.MeasuredCurrent.ToString("0.00") + " " + channel.CurrentUnitText, new Font("Segoe UI Semibold", 7.3f, FontStyle.Bold), Color.FromArgb(240, 243, 245), new Rectangle(currentColumn.Left, bounds.Bottom - footerHeight, currentColumn.Width, footerHeight), TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
        }

        private void DrawFooter(Graphics graphics, Rectangle bounds, ChannelInfo channel, ChannelHitInfo hitInfo)
        {
            using (SolidBrush footerBrush = new SolidBrush(Color.FromArgb(33, 37, 41)))
            {
                graphics.FillRectangle(footerBrush, bounds);
            }

            Rectangle setpointLabelRect = new Rectangle(bounds.Left + 2, bounds.Top + 5, (int)(bounds.Width * 0.30f), 14);
            Rectangle setpointBoxRect = new Rectangle(setpointLabelRect.Right + 2, bounds.Top + 3, (int)(bounds.Width * 0.56f), 18);
            Rectangle unitRect = new Rectangle(setpointBoxRect.Right + 4, bounds.Top + 4, 14, 14);
            Rectangle checkboxRect = new Rectangle(bounds.Left + 2, bounds.Bottom - 15, 10, 10);
            Rectangle enabledRect = new Rectangle(checkboxRect.Right + 4, checkboxRect.Top - 3, bounds.Width - 24, 16);
            hitInfo.SetpointBoxBounds = setpointBoxRect;
            hitInfo.CheckboxHitBounds = Rectangle.Union(checkboxRect, enabledRect);

            DrawText(graphics, "Setpoint", new Font("Segoe UI", 7.1f, FontStyle.Regular), Color.FromArgb(237, 240, 243), setpointLabelRect, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            using (SolidBrush boxBrush = new SolidBrush(Color.FromArgb(23, 26, 29)))
            using (Pen boxPen = new Pen(Color.FromArgb(110, 118, 126), 1f))
            {
                graphics.FillRectangle(boxBrush, setpointBoxRect);
                graphics.DrawRectangle(boxPen, setpointBoxRect);
            }

            DrawText(graphics, channel.SetpointVoltage.ToString("0.00"), new Font("Segoe UI", 7.0f, FontStyle.Regular), Color.FromArgb(244, 246, 247), setpointBoxRect, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            DrawText(graphics, "V", new Font("Segoe UI", 7.0f, FontStyle.Regular), Color.FromArgb(237, 240, 243), unitRect, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            Color checkboxFillColor = channel.OutputEnabled ? Color.FromArgb(52, 134, 221) : Color.White;
            Color checkboxBorderColor = channel.OutputEnabled ? Color.FromArgb(121, 189, 255) : Color.FromArgb(218, 222, 225);
            Color checkboxTextColor = channel.OutputEnabled ? Color.FromArgb(174, 219, 255) : Color.FromArgb(237, 240, 243);
            using (SolidBrush checkBrush = new SolidBrush(checkboxFillColor))
            using (Pen checkPen = new Pen(checkboxBorderColor, 1f))
            {
                graphics.FillRectangle(checkBrush, checkboxRect);
                graphics.DrawRectangle(checkPen, checkboxRect);
            }

            if (channel.OutputEnabled)
            {
                using (Pen tickPen = new Pen(Color.White, 1.6f))
                {
                    graphics.DrawLines(tickPen, new[]
                    {
                        new Point(checkboxRect.Left + 2, checkboxRect.Top + 5),
                        new Point(checkboxRect.Left + 4, checkboxRect.Bottom - 2),
                        new Point(checkboxRect.Right - 2, checkboxRect.Top + 2)
                    });
                }
            }

            DrawText(graphics, "Habilitado", new Font("Segoe UI", 7.0f, FontStyle.Regular), checkboxTextColor, enabledRect, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private void DrawSlider(Graphics graphics, Rectangle bounds, ChannelInfo channel, ChannelHitInfo hitInfo)
        {
            Rectangle scaleRect = new Rectangle(bounds.Left, bounds.Top + 6, Math.Max(8, bounds.Width / 3), Math.Max(20, bounds.Height - 12));
            Rectangle trackRect = new Rectangle(bounds.Left + scaleRect.Width + 2, bounds.Top + 8, Math.Max(10, bounds.Width / 3), Math.Max(20, bounds.Height - 16));
            hitInfo.SliderTrackBounds = trackRect;
            hitInfo.SliderHitBounds = Rectangle.Inflate(Rectangle.Union(scaleRect, trackRect), 4, 4);

            using (Pen tickPen = new Pen(Color.FromArgb(120, 154, 164, 172), 1f))
            {
                for (int tick = 0; tick < 16; tick++)
                {
                    float fraction = tick / 15f;
                    float y = trackRect.Bottom - (fraction * trackRect.Height);
                    int tickLength = tick % 3 == 0 ? 6 : 4;
                    graphics.DrawLine(tickPen, scaleRect.Right - tickLength, y, scaleRect.Right, y);
                }
            }

            DrawVerticalTrack(graphics, trackRect, Color.FromArgb(59, 64, 70), false, 0d, 0d, Color.Empty, Color.FromArgb(72, 150, 214), channel.SetpointVoltage / Math.Max(0.1d, channel.VoltageRangeMax), true);
        }
        private TextBox CreateSetpointEditor()
        {
            TextBox editor = new TextBox();
            editor.Visible = false;
            editor.BorderStyle = BorderStyle.FixedSingle;
            editor.BackColor = Color.FromArgb(23, 26, 29);
            editor.ForeColor = Color.FromArgb(244, 246, 247);
            editor.Font = new Font("Segoe UI", 7.1f, FontStyle.Regular, GraphicsUnit.Point, 0);
            editor.TextAlign = HorizontalAlignment.Center;
            editor.TabStop = false;
            editor.Leave += SetpointEditor_Leave;
            editor.KeyDown += SetpointEditor_KeyDown;
            return editor;
        }

        private void BeginSliderDrag(int channelIndex, Point location)
        {
            _activeSliderIndex = channelIndex;
            _isDraggingSlider = true;
            Capture = true;
            Cursor = Cursors.SizeNS;
            UpdateSliderFromPoint(channelIndex, location);
        }

        private void UpdateSliderFromPoint(int channelIndex, Point location)
        {
            Rectangle trackRect = _hitInfos[channelIndex].SliderTrackBounds;
            if (trackRect.Height <= 0)
                return;

            int clampedY = Math.Max(trackRect.Top, Math.Min(trackRect.Bottom, location.Y));
            double fraction = (trackRect.Bottom - clampedY) / (double)Math.Max(1, trackRect.Height);
            SetSetpointVoltage(channelIndex, _channels[channelIndex].VoltageRangeMax * fraction, true);
        }

        private void EndSliderDrag()
        {
            _isDraggingSlider = false;
            _activeSliderIndex = -1;
            Capture = false;
            Cursor = Cursors.Default;
        }

        private void ToggleOutputEnabled(int channelIndex)
        {
            _channels[channelIndex].OutputEnabled = !_channels[channelIndex].OutputEnabled;
            InvalidateCache();
            OnOutputEnabledChanged(channelIndex, _channels[channelIndex].OutputEnabled);
        }

        private void BeginSetpointEdit(int channelIndex)
        {
            CommitPendingSetpointEdit();
            _editingChannelIndex = channelIndex;
            PositionSetpointEditor(channelIndex);
            _setpointEditor.Text = _channels[channelIndex].SetpointVoltage.ToString("0.00");
            _setpointEditor.Visible = true;
            _setpointEditor.BringToFront();
            _setpointEditor.Focus();
            _setpointEditor.SelectAll();
        }

        private void PositionSetpointEditor(int channelIndex)
        {
            Rectangle bounds = _hitInfos[channelIndex].SetpointBoxBounds;
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            _setpointEditor.Bounds = Rectangle.Inflate(bounds, -1, -1);
        }

        private void CommitPendingSetpointEdit()
        {
            if (_editingChannelIndex < 0)
                return;

            double parsedValue;
            if (TryParseSetpoint(_setpointEditor.Text, out parsedValue))
                SetSetpointVoltage(_editingChannelIndex, parsedValue, true);

            _editingChannelIndex = -1;
            _setpointEditor.Visible = false;
            Focus();
        }

        private void CancelPendingSetpointEdit()
        {
            _editingChannelIndex = -1;
            _setpointEditor.Visible = false;
            Focus();
            InvalidateCache();
        }

        private void SetpointEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CommitPendingSetpointEdit();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                CancelPendingSetpointEdit();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void SetpointEditor_Leave(object sender, EventArgs e)
        {
            CommitPendingSetpointEdit();
        }

        private bool TryParseSetpoint(string text, out double value)
        {
            string parsedText = (text ?? string.Empty).Trim();
            if (double.TryParse(parsedText, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                return true;

            parsedText = parsedText.Replace(',', '.');
            return double.TryParse(parsedText, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private void SetSetpointVoltage(int channelIndex, double value, bool raiseEvent)
        {
            double clampedValue = Clamp(value, 0d, _channels[channelIndex].VoltageRangeMax);
            if (Math.Abs(_channels[channelIndex].SetpointVoltage - clampedValue) < 0.0001d)
                return;

            _channels[channelIndex].SetpointVoltage = clampedValue;
            if (_editingChannelIndex == channelIndex)
                _setpointEditor.Text = clampedValue.ToString("0.00");

            InvalidateCache();

            if (raiseEvent)
                OnSetpointVoltageChanged(channelIndex, clampedValue);
        }

        private void UpdateCursor(Point location)
        {
            int channelIndex = HitTestChannel(location);
            if (channelIndex < 0)
            {
                Cursor = Cursors.Default;
                return;
            }

            ChannelHitInfo hitInfo = _hitInfos[channelIndex];
            if (hitInfo.SliderHitBounds.Contains(location))
            {
                Cursor = Cursors.SizeNS;
                return;
            }

            if (hitInfo.SetpointBoxBounds.Contains(location))
            {
                Cursor = Cursors.IBeam;
                return;
            }

            if (hitInfo.CheckboxHitBounds.Contains(location) || hitInfo.ConfigButtonBounds.Contains(location))
            {
                Cursor = Cursors.Hand;
                return;
            }

            Cursor = Cursors.Default;
        }

        private int HitTestChannel(Point location)
        {
            for (int i = 0; i < _hitInfos.Length; i++)
            {
                if (_hitInfos[i].CardBounds.Contains(location))
                    return i;
            }

            return -1;
        }

        private void OnSetpointVoltageChanged(int channelIndex, double value)
        {
            EventHandler<GsaChannelSetpointChangedEventArgs> handler = SetpointVoltageChanged;
            if (handler != null)
                handler(this, new GsaChannelSetpointChangedEventArgs(channelIndex + 1, value));
        }

        private void OnOutputEnabledChanged(int channelIndex, bool enabled)
        {
            EventHandler<GsaChannelOutputEnabledChangedEventArgs> handler = OutputEnabledChanged;
            if (handler != null)
                handler(this, new GsaChannelOutputEnabledChangedEventArgs(channelIndex + 1, enabled));
        }

        private void OnConfigButtonClick(int channelIndex)
        {
            EventHandler<GsaChannelConfigClickEventArgs> handler = ConfigButtonClick;
            if (handler != null)
                handler(this, new GsaChannelConfigClickEventArgs(channelIndex + 1));
        }
        private void DrawGauge(Graphics graphics, Rectangle bounds, double value, double maximum, bool drawDangerBand, Color fillColor)
        {
            int scaleWidth = Math.Max(16, (int)Math.Round(bounds.Width * 0.34));
            Rectangle scaleRect = new Rectangle(bounds.Left, bounds.Top, scaleWidth, bounds.Height);
            Rectangle gaugeRect = new Rectangle(bounds.Left + scaleWidth, bounds.Top + 2, Math.Max(18, bounds.Width - scaleWidth), Math.Max(20, bounds.Height - 4));

            using (Font scaleFont = new Font("Segoe UI", 6.7f, FontStyle.Regular))
            {
                for (int tick = 0; tick < 5; tick++)
                {
                    float fraction = tick / 4f;
                    float y = gaugeRect.Bottom - (fraction * gaugeRect.Height);
                    double scaleValue = maximum * fraction;
                    string scaleText = maximum > 20d ? scaleValue.ToString("0") : scaleValue.ToString("0.0");
                    DrawText(graphics, scaleText, scaleFont, Color.FromArgb(230, 234, 237), new Rectangle(scaleRect.Left, (int)Math.Round(y - 8), scaleRect.Width - 2, 16), TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
            }

            DrawVerticalTrack(graphics, gaugeRect, Color.FromArgb(73, 78, 84), drawDangerBand, 0.75d, 0.90d, Color.FromArgb(204, 72, 72), fillColor, value / Math.Max(0.1d, maximum), false);
        }

        private void DrawVerticalTrack(Graphics graphics, Rectangle outerBounds, Color frameColor, bool drawDangerBand, double warningFraction, double dangerFraction, Color dangerColor, Color fillColor, double normalizedValue, bool drawThumb)
        {
            if (outerBounds.Width <= 2 || outerBounds.Height <= 2)
                return;

            using (GraphicsPath outerPath = CreateRoundedPath(outerBounds, Math.Max(8, outerBounds.Width / 4)))
            using (LinearGradientBrush outerBrush = new LinearGradientBrush(outerBounds, ControlPaint.Light(frameColor, 0.18f), ControlPaint.Dark(frameColor, 0.14f), LinearGradientMode.Vertical))
            using (Pen outerPen = new Pen(Color.FromArgb(132, 138, 145), 1.2f))
            {
                graphics.FillPath(outerBrush, outerPath);
                graphics.DrawPath(outerPen, outerPath);
            }

            Rectangle channelRect = Rectangle.Inflate(outerBounds, -6, -8);
            using (GraphicsPath channelPath = CreateRoundedPath(channelRect, Math.Max(6, channelRect.Width / 4)))
            using (SolidBrush channelBrush = new SolidBrush(Color.FromArgb(20, 23, 26)))
            using (Pen channelPen = new Pen(Color.FromArgb(62, Color.White), 1f))
            {
                graphics.FillPath(channelBrush, channelPath);
                graphics.DrawPath(channelPen, channelPath);

                GraphicsState state = graphics.Save();
                try
                {
                    graphics.SetClip(channelPath);

                    if (drawDangerBand)
                    {
                        int warningY = channelRect.Top + (int)Math.Round(channelRect.Height * (1d - warningFraction));
                        int dangerY = channelRect.Top + (int)Math.Round(channelRect.Height * (1d - dangerFraction));

                        using (SolidBrush warningBrush = new SolidBrush(Color.FromArgb(44, 220, 171, 46)))
                        using (SolidBrush dangerBrush = new SolidBrush(Color.FromArgb(62, dangerColor)))
                        {
                            graphics.FillRectangle(warningBrush, new Rectangle(channelRect.Left, warningY, channelRect.Width, Math.Max(2, dangerY - warningY)));
                            graphics.FillRectangle(dangerBrush, new Rectangle(channelRect.Left, channelRect.Top, channelRect.Width, Math.Max(2, dangerY - channelRect.Top)));
                        }
                    }

                    int fillHeight = (int)Math.Round(channelRect.Height * Clamp(normalizedValue, 0d, 1d));
                    Rectangle fillRect = new Rectangle(channelRect.Left, channelRect.Bottom - fillHeight, channelRect.Width, fillHeight);
                    if (!drawThumb && fillRect.Height > 0)
                    {
                        using (LinearGradientBrush fillBrush = new LinearGradientBrush(fillRect, ControlPaint.Light(fillColor, 0.12f), ControlPaint.Dark(fillColor, 0.12f), LinearGradientMode.Vertical))
                        using (SolidBrush glossBrush = new SolidBrush(Color.FromArgb(34, Color.White)))
                        {
                            graphics.FillRectangle(fillBrush, fillRect);
                            graphics.FillRectangle(glossBrush, new Rectangle(fillRect.Left + 1, fillRect.Top, Math.Max(2, fillRect.Width / 4), fillRect.Height));
                        }
                    }
                }
                finally
                {
                    graphics.Restore(state);
                }
            }

            using (Pen tickPen = new Pen(Color.FromArgb(118, 160, 170, 176), 1f))
            {
                for (int tick = 0; tick < 16; tick++)
                {
                    float fraction = tick / 15f;
                    float y = channelRect.Bottom - (fraction * channelRect.Height);
                    graphics.DrawLine(tickPen, outerBounds.Right - 5, y, outerBounds.Right - 2, y);
                }
            }

            if (!drawThumb)
                return;

            int thumbY = channelRect.Bottom - (int)Math.Round(channelRect.Height * Clamp(normalizedValue, 0d, 1d));
            Rectangle thumbRect = new Rectangle(Math.Max(outerBounds.Left - 6, 1), thumbY - 7, Math.Min(outerBounds.Width + 12, 26), 14);
            using (GraphicsPath thumbPath = CreateRoundedPath(thumbRect, 7))
            using (LinearGradientBrush thumbBrush = new LinearGradientBrush(thumbRect, Color.FromArgb(100, 183, 241), Color.FromArgb(53, 122, 184), LinearGradientMode.Vertical))
            using (Pen thumbPen = new Pen(Color.FromArgb(79, 107, 136), 1f))
            {
                graphics.FillPath(thumbBrush, thumbPath);
                graphics.DrawPath(thumbPen, thumbPath);
            }
        }

        private static void DrawLed(Graphics graphics, Rectangle bounds, bool active)
        {
            Color color = active ? Color.FromArgb(239, 73, 73) : Color.FromArgb(74, 28, 30);
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(bounds);
                using (LinearGradientBrush brush = new LinearGradientBrush(bounds, ControlPaint.Light(color, 0.18f), ControlPaint.Dark(color, 0.18f), LinearGradientMode.Vertical))
                using (Pen borderPen = new Pen(Color.FromArgb(34, 36, 38), 1f))
                {
                    graphics.FillPath(brush, path);
                    graphics.DrawPath(borderPen, path);
                }
            }
        }

        private static void DrawGearIcon(Graphics graphics, Rectangle bounds, Color color)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle innerBounds = Rectangle.Inflate(bounds, -4, -4);
            int centerX = innerBounds.Left + (innerBounds.Width / 2);
            int centerY = innerBounds.Top + (innerBounds.Height / 2);
            int outerRadius = Math.Max(4, Math.Min(innerBounds.Width, innerBounds.Height) / 2);
            int innerRadius = Math.Max(2, outerRadius / 2);

            using (Pen pen = new Pen(color, 1.3f))
            using (SolidBrush hubBrush = new SolidBrush(Color.FromArgb(30, color)))
            {
                for (int tooth = 0; tooth < 8; tooth++)
                {
                    double angle = (Math.PI / 4d) * tooth;
                    int x1 = centerX + (int)Math.Round(Math.Cos(angle) * (innerRadius + 1));
                    int y1 = centerY + (int)Math.Round(Math.Sin(angle) * (innerRadius + 1));
                    int x2 = centerX + (int)Math.Round(Math.Cos(angle) * (outerRadius + 1));
                    int y2 = centerY + (int)Math.Round(Math.Sin(angle) * (outerRadius + 1));
                    graphics.DrawLine(pen, x1, y1, x2, y2);
                }

                Rectangle outerCircle = new Rectangle(centerX - innerRadius - 1, centerY - innerRadius - 1, (innerRadius + 1) * 2, (innerRadius + 1) * 2);
                Rectangle hubCircle = new Rectangle(centerX - 1, centerY - 1, 3, 3);
                graphics.DrawEllipse(pen, outerCircle);
                graphics.FillEllipse(hubBrush, hubCircle);
            }
        }

        private static void DrawText(Graphics graphics, string text, Font font, Color color, Rectangle bounds, TextFormatFlags flags)
        {
            TextRenderer.DrawText(graphics, text ?? string.Empty, font, bounds, color, flags | TextFormatFlags.NoPrefix);
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

        private static double Clamp(double value, double minimum, double maximum)
        {
            if (value < minimum)
                return minimum;
            if (value > maximum)
                return maximum;
            return value;
        }

        private sealed class ChannelInfo
        {
            public string ChannelTitle;
            public double VoltageRangeMax;
            public double SetpointVoltage;
            public double MeasuredVoltage;
            public double MeasuredCurrent;
            public double CurrentMax;
            public string CurrentUnitText;
            public bool OutputEnabled;
            public bool FaultActive;
        }

        private sealed class ChannelHitInfo
        {
            public Rectangle CardBounds;
            public Rectangle ConfigButtonBounds;
            public Rectangle SliderTrackBounds;
            public Rectangle SliderHitBounds;
            public Rectangle SetpointBoxBounds;
            public Rectangle CheckboxHitBounds;

            public void Reset()
            {
                CardBounds = Rectangle.Empty;
                ConfigButtonBounds = Rectangle.Empty;
                SliderTrackBounds = Rectangle.Empty;
                SliderHitBounds = Rectangle.Empty;
                SetpointBoxBounds = Rectangle.Empty;
                CheckboxHitBounds = Rectangle.Empty;
            }
        }

        public sealed class GsaChannelSetpointChangedEventArgs : EventArgs
        {
            public GsaChannelSetpointChangedEventArgs(int channelNumber, double setpointVoltage)
            {
                ChannelNumber = channelNumber;
                SetpointVoltage = setpointVoltage;
            }

            public int ChannelNumber { get; private set; }
            public double SetpointVoltage { get; private set; }
        }

        public sealed class GsaChannelOutputEnabledChangedEventArgs : EventArgs
        {
            public GsaChannelOutputEnabledChangedEventArgs(int channelNumber, bool outputEnabled)
            {
                ChannelNumber = channelNumber;
                OutputEnabled = outputEnabled;
            }

            public int ChannelNumber { get; private set; }
            public bool OutputEnabled { get; private set; }
        }

        public sealed class GsaChannelConfigClickEventArgs : EventArgs
        {
            public GsaChannelConfigClickEventArgs(int channelNumber)
            {
                ChannelNumber = channelNumber;
            }

            public int ChannelNumber { get; private set; }
        }
    }
}

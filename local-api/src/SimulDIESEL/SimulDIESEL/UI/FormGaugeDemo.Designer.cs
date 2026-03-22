using SimulDIESEL.UI.Controls;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SimulDIESEL.UI
{
    partial class FormGaugeDemo
    {
        private IContainer components = null;
        private Timer _animationTimer;
        private Label _titleLabel;
        private Label _subtitleLabel;
        private FlowLayoutPanel _gaugesPanel;
        private SdVerticalGauge _batteryGauge;
        private SdVerticalGauge _pwmGauge;
        private SdVerticalGauge _temperatureGauge;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this._animationTimer = new Timer(this.components);
            this._titleLabel = new Label();
            this._subtitleLabel = new Label();
            this._gaugesPanel = new FlowLayoutPanel();
            this._batteryGauge = new SdVerticalGauge();
            this._pwmGauge = new SdVerticalGauge();
            this._temperatureGauge = new SdVerticalGauge();
            this._gaugesPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _titleLabel
            // 
            this._titleLabel.AutoSize = false;
            this._titleLabel.Dock = DockStyle.Top;
            this._titleLabel.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            this._titleLabel.ForeColor = Color.FromArgb(228, 231, 234);
            this._titleLabel.Height = 34;
            this._titleLabel.Name = "_titleLabel";
            this._titleLabel.Text = "Painel de Gauges Verticais";
            this._titleLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _subtitleLabel
            // 
            this._subtitleLabel.AutoSize = false;
            this._subtitleLabel.Dock = DockStyle.Top;
            this._subtitleLabel.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point, 0);
            this._subtitleLabel.ForeColor = Color.FromArgb(170, 176, 183);
            this._subtitleLabel.Height = 28;
            this._subtitleLabel.Name = "_subtitleLabel";
            this._subtitleLabel.Text = "Simulação de telemetria industrial para bancada e diagnóstico.";
            this._subtitleLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _gaugesPanel
            // 
            this._gaugesPanel.BackColor = Color.FromArgb(30, 34, 38);
            this._gaugesPanel.Controls.Add(this._batteryGauge);
            this._gaugesPanel.Controls.Add(this._pwmGauge);
            this._gaugesPanel.Controls.Add(this._temperatureGauge);
            this._gaugesPanel.Dock = DockStyle.Fill;
            this._gaugesPanel.FlowDirection = FlowDirection.LeftToRight;
            this._gaugesPanel.Location = new Point(16, 78);
            this._gaugesPanel.Name = "_gaugesPanel";
            this._gaugesPanel.Padding = new Padding(18, 18, 18, 12);
            this._gaugesPanel.Size = new Size(400, 304);
            this._gaugesPanel.TabIndex = 0;
            this._gaugesPanel.WrapContents = false;
            // 
            // _batteryGauge
            // 
            ConfigureGaugeBase(this._batteryGauge);
            this._batteryGauge.Name = "_batteryGauge";
            // 
            // _pwmGauge
            // 
            ConfigureGaugeBase(this._pwmGauge);
            this._pwmGauge.Name = "_pwmGauge";
            // 
            // _temperatureGauge
            // 
            ConfigureGaugeBase(this._temperatureGauge);
            this._temperatureGauge.Name = "_temperatureGauge";
            // 
            // FormGaugeDemo
            // 
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(24, 27, 31);
            this.ClientSize = new Size(432, 398);
            this.Controls.Add(this._gaugesPanel);
            this.Controls.Add(this._subtitleLabel);
            this.Controls.Add(this._titleLabel);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.ForeColor = Color.Gainsboro;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new Size(410, 360);
            this.Name = "FormGaugeDemo";
            this.Padding = new Padding(16);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Demo - SdVerticalGauge";
            this._gaugesPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private void ConfigureGaugeBase(SdVerticalGauge gauge)
        {
            // Mantem a base visual consistente para o painel de demonstracao.
            gauge.BorderColor = Color.FromArgb(128, 134, 140);
            gauge.BorderThickness = 2;
            gauge.ChannelColor = Color.FromArgb(20, 23, 26);
            gauge.CornerRadius = 12;
            gauge.GaugeBackColor = Color.FromArgb(46, 50, 55);
            gauge.Margin = new Padding(8, 0, 8, 0);
            gauge.MinorTicksPerMajor = 3;
            gauge.ShowGlassEffect = true;
            gauge.ShowScaleLabels = true;
            gauge.ShowTicks = true;
            gauge.ShowTitle = true;
            gauge.Size = new Size(110, 300);
            gauge.TabIndex = 0;
            gauge.TextColor = Color.Gainsboro;
        }
    }
}

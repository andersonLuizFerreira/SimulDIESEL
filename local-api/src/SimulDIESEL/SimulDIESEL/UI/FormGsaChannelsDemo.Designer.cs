using SimulDIESEL.UI.Controls;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SimulDIESEL.UI
{
    partial class FormGsaChannelsDemo
    {
        private IContainer components = null;
        private Label _titleLabel;
        private Label _subtitleLabel;
        private TableLayoutPanel _channelsLayout;
        private GsaChannelControl _channel1Control;
        private GsaChannelControl _channel2Control;
        private GsaChannelControl _channel3Control;
        private GsaChannelControl _channel4Control;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this._titleLabel = new Label();
            this._subtitleLabel = new Label();
            this._channelsLayout = new TableLayoutPanel();
            this._channel1Control = new GsaChannelControl();
            this._channel2Control = new GsaChannelControl();
            this._channel3Control = new GsaChannelControl();
            this._channel4Control = new GsaChannelControl();
            this._channelsLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // _titleLabel
            // 
            this._titleLabel.Dock = DockStyle.Top;
            this._titleLabel.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            this._titleLabel.ForeColor = Color.FromArgb(232, 236, 239);
            this._titleLabel.Location = new Point(18, 18);
            this._titleLabel.Margin = new Padding(0);
            this._titleLabel.Name = "_titleLabel";
            this._titleLabel.Size = new Size(1100, 34);
            this._titleLabel.TabIndex = 0;
            this._titleLabel.Text = "Dashboard de Canais GSA";
            this._titleLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _subtitleLabel
            // 
            this._subtitleLabel.Dock = DockStyle.Top;
            this._subtitleLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this._subtitleLabel.ForeColor = Color.FromArgb(167, 175, 182);
            this._subtitleLabel.Location = new Point(18, 52);
            this._subtitleLabel.Margin = new Padding(0, 0, 0, 10);
            this._subtitleLabel.Name = "_subtitleLabel";
            this._subtitleLabel.Size = new Size(1100, 28);
            this._subtitleLabel.TabIndex = 1;
            this._subtitleLabel.Text = "Exemplo de composição reutilizável para canais 0-5 V e 0-12 V do Gerador de Sinais Analógicos.";
            this._subtitleLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _channelsLayout
            // 
            this._channelsLayout.ColumnCount = 2;
            this._channelsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this._channelsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this._channelsLayout.Controls.Add(this._channel1Control, 0, 0);
            this._channelsLayout.Controls.Add(this._channel2Control, 1, 0);
            this._channelsLayout.Controls.Add(this._channel3Control, 0, 1);
            this._channelsLayout.Controls.Add(this._channel4Control, 1, 1);
            this._channelsLayout.Dock = DockStyle.Fill;
            this._channelsLayout.Location = new Point(18, 80);
            this._channelsLayout.Margin = new Padding(0);
            this._channelsLayout.Name = "_channelsLayout";
            this._channelsLayout.Padding = new Padding(4);
            this._channelsLayout.RowCount = 2;
            this._channelsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            this._channelsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            this._channelsLayout.Size = new Size(1100, 720);
            this._channelsLayout.TabIndex = 2;
            // 
            // _channel1Control
            // 
            this._channel1Control.ChannelTitle = "Canal 1";
            this._channel1Control.Dock = DockStyle.Fill;
            this._channel1Control.Location = new Point(16, 16);
            this._channel1Control.Margin = new Padding(12);
            this._channel1Control.Name = "_channel1Control";
            this._channel1Control.Size = new Size(520, 340);
            this._channel1Control.TabIndex = 0;
            // 
            // _channel2Control
            // 
            this._channel2Control.ChannelTitle = "Canal 2";
            this._channel2Control.Dock = DockStyle.Fill;
            this._channel2Control.Location = new Point(562, 16);
            this._channel2Control.Margin = new Padding(12);
            this._channel2Control.Name = "_channel2Control";
            this._channel2Control.Size = new Size(522, 340);
            this._channel2Control.TabIndex = 1;
            // 
            // _channel3Control
            // 
            this._channel3Control.ChannelTitle = "Canal 3";
            this._channel3Control.Dock = DockStyle.Fill;
            this._channel3Control.Location = new Point(16, 376);
            this._channel3Control.Margin = new Padding(12);
            this._channel3Control.Name = "_channel3Control";
            this._channel3Control.Size = new Size(520, 340);
            this._channel3Control.TabIndex = 2;
            // 
            // _channel4Control
            // 
            this._channel4Control.ChannelTitle = "Canal 4";
            this._channel4Control.Dock = DockStyle.Fill;
            this._channel4Control.Location = new Point(562, 376);
            this._channel4Control.Margin = new Padding(12);
            this._channel4Control.Name = "_channel4Control";
            this._channel4Control.Size = new Size(522, 340);
            this._channel4Control.TabIndex = 3;
            // 
            // FormGsaChannelsDemo
            // 
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(22, 25, 29);
            this.ClientSize = new Size(1136, 818);
            this.Controls.Add(this._channelsLayout);
            this.Controls.Add(this._subtitleLabel);
            this.Controls.Add(this._titleLabel);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = Color.Gainsboro;
            this.MinimumSize = new Size(980, 720);
            this.Name = "FormGsaChannelsDemo";
            this.Padding = new Padding(18);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Demo - GsaChannelControl";
            this._channelsLayout.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}

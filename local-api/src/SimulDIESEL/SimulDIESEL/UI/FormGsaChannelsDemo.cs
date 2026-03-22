using System;
using System.Windows.Forms;
using SimulDIESEL.UI.Controls;

namespace SimulDIESEL.UI
{
    public partial class FormGsaChannelsDemo : Form
    {
        public FormGsaChannelsDemo()
        {
            InitializeComponent();
            ConfigureChannels();
        }

        private void ConfigureChannels()
        {
            ConfigureChannel(_channel1Control, "Canal 1", 5d, 2.35d, 2.28d, 0.42d, 2.00d, 1.35d, 1.70d, "A", true, false);
            ConfigureChannel(_channel2Control, "Canal 2", 5d, 4.70d, 4.15d, 1.82d, 2.00d, 1.35d, 1.70d, "A", false, true);
            ConfigureChannel(_channel3Control, "Canal 3", 12d, 7.80d, 7.62d, 0.68d, 2.50d, 1.80d, 2.15d, "A", true, false);
            ConfigureChannel(_channel4Control, "Canal 4", 12d, 10.90d, 10.72d, 0.24d, 2.50d, 1.80d, 2.15d, "A", false, false);

            _channel1Control.ConfigButtonClick += ChannelControl_ConfigButtonClick;
            _channel2Control.ConfigButtonClick += ChannelControl_ConfigButtonClick;
            _channel3Control.ConfigButtonClick += ChannelControl_ConfigButtonClick;
            _channel4Control.ConfigButtonClick += ChannelControl_ConfigButtonClick;
        }

        private static void ConfigureChannel(
            GsaChannelControl channel,
            string title,
            double voltageRange,
            double setpointVoltage,
            double measuredVoltage,
            double measuredCurrent,
            double currentMax,
            double warningThreshold,
            double dangerThreshold,
            string currentUnit,
            bool outputEnabled,
            bool faultActive)
        {
            // Mantem o demo pronto para arrastar e testar no Designer sem depender de backend.
            channel.ChannelTitle = title;
            channel.VoltageRangeMax = voltageRange;
            channel.SetpointVoltage = setpointVoltage;
            channel.MeasuredVoltage = measuredVoltage;
            channel.MeasuredCurrent = measuredCurrent;
            channel.CurrentMax = currentMax;
            channel.CurrentWarningThreshold = warningThreshold;
            channel.CurrentDangerThreshold = dangerThreshold;
            channel.CurrentUnitText = currentUnit;
            channel.OutputEnabled = outputEnabled;
            channel.FaultActive = faultActive;
        }

        private void ChannelControl_ConfigButtonClick(object sender, EventArgs e)
        {
            GsaChannelControl channel = sender as GsaChannelControl;
            if (channel == null)
                return;

            MessageBox.Show(
                string.Format("Abrir configuracoes de calibracao do {0}.", channel.ChannelTitle),
                "GSA",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}

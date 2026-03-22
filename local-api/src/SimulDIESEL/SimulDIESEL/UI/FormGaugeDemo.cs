using System;
using System.Drawing;
using System.Windows.Forms;

namespace SimulDIESEL.UI
{
    public partial class FormGaugeDemo : Form
    {
        private static FormGaugeDemo _instance;
        private int _animationStep;

        public FormGaugeDemo()
        {
            InitializeComponent();

            ConfigureBatteryGauge();
            ConfigurePwmGauge();
            ConfigureTemperatureGauge();

            _animationTimer.Interval = 85;
            _animationTimer.Tick += AnimationTimer_Tick;
        }

        public static FormGaugeDemo Instance
        {
            get
            {
                if (_instance == null || _instance.IsDisposed)
                    _instance = new FormGaugeDemo();

                return _instance;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _animationTimer.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _animationTimer.Stop();
            base.OnFormClosing(e);
        }

        private void ConfigureBatteryGauge()
        {
            _batteryGauge.TitleText = "Bateria";
            _batteryGauge.UnitText = "V";
            _batteryGauge.Minimum = 8;
            _batteryGauge.Maximum = 16;
            _batteryGauge.Value = 12;
            _batteryGauge.MajorTickCount = 5;
            _batteryGauge.WarningThreshold = 14;
            _batteryGauge.DangerThreshold = 15;
            _batteryGauge.FillColor = Color.FromArgb(66, 171, 120);
            _batteryGauge.WarningColor = Color.FromArgb(213, 163, 46);
            _batteryGauge.DangerColor = Color.FromArgb(198, 72, 72);
        }

        private void ConfigurePwmGauge()
        {
            _pwmGauge.TitleText = "PWM";
            _pwmGauge.UnitText = "%";
            _pwmGauge.Minimum = 0;
            _pwmGauge.Maximum = 100;
            _pwmGauge.Value = 0;
            _pwmGauge.MajorTickCount = 6;
            _pwmGauge.ShowPercentage = true;
            _pwmGauge.WarningThreshold = 70;
            _pwmGauge.DangerThreshold = 90;
            _pwmGauge.FillColor = Color.FromArgb(52, 168, 83);
            _pwmGauge.WarningColor = Color.FromArgb(219, 173, 42);
            _pwmGauge.DangerColor = Color.FromArgb(204, 68, 68);
        }

        private void ConfigureTemperatureGauge()
        {
            _temperatureGauge.TitleText = "Temperatura";
            _temperatureGauge.UnitText = "°C";
            _temperatureGauge.Minimum = 0;
            _temperatureGauge.Maximum = 120;
            _temperatureGauge.Value = 35;
            _temperatureGauge.MajorTickCount = 7;
            _temperatureGauge.WarningThreshold = 90;
            _temperatureGauge.DangerThreshold = 105;
            _temperatureGauge.FillColor = Color.FromArgb(72, 159, 214);
            _temperatureGauge.WarningColor = Color.FromArgb(226, 170, 44);
            _temperatureGauge.DangerColor = Color.FromArgb(209, 75, 68);
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            _animationStep += 4;

            // Oscilacao suave para lembrar um painel de bancada em tempo real.
            double batteryWave = 12.0 + (Math.Sin(_animationStep * 0.045) * 1.9);
            double pwmWave = 52.0 + (Math.Sin((_animationStep * 0.085) + 0.7) * 42.0);
            double tempWave = 74.0 + (Math.Sin((_animationStep * 0.055) + 1.5) * 28.0);

            _batteryGauge.Value = (int)Math.Round(batteryWave);
            _pwmGauge.Value = (int)Math.Round(pwmWave);
            _temperatureGauge.Value = (int)Math.Round(tempWave);
        }
    }
}

/*
Instrucoes rapidas de integracao:
- Coloque este formulario em: local-api/src/SimulDIESEL/SimulDIESEL/UI/FormGaugeDemo.cs
- Coloque o controle em: local-api/src/SimulDIESEL/SimulDIESEL/UI/Controls/SdVerticalGauge.cs
- Compile a solucao SimulDIESEL; apos o build, o controle SdVerticalGauge passa a aparecer no Toolbox do WinForms.
- Para usar no Designer, abra um Form, procure por SdVerticalGauge no Toolbox e arraste para a superficie.
- Para instanciar por codigo: var gauge = new Controls.SdVerticalGauge { TitleText = "Bateria", UnitText = "V", Minimum = 8, Maximum = 16, Value = 12 };
*/

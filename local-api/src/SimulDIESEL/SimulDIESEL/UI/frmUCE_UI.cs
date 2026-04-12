using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimulDIESEL.BLL.Boards.UCE;
using SimulDIESEL.BLL.FormsLogic.UCE;

namespace SimulDIESEL.UI
{
    public sealed class frmUCE_UI : Form
    {
        private static frmUCE_UI _instance;

        private readonly FrmUceLogic _logic;
        private readonly CheckBox _builtinLedCheckBox;
        private readonly Button _startBlinkButton;
        private readonly Button _stopBlinkButton;
        private readonly TextBox _intervalTextBox;
        private readonly Label _statusLabel;
        private readonly Timer _blinkTimer;

        private bool _acceptedLedState;
        private bool _suppressBuiltinLedEvent;
        private bool _blinkTickInFlight;

        public frmUCE_UI()
        {
            _logic = FrmUceLogic.CreateDefault();
            _blinkTimer = new Timer();

            Text = "UCE - Unidade de Comunicacao Externa";
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ClientSize = new Size(560, 240);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;

            var container = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(16)
            };
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
            container.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            container.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            container.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            container.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            container.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var titleLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Controle do LED_BUILTIN da UCE",
                Font = new Font(Font, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            container.SetColumnSpan(titleLabel, 2);

            var intervalLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Intervalo do pisca (ms)",
                TextAlign = ContentAlignment.MiddleLeft
            };

            _intervalTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Text = "500"
            };

            _builtinLedCheckBox = new CheckBox
            {
                Dock = DockStyle.Fill,
                Text = "LED builtin ligado",
                AutoSize = true
            };
            container.SetColumnSpan(_builtinLedCheckBox, 2);

            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight
            };
            _startBlinkButton = new Button
            {
                AutoSize = true,
                Text = "Iniciar pisca"
            };
            _stopBlinkButton = new Button
            {
                AutoSize = true,
                Text = "Parar pisca",
                Enabled = false
            };
            buttonsPanel.Controls.Add(_startBlinkButton);
            buttonsPanel.Controls.Add(_stopBlinkButton);
            container.SetColumnSpan(buttonsPanel, 2);

            _statusLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Status: aguardando comando da UCE.",
                ForeColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.TopLeft
            };
            container.SetColumnSpan(_statusLabel, 2);

            container.Controls.Add(titleLabel, 0, 0);
            container.Controls.Add(intervalLabel, 0, 1);
            container.Controls.Add(_intervalTextBox, 1, 1);
            container.Controls.Add(_builtinLedCheckBox, 0, 2);
            container.Controls.Add(buttonsPanel, 0, 3);
            container.Controls.Add(_statusLabel, 0, 4);
            Controls.Add(container);

            _builtinLedCheckBox.CheckedChanged += BuiltinLedCheckBox_CheckedChanged;
            _startBlinkButton.Click += StartBlinkButton_Click;
            _stopBlinkButton.Click += StopBlinkButton_Click;
            _blinkTimer.Tick += BlinkTimer_Tick;
        }

        public static frmUCE_UI Instance
        {
            get
            {
                if (_instance == null || _instance.IsDisposed)
                    _instance = new frmUCE_UI();

                return _instance;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _blinkTimer.Stop();
            _blinkTimer.Tick -= BlinkTimer_Tick;
            _builtinLedCheckBox.CheckedChanged -= BuiltinLedCheckBox_CheckedChanged;
            _startBlinkButton.Click -= StartBlinkButton_Click;
            _stopBlinkButton.Click -= StopBlinkButton_Click;

            base.OnFormClosed(e);
        }

        private async void BuiltinLedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_suppressBuiltinLedEvent)
                return;

            await ApplyLedStateAsync(_builtinLedCheckBox.Checked, false).ConfigureAwait(true);
        }

        private async void StartBlinkButton_Click(object sender, EventArgs e)
        {
            int intervalMs;
            if (!TryParseInterval(out intervalMs))
                return;

            _blinkTimer.Interval = intervalMs;
            _blinkTimer.Start();
            SetBlinkUiState(true);
            await ApplyLedStateAsync(true, true).ConfigureAwait(true);
            SetStatus("Pisca da UCE iniciado no host com intervalo de " + intervalMs.ToString() + " ms.", true);
        }

        private async void StopBlinkButton_Click(object sender, EventArgs e)
        {
            _blinkTimer.Stop();
            SetBlinkUiState(false);
            await ApplyLedStateAsync(false, true).ConfigureAwait(true);
            SetStatus("Pisca da UCE interrompido no host. LED builtin desligado.", true);
        }

        private async void BlinkTimer_Tick(object sender, EventArgs e)
        {
            if (_blinkTickInFlight)
                return;

            _blinkTickInFlight = true;
            try
            {
                await ApplyLedStateAsync(!_acceptedLedState, true).ConfigureAwait(true);
            }
            finally
            {
                _blinkTickInFlight = false;
            }
        }

        private async Task ApplyLedStateAsync(bool desiredState, bool suppressPopup)
        {
            UceCommandResult result = await _logic
                .SetBuiltinLedAsync(desiredState)
                .ConfigureAwait(true);

            if (!result.Success || !result.AcceptedState.HasValue)
            {
                SetBuiltinLedCheckboxState(_acceptedLedState);
                if (suppressPopup)
                {
                    SetStatus(string.IsNullOrWhiteSpace(result.Message) ? "Falha na operacao da UCE." : result.Message, false);
                    return;
                }

                ShowOperationError(result.Message);
                return;
            }

            _acceptedLedState = result.AcceptedState.Value;
            SetBuiltinLedCheckboxState(_acceptedLedState);
            SetStatus("UCE respondeu sincronamente: LED builtin em " + (_acceptedLedState ? "ON" : "OFF") + ".", true);
        }

        private void SetBuiltinLedCheckboxState(bool value)
        {
            _suppressBuiltinLedEvent = true;
            try
            {
                _builtinLedCheckBox.Checked = value;
            }
            finally
            {
                _suppressBuiltinLedEvent = false;
            }
        }

        private void SetBlinkUiState(bool blinking)
        {
            _startBlinkButton.Enabled = !blinking;
            _stopBlinkButton.Enabled = blinking;
            _builtinLedCheckBox.Enabled = !blinking;
            _intervalTextBox.Enabled = !blinking;
        }

        private bool TryParseInterval(out int intervalMs)
        {
            intervalMs = 0;
            if (!int.TryParse(_intervalTextBox.Text, out intervalMs) || intervalMs < 50)
            {
                MessageBox.Show(
                    this,
                    "Informe um intervalo inteiro maior ou igual a 50 ms.",
                    "UCE",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void ShowOperationError(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "A operacao da UCE falhou.";

            SetStatus(message, false);
            MessageBox.Show(
                this,
                message,
                "UCE",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private void SetStatus(string text, bool success)
        {
            _statusLabel.Text = "Status: " + text;
            _statusLabel.ForeColor = success
                ? Color.FromArgb(46, 125, 50)
                : Color.FromArgb(183, 28, 28);
        }
    }
}

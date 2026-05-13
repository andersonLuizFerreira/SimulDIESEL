using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimulDIESEL.BLL.Boards.UCE;
using SimulDIESEL.BLL.FormsLogic.UCE;
using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Boards.UCE.Can;
using SimulDIESEL.DTL.Protocols.J1939.Capture;
using SimulDIESEL.DTL.Protocols.J1939.Diagnostics;
using SimulDIESEL.DTL.Protocols.J1939.NetworkManagement;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.UI
{
    public partial class frmUCE_UI : Form
    {
        private static frmUCE_UI _instance;

        private readonly FrmUceLogic _logic;
        private bool _acceptedLedState;
        private bool _suppressLedEvent;
        private bool _suppressCanEvents;
        private readonly Timer _canDriverLogTimer;
        private readonly Timer _canRxGridRefreshTimer;
        private readonly Timer _j1939DiagnosticReadTimer;
        private bool _canDriverLogPolling;
        private bool _canPeriodicTxActive;
        private bool _dispatcherOverflowDialogShown;
        private bool _j1939DiagnosticReadActive;
        private DateTime _j1939DiagnosticReadDeadline;
        private UceCanStatusResponse _lastCanStatus;
        private Button _btnCanRxClear;
        private TabPage _tabJ1939Diagnostics;
        private Button _btnReadJ1939FaultCodes;
        private DataGridView _dgJ1939Diagnostics;
        private Label _lblJ1939DiagnosticsStatus;
        private TabPage _tabJ1939Data;
        private Button _btnJ1939DataClear;
        private Button _btnJ1939CaptureStart;
        private Button _btnJ1939CaptureStop;
        private Label _lblJ1939CaptureStatus;
        private DataGridView _dgJ1939Data;
        private TabPage _tabJ1939Identification;
        private Button _btnJ1939IdentificationClear;
        private DataGridView _dgJ1939Identification;
        private readonly Dictionary<UiCanMonitorKey, UiCanMonitorRow> _canMonitorRowsByKey =
            new Dictionary<UiCanMonitorKey, UiCanMonitorRow>();
        private readonly List<UiCanMonitorRow> _canMonitorRows = new List<UiCanMonitorRow>();
        private readonly Dictionary<UiJ1939DataKey, UiJ1939DataRow> _j1939DataRowsByKey =
            new Dictionary<UiJ1939DataKey, UiJ1939DataRow>();
        private readonly List<UiJ1939DataRow> _j1939DataRows = new List<UiJ1939DataRow>();
        private readonly Dictionary<UiJ1939DiagnosticKey, UiJ1939DiagnosticRow> _j1939DiagnosticRowsByKey =
            new Dictionary<UiJ1939DiagnosticKey, UiJ1939DiagnosticRow>();
        private readonly List<UiJ1939DiagnosticRow> _j1939DiagnosticRows =
            new List<UiJ1939DiagnosticRow>();
        private readonly List<UiJ1939IdentificationRow> _j1939IdentificationRows =
            new List<UiJ1939IdentificationRow>();

        public event EventHandler J1939AddressRegistrySnapshotChanged;

        public frmUCE_UI()
        {
            InitializeComponent();

            _logic = FrmUceLogic.CreateDefault();
            _logic.LedEventReceived += Logic_LedEventReceived;
            _logic.CanRxFrameAvailable += Logic_CanRxFrameAvailable;
            _logic.CanRxTableChanged += Logic_CanRxTableChanged;
            _logic.DispatcherOverflowDiagnosticReceived += Logic_DispatcherOverflowDiagnosticReceived;
            _logic.CanDiagnosticStateChanged += Logic_CanDiagnosticStateChanged;

            chkLed.CheckedChanged += ChkLed_CheckedChanged;
            chkCanEnabled.CheckedChanged += CanControl_Changed;
            cmbCanSpeed.SelectedIndexChanged += CanControl_Changed;
            cmbCanMode.SelectedIndexChanged += CanControl_Changed;
            btnEnable.Click += BtnEnable_Click;
            Load += FrmUCE_UI_Load;
            tabUCE.SelectedIndexChanged += TabUCE_SelectedIndexChanged;

            _canDriverLogTimer = new Timer();
            _canDriverLogTimer.Interval = 500;
            _canDriverLogTimer.Tick += CanDriverLogTimer_Tick;

            _canRxGridRefreshTimer = new Timer();
            _canRxGridRefreshTimer.Interval = 200;
            _canRxGridRefreshTimer.Tick += CanRxGridRefreshTimer_Tick;

            _j1939DiagnosticReadTimer = new Timer();
            _j1939DiagnosticReadTimer.Interval = 200;
            _j1939DiagnosticReadTimer.Tick += J1939DiagnosticReadTimer_Tick;

            ApplyInitialCanUiState();
            ConfigureCanRxGrid();
            ConfigureJ1939DiagnosticsTab();
            ConfigureJ1939DataTab();
            ConfigureJ1939IdentificationTab();
            RefreshCanRxGrid();
            RefreshJ1939DataGrid();
            RefreshJ1939IdentificationGrid();
            UpdateCanDiagnosticIndicators();
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

        public IReadOnlyList<J1939AddressRegistryEntryDto> GetJ1939AddressRegistrySnapshotForRedeCan()
        {
            return _logic.GetJ1939AddressRegistrySnapshot();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            chkLed.CheckedChanged -= ChkLed_CheckedChanged;
            _logic.LedEventReceived -= Logic_LedEventReceived;
            _logic.CanRxFrameAvailable -= Logic_CanRxFrameAvailable;
            _logic.CanRxTableChanged -= Logic_CanRxTableChanged;
            _logic.DispatcherOverflowDiagnosticReceived -= Logic_DispatcherOverflowDiagnosticReceived;
            _logic.CanDiagnosticStateChanged -= Logic_CanDiagnosticStateChanged;
            chkCanEnabled.CheckedChanged -= CanControl_Changed;
            cmbCanSpeed.SelectedIndexChanged -= CanControl_Changed;
            cmbCanMode.SelectedIndexChanged -= CanControl_Changed;
            btnEnable.Click -= BtnEnable_Click;
            Load -= FrmUCE_UI_Load;
            tabUCE.SelectedIndexChanged -= TabUCE_SelectedIndexChanged;
            if (_btnCanRxClear != null)
                _btnCanRxClear.Click -= BtnCanRxClear_Click;
            if (_btnReadJ1939FaultCodes != null)
                _btnReadJ1939FaultCodes.Click -= BtnReadJ1939FaultCodes_Click;
            if (_btnJ1939DataClear != null)
                _btnJ1939DataClear.Click -= BtnJ1939DataClear_Click;
            if (_btnJ1939CaptureStart != null)
                _btnJ1939CaptureStart.Click -= BtnJ1939CaptureStart_Click;
            if (_btnJ1939CaptureStop != null)
                _btnJ1939CaptureStop.Click -= BtnJ1939CaptureStop_Click;
            if (_btnJ1939IdentificationClear != null)
                _btnJ1939IdentificationClear.Click -= BtnJ1939IdentificationClear_Click;
            _canDriverLogTimer.Stop();
            _canDriverLogTimer.Tick -= CanDriverLogTimer_Tick;
            _canDriverLogTimer.Dispose();
            _canRxGridRefreshTimer.Stop();
            _canRxGridRefreshTimer.Tick -= CanRxGridRefreshTimer_Tick;
            _canRxGridRefreshTimer.Dispose();
            _j1939DiagnosticReadTimer.Stop();
            _j1939DiagnosticReadTimer.Tick -= J1939DiagnosticReadTimer_Tick;
            _j1939DiagnosticReadTimer.Dispose();
            _logic.Dispose();

            base.OnFormClosed(e);
        }

        private void ApplyInitialCanUiState()
        {
            _suppressCanEvents = true;
            try
            {
                if (cmbCanSpeed.Items.Count > 5)
                    cmbCanSpeed.SelectedIndex = 5;
                else if (cmbCanSpeed.Items.Count > 0)
                    cmbCanSpeed.SelectedIndex = 0;

                if (cmbCanMode.Items.Count > 0)
                    cmbCanMode.SelectedIndex = 0;

                chkCanEnabled.Checked = false;
            }
            finally
            {
                _suppressCanEvents = false;
            }

            lblCanStatus.Text = "Status CAN: carregando estado da UCE...";
        }

        private async void FrmUCE_UI_Load(object sender, EventArgs e)
        {
            await RefreshCanStatusAsync(false).ConfigureAwait(true);
            // CAN_RX_EVENT assíncrono é o caminho primário; o poll fica preservado como fallback/diagnóstico.
            _canDriverLogTimer.Start();
            _canRxGridRefreshTimer.Start();
            RefreshCanRxGrid();
            UpdateCanDiagnosticIndicators();
        }

        private async void TabUCE_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ReferenceEquals(tabUCE.SelectedTab, tabDados))
            {
                await RefreshCanStatusAsync(false).ConfigureAwait(true);
                RefreshCanRxGrid();
                UpdateCanDiagnosticIndicators();
            }
            else if (ReferenceEquals(tabUCE.SelectedTab, _tabJ1939Data))
            {
                RefreshJ1939DataGrid();
            }
            else if (ReferenceEquals(tabUCE.SelectedTab, _tabJ1939Identification))
            {
                RefreshJ1939IdentificationGrid();
            }
        }

        private async void ChkLed_CheckedChanged(object sender, EventArgs e)
        {
            if (_suppressLedEvent)
                return;

            await ApplyLedStateAsync(chkLed.Checked).ConfigureAwait(true);
        }

        private async void CanControl_Changed(object sender, EventArgs e)
        {
            if (_suppressCanEvents)
                return;

            if (ReferenceEquals(sender, chkCanEnabled))
            {
                await ApplyCanEnabledAsync(chkCanEnabled.Checked).ConfigureAwait(true);
                return;
            }

            await ApplyCanConfigAsync().ConfigureAwait(true);
        }

        private void CanRxGridRefreshTimer_Tick(object sender, EventArgs e)
        {
            if (IsDisposed || Disposing)
                return;

            if (DrainCanRxOutputBuffer())
            {
                RefreshCanRxGrid();
                RefreshJ1939DiagnosticsGrid();
                RefreshJ1939DataGrid();
                RefreshJ1939IdentificationGrid();
            }
        }

        private void J1939DiagnosticReadTimer_Tick(object sender, EventArgs e)
        {
            if (IsDisposed || Disposing)
                return;

            bool changed = DrainCanRxOutputBuffer();
            if (changed)
            {
                RefreshCanRxGrid();
                RefreshJ1939DiagnosticsGrid();
                RefreshJ1939DataGrid();
                RefreshJ1939IdentificationGrid();
            }

            if (_j1939DiagnosticReadActive && DateTime.Now >= _j1939DiagnosticReadDeadline)
            {
                _j1939DiagnosticReadActive = false;
                _j1939DiagnosticReadTimer.Stop();
                _lblJ1939DiagnosticsStatus.Text = _j1939DiagnosticRows.Count == 0
                    ? "Sem resposta ou nenhum código recebido."
                    : "Leitura concluída. Falhas únicas: " + _j1939DiagnosticRows.Count.ToString(CultureInfo.InvariantCulture) + ".";
            }
        }

        private async void CanDriverLogTimer_Tick(object sender, EventArgs e)
        {
            if (_canDriverLogPolling)
                return;

            _canDriverLogPolling = true;
            try
            {
                await PollCanDriverLogAsync().ConfigureAwait(true);
            }
            finally
            {
                _canDriverLogPolling = false;
            }
        }

        private async void BtnEnable_Click(object sender, EventArgs e)
        {
            btnEnable.Enabled = false;
            try
            {
                if (_canPeriodicTxActive)
                {
                    await StopCanTxAsync().ConfigureAwait(true);
                    return;
                }

                CanTxInput input;
                string validationError;
                if (!TryReadCanTxInput(out input, out validationError))
                {
                    ShowOperationError(validationError);
                    return;
                }

                UceOperationResult<UceCanTxResponse> result = await _logic
                    .SendCanAsync(input.Extended, input.Id, input.Dlc, input.Data, input.PeriodMs)
                    .ConfigureAwait(true);

                if (!result.Success || result.Response == null)
                {
                    ShowOperationError(result.Message);
                    return;
                }

                if (result.Response.TxStatus == SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanTxStatusAcceptedSent)
                {
                    lstMensagens.Items.Add("CAN_TX one-shot enviado.");
                    return;
                }

                if (result.Response.TxStatus == SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanTxStatusPeriodicStarted)
                {
                    _canPeriodicTxActive = true;
                    btnEnable.Text = "Parar";
                    lstMensagens.Items.Add("CAN_TX periódico iniciado. Slot " + result.Response.SequenceOrSlot.ToString(CultureInfo.InvariantCulture) + ".");
                    return;
                }

                ShowOperationError("UCE retornou CAN_TX: " + UceCanProtocol.ToDisplayTxStatus(result.Response.TxStatus) + ".");
            }
            finally
            {
                btnEnable.Enabled = true;
            }
        }

        private async Task ApplyLedStateAsync(bool desiredState)
        {
            UceCommandResult result = await _logic
                .SetBuiltinLedAsync(desiredState)
                .ConfigureAwait(true);

            if (!result.Success || !result.AcceptedState.HasValue)
            {
                SetLedCheckboxState(_acceptedLedState);
                ShowOperationError(result.Message);
                return;
            }

            _acceptedLedState = result.AcceptedState.Value;
            SetLedCheckboxState(_acceptedLedState);
        }

        private void Logic_LedEventReceived(UceLedEvent ledEvent)
        {
            if (ledEvent == null)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Logic_LedEventReceived(ledEvent)));
                return;
            }

            _acceptedLedState = ledEvent.LedState;
            SetLedCheckboxState(ledEvent.LedState);
            lstMensagens.Items.Add(
                "UCE LED EVENT state=" +
                (ledEvent.LedState ? "ON" : "OFF") +
                " code=0x" +
                ledEvent.EventCode.ToString("X2", CultureInfo.InvariantCulture) +
                " counter=" +
                ledEvent.Counter.ToString(CultureInfo.InvariantCulture));

            while (lstMensagens.Items.Count > 200)
                lstMensagens.Items.RemoveAt(0);

            if (lstMensagens.Items.Count > 0)
                lstMensagens.TopIndex = lstMensagens.Items.Count - 1;
        }

        private void Logic_CanRxFrameAvailable(object sender, EventArgs e)
        {
            ScheduleCanRxGridRefresh();
        }

        private void Logic_CanRxTableChanged(object sender, EventArgs e)
        {
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
            {
                if (!IsDisposed && !Disposing)
                    BeginInvoke(new Action(() => Logic_CanRxTableChanged(sender, e)));
                return;
            }

            UpdateCanDiagnosticIndicators();
        }

        private void Logic_CanDiagnosticStateChanged(object sender, EventArgs e)
        {
            UpdateCanDiagnosticIndicators();
        }

        private void Logic_DispatcherOverflowDiagnosticReceived(UceDispatcherOverflowDiagnostic diagnostic)
        {
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action(() => Logic_DispatcherOverflowDiagnosticReceived(diagnostic)));
                }
                catch (InvalidOperationException)
                {
                }

                return;
            }

            UpdateCanDiagnosticIndicators();

            if (_dispatcherOverflowDialogShown)
                return;

            _dispatcherOverflowDialogShown = true;
            ShowOperationError(UceGatewayDiagnosticLog.BuildDispatcherFifoOverflowMessage(diagnostic));
        }

        private void ScheduleCanRxGridRefresh()
        {
            if (IsDisposed || Disposing)
                return;

            if (!_canRxGridRefreshTimer.Enabled)
                _canRxGridRefreshTimer.Start();
        }

        private void SetLedCheckboxState(bool value)
        {
            _suppressLedEvent = true;
            try
            {
                chkLed.Checked = value;
            }
            finally
            {
                _suppressLedEvent = false;
            }
        }

        private async Task ApplyCanConfigAsync()
        {
            int bitrateKbps;
            string mode;
            if (!TryGetSelectedCanConfig(out bitrateKbps, out mode))
                return;

            SetCanStatusText("Status CAN: aplicando configuração...");

            UceOperationResult<UceCanConfigResponse> result = await _logic
                .SetCanConfigAsync(bitrateKbps, mode)
                .ConfigureAwait(true);

            if (!result.Success || result.Response == null)
            {
                await RefreshCanStatusAsync(false).ConfigureAwait(true);
                SetCanStatusError(result.Message);
                ShowOperationError(result.Message);
                return;
            }

            await RefreshCanStatusAsync(false).ConfigureAwait(true);
        }

        private async Task ApplyCanEnabledAsync(bool enabled)
        {
            SetCanStatusText(enabled
                ? "Status CAN: habilitando porta..."
                : "Status CAN: desabilitando porta...");

            UceOperationResult<UceCanEnableResponse> result = await _logic
                .SetCanEnabledAsync(enabled)
                .ConfigureAwait(true);

            if (!result.Success || result.Response == null)
            {
                await RefreshCanStatusAsync(false).ConfigureAwait(true);
                SetCanStatusError(result.Message);
                ShowOperationError(result.Message);
                return;
            }

            await RefreshCanStatusAsync(false).ConfigureAwait(true);
        }

        private async Task RefreshCanStatusAsync(bool showErrorDialog)
        {
            UceOperationResult<UceCanStatusResponse> result = await _logic
                .GetCanStatusAsync()
                .ConfigureAwait(true);

            if (!result.Success || result.Response == null)
            {
                _lastCanStatus = null;
                SetCanStatusError(result.Message);
                UpdateCanDiagnosticIndicators();
                if (showErrorDialog)
                    ShowOperationError(result.Message);
                return;
            }

            _lastCanStatus = result.Response;
            ApplyCanStatus(result.Response);
            UpdateCanDiagnosticIndicators();
        }

        private void ConfigureCanRxGrid()
        {
            if (dgCanRx.Columns.Count > 0)
                return;

            ConfigureCanRxClearButton();
            dgCanRx.AutoGenerateColumns = false;
            dgCanRx.Columns.Add("CAN_ID", "CAN_ID");
            dgCanRx.Columns.Add("EXT", "EXT");
            dgCanRx.Columns.Add("RTR", "RTR");
            dgCanRx.Columns.Add("DLC", "DLC");
            dgCanRx.Columns.Add("D0", "D0");
            dgCanRx.Columns.Add("D1", "D1");
            dgCanRx.Columns.Add("D2", "D2");
            dgCanRx.Columns.Add("D3", "D3");
            dgCanRx.Columns.Add("D4", "D4");
            dgCanRx.Columns.Add("D5", "D5");
            dgCanRx.Columns.Add("D6", "D6");
            dgCanRx.Columns.Add("D7", "D7");
            dgCanRx.Columns.Add("LAST_TIMESTAMP", "LAST_TIMESTAMP");
            dgCanRx.Columns.Add("RX_COUNT", "RX_COUNT");
            dgCanRx.Columns.Add("SOURCE", "SOURCE");
            dgCanRx.Columns.Add("LAST_UPDATE", "LAST_UPDATE");

            dgCanRx.Rows.Clear();
        }

        private void ConfigureJ1939DiagnosticsTab()
        {
            if (_tabJ1939Diagnostics != null)
                return;

            _tabJ1939Diagnostics = new TabPage
            {
                Text = "Exibir Códigos de falha",
                Name = "tabJ1939Diagnostics",
                Padding = new Padding(8)
            };

            _btnReadJ1939FaultCodes = new Button
            {
                Text = "Ler códigos de falha",
                Width = 160,
                Height = 28,
                Location = new System.Drawing.Point(8, 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            _btnReadJ1939FaultCodes.Click += BtnReadJ1939FaultCodes_Click;

            _lblJ1939DiagnosticsStatus = new Label
            {
                Text = "Aguardando leitura.",
                AutoSize = false,
                Height = 28,
                Location = new System.Drawing.Point(178, 13),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            _dgJ1939Diagnostics = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoGenerateColumns = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Location = new System.Drawing.Point(8, 48),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            ConfigureJ1939DiagnosticsGridColumns();
            _tabJ1939Diagnostics.Controls.Add(_btnReadJ1939FaultCodes);
            _tabJ1939Diagnostics.Controls.Add(_lblJ1939DiagnosticsStatus);
            _tabJ1939Diagnostics.Controls.Add(_dgJ1939Diagnostics);
            _tabJ1939Diagnostics.Resize += (sender, args) => LayoutJ1939DiagnosticsControls();
            tabUCE.Controls.Add(_tabJ1939Diagnostics);
            LayoutJ1939DiagnosticsControls();
        }

        private void ConfigureJ1939DataTab()
        {
            if (_tabJ1939Data != null)
                return;

            _tabJ1939Data = new TabPage
            {
                Text = "Dados J1939",
                Name = "tabJ1939Data",
                Padding = new Padding(8)
            };

            _btnJ1939DataClear = new Button
            {
                Text = "Limpar",
                Width = 90,
                Height = 28,
                Location = new System.Drawing.Point(8, 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            _btnJ1939DataClear.Click += BtnJ1939DataClear_Click;

            _btnJ1939CaptureStart = new Button
            {
                Text = "Iniciar Captura",
                Width = 120,
                Height = 28,
                Location = new System.Drawing.Point(106, 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            _btnJ1939CaptureStart.Click += BtnJ1939CaptureStart_Click;

            _btnJ1939CaptureStop = new Button
            {
                Text = "Finalizar Captura",
                Width = 130,
                Height = 28,
                Location = new System.Drawing.Point(234, 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Enabled = false
            };
            _btnJ1939CaptureStop.Click += BtnJ1939CaptureStop_Click;

            _lblJ1939CaptureStatus = new Label
            {
                Text = "Captura: Parado",
                AutoSize = false,
                Height = 20,
                Location = new System.Drawing.Point(372, 13),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            _dgJ1939Data = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoGenerateColumns = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Location = new System.Drawing.Point(8, 44),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            EnableDoubleBuffered(_dgJ1939Data);
            ConfigureJ1939DataGridColumns();
            _tabJ1939Data.Controls.Add(_btnJ1939DataClear);
            _tabJ1939Data.Controls.Add(_btnJ1939CaptureStart);
            _tabJ1939Data.Controls.Add(_btnJ1939CaptureStop);
            _tabJ1939Data.Controls.Add(_lblJ1939CaptureStatus);
            _tabJ1939Data.Controls.Add(_dgJ1939Data);
            _tabJ1939Data.Resize += (sender, args) => LayoutJ1939DataControls();
            tabUCE.Controls.Add(_tabJ1939Data);
            LayoutJ1939DataControls();
        }

        private void ConfigureJ1939IdentificationTab()
        {
            if (_tabJ1939Identification != null)
                return;

            _tabJ1939Identification = new TabPage
            {
                Text = "Identificação J1939",
                Name = "tabJ1939Identification",
                Padding = new Padding(8)
            };

            _btnJ1939IdentificationClear = new Button
            {
                Text = "Limpar Identificação",
                Width = 150,
                Height = 28,
                Location = new System.Drawing.Point(8, 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            _btnJ1939IdentificationClear.Click += BtnJ1939IdentificationClear_Click;

            _dgJ1939Identification = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoGenerateColumns = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Location = new System.Drawing.Point(8, 44),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            EnableDoubleBuffered(_dgJ1939Identification);
            ConfigureJ1939IdentificationGridColumns();
            _tabJ1939Identification.Controls.Add(_btnJ1939IdentificationClear);
            _tabJ1939Identification.Controls.Add(_dgJ1939Identification);
            _tabJ1939Identification.Resize += (sender, args) => LayoutJ1939IdentificationControls();
            tabUCE.Controls.Add(_tabJ1939Identification);
            LayoutJ1939IdentificationControls();
        }

        private void ConfigureJ1939DiagnosticsGridColumns()
        {
            _dgJ1939Diagnostics.Columns.Add("TYPE", "Tipo");
            _dgJ1939Diagnostics.Columns.Add("SA", "Source Address");
            _dgJ1939Diagnostics.Columns.Add("SPN", "SPN");
            _dgJ1939Diagnostics.Columns.Add("SPN_NAME", "SPN Name");
            _dgJ1939Diagnostics.Columns.Add("FMI", "FMI");
            _dgJ1939Diagnostics.Columns.Add("FMI_DESCRIPTION", "FMI Description");
            _dgJ1939Diagnostics.Columns.Add("OC", "OC");
            _dgJ1939Diagnostics.Columns.Add("CM", "CM");
            _dgJ1939Diagnostics.Columns.Add("MIL", "MIL");
            _dgJ1939Diagnostics.Columns.Add("RED_STOP", "Red Stop");
            _dgJ1939Diagnostics.Columns.Add("AMBER", "Amber Warning");
            _dgJ1939Diagnostics.Columns.Add("PROTECT", "Protect");
            _dgJ1939Diagnostics.Columns.Add("TIMESTAMP", "Timestamp");
            _dgJ1939Diagnostics.Columns.Add("STATUS", "Status");
        }

        private void ConfigureJ1939DataGridColumns()
        {
            _dgJ1939Data.Columns.Add("ORIGIN", "Origem");
            _dgJ1939Data.Columns.Add("DESTINATION", "Destino");
            _dgJ1939Data.Columns.Add("PGN", "PGN");
            _dgJ1939Data.Columns.Add("DATA", "Dados");
            _dgJ1939Data.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void ConfigureJ1939IdentificationGridColumns()
        {
            _dgJ1939Identification.Columns.Add("ORIGIN", "Origem");
            _dgJ1939Identification.Columns.Add("FIELD", "Campo");
            _dgJ1939Identification.Columns.Add("HEX", "Valor Hex");
            _dgJ1939Identification.Columns.Add("DEC", "Valor Dec");
            _dgJ1939Identification.Columns.Add("DESCRIPTION", "Descrição");
            _dgJ1939Identification.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void LayoutJ1939DiagnosticsControls()
        {
            if (_tabJ1939Diagnostics == null || _dgJ1939Diagnostics == null)
                return;

            _lblJ1939DiagnosticsStatus.Width = Math.Max(80, _tabJ1939Diagnostics.ClientSize.Width - _lblJ1939DiagnosticsStatus.Left - 8);
            _dgJ1939Diagnostics.Size = new System.Drawing.Size(
                Math.Max(80, _tabJ1939Diagnostics.ClientSize.Width - 16),
                Math.Max(80, _tabJ1939Diagnostics.ClientSize.Height - _dgJ1939Diagnostics.Top - 8));
        }

        private void LayoutJ1939DataControls()
        {
            if (_tabJ1939Data == null || _dgJ1939Data == null)
                return;

            if (_lblJ1939CaptureStatus != null)
                _lblJ1939CaptureStatus.Width = Math.Max(80, _tabJ1939Data.ClientSize.Width - _lblJ1939CaptureStatus.Left - 8);

            _dgJ1939Data.Size = new System.Drawing.Size(
                Math.Max(80, _tabJ1939Data.ClientSize.Width - 16),
                Math.Max(80, _tabJ1939Data.ClientSize.Height - _dgJ1939Data.Top - 8));
        }

        private void LayoutJ1939IdentificationControls()
        {
            if (_tabJ1939Identification == null || _dgJ1939Identification == null)
                return;

            _dgJ1939Identification.Size = new System.Drawing.Size(
                Math.Max(80, _tabJ1939Identification.ClientSize.Width - 16),
                Math.Max(80, _tabJ1939Identification.ClientSize.Height - _dgJ1939Identification.Top - 8));
        }

        private void RefreshCanRxGrid()
        {
            DrainCanRxOutputBuffer();

            while (dgCanRx.Rows.Count < _canMonitorRows.Count)
                dgCanRx.Rows.Add();

            while (dgCanRx.Rows.Count > _canMonitorRows.Count)
                dgCanRx.Rows.RemoveAt(dgCanRx.Rows.Count - 1);

            for (int index = 0; index < _canMonitorRows.Count; ++index)
            {
                PopulateCanRxGridRow(dgCanRx.Rows[index], _canMonitorRows[index]);
            }
        }

        private void ConfigureCanRxClearButton()
        {
            if (_btnCanRxClear != null)
                return;

            _btnCanRxClear = new Button
            {
                Text = "LIMPAR",
                Width = 90,
                Height = 26,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnCanRxClear.Click += BtnCanRxClear_Click;
            groupBox1.Controls.Add(_btnCanRxClear);

            dgCanRx.Dock = DockStyle.None;
            dgCanRx.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            LayoutCanRxMonitorControls();
            groupBox1.Resize += (sender, args) => LayoutCanRxMonitorControls();
        }

        private void LayoutCanRxMonitorControls()
        {
            if (_btnCanRxClear == null)
                return;

            _btnCanRxClear.Location = new System.Drawing.Point(Math.Max(6, groupBox1.ClientSize.Width - _btnCanRxClear.Width - 8), 18);
            dgCanRx.Location = new System.Drawing.Point(6, 50);
            dgCanRx.Size = new System.Drawing.Size(Math.Max(50, groupBox1.ClientSize.Width - 12), Math.Max(50, groupBox1.ClientSize.Height - 56));
        }

        private void BtnCanRxClear_Click(object sender, EventArgs e)
        {
            _canMonitorRowsByKey.Clear();
            _canMonitorRows.Clear();
            dgCanRx.Rows.Clear();
        }

        private void BtnJ1939DataClear_Click(object sender, EventArgs e)
        {
            _j1939DataRowsByKey.Clear();
            _j1939DataRows.Clear();
            _dgJ1939Data.Rows.Clear();
        }

        private void BtnJ1939CaptureStart_Click(object sender, EventArgs e)
        {
            _logic.ClearJ1939TemporalCapture();
            J1939CaptureSessionDto session = _logic.StartJ1939TemporalCapture();
            SetJ1939CaptureUiState(true, session);
            lstMensagens.Items.Add("Captura temporal J1939 iniciada.");
        }

        private async void BtnJ1939CaptureStop_Click(object sender, EventArgs e)
        {
            _btnJ1939CaptureStop.Enabled = false;
            J1939CaptureSessionDto session = _logic.StopJ1939TemporalCapture();
            SetJ1939CaptureUiState(false, session);

            if (session == null)
                return;

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "Salvar captura temporal J1939";
                dialog.Filter = "Markdown (*.md)|*.md|Texto (*.txt)|*.txt";
                dialog.DefaultExt = "md";
                dialog.AddExtension = true;
                dialog.OverwritePrompt = true;
                dialog.FileName = "captura-j1939-" + DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + ".md";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    lstMensagens.Items.Add("Captura temporal J1939 finalizada sem exportacao.");
                    return;
                }

                try
                {
                    string path = dialog.FileName;
                    await Task.Run(() => _logic.ExportJ1939TemporalCapture(session, path)).ConfigureAwait(true);
                    lstMensagens.Items.Add("Captura temporal J1939 exportada: " + path);
                    SetJ1939CaptureStatusText("Captura: Parado - arquivo salvo (" + session.Events.Count.ToString(CultureInfo.InvariantCulture) + " eventos)");
                }
                catch (Exception ex)
                {
                    ShowOperationError("Falha ao exportar captura temporal J1939: " + ex.Message);
                }
            }
        }

        private void BtnJ1939IdentificationClear_Click(object sender, EventArgs e)
        {
            _j1939IdentificationRows.Clear();
            if (_dgJ1939Identification != null)
                _dgJ1939Identification.Rows.Clear();
        }

        private void SetJ1939CaptureUiState(bool capturing, J1939CaptureSessionDto session)
        {
            if (_btnJ1939CaptureStart != null)
                _btnJ1939CaptureStart.Enabled = !capturing;
            if (_btnJ1939CaptureStop != null)
                _btnJ1939CaptureStop.Enabled = capturing;

            if (capturing)
            {
                SetJ1939CaptureStatusText("Captura: Capturando desde " +
                    (session != null ? session.StartedAt.ToString("HH:mm:ss", CultureInfo.InvariantCulture) : DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture)));
                return;
            }

            int eventCount = session != null && session.Events != null ? session.Events.Count : 0;
            SetJ1939CaptureStatusText("Captura: Parado - " + eventCount.ToString(CultureInfo.InvariantCulture) + " eventos");
        }

        private void SetJ1939CaptureStatusText(string text)
        {
            if (_lblJ1939CaptureStatus != null)
                _lblJ1939CaptureStatus.Text = text;
        }

        private async void BtnReadJ1939FaultCodes_Click(object sender, EventArgs e)
        {
            _btnReadJ1939FaultCodes.Enabled = false;
            _j1939DiagnosticRowsByKey.Clear();
            _j1939DiagnosticRows.Clear();
            RefreshJ1939DiagnosticsGrid();
            _lblJ1939DiagnosticsStatus.Text = "Solicitando códigos de falha...";

            try
            {
                UceOperationResult<J1939DiagnosticReadResultDto> result = await _logic
                    .RequestJ1939DiagnosticCodesAsync()
                    .ConfigureAwait(true);

                if (!result.Success || result.Response == null)
                {
                    _lblJ1939DiagnosticsStatus.Text = "Falha ao solicitar códigos de falha.";
                    ShowOperationError(result.Message);
                    return;
                }

                _j1939DiagnosticReadActive = true;
                _j1939DiagnosticReadDeadline = DateTime.Now.AddMilliseconds(1500);
                _j1939DiagnosticReadTimer.Start();
                _lblJ1939DiagnosticsStatus.Text = "Requests DM1/DM2 enviados. Aguardando respostas...";
            }
            finally
            {
                _btnReadJ1939FaultCodes.Enabled = true;
            }
        }

        private bool DrainCanRxOutputBuffer()
        {
            bool changed = false;
            CanFrameDto frame;
            while (_logic.TryReadRxFrame(out frame))
            {
                UpdateCanMonitorRow(frame);
                ProcessJ1939Frame(frame);
                changed = true;
            }

            return changed;
        }

        private void ProcessJ1939Frame(CanFrameDto frame)
        {
            J1939DiagnosticMessageDto diagnostic;
            J1939DataMonitorMessageDto dataMessage;
            J1939NetworkEventDto networkEvent;
            if (!_logic.TryDecodeJ1939Frame(frame, out diagnostic, out dataMessage))
                diagnostic = null;

            if (diagnostic != null)
                UpdateJ1939DiagnosticRows(diagnostic);

            if (dataMessage != null)
            {
                UpdateJ1939DataRow(dataMessage);
                _logic.RegisterJ1939TemporalCaptureMessage(dataMessage, "Pipeline Dados J1939");
            }

            if (_logic.TryProcessJ1939NetworkFrame(frame, out networkEvent) && networkEvent != null && networkEvent.AddressClaim != null)
            {
                RebuildJ1939IdentificationRows();
                OnJ1939AddressRegistrySnapshotChanged();
            }
        }

        private void RefreshJ1939DiagnosticsGrid()
        {
            if (_dgJ1939Diagnostics == null)
                return;

            while (_dgJ1939Diagnostics.Rows.Count < _j1939DiagnosticRows.Count)
                _dgJ1939Diagnostics.Rows.Add();

            while (_dgJ1939Diagnostics.Rows.Count > _j1939DiagnosticRows.Count)
                _dgJ1939Diagnostics.Rows.RemoveAt(_dgJ1939Diagnostics.Rows.Count - 1);

            for (int index = 0; index < _j1939DiagnosticRows.Count; ++index)
                PopulateJ1939DiagnosticGridRow(_dgJ1939Diagnostics.Rows[index], _j1939DiagnosticRows[index]);
        }

        private void UpdateJ1939DiagnosticRows(J1939DiagnosticMessageDto message)
        {
            if (message == null)
                return;

            if (message.Dtcs == null || message.Dtcs.Count == 0)
            {
                UpdateJ1939DiagnosticRow(message, null);
                return;
            }

            foreach (J1939DtcDto dtc in message.Dtcs)
                UpdateJ1939DiagnosticRow(message, dtc);
        }

        private void UpdateJ1939DiagnosticRow(J1939DiagnosticMessageDto message, J1939DtcDto dtc)
        {
            UiJ1939DiagnosticKey key = new UiJ1939DiagnosticKey(
                message.Type,
                message.SourceAddress,
                dtc != null ? dtc.Spn : 0,
                dtc != null ? dtc.Fmi : (byte)0,
                dtc != null ? dtc.ConversionMethod : (byte)0,
                dtc != null);

            UiJ1939DiagnosticRow row;
            if (!_j1939DiagnosticRowsByKey.TryGetValue(key, out row))
            {
                row = new UiJ1939DiagnosticRow(key);
                _j1939DiagnosticRowsByKey.Add(key, row);
                _j1939DiagnosticRows.Add(row);
            }

            J1939LampStatusDto lamps = message.LampStatus;

            row.Type = message.Type;
            row.SourceAddress = message.SourceAddress;
            row.Spn = dtc != null ? dtc.Spn.ToString(CultureInfo.InvariantCulture) : "-";
            row.SpnName = dtc != null ? dtc.SpnName : "-";
            row.Fmi = dtc != null ? dtc.Fmi.ToString(CultureInfo.InvariantCulture) : "-";
            row.FmiDescription = dtc != null ? dtc.FmiDescription : "-";
            row.OccurrenceCount = dtc != null ? dtc.OccurrenceCount.ToString(CultureInfo.InvariantCulture) : "-";
            row.ConversionMethod = dtc != null ? dtc.ConversionMethod.ToString(CultureInfo.InvariantCulture) : "-";
            row.Mil = lamps != null ? lamps.Mil : "-";
            row.RedStop = lamps != null ? lamps.RedStop : "-";
            row.AmberWarning = lamps != null ? lamps.AmberWarning : "-";
            row.Protect = lamps != null ? lamps.Protect : "-";
            row.Timestamp = message.Timestamp == default(DateTime) ? DateTime.Now : message.Timestamp;
            row.Status = dtc != null ? dtc.Status : message.Status;
        }

        private static void PopulateJ1939DiagnosticGridRow(DataGridViewRow gridRow, UiJ1939DiagnosticRow row)
        {
            gridRow.Cells[0].Value = row.Type;
            gridRow.Cells[1].Value = "0x" + row.SourceAddress.ToString("X2", CultureInfo.InvariantCulture);
            gridRow.Cells[2].Value = row.Spn;
            gridRow.Cells[3].Value = row.SpnName;
            gridRow.Cells[4].Value = row.Fmi;
            gridRow.Cells[5].Value = row.FmiDescription;
            gridRow.Cells[6].Value = row.OccurrenceCount;
            gridRow.Cells[7].Value = row.ConversionMethod;
            gridRow.Cells[8].Value = row.Mil;
            gridRow.Cells[9].Value = row.RedStop;
            gridRow.Cells[10].Value = row.AmberWarning;
            gridRow.Cells[11].Value = row.Protect;
            gridRow.Cells[12].Value = row.Timestamp.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
            gridRow.Cells[13].Value = row.Status;
        }

        private void UpdateJ1939DataRow(J1939DataMonitorMessageDto message)
        {
            if (message == null)
                return;

            UiJ1939DataKey key = new UiJ1939DataKey(
                message.SourceAddress,
                message.IsGlobalDestination ? (byte)0xFF : message.DestinationAddress.GetValueOrDefault(0xFF),
                message.Pgn);

            UiJ1939DataRow row;
            if (!_j1939DataRowsByKey.TryGetValue(key, out row))
            {
                row = new UiJ1939DataRow(key);
                _j1939DataRowsByKey.Add(key, row);
                _j1939DataRows.Add(row);
            }

            row.DestinationDisplay = message.IsGlobalDestination
                ? "GLOBAL"
                : "0x" + message.DestinationAddress.GetValueOrDefault().ToString("X2", CultureInfo.InvariantCulture);
            row.FormattedPgn = string.IsNullOrWhiteSpace(message.FormattedPgn)
                ? message.Pgn.ToString("X6", CultureInfo.InvariantCulture)
                : message.FormattedPgn;
            row.Data = message.Data != null ? (byte[])message.Data.Clone() : new byte[0];
            row.LastUpdate = message.Timestamp == default(DateTime) ? DateTime.Now : message.Timestamp;
        }

        private void RefreshJ1939DataGrid()
        {
            if (_dgJ1939Data == null)
                return;

            while (_dgJ1939Data.Rows.Count < _j1939DataRows.Count)
                _dgJ1939Data.Rows.Add();

            while (_dgJ1939Data.Rows.Count > _j1939DataRows.Count)
                _dgJ1939Data.Rows.RemoveAt(_dgJ1939Data.Rows.Count - 1);

            for (int index = 0; index < _j1939DataRows.Count; ++index)
                PopulateJ1939DataGridRow(_dgJ1939Data.Rows[index], _j1939DataRows[index]);
        }

        private void RebuildJ1939IdentificationRows()
        {
            _j1939IdentificationRows.Clear();

            foreach (J1939AddressRegistryEntryDto entry in _logic.GetJ1939AddressRegistrySnapshot())
                AddJ1939IdentificationRows(entry);
        }

        private void OnJ1939AddressRegistrySnapshotChanged()
        {
            EventHandler handler = J1939AddressRegistrySnapshotChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void AddJ1939IdentificationRows(J1939AddressRegistryEntryDto entry)
        {
            if (entry == null || entry.ParsedName == null)
                return;

            string origin = "0x" + entry.SourceAddress.ToString("X2", CultureInfo.InvariantCulture);
            J1939NameDto name = entry.ParsedName;

            _j1939IdentificationRows.Add(CreateJ1939IdentificationRow(origin, "NAME", "0x" + name.NameHex, "-", "Address Claimed Name"));
            _j1939IdentificationRows.Add(CreateJ1939IdentificationRow(origin, "Identity Number", FormatHex(name.IdentityNumber), name.IdentityNumber.ToString(CultureInfo.InvariantCulture), string.Empty));
            _j1939IdentificationRows.Add(CreateJ1939IdentificationRow(origin, "Manufacturer Code", FormatHex(name.ManufacturerCode), name.ManufacturerCode.ToString(CultureInfo.InvariantCulture), "Desconhecido"));
            _j1939IdentificationRows.Add(CreateJ1939IdentificationRow(origin, "ECU Instance", FormatHex(name.EcuInstance), name.EcuInstance.ToString(CultureInfo.InvariantCulture), string.Empty));
            _j1939IdentificationRows.Add(CreateJ1939IdentificationRow(origin, "Function Instance", FormatHex(name.FunctionInstance), name.FunctionInstance.ToString(CultureInfo.InvariantCulture), string.Empty));
            _j1939IdentificationRows.Add(CreateJ1939IdentificationRow(origin, "Function", FormatHex(name.Function), name.Function.ToString(CultureInfo.InvariantCulture), "Desconhecido"));
            _j1939IdentificationRows.Add(CreateJ1939IdentificationRow(origin, "Vehicle System", FormatHex(name.VehicleSystem), name.VehicleSystem.ToString(CultureInfo.InvariantCulture), "Desconhecido"));
            _j1939IdentificationRows.Add(CreateJ1939IdentificationRow(origin, "Vehicle System Instance", FormatHex(name.VehicleSystemInstance), name.VehicleSystemInstance.ToString(CultureInfo.InvariantCulture), string.Empty));
            _j1939IdentificationRows.Add(CreateJ1939IdentificationRow(origin, "Industry Group", FormatHex(name.IndustryGroup), name.IndustryGroup.ToString(CultureInfo.InvariantCulture), "Desconhecido"));
            _j1939IdentificationRows.Add(CreateJ1939IdentificationRow(origin, "Arbitrary Address Capable", FormatHex((byte)(name.IsArbitraryAddressCapable ? 1 : 0)), name.IsArbitraryAddressCapable ? "1" : "0", name.IsArbitraryAddressCapable ? "Sim" : "Nao"));
        }

        private void RefreshJ1939IdentificationGrid()
        {
            if (_dgJ1939Identification == null)
                return;

            while (_dgJ1939Identification.Rows.Count < _j1939IdentificationRows.Count)
                _dgJ1939Identification.Rows.Add();

            while (_dgJ1939Identification.Rows.Count > _j1939IdentificationRows.Count)
                _dgJ1939Identification.Rows.RemoveAt(_dgJ1939Identification.Rows.Count - 1);

            for (int index = 0; index < _j1939IdentificationRows.Count; ++index)
                PopulateJ1939IdentificationGridRow(_dgJ1939Identification.Rows[index], _j1939IdentificationRows[index]);
        }

        private static void PopulateJ1939DataGridRow(DataGridViewRow gridRow, UiJ1939DataRow row)
        {
            gridRow.Cells[0].Value = "0x" + row.Key.SourceAddress.ToString("X2", CultureInfo.InvariantCulture);
            gridRow.Cells[1].Value = row.DestinationDisplay;
            gridRow.Cells[2].Value = "0x" + row.FormattedPgn;
            gridRow.Cells[3].Value = FormatPayload(row.Data);
        }

        private static void PopulateJ1939IdentificationGridRow(DataGridViewRow gridRow, UiJ1939IdentificationRow row)
        {
            gridRow.Cells[0].Value = row.Origin;
            gridRow.Cells[1].Value = row.Field;
            gridRow.Cells[2].Value = row.HexValue;
            gridRow.Cells[3].Value = row.DecimalValue;
            gridRow.Cells[4].Value = row.Description;
        }

        private void UpdateCanMonitorRow(CanFrameDto frame)
        {
            if (frame == null)
                return;

            UiCanMonitorKey key = new UiCanMonitorKey(frame.CanId, frame.IsExtended, frame.IsRemoteRequest);
            UiCanMonitorRow row;
            if (!_canMonitorRowsByKey.TryGetValue(key, out row))
            {
                row = new UiCanMonitorRow(key);
                _canMonitorRowsByKey.Add(key, row);
                _canMonitorRows.Add(row);
            }

            row.Dlc = frame.Dlc > 8 ? (byte)8 : frame.Dlc;
            if (row.Data == null || row.Data.Length != 8)
                row.Data = new byte[8];

            for (int dataIndex = 0; dataIndex < 8; ++dataIndex)
                row.Data[dataIndex] = frame.Data != null && dataIndex < frame.Data.Length ? frame.Data[dataIndex] : (byte)0;

            row.LastTimestamp = frame.Timestamp == default(DateTime) ? DateTime.Now : frame.Timestamp;
            row.LastUpdate = DateTime.Now;
            row.Source = frame.Source;
            ++row.RxCount;
        }

        private static void PopulateCanRxGridRow(DataGridViewRow gridRow, UiCanMonitorRow row)
        {
            gridRow.Cells[0].Value = "0x" + row.Key.CanId.ToString(row.Key.IsExtended ? "X8" : "X3", CultureInfo.InvariantCulture);
            gridRow.Cells[1].Value = row.Key.IsExtended ? "True" : "False";
            gridRow.Cells[2].Value = row.Key.IsRemoteRequest ? "True" : "False";
            gridRow.Cells[3].Value = row.Dlc.ToString(CultureInfo.InvariantCulture);

            for (int dataIndex = 0; dataIndex < 8; ++dataIndex)
            {
                string cellValue = dataIndex < row.Dlc && row.Data != null && dataIndex < row.Data.Length
                    ? row.Data[dataIndex].ToString("X2", CultureInfo.InvariantCulture)
                    : string.Empty;

                gridRow.Cells[4 + dataIndex].Value = cellValue;
            }

            gridRow.Cells[12].Value = row.LastTimestamp.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
            gridRow.Cells[13].Value = row.RxCount.ToString(CultureInfo.InvariantCulture);
            gridRow.Cells[14].Value = row.Source.ToString();
            gridRow.Cells[15].Value = row.LastUpdate.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
        }

        private async Task PollCanDriverLogAsync()
        {
            UceOperationResult<UceCanDriverLogPollResponse> result = await _logic
                .PollCanDriverLogAsync()
                .ConfigureAwait(true);

            if (!result.Success || result.Response == null)
                return;

            foreach (UceCanDriverLogEntry entry in result.Response.Entries)
            {
                lstMensagens.Items.Add(FormatDriverLogEntry(result.Response.Controller, entry));
            }

            while (lstMensagens.Items.Count > 200)
                lstMensagens.Items.RemoveAt(0);

            if (lstMensagens.Items.Count > 0)
                lstMensagens.TopIndex = lstMensagens.Items.Count - 1;
        }

        private void ApplyCanStatus(UceCanStatusResponse status)
        {
            _suppressCanEvents = true;
            try
            {
                chkCanEnabled.Checked = UceCanProtocol.IsEnabled(status.State);
                SetCanBitrateSelection(status.BitrateKbps);
                SetCanModeSelection(status.Mode);
            }
            finally
            {
                _suppressCanEvents = false;
            }

            SetCanStatusText(
                "Status CAN: " +
                UceCanProtocol.ToDisplayState(status.State) +
                ", " +
                status.BitrateKbps +
                " kbps, modo " +
                UceCanProtocol.ToDisplayMode(status.Mode));
            UpdateCanDiagnosticIndicators();
        }

        private void UpdateCanDiagnosticIndicators()
        {
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action(UpdateCanDiagnosticIndicators));
                }
                catch (InvalidOperationException)
                {
                }

                return;
            }

            CanDiagnosticStatusDto status = _logic.GetCanDiagnosticStatus(_lastCanStatus);
            lblCanDiagMirror.Text = "Mirror: " + status.MirrorStatusText;
            lblCanDiagSync.Text = "Sync: " + status.SyncStatusText;
            lblCanDiagDispatcher.Text = "Dispatcher FIFO: " + status.DispatcherStatusText;
            lblCanDiagTable.Text = "Tabela: " + status.TableStatusText;
            lblCanDiagCan.Text = "CAN: " + status.CanStatusText;
            lblCanDiagLastError.Text = "Último erro: " + status.LastErrorText;

            SetDiagnosticLabelColor(lblCanDiagMirror, status.MirrorStatusText == "OK");
            SetDiagnosticLabelColor(lblCanDiagSync, status.SyncStatusText == "Estável");
            SetDiagnosticLabelColor(lblCanDiagDispatcher, status.DispatcherStatusText == "OK");
            SetDiagnosticLabelColor(lblCanDiagTable, true);
            SetDiagnosticLabelColor(lblCanDiagCan, status.CanStatusText != "ERRO");
            SetDiagnosticLabelColor(lblCanDiagLastError, status.LastErrorText == "-");
        }

        private static void SetDiagnosticLabelColor(Label label, bool normal)
        {
            label.ForeColor = normal
                ? System.Drawing.SystemColors.ControlText
                : System.Drawing.Color.DarkOrange;
        }

        private void SetCanBitrateSelection(int bitrateKbps)
        {
            switch (bitrateKbps)
            {
                case 5:
                    cmbCanSpeed.SelectedIndex = 0;
                    break;
                case 10:
                    cmbCanSpeed.SelectedIndex = 1;
                    break;
                case 25:
                    cmbCanSpeed.SelectedIndex = 2;
                    break;
                case 50:
                    cmbCanSpeed.SelectedIndex = 3;
                    break;
                case 125:
                    cmbCanSpeed.SelectedIndex = 4;
                    break;
                case 250:
                    cmbCanSpeed.SelectedIndex = 5;
                    break;
                case 500:
                    cmbCanSpeed.SelectedIndex = 6;
                    break;
                case 800:
                    cmbCanSpeed.SelectedIndex = 7;
                    break;
                case 1000:
                    cmbCanSpeed.SelectedIndex = 8;
                    break;
            }
        }

        private void SetCanModeSelection(UceCanMode mode)
        {
            switch (mode)
            {
                case UceCanMode.Listen:
                    cmbCanMode.SelectedIndex = 1;
                    break;
                case UceCanMode.Loopback:
                    cmbCanMode.SelectedIndex = 2;
                    break;
                default:
                    cmbCanMode.SelectedIndex = 0;
                    break;
            }
        }

        private bool TryGetSelectedCanConfig(out int bitrateKbps, out string mode)
        {
            bitrateKbps = 250;
            mode = "normal";

            switch (cmbCanSpeed.SelectedIndex)
            {
                case 0:
                    bitrateKbps = 5;
                    break;
                case 1:
                    bitrateKbps = 10;
                    break;
                case 2:
                    bitrateKbps = 25;
                    break;
                case 3:
                    bitrateKbps = 50;
                    break;
                case 4:
                    bitrateKbps = 125;
                    break;
                case 5:
                    bitrateKbps = 250;
                    break;
                case 6:
                    bitrateKbps = 500;
                    break;
                case 7:
                    bitrateKbps = 800;
                    break;
                case 8:
                    bitrateKbps = 1000;
                    break;
                default:
                    return false;
            }

            switch (cmbCanMode.SelectedIndex)
            {
                case 0:
                    mode = "normal";
                    return true;
                case 1:
                    mode = "listen";
                    return true;
                case 2:
                    mode = "loopback";
                    return true;
                default:
                    return false;
            }
        }

        private struct UiCanMonitorKey : IEquatable<UiCanMonitorKey>
        {
            public readonly uint CanId;
            public readonly bool IsExtended;
            public readonly bool IsRemoteRequest;

            public UiCanMonitorKey(uint canId, bool isExtended, bool isRemoteRequest)
            {
                CanId = canId;
                IsExtended = isExtended;
                IsRemoteRequest = isRemoteRequest;
            }

            public bool Equals(UiCanMonitorKey other)
            {
                return CanId == other.CanId &&
                    IsExtended == other.IsExtended &&
                    IsRemoteRequest == other.IsRemoteRequest;
            }

            public override bool Equals(object obj)
            {
                return obj is UiCanMonitorKey && Equals((UiCanMonitorKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = (int)CanId;
                    hash = (hash * 397) ^ IsExtended.GetHashCode();
                    hash = (hash * 397) ^ IsRemoteRequest.GetHashCode();
                    return hash;
                }
            }
        }

        private sealed class UiCanMonitorRow
        {
            public UiCanMonitorRow(UiCanMonitorKey key)
            {
                Key = key;
                Data = new byte[8];
            }

            public UiCanMonitorKey Key { get; private set; }
            public byte Dlc { get; set; }
            public byte[] Data { get; set; }
            public DateTime LastTimestamp { get; set; }
            public uint RxCount { get; set; }
            public CanFrameSource Source { get; set; }
            public DateTime LastUpdate { get; set; }
        }

        private struct UiJ1939DataKey : IEquatable<UiJ1939DataKey>
        {
            public readonly byte SourceAddress;
            public readonly byte DestinationAddress;
            public readonly uint Pgn;

            public UiJ1939DataKey(byte sourceAddress, byte destinationAddress, uint pgn)
            {
                SourceAddress = sourceAddress;
                DestinationAddress = destinationAddress;
                Pgn = pgn;
            }

            public bool Equals(UiJ1939DataKey other)
            {
                return SourceAddress == other.SourceAddress &&
                    DestinationAddress == other.DestinationAddress &&
                    Pgn == other.Pgn;
            }

            public override bool Equals(object obj)
            {
                return obj is UiJ1939DataKey && Equals((UiJ1939DataKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = SourceAddress.GetHashCode();
                    hash = (hash * 397) ^ DestinationAddress.GetHashCode();
                    hash = (hash * 397) ^ (int)Pgn;
                    return hash;
                }
            }
        }

        private sealed class UiJ1939DataRow
        {
            public UiJ1939DataRow(UiJ1939DataKey key)
            {
                Key = key;
                Data = new byte[0];
            }

            public UiJ1939DataKey Key { get; private set; }
            public string DestinationDisplay { get; set; }
            public string FormattedPgn { get; set; }
            public byte[] Data { get; set; }
            public DateTime LastUpdate { get; set; }
        }

        private struct UiJ1939DiagnosticKey : IEquatable<UiJ1939DiagnosticKey>
        {
            public readonly string Type;
            public readonly byte SourceAddress;
            public readonly uint Spn;
            public readonly byte Fmi;
            public readonly byte ConversionMethod;
            public readonly bool HasDtc;

            public UiJ1939DiagnosticKey(string type, byte sourceAddress, uint spn, byte fmi, byte conversionMethod, bool hasDtc)
            {
                Type = string.IsNullOrWhiteSpace(type) ? string.Empty : type;
                SourceAddress = sourceAddress;
                Spn = spn;
                Fmi = fmi;
                ConversionMethod = conversionMethod;
                HasDtc = hasDtc;
            }

            public bool Equals(UiJ1939DiagnosticKey other)
            {
                return string.Equals(Type, other.Type, StringComparison.Ordinal) &&
                    SourceAddress == other.SourceAddress &&
                    Spn == other.Spn &&
                    Fmi == other.Fmi &&
                    ConversionMethod == other.ConversionMethod &&
                    HasDtc == other.HasDtc;
            }

            public override bool Equals(object obj)
            {
                return obj is UiJ1939DiagnosticKey && Equals((UiJ1939DiagnosticKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = Type.GetHashCode();
                    hash = (hash * 397) ^ SourceAddress.GetHashCode();
                    hash = (hash * 397) ^ (int)Spn;
                    hash = (hash * 397) ^ Fmi.GetHashCode();
                    hash = (hash * 397) ^ ConversionMethod.GetHashCode();
                    hash = (hash * 397) ^ HasDtc.GetHashCode();
                    return hash;
                }
            }
        }

        private sealed class UiJ1939DiagnosticRow
        {
            public UiJ1939DiagnosticRow(UiJ1939DiagnosticKey key)
            {
                Key = key;
            }

            public UiJ1939DiagnosticKey Key { get; private set; }
            public string Type { get; set; }
            public byte SourceAddress { get; set; }
            public string Spn { get; set; }
            public string SpnName { get; set; }
            public string Fmi { get; set; }
            public string FmiDescription { get; set; }
            public string OccurrenceCount { get; set; }
            public string ConversionMethod { get; set; }
            public string Mil { get; set; }
            public string RedStop { get; set; }
            public string AmberWarning { get; set; }
            public string Protect { get; set; }
            public DateTime Timestamp { get; set; }
            public string Status { get; set; }
        }

        private sealed class UiJ1939IdentificationRow
        {
            public string Origin { get; set; }
            public string Field { get; set; }
            public string HexValue { get; set; }
            public string DecimalValue { get; set; }
            public string Description { get; set; }
        }

        private async Task StopCanTxAsync()
        {
            UceOperationResult<UceCanTxStopResponse> result = await _logic
                .StopCanTxAsync()
                .ConfigureAwait(true);

            if (!result.Success || result.Response == null)
            {
                ShowOperationError(result.Message);
                return;
            }

            if (result.Response.TxStatus != SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanTxStatusPeriodicStopped)
            {
                ShowOperationError("UCE retornou CAN_TX_STOP: " + UceCanProtocol.ToDisplayTxStatus(result.Response.TxStatus) + ".");
                return;
            }

            _canPeriodicTxActive = false;
            btnEnable.Text = "Iniciar";
            lstMensagens.Items.Add("CAN_TX periódico parado.");
        }

        private struct CanTxInput
        {
            public bool Extended;
            public uint Id;
            public byte Dlc;
            public byte[] Data;
            public ushort PeriodMs;
        }

        private bool TryReadCanTxInput(out CanTxInput input, out string error)
        {
            input = new CanTxInput { Data = new byte[8] };
            error = null;

            bool extended;
            if (!TryGetCanTxIdKind(out extended))
            {
                error = "Selecione o tipo de mensagem CAN: 1.0/STD ou 2.0/EXT.";
                return false;
            }

            uint id;
            if (!TryParseUInt32Flexible(txtID.Text, out id))
            {
                error = "Informe um ID CAN válido.";
                return false;
            }

            if (!extended && id > 0x7FFU)
            {
                error = "ID inválido para CAN 1.0 / STD. Valor máximo: 0x7FF.";
                return false;
            }

            if (extended && id > 0x1FFFFFFFU)
            {
                error = "ID inválido para CAN 2.0 / EXT. Valor máximo: 0x1FFFFFFF.";
                return false;
            }

            int len;
            if (!int.TryParse(txtLEN.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out len) ||
                len < 0 ||
                len > 8)
            {
                error = "LEN deve estar entre 0 e 8.";
                return false;
            }

            TextBox[] dataFields = { txtD0, txtD1, txtD2, txtD3, txtD4, txtD5, txtD6, txtD7 };
            for (int i = 0; i < len; ++i)
            {
                byte dataByte;
                if (!TryParseByteFlexible(dataFields[i].Text, out dataByte))
                {
                    error = "D" + i.ToString(CultureInfo.InvariantCulture) + " deve ser hexadecimal entre 00 e FF.";
                    return false;
                }

                input.Data[i] = dataByte;
            }

            int repeatMs;
            if (!int.TryParse(txtTime.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out repeatMs) ||
                repeatMs < 0)
            {
                error = "Tempo de repetição deve ser maior ou igual a 0 ms.";
                return false;
            }

            if (repeatMs > ushort.MaxValue)
            {
                error = "Tempo de repetição deve estar entre 0 e 65535 ms.";
                return false;
            }

            input.Extended = extended;
            input.Id = id;
            input.Dlc = (byte)len;
            input.PeriodMs = (ushort)repeatMs;
            return true;
        }

        private bool TryGetCanTxIdKind(out bool extended)
        {
            extended = true;
            string value = cboCANTYPE.Text != null ? cboCANTYPE.Text.Trim() : string.Empty;
            if (string.Equals(value, "1.0", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "STD", StringComparison.OrdinalIgnoreCase) ||
                value.IndexOf("STD", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                extended = false;
                return true;
            }

            if (string.Equals(value, "2.0", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "EXT", StringComparison.OrdinalIgnoreCase) ||
                value.IndexOf("EXT", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                extended = true;
                return true;
            }

            return false;
        }

        private static bool TryParseUInt32Flexible(string text, out uint value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            string trimmed = text.Trim();
            if (trimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return uint.TryParse(
                    trimmed.Substring(2),
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out value);
            }

            return uint.TryParse(trimmed, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryParseByteFlexible(string text, out byte value)
        {
            value = 0;
            uint parsed;
            if (!TryParseUInt32Flexible(text, out parsed) || parsed > byte.MaxValue)
                return false;

            value = (byte)parsed;
            return true;
        }

        private static string FormatCanFrame(UceCanController controller, UceCanFrame frame)
        {
            var builder = new StringBuilder();
            builder.Append(controller == UceCanController.Can1 ? "CAN1 " : "CAN0 ");
            builder.Append(frame.Extended ? "EXT " : "STD ");
            builder.Append("ID=0x");
            builder.Append(frame.Id.ToString(frame.Extended ? "X8" : "X3", CultureInfo.InvariantCulture));
            builder.Append(" LEN=");
            builder.Append(frame.Dlc.ToString(CultureInfo.InvariantCulture));

            if (frame.RemoteRequest)
                builder.Append(" RTR");

            for (int i = 0; i < 8; ++i)
            {
                builder.Append(" D");
                builder.Append(i.ToString(CultureInfo.InvariantCulture));
                builder.Append("=");
                builder.Append(frame.Data[i].ToString("X2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        private static UiJ1939IdentificationRow CreateJ1939IdentificationRow(
            string origin,
            string field,
            string hexValue,
            string decimalValue,
            string description)
        {
            return new UiJ1939IdentificationRow
            {
                Origin = origin,
                Field = field,
                HexValue = hexValue,
                DecimalValue = string.IsNullOrWhiteSpace(decimalValue) ? "-" : decimalValue,
                Description = string.IsNullOrWhiteSpace(description) ? string.Empty : description
            };
        }

        private static string FormatHex(uint value)
        {
            return "0x" + value.ToString("X", CultureInfo.InvariantCulture);
        }

        private static string FormatHex(ushort value)
        {
            return "0x" + value.ToString("X", CultureInfo.InvariantCulture);
        }

        private static string FormatHex(byte value)
        {
            return "0x" + value.ToString("X", CultureInfo.InvariantCulture);
        }

        private static string FormatPayload(byte[] data)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            StringBuilder builder = new StringBuilder(data.Length * 3);
            for (int index = 0; index < data.Length; ++index)
            {
                if (index > 0)
                    builder.Append(' ');

                builder.Append(data[index].ToString("X2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        private static void EnableDoubleBuffered(DataGridView grid)
        {
            if (grid == null)
                return;

            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.SetProperty,
                null,
                grid,
                new object[] { true },
                CultureInfo.InvariantCulture);
        }

        private static string FormatDriverLogEntry(UceCanController controller, UceCanDriverLogEntry entry)
        {
            var builder = new StringBuilder();
            builder.Append("[");
            builder.Append(controller == UceCanController.Can1 ? "CAN1" : "CAN0");
            builder.Append("] ");
            builder.Append(DescribeDriverEvent(entry.EventCode));
            builder.Append(" - ");
            builder.Append(entry.BitrateKbps.ToString(CultureInfo.InvariantCulture));
            builder.Append(" kbps - ");
            builder.Append(UceCanProtocol.ToDisplayMode(entry.Mode));
            builder.Append(" - state=");
            builder.Append(UceCanProtocol.ToDisplayState(entry.InterfaceState));

            string detail = FormatDriverEventDetail(entry);
            if (!string.IsNullOrEmpty(detail))
            {
                builder.Append(" - ");
                builder.Append(detail);
            }

            return builder.ToString();
        }

        private static string DescribeDriverEvent(byte eventCode)
        {
            switch (eventCode)
            {
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventBegin:
                    return "DRIVER BEGIN";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventConfigRequested:
                    return "CONFIG REQUESTED";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventConfigOk:
                    return "CONFIG OK";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventConfigFault:
                    return "CONFIG FAULT";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventOpenRequested:
                    return "OPEN REQUESTED";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventOpenOk:
                    return "OPEN OK";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventOpenFault:
                    return "OPEN FAULT";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventCloseRequested:
                    return "CLOSE REQUESTED";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventCloseOk:
                    return "CLOSE OK";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventResetRequested:
                    return "RESET REQUESTED";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventResetOk:
                    return "RESET OK";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventStatusSnapshot:
                    return "STATUS SNAPSHOT";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventRxPoll:
                    return "RX POLL";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventRxFrameRead:
                    return "RX FRAME READ";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventUnsupportedController:
                    return "UNSUPPORTED CONTROLLER";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventInvalidBitrate:
                    return "INVALID BITRATE";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventInvalidMode:
                    return "INVALID MODE";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventCanPhysicalError:
                    return "CAN PHYSICAL ERROR";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventLoopbackDropped:
                    return "LOOPBACK DROPPED";
                default:
                    return "EVENT 0x" + eventCode.ToString("X2", CultureInfo.InvariantCulture);
            }
        }

        private static string FormatDriverEventDetail(UceCanDriverLogEntry entry)
        {
            switch (entry.EventCode)
            {
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventConfigRequested:
                    return "bitrateCode=0x" + entry.Detail0.ToString("X2", CultureInfo.InvariantCulture) +
                           ", modeCode=0x" + entry.Detail1.ToString("X2", CultureInfo.InvariantCulture);
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventConfigOk:
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventOpenOk:
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventStatusSnapshot:
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventCanPhysicalError:
                    return "status=0x" + entry.Detail0.ToString("X2", CultureInfo.InvariantCulture) +
                           ", txErr=" + entry.Detail1.ToString(CultureInfo.InvariantCulture) +
                           ", rxErr=" + entry.Detail2.ToString(CultureInfo.InvariantCulture);
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventRxPoll:
                    return "maxFrames=" + entry.Detail0.ToString(CultureInfo.InvariantCulture);
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventRxFrameRead:
                    return "ext=" + entry.Detail0.ToString(CultureInfo.InvariantCulture) +
                           ", rtr=" + entry.Detail1.ToString(CultureInfo.InvariantCulture) +
                           ", dlc=" + entry.Detail2.ToString(CultureInfo.InvariantCulture);
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventInvalidBitrate:
                    return "bitrateCode=0x" + entry.Detail0.ToString("X2", CultureInfo.InvariantCulture);
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventInvalidMode:
                    return "modeCode=0x" + entry.Detail0.ToString("X2", CultureInfo.InvariantCulture);
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventLoopbackDropped:
                    return "queued=" + entry.Detail0.ToString(CultureInfo.InvariantCulture);
                default:
                    return string.Empty;
            }
        }

        private void SetCanStatusText(string message)
        {
            lblCanStatus.Text = message;
        }

        private void SetCanStatusError(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "falha desconhecida";

            lblCanStatus.Text = "Status CAN: erro - " + message;
        }

        private void ShowOperationError(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "A operacao da UCE falhou.";

            MessageBox.Show(
                this,
                message,
                "UCE",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using SimulDIESEL.BLL.Protocols.J1939.NetworkManagement;
using SimulDIESEL.BLL.Services.Database;
using SimulDIESEL.DTL.Protocols.J1939.NetworkManagement;

namespace SimulDIESEL.UI
{
    public sealed class FrmRedeCan : Form
    {
        private static FrmRedeCan _instance;

        private readonly Func<IReadOnlyList<J1939AddressRegistryEntryDto>> _snapshotSource;
        private readonly J1939NodeIdentityService _nodeIdentityService;
        private readonly List<J1939NodeIdentityDto> _visibleNodes = new List<J1939NodeIdentityDto>();
        private frmUCE_UI _uceForm;
        private readonly DataGridView _grid;
        private readonly TextBox _detailsText;
        private readonly Label _statusLabel;

        public FrmRedeCan()
            : this(
                frmUCE_UI.Instance,
                new J1939NodeIdentityService(LocalDatabaseService.CreateDefaultJ1939ReferenceCatalogService()))
        {
        }

        private FrmRedeCan(
            frmUCE_UI uceForm,
            J1939NodeIdentityService nodeIdentityService)
            : this(
                () => uceForm.GetJ1939AddressRegistrySnapshotForRedeCan(),
                nodeIdentityService)
        {
            _uceForm = uceForm ?? throw new ArgumentNullException(nameof(uceForm));
            _uceForm.J1939AddressRegistrySnapshotChanged += UceForm_J1939AddressRegistrySnapshotChanged;
        }

        public FrmRedeCan(
            Func<IReadOnlyList<J1939AddressRegistryEntryDto>> snapshotSource,
            J1939NodeIdentityService nodeIdentityService)
        {
            _snapshotSource = snapshotSource ?? throw new ArgumentNullException(nameof(snapshotSource));
            _nodeIdentityService = nodeIdentityService ?? throw new ArgumentNullException(nameof(nodeIdentityService));

            Text = "Rede CAN";
            Width = 1180;
            Height = 720;
            MinimumSize = new Size(900, 560);

            var toolStrip = new ToolStrip
            {
                GripStyle = ToolStripGripStyle.Hidden,
                RenderMode = ToolStripRenderMode.System
            };

            var btnRefresh = new ToolStripButton("Atualizar");
            btnRefresh.Click += BtnRefresh_Click;

            var btnClear = new ToolStripButton("Limpar visualizacao");
            btnClear.Click += BtnClear_Click;

            toolStrip.Items.Add(btnRefresh);
            toolStrip.Items.Add(btnClear);

            _statusLabel = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 24,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 8, 0),
                Text = "Rede CAN: aguardando atualizacao."
            };

            _grid = CreateGrid();
            _grid.SelectionChanged += Grid_SelectionChanged;

            _detailsText = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font(FontFamily.GenericMonospace, 9f),
                BorderStyle = BorderStyle.FixedSingle
            };

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 360
            };
            split.Panel1.Controls.Add(_grid);
            split.Panel2.Controls.Add(_detailsText);

            Controls.Add(split);
            Controls.Add(_statusLabel);
            Controls.Add(toolStrip);

            Load += FrmRedeCan_Load;
            Activated += FrmRedeCan_Activated;
            FormClosed += FrmRedeCan_FormClosed;
        }

        public static FrmRedeCan Instance
        {
            get
            {
                if (_instance == null || _instance.IsDisposed)
                    _instance = new FrmRedeCan();

                return _instance;
            }
        }

        public void RefreshSnapshot()
        {
            IReadOnlyList<J1939AddressRegistryEntryDto> snapshot = _snapshotSource();
            RefreshSnapshot(snapshot);
        }

        public void RefreshSnapshot(IReadOnlyList<J1939AddressRegistryEntryDto> snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            IReadOnlyList<J1939NodeIdentityDto> identities = _nodeIdentityService.ResolveAll(snapshot);

            _visibleNodes.Clear();
            _visibleNodes.AddRange(identities);

            RebuildGridRows();
            UpdateDetails();
            _statusLabel.Text = "Rede CAN: " + _visibleNodes.Count.ToString(CultureInfo.InvariantCulture) + " modulo(s) detectado(s).";
        }

        private static DataGridView CreateGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = SystemColors.Window,
                BorderStyle = BorderStyle.FixedSingle,
                MultiSelect = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            AddColumn(grid, "Status", "Status", 120);
            AddColumn(grid, "SourceAddressHex", "SA Hex", 70);
            AddColumn(grid, "SourceAddressDecimal", "SA Decimal", 90);
            AddColumn(grid, "ManufacturerName", "Fabricante", 180);
            AddColumn(grid, "FunctionName", "Funcao", 160);
            AddColumn(grid, "IndustryGroupName", "Grupo", 160);
            AddColumn(grid, "PreferredAddressName", "Endereco preferencial", 190);
            AddColumn(grid, "LastSeenAt", "Ultimo RX", 140);
            AddColumn(grid, "NameHex", "NAME Hex", 170);

            return grid;
        }

        private static void AddColumn(DataGridView grid, string name, string headerText, int width)
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = headerText,
                MinimumWidth = 60,
                Width = width,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
        }

        private void FrmRedeCan_Load(object sender, EventArgs e)
        {
            RefreshSnapshot();
        }

        private void FrmRedeCan_Activated(object sender, EventArgs e)
        {
            RefreshSnapshot();
        }

        private void FrmRedeCan_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_uceForm != null)
                _uceForm.J1939AddressRegistrySnapshotChanged -= UceForm_J1939AddressRegistrySnapshotChanged;

            Load -= FrmRedeCan_Load;
            Activated -= FrmRedeCan_Activated;
            FormClosed -= FrmRedeCan_FormClosed;
            _grid.SelectionChanged -= Grid_SelectionChanged;
        }

        private void UceForm_J1939AddressRegistrySnapshotChanged(object sender, EventArgs e)
        {
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(RefreshSnapshot));
                return;
            }

            RefreshSnapshot();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            RefreshSnapshot();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            _visibleNodes.Clear();
            RebuildGridRows();
            UpdateDetails();
            _statusLabel.Text = "Rede CAN: visualizacao local limpa.";
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            UpdateDetails();
        }

        private void RebuildGridRows()
        {
            _grid.Rows.Clear();

            foreach (J1939NodeIdentityDto node in _visibleNodes)
            {
                int rowIndex = _grid.Rows.Add(
                    node.Status,
                    node.SourceAddressHex,
                    node.SourceAddressDecimal.ToString(CultureInfo.InvariantCulture),
                    node.ManufacturerName,
                    node.FunctionName,
                    node.IndustryGroupName,
                    node.PreferredAddressName,
                    FormatTimestamp(node.LastSeenAt),
                    node.NameHex);

                _grid.Rows[rowIndex].Tag = node;
            }

            if (_grid.Rows.Count > 0)
                _grid.Rows[0].Selected = true;
        }

        private void UpdateDetails()
        {
            J1939NodeIdentityDto selected = GetSelectedNode();
            if (selected == null)
            {
                _detailsText.Clear();
                return;
            }

            _detailsText.Text = BuildDetails(selected);
        }

        private J1939NodeIdentityDto GetSelectedNode()
        {
            if (_grid.SelectedRows.Count == 0)
                return null;

            return _grid.SelectedRows[0].Tag as J1939NodeIdentityDto;
        }

        private static string BuildDetails(J1939NodeIdentityDto node)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Resumo: " + node.Summary);
            builder.AppendLine("Status: " + node.Status);
            builder.AppendLine("Source Address: " + node.SourceAddressHex + " / " + node.SourceAddressDecimal.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("NAME: " + Safe(node.NameHex));
            builder.AppendLine();
            builder.AppendLine("Manufacturer Code: " + FormatCode(node.ManufacturerCode, node.ManufacturerName, node.ManufacturerKnown));
            builder.AppendLine("Function Code: " + FormatCode(node.FunctionCode, node.FunctionName, node.FunctionKnown));
            builder.AppendLine("Industry Group: " + FormatCode(node.IndustryGroupCode, node.IndustryGroupName, node.IndustryGroupKnown));
            builder.AppendLine("Vehicle System: " + FormatCode(node.VehicleSystemCode, node.VehicleSystemName, node.VehicleSystemKnown));
            builder.AppendLine("Endereco preferencial: " + Safe(node.PreferredAddressName) + FormatKnown(node.PreferredAddressKnown));
            builder.AppendLine();
            builder.AppendLine("ECU Instance: " + node.EcuInstance.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("Function Instance: " + node.FunctionInstance.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("Vehicle System Instance: " + node.VehicleSystemInstance.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("Identity Number: " + node.IdentityNumber.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("Arbitrary Address Capable: " + (node.ArbitraryAddressCapable ? "Sim" : "Nao"));
            builder.AppendLine("Ultimo RX: " + FormatTimestamp(node.LastSeenAt));
            return builder.ToString();
        }

        private static string FormatCode(int code, string name, bool known)
        {
            return code.ToString(CultureInfo.InvariantCulture) + " - " + Safe(name) + FormatKnown(known);
        }

        private static string FormatKnown(bool known)
        {
            return known ? " (catalogado)" : " (desconhecido)";
        }

        private static string FormatTimestamp(DateTime timestamp)
        {
            if (timestamp == default(DateTime))
                return string.Empty;

            return timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        private static string Safe(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "Desconhecido" : value;
        }
    }
}

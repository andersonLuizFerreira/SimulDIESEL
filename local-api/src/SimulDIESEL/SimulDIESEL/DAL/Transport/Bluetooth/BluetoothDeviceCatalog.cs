using Microsoft.Win32;
using SimulDIESEL.DTL.Boards.BPM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace SimulDIESEL.DAL.Transport.Bluetooth
{
    /// <summary>
    /// Descoberta pragmatica de alvos Bluetooth para a primeira versao SPP.
    /// Lista dispositivos pareados conhecidos pelo Windows e marca como
    /// disponiveis aqueles com uma porta COM SPP mapeada no host.
    /// </summary>
    public static class BluetoothDeviceCatalog
    {
        public const string PreferredBpmDeviceName = "SimulDIESEL - BPM";
        public const string PreferredBpmLegacyDeviceName = "SimulDIESEL-BPM";
        private const string PairedDevicesRegistryPath = @"SYSTEM\CurrentControlSet\Services\BTHPORT\Parameters\Devices";
        private const string BthEnumRegistryPath = @"SYSTEM\CurrentControlSet\Enum\BTHENUM";
        private const string BthModemRegistryPath = @"SYSTEM\CurrentControlSet\Enum\BTHMODEM";

        public static BluetoothDeviceDto[] ListDevices()
        {
            Dictionary<string, BluetoothDeviceDto> pairedDevices = LoadPairedDevices();
            Dictionary<string, BluetoothDeviceDto> usableDevices = LoadUsableDevices();

            foreach (KeyValuePair<string, BluetoothDeviceDto> binding in usableDevices)
            {
                BluetoothDeviceDto paired;
                if (pairedDevices.TryGetValue(binding.Key, out paired))
                {
                    paired.PortName = binding.Value.PortName;
                    paired.IsAvailable = binding.Value.IsAvailable;
                    paired.StatusText = binding.Value.StatusText;

                    if (string.IsNullOrWhiteSpace(paired.Name))
                        paired.Name = binding.Value.Name;
                }
                else
                {
                    pairedDevices[binding.Key] = binding.Value;
                }
            }

            return pairedDevices
                .Values
                .OrderByDescending(device => device.IsAvailable)
                .ThenBy(device => device.DisplayName)
                .ToArray();
        }

        public static bool TryResolvePreferredBpmDevice(out BluetoothDeviceDto device, out string reason)
        {
            BluetoothDeviceDto[] devices = ListDevices();
            BluetoothDeviceDto preferredDevice = SelectBestMatch(devices);

            if (preferredDevice == null)
            {
                device = null;
                reason = "Dispositivo Bluetooth pareado '" + PreferredBpmDeviceName + "' nao encontrado. Verifique se a BPM esta ligada e se o Windows ja concluiu o pareamento.";
                return false;
            }

            if (!preferredDevice.IsAvailable || string.IsNullOrWhiteSpace(preferredDevice.PortName))
            {
                device = preferredDevice;
                reason = "Nenhuma porta SPP Bluetooth encontrada para '" + preferredDevice.DisplayName + "'. Verifique se o Windows criou a COM Bluetooth e se o dispositivo continua pareado.";
                return false;
            }

            device = preferredDevice;
            reason = string.Empty;
            return true;
        }

        private static Dictionary<string, BluetoothDeviceDto> LoadPairedDevices()
        {
            Dictionary<string, BluetoothDeviceDto> devices = new Dictionary<string, BluetoothDeviceDto>(StringComparer.OrdinalIgnoreCase);

            using (RegistryKey root = SafeOpenLocalMachineSubKey(PairedDevicesRegistryPath))
            {
                if (root == null)
                    return devices;

                foreach (string subKeyName in SafeGetSubKeyNames(root))
                {
                    using (RegistryKey deviceKey = SafeOpenChildSubKey(root, subKeyName))
                    {
                        if (deviceKey == null)
                            continue;

                        string normalizedAddress = NormalizeAddress(subKeyName);
                        devices[normalizedAddress] = new BluetoothDeviceDto
                        {
                            Name = FirstNonEmpty(
                                ReadRegistryStringValue(deviceKey, "FriendlyName"),
                                ReadRegistryStringValue(deviceKey, "Name"),
                                "Dispositivo pareado"),
                            Address = FormatAddress(normalizedAddress),
                            PortName = string.Empty,
                            IsPaired = true,
                            IsAvailable = false,
                            StatusText = "Pareado sem porta SPP"
                        };
                    }
                }
            }

            return devices;
        }

        private static Dictionary<string, BluetoothDeviceDto> LoadUsableDevices()
        {
            Dictionary<string, BluetoothDeviceDto> devices = new Dictionary<string, BluetoothDeviceDto>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> systemPorts = new HashSet<string>(BluetoothTransport.ListPorts(), StringComparer.OrdinalIgnoreCase);

            CollectSppDevices(devices, systemPorts, BthEnumRegistryPath, false);
            CollectSppDevices(devices, systemPorts, BthModemRegistryPath, true);

            return devices;
        }

        private static void CollectSppDevices(
            Dictionary<string, BluetoothDeviceDto> devices,
            HashSet<string> systemPorts,
            string rootPath,
            bool acceptAnyBluetoothPath)
        {
            using (RegistryKey root = SafeOpenLocalMachineSubKey(rootPath))
            {
                if (root == null)
                    return;

                WalkRegistry(root, rootPath, devices, systemPorts, acceptAnyBluetoothPath);
            }
        }

        private static void WalkRegistry(
            RegistryKey currentKey,
            string currentPath,
            Dictionary<string, BluetoothDeviceDto> devices,
            HashSet<string> systemPorts,
            bool acceptAnyBluetoothPath)
        {
            string portName = ReadRegistryStringValue(currentKey, "PortName");
            if (!string.IsNullOrWhiteSpace(portName))
            {
                bool pathIsBluetooth = acceptAnyBluetoothPath ||
                    currentPath.IndexOf("00001101", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    currentPath.IndexOf("BTH", StringComparison.OrdinalIgnoreCase) >= 0;

                if (pathIsBluetooth && systemPorts.Contains(portName))
                {
                    string normalizedAddress = ExtractAddressFromPath(currentPath);
                    string key = string.IsNullOrWhiteSpace(normalizedAddress)
                        ? "PORT:" + portName.ToUpperInvariant()
                        : normalizedAddress;

                    BluetoothDeviceDto device;
                    if (!devices.TryGetValue(key, out device))
                    {
                        device = new BluetoothDeviceDto();
                        devices[key] = device;
                    }

                    device.Name = FirstNonEmpty(
                        device.Name,
                        ReadFriendlyName(currentKey, currentPath),
                        "Dispositivo Bluetooth");
                    device.Address = string.IsNullOrWhiteSpace(normalizedAddress)
                        ? "Nao informado"
                        : FormatAddress(normalizedAddress);
                    device.PortName = portName;
                    device.IsPaired = true;
                    device.IsAvailable = true;
                    device.StatusText = "Disponivel para conectar";
                }
            }

            foreach (string childName in SafeGetSubKeyNames(currentKey))
            {
                using (RegistryKey childKey = SafeOpenChildSubKey(currentKey, childName))
                {
                    if (childKey == null)
                        continue;

                    WalkRegistry(
                        childKey,
                        currentPath + "\\" + childName,
                        devices,
                        systemPorts,
                        acceptAnyBluetoothPath);
                }
            }
        }

        private static string ReadFriendlyName(RegistryKey currentKey, string currentPath)
        {
            string current = NormalizeFriendlyName(ReadRegistryStringValue(currentKey, "FriendlyName"));
            if (!string.IsNullOrWhiteSpace(current))
                return current;

            int lastSlash = currentPath.LastIndexOf('\\');
            if (lastSlash <= 0)
                return string.Empty;

            string parentPath = currentPath.Substring(0, lastSlash);
            using (RegistryKey parentKey = SafeOpenLocalMachineSubKey(parentPath))
            {
                if (parentKey == null)
                    return string.Empty;

                return NormalizeFriendlyName(ReadRegistryStringValue(parentKey, "FriendlyName"));
            }
        }

        private static string ReadRegistryStringValue(RegistryKey key, string valueName)
        {
            object value;
            try
            {
                value = key.GetValue(valueName);
            }
            catch (UnauthorizedAccessException)
            {
                return string.Empty;
            }
            catch (SecurityException)
            {
                return string.Empty;
            }

            if (value == null)
                return string.Empty;

            string asString = value as string;
            if (asString != null)
                return asString.Trim();

            byte[] bytes = value as byte[];
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            return DecodeRegistryBytes(bytes);
        }

        private static string NormalizeFriendlyName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;

            Match match = Regex.Match(raw, @"\(([^()]+)\)\s*$");
            if (match.Success)
                return match.Groups[1].Value.Trim();

            return raw.Replace("\0", string.Empty).Trim();
        }

        private static BluetoothDeviceDto SelectBestMatch(IEnumerable<BluetoothDeviceDto> devices)
        {
            if (devices == null)
                return null;

            BluetoothDeviceDto[] candidates = devices
                .Where(IsPreferredBpmDevice)
                .OrderByDescending(device => device.IsAvailable)
                .ThenBy(device => device.PortName)
                .ThenBy(device => device.DisplayName)
                .ToArray();

            if (candidates.Length > 0)
                return candidates[0];

            return devices
                .Where(device =>
                {
                    string normalized = NormalizeDeviceName(device != null ? device.DisplayName : string.Empty);
                    return normalized.Contains("SIMULDIESEL") && normalized.Contains("BPM");
                })
                .OrderByDescending(device => device.IsAvailable)
                .ThenBy(device => device.PortName)
                .ThenBy(device => device.DisplayName)
                .FirstOrDefault();
        }

        private static bool IsPreferredBpmDevice(BluetoothDeviceDto device)
        {
            if (device == null)
                return false;

            string normalized = NormalizeDeviceName(device.DisplayName);
            return normalized == NormalizeDeviceName(PreferredBpmDeviceName)
                || normalized == NormalizeDeviceName(PreferredBpmLegacyDeviceName);
        }

        private static string NormalizeDeviceName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            return Regex.Replace(name.Trim().ToUpperInvariant(), @"[^A-Z0-9]", string.Empty);
        }

        private static string ExtractAddressFromPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            string[] patterns =
            {
                @"Dev_([0-9A-F]{12})",
                @"BluetoothDevice_([0-9A-F]{12})",
                @"&0&([0-9A-F]{12})_",
                @"\\([0-9A-F]{12})\\"
            };

            foreach (string pattern in patterns)
            {
                Match match = Regex.Match(path, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                    return NormalizeAddress(match.Groups[1].Value);
            }

            return string.Empty;
        }

        private static string NormalizeAddress(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;

            return raw.Replace(":", string.Empty)
                .Replace("-", string.Empty)
                .Trim()
                .ToUpperInvariant();
        }

        private static string FormatAddress(string normalizedAddress)
        {
            if (string.IsNullOrWhiteSpace(normalizedAddress) || normalizedAddress.Length != 12)
                return "Nao informado";

            return string.Join(":", Enumerable.Range(0, 6).Select(i => normalizedAddress.Substring(i * 2, 2)));
        }

        private static string FirstNonEmpty(params string[] values)
        {
            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }

            return string.Empty;
        }

        private static string DecodeRegistryBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            int zeroIndex = Array.IndexOf(bytes, (byte)0x00);
            int length = zeroIndex >= 0 ? zeroIndex : bytes.Length;
            if (length <= 0)
                return string.Empty;

            bool looksUtf16Le = length > 1;
            for (int i = 1; i < length; i += 2)
            {
                if (bytes[i] != 0x00)
                {
                    looksUtf16Le = false;
                    break;
                }
            }

            string decoded;
            if (looksUtf16Le && length > 1)
            {
                decoded = Encoding.Unicode.GetString(bytes, 0, length);
            }
            else
            {
                decoded = Encoding.UTF8.GetString(bytes, 0, length);
                if (decoded.IndexOf('\uFFFD') >= 0)
                    decoded = Encoding.ASCII.GetString(bytes, 0, length);
            }

            return decoded
                .Replace("\0", string.Empty)
                .Trim();
        }

        private static RegistryKey SafeOpenLocalMachineSubKey(string path)
        {
            try
            {
                return Registry.LocalMachine.OpenSubKey(path);
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
            catch (SecurityException)
            {
                return null;
            }
        }

        private static RegistryKey SafeOpenChildSubKey(RegistryKey parent, string childName)
        {
            try
            {
                return parent.OpenSubKey(childName);
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
            catch (SecurityException)
            {
                return null;
            }
        }

        private static string[] SafeGetSubKeyNames(RegistryKey key)
        {
            try
            {
                return key.GetSubKeyNames();
            }
            catch (UnauthorizedAccessException)
            {
                return Array.Empty<string>();
            }
            catch (SecurityException)
            {
                return Array.Empty<string>();
            }
        }
    }
}

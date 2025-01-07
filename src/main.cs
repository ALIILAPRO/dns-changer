using Microsoft.Win32;
using System.Linq;
using System.Net.NetworkInformation;
using System.Management;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System;
using System.Net.Http;
using System.Net;

namespace DNS_Changer___by_aliilapro__.frm
{
    public partial class main : Form
    {

        private const string AppDataFolder = "DnsChangerByALIILAPRO";
        private const string ConfigFileName = "dns.config";
        private const string RegistryKeyName = "DNS.Changer.by.aliilapro.exe";
        private static Random rnd = new Random();
        private string GenerateUniqCode(int len)
        {
            string _allstring = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string _re = "";
            while (_re.Length < len)
            {
                _re += _allstring[rnd.Next(0, _allstring.Length - 1)].ToString();
            }
            return _re;
        }
        public Boolean Isconnect;
        private Dictionary<string, string[]> dnsServers = new Dictionary<string, string[]>
    {

        { "Shecan.ir", new[] { "178.22.122.100", "185.51.200.2" } },
        { "Begzar.ir", new[] { "185.55.225.25", "185.55.226.26" } },
        { "403.online", new[] { "10.202.10.202", "10.202.10.102" } },
        { "Radar.game", new[] { "10.202.10.10", "10.202.10.11" } },
        { "electrotm.org", new[] { "78.157.42.100", "78.157.42.101" } },
        { "Google Public DNS", new[] { "8.8.8.8", "8.8.4.4" } },
        { "Cloudflare DNS", new[] { "1.1.1.1", "1.0.0.1" } },
        { "OpenDNS", new[] { "208.67.222.222", "208.67.220.220" } },
        { "Quad9", new[] { "9.9.9.9", "149.112.112.112" } },
        { "Verisign Public DNS", new[] { "64.6.64.6", "64.6.65.6" } },
        { "Norton ConnectSafe", new[] { "199.85.126.10", "199.85.127.10" } },
        { "Comodo Secure DNS", new[] { "8.26.56.26", "8.20.247.20" } },
        { "OpenNIC Project", new[] { "23.94.60.240", "128.52.130.209" } }
    };

        public main()
        {
            InitializeComponent();

            LoadDnsServersFromConfig();
            UpdateConnectContextMenu();

            foreach (var server in dnsServers.Keys)
            {
                cmb_dns.Items.Add(server);
            }

            if (cmb_dns.Items.Count > 0)
            {
                cmb_dns.SelectedIndex = 0;
            }

            var currentInterface = GetActiveEthernetOrWifiNetworkInterface();
            if (currentInterface != null)
            {
                ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection objMOC = objMC.GetInstances();
                bool dnsFound = false;
                foreach (ManagementObject objMO in objMOC)
                {
                    if ((bool)objMO["IPEnabled"])
                    {
                        if (objMO["Caption"].ToString().Contains(currentInterface.Description))
                        {
                            string[] dnsservers = (string[])objMO["DNSServerSearchOrder"];
                            if (dnsservers != null && dnsservers.Length > 0)
                            {
                                foreach (KeyValuePair<string, string[]> entry in dnsServers)
                                {
                                    if (entry.Value.Any(ip => dnsservers.Contains(ip)))
                                    {
                                        string message = $"DNS servers currently set.\r\nDNS name: {entry.Key}\r\nDNS server: {string.Join(", ", dnsservers)}\r\n";
                                        txt_log.AppendText(message);
                                        dnsFound = true;
                                        Isconnect = true;
                                        break;
                                    }

                                }
                                if (!dnsFound)
                                {
                                    string message = "DNS servers currently set: " + string.Join(", ", dnsservers);
                                    txt_log.AppendText(message + "\r\n");
                                }
                            }
                        }
                    }

                }

            }

        }

        private void LoadDnsServersFromConfig()
        {
            string appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string folderPath = Path.Combine(appDataFolderPath, AppDataFolder);
            Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, ConfigFileName);
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                dnsServers.Clear();

                foreach (string line in lines)
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        string dnsName = parts[0];
                        string[] dnsIPs = parts[1].Split(',');

                        dnsServers.Add(dnsName, dnsIPs);
                    }
                }

                txt_log.AppendText("DNS servers loaded from file dns.config\r\n");
            }
            else
            {
                GenerateFolderAndSaveDNS();
            }
        }

        private void SaveDnsServersToConfig()
        {
            string appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string folderPath = Path.Combine(appDataFolderPath, AppDataFolder);
            Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, ConfigFileName);

            List<string> lines = new List<string>();
            foreach (var dnsServer in dnsServers)
            {
                string dnsName = dnsServer.Key;
                string[] dnsIPs = dnsServer.Value;
                string line = dnsName + "=" + string.Join(",", dnsIPs);
                lines.Add(line);
            }

            File.WriteAllLines(filePath, lines);

            txt_log.AppendText("DNS servers saved in file dns.config\r\n");
        }

        private void GenerateFolderAndSaveDNS()
        {
            string appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string folderPath = Path.Combine(appDataFolderPath, AppDataFolder);
            Directory.CreateDirectory(folderPath);

            SaveDnsServersToConfig();
        }


        public static NetworkInterface GetActiveEthernetOrWifiNetworkInterface()
        {
            var Nic = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(
                a => a.OperationalStatus == OperationalStatus.Up &&
                (a.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || a.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
                a.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily.ToString() == "InterNetwork"));

            return Nic;
        }

        public static void SetDNS(string DnsString1, string DnsString2)
        {
            string[] Dns = { DnsString1, DnsString2 };
            var CurrentInterface = GetActiveEthernetOrWifiNetworkInterface();
            if (CurrentInterface == null) return;

            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();
            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"])
                {
                    if (objMO["Caption"].ToString().Contains(CurrentInterface.Description))
                    {
                        ManagementBaseObject objdns = objMO.GetMethodParameters("SetDNSServerSearchOrder");
                        if (objdns != null)
                        {
                            objdns["DNSServerSearchOrder"] = Dns;
                            objMO.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
                        }
                    }
                }
            }
        }

        public static void UnsetDNS()
        {
            var CurrentInterface = GetActiveEthernetOrWifiNetworkInterface();
            if (CurrentInterface == null) return;

            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();
            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"])
                {
                    if (objMO["Caption"].ToString().Contains(CurrentInterface.Description))
                    {
                        ManagementBaseObject objdns = objMO.GetMethodParameters("SetDNSServerSearchOrder");
                        if (objdns != null)
                        {
                            objdns["DNSServerSearchOrder"] = null;
                            objMO.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
                        }
                    }
                }
            }
        }

        private void btn_connect_Click(object sender, System.EventArgs e)
        {
            var selectedServer = cmb_dns.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedServer)) return;

            if (!Isconnect)
            {
                var dns = dnsServers[selectedServer];
                SetDNS(dns[0], dns[1]);

                txt_log.AppendText("DNS " + selectedServer + " has been set.\r\n");
                btn_connect.Enabled = false;
                btn_disconnect.Enabled = true;
                cmb_dns.Enabled = false;
                noti_btndisconnect.Enabled = true;
                Isconnect = true;
            }
        }

        private void btn_disconnect_Click(object sender, System.EventArgs e)
        {
            if (Isconnect)
            {
                UnsetDNS();

                txt_log.AppendText("DNS settings have been cleared.\r\n");
                btn_connect.Enabled = true;
                btn_disconnect.Enabled = false;
                cmb_dns.Enabled = true;
                noti_btndisconnect.Enabled = true;
                Isconnect = false;
            }
        }
        
        private void main_Load(object sender, System.EventArgs e)
        {

            if (IsSetToRunAtStartup())
            {
                ch_startup.Checked = true;
            }
            else
            {
                ch_startup.Checked = false;
            }

            noti.BalloonTipTitle = "DNS Changer [ by aliilapro ]";
            noti.BalloonTipText = "The program is running ...";
            noti.Text = "DNS Changer [ by aliilapro ]";


           
            noti.ContextMenuStrip = contextMenuStrip1;
        }

        private void noti_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            noti.Visible = false;
            WindowState = FormWindowState.Normal;
        }

        private void main_Resize(object sender, System.EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                noti.Visible = true;
                noti.ShowBalloonTip(1000);
            }
            else if (FormWindowState.Normal == this.WindowState)
            { noti.Visible = false; }
        }

        private async void btn_check_Click(object sender, System.EventArgs e)
        {
            string url = txt_url.Text.Trim();

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                txt_log.AppendText("Invalid URL format.\r\n");
                return;
            }

            txt_log.AppendText($"Start checking URL: {url}\r\n");

            foreach (var dnsEntry in dnsServers)
            {
                var selectedServer = dnsEntry.Key;
                var dns = dnsEntry.Value;
                SetDNS(dns[0], dns[1]);

                try
                {
                    var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(5);
                    HttpResponseMessage response = await client.GetAsync(uri);
                    if (response.IsSuccessStatusCode)
                    {
                        txt_log.AppendText($"URL is up and running with DNS server {selectedServer}.\r\n");
                    }
                    else
                    {
                        txt_log.AppendText($"Request failed with DNS server {selectedServer}.\r\n");
                    }
                }
                catch (Exception ex)
                {
                    txt_log.AppendText($"An error occurred with DNS server {selectedServer}. Maybe blocked by your country.\r\n");
                }
            }

            UnsetDNS();

        }

        private void btn_connect_custom_Click(object sender, EventArgs e)
        {
            string dns1 = txt_custom_dns_ip1.Text.Trim();
            string dns2 = txt_custom_dns_ip2.Text.Trim();

            SetDNS(dns1, dns2);
            txt_log.AppendText($"DNS {dns1} / {dns2}  has been set.\r\n");
            btn_connect_custom.Enabled = false;
            btn_disconnect_custom.Enabled = true;
            noti_btndisconnect.Enabled = true;
        }

        private void btn_disconnect_custom_Click(object sender, EventArgs e)
        {
            UnsetDNS();
            txt_log.AppendText("DNS settings have been cleared.\r\n");
            btn_connect_custom.Enabled = true;
            btn_disconnect_custom.Enabled = false;
            noti_btndisconnect.Enabled = false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            bool @true = this.checkBox1.Checked;
            if (@true)
            {
                string customDNSName = this.GenerateUniqCode(6);
                txt_custom_dns_name.Text = "DNS_" + customDNSName;
                btn_connect_custom.Enabled = true;
                btn_disconnect_custom.Enabled = false;
                btn_save.Enabled = true;
                btn_connect.Enabled = false;
                btn_disconnect.Enabled = false;
            }
            else
            {
                btn_connect_custom.Enabled = false;
                btn_disconnect_custom.Enabled = false;
                btn_save.Enabled = false;
                btn_connect.Enabled = true;
                btn_disconnect.Enabled = true;
            }
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            string dnsName = txt_custom_dns_name.Text;
            string dnsIP1 = txt_custom_dns_ip1.Text;
            string dnsIP2 = txt_custom_dns_ip2.Text;

            if (!string.IsNullOrEmpty(dnsName) && !string.IsNullOrEmpty(dnsIP1) && !string.IsNullOrEmpty(dnsIP2))
            {
                string[] dnsIPs = { dnsIP1, dnsIP2 };
                dnsServers.Add(dnsName, dnsIPs);
                cmb_dns.Items.Add(dnsName);

                SaveDnsServersToConfig();

                txt_log.AppendText($"Custom DNS '{dnsName}' saved in dns.config\r\n");
            }
            else
            {
                MessageBox.Show("Please enter valid DNS details.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btn_updateconfig_Click(object sender, EventArgs e)
        {
            string url = "https://raw.githubusercontent.com/ALIILAPRO/dns-changer/master/config/dns.txt";

            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebResponse r = (HttpWebResponse)((HttpWebRequest)WebRequest.Create(url)).GetResponse();
                string response = (new StreamReader(r.GetResponseStream())).ReadToEnd();

                if (!string.IsNullOrEmpty(response))
                {
                    string[] lines = response.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string line in lines)
                    {
                        string[] dnsDetails = line.Split(',');

                        if (dnsDetails.Length == 3)
                        {
                            string dnsName = dnsDetails[0].Trim();
                            string dnsIP1 = dnsDetails[1].Trim();
                            string dnsIP2 = dnsDetails[2].Trim();

                            // Check if the DNS name already exists
                            if (!dnsServers.ContainsKey(dnsName))
                            {
                                string[] dnsIPs = { dnsIP1, dnsIP2 };
                                dnsServers.Add(dnsName, dnsIPs);
                                cmb_dns.Items.Add(dnsName);

                                SaveDnsServersToConfig();
                                txt_log.AppendText($"Custom DNS '{dnsName}' added.\r\n");
                            }
                            else
                            {
                                txt_log.AppendText($"Skipping existing DNS: '{dnsName}'.\r\n");
                            }
                        }
                        else
                        {
                            txt_log.AppendText($"Error: Invalid format in line: {line}\r\n");
                        }
                    }
                }
                else
                {
                    txt_log.AppendText("Error: Empty response.\r\n");
                }
            }
            catch (Exception ex)
            {
                txt_log.AppendText($"Error: {ex.Message}\r\n");
            }

        }

        private bool IsSetToRunAtStartup()
        {
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);
            if (rkApp != null)
            {
                string value = (string)rkApp.GetValue(RegistryKeyName);
                rkApp.Close();

                if (value != null && value == Application.ExecutablePath.ToString())
                {
                    return true;
                }
            }

            return false;
        }

        private void AddToStartup()
        {
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rkApp.SetValue(RegistryKeyName, Application.ExecutablePath.ToString());
            rkApp.Close();
            ch_startup.Checked = true;
            txt_log.AppendText("Info: DNS changer has been added to startup.\r\n");
        }

        private void RemoveFromStartup()
        {
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rkApp.DeleteValue(RegistryKeyName, false); 
            rkApp.Close();
            ch_startup.Checked = false;
            txt_log.AppendText("Info: DNS changer has been removed from startup.\r\n");
        }

        private void DnsServerMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem && menuItem.Tag is string[])
            {
                string[] dnsIPs = (string[])menuItem.Tag;
                SetDNS(dnsIPs[0], dnsIPs[1]);
                noti.ShowBalloonTip(2000, "DNS Server Connected", "DNS " + menuItem + " has been set.\r\n", ToolTipIcon.Info);
                txt_log.AppendText("DNS " + menuItem + " has been set.\r\n");
                noti_btndisconnect.Enabled = true;
            }
        }
        private void UpdateConnectContextMenu()
        {
            noti_btnconnect.DropDownItems.Clear();

            foreach (var dnsServer in dnsServers)
            {
                ToolStripMenuItem menuItem = new ToolStripMenuItem(dnsServer.Key);
                menuItem.Tag = dnsServer.Value;
                menuItem.Click += DnsServerMenuItem_Click;
                noti_btnconnect.DropDownItems.Add(menuItem);
            }
        }

        private void ch_startup_CheckedChanged(object sender, EventArgs e)
        {
            if (ch_startup.Checked)
            {
                AddToStartup();
            }
            else
            {
                RemoveFromStartup();
            }
        }

        private void noti_btndisconnect_Click(object sender, EventArgs e)
        {
            UnsetDNS();
            noti.ShowBalloonTip(2000, "DNS Server Disonnected", "DNS settings have been cleared.", ToolTipIcon.Info);
            txt_log.AppendText("DNS settings have been cleared.\r\n");
            if (btn_connect.Enabled == false && btn_disconnect.Enabled == true)
            {
                btn_connect.Enabled = true;
                btn_disconnect.Enabled = false;
            }
            if (btn_connect_custom.Enabled == false && btn_disconnect_custom.Enabled == true)
            {
                btn_connect_custom.Enabled = true;
                btn_disconnect_custom.Enabled = false;
            }
        }

        private long GetPingTime(string ipAddress)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = ping.Send(ipAddress, 1000); 
                    if (reply.Status == IPStatus.Success)
                    {
                        return reply.RoundtripTime;
                    }
                }
            }
            catch
            {
               
            }
            return long.MaxValue;
        }

        private string[] FindOptimalDNS()
        {
            string optimalDNSName = "";
            long minimumPing = long.MaxValue;
            string[] optimalDNS = null;

            foreach (var dnsEntry in dnsServers)
            {
                string dnsName = dnsEntry.Key;
                string[] dnsIPs = dnsEntry.Value;

                long averagePing = (GetPingTime(dnsIPs[0]) + GetPingTime(dnsIPs[1])) / 2;

                if (averagePing < minimumPing)
                {
                    minimumPing = averagePing;
                    optimalDNSName = dnsName;
                    optimalDNS = dnsIPs;
                }

                txt_log.AppendText($"DNS: {dnsName} -- Ping: {averagePing} ms\r\n");
            }

            if (optimalDNS != null)
            {
                txt_log.AppendText($"Optimal DNS: {optimalDNSName} -- Ping: {minimumPing} ms\r\n");
            }
            return optimalDNS;
        }



        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://idpay.ir/aliilapro");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://paypal.me/aliilapro");

        }

        private void toolStripMenuItem2_Click(object sender, System.EventArgs e)
        {
            Process.Start("https://github.com/ALIILAPRO/dns-changer/releases");
        }

        private void toolStripMenuItem1_Click(object sender, System.EventArgs e)
        {
            Process.Start("https://github.com/ALIILAPRO/DNS-Server-Switcher");
        }

        private void telegramToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            Process.Start("https://t.me/aliilapro");
        }

        private void githubToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            Process.Start("https://github.com/ALIILAPRO/dns-changer");
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] optimalDNS = FindOptimalDNS();
            if (optimalDNS != null)
            {
                SetDNS(optimalDNS[0], optimalDNS[1]);
                txt_log.AppendText($"Optimal DNS {string.Join(", ", optimalDNS)} has been set.\r\n");
            }
            else
            {
                txt_log.AppendText("Failed to find an optimal DNS.\r\n");
            }
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }
    }
}
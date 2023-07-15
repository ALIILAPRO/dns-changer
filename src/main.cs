using System.Linq;
using System.Net.NetworkInformation;
using System.Management;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System;
using System.Net.Http;

namespace DNS_Changer___by_aliilapro__.frm
{
    public partial class main : Form
    {
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
                Isconnect = false;
            }
        }
        private void telegramToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            Process.Start("https://t.me/aliilapro");
        }

        private void githubToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            Process.Start("https://github.com/ALIILAPRO/dns-changer");
        }

        private void main_Load(object sender, System.EventArgs e)
        {
            noti.BalloonTipTitle = "DNS Changer [ by aliilapro ]";
            noti.BalloonTipText = "The program is running ...";
            noti.Text = "DNS Changer [ by aliilapro ]";
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

        private void label1_Click(object sender, System.EventArgs e)
        {

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
            string dns1 = txt_custom1.Text.Trim();
            string dns2 = txt_custom2.Text.Trim();

            SetDNS(dns1, dns2);
            txt_log.AppendText($"DNS {dns1} / {dns2}  has been set.\r\n");
        }

        private void btn_disconnect_custom_Click(object sender, EventArgs e)
        {
            UnsetDNS();
            txt_log.AppendText("DNS settings have been cleared.\r\n");
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            bool @true = this.checkBox1.Checked;
            if (@true)
            {
                btn_connect_custom.Enabled = true;
                btn_disconnect_custom.Enabled = true;
                btn_connect.Enabled = false;
                btn_disconnect.Enabled = false;
            }
            else
            {
                btn_connect_custom.Enabled = false;
                btn_disconnect_custom.Enabled = false;
                btn_connect.Enabled = true;
                btn_disconnect.Enabled = true;
            }
        }

    }
}
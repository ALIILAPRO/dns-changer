using System.Linq;
using System.Net.NetworkInformation;
using System.Management;
using System.Windows.Forms;
using System.Diagnostics;

namespace DNS_Changer___by_aliilapro__.frm
{
    public partial class main : Form
    {
        bool isconnect = false;
        public main()
        {
            InitializeComponent();
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

        private void btn_shecan_Click(object sender, System.EventArgs e)
        {
            if (!isconnect)
            {
                SetDNS("178.22.122.100", "185.51.200.2");
                pic_off.Visible = false;
                pic_on.Visible = true;
                btn_shecan.Text = "Disconnect";
                tabPage2.Enabled = false;
                isconnect = true;
            }
            else
            {
                UnsetDNS();
                pic_off.Visible = true;
                pic_on.Visible = false;
                btn_shecan.Text = "Connect";
                tabPage2.Enabled = true;
                isconnect = false;
            }
        }

        private void btn_begzar_Click(object sender, System.EventArgs e)
        {
            if (!isconnect)
            {
                SetDNS("185.55.225.25", "185.55.226.26");
                pic_off_cl.Visible = false;
                pic_on_cl.Visible = true;
                btn_begzar.Text = "Disconnect";
                tabPage1.Enabled = false;
                isconnect = true;
            }
            else
            {
                UnsetDNS();
                pic_off_cl.Visible = true;
                pic_on_cl.Visible = false;
                btn_begzar.Text = "Connect";
                tabPage1.Enabled = true;
                isconnect = false;
            }
        }

        private void telegramToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            Process.Start("https://t.me/aliilapro");
        }

        private void githubToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            Process.Start("https://github.com/aliilapro");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace ChatWpfUI.Common
{
    public class ComputerInfoUtils
    {
        public static string GetMacAddress()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            string macAddress = string.Empty;

            foreach (NetworkInterface adapter in nics)
            {
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                    adapter.OperationalStatus == OperationalStatus.Up)
                {
                    macAddress = adapter.GetPhysicalAddress().ToString();
                    break;
                }
            }

            return macAddress;
        }
    }
}

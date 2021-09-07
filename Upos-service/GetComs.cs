using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Upos_service
{
    internal class ProcessConnection
    {

        public static ConnectionOptions ProcessConnectionOptions()

        {

            ConnectionOptions options = new ConnectionOptions();

            options.Impersonation = ImpersonationLevel.Impersonate;

            options.Authentication = AuthenticationLevel.Default;

            options.EnablePrivileges = true;

            return options;

        }



        public static ManagementScope ConnectionScope(string machineName, ConnectionOptions options, string path)

        {

            ManagementScope connectScope = new ManagementScope();

            connectScope.Path = new ManagementPath(@"\\" + machineName + path);

            connectScope.Options = options;

            connectScope.Connect();

            return connectScope;

        }

    }



    public class COMPortInfo

    {

        public string Name { get; set; }

        public string Description { get; set; }
        public string vid { get; set; }



        public COMPortInfo() { }



        public static List<COMPortInfo> GetCOMPortsInfo()

        {

            List<COMPortInfo> comPortInfoList = new List<COMPortInfo>();



            ConnectionOptions options = ProcessConnection.ProcessConnectionOptions();

            ManagementScope connectionScope = ProcessConnection.ConnectionScope(Environment.MachineName, options, @"\root\CIMV2");



            ObjectQuery objectQuery = new ObjectQuery("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0");

            ManagementObjectSearcher comPortSearcher = new ManagementObjectSearcher(connectionScope, objectQuery);



            using (comPortSearcher)

            {

                string caption = null;
                string vidi = null;
                List<string> portwmi = new List<string>();
                string[] ports = SerialPort.GetPortNames();
                foreach (ManagementObject obj in comPortSearcher.Get())

                {

                    if (obj != null)

                    {

                        object captionObj = obj["Caption"];
                        object vidObj = obj["DeviceID"];

                        if (captionObj != null)

                        { 
                           
                            vidi = vidObj.ToString();
                            caption = captionObj.ToString();

                            if (caption.Contains("(COM"))

                            {
                             
                                COMPortInfo comPortInfo = new COMPortInfo();

                                comPortInfo.Name = caption.Substring(caption.LastIndexOf("(COM")).Replace("(", string.Empty).Replace(")", string.Empty);
                                portwmi.Add(comPortInfo.Name);
                                comPortInfo.Description = caption;
                                comPortInfo.vid = Regex.Match(vidi,"VID_[\\w+?]+&PID_[\\w +?]+").Value;
                                comPortInfoList.Add(comPortInfo);

                            }
                            if ( caption.Contains("Comm"))

                            {
                                
                                COMPortInfo comPortInfo = new COMPortInfo();
                                comPortInfo.Name = (ports.Except(portwmi)).First();
                                comPortInfo.Description = caption;
                                comPortInfo.vid = Regex.Match(vidi, "VID_[\\w+?]+&PID_[\\w +?]+").Value;
                                comPortInfoList.Add(comPortInfo);

                            }

                        }

                    }

                    
                }

            }

            return comPortInfoList;

        }
    }
}

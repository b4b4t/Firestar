using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Firestar
{
    public class FirestarEngine
    {
        #region Constants

        private const string IP_ADDRESS_KEY = "IpAddress";
        private const string ACCOUNT_NAME_KEY = "TargetUserName";
        private const string SECURITY_TYPE = "Security";

        #endregion

        #region Properties

        private string _logPath = null;
        private DateTime _lastReadExecution = DateTime.MinValue;
        private readonly EventLog _eventLog = null;
        private Dictionary<string, IpAddressInfo> _ipAddressesRecorded = new Dictionary<string, IpAddressInfo>();
        private static XNamespace ns = "http://schemas.microsoft.com/win/2004/08/events/event";

        #endregion

        public FirestarEngine()
        {
            _eventLog = new EventLog(SECURITY_TYPE, Environment.MachineName);
            _lastReadExecution = DateTime.Now.AddHours(-1);
            _logPath = ConfigurationManager.AppSettings["ServicePath"].ToString();
        }

        /// <summary>
        /// Analyse all ip addresses to find brute force attack
        /// </summary>
        /// <param name="ipAddress"></param>
        public void AnalyseIpAddresses()
        {
            // Log in file
            string fileName = "logs.txt";
            List<string> logs = new List<string>();

            foreach (IpAddressInfo ipAddress in _ipAddressesRecorded.Values)
            {
                if(ipAddress.BlockedAtDate < DateTime.Now && ipAddress.Blocked)
                {
                    // Set firewall rule
                    //AddRule(ipAddress.IpAddress);

                    ipAddress.SetBlockedDate();

                    logs.Add($"{ipAddress.LastRequest.ToString("dd/MM/yyyy HH:mm:ss")} : {ipAddress.IpAddress} blocked until {ipAddress.BlockedAtDate.ToString("dd/MM/yyyy HH:mm:ss")}");
                }
                else if(ipAddress.BlockedAtDate < DateTime.Now && ipAddress.Blocked && ipAddress.Counter > 0)
                {
                    // Delete firewall rule
                    //DeleteRule(ipAddress.IpAddress);

                    // Add log
                    logs.Add($"{ipAddress.LastRequest.ToString("dd/MM/yyyy HH:mm:ss")} : {ipAddress.IpAddress} unblocked");

                    // Reset Ip address
                    ipAddress.Reset();
                }
            }

            File.AppendAllLines($"{_logPath}\\{fileName}", logs);
        }

        /// <summary>
        /// Read addresses ip in security logs
        /// </summary>
        /// <param name="ipAddressesRecorded"></param>
        public void ReadIpAddresses()
        {
            string accountName = null;
            string ipAddress = null;
            EventLogEntry entry = null;
            DateTime currentExecutionDate = DateTime.Now;
            IpAddressInfo ipAddressInfo = null;
            XDocument xml = null;
            int lastLog = _eventLog.Entries.Count;

            for (int i = _eventLog.Entries.Count; i > 0; i--)
            {
                entry = _eventLog.Entries[i];

                // Take audit failed logs before last read execution
                if (entry.InstanceId != 4625 || entry.TimeGenerated < _lastReadExecution)
                    continue;

                xml = XDocument.Parse(entry.Message);

                // Add Ip address
                var data = xml.Descendants(ns + "Data");
                ipAddress = data.SingleOrDefault(d => d.Name == IP_ADDRESS_KEY)?.Value ?? String.Empty;
                accountName = data.SingleOrDefault(d => d.Name == ACCOUNT_NAME_KEY)?.Value ?? String.Empty;

                if (String.IsNullOrEmpty(ipAddress))
                    continue;

                if(_ipAddressesRecorded.ContainsKey(ipAddress))
                {
                    ipAddressInfo = _ipAddressesRecorded[ipAddress];
                }
                else
                {
                    ipAddressInfo = new IpAddressInfo(ipAddress);

                    // Add Ip address to list
                    _ipAddressesRecorded[ipAddress] = ipAddressInfo;
                }

                ipAddressInfo.Increase();
            }
        }


        #region Utils

        /// <summary>
        /// Add a rule in the windows firewall
        /// </summary>
        /// <param name="ipAddress"></param>
        public void AddRule(string ipAddress)
        {
            // Create rule
            INetFwRule2 rule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
            rule.Name = $"Firestar Access Block {ipAddress}";
            rule.Description = "Block Incoming Connections from IP Address.";
            rule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            rule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
            rule.Enabled = true;
            rule.InterfaceTypes = "All";
            rule.RemoteAddresses = ipAddress;

            // Add rule in the firewall
            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            firewallPolicy.Rules.Add(rule);

            String msg = $"IP Address {ipAddress} Blocked Successfully!";
            Console.WriteLine(msg);
        }

        /// <summary>
        /// Delete corresponding rules for an ip address
        /// </summary>
        /// <param name="ipAddress"></param>
        public void DeleteRule(string ipAddress)
        {

        }

        #endregion
    }
}

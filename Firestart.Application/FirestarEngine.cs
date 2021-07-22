using Microsoft.Extensions.Caching.Memory;
using NetFwTypeLib;
using System;
using System.Diagnostics.Eventing.Reader;
using System.Net;
using System.Text.RegularExpressions;

namespace Firestar
{
    public enum RuleSetMode
    {
        Add,
        Remove
    }

    public class FirestarEngine
    {
        #region Constants

        private const string IP_ADDRESS_PATTERN = "<Data Name='IpAddress'>(.*?)</Data>";
        private const string SQL_SERVER_IP_ADDRESS_PATTERN = "\\[CLIENT: (.*?)\\]";
        private const string SECURITY_TYPE = "Security";
        private const string APPLICATION_TYPE = "Application";
        private const string RDP_RULE_NAME = "Firestar RDP";
        private const string SQL_RULE_NAME = "Firestar SQL server";
        private const char IP_SEPARATOR = ',';

        #endregion

        #region Properties

        private readonly CacheEngine _cacheEngine;

        #endregion

        public FirestarEngine(CacheEngine cacheEngine)
        {
            _cacheEngine = cacheEngine;

            ResetRules();
        }

        #region Subscriptions

        public void EventSubscription()
        {
            string eventQueryString = "*[System/EventID=4625]";

            EventLogQuery eventQuery = new EventLogQuery(SECURITY_TYPE, PathType.LogName, eventQueryString);

            EventLogWatcher watcher = new EventLogWatcher(eventQuery);
            watcher.EventRecordWritten += new EventHandler<EventRecordWrittenEventArgs>(RdpHandlerEvent);
            watcher.Enabled = true;
        }

        public void SqlServerEventSubscription()
        {
            string eventQueryString = "*[System/EventID=18456 and System/Provider/@Name=\"MSSQLSERVER\"]";

            EventLogQuery eventQuery = new EventLogQuery(APPLICATION_TYPE, PathType.LogName, eventQueryString);

            EventLogWatcher watcher = new EventLogWatcher(eventQuery);
            watcher.EventRecordWritten += new EventHandler<EventRecordWrittenEventArgs>(SqlServerHandlerEvent);
            watcher.Enabled = true;
        }

        #endregion

        #region Handlers

        public void RdpHandlerEvent(object obj, EventRecordWrittenEventArgs arg)
        {
            if (arg.EventRecord != null)
            {
                string eventXml = arg.EventRecord.ToXml();
                Match match = Regex.Match(eventXml, IP_ADDRESS_PATTERN);

                if (match.Success)
                {
                    string ipAddress = match.Groups[1].Value;

                    if (!string.IsNullOrEmpty(ipAddress) && !ipAddress.StartsWith("127.") && !ipAddress.StartsWith("0"))
                    {
                        AddIpAddress(ipAddress, RDP_RULE_NAME);
                    }
                }
            }
        }

        public void SqlServerHandlerEvent(object obj, EventRecordWrittenEventArgs arg)
        {
            if (arg.EventRecord != null)
            {
                string eventXml = arg.EventRecord.ToXml();
                Match match = Regex.Match(eventXml, SQL_SERVER_IP_ADDRESS_PATTERN);

                if (match.Success)
                {
                    string strIpAddress = match.Groups[1].Value;

                    if (IPAddress.TryParse(strIpAddress, out _)
                        && match.Success
                        && !strIpAddress.StartsWith("127.")
                        && !strIpAddress.StartsWith("0")
                    ) {
                        AddIpAddress(strIpAddress, SQL_RULE_NAME);
                    }
                }
            }
        }

        #endregion

        #region Utils

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipAddress"></param>
        private void AddIpAddress(string ipAddress, string ruleName)
        {
            if (_cacheEngine.GetCacheObject(ipAddress, out IpAddress blockedIpAddress))
            {
                blockedIpAddress.Increase();

                bool shouldBeBlocked = blockedIpAddress.IsBlocked();

                if (shouldBeBlocked)
                {
                    // Block access
                    SetRule(ipAddress, ruleName, RuleSetMode.Add);
                }
            }
            else
            {
                PostEvictionDelegate callback = (key, value, reason, state) =>
                {
                    SetRule(ipAddress, ruleName, RuleSetMode.Remove);
                };

                _cacheEngine.SetCacheObject(ipAddress, new IpAddress(ipAddress), callback);
            }
        }

        /// <summary>
        /// Add a rule in the windows firewall
        /// </summary>
        /// <param name="ipAddress"></param>
        private void SetRule(string ipAddress, string ruleName, RuleSetMode mode)
        {
            // Get rule
            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            INetFwRule2 currentRule = null;

            try
            {
                currentRule = firewallPolicy.Rules.Item(ruleName) as INetFwRule2;
            }
            catch
            {

            }

            try
            {
                INetFwRule2 rule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                rule.Name = ruleName;
                rule.Description = "Block Incoming Connections from IP Address.";
                rule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
                rule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                rule.Enabled = true;
                rule.InterfaceTypes = "All";

                string[] currentIps = GetIpAddresses(currentRule?.RemoteAddresses);
            
                if (mode == RuleSetMode.Add)
                {
                    int ipSize = currentIps.Length + 1;
                    string[] remoteAdresses = new string[ipSize];
                    Array.Copy(currentIps, remoteAdresses, currentIps.Length);
                    remoteAdresses[ipSize - 1] = ipAddress;

                    rule.RemoteAddresses = GetIpAddresses(remoteAdresses);
                }
                else if (mode == RuleSetMode.Remove)
                {
                    int index = Array.FindIndex(currentIps, ip => ip == ipAddress);

                    if (index < 0)
                    {
                        return;
                    }

                    string[] remoteAdresses = RemoveAt(currentIps, index);

                    rule.RemoteAddresses = GetIpAddresses(remoteAdresses);
                }


                // Delete old rule
                if (currentRule != null)
                {
                    firewallPolicy.Rules.Remove(ruleName);
                }

                if (string.IsNullOrEmpty(rule.RemoteAddresses))
                {
                    return;
                }

                // Add rule in the firewall
                firewallPolicy.Rules.Add(rule);
            }
            catch
            {

            }
        }

        private static string[] GetIpAddresses(string ipAddresses)
        {
            if (string.IsNullOrEmpty(ipAddresses))
            {
                return new string[0];
            }

            return ipAddresses.Split(',');
        }

        private static string GetIpAddresses(string[] ipAddresses)
        {
            return string.Join(IP_SEPARATOR, ipAddresses);
        }

        public static string[] RemoveAt(string[] source, int index)
        {
            string[] dest = new string[source.Length - 1];

            if (index > 0)
            {
                Array.Copy(source, 0, dest, 0, index);
            }

            if (index < source.Length - 1)
            {
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);
            }

            return dest;
        }

        private void ResetRules()
        {
            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            firewallPolicy.Rules.Remove(SQL_RULE_NAME);
            firewallPolicy.Rules.Remove(RDP_RULE_NAME);
        }

        #endregion
    }
}

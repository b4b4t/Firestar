using System;
using System.Linq;
using System.Windows.Forms;

namespace Firestar
{
    public partial class Status : Form
    {
        private readonly CacheEngine _cacheEngine;

        public Status(CacheEngine cacheEngine)
        {
            _cacheEngine = cacheEngine;

            InitializeComponent();

            RefreshStatus();
        }

        private void RefreshStatus()
        {
            UpdateStatus(null, null);

            var timer = new Timer();
            timer.Tick += new EventHandler(UpdateStatus);
            timer.Interval = 5000;
            timer.Start();
        }

        private void UpdateStatus(object? sender, EventArgs e)
        {
            int nbBlockedIp = 0;

            foreach (string entry in _cacheEngine.GetAllEntries())
            {
                if (_cacheEngine.GetCacheObject(entry, out IpAddress ipAddess) && ipAddess.IsBlocked())
                {
                    nbBlockedIp++;
                }
            }

            NbBlockedIps.Text = nbBlockedIp.ToString();
        }
    }
}

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Firestar
{
    public class FirestarApplicationContext : ApplicationContext
    {
        private readonly CacheEngine _cacheEngine;
        private FirestarEngine _firestarEngine;

        private readonly NotifyIcon notifyIcon = new NotifyIcon();

        public FirestarApplicationContext(CacheEngine cacheEngine)
        {
            _cacheEngine = cacheEngine;

            CreateBlocker();

            ToolStripMenuItem statusMenuItem = new ToolStripMenuItem("Status");
            statusMenuItem.Click += new EventHandler(DisplayStatus);
            
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit");
            exitMenuItem.Click += new EventHandler(Exit);

            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(statusMenuItem);
            contextMenu.Items.Add(exitMenuItem);
            
            notifyIcon.Icon = new Icon("icon.ico");
            notifyIcon.ContextMenuStrip = contextMenu;
            notifyIcon.Visible = true;
        }

        void CreateBlocker()
        {
            var thread = new Thread(StartBlocker);
            thread.IsBackground = true;
            thread.Start();
        }

        void StartBlocker()
        {
            _firestarEngine = new FirestarEngine(_cacheEngine);
            _firestarEngine.EventSubscription();
            _firestarEngine.SqlServerEventSubscription();
        }

        void DisplayStatus(object sender, EventArgs e)
        {
            Status statusView = new Status(_cacheEngine);
            statusView.Visible = true;
        }
        
        void Exit(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            System.Windows.Forms.Application.Exit();
        }
    }
}

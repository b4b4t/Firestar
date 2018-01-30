using System.Net;
using System.ServiceProcess;
using System.Threading;

namespace Firestar
{
    public partial class FirestarService : ServiceBase
    {
        private readonly FirestarEngine _firestarEngine = null;

        public FirestarService()
        {
            InitializeComponent();

            // Start firestar
            _firestarEngine = new FirestarEngine();
        }

        protected override void OnStart(string[] args)
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 60 * 1000;
            timer.Elapsed += TimerElapsed;
            timer.Start();
        }

        protected override void OnStop()
        {
        }

        /// <summary>
        /// Function to execute after each interval
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _firestarEngine.ReadIpAddresses();
            _firestarEngine.AnalyseIpAddresses();
        }
    }
}

using System.ServiceProcess;

namespace Firestar
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new FirestarService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}

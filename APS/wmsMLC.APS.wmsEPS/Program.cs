using System;
using wmsMLC.General.Services.Service;

namespace wmsMLC.APS.wmsEPS
{
    class Program
    {
        private static void Main()
        {
            var starter = new HostStarter("EPS", new EpsHostFactory());
            starter.SetParametersFromCommandLine(Environment.CommandLine);
            starter.Description = Properties.Resources.AppDescription;
            starter.Start();
        }
    }
}

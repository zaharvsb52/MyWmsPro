using System;
using wmsMLC.General.Services.Service;

namespace wmsMLC.APS.wmsSDCL
{
    class Program
    {
        private static void Main()
        {
            var starter = new HostStarter("SDCL", new SdclHostFactory());
            starter.SetParametersFromCommandLine(Environment.CommandLine);
            starter.Description = Properties.Resources.AppDescription;
            starter.Start();
        }
    }
}

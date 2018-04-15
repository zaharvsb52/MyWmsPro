using System;
using wmsMLC.General.Services.Service;

namespace wmsMLC.APS.wmsRS
{
    class Program
    {
        static void Main()
        {
            var starter = new HostStarter("RS", new RsHostFactory());
            starter.SetParametersFromCommandLine(Environment.CommandLine);
            starter.Description = Properties.Resources.AppDescription;
            starter.Start();
        }
    }
}

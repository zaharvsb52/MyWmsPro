using System;
using wmsMLC.General.Services.Service;

namespace wmsMLC.APS.wmsSI
{
    class Program
    {
        static void Main(string[] args)
        {
            var starter = new HostStarter("SI", new IntegrationServiceHostFactory());
            starter.SetParametersFromCommandLine(Environment.CommandLine);
            starter.Description = Properties.Resources.AppDescription;
            starter.Start();
        }
    }
}

using System;
using System.IO;
using log4net;
using log4net.Config;

namespace wmsMLC.General
{
    public static class Log4NetHelper
    {
        public const string DefaultConfigFileName = "log4net.config";
        public const string SpecConfigFileTemplate = "log4net.{0}.config";
        public const string ServiceNamePropertyName = "ServiceName";

        public static void Configure(string name)
        {
            var workpath = AppDomain.CurrentDomain.BaseDirectory;
            //Комментарий: если будут проблемы с использованием AppDomain.CurrentDomain.BaseDirectory - заменить на метод Assembly.GetEntryAssembly().Location или Assembly.GetExecutingAssembly().Location.

            var fileName = Path.Combine(workpath, string.Format(SpecConfigFileTemplate, name));
            if (!File.Exists(fileName))
                fileName = Path.Combine(workpath, DefaultConfigFileName);

            if (string.IsNullOrEmpty(fileName))
                XmlConfigurator.Configure();
            else
                XmlConfigurator.ConfigureAndWatch(new FileInfo(fileName));
        }

        public static object Get(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return null;
            return GlobalContext.Properties[propertyName];
        }

        public static void Set(string propertyName, object value)
        {
            if (string.IsNullOrEmpty(propertyName))
                return;
            GlobalContext.Properties[propertyName] = value;
        }

        public static void SetServiceName(string serviceName)
        {
            GlobalContext.Properties[ServiceNamePropertyName] = serviceName;
        }
    }
}
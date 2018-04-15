namespace wmsMLC.DCL.Configurator.Helpers
{
    public static class ConfiguratorHelper
    {
        public static string CreatePmAllowedMethodsKey(string operationCode, string objectEntityCode, string objectName)
        {
            return string.Format("'{0}'_'{1}'_'{2}'", operationCode, objectEntityCode, objectName);
        }

        public static string CreateAllowedDetailsPmMethodKey(string operationCode, string objectEntityCode, string objectName, string methodCode, string property)
        {
            return string.Format("{0}_'{1}'_'{2}'", CreatePmAllowedMethodsKey(operationCode: operationCode, objectName: objectName,
                    objectEntityCode: objectEntityCode), methodCode, property);
        }
    }
}

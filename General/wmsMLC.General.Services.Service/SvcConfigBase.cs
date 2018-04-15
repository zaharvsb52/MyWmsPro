namespace wmsMLC.General.Services.Service
{
    public class SvcConfigBase : ConfigBase
    {
        public const string ParamEndpoint = "endpoint";
        public const string ParamHttp = "http";

        public SvcConfigBase(ServiceContext context) : base(context)
        {
        }

        public string Endpoint
        {
            get { return Get<string>(ParamEndpoint); }
        }

        public string Http
        {
            get { return Get<string>(ParamHttp); }
        }
    }
}
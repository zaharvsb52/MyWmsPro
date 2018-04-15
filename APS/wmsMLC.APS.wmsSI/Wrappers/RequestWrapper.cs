using System;

namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class RequestWrapper : BaseWrapper
    {
        public string MandantCode { get; set; }
        public string ENTITY { get; set; }
        public string FILTER { get; set; }
        public string VALUES { get; set; }
    }
}

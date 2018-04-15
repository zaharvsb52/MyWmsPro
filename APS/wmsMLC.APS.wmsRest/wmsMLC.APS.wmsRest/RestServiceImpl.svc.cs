using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace wmsMLC.APS.wmsRest
{
    public class RestServiceImpl : IRestServiceImpl
    {
        public string XMLData(string id)
        {
            return "You requested product" + id;
        }
    }
}

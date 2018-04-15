using System;
using System.Collections.Generic;

namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class WorkerWrapper : BaseWrapper
    {
        #region . Properties .
        public Boolean WORKEREMPLOYEE { get; set; }
        public Decimal? WORKERID { get; set; }
        public String WORKERPHONEWORK { get; set; }
        public String WORKERPHONEMOBILE { get; set; }
        public String WORKERNAME { get; set; }
        public String WORKERMIDDLENAME { get; set; }
        public String WORKERLASTNAME { get; set; }
        public String WORKEREMAILWORK { get; set; }
        public String WORKEREMAILPERSONAL { get; set; }

        public List<WorkerGpvWrapper> GLOBALPARAMVAL { get; set; }
        public List<AddressBookWrapper> WORKERADDRESS { get; set; }
        public List<WorkerPassWrapper> WORKERPASSL { get; set; }
        #endregion
    }
}

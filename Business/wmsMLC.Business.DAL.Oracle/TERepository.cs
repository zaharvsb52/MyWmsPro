using System;
using BLToolkit.DataAccess;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class TERepository : BaseHistoryRepository<TE, string>, ITERepository
    {
        private const string PkgTEName = "pkgTE";

        [SprocName(PkgTEName + "." + "chngTEStatus")]
        [DiscoverParameters(false)]
        public abstract void ChangeStatusInternal(string teCode, string operationCode);

        public void ChangeStatus(string teCode, string operation)
        {
            ChangeStatusInternal(teCode, operation);
        }

        protected override string GetPakageName(Type type)
        {
            return PkgTEName;
        }
    }
}

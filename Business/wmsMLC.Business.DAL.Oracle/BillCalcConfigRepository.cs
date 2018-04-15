using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class BillCalcConfigRepository : XamlRepository<BillCalcConfig, decimal>
    {
        protected BillCalcConfigRepository()
        {
            PkgName = "pkgBillCalcConfig";
            GetName = "getBillCalcConfigSQL";
            UpdName = "updBillCalcConfigSQL";
        }
    }
}
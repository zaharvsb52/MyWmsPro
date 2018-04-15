using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class DashboardRepository : XamlRepository<Dashboard, string> 
    {
        protected DashboardRepository()
        {
            PkgName = "pkgDashboard";
            GetName = "getDashboardBody";
            UpdName = "updDashboardBody";
        }
    }
}
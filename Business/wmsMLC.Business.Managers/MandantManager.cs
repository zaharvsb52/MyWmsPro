using wmsMLC.Business.Objects;

namespace wmsMLC.Business.Managers
{
    /// <summary>
    /// Менеджер мандантов.
    /// </summary>
    public class MandantManager : WMSBusinessObjectManager<Mandant, decimal>, IMandantHandler
    {
        public decimal? GetMandantCode(WMSBusinessObject bo)
        {
            return bo == null ? null : bo.GetKey() as decimal?;
        }
    }
}

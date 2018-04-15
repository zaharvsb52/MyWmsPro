using wmsMLC.Business.Objects;

namespace wmsMLC.Business.Managers
{
    /// <summary>
    /// Менеджер партнёров
    /// </summary>
    public class PartnerManager : WMSBusinessObjectManager<Partner, decimal>, IMandantHandler
    {
        public decimal? GetMandantCode(WMSBusinessObject bo)
        {
            var entity = bo as Partner;
            return entity == null ? null : entity.MandantId;
        }
    }
}
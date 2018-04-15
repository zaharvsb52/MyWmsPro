using wmsMLC.Business.Objects;

namespace wmsMLC.Business.Managers
{
    public interface IPosManager
    {
        void FillMandantAndFactory(WMSBusinessObject entity);
        void FillFromSku(WMSBusinessObject entity);
    }
}
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.Managers
{
    public class IWBPosManager : WMSBusinessObjectManager<IWBPos, decimal>, IPosManager
    {
        public void FillMandantAndFactory(WMSBusinessObject entity)
        {
            var obj = entity as IWBPos;
            if (obj == null)
                return;

            if (obj.IWBID_R == null)
            {
                obj.MandantID = null;
                obj.FactoryID_R = null;
                return;
            }

            IWB iwb = null;
            using (var mgr = GetManager<IWB>())
                iwb = mgr.Get(obj.IWBID_R);
            obj.MandantID = iwb == null ? null : iwb.MandantID;
            obj.FactoryID_R = iwb == null ? null : iwb.FactoryID_R;
        }

        public void FillFromSku(WMSBusinessObject entity)
        {
            var obj = entity as IWBPos;
            if (obj == null)
                return;

            SKU sku = null;
            using (var mgr = GetManager<SKU>())
                sku = mgr.Get(obj.SKUID);
            if (sku == null)
            {
                obj.VArtName = null;
                obj.VArtDesc = null;
                obj.VMeasureName = null;
                obj.IWBPosCount2SKU = 0;
                return;
            }

            obj.VArtName = sku.ArtCode;
            obj.VArtDesc = sku.VArtDesc;
            obj.VMeasureName = sku.VMeasureName;
            obj.IWBPosCount2SKU = sku.SKUCount;
        }
    }
}
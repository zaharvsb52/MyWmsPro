using wmsMLC.Business.Objects;

namespace wmsMLC.Business.Managers
{
    public class OWBPosManager : WMSBusinessObjectManager<OWBPos, decimal>, IPosManager
    {
        public void FillMandantAndFactory(WMSBusinessObject entity)
        {
            var obj = entity as OWBPos;
            if (obj == null)
                return;

            if (obj.OWBID_R == null)
            {
                obj.MandantID = null;
                obj.FactoryID_R = null;
                return;
            }

            OWB owb = null;
            using (var mgr = GetManager<OWB>())
                owb = mgr.Get(obj.OWBID_R);
            obj.MandantID = owb == null ? null : owb.MandantID;
            obj.FactoryID_R = owb == null ? null : owb.FactoryID_R;
        }

        public void FillFromSku(WMSBusinessObject entity)
        {
            var obj = entity as OWBPos;
            if (obj == null)
                return;

            obj.VArtName = null;
            obj.VArtDesc = null;
            obj.VMeasureName = null;

            if (obj.SKUID == null)
                return;

            SKU sku = null;
            using (var mgr = GetManager<SKU>())
                sku = mgr.Get(obj.SKUID);
            if (sku == null)
                return;

            obj.VArtName = sku.ArtCode;
            obj.VArtDesc = sku.VArtDesc;
            obj.VMeasureName = sku.VMeasureName;
        }
    }
}
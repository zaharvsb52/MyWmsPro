using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL;

namespace wmsMLC.APS.wmsSI.Helpers
{
    public static class BarcodeHelper
    {
        public const string CheckBarcodeMessage = "Проверка ШК";
        private const string SkuEntityName = "SKU";

        public static bool ValidateBarcode(decimal? skuid, string barcodeValue, IUnitOfWork uow)
        {
            if (!skuid.HasValue)
                throw new DeveloperException(CheckBarcodeMessage + ". Не определен ид. SKU.");

            //if (string.IsNullOrEmpty(barcode))
            //    throw new IntegrationService.IntegrationLogicalException(CheckBarcodeMessage + ". Не определен ШК.");

            var type = typeof (Barcode);
            var filter = string.Format("{0} = '{1}' AND {2} = '{3}' and {4} = '{5}'",
                SourceNameHelper.Instance.GetPropertySourceName(type, Barcode.BarcodeEntityPropertyName),
                SkuEntityName,
                SourceNameHelper.Instance.GetPropertySourceName(type, Barcode.BarcodeKeyPropertyName),
                skuid,
                SourceNameHelper.Instance.GetPropertySourceName(type, Barcode.BarcodeValuePropertyName),
                barcodeValue);

            Barcode[] barcodes;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Barcode>>())
            {
                if (uow != null)
                    mgr.SetUnitOfWork(uow);

                barcodes = mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();
            }

            return barcodes.Length != 0;
        }

        public static void AddBarcode(decimal? skuid, string barcodeValue, IUnitOfWork uow)
        {
            if (!skuid.HasValue || string.IsNullOrEmpty(barcodeValue))
                return;

            var barcodeEntity = new Barcode
            {
                BarcodeEntity = SkuEntityName,
                BarcodeValue = barcodeValue,
                BarcodeKey = skuid.ToString()
            };

            using (var mgr = IoC.Instance.Resolve<IBaseManager<Barcode>>())
            {
                if (uow != null)
                    mgr.SetUnitOfWork(uow);

                mgr.Insert(ref barcodeEntity);
            }
        }
    }
}

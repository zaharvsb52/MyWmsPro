using DevExpress.XtraEditors.DXErrorProvider;
using wmsMLC.Business.Objects;
using wmsMLC.General.BL;
using WebClient.Common.Types;

namespace wmsMLC.DCL.Content.Acceptance.ViewModels
{
    [SysObjectName("IWBPosInput")]
    public class AcceptanceItemInfo : IWBPosInput, IDXDataErrorInfo
    {
        private EntityReference _place;

        public AcceptanceItemInfo()
        {
        }

        public AcceptanceItemInfo(IWBPosInput iwbPosInput)
        {
            if (iwbPosInput == null)
                return;

            try
            {
                SuspendNotifications();
                Copy(iwbPosInput, this);

                if (iwbPosInput.PlaceCode != null)
                    Place = new EntityReference(iwbPosInput.PlaceCode, wmsMLC.Business.Objects.Place.EntityType, new[]
                    {
                        new EntityReferenceFieldValue("PlaceCode", iwbPosInput.PlaceCode),
                        // пока нет виртуалки на имя места - не можем построить нормальный EntityReference. Подставляем пока PlaceCode
                        new EntityReferenceFieldValue("PlaceName", iwbPosInput.PlaceCode)
                    });

                AcceptChanges();
            }
            finally
            {
                ResumeNotifications();
            }
        }

        public override object Clone()
        {
            var res = (AcceptanceItemInfo) base.Clone();
            res.Place = Place;
            return res;
        }

        /// <summary>
        /// Место приемки. EntityReference
        /// </summary>
        public EntityReference Place
        {
            get { return _place; }
            set
            {
                if (_place == value)
                    return;

                var oldValue = _place;
                _place = value;
                OnPropertyChanged("Place");
                OnPlaceChanged(oldValue, _place);
            }
        }

        /// <summary>
        /// Признак того, что у записи есть клоны
        /// </summary>
        public bool HasChildren { get; set; }

        private void OnPlaceChanged(EntityReference oldValue, EntityReference newValue)
        {
            // синхронизируем с местом
            PlaceCode = newValue != null ? (string) newValue.Id : null;
        }

        void IDXDataErrorInfo.GetPropertyError(string propertyName, ErrorInfo info)
        {
            var errorText = this[propertyName];
            if (!string.IsNullOrEmpty(errorText))
            {
                info.ErrorText = errorText;
                info.ErrorType = ErrorType.Information;
            }
        }

        void IDXDataErrorInfo.GetError(ErrorInfo info)
        {
            if (string.IsNullOrEmpty(BatchcodeErrorMessage))
            {
                SetErrorInfo(info, null, ErrorType.None);
                return;
            }

            SetErrorInfo(info, BatchcodeErrorMessage,
                string.IsNullOrEmpty(IWBPosInputBatch) ? ErrorType.Information : ErrorType.Critical);
        }

        private void SetErrorInfo(ErrorInfo info, string errorText, ErrorType errorType)
        {
            info.ErrorText = errorText;
            info.ErrorType = errorType;
        }
    }
}
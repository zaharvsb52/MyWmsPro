using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.Activities.Business
{
    public class IsBlockedPlaceActivity : NativeActivity<bool>
    {
        #region .  Arguments  .

        [DisplayName(@"Показать сообщение")]
        public bool ShowDialog { get; set; }

        [DisplayName(@"Код места")]
        public InArgument<string> PlaceCode { get; set; }

        [DisplayName(@"Размер шрифта")]
        [DefaultValue(14)]
        public InArgument<double> FontSize { get; set; }

        [DisplayName(@"Операция")]
        public PlaceOperationEnum Operation { get; set; }

        #endregion

        private double _fontSize;

        public IsBlockedPlaceActivity()
        {
            DisplayName = @"Проверка блокировки места";
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, PlaceCode, type.ExtractPropertyName(() => PlaceCode));
            ActivityHelpers.AddCacheMetadata(collection, metadata, FontSize, type.ExtractPropertyName(() => FontSize));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var placeCode = PlaceCode.Get(context);
            _fontSize = FontSize.Get(context);
            try
            {
                if (string.IsNullOrEmpty(placeCode))
                    throw new NullReferenceException("Не указан код места");
                using (var mgr = IoC.Instance.Resolve<IBaseManager<Place2Blocking>>())
                {
                    var blocking = mgr.GetFiltered(string.Format("{0}='{1}'",
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(Place2Blocking), Place2Blocking.Place2BlockingPlaceCodePropertyName),
                        placeCode)).ToArray();
                    if (blocking.Length > 0)
                    {
                        Place2Blocking[] actualBlocks;
                        switch (Operation)
                        {
                            case PlaceOperationEnum.IN:
                                actualBlocks =
                                    blocking.Where(
                                        i => Equals(i.Place2BlockingBlockingCode, PlaceBlockingEnum.PLACE_BAN.ToString()) || Equals(i.Place2BlockingBlockingCode, PlaceBlockingEnum.PLACE_BAN_IN.ToString()))
                                        .ToArray();
                                break;
                            case PlaceOperationEnum.OUT:
                                actualBlocks =
                                    blocking.Where(
                                        i => Equals(i.Place2BlockingBlockingCode, PlaceBlockingEnum.PLACE_BAN.ToString()))
                                        .ToArray();
                                break;
                            default:
                                actualBlocks = blocking;
                                break;
                        }
                        if (actualBlocks.Length == 0)
                            Result.Set(context, false);
                        else
                        {
                            Result.Set(context, true);
                            if (ShowDialog)
                            {
                                var placeName = actualBlocks[0].GetProperty(Place2Blocking.VPLACENAMEPropertyName);
                                var message = new StringBuilder();
                                message.AppendFormat("Место '{0}' заблокировано!", placeName);
                                foreach (var b in actualBlocks)
                                    message.AppendFormat("{0}{1}: {2}", System.Environment.NewLine, b.GetProperty(Place2Blocking.VBLOCKINGNAMEPropertyName), b.Place2BlockingDesc);
                                ShowWarningMessage("Предупреждение", message.ToString());
                            }
                        }
                    }
                    else
                        Result.Set(context, false);
                }
            }
            catch (Exception ex)
            {
                if (ShowDialog)
                    ShowErrorMessage(ex);
                Result.Set(context, false);
            }
        }

        private void ShowErrorMessage(Exception ex)
        {
            var viewService = IoC.Instance.Resolve<IViewService>();
            viewService.ShowDialog("Ошибка", BPH.GetInnerException(ex), System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error, System.Windows.MessageBoxResult.OK, _fontSize);
        }

        private void ShowWarningMessage(string title, string message)
        {
            var viewService = IoC.Instance.Resolve<IViewService>();
            viewService.ShowDialog(title, message, System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning, System.Windows.MessageBoxResult.OK, _fontSize);
        }
    }
}

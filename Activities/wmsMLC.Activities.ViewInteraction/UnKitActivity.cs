using System.Activities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Content.ViewModels;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.Activities.ViewInteraction
{
    public class UnKitActivity : NativeActivity<int>
    {
        #region .  Properties  .
        [DisplayName(@"��������� ������")]
        [DefaultValue(null)]
        public InArgument<Product> Source { get; set; }

        /// <summary>
        /// ��������� �������.
        /// </summary>
        [DisplayName(@"��������� �������")]
        public OutArgument<bool?> DialogResult { get; set; }

        /// <summary>
        /// ��� ���������� ���������.
        /// </summary>
        [DisplayName(@"��� ���������� ���������")]
        public OutArgument<string> SelectedKitCode { get; set; }
        #endregion

        #region . Methods  .

        public UnKitActivity()
        {
            DisplayName = "����� ���������";
        }

        protected override void Execute(NativeActivityContext context)
        {
            var product = Source.Get(context);
            var vs = IoC.Instance.Resolve<IViewService>();

            var obj = new UnKitViewModel()
            {
                PanelCaption = string.Format("����� ���������"),
                SelectedProduct = product
            };

            bool? dialogRes;

            while (true)
            {
                dialogRes = vs.ShowDialogWindow(obj, true, false, "70%", "50%");

                if (dialogRes == null || !dialogRes.Value) break;
                if (obj.SelectedKit == null)
                {
                    vs.ShowDialog("������","�������� ��������",MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    continue;
                }
                SelectedKitCode.Set(context, obj.SelectedKit.KitCode);
                break;
            }

            DialogResult.Set(context, dialogRes);
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, Source, type.ExtractPropertyName(() => Source));
            ActivityHelpers.AddCacheMetadata(collection, metadata, DialogResult, type.ExtractPropertyName(() => DialogResult));
            ActivityHelpers.AddCacheMetadata(collection, metadata, SelectedKitCode, type.ExtractPropertyName(() => SelectedKitCode));

            metadata.SetArgumentsCollection(collection);
        }

        #endregion
    }
}
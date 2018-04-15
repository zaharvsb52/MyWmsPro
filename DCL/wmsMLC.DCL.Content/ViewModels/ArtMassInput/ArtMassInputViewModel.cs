using wmsMLC.DCL.Content.Views;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Resources;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels.ArtMassInput
{
    [View(typeof(ArtMassInputView))]
    public class ArtMassInputViewModel : PanelViewModelBase
    {
        public ArtListMassInputViewModel ArtList { get; set; }

        public ArtMassInputViewModel()
        {
            PanelCaption = StringResources.ArtMassInputCaption;
            PanelCaptionImage = ImageResources.DCLJournal16.GetBitmapImage();
            ArtList = new ArtListMassInputViewModel(true);
            ArtList.BeforeSave += (sender, args) => { WaitIndicatorVisible = true; };
            ArtList.AfterSave += (sender, args) => { WaitIndicatorVisible = false; };
        }
    }
}
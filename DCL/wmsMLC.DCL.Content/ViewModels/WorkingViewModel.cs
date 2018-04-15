using System;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof (ObjectView))]
    public class WorkingViewModel : ObjectViewModelBase<Working>
    {
        protected override void OnSourceChanged()
        {
            base.OnSourceChanged();
            if (Source == null)
                return;

            if (!Source.IsNew)
                return;

            Source.WORKINGFROM = DateTime.Now;
        }

    }
}
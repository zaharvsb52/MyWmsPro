﻿using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectListView))]
    public class ReportRedefinitionListViewModel : ObjectListViewModelBase<ReportRedefinition>
    {
        public ReportRedefinitionListViewModel()
        {
            ShowDetail = true;
        }
    }
}

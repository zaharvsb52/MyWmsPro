using System.Collections.Generic;
using System.Windows.Input;
using DevExpress.Mvvm;
using MLC.Ext.Common.Data;
using MLC.Ext.Wpf.ViewModels;
using MLC.Ext.Wpf.ViewModels.Menu;
using wmsMLC.DCL.Resources;
using wmsMLC.General.PL.WPF;
using WebClient.Common.Client.Protocol.DataTransferObjects.Query;

namespace wmsMLC.DCL.General.ViewModels.Ext
{
    public class EntityJournalHistoryViewModel : EntityJournalViewModel
    {
        private readonly IViewModelFactory _entityViewModelFactory;
        private bool _isArchive;
        private readonly MLC.Ext.Common.Model.Filter _archFilter;

        public EntityJournalHistoryViewModel(string entityType, IDataServiceProxy dataServiceProxy,
            IEntityViewModelFactory entityViewModelFactory, IDocumentManagerService documentManagerService)
            : base(entityType, dataServiceProxy, entityViewModelFactory, documentManagerService)
        {
            _entityViewModelFactory = entityViewModelFactory;
            IsAllowAutoLoadStoreOnInit = false;

            _archFilter = new MLC.Ext.Common.Model.Filter
            {
                Property = "IsArchive",
                Operator = JsFilterOperator.EQ,
                Value = false
            };
        }

        protected override MLC.Ext.Wpf.ViewModels.Menu.MenuViewModel CreateMenu()
        {
            base.CreateMenu();
            var res = new MLC.Ext.Wpf.ViewModels.Menu.MenuViewModel();

            var barEdit = _entityViewModelFactory.Create<BarInfo>();
            barEdit.Name = "Edit";

            var menuRefresh = _entityViewModelFactory.Create<BarButtonInfo>();
            menuRefresh.Caption = StringResources.RefreshData;
            menuRefresh.KeyGesture = new KeyGesture(Key.F5);
            menuRefresh.SmallGlyph = ImageResources.DCLFilterRefresh16.GetBitmapImage();
            menuRefresh.LargeGlyph = ImageResources.DCLFilterRefresh32.GetBitmapImage();
            menuRefresh.Command = new DelegateCommand(Refresh);
            barEdit.Commands.Add(menuRefresh);
            
            var menuIsArchive = _entityViewModelFactory.Create<BarButtonInfo>();
            menuIsArchive.Caption = StringResources.HistoryUseArchive;
            //menuRefresh.KeyGesture = new KeyGesture(Key.F5);
            //menuIsArchive.SmallGlyph = DXImageHelper.GetImageSource(imageReading16X16);
            menuIsArchive.SmallGlyph = ImageResources.DCLReading_16x16.GetBitmapImage();
            menuIsArchive.LargeGlyph = ImageResources.DCLReading_32x32.GetBitmapImage();
            menuIsArchive.Command = new DelegateCommand(() =>
            {
                _isArchive = !_isArchive;
                if (_isArchive)
                {
                    menuIsArchive.Caption = StringResources.HistoryDoNotUseArchive;
                    menuIsArchive.SmallGlyph = ImageResources.DCLReset_16x16.GetBitmapImage();
                    menuIsArchive.LargeGlyph = ImageResources.DCLReset_32x32.GetBitmapImage();
                }
                else
                {
                    menuIsArchive.Caption = StringResources.HistoryUseArchive;
                    menuIsArchive.SmallGlyph = ImageResources.DCLReading_16x16.GetBitmapImage();
                    menuIsArchive.LargeGlyph = ImageResources.DCLReading_32x32.GetBitmapImage();
                }
                RefreshFilters();
            });
            barEdit.Commands.Add(menuIsArchive);

            res.Bars.Add(barEdit);
            return res;
        }

        public override bool CanProcessSelected()
        {
            return false;
        }

        private void RefreshFilters()
        {
            FiltersSet(DefaultStore.FixFilters);
        }

        public override void FiltersSet(IEnumerable<MLC.Ext.Common.Model.Filter> items)
        {
            var filters = new List<MLC.Ext.Common.Model.Filter>(items);
            if (_isArchive)
            {
                if (filters.Contains(_archFilter))
                    filters.Remove(_archFilter);
            }
            else
            {
                if (!filters.Contains(_archFilter))
                    filters.Add(_archFilter);
            }

            base.FiltersSet(filters);
        }
    }
}

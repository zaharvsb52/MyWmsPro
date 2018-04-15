using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Mvvm;
using DevExpress.Utils.Design;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using MLC.Ext.Common.Data;
using MLC.Ext.Common.Model;
using MLC.Ext.Wpf.ViewModels;
using MLC.Ext.Wpf.ViewModels.Menu;
using wmsMLC.Business.Objects;
using WebClient.Common.Client.Model;
using WebClient.Common.Client.Protocol.DataTransferObjects.Query;
using WebClient.Common.Types;

namespace wmsMLC.DCL.Content.Acceptance.ViewModels
{
    public class NewAcceptanceWorkingViewModel : EntityJournalViewModel
    {
        private readonly IEntityViewModelFactory _entityViewModelFactory;

        public decimal? WorkId { get; set; }

        public NewAcceptanceWorkingViewModel(IDataServiceProxy dataServiceProxy,
            IEntityViewModelFactory entityViewModelFactory, IDocumentManagerService documentManagerService)
            : base(Working.EntityType, dataServiceProxy, entityViewModelFactory, documentManagerService)
        {
            _entityViewModelFactory = entityViewModelFactory;

            IsReadOnly = false;
            NewItemRowPosition = NewItemRowPosition.Bottom;
        }

        protected override IEnumerable<IField> CreateFields()
        {
            var res = base.CreateFields();
            return res;
        }

        protected override MenuViewModel CreateMenu()
        {
            var menu = base.CreateMenu();
            var barEdit = menu.Bars.Single(i => i.Name == "Edit");

            var biSave = _entityViewModelFactory.Create<BarButtonInfo>();
            biSave.Caption = "Save";
            biSave.Command = new DelegateCommand(Save);
            biSave.SmallGlyph = DXImageHelper.GetImageSource(DevExpress.Images.DXImages.Save, ImageSize.Size16x16);
            biSave.LargeGlyph = DXImageHelper.GetImageSource(DevExpress.Images.DXImages.Save, ImageSize.Size32x32);
            barEdit.Commands.Add(biSave);

            return menu;
        }

        protected override IDataStore CreateDefaultStore()
        {
            var store = base.CreateDefaultStore();
            store.NewItem += OnNewItem;
            // убираем пейджирование
            //store.PageSize = 0;

            if (WorkId != null)
            {
                var stFilter = new Filter()
                {
                    Property = "Work",
                    Operator = JsFilterOperator.EQ,
                    Value = new EntityReference(WorkId, "WmsWork",
                        new[]
                        {
                            new EntityReferenceFieldValue("WorkID", WorkId)
                        })
                };

                if (store.FixFilters != null)
                {
                    store.FixFilters.ToList().Add(stFilter);
                }
                else
                {
                    store.FixFilters = new List<Filter>()
                    {
                        stFilter
                    };
                }
            }

            return store;
        }

        private int _newItemCounter;
        private void OnNewItem(object sender, NewDataSourceItemEventArgs ea)
        {
            DefaultStore.SetFieldValue(ea.Item, "WorkingID", new EntityId(--_newItemCounter, EntityType));
            DefaultStore.SetFieldValue(ea.Item, "Work", new EntityReference(WorkId, "WmsWork", new EntityReferenceFieldValue[0]));
            DefaultStore.SetFieldValue(ea.Item, "WorkingFrom", DateTime.Now.Date);
            DefaultStore.SetFieldValue(ea.Item, "WorkingMult", 100);
            DefaultStore.SetFieldValue(ea.Item, "Transact", 1);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Content.Views;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof (UnKitView))]
    public class UnKitViewModel : PanelViewModelBase
    {
        #region .  ctors  .

        public UnKitViewModel()
        {
            FillFields();
        }

        #endregion

        #region .  Fields & consts  .

        private ObservableCollection<Kit> _availableKits;
        private ObservableCollection<KitPos> _kitItems;
        private Kit _selectedKit;
        private Product _selectedProduct;
        private string _skuNameSkuCount;

        public const string AvailableKitsPropertyName = "AvailableKits";
        public const string SelectedKitPropertyName = "SelectedKit";
        public const string KitItemsPropertyName = "KitItems";

        #endregion

        #region  .  Properties  .

        public Kit SelectedKit
        {
            get { return _selectedKit; }
            set
            {
                if (_selectedKit == value)
                    return;

                _selectedKit = value;
                OnPropertyChanged(SelectedKitPropertyName);
                OnSelectedKitChanged();
            }
        }

        public Product SelectedProduct
        {
            get { return _selectedProduct; }
            set
            {
                if (_selectedProduct != null && _selectedProduct == value)
                    return;
                _selectedProduct = value;

                OnSelectedProductChanged();
            }
        }

        private void OnSelectedProductChanged()
        {
            RefreshBoxListAsync();
        }

        public ObservableCollection<KitPos> KitItems
        {
            get { return _kitItems; }
            set
            {
                _kitItems = value;
                OnPropertyChanged(KitItemsPropertyName);
            }
        }

        public ObservableCollection<Kit> AvailableKits
        {
            get { return _availableKits; }
            set
            {
                _availableKits = value;
                OnPropertyChanged(AvailableKitsPropertyName);
            }
        }

        public ObservableCollection<DataField> KitFields { get; set; }

        public ObservableCollection<DataField> KitItemsFields { get; set; }

        #endregion

        #region  .  Methods  .

        private void FillFields()
        {
            string[] kitFields = {"KITCODE","VARTNAME"};
            string[] kitPosFields = {"VSKUNAME", "KITPOSCOUNT" };

            KitFields = GetFilteredDataFields(typeof(Kit), SettingDisplay.List, kitFields);
            KitItemsFields = GetFilteredDataFields(typeof(KitPos), SettingDisplay.List, kitPosFields);
        }

        private static ObservableCollection<DataField> GetFilteredDataFields(Type type, SettingDisplay settingDisplay, IEnumerable<string> filterFileds)
        {
            var bufferCOllection = DataFieldHelper.Instance.GetDataFields(type, settingDisplay).Where(i => filterFileds.Contains(i.FieldName));
            var retColl = new ObservableCollection<DataField>();

            if (!bufferCOllection.Any()) return retColl;
            foreach (var dataField in bufferCOllection.OrderBy(x => x.FieldName == "VSKUNAME" ? 0 : 1))
            {
                retColl.Add(dataField);
            }
            return retColl;
        }

        private void OnSelectedKitChanged()
        {
            RefreshBoxKitPosListAsync();
            RiseCommandsCanExecuteChanged();
        }

        private async void RefreshBoxListAsync()
        {
            WaitStart();
            try
            {
                var items = await Task.Factory.StartNew<IEnumerable<Kit>>(() =>
                {
                    using (var mgr = GetManager<Kit>())
                    {
                        if (SelectedProduct == null)
                            return null;
                        return mgr.GetFiltered(string.Format(" ARTCODE_R = '{0}'",SelectedProduct.ArtCode_R));
                    }
                });
                AvailableKits = new ObservableCollection<Kit>(items);
                SelectedKit = AvailableKits.FirstOrDefault();
            }
            finally
            {
                WaitStop();
            }
        }

        private async void RefreshBoxKitPosListAsync()
        {
            WaitStart();
           
            try
            {
              var items = await Task.Factory.StartNew<IEnumerable<KitPos>>(() =>
                {
                    using (var mgr = GetManager<KitPos>())
                    {
                        return mgr.GetFiltered(string.Format("KITCODE_R = '{0}' ", SelectedKit.KitCode));
                    }
                });
              KitItems = new ObservableCollection<KitPos>(items);
            }
            finally
            {
                WaitStop();
            }
        }

        private static IBaseManager<T> GetManager<T>(IUnitOfWork uow = null) where T : WMSBusinessObject
        {
            var mgr = IoC.Instance.Resolve<IBaseManager<T>>();
            if (uow != null)
                mgr.SetUnitOfWork(uow);
            return mgr;
        }

        #endregion
    }
}
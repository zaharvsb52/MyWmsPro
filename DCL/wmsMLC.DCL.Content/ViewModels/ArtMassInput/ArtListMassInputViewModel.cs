using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.DCL.Content.ViewModels.ArtMassInput
{
    public class ArtListMassInputViewModel : ListMassInputViewModel<ArtItem>
    {
        public ArtListMassInputViewModel(bool canHandleHotKeys)
            : base(canHandleHotKeys)
        {
        }

        public ICommand SaveCommand { get; private set; }

        public event EventHandler<EventArgs> BeforeSave;
        public event EventHandler<EventArgs> AfterSave;

        protected override MenuViewModel CreateMenuViewModel()
        {
            //Используем глобальные настройки вида панели инструментов
            return new MenuViewModel(string.Empty);
        }

        protected override List<CommandMenuItem> CreateCommandMenuItems()
        {
            var baseItems = base.CreateCommandMenuItems();
            foreach (var p in baseItems)
            {
                p.GlyphSize = GlyphSizeType.Default;
            }

            SaveCommand = new DelegateCustomCommand(Save);
            var save = new CommandMenuItem
            {
                Caption = StringResources.Save,
                Command = SaveCommand,
                HotKey = CanHandleHotKeys ? new KeyGesture(Key.F6) : null,
                ImageSmall = ImageResources.DCLSave16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLSave32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                Priority = 30
            };
            baseItems.Add(save);
            return baseItems;
        }

        protected override bool ConfirmDelete()
        {
            return true;
        }

        protected virtual bool ExceptionHandler(Exception ex, string message)
        {
            if (ex == null)
                return true;

            var result = new OperationException(message, ex);
            return ExceptionPolicy.Instance.HandleException(result, "PL");
        }

        protected virtual void OnBeforeSave()
        {
            var ev = BeforeSave;
            if (ev != null)
                ev(this, EventArgs.Empty);
        }

        protected virtual void OnAfterSave()
        {
            var ev = AfterSave;
            if (ev != null)
                ev(this, EventArgs.Empty);
        }

        private bool HasChanges(ArtItem artItem)
        {
            if (artItem.IsDirty)
                return true;

            if (artItem.Art2GroupList.Items.IsDirty)
                return true;

            if (artItem.SKUList.Items.IsDirty)
                return true;

            foreach (var skuItem in artItem.SKUList.Items)
            {
                if (skuItem.SKU2TTEList.Items.IsDirty)
                    return true;

                if (skuItem.ArtPriceList.Items.IsDirty)
                    return true;

                if (skuItem.SKUBCList.Items.IsDirty)
                    return true;
            }

            return false;
        }

//        private void AcceptChanges(ArtItem artItem)
//        {
//            artItem.AcceptChanges();
//
//            artItem.Art2GroupList.Items.Clean();
//            foreach (var art2Group in artItem.Art2GroupList.Items)
//            {
//                art2Group.AcceptChanges();
//            }
//
//            artItem.SKUList.Items.Clean();
//            foreach (var skuItem in artItem.SKUList.Items)
//            {
//                skuItem.SKU2TTEList.Items.Clean();
//                foreach (var sku2Tte in skuItem.SKU2TTEList.Items)
//                {
//                    sku2Tte.AcceptChanges();
//                }
//
//                skuItem.ArtPriceList.Items.Clean();
//                foreach (var artPrice in skuItem.ArtPriceList.Items)
//                {
//                    artPrice.AcceptChanges();
//                }
//
//                skuItem.SKUBCList.Items.Clean();
//                foreach (var skubc in skuItem.SKUBCList.Items)
//                {
//                    skubc.AcceptChanges();
//                }
//            }
//        }

        private void DoSave()
        {
            var isValid = true;
            foreach (var artItem in Items)
            {
                isValid = isValid && Validate(GetArtFromItem(artItem));
            }

            if (!isValid)
            {
                GetViewService().ShowDialog
                    (StringResources.Error
                        , string.Format("{0}{1}{2}", StringResources.ErrorSave, Environment.NewLine, StringResources.FormContainsErrors)
                        , MessageBoxButton.OK
                        , MessageBoxImage.Error
                        , MessageBoxResult.Yes);

                return;
            }

            var factory = IoC.Instance.Resolve<IUnitOfWorkFactory>();
            IUnitOfWork uow = null;
            try
            {
                using (uow = factory.Create(false))
                {
                    uow.BeginChanges();

                    var artsKeyToReReadFromDb = new List<object>();
                    foreach (var artItem in Items)
                    {
                        if (!HasChanges(artItem))
                        {
                            artsKeyToReReadFromDb.Add(artItem.GetKey());
                            continue;
                        }
                        
                        var art = GetArtFromItem(artItem);
                        art = SaveOrUpdate(art, uow);
                        artsKeyToReReadFromDb.Add(art.GetKey());
                    }

                    uow.CommitChanges();

                    DispatcherHelper.Invoke(new Action(() => Items.Clear()));

                    using (var mgr = IoC.Instance.Resolve<IBaseManager<Art>>())
                    {
                        foreach (var artKey in artsKeyToReReadFromDb)
                        {
                            var artItem = GetArtItemFromArt(mgr.Get(artKey, GetModeEnum.Full));
                            artItem.AcceptChanges();
                            DispatcherHelper.Invoke(new Action(() => Items.Add(artItem)));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (uow != null)
                {
                    uow.RollbackChanges();
                    uow.Dispose();
                }
                if (!ExceptionHandler(ex, ExceptionResources.ItemCantSave))
                    throw;
            }
        }

        private T SaveOrUpdate<T>(T entity, IUnitOfWork uow) where T : WMSBusinessObject
        {
            using (var mgr = IoC.Instance.Resolve<IBaseManager<T>>())
            {
                mgr.SetUnitOfWork(uow);

                var cs = entity as ICustomXmlSerializable;
                if (cs != null) cs.OverrideIgnore = false;

                //                var sb = new StringBuilder();
                //                using (var sr = new StringWriter(sb))
                //                using (var xmlWriter = new XmlTextWriter(sr))
                //                    cs.WriteXml(xmlWriter);
                //                var xml = sb.ToString();

                if (!entity.IsPersisted())
                {
                    var ent = entity;
                    mgr.Insert(ref ent);
                    return ent;
                }
                else
                {
                    mgr.Update(entity);
                    return entity;
                }
            }
        }

        private void Save()
        {
            OnBeforeSave();
            Task.Factory
                .StartNew(DoSave)
                .ContinueWith(t => OnAfterSave());
        }

        private Art GetArtFromItem(ArtItem item)
        {
            var art = new Art();
            Mapper.Map(item, art);

            art.Art2GroupL = new WMSBusinessCollection<Art2Group>(item.Art2GroupList.Items);
            art.SKUL = new WMSBusinessCollection<SKU>(item.SKUList.Items.Select(GetSkuFromItem));

            return art;
        }

        private SKU GetSkuFromItem(SKUItem item)
        {
            var sku = new SKU();
            Mapper.Map(item, sku);

            sku.SKU2TTEL = new WMSBusinessCollection<SKU2TTE>(item.SKU2TTEList.Items);
            sku.ArtPriceL = new WMSBusinessCollection<ArtPrice>(item.ArtPriceList.Items);
            sku.SKUBCL = new WMSBusinessCollection<SKUBC>(item.SKUBCList.Items);

            return sku;
        }

        private ArtItem GetArtItemFromArt(Art art)
        {
            var artItem = new ArtItem();
            Mapper.Map(art, artItem);

            if (art.Art2GroupL != null) artItem.Art2GroupList.Items.AddRange(art.Art2GroupL);
            if (art.SKUL != null) artItem.SKUList.Items.AddRange(art.SKUL.Select(GetSkuItemFromSku));

            return artItem;
        }

        private SKUItem GetSkuItemFromSku(SKU sku)
        {
            var skuItem = new SKUItem();
            Mapper.Map(sku, skuItem);

            if (sku.SKU2TTEL != null) skuItem.SKU2TTEList.Items.AddRange(sku.SKU2TTEL);
            if (sku.ArtPriceL != null) skuItem.ArtPriceList.Items.AddRange(sku.ArtPriceL);
            if (sku.SKUBCL != null) skuItem.SKUBCList.Items.AddRange(sku.SKUBCL);

            return skuItem;
        }

        private static readonly List<string> _excludedArtFields = new List<string>()
        {
            Art.ARTCODEPropertyName, 
            Art.UserInsPropertyName,
            Art.DateInsPropertyName,
            Art.UserUpdPropertyName,
            Art.DateUpdPropertyName,
            Art.TransactPropertyName,
            Art.SKULPropertyName,
            Art.GROUP2ARTLPropertyName,
            Art.CUSTOMPARAMVALPropertyName
        };

        protected override List<string> ExcludedFields
        {
            get { return _excludedArtFields; }
        }

        private static readonly List<string> _fieldsOrder = new List<string>()
        {
            Art.MANDANTIDPropertyName,
            Art.ArtNamePropertyName,
            Art.ARTDESCPropertyName,
            Art.ARTDESCEXTPropertyName,
            Art.ARTSIZEPropertyName,
            Art.ARTCOLORPropertyName,
            Art.ARTCOLORTONEPropertyName,
            Art.FACTORYID_RPropertyName,
            Art.ARTDANGERLEVELPropertyName,
            Art.ARTDANGERSUBLEVELPropertyName,
            Art.ARTLIFETIMEPropertyName,
            Art.ARTLIFETIMEMEASUREPropertyName,
            Art.ARTTEMPMINPropertyName,
            Art.ARTTEMPMAXPropertyName,
            Art.ARTCOMMERCDAYPropertyName,
            Art.ARTABCDPropertyName,
            Art.ARTINPUTDATEMETHODPropertyName,
            Art.ARTIWBTYPEPropertyName,
            Art.ARTPICKORDERPropertyName,
            Art.ARTHOSTREFPropertyName
        };

        protected override List<string> FieldsOrder
        {
            get { return _fieldsOrder; }
        }

        protected override void New()
        {
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Art>>())
            {
                var art = mgr.New();
                var artItem = new ArtItem();
                Mapper.Map(art, artItem);
                SetDefaultValues(artItem);
                SetPrimaryKeyValue(artItem);
                Validate(artItem);
                Items.Add(artItem);
                OnItemAdded(artItem);
            }
        }
    }
}
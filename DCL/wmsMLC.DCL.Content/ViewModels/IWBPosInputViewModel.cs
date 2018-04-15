using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using log4net;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectView))]
    public class IWBPosInputViewModel : CustomObjectViewModelBase<IWBPosInput>
    {
        #region .  Fields  .

        private ILog _log = LogManager.GetLogger(typeof(IWBPosInputViewModel));

        #endregion .  Fields  .

        public IWBPosInputViewModel()
        {
            BatchProcessCommand = new DelegateCustomCommand(() => { }, CanBatchProcess);
            ShowBatchCommand =
                new DelegateCustomCommand(() => ProcessBatchCode(Source.IWBPosInputBatch, Source, true, ShowBatchComplete));
            ApplyBatchCommand =
                new DelegateCustomCommand(() => ProcessBatchCode(Source.IWBPosInputBatch, Source, false, ApplyBatchComplete));
            ChangeOvxSkuCommand = new DelegateCustomCommand(OnChangeOvxSkuCommand, CanChangeOvxSkuCommand);
        }

        #region .  Properties  .
        public decimal? MandantId { get; set; }
        public bool CanUseBatch { get; set; }
        public Func<bool> CanChangeOvxSkuHanler { get; set; }
        public Action OnChangeOvxSkuHanler { get; set; }
        private DelegateCustomCommand BatchProcessCommand { get; set; }
        private DelegateCustomCommand ShowBatchCommand { get; set; }
        private DelegateCustomCommand ApplyBatchCommand { get; set; }
        public ICommand ChangeOvxSkuCommand { get; private set; }
        #endregion .  Properties  .

        public override void InitializeMenus()
        {
            MenuSuffix = GetType().GetFullNameWithoutVersion();
            base.InitializeMenus();
            InitializeMenu();
        }

        private void InitializeMenu()
        {
            if (Menu == null)
                Menu = new MenuViewModel("iwbposinput");

            if (CanUseBatch)
            {
                var actionMenu = Menu.GetOrCreateBarItem("Actions");
                var btnBatchOps = new SubListMenuItem
                {
                    Caption = StringResources.BatchCodeBarCaption,
                    ImageSmall = ImageResources.DCLBatchProcessFull16.GetBitmapImage(),
                    ImageLarge = ImageResources.DCLBatchProcessFull32.GetBitmapImage(),
                    Command = BatchProcessCommand
                };
                actionMenu.MenuItems.Add(btnBatchOps);

                var btnParseBatchCode = new CommandMenuItem
                {
                    Caption = StringResources.Decrypt,
                    Command = ShowBatchCommand,
                    ImageSmall = ImageResources.DCLBatchProcessParse16.GetBitmapImage(),
                    ImageLarge = ImageResources.DCLBatchProcessParse32.GetBitmapImage()
                };
                btnBatchOps.MenuItems.Add(btnParseBatchCode);

                var btnApplyBatchCode = new CommandMenuItem
                {
                    Caption = StringResources.Accept,
                    Command = ApplyBatchCommand,
                    ImageSmall = ImageResources.DCLBatchProcessApply16.GetBitmapImage(),
                    ImageLarge = ImageResources.DCLBatchProcessApply32.GetBitmapImage()
                };
                btnBatchOps.MenuItems.Add(btnApplyBatchCode);
            }

            var barChangeOvxSku = Menu.GetOrCreateBarItem(StringResources.SkuCaption, 3);
            barChangeOvxSku.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.ChangeOvxSkuCaption,
                Command = ChangeOvxSkuCommand,
                ImageSmall = ImageResources.DCLChangeOvxSku16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLChangeOvxSku32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 2
            });
        }

        private bool CanBatchProcess()
        {
            return Source != null && CanUseBatch && !string.IsNullOrEmpty(Source.IWBPosInputBatch);
        }

        private void ShowBatchComplete(CompleteContext ctx)
        {
            try
            {
                // обработаем ошибку
                if (ctx.Exception != null)
                    throw ctx.Exception;

                // получим распаршеные параметры
                var bpContext = ctx.Parameters[BpContext.BpContextArgumentName] as BpContext;
                if (bpContext == null)
                    throw new DeveloperException("Неопределен BpContext. Проверьте процесс разбора.");

                var desc = bpContext.Get<string>("ParseResult");
                if (string.IsNullOrEmpty(desc))
                    throw new DeveloperException("Описание не может быть пустым. Проверьте процесс разбора.");

                ShowInfo(string.Format("{0}:{1}{2}", Source.IWBPosInputBatch, Environment.NewLine,
                    desc.Replace(';', '\n')));
            }
            catch (Exception ex)
            {
                ShowError(string.Format("Ошибка определения batch-кода '{0}'. {1}", Source.IWBPosInputBatch,
                    ExceptionHelper.GetErrorMessage(ex)));
            }
        }

        private void ApplyBatchComplete(CompleteContext ctx)
        {
            if (ctx.Exception != null)
            {
                var message = string.Format("Ошибка применения batch-кода '{0}'. {1}",
                    Source.IWBPosInputBatch,
                    ExceptionHelper.GetErrorMessage(ctx.Exception));
                ShowError(message);
            }
        }

        private bool CanChangeOvxSkuCommand()
        {
            return CanChangeOvxSkuHanler != null && CanChangeOvxSkuHanler() && OnChangeOvxSkuHanler != null;
        }

        private void OnChangeOvxSkuCommand()
        {
            if (!CanChangeOvxSkuCommand())
                return;
            OnChangeOvxSkuHanler();
        }

        private void ShowError(string message)
        {
            var vs = GetViewService();
            vs.ShowDialog("Ошибка", message, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }

        private void ShowInfo(string message)
        {
            var vs = GetViewService();
            vs.ShowDialog("Информация", message, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
        }

        protected override void OnSourceChanged()
        {
            Source.LastSkuId = Source.SKUID;
            Source.IwbPosCountDouble = Source.IWBPosCount;
            Source.ProductCountSkuDouble = Source.ProductCountSKU;
            Source.CheckIsBaseSKU();
            if (InPropertyEditMode)
            {
                foreach (var item in PropertyEditSource)
                {
                    item.LastSkuId = item.SKUID;
                    item.IwbPosCountDouble = item.IWBPosCount;
                    item.ProductCountSkuDouble = item.ProductCountSKU;
                }
                Source.DisableCalculate = PropertyEditSource.DistinctBy(i => i.SKUID).Count() > 1;
                if (Source.DisableCalculate)
                    Source.OverrideSKU2TTEQuantityMax = PropertyEditSource.Min(i => i.SKU2TTEQuantityMax);
            }
            base.OnSourceChanged();
        }

        protected override void SourceObjectPropertyChanged(object sender,
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.SourceObjectPropertyChanged(sender, e);

            if (Source == null)
                return;

            var editable = Source as IEditable;
            if (editable.IsInRejectChanges)
                return;

            if (e.PropertyName.EqIgnoreCase(IWBPosInput.SKUIDPropertyName))
            {
                if (InPropertyEditMode ||
                    Source.LastSkuId.HasValue && Source.SKUID.HasValue && Source.LastSkuId != Source.SKUID)
                {
                    decimal skuIndex1 = 1;
                    decimal skuIndex2 = 1;
                    try
                    {
                        using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
                        {
                            //INFO: коэффициент пересчета по документу
                            skuIndex1 = (Source.LastSkuId.HasValue & Source.SKUID.HasValue)
                                ? mgr.ConvertSKUtoSKU(Source.LastSkuId.Value, Source.SKUID.Value, 1, (decimal)(Source.IWBPosCount * Source.ProductCount))
                                : 1;
                            //INFO: коэффициент пересчета принятых
                            skuIndex2 = (Source.LastSkuId.HasValue & Source.SKUID.HasValue)
                                ? mgr.ConvertSKUtoSKU(Source.LastSkuId.Value, Source.SKUID.Value, 0, 1)
                                : 1;

                            if (InPropertyEditMode)
                                foreach (var item in PropertyEditSource)
                                {
                                    var itemSkuIndex1 = (item.LastSkuId.HasValue & Source.SKUID.HasValue)
                                        ? mgr.ConvertSKUtoSKU(item.LastSkuId.Value, Source.SKUID.Value, 1, (decimal)(item.IWBPosCount * item.ProductCount))
                                        : 1;
                                    var itemSkuIndex2 = (item.LastSkuId.HasValue & Source.SKUID.HasValue)
                                        ? mgr.ConvertSKUtoSKU(item.LastSkuId.Value, Source.SKUID.Value, 0, 1)
                                        : 1;
                                    CalculateSkuIndex(item, itemSkuIndex1, itemSkuIndex2);
                                    item.LastSkuId = Source.SKUID;
                                }
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Warn(ex.Message);
                        _log.Debug(ex);

                        GetViewService()
                            .ShowDialog("Внимание", "Проверьте настройки единиц учета по принимаемому артикулу.",
                                MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                    }

                    if (InPropertyEditMode)
                    {
                        Source.DisableCalculate = Source.SKUID.HasValue == false &&
                                                  PropertyEditSource.DistinctBy(i => i.SKUID).Count() > 1;
                        if (Source.DisableCalculate)
                            Source.OverrideSKU2TTEQuantityMax = PropertyEditSource.Min(i => i.SKU2TTEQuantityMax);
                        else
                            Source.OverrideSKU2TTEQuantityMax = null;
                        if (PropertyEditSource.DistinctBy(i => i.SKUID).Count() == 1)
                            CalculateSkuIndex(Source, skuIndex1, skuIndex2);
                    }
                    else
                    {
                        CalculateSkuIndex(Source, skuIndex1, skuIndex2);
                    }
                    Source.LastSkuId = Source.SKUID;
                    Source.CheckIsBaseSKU();
                    RefreshView();
                }
            }

            // обновим доступность кнопки разбора batch-ей
            if (e.PropertyName.EqIgnoreCase(IWBPosInput.IWBPosInputBatchCodePropertyName))
                BatchProcessCommand.RaiseCanExecuteChanged();
        }

        private void CalculateSkuIndex(IWBPosInput obj, decimal skuIndex1, decimal skuIndex2)
        {
            obj.IwbPosCountDouble = (double)skuIndex1;
            obj.ProductCountSkuDouble = obj.ProductCountSkuDouble * (double)skuIndex2;

            obj.IwbPosCountDouble = DeltaSku(obj.IwbPosCountDouble);
            obj.ProductCountSkuDouble = DeltaSku(obj.ProductCountSkuDouble);

            obj.IWBPosCount = obj.IwbPosCountDouble;
            obj.ProductCountSKU = obj.ProductCountSkuDouble;
            obj.RemainCount = obj.IwbPosCountDouble - obj.ProductCountSkuDouble > 0
                ? DeltaSku(obj.IwbPosCountDouble - obj.ProductCountSkuDouble)
                : 0;
            // запоминаем актуальное значение, для перевода в другую SKU
            obj.RequiredSKUCount = (decimal)DeltaSku(obj.RemainCount.Value);
        }

        public double DeltaSku(double dlt)
        {
            if (dlt == 0) 
                return dlt;
            const double err = 0.00000001;
            var ret = dlt > Math.Round(dlt) ? 1 - Math.Round(dlt) / dlt : 1 - dlt / Math.Round(dlt);
            return ret < err ? Math.Round(dlt) : dlt;
        }

        public override ObservableCollection<DataField> GetDataFields(SettingDisplay displaySetting)
        {

            var fields = DataFieldHelper.Instance.GetDataFields(typeof(IWBPosInput), displaySetting);

            #region .  SKU filter  .
            var skuField = fields.FirstOrDefault(i => i.Name.EqIgnoreCase(IWBPosInput.SKUIDPropertyName));
            if (skuField != null)
            {
                skuField.LookupFilterExt = string.Empty;

                if (MandantId != null)
                    skuField.LookupFilterExt = string.Format("mandantid = {0}", MandantId.Value);

                if (!string.IsNullOrEmpty(Source.ArtCode))
                {
                    skuField.LookupFilterExt += string.IsNullOrEmpty(skuField.LookupFilterExt)
                        ? string.Format("upper(artcode_r)='{0}'", Source.ArtCode.ToUpper())
                        : string.Format(" and upper(artcode_r)='{0}'", Source.ArtCode.ToUpper());
                }
            }
            #endregion

            #region .  Factory filter  .
            var factoryField = fields.FirstOrDefault(i => i.Name.EqIgnoreCase(IWBPosInput.FACTORYID_RPropertyName));
            if (factoryField != null && MandantId != null)
                factoryField.LookupFilterExt = string.Format("PartnerID_r = {0}", MandantId.Value);
            #endregion

            #region .  TEType filter  .

            if (InPropertyEditMode)
            {
                var teTypeField = fields.FirstOrDefault(i => i.Name.EqIgnoreCase(IWBPosInput.TETypeCodePropertyName));
                var filter = string.Format("1=1 or (TETYPECODE in (select TETYPECODE_R from wmssku2tte where wmssku2tte.SKUID_R in ({0}) group by TETYPECODE_R having count(*) = {1}))", string.Join(",", PropertyEditSource.Select(i => i.GetProperty(IWBPosInput.SKUIDPropertyName))), PropertyEditSource.Count());
                if (teTypeField != null)
                    teTypeField.LookupFilterExt = string.IsNullOrEmpty(teTypeField.LookupFilterExt) ? filter : " and " + filter;
            }

            #endregion

            // не даем редактировать, если SKU базовая
            var productCountField = fields.FirstOrDefault(i => i.Name.EqIgnoreCase(IWBPosInput.ProductCountPropertyName));
            if (productCountField != null)
                productCountField.IsEnabled = InPropertyEditMode ? PropertyEditSource.FirstOrDefault(i => i.IsBaseSKU) == null : !Source.IsBaseSKU;
            return fields;
        }

        private void ProcessBatchCode(string batchCode, IWBPosInput target, bool onlyParse, Action<CompleteContext> completedHandler)
        {
            var batch = GetBatch();
            if (batch == null)
            {
                ShowError("Для принимаемой позиции не удалось определить алгоритм распознования batch-кода.");
                return;
            }

            if (string.IsNullOrEmpty(batch.WorkflowCode))
            {
                ShowError(string.Format("Для принимаемой позиции найден алгоритм ({0}), но в нём не указана реализация.", batch.GetKey()));
                return;
            }

            var bpContext = new BpContext();
            bpContext.Set("BatchCode", batchCode);
            bpContext.Set("Target", target);
            bpContext.Set("OnlyParse", onlyParse);
            bpContext.Set<string>("ParseResult", null);

            var executionContext = new ExecutionContext(batch.WorkflowCode,
                new Dictionary<string, object> { { BpContext.BpContextArgumentName, bpContext } });
            var engine = IoC.Instance.Resolve<IProcessExecutorEngine>(WorkflowProcessExecutorConstants.Workflow);
            engine.Run(context: executionContext, completedHandler: completedHandler);
        }

        private BPBatch GetBatch()
        {
            using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
                return mgr.GetDefaultBatchCode(null, Source.SKUID, Source.ArtCode);
        }
    }
}

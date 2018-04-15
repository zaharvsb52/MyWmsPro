using System;
using System.Activities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using wmsMLC.Activities.General;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Factory;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.Activities.Business
{
    public class CreateTeActivity : NativeActivity<bool>
    {
        private const string LayoutSuffix = "0D22EA57-73C1-4F5C-BC26-00399D6F2EF3";
        #region .  Arguments  .
        
        /// <summary>
        /// Код создаваемой ТЕ
        /// </summary>
        [DisplayName(@"Код ТЕ")]
        [Description("Код создаваемой ТЕ (может быть пустым)")]
        public InOutArgument<string> TeCode { get; set; }
        
        /// <summary>
        /// Код места
        /// </summary>
        [DisplayName(@"Код места")]
        [Description("Код места создаваемой ТЕ")]
        public InArgument<string> PlaceCode { get; set; }

        /// <summary>
        /// ТЕ является упаковкой
        /// </summary>
        [DisplayName(@"Упаковка")]
        [Description("Признак того, что ТЕ является упаковкой")]
        public InArgument<bool> IsPack { get; set; }

        [DisplayName(@"Длина")]
        [Description("Длина создаваемой ТЕ (если пусто, то берется из типа ТЕ)")]
        public InArgument<decimal?> Length { get; set; }

        [DisplayName(@"Ширина")]
        [Description("Ширина создаваемой ТЕ (если пусто, то берется из типа ТЕ)")]
        public InArgument<decimal?> Width { get; set; }

        [DisplayName(@"Высота")]
        [Description("Высота создаваемой ТЕ (если пусто, то берется из типа ТЕ)")]
        public InArgument<decimal?> Height { get; set; }

        [DisplayName(@"Вес тары")]
        [Description("Вес тары создаваемой ТЕ")]
        public InArgument<decimal?> TareWeight { get; set; }

        [DisplayName(@"Итоговый вес")]
        [Description("Итоговый вес создаваемой ТЕ (может быть пусто, если рассчитывается по товару)")]
        public InArgument<decimal?> Weight { get; set; }

        [DisplayName(@"Манданты")]
        [Description("Идентификаторы мандантов через ',', которыми ограничить список типов создаваемой ТЕ")]
        public InArgument<string> Mandants { get; set; }

        [DisplayName(@"Тип ТЕ")]
        [Description("Тип создаваемой ТЕ ()")]
        public InArgument<string> TeTypeCode { get; set; }

        [DisplayName(@"Определить тип ТЕ")]
        [Description("Признак включения механизма автоматического определения типа ТЕ")]
        public InArgument<bool> AutoTeType { get; set; }

        [DisplayName(@"ТЕ")]
        [Description("Созданная ТЕ или полученная по коду")]
        public OutArgument<TE> OutTe { get; set; }

        [DisplayName(@"Размер шрифта")]
        [DefaultValue(14)]
        public InArgument<double> FontSize { get; set; }

        [DisplayName(@"Ошибка")]
        [Description("Сообщение об ошибке")]
        public OutArgument<Exception> ExceptionResult { get; set; }

        [DisplayName(@"ТЕ существует")]
        [Description("Признак того, что ТЕ уже существует")]
        public OutArgument<bool> Exist { get; set; }

        [DisplayName(@"Фильтр типов ТЕ")]
        [Description("Дополнительный фильтр по типам ТЕ")]
        public InArgument<string> Filter { get; set; }

        public InArgument<bool> SuspendNotifyCollectionChanged { get; set; } 
        #endregion

        private double _fontSize;

        public CreateTeActivity()
        {
            DisplayName = "Создать ТЕ";
            SuspendNotifyCollectionChanged = false;
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, TeCode, type.ExtractPropertyName(() => TeCode));
            ActivityHelpers.AddCacheMetadata(collection, metadata, PlaceCode, type.ExtractPropertyName(() => PlaceCode));
            ActivityHelpers.AddCacheMetadata(collection, metadata, IsPack, type.ExtractPropertyName(() => IsPack));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Length, type.ExtractPropertyName(() => Length));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Width, type.ExtractPropertyName(() => Width));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Height, type.ExtractPropertyName(() => Height));
            ActivityHelpers.AddCacheMetadata(collection, metadata, TareWeight, type.ExtractPropertyName(() => TareWeight));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Weight, type.ExtractPropertyName(() => Weight));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Mandants, type.ExtractPropertyName(() => Mandants));
            ActivityHelpers.AddCacheMetadata(collection, metadata, TeTypeCode, type.ExtractPropertyName(() => TeTypeCode));
            ActivityHelpers.AddCacheMetadata(collection, metadata, AutoTeType, type.ExtractPropertyName(() => AutoTeType));
            ActivityHelpers.AddCacheMetadata(collection, metadata, OutTe, type.ExtractPropertyName(() => OutTe));
            ActivityHelpers.AddCacheMetadata(collection, metadata, ExceptionResult, type.ExtractPropertyName(() => ExceptionResult));
            ActivityHelpers.AddCacheMetadata(collection, metadata, FontSize, type.ExtractPropertyName(() => FontSize));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Exist, type.ExtractPropertyName(() => Exist));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Filter, type.ExtractPropertyName(() => Filter));
            ActivityHelpers.AddCacheMetadata(collection, metadata, SuspendNotifyCollectionChanged, type.ExtractPropertyName(() => SuspendNotifyCollectionChanged));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            // получим все параметры
            var teCode = TeCode.Get(context);
            // переведем код ТЕ в верхний регистр
            if (!string.IsNullOrEmpty(teCode))
                teCode = teCode.ToUpper();
            var placeCode = PlaceCode.Get(context);
            var isPack = IsPack.Get(context);
            var length = Length.Get(context);
            var width = Width.Get(context);
            var height = Height.Get(context);
            var tareWeight = TareWeight.Get(context);
            var weight = Weight.Get(context);
            var mandants = Mandants.Get(context);
            var teTypeCode = TeTypeCode.Get(context);
            var autoTeType = AutoTeType.Get(context);
            var extFilter = Filter.Get(context);
            _fontSize = FontSize.Get(context);
            var suspendNotifyCollectionChanged = SuspendNotifyCollectionChanged.Get(context);

            var teManager = IoC.Instance.Resolve<IBaseManager<TE>>();

            try
            {
                if (suspendNotifyCollectionChanged)
                    teManager.SuspendNotifications();

                // если поле тип ТЕ пустое и не стоит признак пытаться определить тип ТЕ автоматически
                if (string.IsNullOrEmpty(teTypeCode) && !autoTeType)
                    throw new OperationException("Не указан тип ТЕ");

                // если поле тип ТЕ заполнено и установлен признак получения автоматически
                if (!string.IsNullOrEmpty(teTypeCode) && autoTeType)
                    throw new OperationException("Неверные настройки получения типа ТЕ");

                var uw = BeginTransactionActivity.GetUnitOfWork(context);
                if (uw != null)
                    throw new OperationException("Действие в транзакции запрещено");

                // проверим существование ТЕ
                if (!string.IsNullOrEmpty(teCode))
                {
                    var bpManager = IoC.Instance.Resolve<IBPProcessManager>();
                    var existTe = bpManager.CheckInstanceEntity("TE", teCode);
                    if (existTe == 1)
                    {
                        var existTeObj = teManager.Get(teCode);
                        if (existTeObj == null)
                            throw new OperationException("Нет прав на ТЕ с кодом {0}", teCode);
                        ExceptionResult.Set(context, null);
                        TeCode.Set(context, teCode);
                        OutTe.Set(context, existTeObj);
                        Exist.Set(context, true);
                        Result.Set(context, true);
                        return;
                    }
                }

                // фильтр на тип ТЕ
                var filter = string.Empty;
                // фильтр по мандантам
                if (!string.IsNullOrEmpty(mandants))
                    filter = string.Format(
                        "tetypecode in (select tt2m.tetypecode_r from wmstetype2mandant tt2m where tt2m.partnerid_r in ({0}))",
                        mandants);

                // фильтр по упаковкам
                if (isPack)
                    filter =
                        string.Format(
                            "{0}tetypecode in (select CUSTOMPARAMVAL.cpvkey from wmscustomparamvalue CUSTOMPARAMVAL  where CUSTOMPARAMVAL.CPV2ENTITY = 'TETYPE' and CUSTOMPARAMVAL.CUSTOMPARAMCODE_R = 'TETypeIsPackingL2' and CUSTOMPARAMVAL.CPVVALUE is not null and CUSTOMPARAMVAL.CPVVALUE != '0')",
                            string.IsNullOrEmpty(filter) ? string.Empty : filter + " and ");
                
                // дополнительный фильтр по типам ТЕ
                if (!string.IsNullOrEmpty(extFilter))
                    filter =
                        string.Format(
                            "{0}{1}",
                            string.IsNullOrEmpty(filter) ? string.Empty : filter + " and ", extFilter);

                // если надо определить тип ТЕ автоматически
                if (autoTeType)
                    teTypeCode = BPH.DetermineTeTypeCodeByTeCode(teCode, filter);

                var teTypeObj = GetTeType(teTypeCode, filter);

                // если не выбрали тип ТЕ
                if (teTypeObj == null)
                {
                    ExceptionResult.Set(context, null);
                    TeCode.Set(context, teCode);
                    OutTe.Set(context, null);
                    Exist.Set(context, false);
                    Result.Set(context, false);
                    return;
                }

                var teObj = new TE();
                teObj.SetKey(teCode);
                teObj.SetProperty(TE.TETypeCodePropertyName, teTypeObj.GetKey());
                teObj.SetProperty(TE.CreatePlacePropertyName, placeCode);
                teObj.SetProperty(TE.CurrentPlacePropertyName, placeCode);
                teObj.SetProperty(TE.StatusCodePropertyName, TEStates.TE_FREE.ToString());
                teObj.SetProperty(TE.TEPackStatusPropertyName,
                    isPack ? TEPackStatus.TE_PKG_CREATED.ToString() : TEPackStatus.TE_PKG_NONE.ToString());

                teObj.SetProperty(TE.TELengthPropertyName, length ?? teTypeObj.GetProperty(TEType.LengthPropertyName));
                teObj.SetProperty(TE.TEWidthPropertyName, width ?? teTypeObj.GetProperty(TEType.WidthPropertyName));
                teObj.SetProperty(TE.TEHeightPropertyName, height ?? teTypeObj.GetProperty(TEType.HeightPropertyName));
                teObj.SetProperty(TE.TETareWeightPropertyName,
                    tareWeight ?? teTypeObj.GetProperty(TEType.TareWeightPropertyName));
                teObj.SetProperty(TE.TEMaxWeightPropertyName,
                    tareWeight ?? teTypeObj.GetProperty(TEType.MaxWeightPropertyName));
                teObj.SetProperty(TE.TEWeightPropertyName, 
                    weight ?? teTypeObj.GetProperty(TEType.TareWeightPropertyName));

                ((ISecurityAccess) teManager).SuspendRightChecking();
                teManager.Insert(ref teObj);

                ExceptionResult.Set(context, null);
                TeCode.Set(context, teCode);
                OutTe.Set(context, teObj);
                Exist.Set(context, false);
                Result.Set(context, true);
            }
            catch (Exception ex)
            {
                TeCode.Set(context, teCode);
                ExceptionResult.Set(context, ex);
                OutTe.Set(context, null);
                Exist.Set(context, false);
                Result.Set(context, false);
            }
            finally
            {
                if (suspendNotifyCollectionChanged)
                    teManager.ResumeNotifications();
                ((ISecurityAccess)teManager).ResumeRightChecking();
            }
        }

        private TEType GetTeType(string teTypeCode, string filter)
        {
            if (string.IsNullOrEmpty(teTypeCode))
            {
                teTypeCode = GetTeTypeModel(filter);
                if (string.IsNullOrEmpty(teTypeCode))
                    return null;
            }
            filter = string.Format("{0}tetypecode = '{1}'", string.IsNullOrEmpty(filter) ? string.Empty : filter + " and ", teTypeCode);
            var teTypeManager = IoC.Instance.Resolve<IBaseManager<TEType>>();
            var teType = teTypeManager.GetFiltered(filter, GetModeEnum.Partial).FirstOrDefault();
            return teType;
        }

        private string GetTeTypeModel(string filter)
        {
            var teType = string.Empty;
            DispatcherHelper.Invoke(new Action(() =>
            {

                var factory = IoC.Instance.Resolve<IObjectListFactory>();
                var model = factory.CreateModel<TEType>("Укажите тип ТЕ", filter, null, _fontSize, LayoutSuffix);
                var vs = IoC.Instance.Resolve<IViewService>();
                var result = vs.ShowDialogWindow(model, true, false, "50%", "50%");
                if (result == true)
                {
                    var selectable = model as ISelectable;
                    if (selectable == null)
                        throw new OperationException("Модель '{0}' не реализует интерфейс ISelectable", model);
                    var selKeys = selectable.GetSelectedKeys();
                    if (selKeys != null && selKeys.Count() != 0)
                        teType = selKeys[0].To<string>();
                }
                var customdisp = model as ICustomDisposable;
                if (customdisp == null) 
                    return;
                customdisp.SuspendDispose = false;
                customdisp.Dispose();
            }));
            return teType;
        }
    }
}

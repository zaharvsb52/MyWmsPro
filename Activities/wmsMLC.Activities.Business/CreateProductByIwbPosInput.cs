using System;
using System.Activities;
using System.Activities.Expressions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using wmsMLC.Activities.General;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.Activities.Business
{
    public class CreateProductByIwbPosInput : NativeActivity<List<Product>>
    {
        #region . Arguments .

        [DisplayName(@"Позиции")]
        [RequiredArgument, DefaultValue(null)]
        public InArgument<List<IWBPosInput>> IWBPosList { get; set; }

        [DisplayName(@"Номер накладной")]
        [RequiredArgument]
        public InArgument<decimal> IWBId { get; set; }

        [DisplayName(@"Место приемки")]
        [RequiredArgument]
        public InArgument<string> PlaceCode { get; set; }

        [DisplayName(@"Миграция")]
        [RequiredArgument, DefaultValue(false)]
        public InArgument<bool> IsMigration { get; set; }

        [DisplayName(@"Не принятые позиции")]
        [RequiredArgument, DefaultValue(null)]
        public OutArgument<List<IWBPosInput>> FailedIWBPosList { get; set; }

        [DisplayName(@"Список ошибок")]
        public OutArgument<List<Exception>> ExceptionList { get; set; }

        [DisplayName(@"Сообщение ошибок")]
        public OutArgument<string> ErrorMessage { get; set; }

        [DisplayName(@"Время ожидания выполнения")]
        [DefaultValue(null)]
        public InArgument<int?> TimeOut { get; set; }

        [DisplayName(@"Операция")]
        [RequiredArgument]
        public InArgument<string> OperationCode { get; set; }

        #endregion

        #region . Fields .

        private ConcurrentQueue<Product> _productList;
        private List<IWBPosInput> _items;
        private ConcurrentQueue<IWBPosInput> _failedItems;
        private ConcurrentQueue<Exception> _exceptionList;

        private IViewService _viewService;
        private StringBuilder _messageBuilder;
        private decimal _partnerId;

        #endregion

        public CreateProductByIwbPosInput()
        {
            DisplayName = "Создать товар по позициям";
        }

        #region .  Methods  .

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, IWBPosList,
                type.ExtractPropertyName(() => IWBPosList));
            ActivityHelpers.AddCacheMetadata(collection, metadata, FailedIWBPosList,
                type.ExtractPropertyName(() => FailedIWBPosList));
            ActivityHelpers.AddCacheMetadata(collection, metadata, IWBId, type.ExtractPropertyName(() => IWBId));
            ActivityHelpers.AddCacheMetadata(collection, metadata, PlaceCode, type.ExtractPropertyName(() => PlaceCode));
            ActivityHelpers.AddCacheMetadata(collection, metadata, IsMigration,
                type.ExtractPropertyName(() => IsMigration));
            ActivityHelpers.AddCacheMetadata(collection, metadata, ExceptionList,
                type.ExtractPropertyName(() => ExceptionList));
            ActivityHelpers.AddCacheMetadata(collection, metadata, ErrorMessage,
                type.ExtractPropertyName(() => ErrorMessage));
            ActivityHelpers.AddCacheMetadata(collection, metadata, TimeOut, type.ExtractPropertyName(() => TimeOut));
            ActivityHelpers.AddCacheMetadata(collection, metadata, OperationCode,
                type.ExtractPropertyName(() => OperationCode));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            _productList = new ConcurrentQueue<Product>();
            _failedItems = new ConcurrentQueue<IWBPosInput>();
            _exceptionList = new ConcurrentQueue<Exception>();
            _items = IWBPosList.Get(context);


            _viewService = IoC.Instance.Resolve<IViewService>();
            _messageBuilder = new StringBuilder();

            using (var mgr = IoC.Instance.Resolve<IBaseManager<IWB>>())
            {
                var iwbId = IWBId.Get(context);
                var iwb = mgr.Get(iwbId);
                if (iwb.MandantID != null)
                    _partnerId = iwb.MandantID.Value;
            }

            // падать тут ничего не должно
            try
            {
                ProcessCreateProduct(context);
            }
            catch (Exception ex)
            {
                _exceptionList.Enqueue(ex);
                _messageBuilder.AppendLine("Процесс был прерван. " + ExceptionHelper.GetErrorMessage(ex));
            }

            ErrorMessage.Set(context, _messageBuilder.Length > 0 ? _messageBuilder.ToString() : string.Empty);
            Result.Set(context, _productList.ToList());
            FailedIWBPosList.Set(context, _failedItems.ToList());
            ExceptionList.Set(context, _exceptionList.ToList());
        }

        private void ProcessCreateProduct(NativeActivityContext context)
        {
            #region  .  Checks&Ini  .

            if (_items == null || _items.Count == 0)
                return;

            var iwbId = IWBId.Get(context);
            var placeCode = PlaceCode.Get(context);
            var operationCode = OperationCode.Get(context);
            var isMigration = IsMigration.Get(context) ? 1 : 0;
            var timeOut = TimeOut.Get(context);
            var wfUow = BeginTransactionActivity.GetUnitOfWork(context);

            // получаем менеджер приемки
            Min min = null;
            using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
            {
                if (wfUow != null)
                    mgr.SetUnitOfWork(wfUow);

                var minId = mgr.GetDefaultMIN(iwbId);
                if (minId.HasValue)
                {
                    using (var mgrIn = IoC.Instance.Resolve<IBaseManager<Min>>())
                    {
                        if (wfUow != null)
                            mgr.SetUnitOfWork(wfUow);

                        min = mgrIn.Get(minId);
                    }
                }
            }

            // определяем параметры приемки
            var isMinCpvExist = min != null && min.CustomParamVal != null && min.CustomParamVal.Count != 0;
            var isNeedControlOver = isMinCpvExist &&
                                    min.CustomParamVal.Any(
                                        i => i.CustomParamCode == MinCpv.MINOverEnableCpvName && i.CPVValue == "1");
            var isNeedConfirmOver = isNeedControlOver &&
                                    min.CustomParamVal.Any(
                                        i =>
                                            i.CustomParamCode == MinCpv.MINOverEnableNeedConfirmCpvName &&
                                            i.CPVValue == "1");
            var isMinLimit = isMinCpvExist && min.CustomParamVal.Any(i => i.VCustomParamParent == MinCpv.MinLimitL2CpvName);
            var itemsToProcess = new List<IWBPosInput>();

            #endregion

            #region  .  BatchCode&OverCount  .

            foreach (var item in _items)
            {
                var itemKey = item.GetKey();
                if (!item.IsSelected || itemKey == null)
                {
                    _failedItems.Enqueue(item);
                    continue;
                }

                item.ManageFlag = null;

                //Ошибки при распознавании batch-кода
                if (!item.NotCriticalBatchcodeError && !item.IsBpBatchcodeCompleted)
                {
                    var message = string.IsNullOrEmpty(item.BatchcodeErrorMessage)
                        ? "Ошибка не определена."
                        : item.BatchcodeErrorMessage;
                    SkipItem(item, message);
                    continue;
                }

                if (item.RequiredSKUCount < 1)
                {
                    SkipItem(item, "Некорректно введено 'Количество по факту'");
                    continue;
                }

                // если пытаемся принять с избытком
                if (isNeedControlOver)
                {
                    var count = item.ProductCountSKU + (double) item.RequiredSKUCount;
                    var overCount = count - item.IWBPosCount;
                    if (overCount > 0 && isNeedConfirmOver)
                    {
                        var questionMessage = string.Format(
                            "Позиция '{0}' ед.уч. '{1}'.{4}Фактическое кол-во = '{2}'.{4}Излишек составит '{3}'.{4}Принять излишек?",
                            itemKey, item.SKUNAME, count, overCount, Environment.NewLine);

                        var dr = _viewService.ShowDialog(
                            GetDefaultMessageBoxCaption(iwbId),
                            questionMessage,
                            MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, 14);

                        if (dr == MessageBoxResult.Yes)
                        {
                            item.ManageFlag += (!string.IsNullOrEmpty(item.ManageFlag) ? "," : string.Empty) +
                                               "OVERASK_OK";
                        }
                        else
                        {
                            var message = string.Format("Пользователь отказался принимать излишек по позиции '{0}'",
                                itemKey, item.SKUNAME);
                            SkipItem(item, message);
                            continue;
                        }
                    }
                }

                itemsToProcess.Add(item);
            }

            #endregion

            // группируем по артикулу
            var groupItems = itemsToProcess.GroupBy(i => i.ArtCode).ToArray();

            //обработка сроков годности
            if (isMinLimit)
            {
                var ret = ProccesExpireDate(itemsToProcess, min, groupItems, wfUow, iwbId);
                if (!ret)
                    return;
            }

            var ask = new ConcurrentQueue<IWBPosInput>();

            while (true)
            {
                RunCreateProduct(itemsToProcess, ref ask, timeOut, wfUow, operationCode, iwbId, isMigration, placeCode,
                    isNeedControlOver);
                if (ask == null || ask.Count <= 0)
                    break;

                itemsToProcess.Clear();
                foreach (var item in ask)
                {
                    var message = string.Format("Принять излишек по позиции '{0}' ед.уч. '{1}' ?", item.GetKey(),
                        item.SKUNAME);
                    var res = _viewService.ShowDialog(GetDefaultMessageBoxCaption(iwbId), message,
                        MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, 14);

                    if (res != MessageBoxResult.Yes)
                    {
                        SkipItem(item,
                            string.Format("Пользователь отказался принимать излишек по позиции '{0}'", item.GetKey()));
                        continue;
                    }
                    itemsToProcess.Add(item);
                }

                while (!ask.IsEmpty)
                {
                    IWBPosInput someItem;
                    ask.TryDequeue(out someItem);
                }
            }
        }

        /// <summary>
        /// Обработка сроков годности
        /// </summary>
        /// <param name="itemsToProcess">Обрабатываемые позции прихода</param>
        /// <param name="min">Менеджер приемки</param>
        /// <param name="groupItems">Сгруппированные артикула</param>
        /// <param name="wfUow">Unit Of Work</param>
        /// <param name="iwbId">Id приходной накладной</param>
        private bool ProccesExpireDate(List<IWBPosInput> itemsToProcess, Min min,
            IEnumerable<IGrouping<string, IWBPosInput>> groupItems, IUnitOfWork wfUow, decimal iwbId)
        {
            var minExpiryDateInLimitAsk =
                min.CustomParamVal.FirstOrDefault(i => i.CustomParamCode == MinCpv.MinExpiryDateInLimitAsk);
            var minExpiryDateInLimitDeny =
                min.CustomParamVal.FirstOrDefault(i => i.CustomParamCode == MinCpv.MinExpiryDateInLimitDeny);
            var minExpiryDateInLimitAskBlock =
                min.CustomParamVal.FirstOrDefault(i => i.CustomParamCode == MinCpv.MinExpiryDateInLimitAskBlock);
            var minExpiryDateInLimitAskQlf =
                min.CustomParamVal.FirstOrDefault(i => i.CustomParamCode == MinCpv.MinExpiryDateInLimitAskQlf);
            var defaultBlock = min.CustomParamVal.FirstOrDefault(i => i.CustomParamCode == MinCpv.DefaultBlock);
            var canOnDclBlock = min.CustomParamVal.FirstOrDefault(i => i.CustomParamCode == MinCpv.CanOnDclBlock);
            var defaultQlf = min.CustomParamVal.FirstOrDefault(i => i.CustomParamCode == MinCpv.DefaultQlf);
            var canOnDclQlf = min.CustomParamVal.FirstOrDefault(i => i.CustomParamCode == MinCpv.CanOnDclQlf);

            // если нужно контролировать срок годности, то для оставшихся после первого шага артикулов получаем параметры контроля
            var expDates = new Dictionary<string, DateTime?>();
            if (minExpiryDateInLimitAsk == null && minExpiryDateInLimitDeny == null &&
                minExpiryDateInLimitAskBlock == null && minExpiryDateInLimitAskQlf == null) return true;

            // получаем список уникальных артикулов
            var artLst = itemsToProcess.Select(i => i.ArtCode).Distinct();

            // формируем запрос сроков годности
            var sb = new StringBuilder();
            foreach (var artCode in artLst)
            {
                if (sb.Length > 0)
                    sb.AppendLine("union all");
                sb.AppendLine(
                    string.Format("select '{0}' a, pkgBpExpiryDate.bpCalcExpiryDate({1}, '{0}') d from dual",
                        artCode, iwbId));
            }

            // получаем сроки годности
            if (sb.Length > 0)
            {
                // получаем планируемый срок для каждого артикула
                using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
                {
                    if (wfUow != null)
                        mgr.SetUnitOfWork(wfUow);

                    var sql = sb.ToString();
                    var table = mgr.ExecuteDataTable(sql);

                    // переносим в Dictionary
                    foreach (DataRow row in table.Rows)
                        expDates.Add((string) row["a"], row.IsNull("d") ? null : (DateTime?) row["d"]);
                }
            }

            // по каждой группе артикулов проверям сроки годности
            foreach (var groupItem in groupItems)
            {
                var artCode = groupItem.Key;
                if (!expDates.ContainsKey(artCode) || !expDates[artCode].HasValue) continue;

                var expDate = expDates[artCode].Value;
                foreach (var item in groupItem)
                {
                    try
                    {
                        // если даты нет - ошибка
                        if (!item.IWBPosExpiryDate.HasValue)
                        {
                            SkipItem(item, "Настроен контроль срока годности, но в позиции срок годности отсутсвует");
                            itemsToProcess.Remove(item);
                            continue;
                        }

                        // срок годности (СГ) - Откл. > тек. дата (ТД)
                        // => СГ > ТД + Откл
                        // => знак меням, чтобы ошибку обработать
                        if (item.IWBPosExpiryDate.Value > expDate) continue;

                        if (minExpiryDateInLimitDeny != null && minExpiryDateInLimitDeny.CPVValue == "1")
                        {
                            SkipItem(item, ExpDateMessage(expDate, item) + " Приемка заблокирована");
                            itemsToProcess.Remove(item);
                            continue;
                        }

                        if (minExpiryDateInLimitAsk != null && minExpiryDateInLimitAsk.CPVValue == "1")
                        {
                            var dr = _viewService.ShowDialog(
                                GetDefaultMessageBoxCaption(iwbId),
                                ExpDateMessage(expDate, item) + " Продолжить?",
                                MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes, 14);

                            switch (dr)
                            {
                                case MessageBoxResult.Yes:
                                    item.ManageFlag += (!string.IsNullOrEmpty(item.ManageFlag) ? "," : string.Empty) +
                                                       "EXPDATEASK_OK";
                                    break;
                                case MessageBoxResult.No:
                                    SkipItem(item, ExpDateMessage(expDate, item) + " Пользователь отказался продолжать");
                                    itemsToProcess.Remove(item);
                                    continue;
                                default:
                                    SkipItem(item,
                                        ExpDateMessage(expDate, item) +
                                        " Пользователь отказался продолжать. Прерываем выполнение");
                                    return false;
                            }
                        }
                        //обработка блокировки
                        if (minExpiryDateInLimitAskBlock != null && minExpiryDateInLimitAskBlock.CPVValue == "1")
                        {
                            var retBlock = ProccesCvps(itemsToProcess, canOnDclBlock, item, expDate, defaultBlock,
                                "BLOCK", AskBlocking);
                            if (!retBlock)
                                return false;
                        }
                        //обработка квалификации
                        if (minExpiryDateInLimitAskQlf != null && minExpiryDateInLimitAskQlf.CPVValue == "1")
                        {
                            var retQlf = ProccesCvps(itemsToProcess, canOnDclQlf, item, expDate, defaultQlf, "QLF",
                                AskQualifi);
                            if (!retQlf)
                                return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        SkipItem(item, ExceptionHelper.GetErrorMessage(ex));
                        _exceptionList.Enqueue(ex);
                        return false;
                    }
                }
            }
            return true;
        }

        private bool ProccesCvps(ICollection<IWBPosInput> itemsToProcess, CustomParamValue canAskCpvConfirm,
            IWBPosInput item,
            DateTime expDate,
            CustomParamValue defaultValueCpv, string cpvType,
            Func<IWBPosInput, DateTime, string, MessageBoxResult> procBlQlf)
        {
            var exceptionMessage = string.Empty;
            var extMessage = string.Empty;
            var manageFlage = string.Empty;
            var defCpvVal = defaultValueCpv == null ? string.Empty : defaultValueCpv.CPVValue;

            switch (cpvType)
            {
                case "BLOCK":
                    exceptionMessage =
                        "Должно быть указано не менее одного параметра: 'Блокировка' или 'Разрешить выбор на DCL' в менеджере приемки. Пожалуйста, свяжитесь со службой поддержки";
                    extMessage = "Пользователь отказался вводить блокировку";
                    manageFlage = "EXPDATEACCEPTWITHBLOCKING_OK";
                    break;
                case "QLF":
                    exceptionMessage =
                        "Должно быть указано не менее одного параметра: 'Квалификация' или 'Разрешить выбор на DCL' в менеджере приемки. Пожалуйста, свяжитесь со службой поддержки";
                    extMessage = "Пользователь отказался вводить квалификацию";
                    manageFlage = "EXPDATEACCEPTWITHQUALIFI_OK";
                    break;
            }

            var funcRes = MessageBoxResult.No;
            if (canAskCpvConfirm != null && canAskCpvConfirm.CPVValue == "1")
            {
                funcRes = procBlQlf(item, expDate, defCpvVal);
            }
            else if (defaultValueCpv != null &&
                     (canAskCpvConfirm == null || (canAskCpvConfirm.CPVValue == "0")))
            {
                switch (cpvType)
                {
                    case "BLOCK":
                        item.IWBPosBlocking = string.IsNullOrEmpty(item.IWBPosBlocking)
                            ? defaultValueCpv.CPVValue
                            : item.IWBPosBlocking;
                        break;
                    case "QLF":
                        item.QLFCODE_R = string.IsNullOrEmpty(item.QLFCODE_R) || item.QLFCODE_R == "QLFNORMAL"
                            ? defaultValueCpv.CPVValue ?? "QLFNORMAL"
                            : item.QLFCODE_R;
                        break;
                }
                funcRes = MessageBoxResult.Yes;
            }
            else if (defaultValueCpv == null && (canAskCpvConfirm == null || canAskCpvConfirm.CPVValue == "0"))
            {
                throw new OperationException(
                    exceptionMessage);
            }

            switch (funcRes)
            {
                case MessageBoxResult.Yes:
                    item.ManageFlag += (!string.IsNullOrEmpty(item.ManageFlag) ? "," : string.Empty) +
                                       manageFlage;
                    break;
                case MessageBoxResult.No:
                    SkipItem(item,
                        ExpDateMessage(expDate, item) + extMessage);
                    itemsToProcess.Remove(item);
                    break;
                default:
                    SkipItem(item,
                        ExpDateMessage(expDate, item) +
                        extMessage + " Прерываем выполнение");
                    return false;
            }
            return true;
        }

        private void RunCreateProduct(List<IWBPosInput> itemsToProcess, ref ConcurrentQueue<IWBPosInput> ask,
            int? timeOut, IUnitOfWork wfUow, string operationCode, decimal iwbId, int isMigration, string placeCode,
            bool isNeedControlOver)
        {
            if (itemsToProcess == null)
                throw new ArgumentNullException("itemsToProcess");

            var queue = ask;

            itemsToProcess
                .GroupBy(i => i.ArtCode)
                .AsParallel()
                .ForAll(items =>
                {
                    var uow = wfUow ?? UnitOfWorkHelper.GetUnit();
                    try
                    {
                        if (timeOut.HasValue)
                            uow.TimeOut = timeOut;

                        using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
                        {
                            mgr.SetUnitOfWork(uow);
                            foreach (var item in items)
                            {
                                try
                                {
                                    // сообщаем, что все проверили на нашей стороне
                                    var manageFlag = item.ManageFlag;
                                    string manageFlagParam = null;

                                    // создаем товары
                                    var products =
                                        mgr.CreateProductByPos(ref manageFlag, ref manageFlagParam, operationCode, iwbId,
                                            item, isMigration, placeCode).ToArray();
                                    if ("OVERASK".Equals(manageFlag))
                                    {
                                        manageFlag += (!string.IsNullOrEmpty(manageFlag) ? "," : string.Empty) +
                                                      "OVERASK_OK";

                                        if (isNeedControlOver)
                                        {
                                            item.ManageFlag = manageFlag;
                                            queue.Enqueue(item);
                                            continue;
                                        }

                                        Array.Clear(products, 0, products.Length);
                                        products =
                                            mgr.CreateProductByPos(ref manageFlag, ref manageFlagParam, operationCode,
                                                iwbId, item, isMigration, placeCode).ToArray();
                                    }

                                    if (!string.IsNullOrEmpty(manageFlag))
                                        throw new OperationException(
                                            "Все параметры приемки должны были быть обработаны до создания товара. Пожалуйста, свяжитесь со службой поддержки");

                                    foreach (var t in products)
                                        _productList.Enqueue(t);

                                    //HACK: Делаем допущение, что мы всегда принимаем то кол-во, которое запросили. Частично принять мы не можем.
                                    //Обновляем приятое и оставшееся кол-во здесь для возможности последующего использования данного объекта
                                    //var acceptedCount = (double)item.RequiredSKUCount;
                                    //item.ProductCountSKU += acceptedCount;
                                    //item.RemainCount -= acceptedCount;
                                    //item.RequiredSKUCount = item.RemainCount < item.ProductCountSKU ? 0 : (decimal) (item.RemainCount - item.ProductCountSKU);
                                }
                                catch (Exception ex)
                                {
                                    SkipItem(item, ExceptionHelper.GetErrorMessage(ex));
                                    _exceptionList.Enqueue(ex);
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (wfUow == null)
                            uow.Dispose();
                    }
                });

            ask = queue;
        }

        private void SkipItem(IWBPosInput item, string message)
        {
            _messageBuilder.AppendLine(GetSkipMessage(item));
            _messageBuilder.AppendLine(message);
            _failedItems.Enqueue(item);
        }

        private MessageBoxResult AskBlocking(IWBPosInput item, DateTime planDate, string defaultBlock = null)
        {
            var field = IWBPosInput.IWBPosBlockingPropertyName;
            // формируем поля дозапроса
            var values = new List<ValueDataField>
            {
                new ValueDataField
                {
                    Name = "desc",
                    FieldName = "desc",
                    SourceName = "desc",
                    FieldType = typeof (string),
                    LabelPosition = "Top",
                    IsEnabled = false,
                    Value = ExpDateMessage(planDate, item) + " Укажите блокировку?"
                },
                new ValueDataField
                {
                    Name = field,
                    FieldName = field,
                    SourceName = field,
                    Caption = "Блокировка",
                    FieldType = typeof (string),
                    LabelPosition = "Top",
                    LookupCode = "_PRODUCTBLOCKING_PRODUCT",
                    Value = string.IsNullOrEmpty(item.IWBPosBlocking) ? defaultBlock : item.IWBPosBlocking
                }
            };

            var model = new ExpandoObjectViewModelBase
            {
                Fields = new ObservableCollection<ValueDataField>(values),
                PanelCaption = "Блокировка по сроку годности"
            };

            bool? dr;
            while (true)
            {
                dr = _viewService.ShowDialogWindow(model, true, buttons: MessageBoxButton.YesNoCancel, width: "30%");
                if (dr != true)
                    break;

                var blockingCode = model[field] as string;
                if (string.IsNullOrEmpty(blockingCode))
                {
                    _viewService.ShowDialog("Ошибка", "Поле 'Блокировка' не может быть пустым", MessageBoxButton.OK,
                        MessageBoxImage.Warning, MessageBoxResult.OK);
                }
                else
                {
                    item.IWBPosBlocking = blockingCode;
                    return MessageBoxResult.Yes;
                }
            }
            return dr == false ? MessageBoxResult.No : MessageBoxResult.Cancel;
        }

        private MessageBoxResult AskQualifi(IWBPosInput item, DateTime planDate, string defaultQlf = null)
        {
            const string field = IWBPosInput.QLFCodePropertyName;
            // формируем поля дозапроса
            var values = new List<ValueDataField>
            {
                new ValueDataField
                {
                    Name = "desc",
                    FieldName = "desc",
                    SourceName = "desc",
                    FieldType = typeof (string),
                    LabelPosition = "Top",
                    IsEnabled = false,
                    Value = ExpDateMessage(planDate, item) + " Укажите квалификацию?"
                },
                new ValueDataField
                {
                    Name = field,
                    FieldName = field,
                    SourceName = field,
                    Caption = "Квалификация",
                    FieldType = typeof (string),
                    LabelPosition = "Top",
                    LookupFilterExt =
                        string.Format(
                            " exists(select * from wmsqlf2mandant ql2mn where ql2mn.qlfcode_r = qlfcode and ql2mn.partnerid_r = {0})",
                            _partnerId),
                    LookupCode = "QLF_QLFCODE",
                    Value = String.IsNullOrEmpty(item.QLFCODE_R) || item.QLFCODE_R == "QLFNORMAL" ? defaultQlf ?? "QLFNORMAL" : item.QLFCODE_R
                }
            };

            var model = new ExpandoObjectViewModelBase
            {
                Fields = new ObservableCollection<ValueDataField>(values),
                PanelCaption = "Квалификация по сроку годности"
            };

            bool? dr;
            while (true)
            {
                dr = _viewService.ShowDialogWindow(model, true, buttons: MessageBoxButton.YesNoCancel, width: "30%");
                if (dr != true)
                    break;

                var qlfCode = model[field] as string;
                if (string.IsNullOrEmpty(qlfCode))
                {
                    _viewService.ShowDialog("Ошибка", "Поле 'Квалификация' не может быть пустым", MessageBoxButton.OK,
                        MessageBoxImage.Warning, MessageBoxResult.OK);
                }
                else
                {
                    item.QLFCODE_R = qlfCode;
                    return MessageBoxResult.Yes;
                }
            }

            return dr == false ? MessageBoxResult.No : MessageBoxResult.Cancel;
        }

        private static string ExpDateMessage(DateTime planDate, IWBPosInput item)
        {
            return string.Format(
                "Позиция '{0}' ед.уч. '{1}' имеет срок годности '{2:dd.MM.yyyy}' меньший минимально допустимого {3:dd.MM.yyyy}.",
                item.GetKey(), item.SKUNAME, item.IWBPosExpiryDate.Value, planDate);
        }

        private static string GetSkipMessage(IWBPosInput item)
        {
            return string.Format("По позиции с кодом '{0}' ед.уч. '{1}' товар не принят:", item.GetKey(), item.SKUNAME);
        }

        private static string GetDefaultMessageBoxCaption(decimal iwbId)
        {
            return string.Format("Приемка накладной '{0}'", iwbId);
        }

        #endregion
    }
}
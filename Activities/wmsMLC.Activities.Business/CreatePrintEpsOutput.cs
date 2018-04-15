using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using wmsMLC.Activities.Dialogs.Views.Editors;
using wmsMLC.Activities.General;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.Activities.Business
{
    [DisplayName(@"Создать задание на печать")]
    [Designer(typeof(NameValueCollectionEditorDialog))]
    public class CreatePrintEpsOutput : NativeActivity<PrintReportStatus>
    {
        #region . Fields & Consts .
        private const string DialogCaption = "Заполните все параметры отчета";

        private string _oldReportCode = string.Empty;

        private Dictionary<string, Argument> _parameters;
        #endregion

        #region . Arguments .

        [DisplayName(@"Код отчета")]
        [RequiredArgument]
        public InArgument<string> ReportCode { get; set; }

        [DisplayName(@"Параметры отчета")]
        [Browsable(false)]
        public Dictionary<string, Argument> Parameters {
            get
            {
                if (_parameters == null)
                    _parameters = new Dictionary<string, Argument>();
                return _parameters;
            }
            set
            {
                _parameters = value;
            }
        }

        [DisplayName(@"(Параметры)Запрашивать пользователя")]
        [Description(@"Разрешить дозапрос параметров у пользователя")]
        [DefaultValue(false)]
        public bool RequestUser { get; set; }

        [DisplayName(@"(Параметры)Не печатать")]
        [DefaultValue(false)]
        public bool DoNotPrint { get; set; }

        [DisplayName(@"(Параметры)Игнорировать ошибки")]
        [DefaultValue(false)]
        public bool IgnoreErrors { get; set; }

        [DisplayName(@"(Параметры)Код манданта")]
        [Description(@"Если не задан, но укзаны сущности для печати, то будет произведен поиск по сущностям. Если и сущности не указаны - применятся параметры из настроек")]
        [DefaultValue(false)]
        public string MandantCode { get; set; }

        [DisplayName(@"(Параметры)ШК принтера")]
        [Description(@"Если не задан, то принтер будет взят из настроек")]
        [DefaultValue(false)]
        public string Barcode { get; set; }

        [Browsable(false)]
        public string DesignerText { get; private set; }

        #endregion

        #region .  Methods  .
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            foreach (var key in Parameters.Keys)
            {
                var argument = Parameters[key];
                var runtimeArgument = new RuntimeArgument(key, argument.ArgumentType, argument.Direction);
                metadata.Bind(argument, runtimeArgument);
                collection.Add(runtimeArgument);
            }

            var type = GetType();
            ActivityHelpers.AddCacheMetadata(collection, metadata, ReportCode, type.ExtractPropertyName(() => ReportCode));
            metadata.SetArgumentsCollection(collection);
        }

        public CreatePrintEpsOutput()
        {            
            DesignerText = @"Создание задания на печать";            
        }

        protected override void Execute(NativeActivityContext context)
        {
            var reportCode = ReportCode.Get(context);

            if (!_oldReportCode.Equals(reportCode) && Parameters.Count < 1)
            {
                if (reportCode != null)
                {
                    var val = reportCode.Replace("\"", string.Empty);
                    var prm = GetParams(val);
                    foreach (var p in prm)
                    {
                        var argument = new InArgument<string> { Expression = p.EpsConfigValue };
                        Parameters.Add(p.EpsConfigParamCode, argument);
                    }
                }
                _oldReportCode = reportCode ?? string.Empty;
            }            

            var parameters = new List<NameValueObject>();
            foreach (var p in Parameters)
                parameters.Add(new NameValueObject{ Name = p.Key, DisplayName = p.Key, Value = p.Value.Get(context)});
            // проверим корректность данных
            if (!CheckParams(parameters))
            {
                // Запрещена ли печать при ошибке
                if (DoNotPrint)
                    return;

                // если нужно спросить пользователя
                if (RequestUser)
                {
                    var fields = new List<ValueDataField>();
                    foreach (var p in parameters)
                    {
                        var name = p.Name.Replace("{", string.Empty).Replace("}", string.Empty);
                        var field = new ValueDataField
                            {
                                Name = name,
                                SourceName = name,
                                Caption = p.DisplayName,
                                FieldName = name,
                                FieldType = typeof(string),
                                Value = p.Value
                            };
                        fields.Add(field);
                    }

                    var model = new ExpandoObjectViewModelBase();
                    model.Fields = new ObservableCollection<ValueDataField>(fields);
                    model.PanelCaption = DialogCaption;
                    if (!ShowDialog(model))
                        return;

                    foreach (var p in parameters)
                        p.Value = model[p.Name.Replace("{", string.Empty).Replace("}", string.Empty)].ToString();

                    // проверяем последний раз
                    if (!CheckParams(parameters))
                    {
                        // игнорировать ошибки
                        if (!IgnoreErrors)
                            return;
                    }
                }
                // если не игнорируем ошибки
                else if (!IgnoreErrors)
                {
                    return;
                }
            }

            // заполняем параметры отчета
            var outputParams = new List<OutputParam>();
            foreach (var p in parameters)
            {
                outputParams.Add(new OutputParam
                    {
                        OutputParamCode = p.Name,
                        OutputParamType = string.Empty, // заполнится в менеджере
                        OutputParamValue = p.Value != null ? p.Value.ToString() : null
                    });
            }

            // определяем id манданта по коду
            decimal? mandantId = null;
            if (!string.IsNullOrEmpty(MandantCode))
            {
                var mandantMgr = IoC.Instance.Resolve<IBaseManager<Mandant>>();
                var codeFilterName = SourceNameHelper.Instance.GetPropertySourceName(typeof(Mandant), Mandant.MANDANTCODEPropertyName);
                var filter = string.Format("{0} = '{1}'", codeFilterName, MandantCode);
                var mandants = mandantMgr.GetFiltered(filter).ToArray();
                if (mandants.Length > 1)
                    throw new DeveloperException("Получено более одного Манданта с кодом " + MandantCode);

                if (mandants.Length == 1)
                    mandantId = mandants[0].MandantId;
            }

            // поехали!
            var reportMgr = IoC.Instance.Resolve<Report2EntityManager>();
            var uow = BeginTransactionActivity.GetUnitOfWork(context);
            if (uow != null)
                reportMgr.SetUnitOfWork(uow);
            var result = reportMgr.PrintReport(mandantId, reportCode.Replace("\"", string.Empty), Barcode, outputParams);

            // возвращает статус задания
            Result.Set(context, result);

            // возвращает код задания
            //Result.Set(context, result.Job);
        }

        private static IEnumerable<ReportCfg> GetParams(string code)
        {
            var mgr = IoC.Instance.Resolve<IBaseManager<ReportCfg>>();
            var filter = string.Format("({0} = 'REPORT' AND {1} = '{2}')",
                SourceNameHelper.Instance.GetPropertySourceName(typeof(EpsConfig), EpsConfig.EpsConfig2EntityParamCodePropertyName),
                SourceNameHelper.Instance.GetPropertySourceName(typeof(EpsConfig), EpsConfig.EpsConfigKeyPropertyName),
                code);

            return mgr.GetFiltered(filter);
        }

        private static bool CheckParams(IEnumerable<NameValueObject> paramsToCheck)
        {
            return paramsToCheck.All(p => p.Value != null);
        }

        private static bool ShowDialog(ExpandoObjectViewModelBase model)
        {
            var viewService = IoC.Instance.Resolve<IViewService>();
            return viewService.ShowDialogWindow(model, true) == true;
        } 
        #endregion
    }
}

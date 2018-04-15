using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Converters;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectView))]
    public class BillWorkActDetailViewModel : ObjectViewModelBase<BillWorkActDetail>
    {
        private const string BpCode = "BillWorkActDetailCalcParameters";
        private const string DefaultDateTimeDisplayFormat = "dd.MM.yyyy";

        private const double DefaultCalcCount = 1;
        private const double DefaultCalcFactor = 1;
        private DateTime? _defaltCalcDate;

        private readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(BillWorkActDetailViewModel));

        private List<BillOperationCause> _causes;
        private BillOperation2Contract _billOperation2Contract;
        private string[] _dateTimeFormats;
        private DateTime _bpStartTime;
        private bool _doNotCalculate;
        private readonly object _lock = new object();

        protected override void OnSourceChanged()
        {
            base.OnSourceChanged();
            if (Source == null)
                return;

            // Выставляем значение ручного ввода
            if (Source.IsNew)
            {
                if (!Source.WorkActDetailManual)
                    Source.WorkActDetailManual = true;
                Source.IsEnabledTotalSum = true;
                Source.IsEnabledCalc = true;
                Source.AcceptChanges(true);
            }

            if (!Source.Operation2ContractID.HasValue)
                return;

            GetCause();
            GetFormula();
            ValidateCalcFields();
            RefreshView();
        }

        private void ValidateCalcFields()
        {
            if (Source == null)
                return;

            var isFormulasExist = IsFormulasExist();
            try
            {
                _doNotCalculate = true;

                Source.IsEnabledTotalSum =
                    Source.IsEnabledCalc = !isFormulasExist;

                if (Source.IsNew)
                {
                    Source.WorkActDetailTotalSum = null;
                    if (isFormulasExist)
                    {
                        Source.WorkActDetailCount = DefaultCalcCount;
                        Source.WorkActDetailMulti = DefaultCalcFactor;
                    }
                    else
                    {
                        Source.WorkActDetailCount = null;
                        Source.WorkActDetailMulti = null;
                    }
                }
            }
            finally
            {
                _doNotCalculate = false;
            }
        }

        protected override void SourceObjectPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.SourceObjectPropertyChanged(sender, e);

            if (Source == null)
                return;

            var editable = Source as IEditable;
            if (editable.IsInRejectChanges)
                return;

            if (e.PropertyName.EqIgnoreCase(BillWorkActDetail.Operation2ContractIDPropertyName))
            {
                GetCause();
                GetFormula();
                ValidateCalcFields();

                if (Source.IsNew)
                {
                    try
                    {
                        _doNotCalculate = true;

                        for (var i = 1; i <= 10; i++)
                        {
                            Source.SetProperty(
                                string.Format("{0}{1}", BillWorkActDetail.WorkActDetailCauseBasePropertyName, i),
                                null);
                        }
                    }
                    finally
                    {
                        _doNotCalculate = false;
                    }
                }

                RefreshView();
                OnCalculate();
            }
            else if (e.PropertyName.ToUpper().Contains(BillWorkActDetail.WorkActDetailCauseBasePropertyName))
            {
                OnCalculate();
            }
            else if (e.PropertyName.EqIgnoreCase(BillWorkActDetail.IsEnabledTotalSumPropertyName))
            {
                if (!Source.IsEnabledTotalSum)
                    OnCalculate();
            }
        }

        protected override ObservableCollection<DataField> GetFields(SettingDisplay displaySetting)
        {
            //const string numberformat = "#0.####";
            const string numberformat = "N4";
            var fields = base.GetFields(displaySetting);

            var totalsumField = fields.SingleOrDefault(p => p.Name.EqIgnoreCase(BillWorkActDetail.WorkActDetailTotalSumPropertyName));
            if (totalsumField == null)
                throw new DeveloperException("Property '{0}' is not exist in BillWorkActDetail.", BillWorkActDetail.WorkActDetailTotalSumPropertyName);

            var countField = fields.SingleOrDefault(p => p.Name.EqIgnoreCase(BillWorkActDetail.WorkActDetailCountPropertyName));
            if (countField == null)
                throw new DeveloperException("Property '{0}' is not exist in BillWorkActDetail.", BillWorkActDetail.WorkActDetailCountPropertyName);

            var multiField = fields.SingleOrDefault(p => p.Name.EqIgnoreCase(BillWorkActDetail.WorkActDetailMultPropertyName));
            if (multiField == null)
                throw new DeveloperException("Property '{0}' is not exist in BillWorkActDetail.", BillWorkActDetail.WorkActDetailMultPropertyName);

            var isFormulasExist = IsFormulasExist();
            Source.IsEnabledCalc = !isFormulasExist;

            totalsumField.Set(ValueDataFieldConstants.PropertiesBinding, new Dictionary<DependencyProperty, Binding>
            {
                {
                    UIElement.IsEnabledProperty, new Binding(BillWorkActDetail.IsEnabledTotalSumPropertyName)
                    {
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    }
                }
            });
            if (string.IsNullOrEmpty(totalsumField.DisplayFormat))
                totalsumField.DisplayFormat = numberformat;

            var binding = new Binding(BillWorkActDetail.IsEnabledCalcPropertyName)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            countField.Set(ValueDataFieldConstants.PropertiesBinding, new Dictionary<DependencyProperty, Binding>
            {
                {
                    UIElement.IsEnabledProperty, binding
                }
            });
            if (string.IsNullOrEmpty(countField.DisplayFormat))
                countField.DisplayFormat = numberformat;

            multiField.Set(ValueDataFieldConstants.PropertiesBinding, new Dictionary<DependencyProperty, Binding>
            {
                {
                    UIElement.IsEnabledProperty, binding
                }
            });
            if (string.IsNullOrEmpty(multiField.DisplayFormat))
                multiField.DisplayFormat = numberformat;

            var editableTotalSumField = new DataField
            {
                FieldType = typeof (bool),
                Name = BillWorkActDetail.IsEnabledTotalSumPropertyName,
                SourceName = BillWorkActDetail.IsEnabledTotalSumPropertyName,
                FieldName = BillWorkActDetail.IsEnabledTotalSumPropertyName,
                EnableEdit = isFormulasExist,
                IsEnabled = isFormulasExist,
                Visible = isFormulasExist,
                Caption = string.Format("Редактировать поле '{0}'", totalsumField.Caption),
                Description = string.Format("Признак редактирования поля '{0}'", totalsumField.Caption)
            };

            var index = fields.IndexOf(totalsumField);
            if (index >= 0)
                fields.Insert(index + 1, editableTotalSumField);
            else
                fields.Add(editableTotalSumField);

            foreach (var dataField in fields.Where(dataField => dataField.SourceName.ToUpper().Contains(BillWorkActDetail.WorkActDetailCauseBasePropertyName)).Where(dataField => dataField.Visible))
            {
                dataField.Visible = false;
            }

            if (_causes == null)
                return fields;

            foreach (var cause in _causes)
            {
                var field = fields.FirstOrDefault(f => f.FieldName.Equals(BillWorkActDetail.WorkActDetailCauseBasePropertyName + cause.OperationCauseOrdinal));
                if (field == null) 
                    continue;

                field.IsChangeLookupCode = true;
                field.Caption = cause.OperationCauseName;
                field.Visible = true;

                if (cause.OperationCauseCpvL != null && cause.OperationCauseCpvL.Count > 0)
                {
                    //тип основания
                    var cpv = cause.OperationCauseCpvL.SingleOrDefault(p => p != null && p.CustomParamCode.EqIgnoreCase("BillOCViewEntityL2"));
                    int typeid;
                    if (cpv != null && !string.IsNullOrEmpty(cpv.CPVValue) && int.TryParse(cpv.CPVValue, out typeid))
                        field.FieldType = IoC.Instance.Resolve<ISysObjectManager>().GetTypeBySysObjectId(typeid);

                    //формат отображения
                    cpv = cause.OperationCauseCpvL.SingleOrDefault(p => p != null && p.CustomParamCode.EqIgnoreCase("BillOCViewFormatL2"));
                    if (cpv != null && !string.IsNullOrEmpty(cpv.CPVValue))
                        field.DisplayFormat = cpv.CPVValue;

                    //лукап
                    cpv = cause.OperationCauseCpvL.SingleOrDefault(p => p != null && p.CustomParamCode.EqIgnoreCase("BillOCViewL1"));
                    if (cpv != null && !string.IsNullOrEmpty(cpv.CPVValue))
                        field.LookupCode = cpv.CPVValue;

                    //IsReadonly
                    cpv = cause.OperationCauseCpvL.SingleOrDefault(p => p != null && p.CustomParamCode.EqIgnoreCase("BillOCViewReadOnlyL2"));
                    bool isreadonly;
                    if (cpv != null && bool.TryParse(cpv.CPVValue, out isreadonly)) 
                        field.IsEnabled = isreadonly != true;

                    //Visibility
                    cpv = cause.OperationCauseCpvL.SingleOrDefault(p => p != null && p.CustomParamCode.EqIgnoreCase("BillOCViewViewL2"));
                    bool visible;
                    if (cpv != null && bool.TryParse(cpv.CPVValue, out visible))
                        field.Visible = visible;

                    //Пост-обработка
                    if (field.FieldType != null)
                    {
                        //Начальное значение
                        var cpvdefvalue = cause.OperationCauseCpvL.SingleOrDefault(p => p != null && p.CustomParamCode.EqIgnoreCase("BillOCViewDefaultValue"));
                        if (cpvdefvalue != null && !string.IsNullOrEmpty(cpvdefvalue.CPVValue))
                            Source.SetProperty(field.Name, cpvdefvalue.CPVValue);

                        var type = field.FieldType.GetNonNullableType();
                        var isnotlookup = string.IsNullOrEmpty(field.LookupCode);
                        //Если лукап конверторы добавляем, но дисплей формат для чисел с плавающей точкой не задаем
                        if (type == typeof (DateTime))
                        {
                            field.Set(ValueDataFieldConstants.BindingIValueConverter, new StringToDateTimeConverter());
                            var parameter = new List<string>();
                            
                            if (string.IsNullOrEmpty(field.DisplayFormat))
                                field.DisplayFormat = DefaultDateTimeDisplayFormat;
                            //DisplayFormat должен быть первым в списке форматов
                            parameter.Add(field.DisplayFormat);
                            field.Set(ValueDataFieldConstants.Parameter, parameter.ToArray());
                        }
                        else if (type.IsPrimitive || type == typeof(decimal))
                        {
                            field.Set(ValueDataFieldConstants.BindingIValueConverter, new StringToNumericConverter());
                            field.Set(ValueDataFieldConstants.Parameter, field.FieldType);
                            if (isnotlookup && type != typeof(bool))
                                field.Set(ValueDataFieldConstants.UseSpinEdit, true);
                        }

                        if (isnotlookup && string.IsNullOrEmpty(field.DisplayFormat) &&
                            ((type == typeof(float) || type == typeof(double) || type == typeof(decimal))))
                            field.DisplayFormat = numberformat;
                    }
                }
            }

            var displayFormats = fields.Where(
                p => p.FieldType.GetNonNullableType() == typeof (DateTime) && !string.IsNullOrEmpty(p.DisplayFormat))
                .Select(p => p.DisplayFormat)
                .ToList();

            var formats = new []
            {
                DefaultDateTimeDisplayFormat,
                "dd MMM yyyy", "dd MMM yyyy HH:mm:ss", "dd MMM yyyy HH:mm", "dd MMM yyyy HH", 
                "dd MMM yy", "dd MMM yy HH:mm:ss", "dd MMM yy HH:mm",  "dd MMM yy HH",
                "g", "G", "d", "D",
                "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy HH:mm", "dd.MM.yyyy HH",
                "dd.MM.yy", "dd.MM.yy HH:mm:ss", "dd.MM.yy HH:mm", "dd.MM.yy HH", 
                "yyyyMMdd HH:mm:ss"
            };

            displayFormats.AddRange(formats);
            _dateTimeFormats = displayFormats.Distinct().ToArray();

            var datetimefields = fields.Where(p => p.FieldType.GetNonNullableType() == typeof(DateTime) && p.Properties.ContainsKey(ValueDataFieldConstants.Parameter)).ToArray();
            foreach (var f in datetimefields)
            {
                var parameter = f.Get<object>(ValueDataFieldConstants.Parameter) as string[];
                if (parameter != null)
                {
                    var parameters = new List<string>(parameter);
                    parameters.AddRange(_dateTimeFormats);
                    f.Set(ValueDataFieldConstants.Parameter, parameters.Distinct().ToArray());
                }
            }

            return fields;
        }

        //Получаем оснвания
        private void GetCause()
        {
            using (var mgr = (IBillWorkActDetailManager) GetManager())
            {
                _causes = mgr.GetCause(Source);
            }
        }

        //Получаем формулы расчета параметров
        private void GetFormula()
        {
            _billOperation2Contract = null;

            //Получаем формулы расчета параметров
            if (Source.Operation2ContractID.HasValue)
            {
                using (var manager = IoC.Instance.Resolve<IBaseManager<BillOperation2Contract>>())
                {
                    _billOperation2Contract = manager.Get(Source.Operation2ContractID);
                }
            }

            if (Source.WorkActID.HasValue)
            {
                using (var mgrBillWorkAct = IoC.Instance.Resolve<IBaseManager<BillWorkAct>>())
                {
                    var workact = mgrBillWorkAct.Get(Source.WorkActID);
                    _defaltCalcDate = workact.WORKACTDATE;
                }                
            }
        }

        private bool IsFormulasExist()
        {
            return !(_billOperation2Contract == null || _billOperation2Contract.Operation2ContractCpvL == null ||
                     _billOperation2Contract.Operation2ContractCpvL.Count == 0);
        }

        private void OnCalculate()
        {
            if (_doNotCalculate || !IsFormulasExist())
                return;

            lock (_lock)
            {
                Calc();
                //OnBpExecute();
            }
        }

        private BpContext CreateContext()
        {
            var result = new BpContext
            {
                Items = new object[] {Source},
            };

            if (_billOperation2Contract != null)
            {
                result.Properties["CPVL"] = _billOperation2Contract.Operation2ContractCpvL;
            }
            result.Properties["dateTimeFormats"] = _dateTimeFormats;
            result.Properties["WORKACTDATE"] = _defaltCalcDate;

            return result;
        }

        private void Calc()
        {
            var cpv = _billOperation2Contract.Operation2ContractCpvL;
            if (cpv != null)
            {
                var formulaDate = cpv.FirstOrDefault(p => p != null && p.CustomParamCode.EqIgnoreCase("BillO2CCalcFDateL2"));
                var formulaDateStr = formulaDate != null && _dateTimeFormats != null ? formulaDate.CPVValue : string.Empty;

                var formulaCount = cpv.FirstOrDefault(p => p != null && p.CustomParamCode.EqIgnoreCase("BillO2CCalcFCountL2"));
                var formulaCountStr = formulaCount != null ? formulaCount.CPVValue : string.Empty;

                var formulaFactor = cpv.FirstOrDefault(p => p != null && p.CustomParamCode.EqIgnoreCase("BillO2CCalcFFactorL2"));
                var formulaFactorStr = formulaFactor != null  ? formulaFactor.CPVValue : string.Empty;

                var formulaSumm = cpv.FirstOrDefault(p => p != null && p.CustomParamCode.EqIgnoreCase("BillO2CCalcFSummL2"));
                var formulaSummStr = formulaSumm != null  ? formulaSumm.CPVValue : string.Empty;

                var engine = new CalcEngine.CalcEngine {DataContext = Source};

                //дата
                DateTime? date;
                if (!formulaDateStr.IsNullOrEmptyAfterTrim())
                {
                    var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
                    try
                    {
                        date = new StringToDateTimeConverter().Convert(engine.Evaluate(formulaDateStr), null, _dateTimeFormats, culture) as DateTime?;
                    }
                    catch (Exception)
                    {
                        date = null;
                    }
                }
                else
                {
                    date = _defaltCalcDate;
                }

                //цена
                double price = 0;
                if (date.HasValue)
                {
                    using (var mgrTariff = IoC.Instance.Resolve<IBaseManager<BillTariff>>())
                    {
                        var filter =
                            string.Format(
                                "operation2contractid_r = {0} and TO_DATE('{1}','YYYY.MM.DD') >= tariffdatefrom and TO_DATE('{1}','YYYY.MM.DD') < tariffdatetill",
                                Source.Operation2ContractID, ((DateTime)date).ToString("yyyy.MM.dd"));
                        var values = mgrTariff.GetFiltered(filter).ToArray();
                        if (values.Length == 1)
                        {
                            price = values[0].Value;
                        }
                    }
                }
                engine.Variables.Add("PRICE", price);

                //кол-во
                double count;
                if (!formulaCountStr.IsNullOrEmptyAfterTrim())
                {
                    try
                    {
                        if (!double.TryParse(engine.Evaluate(formulaCountStr).ToString(), out count))
                            count = 0;
                    }
                    catch (Exception)
                    {
                        count = 0;
                    }
                }
                else
                {
                    count = DefaultCalcCount;
                }
                Source.WorkActDetailCount = count;

                //коэффициент
                double factor;
                if (!formulaFactorStr.IsNullOrEmptyAfterTrim())
                {
                    try
                    {
                        if (!double.TryParse(engine.Evaluate(formulaFactorStr).ToString(), out factor))
                            factor = 0;
                    }
                    catch (Exception)
                    {
                        factor = 0;
                    }
                }
                else
                {
                    factor = DefaultCalcFactor;
                }
                Source.WorkActDetailMulti = factor;

                //сумма
                if (!Source.IsEnabledTotalSum)
                {
                    double summ = 0;
                    if (!formulaSummStr.IsNullOrEmptyAfterTrim())
                    {
                        try
                        {
                            if (!double.TryParse(engine.Evaluate(formulaSummStr).ToString(), out summ))
                                summ = 0;
                        }
                        catch (Exception)
                        {
                            summ = 0;
                        }
                    }
                    Source.WorkActDetailTotalSum = summ;
                }
            }
        }

        protected virtual void OnBpExecute()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;

            try
            {
                WaitStart();
                _bpStartTime = DateTime.Now;
                var managerInstance = IoC.Instance.Resolve<IBPProcessManager>();
                var context = CreateContext();
                managerInstance.Parameters.Add(BpContext.BpContextArgumentName, context);
                _log.DebugFormat("Start process: {0}", BpCode);
                managerInstance.Run(code: BpCode, completedHandler: OnBpProcessEnd);
            }
            catch (Exception)
            {
                WaitStop();
                throw;
            }
            finally
            {
                _log.DebugFormat("Start process: {0} in {1}", BpCode, DateTime.Now - _bpStartTime);
            }
        }

        protected override void OnBpProcessEnd(CompleteContext context)
        {
            _log.DebugFormat("End process: {0} in {1}", BpCode, DateTime.Now - _bpStartTime);
            WaitStop();
        }
    }
}
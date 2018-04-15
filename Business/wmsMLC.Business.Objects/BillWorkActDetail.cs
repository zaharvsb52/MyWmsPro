namespace wmsMLC.Business.Objects
{
    public class BillWorkActDetail : WMSBusinessObject
    {
        #region . Constants .
        public const string WorkActDetailIDPropertyName = "WORKACTDETAILID";
        public const string WorkActIDPropertyName = "WORKACTID_R";
        public const string Operation2ContractIDPropertyName = "OPERATION2CONTRACTID_R";
        public const string TransactionIDPropertyName = "TRANSACTIONID_R";
        public const string WorkActDetailManualPropertyName = "WORKACTDETAILMANUAL";
        public const string WorkActDetailCauseBasePropertyName = "WORKACTDETAILCAUSE";
        public const string WorkActDetailCause1PropertyName = WorkActDetailCauseBasePropertyName + "1";
        public const string WorkActDetailCause2PropertyName = WorkActDetailCauseBasePropertyName + "2";
        public const string WorkActDetailCause3PropertyName = WorkActDetailCauseBasePropertyName + "3";
        public const string WorkActDetailCause4PropertyName = WorkActDetailCauseBasePropertyName + "4";
        public const string WorkActDetailCause5PropertyName = WorkActDetailCauseBasePropertyName + "5";
        public const string WorkActDetailCause6PropertyName = WorkActDetailCauseBasePropertyName + "6";
        public const string WorkActDetailCause7PropertyName = WorkActDetailCauseBasePropertyName + "7";
        public const string WorkActDetailCause8PropertyName = WorkActDetailCauseBasePropertyName + "8";
        public const string WorkActDetailCause9PropertyName = WorkActDetailCauseBasePropertyName + "9";
        public const string WorkActDetailCause10PropertyName = WorkActDetailCauseBasePropertyName + "10";
        public const string WorkActDetailSum1PropertyName = "WORKACTDETAILSUM1";
        public const string WorkActDetailSum2PropertyName = "WORKACTDETAILSUM2";
        public const string WorkActDetailSum3PropertyName = "WORKACTDETAILSUM3";
        public const string WorkActDetailSum4PropertyName = "WORKACTDETAILSUM4";
        public const string WorkActDetailSum5PropertyName = "WORKACTDETAILSUM5";
        public const string WorkActDetailSum6PropertyName = "WORKACTDETAILSUM6";
        public const string WorkActDetailSum7PropertyName = "WORKACTDETAILSUM7";
        public const string WorkActDetailSum8PropertyName = "WORKACTDETAILSUM8";
        public const string WorkActDetailSum9PropertyName = "WORKACTDETAILSUM9";
        public const string WorkActDetailSum10PropertyName = "WORKACTDETAILSUM10";
        public const string WorkActDetailCountPropertyName = "WORKACTDETAILCOUNT";
        public const string WorkActDetailTotalSumPropertyName = "WORKACTDETAILTOTALSUM";
        public const string WorkActDetailMultPropertyName = "WORKACTDETAILMULT";
        public const string IsEnabledTotalSumPropertyName = "IsEnabledTotalSum";
        public const string IsEnabledCalcPropertyName = "IsEnabledCalc";
        #endregion . Constants .

        #region .  Properties .
        public bool WorkActDetailManual
        {
            get { return GetProperty<bool>(WorkActDetailManualPropertyName); }
            set { SetProperty(WorkActDetailManualPropertyName, value); }
        }

        public decimal? Operation2ContractID
        {
            get { return GetProperty<decimal?>(Operation2ContractIDPropertyName); }
            set { SetProperty(Operation2ContractIDPropertyName, value); }
        }

        public double? WorkActDetailTotalSum
        {
            get { return GetProperty<double?>(WorkActDetailTotalSumPropertyName); }
            set { SetProperty(WorkActDetailTotalSumPropertyName, value); }
        }

        public double? WorkActDetailMulti
        {
            get { return GetProperty<double?>(WorkActDetailMultPropertyName); }
            set { SetProperty(WorkActDetailMultPropertyName, value); }
        }

        public double? WorkActDetailCount
        {
            get { return GetProperty<double?>(WorkActDetailCountPropertyName); }
            set { SetProperty(WorkActDetailCountPropertyName, value); }
        }

        public string WorkActDetailCause1
        {
            get { return GetProperty<string>(WorkActDetailCause1PropertyName); }
            set { SetProperty(WorkActDetailCause1PropertyName, value); }
        }

        public string WorkActDetailCause2
        {
            get { return GetProperty<string>(WorkActDetailCause2PropertyName); }
            set { SetProperty(WorkActDetailCause2PropertyName, value); }
        }

        public string WorkActDetailCause3
        {
            get { return GetProperty<string>(WorkActDetailCause3PropertyName); }
            set { SetProperty(WorkActDetailCause3PropertyName, value); }
        }

        public string WorkActDetailCause4
        {
            get { return GetProperty<string>(WorkActDetailCause4PropertyName); }
            set { SetProperty(WorkActDetailCause4PropertyName, value); }
        }

        public string WorkActDetailCause5
        {
            get { return GetProperty<string>(WorkActDetailCause5PropertyName); }
            set { SetProperty(WorkActDetailCause5PropertyName, value); }
        }

        public string WorkActDetailCause6
        {
            get { return GetProperty<string>(WorkActDetailCause6PropertyName); }
            set { SetProperty(WorkActDetailCause6PropertyName, value); }
        }

        public string WorkActDetailCause7
        {
            get { return GetProperty<string>(WorkActDetailCause7PropertyName); }
            set { SetProperty(WorkActDetailCause7PropertyName, value); }
        }

        public string WorkActDetailCause8
        {
            get { return GetProperty<string>(WorkActDetailCause8PropertyName); }
            set { SetProperty(WorkActDetailCause8PropertyName, value); }
        }

        public string WorkActDetailCause9
        {
            get { return GetProperty<string>(WorkActDetailCause9PropertyName); }
            set { SetProperty(WorkActDetailCause9PropertyName, value); }
        }

        public string WorkActDetailCause10
        {
            get { return GetProperty<string>(WorkActDetailCause10PropertyName); }
            set { SetProperty(WorkActDetailCause10PropertyName, value); }
        }

        public decimal? WorkActID
        {
            get { return GetProperty<decimal?>(WorkActIDPropertyName); }
            set { SetProperty(WorkActIDPropertyName, value); }
        }

        private bool _isEnabledTotalSum;
        /// <summary>
        /// Признак редактирования свойства "Итоговая сумма".
        /// </summary>
        public bool IsEnabledTotalSum
        {
            get { return _isEnabledTotalSum; }
            set
            {
                if (_isEnabledTotalSum == value)
                    return;
                _isEnabledTotalSum = value;
                OnPropertyChanged(IsEnabledTotalSumPropertyName);
            }
        }

        private bool _isEnabledCalc;
        /// <summary>
        /// Признак редактирования" вычисляемых полей.
        /// </summary>
        public bool IsEnabledCalc
        {
            get { return _isEnabledCalc; }
            set
            {
                if (_isEnabledCalc == value)
                    return;
                _isEnabledCalc = value;
                OnPropertyChanged(IsEnabledCalcPropertyName);
            }
        }

        #endregion .  Properties .
    }
}
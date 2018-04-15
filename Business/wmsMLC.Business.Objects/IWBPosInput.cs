using System;
using System.Collections.Specialized;
using System.Linq;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation;
using wmsMLC.General.BL.Validation.Attributes;

namespace wmsMLC.Business.Objects
{
    public class IWBPosInput : WMSBusinessObject
    {
        #region . Constants .

        private decimal SKU2TTEQuantityDefault;

        public const string QlfTypeDefect = "QLFDEFECT";

        public bool IsBaseSKU;

        public const string IWBPosIdPropertyName = "IWBPOSID";
        public const string SKUIDPropertyName = "SKUID";
        public const string IWBPosCountPropertyName = "IWBPOSCOUNT";
        public const string RequiredSKUCountPropertyName = "REQUIREDSKUCOUNT";
        public const string TETypeCodePropertyName = "TETYPECODE";
        public const string SKU2TTEQuantityPropertyName = "SKU2TTEQUANTITY";
        public const string SKU2TTEQuantityMaxPropertyName = "SKU2TTEQUANTITYMAX";
        public const string SKU2TTEHeightPropertyName = "SKU2TTEHEIGHT";
        public const string QLFCodePropertyName = "QLFCODE_R";
        public const string IWBPosBlockingPropertyName = "IWBPOSBLOCKING";
        public const string IWBPosExpiryDatePropertyName = "IWBPOSEXPIRYDATE";
        public const string ProductCountSKUPropertyName = "PRODUCTCOUNTSKU";
        public const string ProductCountPropertyName = "PRODUCTCOUNT";
        public const string ArtNamePropertyName = "ARTNAME";
        public const string ArtCodePropertyName = "ARTCODE";
        public const string ArtDescPropertyName = "ARTDESC";
        public const string MeasureCodePropertyName = "MEASURECODE";
        public const string IWBPosColorPropertyName = "IWBPOSCOLOR";
        public const string IWBPosTonePropertyName = "IWBPOSTONE";
        public const string IWBPOSLOTPropertyName = "IWBPOSLOT";
        public const string IWBPosSizePropertyName = "IWBPOSSIZE";
        public const string IWBPosBatchPropertyName = "IWBPOSBATCH";
        public const string IWBPosProductDatePropertyName = "IWBPOSPRODUCTDATE";
        public const string IWBPosSerialNumberPropertyName = "IWBPOSSERIALNUMBER";
        public const string FACTORYID_RPropertyName = "FACTORYID_R";
        public const string ArtInputDateMethodPropertyName = "ARTINPUTDATEMETHOD";
        public const string QLFDetailLPropertyName = "IWBPOSQLFDETAILDESCL";
        public const string IWBPosTEPropertyName = "IWBPOSTE";
        public const string IWBPosInputBatchCodePropertyName = "IWBPOSINPUTBATCHCODE";
        public const string BatchcodeErrorMessagePropertyName = "BatchcodeErrorMessage";
        public const string SKUNAMEPropertyName = "SKUNAME";
        public const string RemainCountPropertyName = "REMAINCOUNT";
        public const string IWBPOSBOXNUMBERPropertyName = "IWBPOSBOXNUMBER";
        public const string POSSKUIDPropertyName = "POSSKUID";
        public const string POSPRODUCTCOUNTPropertyName = "POSPRODUCTCOUNT";
        public const string POSIWBPOSCOUNTPropertyName = "POSIWBPOSCOUNT";

        public const string VSKUHEIGHTPropertyName = "VSKUHEIGHT";
        public const string VSKULENGTHPropertyName = "VSKULENGTH";
        public const string VSKUWIDTHPropertyName = "VSKUWIDTH";

        public const string IDPropertyName = "ID";
        public const string ParentIDPropertyName = "PARENTID";
        public const string PLACECODE_RPropertyName = "PLACECODE_R";
        #endregion . Constants .

        #region . Properties .

        public decimal? Id
        {
            get { return GetProperty<decimal?>(IDPropertyName); }
            set { SetProperty(IDPropertyName, value); }
        }

        public decimal? ParentId
        {
            get { return GetProperty<decimal?>(ParentIDPropertyName); }
            set { SetProperty(ParentIDPropertyName, value); }
        }

        /// <summary>
        /// Место приемки данной позиции
        /// </summary>
        public string PlaceCode
        {
            get { return GetProperty<string>(PLACECODE_RPropertyName); }
            set { SetProperty(PLACECODE_RPropertyName, value); }
        }

        /// <summary>
        /// SKU.
        /// </summary>
        public decimal? SKUID
        {
            get { return GetProperty<decimal?>(SKUIDPropertyName); }
            set { SetProperty(SKUIDPropertyName, value); }
        }

        /// <summary>
        /// IWBPosId
        /// </summary>
        public decimal IWBPosId
        {
            get { return GetProperty<decimal>(IWBPosIdPropertyName); }
            set { SetProperty(IWBPosIdPropertyName, value); }
        }

        /// <summary>
        /// SKU позиции.
        /// </summary>
        public decimal? POSSKUID
        {
            get { return GetProperty<decimal?>(POSSKUIDPropertyName); }
            set { SetProperty(POSSKUIDPropertyName, value); }
        }

        /// <summary>
        /// Требуемое кол-во SKU. 
        /// </summary>
        public double IWBPosCount
        {
            get { return GetProperty<double>(IWBPosCountPropertyName); }
            set { SetProperty(IWBPosCountPropertyName, value); }
        }

        /// <summary>
        /// Количество по документу в позиции. 
        /// </summary>
        public double POSIWBPOSCOUNT
        {
            get { return GetProperty<double>(POSIWBPOSCOUNTPropertyName); }
            set { SetProperty(POSIWBPOSCOUNTPropertyName, value); }
        }

        /// <summary>
        /// Принимаемое количество SKU.
        /// </summary>
        public decimal RequiredSKUCount
        {
            get { return GetProperty<decimal>(RequiredSKUCountPropertyName); }
            set { SetProperty(RequiredSKUCountPropertyName, value); }
        }

        public double? RemainCount
        {
            get { return GetProperty<double?>(RemainCountPropertyName); }
            set { SetProperty(RemainCountPropertyName, value); }
        }

        /// <summary>
        /// Тип ТЕ.
        /// </summary>
        public string TETypeCode
        {
            get { return GetProperty<string>(TETypeCodePropertyName); }
            set { SetProperty(TETypeCodePropertyName, value); }
        }

        /// <summary>
        /// Затарка ТЕ.
        /// </summary>
        public decimal SKU2TTEQuantity
        {
            get { return GetProperty<decimal>(SKU2TTEQuantityPropertyName); }
            set { SetProperty(SKU2TTEQuantityPropertyName, value); }
        }

        /// <summary>
        /// Затарка ТЕ Max.
        /// </summary>
        public decimal SKU2TTEQuantityMax
        {
            get { return GetProperty<decimal>(SKU2TTEQuantityMaxPropertyName); }
            set { SetProperty(SKU2TTEQuantityMaxPropertyName, value); }
        }

        /// <summary>
        /// Высота ТЕ с товаром.
        /// </summary>
        public decimal SKU2TTEHeight
        {
            get { return GetProperty<decimal>(SKU2TTEHeightPropertyName); }
            set { SetProperty(SKU2TTEHeightPropertyName, value); }
        }

        /// <summary>
        /// Квалификация.
        /// </summary>
        [Revalidate(QLFDetailLPropertyName)]
        public string QLFCODE_R
        {
            get { return GetProperty<string>(QLFCodePropertyName); }
            set { SetProperty(QLFCodePropertyName, value); }
        }

        /// <summary>
        /// Блокировка.
        /// </summary>
        public string IWBPosBlocking
        {
            get { return GetProperty<string>(IWBPosBlockingPropertyName); }
            set { SetProperty(IWBPosBlockingPropertyName, value); }
        }

        /// <summary>
        /// Срок годности.
        /// </summary>
        [Revalidate(IWBPosExpiryDatePropertyName)]
        public DateTime? IWBPosExpiryDate
        {
            get { return GetProperty<DateTime?>(IWBPosExpiryDatePropertyName); }
            set { SetProperty(IWBPosExpiryDatePropertyName, value); }
        }

        /// <summary>
        /// Принятое кол-во товара.
        /// </summary>
        public double ProductCountSKU
        {
            get { return GetProperty<double>(ProductCountSKUPropertyName); }
            set { SetProperty(ProductCountSKUPropertyName, value); }
        }

        /// <summary>
        /// Кол-во в основных единицах.
        /// </summary>
        public double ProductCount
        {
            get { return GetProperty<double>(ProductCountPropertyName); }
            set { SetProperty(ProductCountPropertyName, value); }
        }

        /// <summary>
        /// Кол-во в основных единицах в позиции.
        /// </summary>
        public double POSPRODUCTCOUNT
        {
            get { return GetProperty<double>(POSPRODUCTCOUNTPropertyName); }
            set { SetProperty(POSPRODUCTCOUNTPropertyName, value); }
        }

        /// <summary>
        /// Код артикула.
        /// </summary>
        public string ArtCode
        {
            get { return GetProperty<string>(ArtCodePropertyName); }
            set { SetProperty(ArtCodePropertyName, value); }
        }

        /// <summary>
        /// Название артикула.
        /// </summary>
        public string ArtName
        {
            get { return GetProperty<string>(ArtNamePropertyName); }
            set { SetProperty(ArtNamePropertyName, value); }
        }

        /// <summary>
        /// Описание артикула.
        /// </summary>
        public string ArtDesc
        {
            get { return GetProperty<string>(ArtDescPropertyName); }
            set { SetProperty(ArtDescPropertyName, value); }
        }

        /// <summary>
        /// ЕИ.
        /// </summary>
        public string MeasureCode
        {
            get { return GetProperty<string>(MeasureCodePropertyName); }
            set { SetProperty(MeasureCodePropertyName, value); }
        }

        /// <summary>
        /// Цвет.
        /// </summary>
        public string IWBPosColor
        {
            get { return GetProperty<string>(IWBPosColorPropertyName); }
            set { SetProperty(IWBPosColorPropertyName, value); }
        }

        /// <summary>
        /// Тон.
        /// </summary>
        public string IWBPosTone
        {
            get { return GetProperty<string>(IWBPosTonePropertyName); }
            set { SetProperty(IWBPosTonePropertyName, value); }
        }

        /// <summary>
        /// Размер.
        /// </summary>
        public string IWBPosSize
        {
            get { return GetProperty<string>(IWBPosSizePropertyName); }
            set { SetProperty(IWBPosSizePropertyName, value); }
        }

        /// <summary>
        /// Партия.
        /// </summary>
        public string IWBPosBatch
        {
            get { return GetProperty<string>(IWBPosBatchPropertyName); }
            set { SetProperty(IWBPosBatchPropertyName, value); }
        }

        /// <summary>
        /// Дата производства.
        /// </summary>
        [Revalidate(IWBPosProductDatePropertyName)]
        public DateTime? IWBPosProductDate
        {
            get { return GetProperty<DateTime?>(IWBPosProductDatePropertyName); }
            set { SetProperty(IWBPosProductDatePropertyName, value); }
        }

        /// <summary>
        /// Серийный номер.
        /// </summary>
        public string IWBPosSerialNumber
        {
            get { return GetProperty<string>(IWBPosSerialNumberPropertyName); }
            set { SetProperty(IWBPosSerialNumberPropertyName, value); }
        }

        /// <summary>
        /// Код фабрики.
        /// </summary>
        public decimal? FactoryID_R
        {
            get { return GetProperty<decimal?>(FACTORYID_RPropertyName); }
            set { SetProperty(FACTORYID_RPropertyName, value); }
        }

        /// <summary>
        /// Метод артикула.
        /// </summary>
        public string ArtInputDateMethod
        {
            get { return GetProperty<string>(ArtInputDateMethodPropertyName); }
            set { SetProperty(ArtInputDateMethodPropertyName, value); }
        }

        /// <summary>
        /// Метод артикула.
        /// </summary>
        public WMSBusinessCollection<IWBPosQLFDetailDesc> QLFDetailL
        {
            get { return GetProperty<WMSBusinessCollection<IWBPosQLFDetailDesc>>(QLFDetailLPropertyName); }
            set { SetProperty(QLFDetailLPropertyName, value); }
        }

        /// <summary>
        /// ТЕ для размещения.
        /// </summary>
        public string IWBPosTE
        {
            get { return GetProperty<string>(IWBPosTEPropertyName); }
            set { SetProperty(IWBPosTEPropertyName, value); }
        }

        /// <summary>
        /// Номер короба.
        /// </summary>
        public string IwbPosBoxNumber
        {
            get { return GetProperty<string>(IWBPOSBOXNUMBERPropertyName); }
            set { SetProperty(IWBPOSBOXNUMBERPropertyName, value); }
        }

        /// <summary>
        /// Признак того, что запись выбрана.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Флаг управления приемкой
        /// </summary>
        public string ManageFlag { get; set; }

        public string IWBPosInputBatch
        {
            get { return GetProperty<string>(IWBPosInputBatchCodePropertyName); }
            set { SetProperty(IWBPosInputBatchCodePropertyName, value); }
        }

        public bool IsBpBatchcodeCompleted { get; set; }

        /// <summary>
        /// Если значение свойства true, то IsBpBatchcodeCompleted = false, но можно продолжать БП (Не настроен BpBatch, неопределен Batchcode).
        /// </summary>
        public bool NotCriticalBatchcodeError { get; set; }

        private string _batchcodeErrorMessage;
        public string BatchcodeErrorMessage
        {
            get { return _batchcodeErrorMessage; }
            set
            {
                if (_batchcodeErrorMessage == value)
                    return;
                _batchcodeErrorMessage = value;
                OnPropertyChanged(BatchcodeErrorMessagePropertyName);
            }
        }

        public string SKUNAME
        {
            get { return GetProperty<string>(SKUNAMEPropertyName); }
            set { SetProperty(SKUNAMEPropertyName, value); }
        }

        // отмена калькуляции
        public bool DisableCalculate { get; set; }

        public decimal? OverrideSKU2TTEQuantityMax { get; set; }

        public decimal? LastSkuId { get; set; }

        public double IwbPosCountDouble { get; set; }
        public double ProductCountSkuDouble { get; set; }

        #endregion . Properties .

        #region . Methods .

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (InSuspendNotifications)
                return;

            if (propertyName.EqIgnoreCase(IWBPosInputBatchCodePropertyName))
            {
                IsBpBatchcodeCompleted = false;
                NotCriticalBatchcodeError = false;
            }

            var editable = this as IEditable;
            if (editable.IsInRejectChanges)
                return;

            if (propertyName.EqIgnoreCase(IWBPosIdPropertyName))
                CalculateIWBPosId();

            if (propertyName.EqIgnoreCase(SKUIDPropertyName))
                CalculateSKU();

            if (propertyName.EqIgnoreCase(TETypeCodePropertyName))
                CalculateTEType();

            if (propertyName.EqIgnoreCase(SKU2TTEQuantityPropertyName))
                CalculateSKU2TTEQuantity();

            if (propertyName.EqIgnoreCase(RequiredSKUCountPropertyName))
            {
                SuspendNotifications();
                RequiredSKUCount = Math.Floor(RequiredSKUCount);
                ResumeNotifications();
            }
        }

        public void CalculateIWBPosId()
        {
            var key = GetKey();
            var keyStr = (key != null) ? key.ToString() : string.Empty;
            if (!string.IsNullOrEmpty(keyStr))
            {
                using (var productMgr = IoC.Instance.Resolve<IBaseManager<Product>>())
                {
                    var filter = string.Format("IWBPosID_r={0}", keyStr);
                    ProductCountSKU = (int)productMgr.GetFiltered(filter).Sum(i => i.ProductCountSKU);
                    //RequiredSKUCount = (int)(IWBPosCount - ProductCountSKU);
                    RemainCount = IWBPosCount - ProductCountSKU;
                    RequiredSKUCount = (int)RemainCount;
                    if (RequiredSKUCount < 1)
                    {
                        RequiredSKUCount = 0;
                    }
                }
            }
        }

        public void CheckIsBaseSKU()
        {
            var val = SKUID;
            if (val != null)
            {
                SKU sku;
                using (var skuMgr = IoC.Instance.Resolve<IBaseManager<SKU>>())
                    sku = skuMgr.Get(val);
                if (sku != null)
                    IsBaseSKU = sku.SKUPrimary;
            }
        }

        public void CalculateSKU()
        {
            var val = SKUID;

            if (val.HasValue)
            {
                // получим тип ТЕ по умолчанию
                var filter = string.Format("SKUID_r={0} AND SKU2TTEDefault=1", val);
                SKU2TTE[] sku2TTE;
                using (var sku2TTEMgr = IoC.Instance.Resolve<IBaseManager<SKU2TTE>>())
                    sku2TTE = sku2TTEMgr.GetFiltered(filter).ToArray();

                TETypeCode = sku2TTE.Length > 0 ? sku2TTE[0].TETypeCode : null;
                SKU sku;
                using (var skuMgr = IoC.Instance.Resolve<IBaseManager<SKU>>())
                    sku = skuMgr.Get(val);
                if (sku == null)
                    throw new DeveloperException("SKU code {0} not found", val);
                IsBaseSKU = sku.SKUPrimary;

                if (POSSKUID == SKUID)
                {
                    ProductCount = POSPRODUCTCOUNT;
                    IWBPosCount = POSIWBPOSCOUNT;
                }
                else
                {
                    ProductCount = sku.SKUCount;
                }

                // получим артикул
                Art art;
                using (var artMgr = IoC.Instance.Resolve<IBaseManager<Art>>())
                    art = artMgr.Get(sku.ArtCode);

                ArtCode = sku.ArtCode;
                ArtDesc = (art != null) ? art.ArtDesc : null;
                ArtInputDateMethod = (art != null) ? art.ArtInputDateMethod : string.Empty;

                Measure mea;
                using (var measureManager = IoC.Instance.Resolve<IBaseManager<Measure>>())
                    mea = measureManager.GetFiltered(string.Format("MEASURECODE = '{0}'", sku.MeasureCode), GetModeEnum.Partial).FirstOrDefault();
                if (mea != null)
                    MeasureCode = mea.MeasureShortName;
                //INFO: убрали вычисление фабрики (изменилась логика)
                //FactoryID_R = (art != null) ? art.FactoryID_R : null;
            }
        }

        public void CalculateTEType()
        {
            if (DisableCalculate)
                return;
            var val1 = SKUID;
            var val2 = TETypeCode;
            if (val2 != null)
            {
                if (val1 != null && val2 != null)
                {
                    // получим высоту Типа ТЕ
                    var filter = string.Format("SKUID_r={0} AND TETypeCode_r='{1}'", val1, val2);
                    SKU2TTE[] sku2TTE;
                    using (var sku2TTEMgr = IoC.Instance.Resolve<IBaseManager<SKU2TTE>>())
                        sku2TTE = sku2TTEMgr.GetFiltered(filter).ToArray();

                    if (sku2TTE.Length > 0)
                    {
                        SKU2TTEHeight = sku2TTE[0].SKU2TTEHeight;
                        SKU2TTEQuantityDefault = sku2TTE[0].SKU2TTEQuantity;
                        SKU2TTEQuantityMax = sku2TTE[0].SKU2TTEQuantityMax;
                        SKU2TTEQuantity = SKU2TTEQuantityDefault;
                    }
                    else
                    {
                        SKU2TTEHeight = 0;
                        SKU2TTEQuantityDefault = 0;
                        SKU2TTEQuantityMax = 0;
                        SKU2TTEQuantity = 0;
                    }
                }
                else
                {
                    SKU2TTEHeight = 0;
                    SKU2TTEQuantityDefault = 0;
                    SKU2TTEQuantityMax = 0;
                    SKU2TTEQuantity = 0;
                }
            }
        }

        public void CalculateSKU2TTEQuantity()
        {
            // если поменяли саму затарку, а не автоматом по типу ТЕ
            if (SKU2TTEQuantity != SKU2TTEQuantityDefault)
            {
                // если изменили в большую сторону, то отменим изменение
                var maxValue = OverrideSKU2TTEQuantityMax.HasValue
                    ? OverrideSKU2TTEQuantityMax.Value
                    : SKU2TTEQuantityMax;
                if (SKU2TTEQuantity > maxValue)
                    SKU2TTEQuantity = maxValue;
                else if (SKU2TTEQuantity < 1)
                    SKU2TTEQuantity = 1;
            }
        }

        protected override IValidator CreateValidator()
        {
            var factory = IoC.Instance.Resolve<IValidatorFactory>();
            if (!factory.IsNeedValidate(this))
                return null;

            return new IWBPosInputValidator(this);
        }

        public void RiseErrorsChanged()
        {
            base.OnPropertyChanged("Error");
            base.OnPropertyChanged(QLFDetailLPropertyName);
            base.OnPropertyChanged(IWBPosExpiryDatePropertyName);
        }

        protected override CustomPropertyCollection CreateCustomProperties()
        {
            var properties = base.CreateCustomProperties();
            // INFO: т.к. у нас пока нет возможности выставлять Nullable полям со значением по умолчанию в SysObject
            // меняем тип DateTime на DateTime?
            var p1 = properties[IWBPosExpiryDatePropertyName];
            if (p1 != null)
                p1.ChangePropertyType(typeof(DateTime?));
            var p2 = properties[IWBPosProductDatePropertyName];
            if (p2 != null)
                p2.ChangePropertyType(typeof(DateTime?));
            return properties;
        }

        #endregion . Methods .
    }

    public class IWBPosInputValidator : BlankValidator
    {
        public IWBPosInputValidator(IValidatable parent) : base(parent)
        {
        }

        public new IWBPosInput ValidatableObject
        {
            get { return (IWBPosInput)base.ValidatableObject; }
        }

        protected override void ValidateProperty_Internal(System.ComponentModel.PropertyDescriptor propertyInfo)
        {
            if (propertyInfo.Name.Equals(IWBPosInput.IWBPosExpiryDatePropertyName))
            {
                if (ValidatableObject.IWBPosExpiryDate.HasValue &&
                    (ValidatableObject.IWBPosExpiryDate.Value > new DateTime(2100, 01, 01) ||
                     ValidatableObject.IWBPosExpiryDate.Value <= new DateTime(1999, 12, 31)))
                {
                    Errors.Add(IWBPosInput.IWBPosExpiryDatePropertyName, new ValidateError("Срок годности некорректен, введите правильный срок годности.", ValidateErrorLevel.Critical));
                    return;
                }
            }

            if (propertyInfo.Name.Equals(IWBPosInput.IWBPosProductDatePropertyName))
            {
                if (ValidatableObject.IWBPosProductDate.HasValue &&
                    (ValidatableObject.IWBPosProductDate.Value > DateTime.Now ||
                     ValidatableObject.IWBPosProductDate.Value < new DateTime(1900, 01, 01)))
                {
                    Errors.Add(IWBPosInput.IWBPosProductDatePropertyName, new ValidateError("Дата производства некорректна, введите правильную дату производства.", ValidateErrorLevel.Critical));
                    return;
                }
            }

            base.ValidateProperty_Internal(propertyInfo);
        }

        protected override void ValidateChild_Internal(ValidateEventsArgs args)
        {
            base.ValidateChild_Internal(args);

            if (((IValidator)args.Sender).IsYourVlidatableObject(ValidatableObject.QLFDetailL) &&
                "count".EqIgnoreCase(args.PropertyName))
            {
                if (ValidatableObject.QLFCODE_R != null &&
                    ValidatableObject.QLFCODE_R.EqIgnoreCase(IWBPosInput.QlfTypeDefect))
                {
                    //if (ValidatableObject.QLFDetailL != null && ValidatableObject.QLFDetailL.Count > 1)
                    //{
                    //    Errors.Add(IWBPosInput.QLFDetailLPropertyName,
                    //        new ValidateError("testDesc", ValidateErrorLevel.Critical));
                    //}
                    //else
                    //{
                    //    Errors.Remove(IWBPosInput.QLFDetailLPropertyName, "testDesc");
                    //}

                    var qlfCode = ValidatableObject.QLFCODE_R;
                    var alfDetailLst = ValidatableObject.QLFDetailL;
                    var mgrQlf = IoC.Instance.Resolve<IBaseManager<Qlf>>();
                    var filter = string.Format("QLFCODE='{0}'", qlfCode);
                    var qlf = mgrQlf.GetFiltered(filter);

                    if (qlf != null)
                    {
                        if (qlf.First().QLFType == "QLFTYPEDEFECT" && alfDetailLst != null && alfDetailLst.Count > 1)
                        {
                            Errors.Add(IWBPosInput.QLFDetailLPropertyName,
                            new ValidateError("При выбранной квалификации может быть только одна детализация", ValidateErrorLevel.Critical));
                        }
                        else
                        {
                            Errors.Remove(IWBPosInput.QLFDetailLPropertyName, "При выбранной квалификации может быть только одна детализация");
                        }
                    }

                    try
                    {
                        SuspendValidating();
                        ValidatableObject.RiseErrorsChanged();
                    }
                    finally
                    {
                        ResumeValidating();
                    }
                }
            }
        }

        protected override void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Перекрываем вызов базового класса, чтобы глушить перепроверку всего
        }
    }
}
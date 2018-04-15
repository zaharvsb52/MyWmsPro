//
//TODO: AbstractValidator при каждой валидации пересчитывает список подчиненных объектов 
//TODO: ... (свойств с типом IValidatable, и вложенных колекций). Это достаточно затратная операция, т.к. выполняется 
//TODO: ... через reflection. Нужно переделать на хранение такой коллекции и обновлении при изменении объектов
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TB.ComponentModel;
using wmsMLC.General.BL.Validation.Attributes;

namespace wmsMLC.General.BL.Validation
{
    public static class PropertyDescriptorEx
    {
        public static bool IsValidatable(this PropertyDescriptor obj)
        {
            return typeof(IValidatable).IsAssignableFrom(obj.PropertyType);
        }
    }

    public abstract class AbstractValidator : IValidator
    {
        #region .  Constants  .

        public const string ValidatableObjectPropertyName = "ValidatableObject";
        public const string InMainValidatePropertyName = "InMainValidate";
        public const string InSuspendValidatingPropertyName = "InSuspendValidating";

        public const ValidateErrorLevel MaxValidateErrorLevel = ValidateErrorLevel.Critical;

        #endregion

        #region .  Contructor  .

        protected AbstractValidator(IValidatable parent)
        {
            ValidatableObject = parent;

            Subscribe();

            //CacheMethadata(ValidatableObject);
        }

        #endregion

        #region .  Variables  .

        private bool _inMainValidate;

        private ValidateErrors _errors;

        private IValidatable _validatableObject;

        private bool _inSuspendValidating;

        #endregion

        #region .  Properties  .

        /// <summary>
        /// Объект, который необходимо валидировать
        /// </summary>
        public IValidatable ValidatableObject
        {
            get { return _validatableObject; }
            private set
            {
                _validatableObject = value;
                OnPropertyChanged(ValidatableObjectPropertyName);
            }
        }

        /// <summary>
        /// Ошибки валидируемого объекта
        /// </summary>
        public ValidateErrors Errors
        {
            get { return _errors ?? (_errors = new ValidateErrors(ValidatableObject.GetType().FullName)); }
        }

        /// <summary>
        /// Признак того, что запущена полная явная валидация через метод Validate()
        /// </summary>
        public bool InMainValidate
        {
            get { return _inMainValidate; }
            private set
            {
                _inMainValidate = value;
                OnInMainValidateChanged();
                OnPropertyChanged(InMainValidatePropertyName);
            }
        }

        protected virtual void OnInMainValidateChanged() { }

        /// <summary>
        /// Признак приостановки валидации
        /// </summary>
        private bool InSuspendValidating
        {
            get { return _inSuspendValidating; }
            set
            {
                _inSuspendValidating = value;
                OnPropertyChanged(InSuspendValidatingPropertyName);
            }
        }

        /// <summary>
        /// Список подчиненных валидируемых объектов
        /// <remarks>
        /// Лучше не хранить, а каждый раз перерасчитывать, т.к. могут быть проблеммы при удалении объектов (тогда возвращать IEnumerable)
        /// </remarks>
        /// </summary> 
        private IEnumerable<IValidatable> ChildValidatedObjects
        {
            get
            {
                var childValidatedObjects = new List<IValidatable>();

                var vlidatedObjs = ValidatableObject as IEnumerable;

                // если перед объект - коллекция, берем всех IValidatable детей
                // иначе берем значения из всех IValidatable свойств
                if (vlidatedObjs != null)
                    childValidatedObjects.AddRange(vlidatedObjs.OfType<IValidatable>());
                else
                {
                    var values = GetValidatableProperties()
                        .Select(i => i.GetValue(ValidatableObject))
                        .OfType<IValidatable>();
                    childValidatedObjects.AddRange(values);
                }
                return childValidatedObjects;
            }
        }

        #endregion

        #region .  Helpers  .

        private PropertyDescriptor GetProperty(string name)
        {
            var properties = TypeDescriptor.GetProperties(ValidatableObject.GetType());
            return properties[name];
        }

        private IEnumerable<PropertyDescriptor> GetValidatableProperties()
        {
            var properties = TypeDescriptor.GetProperties(ValidatableObject.GetType());
            return properties.Cast<PropertyDescriptor>().Where(i => i.IsValidatable()).ToArray();
        }

        private IEnumerable<PropertyDescriptor> GetNonValidatableProperties()
        {
            var properties = TypeDescriptor.GetProperties(ValidatableObject.GetType()).Cast<PropertyDescriptor>();
            return properties.ToArray();
        }

        #endregion

        #region .  Methods  .

        protected void Subscribe()
        {
            // подписываемся на измениение свойств для их валидации
            var notifyPropertyChanged = ValidatableObject as INotifyPropertyChanged;
            if (notifyPropertyChanged != null)
            {
                notifyPropertyChanged.PropertyChanged -= ValidatableObject_PropertyChanged;
                notifyPropertyChanged.PropertyChanged += ValidatableObject_PropertyChanged;
            }

            var notifyCollectionChanged = ValidatableObject as INotifyCollectionChanged;
            if (notifyCollectionChanged != null)
            {
                notifyCollectionChanged.CollectionChanged -= CollectionChanged;
                notifyCollectionChanged.CollectionChanged += CollectionChanged;
            }

            RefreshValidateMeSubscription();
        }

        protected void UnSubscribe()
        {
            // подписываемся на измениение свойств для их валидации
            var notifyPropertyChanged = ValidatableObject as INotifyPropertyChanged;
            if (notifyPropertyChanged != null)
            {
                notifyPropertyChanged.PropertyChanged -= ValidatableObject_PropertyChanged;
            }

            var notifyCollectionChanged = ValidatableObject as INotifyCollectionChanged;
            if (notifyCollectionChanged != null)
            {
                notifyCollectionChanged.CollectionChanged -= CollectionChanged;
            }

            ResetValidateMeSubscription();
        }

        public void SuspendValidating()
        {
            InSuspendValidating = true;

            UnSubscribe();

            foreach (var o in ChildValidatedObjects)
                o.SuspendValidating();
        }

        public void ResumeValidating()
        {
            foreach (var o in ChildValidatedObjects)
                o.ResumeValidating();

            InSuspendValidating = false;

            // в процессе остановки валидации могли измениться объекты в коллекциях или свойствах
            // переподписываемся на новые объекты. Операция быстрая.
            Subscribe();
        }

        public void RefreshValidateMeSubscription()
        {
            // подписываем внутренние объекты на валидацию
            foreach (IValidatable o in ChildValidatedObjects.Where(o => o.Validator != null))
            {
                o.Validator.ValidateMe -= ValidateChild;
                o.Validator.ValidateMe += ValidateChild;
                o.Validator.RefreshValidateMeSubscription();
            }
        }

        public void ResetValidateMeSubscription()
        {
            // отписываем внутренние объекты от валидации
            foreach (IValidatable o in ChildValidatedObjects.Where(o => o.Validator != null))
            {
                o.Validator.ValidateMe -= ValidateChild;
                o.Validator.ResetValidateMeSubscription();
            }
        }

        public bool IsYourVlidatableObject(object obj)
        {
            return Equals(obj, ValidatableObject);
        }

        public virtual void Validate()
        {
            if (InSuspendValidating)
                return;

            try
            {
                InMainValidate = true;

                Errors.Clear();

                foreach (var p in GetNonValidatableProperties())
                    ValidateProperty(p);

                // валидируем зависимые объекты, реализующие интерфейс IValidatable
                foreach (var childValidatedObject in ChildValidatedObjects)
                    childValidatedObject.Validate();

                OnValidate();
            }
            finally
            {
                InMainValidate = false;
            }
        }

        public void ValidateProperty(string propertyName)
        {
            if (InSuspendValidating)
                return;

            if (propertyName == null)
                throw new ArgumentNullException("propertyName");

            var property = GetProperty(propertyName);
            if (property == null)
                throw new DeveloperException("Can't find property with name " + propertyName);

            ValidateProperty(property);
        }

        protected void ValidateProperty(PropertyDescriptor propertyInfo)
        {
            if (InSuspendValidating)
                return;

            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");

            Errors.Remove(propertyInfo.Name);

            ValidateProperty_Internal(propertyInfo);

            //TODO: Придумать что делать с индексируемыми
            //if (!propertyInfo.GetIndexParameters().Any())
            {
                if (typeof(IValidatable).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    var validatable = propertyInfo.GetValue(ValidatableObject) as IValidatable;
                    if (validatable != null)
                        validatable.Validate();
                }
            }
            OnValidate(propertyInfo.Name);
        }

        // ReSharper disable InconsistentNaming
        protected abstract void ValidateProperty_Internal(PropertyDescriptor propertyInfo);
        // ReSharper restore InconsistentNaming

        public void ValidateChild(ValidateEventsArgs args)
        {
            if (InSuspendValidating)
                return;

            ValidateChild_Internal(args);

            OnValidate(args);
        }

        // ReSharper disable InconsistentNaming
        protected virtual void ValidateChild_Internal(ValidateEventsArgs args) { }
        // ReSharper restore InconsistentNaming

        protected void Revalidate(string propertyName)
        {
            ValidateProperty(propertyName);
        }

        #region ValidateErrorInfo manage

        public void AddSelfErrors(IEnumerable<ValidateErrorInfo> errors)
        {
            if (errors == null)
                return;

            if (!errors.Any())
                return;

            // выбираем свои
            //var identifiable = ValidatableObject as IIdentifiable;
            //if (identifiable != null)
            //{
            //    var selfErrors = errors.Where(e => e.RecordID == identifiable.ID);
            //    foreach (var selfError in selfErrors)
            //        Errors.Add(selfError.Attribute, selfError.Error);
            //}

            // бросаем детям
            foreach (var o in ChildValidatedObjects.Where(o => o.Validator != null))
                o.Validator.AddSelfErrors(errors);
        }

        public ValidateErrorInfo[] GetSelfAndChildsErrors()
        {
            var res = new List<ValidateErrorInfo>();

            // если у объекта не извесен ID, то ошибки мы с него собрать не сможем
            //var voIdent = (ValidatableObject as IIdentifiable);
            //if (voIdent != null)
            //{
            //    // пишем свои
            //    res.AddRange(from errorList in Errors
            //                 let attribute = errorList.Key
            //                 from error in errorList.Value
            //                 select new ValidateErrorInfo(voIdent.ID, attribute, error));
            //}

            // пишем детей
            foreach (var o in ChildValidatedObjects.Where(o => o.Validator != null))
                res.AddRange(o.Validator.GetSelfAndChildsErrors());

            return res.ToArray();
        }

        public void ClearSelfAndChildsErrors()
        {
            Errors.Clear();

            foreach (IValidatable o in ChildValidatedObjects.Where(o => o.Validator != null))
                o.Validator.ClearSelfAndChildsErrors();
        }

        #endregion

        public virtual bool HasChanges()
        {
            return false;
        }

        public virtual bool HasCriticalError()
        {
            // себя
            bool res = Errors.Any(e => e.Value.HasCriticalErrors);

            // детей
            return res || ChildValidatedObjects.Any(o => o.Validator != null && o.Validator.HasCriticalError());
        }

        public virtual string GetErrorDescription()
        {
            // собственные:
            var res = Errors.ToString();

            var childsErrorDescription = string.Empty;

            // зависимые:
            foreach (var childValidatedObject in ChildValidatedObjects)
            {
                if (childValidatedObject.Validator == null)
                    continue;

                var childErrorDescription = childValidatedObject.Validator.GetErrorDescription();
                if (!string.IsNullOrEmpty(childErrorDescription))
                    childsErrorDescription += string.Format("{0}:\n{1}", childValidatedObject, childErrorDescription);
            }

            return res + childsErrorDescription;
        }

        public virtual string GetErrorDescription(string propertyName)
        {
            var errors = GetErrors(propertyName);
            if (errors.Count == 0)
                return null;

            var res = new StringBuilder();
            foreach (var error in errors)
            {
                var desc = error.GetErrorsDescription();
                if (string.IsNullOrEmpty(desc))
                    continue;

                if (desc.EndsWith(Environment.NewLine))
                    desc = desc.Substring(0, desc.Length - Environment.NewLine.Length);

                res.Append(desc).Append(";");
            }
            return res.ToString();
        }

        public virtual ValidateErrorLevel GetMaxErrorLevel()
        {
            var res = Errors.GetMaxErrorLevel();
            if (res == MaxValidateErrorLevel)
                return res;

            var maxErrorLevel = res;
            foreach (IValidatable childValidatedObject in ChildValidatedObjects)
            {
                if (childValidatedObject.Validator == null)
                    continue;

                var childMaxErrorLevel = childValidatedObject.Validator.GetMaxErrorLevel();
                if (maxErrorLevel < childMaxErrorLevel)
                    maxErrorLevel = childMaxErrorLevel;

                if (maxErrorLevel == MaxValidateErrorLevel)
                    return maxErrorLevel;
            }

            return maxErrorLevel;
        }

        public virtual ValidateErrorLevel GetMaxErrorLevel(string propertyName)
        {
            var res = ValidateErrorLevel.None;
            foreach (var error in GetErrors(propertyName))
            {
                var maxErrorLevel = error.GetMaxErrorLevel();
                if (maxErrorLevel > res)
                    res = maxErrorLevel;

                // если уже достигли максимального, то выходим
                if (res == MaxValidateErrorLevel)
                    break;
            }
            return res;
        }

        public virtual List<ValidateErrorsList> GetErrors(string propertyName)
        {
            var allErrorsList = new List<ValidateErrorsList>();

            // проверяем по собственному имени
            var selfErrorsList = Errors[propertyName];
            if (selfErrorsList != null)
                allErrorsList.Add(selfErrorsList);

            // проверки могут быть реализованы через ValidationDependsOnAttribute
            var property = GetProperty(propertyName);
            if (property != null)
            {
                // условная валидация
                var dependsAttributes = property.Attributes;// GetCustomAttributes(typeof(ValidationDependsOnAttribute), true);
                foreach (var attr in dependsAttributes)
                {
                    var validationDependsOnAttribute = attr as ValidationDependsOnAttribute;
                    if (validationDependsOnAttribute != null)
                    {
                        var dependsErrorsList = Errors[validationDependsOnAttribute.MemberName];
                        if (dependsErrorsList != null)
                            allErrorsList.Add(dependsErrorsList);
                    }
                }

                // валидация вложенных сущностей
                var childPropertyValue = property.GetValue(ValidatableObject) as IValidatable;
                if (childPropertyValue == null)
                    return allErrorsList;

                var genericType = childPropertyValue as ITypedList;
                var childPropertyValueItems = childPropertyValue as IList;
                if (genericType == null || childPropertyValueItems == null)
                    return allErrorsList;

                var internalEntityProperties = genericType.GetItemProperties(null);
                foreach (PropertyDescriptor internalEntityProperty in internalEntityProperties)
                {
                    foreach (var childPropertyValueItem in childPropertyValueItems)
                    {
                        var validatable = childPropertyValueItem as IValidatable;
                        if (validatable == null)
                            continue;

                        var internalEntityPropertyErrors = validatable.Validator.GetAllErrors(internalEntityProperty.Name);
                        foreach (var validateError in internalEntityPropertyErrors.Where(validateError => validateError.Value.HasCriticalErrors))
                        {
                            // получаем описание
                            foreach (var err in validateError.Value)
                            {
                                // если атрибут = вложенная сущность, не дописываем имя атрибута
                                var internalEntityPropertyName = !(typeof(IList).IsAssignableFrom(internalEntityProperty.PropertyType))
                                    ? " : " + internalEntityProperty.DisplayName
                                    : " " ;

                                var validatableObj = validatable as IKeyHandler;
                                if (validatableObj != null)
                                    err.Description = property.DisplayName + " (" + validatableObj.GetKey() + ")" + internalEntityPropertyName + ": " + err.Description;
                            }
                            allErrorsList.Add(validateError.Value);
                        }
                    }
                }
            }

            return allErrorsList;
        }

        public virtual ValidateErrors GetAllErrors(string propertyName)
        {
            var errors = new ValidateErrors(ValidatableObject.GetType().FullName);
            var items = GetErrors(propertyName);
            if (items.Count == 0) 
                return errors;
            var errorsList = new ValidateErrorsList();
            foreach (var err in items)
                errorsList.AddRange(err.ToArray());
            errors.Set(propertyName, errorsList);
            return errors;
        }

        public virtual string GetCriticalErrorDescription()
        {
            return Errors.GetCriticalErrorDescription();
        }

        public virtual bool HasErrorsWithLevel(ValidateErrorLevel level, string propertyName = null)
        {
            return Errors.Any(e => e.Value.Any(ve => ve.Level == level) && (propertyName == null || e.Key == propertyName));
        }

        public virtual void Revert()
        {
            ClearSelfAndChildsErrors();
        }

        #region .  Change event handlers  .

        // ReSharper disable InconsistentNaming
        protected virtual void ValidatableObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        // ReSharper restore InconsistentNaming
        {
            if (string.IsNullOrEmpty(e.PropertyName))
                RefreshValidateMeSubscription();
            else
            {
                var property = GetProperty(e.PropertyName);
                if (property != null)
                {
                    var newValue = property.GetValue(ValidatableObject) as IValidatable;
                    if (newValue != null && newValue.Validator != null)
                    {
                        newValue.Validator.ValidateMe -= ValidateChild;
                        newValue.Validator.ValidateMe += ValidateChild;
                        newValue.Validator.RefreshValidateMeSubscription();
                    }
                    ValidateProperty(e.PropertyName);
                }
            }
        }

        protected virtual void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //NOTE: иногда это избыточно. Перекрывайте этот метод в потомках и "глушите" проверку
            Validate();
        }

        #endregion

        #region .  ValidateMe event routings  .

        private void OnValidate()
        {
            if (null != ValidateMe)
                ValidateMe(new ValidateEventsArgs(this));
        }

        private void OnValidate(string propertyName)
        {
            if (null != ValidateMe)
                ValidateMe(new ValidateEventsArgs(this, null, propertyName));
        }

        private void OnValidate(ValidateEventsArgs args)
        {
            if (null != ValidateMe)
                ValidateMe(new ValidateEventsArgs(this, args));
        }

        /// <summary>
        /// Событие, вызываемое по окончании всех проверок валидатора для возможности провести валидацию
        /// родительским объектом
        /// </summary>
        public event ValidateEventHandler ValidateMe;

        #endregion

        public virtual string GetChangesDescription()
        {
            return string.Empty;
        }

        public void OnPropertyChanged(string propertyName)
        {
            var e = PropertyChanged;
            if (e != null)
                e(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
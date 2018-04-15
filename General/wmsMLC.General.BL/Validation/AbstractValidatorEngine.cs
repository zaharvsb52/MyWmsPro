using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using wmsMLC.General.BL.Validation.Attributes;

namespace wmsMLC.General.BL.Validation
{
    /// <summary>
    /// Класс автоматизирующий запуск валидации
    /// </summary>
    public abstract class AbstractValidatorEngine : AbstractValidator
    {
        // Список свойств, для которые запущена ревалидация. Храним, чтобы избежать рекурсий
        private readonly List<String> _inRevalidatePorpertiesNames = new List<string>();

        protected AbstractValidatorEngine(IValidatable parent) : base(parent) { }

        protected override void ValidateProperty_Internal(PropertyDescriptor propertyInfo)
        {
            // проверяем по аттрибутам
            ChecksByAttribute(propertyInfo);

            var list = Errors[propertyInfo.Name];
            if (list != null && list.Count == 0)
                Errors.Remove(propertyInfo.Name);
        }

        private void ChecksByAttribute(PropertyDescriptor propertyInfo)
        {
            var validateAttributes = propertyInfo.Attributes.OfType<BaseValidateAttribute>().ToList();
            if (validateAttributes.Count > 0)
                foreach (var validateAtt in validateAttributes.OrderBy(i => i.Ordinal))
                {
                    var errorsList = Errors.GetOrCreate(propertyInfo.Name);
                    var validateMethod = validateAtt as IValidateMethodHandler;
                    if (validateMethod != null)
                        validateMethod.Validate(ValidatableObject, propertyInfo, errorsList);
                    // если уже взвели критическую ошибку - дальше проверять не нужно
                    if (errorsList.HasCriticalErrors)
                        break;
                }

            //HACK: если вызваны из Validate(), то зависимые проверки делать не нужно, 
            //      они будут сделаны в самом Validate() (т.к. он проверяет все свойства)
            if (!InMainValidate)
            {
                // потом перевалидируем что нужно
                var revalidateAttributes = propertyInfo.Attributes.OfType<RevalidateAttribute>().ToList();
                if (revalidateAttributes.Count > 0)
                    foreach (var revalidateAttribute in revalidateAttributes)
                    {
                        bool startRevalidating = false;
                        try
                        {
                            startRevalidating = !_inRevalidatePorpertiesNames.Contains(revalidateAttribute.PropertyName);
                            if (startRevalidating)
                            {
                                _inRevalidatePorpertiesNames.Add(revalidateAttribute.PropertyName);
                                Revalidate(revalidateAttribute.PropertyName);
                            }
                        }
                        finally
                        {
                            if (startRevalidating)
                                _inRevalidatePorpertiesNames.Remove(revalidateAttribute.PropertyName);
                        }
                    }
            }
        }
    }
}

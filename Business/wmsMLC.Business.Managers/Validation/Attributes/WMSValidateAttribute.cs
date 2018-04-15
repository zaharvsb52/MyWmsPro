using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation;
using wmsMLC.General.BL.Validation.Attributes;

namespace wmsMLC.Business.Managers.Validation.Attributes
{
    public class WMSValidateAttribute : BaseValidateAttribute, IValidateMethodHandler
    {
        #region .  Fields  .
        private static readonly ConcurrentDictionary<string, Func<ValidateStrategyContext, bool>> _strategies
            = new ConcurrentDictionary<string, Func<ValidateStrategyContext, bool>>();
        private readonly Lazy<ObjectValid> _valid;
        #endregion

        public WMSValidateAttribute(decimal objectValidValueId, int ordinal = 0) : base(ordinal)
        {
            _valid = new Lazy<ObjectValid>(() =>
                {
                    var mgr = IoC.Instance.Resolve<IBaseManager<ObjectValid>>();
                    var objectValid = mgr.Get(objectValidValueId);
                    if (objectValid == null)
                        throw new DeveloperException("Невозможно получить параметры валидации по коду '{0}'",
                                                     objectValidValueId.ToString());
                    return objectValid;
                });
        }

        public void Validate(object obj, PropertyDescriptor propInfo, ValidateErrorsList errorsList)
        {
            // получаем стратегию
            var strategy = _strategies[_valid.Value.ObjectValidName];
            if (strategy == null)
                throw new DeveloperException("Strategy '{0}' is not registered for validate", _valid.Value.ObjectValidName ?? "NULL");

            // запускаем стратегию
            var context = new ValidateStrategyContext(obj, propInfo, _valid.Value.ObjectValidParameters, _valid.Value.ObjectValidValue)
                {
                    ErrorMessage = _valid.Value.ObjectValidMessage
                };

            if (strategy(context))
                errorsList.Add(new ValidateError(context.ErrorMessage, _valid.Value.ObjectValidLevel));
        }

        public static void RegisterStrategy(string name, Func<ValidateStrategyContext, bool> action)
        {
            _strategies.AddOrUpdate(name, action, (s, func) => action);
        }

        public static bool TestStrategy(string name, ValidateStrategyContext context)
        {
            return _strategies[name](context);
        }

        public string GetName()
        {
            return _valid.Value.ObjectValidName;
        }
    }
}
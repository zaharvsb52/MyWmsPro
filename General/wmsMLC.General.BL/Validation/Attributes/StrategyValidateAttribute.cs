using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace wmsMLC.General.BL.Validation.Attributes
{
    public class StrategyValidateAttribute : BaseValidateAttribute, IValidateMethodHandler
    {
        public StrategyValidateAttribute(string strategyName, int ordinal = 0) : base(ordinal)
        {
            StrategyName = strategyName;
        }

        public string StrategyName { get; private set; }

        public void Validate(object obj, PropertyDescriptor propInfo, ValidateErrorsList errorsList)
        {
            var strategy = _strategies[StrategyName];
            if (strategy == null)
                throw new DeveloperException(string.Format("Strategy '{0}' is not registered for validate",
                    string.IsNullOrEmpty(StrategyName) ? "NULL" : StrategyName));

            strategy(obj, propInfo, errorsList);
        }

        public static void RegisterStrategy(string name, Action<object, PropertyDescriptor, ValidateErrorsList> action)
        {
            _strategies.Add(name, action);
        }

        private static readonly Dictionary<string, Action<object, PropertyDescriptor, ValidateErrorsList>> _strategies
            = new Dictionary<string, Action<object, PropertyDescriptor, ValidateErrorsList>>();
    }
}
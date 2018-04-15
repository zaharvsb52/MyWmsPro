using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace wmsMLC.General.BL.Validation.Attributes
{
	/// <summary>
	/// Класс позволяет запустить произвольный метод валидатора объекта.
	/// <remarks>
	/// Eсли не задаются специализированные BindingFlags, следует обращать внимание на видимость метода из класса валидатора.
	/// Например, если метод объявлен в классе предка как private, то в потомках он не будет доступен для запуска валидатора. 
	/// В данном случае метод следует указать в предке как protected
	/// </remarks>
	/// </summary>
	public class ValidateMethodAttribute : BaseSimpleValidateAttribute
	{
		#region .  Constants  .

		private const BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
		private const ValidateErrorLevel DefaultValidatetionLevel = ValidateErrorLevel.None;
		private const string DefaultDescription = "test";

		#endregion

		#region .  ctr  .

		public ValidateMethodAttribute(string methodName, 
									   int ordinal = DefaultOrdinal,
									   BindingFlags flags = DefaultBindingFlags,
									   string recomendetDescription = DefaultDescription,
									   ValidateErrorLevel recomendetLevel = DefaultValidatetionLevel)
			: base(ordinal, recomendetDescription, recomendetLevel)
		{
			MethodName = methodName;
			MethodFlags = flags;
		}

		#endregion

		#region .  Properties  .

		/// <summary>
		/// Имя метода в валидаторе объекта
		/// </summary>
		public string MethodName { get; private set; }

		/// <summary>
		/// Флаги доступа к методу
		/// <remarks>По умолчанию: BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public</remarks>
		/// </summary>
		public BindingFlags MethodFlags { get; private set; }

		#endregion

        public override void Validate(object obj, PropertyDescriptor propInfo, ValidateErrorsList errorsList)
		{
			if (string.IsNullOrEmpty(MethodName))
				throw new DeveloperException("Can't use ValidateMethodAttribute without method name");

			var vaidatable = obj as IValidatable;
			if (vaidatable == null || vaidatable.Validator == null)
				throw new DeveloperException(string.Format("Object '{0}' is not validatable or Validator not exist", obj));

			var method = GetMethod(vaidatable.Validator.GetType());
			if (method == null)
				throw new DeveloperException(string.Format("Method '{0}' not found", MethodName));

			var methodParams = method.GetParameters();
			var recomendationsExist = Description != DefaultDescription || Level != DefaultValidatetionLevel;
			if (recomendationsExist || methodParams.Length == 3)
				method.Invoke(vaidatable.Validator, new object[] { errorsList, Description, Level });
			else if (methodParams.Length == 1)
				method.Invoke(vaidatable.Validator, new object[] { errorsList });
			else
				throw new DeveloperException("Params count error in method " + MethodName);
		}

		#region Get method functionality

		private static readonly Dictionary<string, MethodInfo> ValidatableMethodsCache = new Dictionary<string, MethodInfo>();

		private MethodInfo GetMethod(Type type)
		{
			var name = string.Format("{0}.{1}", type.FullName, MethodName);
			if (ValidatableMethodsCache.ContainsKey(name))
				return ValidatableMethodsCache[name];

			lock (((ICollection)ValidatableMethodsCache).SyncRoot)
			{
				if (ValidatableMethodsCache.ContainsKey(name))
					return ValidatableMethodsCache[name];

				var methodInfo = type.GetMethod(MethodName, MethodFlags);
				ValidatableMethodsCache.Add(name, methodInfo);
				return methodInfo;
			}
		}

		#endregion
	}
}
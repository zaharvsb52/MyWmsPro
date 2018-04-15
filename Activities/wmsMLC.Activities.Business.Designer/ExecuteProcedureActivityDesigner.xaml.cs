using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Microsoft.VisualBasic.Activities;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.General;
using wmsMLC.General.PL.Model;

namespace wmsMLC.Activities.Business.Designer
{
    public partial class ExecuteProcedureActivityDesigner
    {
        /// <summary>
        /// Регистрация атрибутов компонента
        /// </summary>
        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            builder.AddCustomAttributes(typeof(ExecuteProcedureActivity), new DesignerAttribute(typeof(ExecuteProcedureActivityDesigner)));
            //builder.AddCustomAttributes(typeof(ExecuteProcedureActivity), new ToolboxBitmapAttribute(typeof(ExecuteProcedureActivity), "DCLDefault16.png"));
            //builder.AddCustomAttributes(typeof(ExecuteProcedureActivity), new CategoryAttribute(@"Процессы"));
            builder.AddCustomAttributes(typeof(ExecuteProcedureActivity), new DisplayNameAttribute(@"Запуск API"));
            builder.AddCustomAttributes(typeof(ExecuteProcedureActivity), new DescriptionAttribute(@"Запуск процесса (через API функцию)"));
        }

        #region .  Consts & Fields  .
        public const string ResultParamName = "result";
        private ParameterInfo[] _methodParameters;
        private MethodInfo _methodInfo;
        private MethodInfo[] _methods;
        private bool _initializing;
        #endregion

        #region .  Constructor  .
        public ExecuteProcedureActivityDesigner()
        {
            _initializing = true;
            Items = new ObservableCollection<NameValueObject>();
            InitializeComponent();
        }
        #endregion

        #region .  Properties  .
        public ObservableCollection<NameValueObject> Items { get; private set; }
        #endregion

        #region .  Methods  .
        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);

            if (ModelItem == null)
                return;

            _methods = GetMethods();
            FillItems();
            UpdateParameters();

            // обновляем выбранное значение из Combobox
            var expression = BindingOperations.GetBindingExpression(cbProperties, Selector.SelectedValueProperty);
            if (expression != null)
                expression.UpdateTarget();
        }

        private MethodInfo[] GetMethods()
        {
            var methodActivity = ModelItem.GetCurrentValue() as IExecuteMethodActivity;
            if (methodActivity == null)
                throw new DeveloperException("Can't use ExecuteProcedureActivityDesigner with non IExecuteMethodActivity implementator");

            // получаем доступные методы
            return methodActivity.GetMethods();
        }
        private void FillItems()
        {
            Items.Clear();
            var tempInfos = new List<NameValueObject>();
            foreach (var method in _methods)
            {
                var displayName = method.Name;
                var displayNameAtt = method.GetOneCustomAttributes<DisplayNameAttribute>();
                if (displayNameAtt != null)
                    displayName = displayNameAtt.DisplayName;

                tempInfos.Add(new NameValueObject { Value = method.Name, DisplayName = displayName, Name = method.Name });
            }

            foreach (var ordmethod in tempInfos.OrderBy(i => i.DisplayName))
            {

                Items.Add(new NameValueObject { Value = ordmethod.Name, DisplayName = ordmethod.DisplayName, Name = ordmethod.Name });
            }
        }
        private void UpdateParameters()
        {
            // получаем имя метода
            var propertyValue = ModelItem.Properties["Value"];
            if (propertyValue == null || propertyValue.Value == null)
                return;

            var argument = (InArgument<string>)propertyValue.Value.GetCurrentValue();
            if (argument == null)
                return;

            var methodName = argument.Expression.ToString();
            var value = argument.Expression as VisualBasicValue<string>;
            if (value != null)
                methodName = value.ExpressionText.Replace("\"", string.Empty);
            if (string.IsNullOrEmpty(methodName))
                return;

            _methodInfo = _methods.FirstOrDefault(i => i.Name == methodName);
            if (_methodInfo == null)
                throw new DeveloperException("Can't find method by name " + methodName);
            var parameters = new List<ParameterInfo>();
            parameters.AddRange(_methodInfo.GetParameters());
            if (_methodInfo.ReturnType != typeof(void))
                parameters.Add(_methodInfo.ReturnParameter);
            _methodParameters = parameters.ToArray();
        }
        private ModelItemDictionary GetParametersProperty()
        {
            var paramProperty = ModelItem.Properties["Parameters"];
            if (paramProperty == null || paramProperty.Dictionary == null)
                throw new DeveloperException("Не задано обязательное св-во модели Parameters");
            return paramProperty.Dictionary;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var options = new DynamicArgumentDesignerOptions
            {
                Title = string.Format("Параметры метода {0}", _methodInfo.Name)
            };

            var modelParameters = GetParametersProperty();
            foreach (var p in _methodParameters)
            {
                if (modelParameters.ContainsKey(p.Name ?? ResultParamName))
                    continue;

                var direction = (p.IsOut || p.IsRetval || string.IsNullOrEmpty(p.Name)) ? ArgumentDirection.Out : ArgumentDirection.In;
                modelParameters.Add(p.Name ?? ResultParamName, ActivityHelpers.CreateDefaultValue(p.ParameterType, direction));
            }

            using (ModelEditingScope change = modelParameters.BeginEdit("ObjectEditing"))
            {
                if (DynamicArgumentDialog.ShowDialog(ModelItem, modelParameters, Context, ModelItem.View, options))
                    change.Complete();
                else
                    change.Revert();
            }
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!_initializing)
            {
                var parameters = GetParametersProperty();
                if (parameters != null)
                    parameters.Clear();
            }
            _initializing = false;
            UpdateParameters();
        }
        #endregion
    }
}

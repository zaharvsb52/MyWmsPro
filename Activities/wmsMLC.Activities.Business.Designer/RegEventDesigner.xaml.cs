using System;
using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Activities.Business.Designer
{
    public partial class RegEventDesigner
    {
        #region .  Fields  .
        private Lazy<IBaseManager<EventKind>> _eventKindManager = new Lazy<IBaseManager<EventKind>>(() => IoC.Instance.Resolve<IBaseManager<EventKind>>());
        private Lazy<IBaseManager<BillOperation>> _operationManager = new Lazy<IBaseManager<BillOperation>>(() => IoC.Instance.Resolve<IBaseManager<BillOperation>>());
        private List<PropertyDescriptor> _eventDetailProperties; 
        #endregion

        /// <summary>
        /// Регистрация атрибутов компонента
        /// </summary>
        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            builder.AddCustomAttributes(typeof(RegEvent), new DesignerAttribute(typeof(RegEventDesigner)));
            //builder.AddCustomAttributes(typeof(ExecuteProcedureActivity), new ToolboxBitmapAttribute(typeof(ExecuteProcedureActivity), "DCLDefault16.png"));
            //builder.AddCustomAttributes(typeof(ExecuteProcedureActivity), new CategoryAttribute(@"Процессы"));
        }

        public RegEventDesigner()
        {
            InitializeComponent();
        }

        #region .  Methods  .
        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            cbEventKind.ItemsSource = _eventKindManager.Value.GetAll(GetModeEnum.Partial);
            cbEventKind.SelectedValuePath = EventKind.EventKindCodePropertyName;
            cbEventKind.DisplayMemberPath = EventKind.EventKindNamePropertyName;

            cbOperation.ItemsSource = _operationManager.Value.GetAll(GetModeEnum.Partial);
            cbOperation.SelectedValuePath = BillOperation.OperationCodePropertyName;
            cbOperation.DisplayMemberPath = BillOperation.OperationNamePropertyName;

            UpdateParameters();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var options = new DynamicArgumentDesignerOptions
            {
                Title = string.Format("Параметры")
            };

            // получаем параметры из активити
            var parameters = GetParametersProperty();

            // дополняем параметрами из класса
            foreach (var propertyDescriptor in _eventDetailProperties)
            {
                if (parameters.ContainsKey(propertyDescriptor.Name))
                    continue;
                parameters.Add(propertyDescriptor.Name, ActivityHelpers.CreateDefaultValue(propertyDescriptor.PropertyType, ArgumentDirection.In));
            }

            using (var change = parameters.BeginEdit("ObjectEditing"))
            {
                if (DynamicArgumentDialog.ShowDialog(ModelItem, parameters, Context, ModelItem.View, options))
                    change.Complete();
                else
                    change.Revert();
            }
        }

        private void UpdateParameters()
        {
            if (_eventDetailProperties != null)
                _eventDetailProperties.Clear();

            var typeProperties = TypeDescriptor.GetProperties(typeof(EventDetail));
            _eventDetailProperties = new List<PropertyDescriptor>();
            foreach (PropertyDescriptor typeProperty in typeProperties)
            {
                // пропускаем служебные св-ва
                var att = typeProperty.Attributes[typeof(HardCodedPropertyAttribute)];
                if (att != null)
                    continue;
                _eventDetailProperties.Add(typeProperty);
            }
        }

        private ModelItemDictionary GetParametersProperty()
        {
            var paramProperty = ModelItem.Properties["Parameters"];
            if (paramProperty == null || paramProperty.Dictionary == null)
                throw new DeveloperException("Не задано обязательное св-во модели Parameters");
            return paramProperty.Dictionary;
        } 
        #endregion
    }
}

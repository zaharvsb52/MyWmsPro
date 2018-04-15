using System;
using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.Model;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using wmsMLC.Activities.General;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.General;

namespace wmsMLC.Activities.Business.Designer
{
    public partial class MultipleDynamicAssignActivityDesigner
    {
        /// <summary>
        /// Регистрация атрибутов компонента
        /// </summary>
        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            builder.AddCustomAttributes(typeof(MultipleDynamicAssignActivity<>), new DesignerAttribute(typeof(MultipleDynamicAssignActivityDesigner)));
            //builder.AddCustomAttributes(typeof(MultipleDynamicAssignActivity), new ToolboxBitmapAttribute(typeof(MultipleDynamicAssignActivity), "DCLDefault16.png"));
            //builder.AddCustomAttributes(typeof(MultipleDynamicAssignActivity), new CategoryAttribute(@"General"));
            builder.AddCustomAttributes(typeof(MultipleDynamicAssignActivity<>), new DisplayNameAttribute(@"Множественное присваивание"));
            builder.AddCustomAttributes(typeof(MultipleDynamicAssignActivity<>), new DescriptionAttribute(@"Позволяет заполнить поля сущности"));
        }

        private PropertyDescriptorCollection _properties;
        private Type _objType;

        public MultipleDynamicAssignActivityDesigner()
        {
            InitializeComponent();
        }

         protected override void OnModelItemChanged(object newItem)
         {
             base.OnModelItemChanged(newItem);
             
             if (ModelItem == null)
                 return;

             // получаем св-ва объекта
             var genericArgs = ModelItem.ItemType.GetGenericArguments();
             if (genericArgs.Length != 1)
                 throw new DeveloperException("Ожидался generic type с одним параметром.");
             _objType = genericArgs[0];
             _properties = TypeDescriptor.GetProperties(_objType);
         }

        private Argument CreateDefaultValue(Type type)
        {
             return ActivityHelpers.CreateDefaultValue(type, ArgumentDirection.In);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            const string propertyFormat = "[{0}]";
            var cnerror = string.Format("!{0}. ", wmsMLC.General.Resources.StringResources.Error);
            var options = new DynamicArgumentDesignerOptions
            {
                Title = string.Format("Свойства {0}", _objType.Name)
            };

            var modelItem = ModelItem.Properties["Properties"].Dictionary;
            foreach (PropertyDescriptor p in _properties)
            {
                var propertySubkey = string.Format(propertyFormat, p.Name);
                var displayNameSubkey = p.DisplayName ?? string.Empty;

                var keyVisual = string.Format("{0} {1}", displayNameSubkey, propertySubkey).GetTrim();

                //ToDo: Блок совместимости со старой версией
                //Ищем по старому и новому ключу
                if (!modelItem.ContainsKey(keyVisual) && !modelItem.ContainsKey(propertySubkey))
                {
                    modelItem.Add(propertySubkey, CreateDefaultValue(p.PropertyType));
                }

                //Преобразуем 
                if (modelItem.ContainsKey(propertySubkey))
                {
                    modelItem[keyVisual] = modelItem[propertySubkey];
                    modelItem.Remove(propertySubkey);
                }

                var baditems = modelItem.Select(g => new
                {
                    Skey = g.Key.GetCurrentValue().To(string.Empty),
                    Pair = g
                })
                    .Where(g => (g.Skey != keyVisual && g.Skey != propertySubkey) && g.Skey.ToUpper().Contains(propertySubkey.ToUpper())) //Этот кошмар для совместимости
                    .Select(g => g.Pair).ToArray();

                foreach (var baditem in baditems)
                {
                    object currentvalue;
                    if (baditem.Value == null || (currentvalue = baditem.Value.GetCurrentValue()) == null ||
                        (currentvalue as Argument != null && ((Argument) currentvalue).Expression == null))
                    {
                        modelItem.Remove(baditem.Key);
                        continue;
                    }

                    var badkey = baditem.Key.GetCurrentValue().To(string.Empty);
                    if (badkey.StartsWith(cnerror))
                        continue;

                    modelItem[string.Format("{0}{1}", cnerror, baditem.Key)] = baditem.Value;
                    modelItem.Remove(baditem.Key);
                }
            }
            
            //Проверка свойств
            foreach (var p in modelItem.ToArray())
            {
                var propertyname = GetProperty(p.Key.GetCurrentValue().To<string>());

                if (string.IsNullOrEmpty(propertyname))
                    continue;
                if (_properties.Find(propertyname, true) != null)
                    continue;
                modelItem[string.Format("{0}{1}", cnerror, p.Key)] = p.Value;
                modelItem.Remove(p.Key);
            }
            
            using (var change = modelItem.BeginEdit("ObjectEditing"))
            {
                if (DynamicArgumentDialog.ShowDialog(ModelItem, modelItem, Context, ModelItem.View, options))
                {
                    change.Complete();
                    //ToDo: Узнать как удалять из modelItem внутри ModelEditingScope
                    RemoveDescription(modelItem, propertyFormat);
                }
                else
                {
                    change.Revert();
                    RemoveDescription(modelItem, propertyFormat);
                }
            }
        }

        private void RemoveDescription(ModelItemDictionary modelItem, string propertyFormat)
        {
            using (var change2 = modelItem.BeginEdit("ObjectEditing2"))
            {
                foreach (var p in modelItem.ToArray())
                {
                    //Убираем null
                    object currentvalue;
                    if (p.Value == null || (currentvalue = p.Value.GetCurrentValue()) == null ||
                        (currentvalue as Argument != null && ((Argument)currentvalue).Expression == null))
                    {
                        modelItem.Remove(p.Key);
                        continue;
                    }

                    //Убираем описание
                    var key = p.Key.GetCurrentValue().To(string.Empty);
                    var propertySubkey = string.Format(propertyFormat, GetProperty(key));
                    modelItem[propertySubkey] = p.Value;
                    modelItem.Remove(p.Key);
                }
                change2.Complete();
            }
        }

        private string GetProperty(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            const string regex = @"\[\s*(.[^\]]*)\s*\]";
            const RegexOptions regexOptions = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
            var expressions = Regex.Matches(key, regex, regexOptions);
            if (expressions.Count > 0)
                return expressions[0].Groups[1].Value;
            return null;
        }
    }
}

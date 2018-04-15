using System;
using System.ComponentModel;
using System.Linq;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Managers.Validation.Attributes;
using wmsMLC.General.PL.WPF.Attributes;

namespace wmsMLC.General.PL.WPF
{
    public static class ProcessAttributeStrategiesHelper
    {
        private static ProcessAttributeStrategy CreateStrategyForVisibilityAttribute(string attrName, Func<string, BaseVisibleAttribute> attrCreator)
        {
            return new ProcessAttributeStrategy(attrName, (list, s) =>
            {
                if (!string.IsNullOrEmpty(s))
                    list.Add(attrCreator(s));
            });
        }

        private static ProcessAttributeStrategy CreateStrategyForFormatAttribute(string attrName, Func<string, BaseFormatAttribute> attrCreator)
        {
            return new ProcessAttributeStrategy(attrName, (list, s) =>
            {
                if (!string.IsNullOrEmpty(s))
                    list.Add(attrCreator(s));
            });
        }

        public static void Initialize()
        {
            // если приходит какой-либо неизвестный аттрибут - ругаемся
            EntityDescription.UnknownAttibuteThrowExceptionMode = true;

            // Наименование
            var attDisplayName = new ProcessAttributeStrategy("OBJECTEXTCAPTION", (list, s) => list.Add(new DisplayNameAttribute(s)));
            EntityDescription.RegisterProcessAttributeStrategy(attDisplayName);

            // Описание
            var attDesc = new ProcessAttributeStrategy("OBJECTEXTDESC", (list, s) =>
            {
                // если аттрибут описания уже есть, то у нас уже загружено расширенное описание
                var exitsDescAtt = list.FirstOrDefault(i => i.GetType() == typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (exitsDescAtt != null)
                {
                    list.Remove(exitsDescAtt);
                    var str = s + Environment.NewLine + exitsDescAtt.Description;
                    list.Add(new DescriptionAttribute(str));
                }
                else
                    list.Add(new DescriptionAttribute(s));
            });
            EntityDescription.RegisterProcessAttributeStrategy(attDesc);

            // Расширенное описание
            var attExtDesc = new ProcessAttributeStrategy("OBJECTEXTDESCEXT", (list, s) =>
            {
                // если аттрибут описания уже есть, то у нас уже загружено расширенное описание
                var exitsDescAtt = list.FirstOrDefault(i => i.GetType() == typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (exitsDescAtt != null)
                {
                    list.Remove(exitsDescAtt);
                    var str = exitsDescAtt.Description + Environment.NewLine + s;
                    list.Add(new DescriptionAttribute(str));
                }
                else
                    list.Add(new DescriptionAttribute(s));
            });
            EntityDescription.RegisterProcessAttributeStrategy(attExtDesc);

            #region View* атрибуты

            #region Видимость

            // Отображать ли в списках
            EntityDescription.RegisterProcessAttributeStrategy(CreateStrategyForVisibilityAttribute(AttributeCodes.View2Grid, s => new ListVisibleAttribute(s == "1")));

            // Отображать ли в форме редактирования объекта
            EntityDescription.RegisterProcessAttributeStrategy(CreateStrategyForVisibilityAttribute(AttributeCodes.View2Detail, s => new DetailVisibleAttribute(s == "1")));

            // Отображать ли в фильтре
            EntityDescription.RegisterProcessAttributeStrategy(CreateStrategyForVisibilityAttribute(AttributeCodes.View2Filter, s => new FilterVisibleAttribute(s == "1")));

            // Отображать ли в lookup
            EntityDescription.RegisterProcessAttributeStrategy(CreateStrategyForVisibilityAttribute(AttributeCodes.View2Lookup, s => new LookUpVisibleAttribute(s == "1")));

            // Отображать ли в subgrid
            EntityDescription.RegisterProcessAttributeStrategy(CreateStrategyForVisibilityAttribute(AttributeCodes.View2SubGrid, s => new SubListVisibleAttribute(s == "1")));

            // Отображать ли в subdetail
            EntityDescription.RegisterProcessAttributeStrategy(CreateStrategyForVisibilityAttribute(AttributeCodes.View2SubDetail, s => new SubDetailVisibleAttribute(s == "1")));

            // Отображать ли в списках через спец. поле
            EntityDescription.RegisterProcessAttributeStrategy(CreateStrategyForVisibilityAttribute(AttributeCodes.View2GridAsMemo, s => new ListMemoVisibleAttribute(s == "0")));

            // Отображать ли при детализации в спец. поле
            EntityDescription.RegisterProcessAttributeStrategy(CreateStrategyForVisibilityAttribute(AttributeCodes.View2DetailAsMemo, s => new DetailMemoVisibleAttribute(s == "0")));

            #endregion

            #region Форматы

            // Формат отображения в списках м-к-м
            EntityDescription.RegisterProcessAttributeStrategy(CreateStrategyForFormatAttribute(AttributeCodes.ViewFormatDefault, s => new DefaultDisplayFormatAttribute(s)));
 
            // Формат отображения в списках
            EntityDescription.RegisterProcessAttributeStrategy(CreateStrategyForFormatAttribute(AttributeCodes.View2GridFormat, s => new ListDisplayFormatAttribute(s)));

            // Формат отображения в форме редактирования
            EntityDescription.RegisterProcessAttributeStrategy(CreateStrategyForFormatAttribute(AttributeCodes.View2DetailFormat, s => new DetailDisplayFormatAttribute(s)));

            // Формат отображения в lookup
            EntityDescription.RegisterProcessAttributeStrategy(CreateStrategyForFormatAttribute(AttributeCodes.View2LookupFormat, s => new LookUpDisplayFormatAttribute(s)));

            // Формат отображения в subgrid
            EntityDescription.RegisterProcessAttributeStrategy(CreateStrategyForFormatAttribute(AttributeCodes.View2SubGridFormat, s => new SubListDisplayFormatAttribute(s)));

            // Формат отображения в subdetail
            EntityDescription.RegisterProcessAttributeStrategy(CreateStrategyForFormatAttribute(AttributeCodes.View2SubDetailFormat, s => new SubDetailDisplayFormatAttribute(s)));

            #endregion

            #endregion

            // Отображать ли в контекстном меню
            var attDisableQuickLink = new ProcessAttributeStrategy("DISABLEQUICKLINK", (list, s) =>
            {
                if (!string.IsNullOrEmpty(s))
                    list.Add(new DisableQuickLinkAttribute());
            });
            EntityDescription.RegisterProcessAttributeStrategy(attDisableQuickLink);

            // ReadOnly
            var attViewEnableEdit = new ProcessAttributeStrategy("VIEWENABLEEDIT", (list, s) =>
            {
                //if (s != "1")
                //    list.Add(ReadOnlyAttribute.Yes);
                if (!string.IsNullOrEmpty(s))
                    list.Add(new EnableEditAttribute(s == "1"));
            });
            EntityDescription.RegisterProcessAttributeStrategy(attViewEnableEdit);

            // 
            var attViewEnableCreate = new ProcessAttributeStrategy("VIEWENABLECREATE", (list, s) =>
            {
                //if (s != "1")
                //    list.Add(ReadOnlyAttribute.Yes);
                if (!string.IsNullOrEmpty(s))
                    list.Add(new EnableCreateAttribute(s == "1"));
            });
            EntityDescription.RegisterProcessAttributeStrategy(attViewEnableCreate);

            //var minRange = new ProcessAttributeStrategy("MINRANGE", (list, s) =>
            //{
            //    if (!string.IsNullOrEmpty(s))
            //        list.Add(new MinRangeAttribute(s));
            //});
            //EntityDescription.RegisterProcessAttributeStrategy(minRange);

            //var maxRange = new ProcessAttributeStrategy("MAXRANGE", (list, s) =>
            //{
            //    if (!string.IsNullOrEmpty(s))
            //        list.Add(new MinRangeAttribute(s));
            //});
            //EntityDescription.RegisterProcessAttributeStrategy(maxRange);


            // Определяет PanelCaption в ObjectListView<odel
            var attListViewCaption = new ProcessAttributeStrategy("OBJECTEXTLISTNAME", (list, s) =>
            {
                if (!string.IsNullOrEmpty(s))
                    list.Add(new ListViewCaptionAttribute(s));
            });
            EntityDescription.RegisterProcessAttributeStrategy(attListViewCaption);

            // Определяет валидаторы. Порядок определяем по правилу "первый пришел - первый выполнился"
            var attValid = new ProcessAttributeStrategy("OBJECTVALIDVALUE", (list, s) => list.Add(new WMSValidateAttribute(decimal.Parse(s), list.Count(i => i is WMSValidateAttribute))));
            EntityDescription.RegisterProcessAttributeStrategy(attValid);

            // для виртуальных полей реализуем правило - нельзя редактировать
            // сама виртуальность должна разбираться на бизнес слое
            var attVirtual = new ProcessAttributeStrategy("VIRTUALFIELDPARAMVALUE", (list, s) =>
            {
                if (!string.IsNullOrEmpty(s))
                {
                    // INFO: не стал удалять, пока что
                    //if (list.Any(i => i is EnableCreateAttribute))
                    //    throw new DeveloperException("Нельзя одновременно устанавливать и VIRTUALFIELDPARAMVALUE и VIEWENABLECREATE");
                    //if (list.Any(i => i is EnableEditAttribute))
                    //    throw new DeveloperException("Нельзя одновременно устанавливать и VIRTUALFIELDPARAMVALUE и VIEWENABLEEDIT");

                    // если аттрибут описания уже есть, то у нас уже загружено расширенное описание
                    var exitsDescAtt = list.FirstOrDefault(i => i.GetType() == typeof(DescriptionAttribute)) as DescriptionAttribute;
                    var text = string.Format("Формула: '{0}'", s);
                    if (exitsDescAtt != null)
                    {
                        list.Remove(exitsDescAtt);
                        var str = exitsDescAtt.Description + Environment.NewLine + text;
                        list.Add(new DescriptionAttribute(str));
                    }
                    else
                        list.Add(new DescriptionAttribute(text));

                    // INFO: не стал удалять, пока что
                    //list.Add(new EnableCreateAttribute(false));
                    //list.Add(new EnableEditAttribute(false));
                }
            });
            EntityDescription.RegisterProcessAttributeStrategy(attVirtual);

            // для Lookup-ых поле добавляем ссылку на виртуальное, если такое имеется
            var attLinkToVirtual = new ProcessAttributeStrategy("VIRTUALFIELDNAME", (list, s) =>
            {
                if (string.IsNullOrEmpty(s))
                    throw new DeveloperException("Для аттрибута VIRTUALFIELDNAME не задано значение");

                list.Add(new LinkToVirtualFieldAttribute(s));
            });
            EntityDescription.RegisterProcessAttributeStrategy(attLinkToVirtual);

            // для Lookup-ых поле добавляем ссылку на виртуальное, если такое имеется
            var attVirtualField_REALFIELDNAME = new ProcessAttributeStrategy("REALFIELDNAME", (list, s) =>
            {
                var attr = list.FirstOrDefault(i => i is VirtualFieldAttribute) as VirtualFieldAttribute;
                if (attr == null)
                {
                    attr = new VirtualFieldAttribute(s);
                    list.Add(attr);
                }
                else
                    attr.ParentFieldName = s;
            });
            EntityDescription.RegisterProcessAttributeStrategy(attVirtualField_REALFIELDNAME);

            var attVirtualField_ISVIRTUAL = new ProcessAttributeStrategy("ISVIRTUAL", (list, s) =>
            {
                var attr = list.FirstOrDefault(i => i is VirtualFieldAttribute) as VirtualFieldAttribute;
                if (attr == null)
                {
                    attr = new VirtualFieldAttribute(s);
                    list.Add(attr);
                }
            });
            EntityDescription.RegisterProcessAttributeStrategy(attVirtualField_ISVIRTUAL);
            // заглушки для атрибутов, которые пока не реализуются
            //            var todoAtts = new[] { "ViewRedifinition" };
            //            foreach (var todoAtt in todoAtts)
            //            {
            //                EntityDescription.RegisterProcessAttributeStrategy(new ProcessAttributeStrategy(todoAtt, (list, s) =>
            //                {
            //                    //доделать
            //                }));
            //            }
        }
    }
}

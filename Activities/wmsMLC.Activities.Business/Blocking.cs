using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using wmsMLC.Business.General;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.Activities.Business
{
    public class Blocking : NativeActivity
    {
        #region . Properties .

        public const string DescriptionPropertyName = "Description";
        public const string ItemsPropertyName = "Items";
        public const string ProductBlockPropertyName = "ProductBlock";

        #endregion

        #region . Arguments .

        [DisplayName(@"Список элементов")]
        [RequiredArgument, DefaultValue(null)]
        public InArgument<IEnumerable<Object>> Items { get; set; }

        [DisplayName(@"Описание блокировки")]
        [RequiredArgument, DefaultValue(null)]
        public string Description { get; set; }

        [Obsolete]
        [DisplayName(@"Тип блокировки")]
        [DefaultValue(null)]
        public ProductBlocking ProductBlock { get; set; }

        [DisplayName(@"Код блокировки")]
        [RequiredArgument, DefaultValue(null)]
        public string BlockingCode { get; set; }

        [DisplayName(@"Запрашивать данные блокировки")]
        [RequiredArgument, DefaultValue(false)]
        public bool UserRequestEnable { get; set; }

        #endregion

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();

            //var aItemsType = typeof(IEnumerable<WMSBusinessObject>);
            var aItemsType = typeof(List<Object>);
            if (Items != null)
                aItemsType = Items.ArgumentType;
            var aTo = new RuntimeArgument(ItemsPropertyName, aItemsType, ArgumentDirection.In, true);
            metadata.Bind(Items, aTo);
            collection.Add(aTo);

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
//            нужно сделать отдельную галочку на дозапрос данных
//            если дозапрос запрещен, а типа нет - ругаемся
            var items = context.GetValue(Items).ToArray();
            if (!items.Any())
                throw new DeveloperException("Нет объектов для блокировки");

            var mto = IoC.Instance.Resolve<IManagerForObject>();
            var mgrType = mto.GetManagerByTypeName(items[0].GetType().Name);
            var blockMgr = IoC.Instance.Resolve(mgrType, null) as IBlockingManager;

            if (blockMgr == null)
                throw new DeveloperException("Тип {0} не поддерживает блокировку", items[0].GetType().Name);

            if (!UserRequestEnable && string.IsNullOrEmpty(BlockingCode))
                throw new DeveloperException("Не задан код блокировки и запрещен дозапрос данных");

            // дозапрашиваем
            if (string.IsNullOrEmpty(BlockingCode))
            {
                // формируем поля дозапроса
                var values = new List<ValueDataField>
                    {
                        new ValueDataField
                            {
                                Name = ProductBlocking.BlockingCodePropertyName,
                                FieldName = ProductBlocking.BlockingCodePropertyName,
                                SourceName = ProductBlocking.BlockingCodePropertyName,
                                Caption = ProductBlocking.BlockingCodePropertyName,
                                FieldType = typeof (string),
                                LookupCode = blockMgr.GetNameLookupBlocking(),
                                Value = BlockingCode
                            },
                        new ValueDataField
                            {
                                Name = DescriptionPropertyName,
                                FieldName = DescriptionPropertyName,
                                SourceName = DescriptionPropertyName,
                                Caption = "Описание блокировки",
                                FieldType = typeof (string),
                            }
                    };

                var model = new ExpandoObjectViewModelBase
                    {
                        Fields = new ObservableCollection<ValueDataField>(values),
                        PanelCaption = "Блокировка"
                    };

                var viewService = IoC.Instance.Resolve<IViewService>();
                if (viewService.ShowDialogWindow(model, true) == true)
                {
                    Description = model[DescriptionPropertyName] as string;
                    BlockingCode = model[ProductBlocking.BlockingCodePropertyName] as string;
                }
                else
                    throw new Exception("Пользователь отказался от продолжения блокирования");
            }

            if (string.IsNullOrEmpty(BlockingCode))
                throw new DeveloperException("Не указан обязательный параметр 'Код блокировки'");

            var mgrBlock = IoC.Instance.Resolve<IBaseManager<ProductBlocking>>();
            var block = mgrBlock.Get(BlockingCode);
            if (block == null)
                throw new DeveloperException("Не найдена блокировка с кодом '{0}'", BlockingCode);

            var error = new List<string>();
            foreach (WMSBusinessObject item in items)
            {
                try
                {
                    blockMgr.Block(item, block, Description);
                }
                catch (Exception ex)
                {
                    error.Add(ex.Message);
                }
            }

            if (error.Count > 0)
                throw new Exception(string.Format("Не заблокированы следующие ТЕ:\n{0}", string.Join("\n", error)));
        }
    }
}

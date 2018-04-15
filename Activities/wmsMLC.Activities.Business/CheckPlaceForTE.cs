using System;
using System.Activities;
using System.ComponentModel;
using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Activities.Business
{
    public class CheckPlaceForTE : NativeActivity
    {
        #region . Arguments .

        /// <summary>
        /// ТЕ для места
        /// </summary>
        [DisplayName(@"ТЕ")]
        public InArgument<TE> TEObject { get; set; }

        /// <summary>
        /// Место которое надо проверить
        /// </summary>
        [DisplayName(@"Место для ТЕ")]
        public InArgument<Place> PlaceObject { get; set; }

        /// <summary>
        /// Проверочный статус места
        /// </summary>
        [DisplayName(@"Статус места")]
        public InArgument<string> PlaceStatus { get; set; }

        /// <summary>
        /// Проверочная вместимость места
        /// </summary>
        [DisplayName(@"Вместимость места >=")]
        public InArgument<int> PlaceCapacity { get; set; }

        /// <summary>
        /// Перевычитать место
        /// </summary>
        [DisplayName(@"Перечитать место")]
        public InArgument<bool> ReloadPlace { get; set; }

        /// <summary>
        /// Проверить, что тип ТЕ привязан к классу места
        /// </summary>
        [DisplayName(@"Проверить, что тип ТЕ привязан к классу места")]
        public InArgument<bool> CheckTEType2PlaceClass { get; set; }

        /// <summary>
        /// Проверить, что вес ТЕ соответстует разрешенному весу места
        /// </summary>
        [DisplayName(@"Проверить, что вес ТЕ соответстует разрешенному весу места")]
        public InArgument<bool> CheckTEWeightWithPlaceWeight { get; set; }

        /// <summary>
        /// Проверить, что совокупный вес тех ТЕ, которые уже стоят на местах одной группы мест с нашим местом назначения 
        /// (а так же перемещаются туда по другим ЗНТ), и вес перемещаемой ТЕ не превышают максимальный вес на группу мест
        /// </summary>
        [DisplayName(@"Проверить, совокупный вес всех ТЕ на группу места")]
        public InArgument<bool> CheckAllTEWeightWithPlaceGroupWeight { get; set; }

        /// <summary>
        /// Проверить, что ТЕ можно поместить на место по габаритам
        /// </summary>
        [DisplayName(@"Проверить, что ТЕ можно поместить на место по габаритам")]
        public InArgument<bool> CheckTESizeWithPlaceSize { get; set; }

        /// <summary>
        /// Сообщение об ошибках
        /// </summary>
        [DisplayName(@"Сообщение ошибки")]
        public OutArgument<string> ErrorMessage { get; set; }

        /// <summary>
        /// Место совместимо с указанной ТЕ
        /// </summary>
        [DisplayName(@"Место подходит")]
        public OutArgument<bool> PlaceIsValid { get; set; }

        #endregion

        public CheckPlaceForTE()
        {
            this.DisplayName = "Проверка места для ТЕ";
        }

        protected override void Execute(NativeActivityContext context)
        {
            var result = CheckPlace(context);
            PlaceIsValid.Set(context, result);
        }

        private bool CheckPlace(NativeActivityContext context)
        {
            // если нужно перевычитать место
            if (ReloadPlace.Get(context))
            {
                var manager = IoC.Instance.Resolve<IBaseManager<Place>>();

                var placeCode = PlaceObject.Get(context).GetKey();
                var tmp = manager.Get(placeCode);
                if (tmp == null)
                {
                    ErrorMessage.Set(context, string.Format("{0}Место с кодом {1} не существует", Environment.NewLine, placeCode));
                    return false;
                }
                PlaceObject.Set(context, tmp);
            }

            var te = TEObject.Get(context);
            var place = PlaceObject.Get(context);
            // проверим статус места
            var placeStatus = place.StatusCode_R;
            if (!placeStatus.Equals(PlaceStatus.Get(context)))
            {
                ErrorMessage.Set(context, "Не верный статус места: " + placeStatus);
                return false;
            }

            var placeCapacity = Convert.ToInt32(place.PlaceCapacity);
            if (placeCapacity < Convert.ToInt32(PlaceCapacity.Get(context)))
            {
                ErrorMessage.Set(context, "Остаточная вместимость места < " + placeCapacity);
                return false;
            }

            // если нужно проверить, что тип ТЕ привязан к классу места
            if (CheckTEType2PlaceClass.Get(context))
            {
                var filter = string.Format("(tetypecode_r='{0}' and placeclasscode_r='{1}')", te.TETypeCode, place.PlaceClassCode);
                var mgr = IoC.Instance.Resolve<IBaseManager<TEType2PlaceClass>>();
                var teTypeToPlaceClassCheck = mgr.GetFiltered(filter);
                if (!teTypeToPlaceClassCheck.Any())
                {
                    ErrorMessage.Set(context, string.Format("Тип ТЕ ({0}) данного ТЕ не привязан к классу ({1}) выбранного места", te.TETypeCode, place.PlaceClassCode));
                    return false;
                }
            }

            // если нужно проверить, что вес ТЕ соответстует разрешенному весу места
            if (CheckTEWeightWithPlaceWeight.Get(context))
            {
                if (te.TEWeight > place.PlaceWeight)
                {
                    ErrorMessage.Set(context, string.Format("Вес ТЕ ({0}) больше допустимой нагрузки на место ({1})", te.TEWeight, place.PlaceWeight));
                    return false;
                }
            }

            // если нужно проверить, что совокупный вес тех ТЕ, которые уже стоят на местах одной группы мест с нашим местом назначения 
            // (а так же перемещаются туда по другим ЗНТ), и вес перемещаемой ТЕ не превышают максимальный вес на группу мест
            if (CheckAllTEWeightWithPlaceGroupWeight.Get(context))
            {
                var placeGroupCode = place.PlaceGroupCode;
                var filter = string.Format("tecode in (select te.tecode from wmste te inner join wmsplace pl on te.tecurrentplace = pl.placecode inner join wmstransporttask tt on tt.ttaskstartplace = pl.placecode or tt.ttaskcurrentplace = pl.placecode or tt.ttasknextplace = pl.placecode or tt.ttaskfinishplace = pl.placecode where pl.placegroupcode = '{0}')", placeGroupCode);
                var mgr = IoC.Instance.Resolve<IBaseManager<TE>>();
                var teList = mgr.GetFiltered(filter);
                var sum = teList.Sum(o => o.TEWeight);
                if ((sum + te.TEWeight) > place.PlaceWeightGroup)
                {
                    ErrorMessage.Set(context, string.Format("Превышение максимального веса на группу ({2}). Вес уже размещенных ТЕ={0}, текущей ТЕ={1}", sum, te.TEWeight, place.PlaceWeightGroup));
                    return false;
                }
            }

            // если нужно проверить, что ТЕ можно поместить на место по габаритам
            if (CheckTESizeWithPlaceSize.Get(context))
            {
                // по высоте
                if (te.TEHeight > place.PlaceHeight)
                {
                    ErrorMessage.Set(context, string.Format("Высота ТЕ ({0}) превышает высоту места ({1})", te.TEHeight, place.PlaceHeight));
                    return false;
                }

                var max1 = te.TEWidth > te.TELength ? te.TEWidth : te.TELength;
                var min1 = te.TEWidth < te.TELength ? te.TEWidth : te.TELength;
                var max2 = place.PlaceWidth > place.PlaceLength ? place.PlaceWidth : place.PlaceLength;
                var min2 = place.PlaceWidth < place.PlaceLength ? place.PlaceWidth : place.PlaceLength;
                if (max1 > max2 || min1 > min2)
                {
                    ErrorMessage.Set(context, string.Format("Размеры ТЕ ({0}x{1}) превышают размеры места ({2}x{3}) по ширине или длине", min1, max1, min2, max2));
                    return false;
                }
            }

            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    /// <summary>
    /// Менеджер мест.
    /// </summary>
    public class PlaceManager : WMSBusinessObjectManager<Place, string>, IPlaceManager, IBlockingManager
    {
        private const int HashMaxValue = 1000;

        public bool FillPlaceCode(Place entity)
        {
            if (entity.SegmentCode == null)
                return false;

            Segment seg;
            SegmentType segType;
            Area area;
            using (var mgrSegment = GetManager<Segment>())
                seg = mgrSegment.Get(entity.SegmentCode);
            if (seg == null)
                return false;

            if (seg.AreaCode_R == null)
                return false;

            using (var mgrArea = GetManager<Area>())
                area = mgrArea.Get(seg.AreaCode_R);
            if (area == null)
                return false;

            if (seg.SegmentTypeCode_R == null)
                return false;

            using (var mgrSegmentType = GetManager<SegmentType>())
                segType = mgrSegmentType.Get(seg.SegmentTypeCode_R);
            if (segType == null)
                return false;

            entity.PlaceCode = segType.SegmentTypeCodeFormat;
            entity.PlaceName = segType.SegmentTypeCodeView;
            entity.AreaName = area.AreaName;
            entity.WarehouseName = area.Warehouse;
            entity.SegmentCode_R_Number = seg.SegmentNumber;

            return true;
        }

        public bool FillFromPlaceType(Place entity)
        {
            if (entity.PlaceTypeCode == null)
                return false;

            PlaceType pt;
            using (var mgrPlaceType = GetManager<PlaceType>())
                pt = mgrPlaceType.Get(entity.PlaceTypeCode);
            if (pt == null)
                return false;

            entity.PlaceWidth = pt.Width;
            entity.PlaceLength = pt.Length;
            entity.PlaceHeight = pt.Height;
            entity.PlaceCapacityMax = pt.Capacity;
            entity.PlaceWeight = pt.MaxWeight;
            return true;
        }


        protected override void BeforeInsert(ref Place entity)
        {
            base.BeforeInsert(ref entity);
            FillCheckNumbers(entity);
            FillPlaceCapacity(entity);
            FillParamGroup(entity);
        }

        private void FillParamGroup(Place entity)
        {
            if (entity.PlaceWeightGroup == 0)
                entity.PlaceWeightGroup = entity.PlaceWeight;
        }

        protected override void BeforeInsert(ref IEnumerable<Place> entities)
        {
            base.BeforeInsert(ref entities);
            foreach (var entity in entities)
            {
                FillCheckNumbers(entity);
                FillPlaceCapacity(entity);
                FillParamGroup(entity);
            }
        }

        private static void FillCheckNumbers(Place entity)
        {
            if (string.IsNullOrEmpty(entity.PlaceCheck))
                entity.PlaceCheck = CheckHash(entity.PlaceCode, HashMaxValue).ToString(CultureInfo.InvariantCulture);
            if (string.IsNullOrEmpty(entity.PlaceCheckY))
                entity.PlaceCheckY = CheckHash(
                    entity.SegmentCode + entity.PlaceS.ToString(CultureInfo.InvariantCulture) +
                        entity.PlaceX.ToString(CultureInfo.InvariantCulture), HashMaxValue)
                            .ToString(CultureInfo.InvariantCulture);
        }

        public static int CheckHash(string inString, int maxValue)
        {
            if (string.IsNullOrEmpty(inString))
                throw new ArgumentNullException("inString");

            const long maxHash = 67108864L; //(long)Math.Pow(2, 26);
            long hash = 0;
            long overload = 0;
            for (var i = inString.Length - 1; i >= 0; i--)
            {
                var mychar = Convert.ToInt16(inString[i]);
                hash = 19 * hash + mychar;
            }
            if (hash > maxHash)
            {
                overload = hash;
                hash = 0;
            }
            var res = (int)((overload + hash) % maxValue);
            return res < 0 ? -1 * res : res;
        }

        public void UpdateFormulasGroupProperties(IEnumerable<Place> entities)
        {
            if (entities == null)
                return;

            var count = 0;
            var segmentGroupCount = 0;
            string placeGroupCode = null;
            var segmentWeightGroup = 0;
            var placessorted = entities.OrderBy(p => p.PlaceX).ToArray();
            Func<WMSBusinessObject, string> getGroupCodeHandler = entity => entity.GetKey().To<string>();

            foreach (var place in placessorted) //Сортируем по горизотали
            {
                if (count == 0) //получаем необходимые данные 
                {
                    Segment segment;
                    using (var segmentManager = GetManager<Segment>())
                        segment = segmentManager.Get(place.SegmentCode, GetModeEnum.Partial);
                    if (segment != null)
                    {
                        SegmentType segmentType;
                        using (var segmentTypeManager = GetManager<SegmentType>())
                            segmentType = segmentTypeManager.Get(segment.SegmentTypeCode_R);
                        if (segmentType != null && segmentType.GlobalParamVal != null && segmentType.GlobalParamVal.Count > 0)
                        {
                            placeGroupCode = getGroupCodeHandler(place);
                            segmentGroupCount = segmentType.GlobalParamVal.Where(p => p.GlobalParamCode_R.To(SegmentTypeGpv.None) == SegmentTypeGpv.SegmentGroupCount)
                                .Select(p => p.GparamValValue).FirstOrDefault().To(0);
                            segmentWeightGroup = segmentType.GlobalParamVal.Where(p => p.GlobalParamCode_R.To(SegmentTypeGpv.None) == SegmentTypeGpv.SegmentWeightGroup)
                                .Select(p => p.GparamValValue).FirstOrDefault().To(0);
                        }
                    }
                }

                if (segmentGroupCount > 0) //разбиваем на группы
                {
                    place.PlaceGroupCode = placeGroupCode; //код группы

                    if (segmentWeightGroup > 0)
                    {
                        place.PlaceWeightGroup = segmentWeightGroup; //Вес на группу
                    }
                }

                count++;
            }

            //проставляем оставшиеся свойства, если есть группы
            var groupby = placessorted.Where(p => !string.IsNullOrEmpty(p.PlaceGroupCode)).GroupBy(p => p.PlaceGroupCode);
            foreach (var gr in groupby) //по каждой группе
            {
                decimal weightGroup = 0;
                var updatePlaceWeightGroup = false;
                if (segmentWeightGroup <= 0)
                {
                    weightGroup = gr.Sum(p => p.PlaceWeight); //Вес на группу
                    updatePlaceWeightGroup = true;
                }

                foreach (var p in gr.Where(p => updatePlaceWeightGroup))
                    p.PlaceWeightGroup = weightGroup;
            }
        }

        private void FillPlaceCapacity(Place entity)
        {
            entity.PlaceCapacity = entity.PlaceCapacityMax;
        }

        public void Block(WMSBusinessObject obj, ProductBlocking blocking, string description)
        {
            // TODO Здесь должна быть проверка прав на блокировку

            if (obj == null)
                throw new ArgumentNullException("obj");

            var place = (Place)obj;
            var filter = string.Format("{0} = '{1}' AND {2} = '{3}'",
                                           SourceNameHelper.Instance.GetPropertySourceName(typeof(Place2Blocking),
                                                                                           Place2Blocking
                                                                                               .Place2BlockingPlaceCodePropertyName),
                                           place.GetKey().ToString().ToUpper(),
                                           SourceNameHelper.Instance.GetPropertySourceName(typeof(Place2Blocking),
                                                                                           Place2Blocking
                                                                                               .Place2BlockingBlockingCodePropertyName),
                                           blocking.GetKey().ToString().ToUpper());
            using (var mgr = GetManager<Place2Blocking>())
            {
                var existsBlockings = mgr.GetFiltered(filter);
                if (existsBlockings.Any())
                    throw new OperationException("Для места с кодом '{0}', уже существует блокировка с кодом '{1}'",
                                                 place.PlaceCode, blocking.BlockingCode);


                var place2Block = new Place2Blocking
                    {
                        Place2BlockingDesc = description,
                        Place2BlockingBlockingCode = blocking.BlockingCode,
                        Place2BlockingPlaceCode = place.PlaceCode
                    };
                try
                {
                    ((ISecurityAccess)this).SuspendRightChecking();
                    mgr.Insert(ref place2Block);
                }
                finally
                {
                    ((ISecurityAccess)this).ResumeRightChecking();
                }
            }
        }

        public string GetNameLookupBlocking()
        {
            return "_PRODUCTBLOCKING_PLACE";
        }
        public Type GetBlockingType()
        {
            return typeof(Place2Blocking);
        }
    }
}
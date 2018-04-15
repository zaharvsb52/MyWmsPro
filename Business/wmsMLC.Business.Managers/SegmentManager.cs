using System;
using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    /// <summary>
    /// Менеджер Segment.
    /// </summary>
    public class SegmentManager : WMSBusinessObjectManager<Segment, string>, IBlockingManager
    {
        public void Block(WMSBusinessObject obj, ProductBlocking blocking, string description)
        {
            // TODO Здесь должна быть проверка прав на блокировку

            if (obj == null)
                throw new ArgumentNullException("obj");

            var segment = (Segment)obj;
            var filter = string.Format("{0} = '{1}' AND {2} = '{3}'",
                                       SourceNameHelper.Instance.GetPropertySourceName(typeof(Segment2Blocking), Segment2Blocking.SegmentCodePropertyName), segment.GetKey().ToString().ToUpper(),
                                       SourceNameHelper.Instance.GetPropertySourceName(typeof(Segment2Blocking), Segment2Blocking.BlockingCodePropertyName), blocking.GetKey().ToString().ToUpper());
            using (var mgr = GetManager<Segment2Blocking>())
            {
                var existsBlockings = mgr.GetFiltered(filter);
                if (existsBlockings.Any())
                    throw new OperationException("Для сектора с кодом '{0}', уже существует блокировка с кодом '{1}'",
                        segment.SegmentCode, blocking.BlockingCode);

                var segment2Block = new Segment2Blocking
                {
                    Segment2BlockingDesc = description,
                    Segment2BlockingBlockingCode = blocking.BlockingCode,
                    Segment2BlockingSegmentCode = segment.SegmentCode
                };
                try
                {
                    ((ISecurityAccess)this).SuspendRightChecking();
                    mgr.Insert(ref segment2Block);
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
            return typeof(Segment2Blocking);
        }
    }
}
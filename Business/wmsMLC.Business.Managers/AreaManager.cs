using System;
using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    /// <summary>
    /// Менеджер Area.
    /// </summary>
    public class AreaManager : WMSBusinessObjectManager<Area, string>, IBlockingManager
    {
        public void Block(WMSBusinessObject obj, ProductBlocking blocking, string description)
        {
            // TODO Здесь должна быть проверка прав на блокировку
            if (obj == null)
                throw new ArgumentNullException("obj");

            var area = (Area)obj;
            var filter = string.Format("{0} = '{1}' AND {2} = '{3}'",
                SourceNameHelper.Instance.GetPropertySourceName(typeof(Area2Blocking),
                    Area2Blocking.AreaCodePropertyName), area.GetKey().ToString().ToUpper(),
                SourceNameHelper.Instance.GetPropertySourceName(typeof(Area2Blocking),
                    Area2Blocking.BlockingCodePropertyName), blocking.GetKey().ToString().ToUpper());
            using (var mgr = GetManager<Area2Blocking>())
            {
                var existsBlockings = mgr.GetFiltered(filter);
                if (existsBlockings.Any())
                    throw new OperationException("Для области с кодом '{0}', уже существует блокировка с кодом '{1}'",
                        area.AreaCode, blocking.BlockingCode);

                var area2Block = new Area2Blocking
                {
                    Area2BlockingDesc = description,
                    Area2BlockingBlockingCode = blocking.BlockingCode,
                    Area2BlockingAreaCode = area.AreaCode
                };
                try
                {
                    ((ISecurityAccess)this).SuspendRightChecking();
                    mgr.Insert(ref area2Block);
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
            return typeof(Area2Blocking);
        }

    }
}
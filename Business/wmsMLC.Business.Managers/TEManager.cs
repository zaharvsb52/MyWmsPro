using System;
using System.Collections.Generic;
using System.Linq;
using wmsMLC.Business.DAL;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    /// <summary>
    /// Менеджер ТЕ.
    /// </summary>
    public class TEManager : WMSBusinessObjectManager<TE, string>, IMandantHandler, ITEManager, IBlockingManager
    {
        public decimal? GetMandantCode(WMSBusinessObject bo)
        {
            return null;
        }

        public void FillCreationPlace(TE entity)
        {
            string clientPlace;

            using (var mgrSession = (IClientSessionManager)GetManager<ClientSession>())
                clientPlace = mgrSession.GetClientInputPlaceValue();

            if (string.IsNullOrEmpty(clientPlace))
                entity.CreatePlace = clientPlace;
        }

        public bool FillDimensionCharacteristics(TE entity)
        {
            if (entity == null || entity.TETypeCode == null)
                return false;

            TEType teType = null;
            using (var mgrTEType = GetManager<TEType>())
                teType = mgrTEType.Get(entity.TETypeCode);

            if (teType == null)
                return false;

            entity.TEHeight = teType.Height;
            entity.TELength = teType.Length;
            entity.TEMaxWeight = teType.MaxWeight;
            entity.TEWeight = teType.TareWeight;
            entity.TEWidth = teType.Width;
            return true;
        }

        public void Block(WMSBusinessObject obj, ProductBlocking blocking, string description)
        {
            // TODO Здесь должна быть проверка прав на блокировку
            if (obj == null)
                throw new ArgumentNullException("obj");

            var te = (TE)obj;
            var filter = string.Format("{0} = '{1}' AND {2} = '{3}'",
                                       SourceNameHelper.Instance.GetPropertySourceName(typeof(TE2Blocking), TE2Blocking.TECodePropertyName), te.GetKey().ToString().ToUpper(),
                                       SourceNameHelper.Instance.GetPropertySourceName(typeof(TE2Blocking), TE2Blocking.BlockingCodePropertyName), blocking.GetKey().ToString().ToUpper());
            using (var mgr = GetManager<TE2Blocking>())
            {
                var existsBlockings = mgr.GetFiltered(filter);
                if (existsBlockings.Any())
                    throw new OperationException("Для ТЕ с кодом '{0}', уже существует блокировка с кодом '{1}'",
                        te.GetKey(), blocking.BlockingCode);

                var te2Block = new TE2Blocking
                {
                    TE2BlockingDesc = description,
                    TE2BlockingBlockingCode = blocking.BlockingCode,
                    TE2BlockingTECode = te.GetKey().To<string>()
                };
                try
                {
                    ((ISecurityAccess)this).SuspendRightChecking();
                    mgr.Insert(ref te2Block);
                }
                finally
                {
                    ((ISecurityAccess)this).ResumeRightChecking();
                }
            }
        }

        public string GetNameLookupBlocking()
        {
            return "_PRODUCTBLOCKING_TE";
        }

        public Type GetBlockingType()
        {
            return typeof(TE2Blocking);
        }

        public IEnumerable<TE> GetByCurrentPlace(string placeCode, GetModeEnum mode = GetModeEnum.Partial)
        {
            if (string.IsNullOrEmpty(placeCode))
                throw new ArgumentNullException("placeCode");

            var filter = string.Format("{0} = '{1}'", TE.CurrentPlacePropertyName, placeCode);
            return GetFiltered(filter, mode);
        }

        public IEnumerable<TE> GetPackingTEOnPlace(string placeCode, GetModeEnum mode = GetModeEnum.Partial)
        {
            if (string.IsNullOrEmpty(placeCode))
                throw new ArgumentNullException("placeCode");

            // фильтр, который отберет все ТЕ с данного места у которых тип - упаковка
            var filter = string.Format("{0} = '{1}' AND NVL(TEPACKSTATUS,' ') != 'TE_PKG_NONE' ", TE.CurrentPlacePropertyName, placeCode);
            return GetFiltered(filter, mode);
        }

        public void ChangeStatus(string teCode, string operation)
        {
            using (var repo = (ITERepository)GetRepository())
                repo.ChangeStatus(teCode, operation);
        }
    }
}

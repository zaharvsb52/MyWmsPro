using System;
using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Business.Managers
{
    /// <summary>
    /// Менеджер LookUP-ов.
    /// </summary>
    public class ObjectLookUpManager : WMSBusinessObjectManager<ObjectLookUp, string>, IGetLookupInfo
    {
        public override ObjectLookUp Get(string key, GetModeEnum mode = GetModeEnum.Full)
        {
            // не лезем в БД за единичными записями (получаем сразу все и кэшируем)
            return GetAll(mode).FirstOrDefault(i => key.Equals(i.GetKey()));
        }

        public override ObjectLookUp Get(string key, string attrentity)
        {
            // не лезем в БД за единичными записями (получаем сразу все и кэшируем)
            return GetAll(attrentity).FirstOrDefault(i => key.Equals(i.GetKey()));
        }

        #region ILookupInfo
        public LookupInfo GetLookupInfo(string lookUpCodeEditor)
        {
            if (string.IsNullOrEmpty(lookUpCodeEditor))
                throw new ArgumentNullException(lookUpCodeEditor);

            var objectLookup = Get(lookUpCodeEditor);
            if (objectLookup == null)
                throw new DeveloperException("Unknown lookup code '{0}'.", lookUpCodeEditor);

            return new LookupInfo(codeEditor: lookUpCodeEditor, valueMember: objectLookup.ObjectLookupPkey,
                displayMember: objectLookup.ObjectLookupDisplay, filter: objectLookup.ObjectLookupFilter,
                fetchRowCount: objectLookup.ObjectLookupFetchRowCount, itemSource: objectLookup.ObjectLookupSource,
                objectLookupSimple: objectLookup.ObjectLookupSimple);
        }
        #endregion ILookupInfo
    }
}
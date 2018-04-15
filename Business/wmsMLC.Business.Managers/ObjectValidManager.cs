using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Business.Managers
{
    public class ObjectValidManager : WMSBusinessObjectManager<ObjectValid, decimal>
    {
        public override ObjectValid Get(decimal key, GetModeEnum mode = GetModeEnum.Full)
        {
            // не лезем в БД за единичными записями (получаем сразу все и кэшируем)
            return GetAll(mode).FirstOrDefault(i => i.ObjectValidId == key);
        }

        public override ObjectValid Get(decimal key, string attrentity)
        {
            // не лезем в БД за единичными записями (получаем сразу все и кэшируем)
            return GetAll(attrentity).FirstOrDefault(i => i.ObjectValidId == key);
        }
    }
}
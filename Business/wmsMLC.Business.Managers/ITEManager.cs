using System.Collections.Generic;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    public interface ITEManager : IBaseManager<TE, string>
    {
        void FillCreationPlace(TE te);
        bool FillDimensionCharacteristics(TE te);
        IEnumerable<TE> GetByCurrentPlace(string placeCode, GetModeEnum mode = GetModeEnum.Partial);
        IEnumerable<TE> GetPackingTEOnPlace(string placeCode, GetModeEnum mode = GetModeEnum.Partial);

        // сменить статус ТЕ через машину состояний
        void ChangeStatus(string teCode, string operation);
    }
}
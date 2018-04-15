using System.Collections.Generic;
using wmsMLC.Business.Objects;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    public interface IPlaceManager : IBaseManager<Place, string>
    {
        bool FillPlaceCode(Place place);
        bool FillFromPlaceType(Place place);
        void UpdateFormulasGroupProperties(IEnumerable<Place> entities);
    }
}
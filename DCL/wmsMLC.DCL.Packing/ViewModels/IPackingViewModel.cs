using wmsMLC.DCL.General.ViewModels;

namespace wmsMLC.DCL.Packing.ViewModels
{
    public interface IPackingViewModel : IModelHandler
    {
        string CurrentPlaceCode { get; set; }
        string PlaceFilter { get; set; }
        bool Hide { get; set; }
        void Pack(bool isAllPack);
        void Pack();
        void MoveTo();
        bool CanMove();
        void ReturnOnSourceTE();
        bool CanPack();
        bool CanReturnOnSourceTE();
    }
}
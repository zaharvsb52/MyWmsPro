namespace wmsMLC.DCL.General.ViewModels
{
    public interface IValueEditController
    {
        bool CanEdit();
        bool EnableEdit(object entity, string propertyName);
    }
}

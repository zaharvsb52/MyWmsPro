using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectView))]
    public class InvTaskGroupViewModel : ObjectViewModelBase<InvTaskGroup>, IValueEditController
    {
        bool IValueEditController.CanEdit()
        {
            throw new System.NotImplementedException();
        }

        public bool EnableEdit(object entity, string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return false;            
            if (propertyName.Equals(string.Empty))
                return true;

            var invTask = entity as InvTask;
            if (invTask == null)
                throw new DeveloperException("Объект не является типом InvTask");
            return invTask.GetProperty<bool>(InvTask.INVTASKMANUALPropertyName) || propertyName.EqIgnoreCase(InvTask.INVTASKCOUNTPropertyName);
        }
    }
}

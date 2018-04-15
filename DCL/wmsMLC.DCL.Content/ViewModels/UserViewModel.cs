using System.Linq;
using System.Windows;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof (ObjectView))]
    public class UserViewModel : ObjectViewModelBase<User>
    {
        protected override bool Save()
        {
            if (Source == null)
                return false;

            using (var mgrUser = IoC.Instance.Resolve<IBaseManager<ClientSession>>())
            {
                var cs = mgrUser.GetFiltered(string.Format("DATEINS > sysdate-2 and USERCODE_R='{0}' and clientsessionend is null", Source.GetKey()) );
                if (cs == null || !cs.Any()) 
                    return base.Save();

                var vs = GetViewService();
                var message = string.Format("” пользовател€ '{0}' есть активна€ сесси€.\n»зменение данных может привести к нестабильной работе этой сессии.\n\n ¬ы уверены, что хотите сохранить изменени€?", Source.GetKey());
                var res = vs.ShowDialog(StringResources.Save, message, MessageBoxButton.OKCancel, MessageBoxImage.Question,
                    MessageBoxResult.Cancel);

                if (!MessageBoxResult.OK.Equals(res))
                    return false;
            }
            return base.Save();
        }
    }
}
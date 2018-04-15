using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectView))]
    public class BillWorkAct2Op2CViewModel : ObjectViewModelBase<BillWorkAct2Op2C>
    {
        private bool _isCan = true;
        private bool _isFirst = true;

        protected override void SourceObjectPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.SourceObjectPropertyChanged(sender, e);

            if (Source == null)
                return;

            var editable = Source as IEditable;
            if (editable.IsInRejectChanges)
                return;

            if (!e.PropertyName.EqIgnoreCase(BillWorkAct2Op2C.WorkActIDPropertyName))
                return;

            CheckStatus();

            RiseCommandsCanExecuteChanged();
        }

        protected override bool CanCloseInternal()
        {
            if (Check())
                return base.CanCloseInternal();


            if (!InPropertyEditMode) 
                return RejectChanges();

            foreach (var editableItem in PropertyEditSource.Select(editItem => editItem as IEditable))
            {
                if (editableItem == null)
                    throw new DeveloperException(DeveloperExceptionResources.CantRejectChangesInNonEditableObject);
                editableItem.RejectChanges();
            }

            return RejectChanges();
        }

        protected override bool CanSave()
        {
            return Check() && base.CanSave();
        }
        
        protected override bool CanRefresh()
        {
            return Check() && base.CanRefresh();
        }

        protected override bool CanDelete()
        {
            return Check() && base.CanDelete();
        }

        protected override bool CanSaveAndClose()
        {
            return Check() && base.CanSaveAndClose();
        }

        private bool CheckStatus()
        {
            using (var mgr = IoC.Instance.Resolve<IBaseManager<BillWorkAct>>())
            {
                var attrEntity = FilterHelper.GetAttrEntity(typeof(BillWorkAct), BillWorkAct.WORKACTIDPropertyName,
                    BillWorkAct.STATUSCODE_RPropertyName);
                var el = mgr.Get(Source.WorkActID, attrEntity);

                if (el == null || el.STATUSCODE_R != BillWorkActStatus.WORKACT_COMPLETED.ToString())
                {
                    _isCan = true;
                    return true;
                }
            }

            _isCan = false;
            return false;
        }


        private bool Check()
        {
            if (Source == null)
                return true;

            var isFirst = _isFirst;

            if (_isFirst)
                _isFirst = false;
            
            if (!_isCan)
                return false;

            return !isFirst || CheckStatus();
        }

       
    }
}
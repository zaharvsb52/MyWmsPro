using System.Windows.Input;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Utils;

namespace wmsMLC.DCL.Main.Views.Controls
{
    public class CustomDateEdit : DateEdit
    {
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            //Делаем проверку установлен ли фокус на контрол и только в этом случае позволяем изменение даты при прокрутке колесика мышки
            if (AllowSpinOnMouseWheel && EditCore != null && !FocusHelper.IsFocused(EditCore)) 
                return;
            base.OnPreviewMouseWheel(e);
        }
    }
}

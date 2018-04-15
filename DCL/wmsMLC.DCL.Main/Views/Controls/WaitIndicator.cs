using System.Windows;
using DevExpress.Xpf.Core;
using WPFLocalizeExtension.Extensions;

namespace wmsMLC.DCL.Main.Views.Controls
{
    public class CustomWaitIndicator : WaitIndicator
    {
        public CustomWaitIndicator()
        {
            DeferedVisibility = false;
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;

            string txt;
            if (new LocExtension("wmsMLC.DCL.Resources:StringResources:Wait").ResolveLocalizedValue(out txt))
            {
                Content = txt;
            }
            //Content = System.ComponentModel.DesignerProperties.GetIsInDesignMode(this) ? "Wait..." : StringResources.Wait;
        }
    }
}

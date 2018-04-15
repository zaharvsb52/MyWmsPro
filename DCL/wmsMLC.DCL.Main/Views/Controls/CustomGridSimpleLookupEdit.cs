using DevExpress.Xpf.Editors;
using wmsMLC.DCL.Resources;

namespace wmsMLC.DCL.Main.Views.Controls
{
    /// <summary>
    /// Простой контрол, который служит для отображения списков подбора в Grid-ах
    /// </summary>
    public class CustomGridSimpleLookupEdit : TextEdit
    {
        protected override void OnLoadedInternal()
        {
            //Баг версии 15.2. Непонятные вызовы события OnLoad при активизации формы приводят к стиранию значения лукапа
            if (!IsLoaded)
                base.OnLoadedInternal();
        }

        protected override string GetCustomDisplayText(object editValue, string displayText)
        {
            if (editValue == null)
                return base.GetCustomDisplayText(null, displayText);

            var settings = Settings as CustomGridSimpleLookupEditSettings;
            if (settings != null)
            {
                if (settings.IsInLoading)
                    return StringResources.Wait;

                var val = settings.GetDisplayValueByEditValue(editValue);
                if (val != null)
                    return val is string ? (string)val : val.ToString();
            }

            return editValue is string ? (string)editValue : editValue.ToString();
        }
    }
}
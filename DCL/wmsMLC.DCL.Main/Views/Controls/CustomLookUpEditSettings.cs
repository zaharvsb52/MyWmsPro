using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Helpers;

namespace wmsMLC.DCL.Main.Views.Controls
{
    /// <summary>
    /// Settings-ы для "честных" Lookup-ов.
    /// </summary>
    public class CustomLookUpEditSettings : CustomBaseLookupEditSetting
    {
        #region .  Constructors  .
        static CustomLookUpEditSettings()
        {
            //EditorSettingsProvider.Default.RegisterUserEditor(typeof(CustomLookUpEdit), typeof(CustomLookUpEditSettings),
            //    () =>
            //    {
            //        var customLookUpEdit = new CustomLookUpEdit();
            //        //Мы почему-то не можем найти стиль по умолчанию. Помогаем.
            //        var style = Application.Current.TryFindResource(customLookUpEdit.GetType()) as Style;
            //        if (style != null)
            //            customLookUpEdit.Style = style;
            //        return customLookUpEdit;
            //    },
            //    () => new CustomLookUpEditSettings());

            EditorSettingsProvider.Default.RegisterUserEditor2(typeof(CustomLookUpEdit),
            typeof(CustomLookUpEditSettings),
            optimized => optimized ? (IBaseEdit)new InplaceBaseEdit() : new CustomLookUpEdit(),
            () => new CustomLookUpEditSettings());
        }

        #endregion .  Constructors  .

        //protected override void AssignToEditCore(IBaseEdit edit)
        //{
        //    base.AssignToEditCore(edit);
        //    var ed = edit as CustomLookUpEdit;
        //    if (ed != null)
        //        ed.IsSimpleMode = true;
        //}
    }
}

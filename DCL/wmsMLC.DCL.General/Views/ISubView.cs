using System.Windows;
using wmsMLC.DCL.General.ViewModels;

namespace wmsMLC.DCL.General.Views
{
    public interface ISubView
    {
        DependencyProperty SourceProperty { get; }
        bool IsReadOnly { get; set; }
        IModelHandler ParentViewModel { get; set; }
        
        /// <summary>
        /// Можно ли этот объект сохранять отдельно от основного.
        /// </summary>
        bool ShouldUpdateSeparately { get; set; }
    }
}

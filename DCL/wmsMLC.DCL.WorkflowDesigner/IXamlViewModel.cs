using System.Windows;
using ICSharpCode.AvalonEdit.Document;

namespace wmsMLC.DCL.WorkflowDesigner
{
    public interface IXamlViewModel
    {
        TextDocument Document
        {
            get;
            set;
        }

        Visibility TextBoxVisibility
        {
            get;
            set;
        }

        string ErrorText
        {
            get;
        }

        Visibility ErrorVisibility
        {
            get;
        }
    }
}

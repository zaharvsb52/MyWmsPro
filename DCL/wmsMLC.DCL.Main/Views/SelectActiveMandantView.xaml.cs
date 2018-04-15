using System;
using System.ComponentModel;
using System.Windows;
using DevExpress.Utils;
using DevExpress.Xpf.Grid;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Resources;
using wmsMLC.General.PL.WPF;

namespace wmsMLC.DCL.Main.Views
{
    /// <summary>
    /// Interaction logic for PrintView.xaml
    /// </summary>
    public partial class SelectActiveMandantView : BaseDialogWindow, IHelpHandler
    {
        public SelectActiveMandantView()
        {
            InitializeComponent();
        }

        #region . IHelpHandler .
        string IHelpHandler.GetHelpLink()
        {
            return "wmsMLC";
        }

        string IHelpHandler.GetHelpEntity()
        {
            return "SelectMandant";
        }
        #endregion
    }
}

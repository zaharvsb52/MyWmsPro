using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using wmsMLC.General.PL.Model;

namespace wmsMLC.DCL.General.ViewModels
{
    public interface IFieldProvider
    {
        FrameworkElement GetElement(DataField field);
    }
}

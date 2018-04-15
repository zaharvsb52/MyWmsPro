using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wmsMLC.Business.Objects;

namespace wmsMLC.DCL.General.ViewModels
{
    public interface ISelectableSublistView
    {
        EventHandler OnChangeSelectItem { get; set; }
        Dictionary<string, object> SelectedItems { get; set; }
    }
}

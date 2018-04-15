using System;
using System.Collections.Generic;
using System.Linq;
using wmsMLC.General;
using wmsMLC.General.PL.Model;

namespace wmsMLC.Activities.Dialogs.Models
{
    public class DialogModel
    {
        public DialogModel()
        {
            Fields = new List<ValueDataField>();
        }

        public string Header { get; set; }
        public string Description { get; set; }
        public double FontSize { get; set; }
        public List<ValueDataField> Fields { get; private set; }

        public ValueDataField this[string name]
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentNullException("name");

                var result = Fields.SingleOrDefault(p => name.EqIgnoreCase(p.Name));
                if (result == null)
                    throw new DeveloperException("Can't find field with name '{0}' in Fields.", name);
                return result;
            }
        }

        //public void SetWarning(int index, string message, bool visible)
        //{
        //    if (index < 0 || index >= Fields.Count)
        //        throw new ArgumentOutOfRangeException("index");
        //    var field = Fields[index];
        //    field.Caption = message;
        //    field.Visible = visible;
        //}
    }
}

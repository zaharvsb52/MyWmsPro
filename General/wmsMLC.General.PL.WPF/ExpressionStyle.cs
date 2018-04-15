using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace wmsMLC.General.PL.WPF
{
    public class ExpressionStyle
    {
        public static readonly Version ActualVersion = new Version(2, 0, 0, 2);

        public ExpressionStyle()
        {
            Options = new List<ExpressionStyleOption>();
            Version = ActualVersion.ToString();
        }

        public string Version { get; set; }

        [XmlArray("ArrayOfExpressionStyleOption")]
        public List<ExpressionStyleOption> Options { get; set; }

        public void Clear()
        {
            Options.Clear();
        }
    }
}

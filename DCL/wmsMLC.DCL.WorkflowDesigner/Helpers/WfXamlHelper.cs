using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace wmsMLC.DCL.WorkflowDesigner.Helpers
{
    //http://blogs.msdn.com/b/jpricket/archive/2012/07/17/tfs-2012-cleaning-up-workflow-xaml-files-aka-removing-versioned-namespaces.aspx
    public class WfXamlHelper
    {
        private const string XmlNamespaceRegex = "(xmlns\\:)(.*?)=\"(.*?)\"";

        public XamlNamespace[] GetNamespaces(string wfContents)
        {
            var regex = new Regex(XmlNamespaceRegex, RegexOptions.Singleline);
            return (from Match m in regex.Matches(wfContents)
                select
                    new XamlNamespace
                    {
                        Prefix = m.Groups[2].Value,
                        Namespace = m.Groups[3].Value,
                        Declaration = m.Groups[0].Value
                    }).ToArray();
        }

        public string[] FindActivityByNameSpace(string wfContents, string mask, XamlNamespace[] namespaces = null)
        {
            if (namespaces == null)
                namespaces = GetNamespaces(wfContents);
            var activitynms = namespaces.Where(p => p.Namespace.Contains(mask)).ToArray();

            const string activityformat = @"<{0}:(?<Activity>\w*)";

            var result = new List<string>();
            foreach (var n in activitynms)
            {
                var regex = new Regex(string.Format(activityformat, n.Prefix), RegexOptions.Singleline);
                result.AddRange(from Match m in regex.Matches(wfContents) select m.Groups[1].Value);
            }

            return result.Distinct().ToArray();
        }

        //public void RemoveUnusedNamespaces(string wfContents, string newFileContents, bool reportOnly)
        //{
        //    var namespaces = new List<XamlNamespace>();
        //    var ignoredNamespaces = new List<String>();
        //    newFileContents = wfContents;
        //    var ignorableRegex = new Regex("(:Ignorable=\")(.*?)\"", RegexOptions.Singleline);
        //    var regex = new Regex(XmlNamespaceRegex, RegexOptions.Singleline);

        //    foreach (Match m in ignorableRegex.Matches(wfContents))
        //    {
        //        ignoredNamespaces.AddRange(m.Groups[2].Value.Split(' '));
        //    }

        //    foreach (Match m in regex.Matches(wfContents))
        //    {
        //        namespaces.Add(new XamlNamespace() { Prefix = m.Groups[2].Value, Namespace = m.Groups[3].Value, Declaration = m.Groups[0].Value });
        //    }

        //    foreach (var ns in namespaces)
        //    {
        //        // Only remove namespaces that are not in the Ignore section
        //        // and contain version=10
        //        // and are not used in the file
        //        if (!ignoredNamespaces.Contains(ns.Prefix) &&
        //            ns.Namespace.IndexOf("version=10", StringComparison.OrdinalIgnoreCase) >= 0 &&
        //            !wfContents.Contains(ns.Prefix + ":"))
        //        {
        //            Console.WriteLine("Removing unused namespace: {0}", ns.Declaration);
        //            newFileContents = newFileContents.Replace(ns.Declaration, String.Empty);
        //        }
        //    }

        //    //if (!reportOnly)
        //    //{
        //    //    File.WriteAllText(outputFile, newFileContents, Encoding.UTF8);
        //    //}
        //}

        public class XamlNamespace
        {
            public string Prefix { get; set; }
            public string Namespace { get; set; }
            public string Declaration { get; set; }
        }
    }
}

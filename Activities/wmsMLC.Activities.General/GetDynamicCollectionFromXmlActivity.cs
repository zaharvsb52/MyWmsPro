using System;
using System.Activities;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using wmsMLC.General.BL.Validation.Attributes;

namespace wmsMLC.Activities.General
{
    public class GetDynamicCollectionFromXmlActivity : NativeActivity<dynamic[]>
    {
        [Required]
        public InArgument<string> InputXml { get; set; }

        [Required]
        public InArgument<string> Descendant { get; set; }        

        public GetDynamicCollectionFromXmlActivity()
        {
            this.DisplayName = GetType().Name.Replace("Activity", string.Empty);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var result = GetExpandoFromXml(InputXml.Get(context), Descendant.Get(context));
            context.SetValue(Result, result as Array);
        }

        public dynamic[] GetExpandoFromXml(string xml, string descendantid)
        {
            var expandoFromXml = new List<dynamic>();

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                var reader = new XmlTextReader(stream);
                var doc = XDocument.Load(reader);
                if (doc.Root != null)
                {
                    foreach (var element in doc.Root.Descendants(descendantid))
                    {
                        dynamic expandoObject = new ExpandoObject();
                        var dictionary = expandoObject as IDictionary<string, object>;
                        dictionary[descendantid] = element.Value;
                        foreach (var child in element.Descendants())
                        {
                            if (child.Name.Namespace == "")
                                dictionary[child.Name.ToString()] = child.Value.Trim();
                        }
                        expandoFromXml.Add(expandoObject);
                    }
                }
            }
            Console.WriteLine("{0} objects was found", expandoFromXml.Count);
            return expandoFromXml.ToArray();
        }
    }
}

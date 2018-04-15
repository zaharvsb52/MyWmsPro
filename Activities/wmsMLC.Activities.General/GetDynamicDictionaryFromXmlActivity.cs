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
    public class GetDynamicDictionaryFromXmlActivity : NativeActivity<IDictionary<string, object>[]>
    {
        private const string InnerXmlPropertyName = "INNERXML";
        [Required]
        public InArgument<string> InputXml { get; set; }

        [Required]
        public InArgument<string> Descendant { get; set; }

        public GetDynamicDictionaryFromXmlActivity()
        {
            DisplayName = GetType().Name.Replace("Activity", string.Empty);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var result = GetExpandoFromXml(InputXml.Get(context), Descendant.Get(context));// as IDictionary<string, object>[];
            context.SetValue(Result, result);
        }

        public IDictionary<string, object>[] GetExpandoFromXml(string xml, string descendantid)
        {
            if (xml == null)
                throw new NullReferenceException(string.Format("Пустой Xml: запрос параметра '{0}'", descendantid));
            var dictionaryFromXml = new List<IDictionary<string, object>>();

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
                        // запомним сам xml объекта для получения списков внутри него
                        dictionary[InnerXmlPropertyName] = element;
                        // значение самого элемента, если в нем есть значение
                        dictionary[descendantid] = element.Value;
                        foreach (var child in element.Descendants())
                        {
                            if (child.Name.Namespace == string.Empty)
                                dictionary[child.Name.ToString()] = child.Value.Trim();
                        }
                        dictionaryFromXml.Add(dictionary);
                    }
                }
            }
            Console.WriteLine(@"{0} objects was found", dictionaryFromXml.Count);
            return dictionaryFromXml.ToArray();
        }
    }
}

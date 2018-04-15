using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xaml;
using System.Xaml.Permissions;
using System.Xml;

namespace wmsMLC.DCL.WorkflowDesigner.Compilers
{
    public class XamlActivityInitializer
    {
        public static void Initialize(Activity a)
        {
            Type aType = a.GetType();
            Stream resourceStream = aType.Assembly.GetManifestResourceStream(aType.FullName + ".xaml");
            if (resourceStream == null)
            {
                throw new InvalidOperationException("Resource not found");
            }
            XamlSchemaContext schemaContext = CreateSchemaContext(aType.Assembly);
            InitializeFromStream(a, aType.Assembly, resourceStream, schemaContext);
        }

        public static void InitializeFromStream(Activity a, Assembly localAssembly, Stream xamlStream,
                                                XamlSchemaContext schemaContext)
        {
            XmlReader xmlReader = null;
            XamlReader reader = null;
            XamlObjectWriter objectWriter = null;
            try
            {
                xmlReader = XmlReader.Create(xamlStream);

                XamlXmlReaderSettings readerSettings = new XamlXmlReaderSettings();
                readerSettings.LocalAssembly = localAssembly;
                readerSettings.AllowProtectedMembersOnRoot = true;
                reader = new XamlXmlReader(xmlReader, schemaContext, readerSettings);

                XamlObjectWriterSettings writerSettings = new XamlObjectWriterSettings();
                writerSettings.RootObjectInstance = a;
                writerSettings.AccessLevel = XamlAccessLevel.PrivateAccessTo(a.GetType());
                objectWriter = new XamlObjectWriter(schemaContext, writerSettings);

                XamlServices.Transform(reader, objectWriter);
            }
            finally
            {
                if (xmlReader != null)
                {
                    ((IDisposable) xmlReader).Dispose();
                }
                if (reader != null)
                {
                    ((IDisposable) reader).Dispose();
                }
                if (objectWriter != null)
                {
                    ((IDisposable) objectWriter).Dispose();
                }
            }
        }

        private static Assembly Load(string assemblyNameVal)
        {
            AssemblyName assemblyName = new AssemblyName(assemblyNameVal);
            byte[] publicKeyToken = assemblyName.GetPublicKeyToken();
            try
            {
                return Assembly.Load(assemblyName.FullName);
            }
            catch (Exception)
            {
                AssemblyName shortName = new AssemblyName(assemblyName.Name);
                if (publicKeyToken != null)
                {
                    shortName.SetPublicKeyToken(publicKeyToken);
                }
                return Assembly.Load(shortName);
            }
        }

        private static IList<Assembly> LoadAssemblies()
        {
            List<string> assemblyNames = new List<string>();
            // TODO: вынески в конфигурацию
            assemblyNames.Add("Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            assemblyNames.Add("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            assemblyNames.Add("System.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            assemblyNames.Add("System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            assemblyNames.Add("System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            assemblyNames.Add("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            assemblyNames.Add(
                "System.ServiceModel.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            assemblyNames.Add("System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            assemblyNames.Add("System.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            assemblyNames.Add("System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            assemblyNames.Add("System.Xml.Linq, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

            IList<Assembly> assemblyList = assemblyNames.Select((name) => Load(name)).ToList();
            return assemblyList;
        }

        private static XamlSchemaContext CreateSchemaContext(Assembly localAssembly)
        {
            IList<Assembly> assemblies = LoadAssemblies();
            assemblies.Add(Assembly.GetExecutingAssembly());
            assemblies.Add(localAssembly);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                assemblies.Add(assembly);
            }
            return new XamlSchemaContext(assemblies);
        }
    }
}

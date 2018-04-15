using System;
using System.Activities;
using System.Activities.Validation;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Xaml;

namespace wmsMLC.DCL.WorkflowDesigner.Compilers
{
    public class XamlActivityCompiler
    {
        public string Compile(string xamlPath, string destinationPath, string[] assemblyNames)
        {
            // Part 0: load the XAML
            XamlSchemaContext schemaContext = new XamlSchemaContext();

            object xamlLoadedObject;
            using (var xamlReader = new XamlXmlReader(xamlPath))
            {
                var builderReader = ActivityXamlServices.CreateBuilderReader(xamlReader, schemaContext);
                xamlLoadedObject = XamlServices.Load(builderReader);
                builderReader.Close();
            }
            ActivityBuilder activityBuilder = (ActivityBuilder)xamlLoadedObject;

            Debug.Assert(activityBuilder.Name.Contains("."));

            string ns = activityBuilder.Name.Substring(0,
                activityBuilder.Name.LastIndexOf("."));

            // Set up the assembly builder based on namespace of activity
            
            AssemblyBuilder ab =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    new AssemblyName(ns) { Version = new Version(1, 0, 0, 0) },
                    AssemblyBuilderAccess.RunAndSave);

            AssemblyBuilder abFake =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    new AssemblyName(ns) { Version = new Version(1, 0, 0, 0) },
                    AssemblyBuilderAccess.Run);

            ModuleBuilder mb = ab.DefineDynamicModule(ns + ".dll", ns + ".dll");
            ModuleBuilder mbFake = abFake.DefineDynamicModule(ns + "dll");

            // Part 1: define the class
            TypeBuilder newClass = BuildType(mb, activityBuilder, false);
            Type createdType = newClass.CreateType();

            TypeBuilder newClassFake = BuildType(mbFake, activityBuilder, true);
            Type fakeType = newClassFake.CreateType();

            string magicXaml = GenerateXAML(activityBuilder, fakeType);
            MemoryStream stringStream = new MemoryStream(Encoding.Unicode.GetBytes(magicXaml));

            // Part 2: embed the XAML as a manifest
            string xamlResourceName = createdType.FullName + ".xaml";

            mb.DefineManifestResource(xamlResourceName, stringStream, ResourceAttributes.Public);

            // End: save the generated assembly
            ab.Save(ns + ".dll");            
            File.Copy(ns + ".dll", Path.Combine(destinationPath, ns + ".dll"), true);
            File.Delete(ns + ".dll");
            return ns + ".dll";
        }

        public TypeBuilder BuildType(ModuleBuilder moduleBuilder, ActivityBuilder activityBuilder, bool fakeType)
        {
            Type baseType = typeof(Activity);
            if (fakeType)
            {
                baseType = typeof(object);
            }

            TypeBuilder newType = moduleBuilder.DefineType(activityBuilder.Name, TypeAttributes.Public, baseType);

            // generate Constructor
            // ctor() : base()
            // {
            //     XamlActivityInitializer.Initialize(this);
            // }
            MethodAttributes ctorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            BindingFlags baseCtorSearchFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            ConstructorBuilder ctor = newType.DefineConstructor(ctorAttributes, CallingConventions.Standard, Type.EmptyTypes);
            ConstructorInfo baseCtor = baseType.GetConstructor(baseCtorSearchFlags, null, Type.EmptyTypes, null);

            var ctorILGenerator = ctor.GetILGenerator();
            ctorILGenerator.Emit(OpCodes.Ldarg_0);
            ctorILGenerator.Emit(OpCodes.Call, baseCtor);
            if (fakeType)
            {
                // Hack: don't call initialize, set properties later
            }
            else
            {
                MethodInfo initialize = typeof(XamlActivityInitializer).GetMethod("Initialize", BindingFlags.Static | BindingFlags.Public);
                ctorILGenerator.Emit(OpCodes.Ldarg_0);
                ctorILGenerator.Emit(OpCodes.Call, initialize);
            }
            ctorILGenerator.Emit(OpCodes.Ret);

            // and generate Properties (including InArguments/OutArguments etc.)
            foreach (var prop in activityBuilder.Properties)
            {
                DefineGetSetProperty(newType, prop.Name, prop.Type);
            }

            if (fakeType)
            {
                // Hack: add our own Implementation and Constraints properties, to emulate System.Activity
                DefineGetSetProperty(newType, "Implementation", typeof(Activity));
                DefineLazyGetProperty(newType, "Constraints", typeof(Collection<Constraint>));
            }

            return newType;
        }

        private void DefineLazyGetProperty(TypeBuilder newType, string propertyName, Type propertyType)
        {
            FieldBuilder backingField = newType.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);
            MethodAttributes accessorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName;
            PropertyBuilder constraintsProperty = newType.DefineProperty(propertyName, PropertyAttributes.None, propertyType, null);

            MethodBuilder getter = newType.DefineMethod("get_" + propertyName, accessorAttributes);
            getter.SetReturnType(propertyType);
            getter.SetParameters(Type.EmptyTypes);

            var getterILGenerator = getter.GetILGenerator();
            var doneInit = getterILGenerator.DefineLabel();
            getterILGenerator.Emit(OpCodes.Ldarg_0);
            getterILGenerator.Emit(OpCodes.Ldfld, backingField);
            getterILGenerator.Emit(OpCodes.Brtrue, doneInit);

            var defaultConstructor = propertyType.GetConstructor(Type.EmptyTypes);
            getterILGenerator.Emit(OpCodes.Ldarg_0);
            getterILGenerator.Emit(OpCodes.Newobj, defaultConstructor);
            getterILGenerator.Emit(OpCodes.Stfld, backingField);

            getterILGenerator.MarkLabel(doneInit);
            getterILGenerator.Emit(OpCodes.Ldarg_0);
            getterILGenerator.Emit(OpCodes.Ldfld, backingField);
            getterILGenerator.Emit(OpCodes.Ret);

            constraintsProperty.SetGetMethod(getter);
        }

        private void DefineGetSetProperty(TypeBuilder newType, string propertyName, Type propertyType)
        {
            FieldBuilder backingField = newType.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);
            MethodAttributes accessorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName;
            PropertyBuilder property = newType.DefineProperty(propertyName, PropertyAttributes.None, propertyType, null);

            MethodBuilder getter = newType.DefineMethod("get_" + propertyName, accessorAttributes);
            getter.SetReturnType(propertyType);
            getter.SetParameters(Type.EmptyTypes);

            var getterILGenerator = getter.GetILGenerator();
            getterILGenerator.Emit(OpCodes.Ldarg_0);
            getterILGenerator.Emit(OpCodes.Ldfld, backingField);
            getterILGenerator.Emit(OpCodes.Ret);

            MethodBuilder setter = newType.DefineMethod("set_" + propertyName, accessorAttributes);
            setter.SetReturnType(null);
            setter.SetParameters(new Type[] { propertyType });

            var setterILGenerator = setter.GetILGenerator();
            setterILGenerator.Emit(OpCodes.Ldarg_0);
            setterILGenerator.Emit(OpCodes.Ldarg_1);
            setterILGenerator.Emit(OpCodes.Stfld, backingField);
            setterILGenerator.Emit(OpCodes.Ret);

            property.SetGetMethod(getter);
            property.SetSetMethod(setter);
        }

        public string GenerateXAML(ActivityBuilder builder, Type fakeType)
        {
            object fakeInstance = Activator.CreateInstance(fakeType);

            foreach (var prop in builder.Properties)
            {
                PropertyInfo pi = fakeInstance.GetType().GetProperty(prop.Name);
                pi.SetValue(fakeInstance, prop.Value, null);
            }

            PropertyInfo implementationProperty = fakeType.GetProperty("Implementation");
            implementationProperty.SetValue(fakeInstance, builder.Implementation, null);

            PropertyInfo constraintsProperty = fakeType.GetProperty("Constraints");
            var constraintsCollection = (Collection<Constraint>)constraintsProperty.GetValue(fakeInstance, null);
            foreach (var constraint in builder.Constraints)
            {
                constraintsCollection.Add(constraint);
            }

            XamlObjectReaderSettings readerSettings = new XamlObjectReaderSettings
            {
                AllowProtectedMembersOnRoot = true
            };
            XamlObjectReader reader = new XamlObjectReader(fakeInstance, readerSettings);

            StringBuilder sb = new StringBuilder();
            StringWriter textWriter = new StringWriter(sb);
            XamlXmlWriter writer = new XamlXmlWriter(textWriter, new XamlSchemaContext());

            XamlServices.Transform(reader, writer);
            writer.Flush();
            textWriter.Flush();

            string strXaml = sb.ToString();
            return strXaml;
        }

        //newClass.AddInterfaceImplementation(typeof(ISupportInitialize));

        //// void BeginInit() {}
        //MethodBuilder beginInit = newClass.DefineMethod("BeginInit", MethodAttributes.Public | MethodAttributes.Virtual);
        //var beginILGenerator = beginInit.GetILGenerator();
        //beginILGenerator.Emit(OpCodes.Ret);

        //// void EndInit() {}
        //MethodBuilder endInit = newClass.DefineMethod("EndInit", MethodAttributes.Public | MethodAttributes.Virtual);
        //var endILGenerator = endInit.GetILGenerator();
        //endILGenerator.Emit(OpCodes.Ret);
    }
}

using System;
using System.Activities;
using System.Activities.Presentation;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace wmsMLC.DCL.WorkflowDesigner.ViewModels.ActivityLoader
{
    public class CompiledActivityLoader : IActivityLoader
    {
        public void LoadActivities(bool clear = true)
        {
            //this.LoadActivities(ActivityPaths.FileName, ActivityPaths.DestFileName, clear);
            this.LoadActivities(ActivityPaths.FileName, ActivityPaths.FileName, clear);
        }

        public IDictionary<string, Type> GetActivities()
        {
            //return this.GetAllCompiledActivities(Directory.GetFiles(ActivityPaths.DestFileName, "*.dll"));
            return this.GetAllCompiledActivities(Directory.GetFiles(ActivityPaths.FileName, "*.dll"));
        }

        public IDictionary<string, Type> GetAllCompiledActivities(string[] files)
        {
            var activities = new Dictionary<string, Type>();
            var loSetup = new AppDomainSetup();
            loSetup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            var dom = AppDomain.CreateDomain("CustomActivities", null, loSetup);
            foreach (var item in files)
            {
                //var fileName = Path.GetTempFileName();
                //File.Copy(item, fileName, true);
                //var asm = Assembly.LoadFrom(fileName);                
                var assemblyName = new AssemblyName();
                assemblyName.CodeBase = item;
                //var asm = Assembly.LoadFrom(item);
                var asm = dom.Load(assemblyName);
                var typeQuery = from type in asm.GetTypes()
                                where type.IsPublic &&
                                      !type.IsNested &&
                                      !type.IsAbstract &&
                                      //!type.ContainsGenericParameters &&
                                      (typeof(Activity).IsAssignableFrom(type) || typeof(Activity<>).IsAssignableFrom(type) ||
                                       typeof(IActivityTemplateFactory).IsAssignableFrom(type))
                                orderby type.Name
                                select type;
                foreach (var type in typeQuery)
                {
                    if (!activities.ContainsKey(type.Name))
                        activities.Add(type.Name, type);
                }
            }
            AppDomain.Unload(dom);
            return activities;
        }

        public void LoadActivities(string fileName, string destFileName, bool clear = true)
        {
            if (!Directory.Exists(fileName))
            {
                Directory.CreateDirectory(fileName);
            }

            if (Directory.Exists(destFileName) && clear)
            {
                foreach (var item in Directory.EnumerateFiles(destFileName))
                {                    
                    File.Delete(item);
                }
            }
            else
            {
                Directory.CreateDirectory(destFileName);
            }

            foreach (var item in Directory.GetFiles(fileName, "*.dll"))
            {
                //var copy = Path.GetTempFileName();
                //File.Copy(item, copy, true);
                //var asm = Assembly.LoadFrom(copy);
                //File.Copy(item, Path.Combine(destFileName, asm.GetName().Name + ".dll"), true);
                //File.Copy(item, Path.Combine(destFileName, Path.GetFileName(item)), true);
            }
        }
    }
}

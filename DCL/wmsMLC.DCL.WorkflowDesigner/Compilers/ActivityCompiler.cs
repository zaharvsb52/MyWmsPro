using System.Collections.Generic;

namespace wmsMLC.DCL.WorkflowDesigner.Compilers
{
    public class ActivityCompiler : IActivityCompiler
    {
        private readonly List<string> _assemblies = new List<string>();

        private string _compiledPath = string.Empty;

        public void Compile(string xamlPath, string compiledPath, string[] assemblyNames)
        {
            AddAssemblies(assemblyNames);
            Compile(xamlPath, compiledPath);
        }

        public void Compile(string xamlPath, string compiledPath)
        {
            _compiledPath = compiledPath;
            Compile(xamlPath);
        }

        public void Compile(string xamlPath, string[] assemblyNames)
        {
            AddAssemblies(assemblyNames);
            Compile(xamlPath);
        }

        public void Compile(string xamlPath)
        {
            var compiler = new XamlActivityCompiler();
            compiler.Compile(xamlPath, _compiledPath, _assemblies.ToArray());
        }

        public void AddAssembly(string assembly)
        {
            if (!string.IsNullOrEmpty(assembly))
            {
                _assemblies.Add(assembly);
            }
        }

        public void AddAssemblies(string[] assemblies)
        {
            if (assemblies != null)
            {
                _assemblies.AddRange(assemblies);
            }
        }
    }
}

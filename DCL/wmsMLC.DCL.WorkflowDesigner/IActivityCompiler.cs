namespace wmsMLC.DCL.WorkflowDesigner
{
    public interface IActivityCompiler
    {
        void Compile(string xamlPath, string compiledPath, string[] assemblyNames);
        void Compile(string xamlPath, string compiledPath);
        void Compile(string xamlPath, string[] assemblyNames);
        void Compile(string xamlPath);
        void AddAssembly(string assembly);
        void AddAssemblies(string[] assemblies);
    }
}

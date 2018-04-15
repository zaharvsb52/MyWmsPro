namespace wmsMLC.General.Modules
{
    /// <summary>
    /// Defines the contract for the modules deployed in the application.
    /// </summary>
    public interface IRunableModule
    {
        /// <summary>
        /// Notifies the module that it has be initialized.
        /// </summary>
        void Initialize();

        void Run();
    }
}

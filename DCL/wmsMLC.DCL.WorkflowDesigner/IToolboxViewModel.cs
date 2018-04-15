namespace wmsMLC.DCL.WorkflowDesigner
{
    public interface IToolboxViewModel
    {
        void ReloadToolboxIcons(IToolboxCreatorService toolboxService);
        void ReloadToolboxIcons();
        void UnloadToolboxIcons();
    }
}

namespace wmsMLC.DCL.Main.Helpers
{
    public class TreeItem
    {
        public TreeItem(int id, int parent, string process, string action, string image)
        {
            Id = id;
            ParentId = parent;
            ProcessName = process;
            ActionName = action;
            ImageName = image;
        }

        public int Id { get; set; }
        public int ParentId { get; set; }
        public string ProcessName { get; set; }
        public string ActionName { get; set; }
        public string ImageName { get; set; }
    }
}
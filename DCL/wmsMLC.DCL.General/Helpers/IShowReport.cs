using System.IO;

namespace wmsMLC.DCL.General.Helpers
{
    public interface IShowReport
    {
        void ShowReport(string fileName);
        void ShowReport(Stream stream);

    }
}
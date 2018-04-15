using System;

namespace wmsMLC.EPS.wmsEPS.ExportFormat
{
    public class ExportFormatFactory
    {
        public IExportFormat GetExportFormat(string name)
        {
            switch (name)
            {
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FRBmp":
                    return new FRBmp();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FRCsv":
                    return new FRCsv();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FRDbf":
                    return new FRDbf();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FRExcel2007":
                    return new FRExcel2007();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FRFpx":
                    return new FRFpx();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FRGif":
                    return new FRGif();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FRHtml":
                    return new FRHtml();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FRJpeg":
                    return new FRJpeg();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FRMetafile":
                    return new FRMetafile();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FRMht":
                    return new FRMht();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FRMhtml":
                    return new FRMhtml();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FROdf":
                    return new FROdf();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FROds":
                    return new FROds();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FROdt":
                    return new FROdt();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FROoBase":
                    return new FROoBase();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FRPdf":
                    return new FRPdf();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FRPowerPoint2007":
                    return new FRPowerPoint2007();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FRRtf":
                    return new FRRtf();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FRText":
                    return new FRText();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FRTiff":
                    return new FRTiff();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FRPng":
                    return new FRPng();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FRWord2007":
                    return new FRWord2007();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FRXml":
                    return new FRXml();
                case "wmsMLC.EPS.wmsEPS.ExportTypes.FRXps":
                    return new FRXps();
                default:
                    throw new ArgumentOutOfRangeException("name");
            }
        }
    }
}
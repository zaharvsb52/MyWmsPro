using System.Text;

namespace wmsMLC.EPS.wmsEPS
{
    public class ExportType
    {
        /// <summary>
        /// Формат файла экспорта.
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Кодировка отчета на выходе, если поддерживается выходным форматом.
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Вырезать из отчета строку "$endline$". Для сохранения пробелов, которые FR убивает.
        /// </summary>
        public bool Spacelife { get; set; }

        public string GetKey()
        {
            return string.Format("{0}-{1}-{2}", Format, Encoding.EncodingName, Spacelife);
        }
    }
}
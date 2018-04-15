using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mime;
using System.Net.Mail;
using System.Web;
using System.IO;
using wmsMLC.General.Resources;

namespace wmsMLC.General.Mail
{
    public class AttachmentHelper
    {
        public static Attachment CreateAttachment(string attachmentFile, string displayName, TransferEncoding transferEncoding)
        {
            var attachment = new Attachment(attachmentFile) {TransferEncoding = transferEncoding};

            string tranferEncodingMarker;
            string encodingMarker;
            int maxChunkLength;

            switch (transferEncoding)
            {
                case TransferEncoding.Base64:
                    tranferEncodingMarker = "B";
                    encodingMarker = "UTF-8";
                    maxChunkLength = 30;
                    break;
                case TransferEncoding.QuotedPrintable:
                    tranferEncodingMarker = "Q";
                    encodingMarker = "ISO-8859-1";
                    maxChunkLength = 76;
                    break;
                default:
                    throw (new ArgumentException(String.Format(ExceptionResources.TransferEncodingNotSupported, transferEncoding)));
            }

            attachment.NameEncoding = Encoding.GetEncoding(encodingMarker);

            string encodingtoken = String.Format("=?{0}?{1}?", encodingMarker, tranferEncodingMarker);
            const string softbreak = "?=";
// ReSharper disable RedundantAssignment
            string encodedAttachmentName = encodingtoken;
// ReSharper restore RedundantAssignment

            var urlEncode = HttpUtility.UrlEncode(displayName, Encoding.Default);
            if (urlEncode != null)
                encodedAttachmentName = attachment.TransferEncoding == TransferEncoding.QuotedPrintable ? urlEncode.Replace("+", " ").Replace("%", "=") : Convert.ToBase64String(Encoding.UTF8.GetBytes(displayName));

            encodedAttachmentName = SplitEncodedAttachmentName(encodingtoken, softbreak, maxChunkLength, encodedAttachmentName);
            attachment.Name = encodedAttachmentName;

            return attachment;
        }

        public static Attachment CreateAttachment(Stream attachmentStream, string displayName, TransferEncoding transferEncoding)
        {
            var attachment = new Attachment(attachmentStream, displayName) {TransferEncoding = transferEncoding};

            string tranferEncodingMarker;
            string encodingMarker;
            int maxChunkLength;

            switch (transferEncoding)
            {
                case TransferEncoding.Base64:
                    tranferEncodingMarker = "B";
                    encodingMarker = "UTF-8";
                    maxChunkLength = 30;
                    break;
                case TransferEncoding.QuotedPrintable:
                    tranferEncodingMarker = "Q";
                    encodingMarker = "ISO-8859-1";
                    maxChunkLength = 76;
                    break;
                default:
                    throw (new ArgumentException(String.Format(ExceptionResources.TransferEncodingNotSupported, transferEncoding)));
            }

            attachment.NameEncoding = Encoding.GetEncoding(encodingMarker);

            var encodingtoken = String.Format("=?{0}?{1}?", encodingMarker, tranferEncodingMarker);
            const string softbreak = "?=";
// ReSharper disable RedundantAssignment
            var encodedAttachmentName = encodingtoken;
// ReSharper restore RedundantAssignment

            var urlEncode = HttpUtility.UrlEncode(displayName, Encoding.Default);
            if (urlEncode != null)
                encodedAttachmentName = attachment.TransferEncoding == TransferEncoding.QuotedPrintable ? urlEncode.Replace("+", " ").Replace("%", "=") : Convert.ToBase64String(Encoding.UTF8.GetBytes(displayName));

            encodedAttachmentName = SplitEncodedAttachmentName(encodingtoken, softbreak, maxChunkLength, encodedAttachmentName);
            attachment.Name = encodedAttachmentName;

            return attachment;
        }

        private static string SplitEncodedAttachmentName(string encodingtoken, string softbreak, int maxChunkLength, string encoded)
        {
            var splitLength = maxChunkLength - encodingtoken.Length - (softbreak.Length * 2);
            var parts = SplitByLength(encoded, splitLength);

            var encodedAttachmentName = parts.Aggregate(encodingtoken, (current, part) => current + (part + softbreak + encodingtoken));

            encodedAttachmentName = encodedAttachmentName.Remove(encodedAttachmentName.Length - encodingtoken.Length, encodingtoken.Length);
            return encodedAttachmentName;
        }

        private static IEnumerable<string> SplitByLength(string stringToSplit, int length)
        {
            while (stringToSplit.Length > length)
            {
                yield return stringToSplit.Substring(0, length);
                stringToSplit = stringToSplit.Substring(length);
            }

            if (stringToSplit.Length > 0) yield return stringToSplit;
        }
    }
}

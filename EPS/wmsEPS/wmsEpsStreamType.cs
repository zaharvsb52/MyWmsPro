/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsEPSTypeStream.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Olga Marysheva</Author>
/// <Date>26.09.2012 13:46:53</Date>
/// <Summary>Класс - имя типа и поток данных</Summary>
/// --------------------------------------------------------------------------------------

using System;
using System.Text;
using System.IO;

namespace wmsMLC.EPS.wmsEPS
{
    /// <summary>
    /// Класс, для передачи данных (имя типа и поток данных) для экпорт отчета FastReport
    /// </summary>
    public class wmsEPSStreamType : IDisposable
    {
        /// <summary>
        /// имя типа
        /// </summary>
        private string _typeName;
        public string TypeName
        {
            get
            {
                return _typeName;
            }
            set
            {
                _typeName = value;
            }
        }

        /// <summary>
        /// поток данных
        /// </summary>
        //private MemoryStream _stream;
        private byte[] _bytes;
        public byte[] Bytes
        {
            get
            {
                return _bytes;
            }
            set
            {
                _bytes = value;
            }
        }

        /// <summary>
        /// Данных для экпорт отчета FastReport
        /// </summary>
        /// <param name="nameType">название типа</param>
        /// <param name="stream">поток данных</param>
        /// <param name="encoding">кодировка</param>
        public wmsEPSStreamType(string nameType, MemoryStream stream, Encoding encoding)
        {
            TypeName = nameType;
            if (stream == null)
            {
                Bytes = null;
            }
            else
            {
                if (encoding != null)
                {
                    Bytes = Encoding.Convert(Encoding.Default, Encoding.UTF8, stream.ToArray());
                }
                else
                {
                    Bytes = stream.ToArray();
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            GC.Collect();
        }
    }
}

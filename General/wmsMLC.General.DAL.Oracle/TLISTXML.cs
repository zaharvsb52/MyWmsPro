using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using Oracle.DataAccess.Types;
using Oracle.DataAccess.Client;

namespace wmsMLC.General.DAL.Oracle
{
    public class TLISTXML : INullable, IOracleCustomType, IXmlSerializable, IConvertible
    {
        private bool m_IsNull;

        private XmlDocument[] m_TLISTXML;

        private OracleUdtStatus[] m_statusArray;

        public TLISTXML()
        {
            // TODO : Add code to initialise the object
        }

        public TLISTXML(string str)
        {
            // TODO : Add code to initialise the object based on the given string 
        }

        public TLISTXML(XmlDocument[] items)
        {
            m_TLISTXML = items;
            m_statusArray = Enumerable.Repeat(OracleUdtStatus.NotNull, items.Length).ToArray();
        }

        public virtual bool IsNull
        {
            get
            {
                return this.m_IsNull;
            }
        }

        public static TLISTXML Null
        {
            get
            {
                TLISTXML obj = new TLISTXML();
                obj.m_IsNull = true;
                return obj;
            }
        }

        [OracleArrayMappingAttribute()]
        public virtual XmlDocument[] Value
        {
            get
            {
                return this.m_TLISTXML;
            }
            set
            {
                this.m_TLISTXML = value;
            }
        }

        public virtual OracleUdtStatus[] StatusArray
        {
            get
            {
                return this.m_statusArray;
            }
            set
            {
                this.m_statusArray = value;
            }
        }

        public virtual void FromCustomObject(OracleConnection con, IntPtr pUdt)
        {
            object objectStatusArray = m_statusArray;
            // Убираю PLINQ, т.к. возможно "из-за него" вылезают ошибки Access Violation
            //var oraXml = m_TLISTXML.AsParallel().Select(i => new OracleXmlType(con, i)).ToArray();
            var oraXml = m_TLISTXML.Select(i => new OracleXmlType(con, i)).ToArray();
            OracleUdt.SetValue(con, pUdt, 0, oraXml, objectStatusArray);
            foreach (var ora in oraXml)
                ora.Dispose();
        }

        public virtual void ToCustomObject(OracleConnection con, IntPtr pUdt)
        {
            object objectStatusArray;
            var oraXml = ((OracleXmlType[])(OracleUdt.GetValue(con, pUdt, 0, out objectStatusArray)));
            // Убираю PLINQ, т.к. возможно "из-за него" вылезают ошибки Access Violation
            //this.m_TLISTXML = oraXml.AsParallel().Select(i => i.GetXmlDocument()).ToArray();
            this.m_TLISTXML = oraXml.Select(i => i.GetXmlDocument()).ToArray();
            this.m_statusArray = ((OracleUdtStatus[])(objectStatusArray));
            foreach (var ora in oraXml)
                ora.Dispose();
        }

        public virtual void ReadXml(XmlReader reader)
        {
            // TODO : Read Serialized Xml Data
            throw new NotImplementedException();
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            // TODO : Serialize object to xml data
            throw new NotImplementedException();
        }

        public virtual XmlSchema GetSchema()
        {
            // TODO : Implement GetSchema
            return null;
        }

        public override string ToString()
        {
            // TODO : Return a string that represents the current object
            return string.Format("TLISTXML (Length: {0})", m_TLISTXML.Length);
        }

        public static TLISTXML Parse(string str)
        {
            // TODO : Add code needed to parse the string and get the object represented by the string
            return new TLISTXML();
        }

        #region .  IConvertible  .
        TypeCode IConvertible.GetTypeCode()
        {
            return TypeCode.Object;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(XmlDocument[]))
            {
                if (m_TLISTXML == null)
                    return null;
                return Value;
            }
            //else if (conversionType == typeof(OracleXmlType[]))
            //{
            //    return Value;
            //}
            //else if (conversionType == typeof(string[]))
            //{
            //    return Value;
            //}

            throw new NotImplementedException();
        }
        #endregion
    }

    // Factory to create an object for the above class
    [OracleCustomTypeMappingAttribute("DEV_MP_WT.TLISTXML")]
    public class TLISTXMLFactory : IOracleCustomTypeFactory, IOracleArrayTypeFactory
    {
        public virtual IOracleCustomType CreateObject()
        {
            return new TLISTXML();
        }

        public virtual System.Array CreateArray(int length)
        {
            OracleXmlType[] collElem = new OracleXmlType[length];
            return collElem;
        }

        public virtual System.Array CreateStatusArray(int length)
        {
            OracleUdtStatus[] udtStatus = new OracleUdtStatus[length];
            return udtStatus;
        }
    }
}

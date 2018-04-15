using System;
using System.Xml.Schema;
using System.Xml.Serialization;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace wmsMLC.General.DAL.Oracle
{
    public class TLISTSTRINGS : INullable, IOracleCustomType, IXmlSerializable
    {
        #region .  Fields  .

        public const string Name = "TLISTSTRINGS";

        private bool m_IsNull;

        private string[] m_TCODESTRINGFIELD;

        private OracleUdtStatus[] m_statusArray;
        #endregion

        public TLISTSTRINGS()
        {
            // TODO : Add code to initialise the object
        }

        public TLISTSTRINGS(string str)
        {

        }

        public TLISTSTRINGS(string[] items)
        {
            m_TCODESTRINGFIELD = items;
        }

        public virtual bool IsNull
        {
            get
            {
                return this.m_IsNull;
            }
        }

        public static TLISTSTRINGS Null
        {
            get
            {
                TLISTSTRINGS obj = new TLISTSTRINGS();
                obj.m_IsNull = true;
                return obj;
            }
        }

        [OracleArrayMapping()]
        public virtual string[] Value
        {
            get
            {
                return this.m_TCODESTRINGFIELD;
            }
            set
            {
                this.m_TCODESTRINGFIELD = value;
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

        public virtual void FromCustomObject(OracleConnection con, System.IntPtr pUdt)
        {
            object objectStatusArray = ((object)(m_statusArray));
            OracleUdt.SetValue(con, pUdt, 0, this.m_TCODESTRINGFIELD, objectStatusArray);
        }

        public virtual void ToCustomObject(OracleConnection con, System.IntPtr pUdt)
        {
            object objectStatusArray = null;
            this.m_TCODESTRINGFIELD = ((string[])(OracleUdt.GetValue(con, pUdt, 0, out objectStatusArray)));
            this.m_statusArray = ((OracleUdtStatus[])(objectStatusArray));
        }

        public virtual void ReadXml(System.Xml.XmlReader reader)
        {
            // TODO : Read Serialized Xml Data
        }

        public virtual void WriteXml(System.Xml.XmlWriter writer)
        {
            // TODO : Serialize object to xml data
        }

        public virtual XmlSchema GetSchema()
        {
            // TODO : Implement GetSchema
            return null;
        }

        public override string ToString()
        {
            // TODO : Return a string that represents the current object
            return "";
        }

        public static TLISTSTRINGS Parse(string str)
        {
            // TODO : Add code needed to parse the string and get the object represented by the string
            return new TLISTSTRINGS();
        }
    }

    // Factory to create an object for the above class
    [OracleCustomTypeMappingAttribute("DEV_MP_WT.TLISTSTRINGS")]
    public class TLISTSTRINGSFactory : IOracleCustomTypeFactory, IOracleArrayTypeFactory
    {
        public virtual IOracleCustomType CreateObject()
        {
            TLISTSTRINGS obj = new TLISTSTRINGS();
            return obj;
        }

        public virtual System.Array CreateArray(int length)
        {
            String[] collElem = new String[length];
            return collElem;
        }

        public virtual System.Array CreateStatusArray(int length)
        {
            OracleUdtStatus[] udtStatus = new OracleUdtStatus[length];
            return udtStatus;
        }
    }
}
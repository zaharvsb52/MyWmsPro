using System;
using System.Xml.Schema;
using System.Xml.Serialization;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace wmsMLC.General.DAL.Oracle
{
    public class TLISTINT : INullable, IOracleCustomType, IXmlSerializable
    {
        #region .  Fields  .

        public const string Name = "TLISTINT";

        private bool m_IsNull;

        private decimal[] m_TCODEINTFIELD;

        private OracleUdtStatus[] m_statusArray;
        #endregion

        public TLISTINT()
        {
            // TODO : Add code to initialise the object
        }

        public TLISTINT(string str)
        {

        }

        public TLISTINT(decimal[] items)
        {
            m_TCODEINTFIELD = items;
        }

        public virtual bool IsNull
        {
            get
            {
                return this.m_IsNull;
            }
        }

        public static TLISTINT Null
        {
            get
            {
                TLISTINT obj = new TLISTINT();
                obj.m_IsNull = true;
                return obj;
            }
        }

        [OracleArrayMapping()]
        public virtual decimal[] Value
        {
            get
            {
                return this.m_TCODEINTFIELD;
            }
            set
            {
                this.m_TCODEINTFIELD = value;
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
            OracleUdt.SetValue(con, pUdt, 0, this.m_TCODEINTFIELD, objectStatusArray);
        }

        public virtual void ToCustomObject(OracleConnection con, System.IntPtr pUdt)
        {
            object objectStatusArray = null;
            this.m_TCODEINTFIELD = ((decimal[])(OracleUdt.GetValue(con, pUdt, 0, out objectStatusArray)));
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

        public static TLISTINT Parse(string str)
        {
            // TODO : Add code needed to parse the string and get the object represented by the string
            return new TLISTINT();
        }
    }

    // Factory to create an object for the above class
    [OracleCustomTypeMappingAttribute("DEV_MP_WT.TLISTINT")]
    public class TLISTINTFactory : IOracleCustomTypeFactory, IOracleArrayTypeFactory
    {
        public virtual IOracleCustomType CreateObject()
        {
            TLISTINT obj = new TLISTINT();
            return obj;
        }

        public virtual System.Array CreateArray(int length)
        {
            Decimal[] collElem = new Decimal[length];
            return collElem;
        }

        public virtual System.Array CreateStatusArray(int length)
        {
            OracleUdtStatus[] udtStatus = new OracleUdtStatus[length];
            return udtStatus;
        }
    }
}
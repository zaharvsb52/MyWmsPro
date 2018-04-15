using System;
using System.Data;
using System.Data.Common;
using System.Xml;
using BLToolkit.Common;
using BLToolkit.Data.DataProvider;
using BLToolkit.Mapping;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace wmsMLC.General.DAL.Oracle
{
    public class ExtendedOdpDataProvider : OdpDataProvider
    {
        public ExtendedOdpDataProvider()
        {
            MappingSchema = new ExtendedOdpMappingSchema();
        }

        public override IDbConnection CreateConnectionObject()
        {
            var res = base.CreateConnectionObject();
            //            res.StateChange += OnConnectionStateChanged;
            //            res.Disposed += OnConnectionDisposed;
            return res;
        }

        //        private void OnConnectionDisposed(object sender, EventArgs e)
        //        {
        //            var con = (OracleConnection) sender;
        //            con.Disposed -= OnConnectionDisposed;
        //            con.StateChange -= OnConnectionStateChanged;
        //        }

        //        private object _connectionPool;

        //        private void OnConnectionStateChanged(object sender, StateChangeEventArgs e)
        //        {
        //            if (e.OriginalState == ConnectionState.Closed && e.CurrentState == ConnectionState.Open
        //                && _connectionPool == null)
        //            {
        //                var con = (OracleConnection) sender;
        //                //m_opoConCtx.m_udtDescPoolerByName
        //                var field = typeof(OracleConnection).GetField("m_opoConCtx", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        //                if (field == null)
        //                    return;

        //                var m_opoConCtx = field.GetValue(con);
        //                if (m_opoConCtx == null)
        //                    return;

        //                var pool = m_opoConCtx.GetType().GetField("pool", BindingFlags.Public | BindingFlags.Instance);
        //                if (pool != null)
        //                {
        //                    _connectionPool = pool.GetValue(m_opoConCtx);
        ////                    if (val != null)
        ////                        val.GetType().InvokeMember("Clear", BindingFlags.Public | BindingFlags.Instance, null, val, null);
        //                }
        //            }

        //            if (e.OriginalState == ConnectionState.Open && e.CurrentState == ConnectionState.Closed)
        //            {

        //            }
        //        }

        public override IDataReader GetDataReader(MappingSchema schema, IDataReader dataReader)
        {
            return dataReader is OracleDataReader ?
                new OracleDataReaderEx((OracleDataReader)dataReader) :
                base.GetDataReader(schema, dataReader);
        }

        public class ExtendedOdpMappingSchema : OdpMappingSchema
        {
            public override DataReaderMapper CreateDataReaderMapper(IDataReader dataReader)
            {
                return new ExOracleDataReaderMapper(this, dataReader);
            }

            public override DataReaderMapper CreateDataReaderMapper(
                IDataReader dataReader,
                NameOrIndexParameter nip)
            {
                return new ExOracleScalarDataReaderMapper(this, dataReader, nip);
            }

            public override Guid ConvertToGuid(object value)
            {
                if (value is byte[])
                {
                    var guid = base.ConvertToGuid(value);
                    return guid.FlipEndian();
                }

                return base.ConvertToGuid(value);
            }

            public override Guid? ConvertToNullableGuid(object value)
            {
                if (value is byte[])
                {
                    var guid = base.ConvertToGuid(value);
                    return guid.FlipEndian();
                }

                return base.ConvertToNullableGuid(value);
            }
        }

        class OracleDataReaderEx : DataReaderEx<OracleDataReader>, IOracleDataReader
        {
            public OracleDataReaderEx(OracleDataReader rd) : base(rd)
            {
            }

            public override DateTimeOffset GetDateTimeOffset(int i)
            {
                var ts = DataReader.GetOracleTimeStampTZ(i);
                return new DateTimeOffset(ts.Value, ts.GetTimeZoneOffset());
            }

            public long FetchSize { get { return DataReader.FetchSize; } set { DataReader.FetchSize = value; } }
            public bool HasRows { get { return DataReader.HasRows; } }
            public int HiddenFieldCount { get { return DataReader.HiddenFieldCount; } }
            public int InitialLOBFetchSize { get { return DataReader.InitialLOBFetchSize; } }
            public int InitialLONGFetchSize { get { return DataReader.InitialLONGFetchSize; } }
            public long RowSize { get { return DataReader.RowSize; } }
            public int VisibleFieldCount { get { return DataReader.VisibleFieldCount; } }

            DbDataReader IOracleDataReader.GetData(int i)
            {
                return (DbDataReader)base.GetData(i);
            }

            public OracleBFile GetOracleBFile(int i)
            {
                return DataReader.GetOracleBFile(i);
            }

            public OracleBinary GetOracleBinary(int i)
            {
                return DataReader.GetOracleBinary(i);
            }

            public OracleBlob GetOracleBlob(int i)
            {
                return DataReader.GetOracleBlob(i);
            }

            public OracleBlob GetOracleBlobForUpdate(int i)
            {
                return DataReader.GetOracleBlobForUpdate(i);
            }

            public OracleBlob GetOracleBlobForUpdate(int i, int wait)
            {
                return DataReader.GetOracleBlobForUpdate(i, wait);
            }

            public OracleClob GetOracleClob(int i)
            {
                return DataReader.GetOracleClob(i);
            }

            public OracleClob GetOracleClobForUpdate(int i)
            {
                return DataReader.GetOracleClobForUpdate(i);
            }

            public OracleClob GetOracleClobForUpdate(int i, int wait)
            {
                return DataReader.GetOracleClobForUpdate(i, wait);
            }

            public OracleDate GetOracleDate(int i)
            {
                return DataReader.GetOracleDate(i);
            }

            public OracleDecimal GetOracleDecimal(int i)
            {
                return DataReader.GetOracleDecimal(i);
            }

            public OracleIntervalDS GetOracleIntervalDS(int i)
            {
                return DataReader.GetOracleIntervalDS(i);
            }

            public OracleIntervalYM GetOracleIntervalYM(int i)
            {
                return DataReader.GetOracleIntervalYM(i);
            }

            public OracleRef GetOracleRef(int i)
            {
                return DataReader.GetOracleRef(i);
            }

            public OracleString GetOracleString(int i)
            {
                return DataReader.GetOracleString(i);
            }

            public OracleTimeStamp GetOracleTimeStamp(int i)
            {
                return DataReader.GetOracleTimeStamp(i);
            }

            public OracleTimeStampLTZ GetOracleTimeStampLTZ(int i)
            {
                return DataReader.GetOracleTimeStampLTZ(i);
            }

            public OracleTimeStampTZ GetOracleTimeStampTZ(int i)
            {
                return DataReader.GetOracleTimeStampTZ(i);
            }

            public object GetOracleValue(int i)
            {
                return DataReader.GetOracleDecimal(i);
            }

            public int GetOracleValues(object[] values)
            {
                return DataReader.GetOracleValues(values);
            }

            public OracleXmlType GetOracleXmlType(int i)
            {
                return DataReader.GetOracleXmlType(i);
            }

            public Type GetProviderSpecificFieldType(int ordinal)
            {
                return DataReader.GetProviderSpecificFieldType(ordinal);
            }

            public object GetProviderSpecificValue(int ordinal)
            {
                return DataReader.GetProviderSpecificValue(ordinal);
            }

            public int GetProviderSpecificValues(object[] values)
            {
                return DataReader.GetProviderSpecificValues(values);
            }

            public TimeSpan GetTimeSpan(int i)
            {
                return DataReader.GetTimeSpan(i);
            }

            public XmlReader GetXmlReader(int i)
            {
                return DataReader.GetXmlReader(i);
            }
        }

        public class ExOracleDataReaderMapper : DataReaderMapper
        {
            public ExOracleDataReaderMapper(MappingSchema mappingSchema, IDataReader dataReader)
                : base(mappingSchema, dataReader)
            {
                _dataReader = dataReader is OracleDataReaderEx ?
                    ((OracleDataReaderEx)dataReader).DataReader :
                    (OracleDataReader)dataReader;
            }

            private readonly OracleDataReader _dataReader;

            public override Type GetFieldType(int index)
            {
                var fieldType = _dataReader.GetProviderSpecificFieldType(index);

                if (fieldType != typeof(OracleBlob)
#if !MANAGED
 && fieldType != typeof(OracleXmlType)
#endif
)
                    fieldType = _dataReader.GetFieldType(index);

                return fieldType;
            }

            public override object GetValue(object o, int index)
            {
                var fieldType = _dataReader.GetProviderSpecificFieldType(index);

#if !MANAGED
                if (fieldType == typeof(OracleXmlType))
                {
                    var xml = _dataReader.GetOracleXmlType(index);
                    return MappingSchema.ConvertToXmlDocument(xml);
                }
#endif
                if (fieldType == typeof(OracleBlob))
                {
                    var blob = _dataReader.GetOracleBlob(index);
                    return MappingSchema.ConvertToStream(blob);
                }

                return _dataReader.IsDBNull(index) ? null :
                    _dataReader.GetValue(index);
            }

            public override Boolean GetBoolean(object o, int index) { return MappingSchema.ConvertToBoolean(GetValue(o, index)); }
            public override Char GetChar(object o, int index) { return MappingSchema.ConvertToChar(GetValue(o, index)); }
            public override Guid GetGuid(object o, int index) { return MappingSchema.ConvertToGuid(GetValue(o, index)); }

            [CLSCompliant(false)]
            public override SByte GetSByte(object o, int index) { return (SByte)_dataReader.GetDecimal(index); }
            [CLSCompliant(false)]
            public override UInt16 GetUInt16(object o, int index) { return (UInt16)_dataReader.GetDecimal(index); }
            [CLSCompliant(false)]
            public override UInt32 GetUInt32(object o, int index) { return (UInt32)_dataReader.GetDecimal(index); }
            [CLSCompliant(false)]
            public override UInt64 GetUInt64(object o, int index) { return (UInt64)_dataReader.GetDecimal(index); }

            public override Decimal GetDecimal(object o, int index) { return OracleDecimal.SetPrecision(_dataReader.GetOracleDecimal(index), 28).Value; }

            public override Boolean? GetNullableBoolean(object o, int index) { return MappingSchema.ConvertToNullableBoolean(GetValue(o, index)); }
            public override Char? GetNullableChar(object o, int index) { return MappingSchema.ConvertToNullableChar(GetValue(o, index)); }
            public override Guid? GetNullableGuid(object o, int index) { return MappingSchema.ConvertToNullableGuid(GetValue(o, index)); }

            [CLSCompliant(false)]
            public override SByte? GetNullableSByte(object o, int index) { return _dataReader.IsDBNull(index) ? null : (SByte?)_dataReader.GetDecimal(index); }
            [CLSCompliant(false)]
            public override UInt16? GetNullableUInt16(object o, int index) { return _dataReader.IsDBNull(index) ? null : (UInt16?)_dataReader.GetDecimal(index); }
            [CLSCompliant(false)]
            public override UInt32? GetNullableUInt32(object o, int index) { return _dataReader.IsDBNull(index) ? null : (UInt32?)_dataReader.GetDecimal(index); }
            [CLSCompliant(false)]
            public override UInt64? GetNullableUInt64(object o, int index) { return _dataReader.IsDBNull(index) ? null : (UInt64?)_dataReader.GetDecimal(index); }

            public override Decimal? GetNullableDecimal(object o, int index) { return _dataReader.IsDBNull(index) ? (decimal?)null : OracleDecimal.SetPrecision(_dataReader.GetOracleDecimal(index), 28).Value; }
        }

        public class ExOracleScalarDataReaderMapper : ScalarDataReaderMapper
        {
            private readonly OracleDataReader _dataReader;

            public ExOracleScalarDataReaderMapper(
                MappingSchema mappingSchema,
                IDataReader dataReader,
                NameOrIndexParameter nameOrIndex)
                : base(mappingSchema, dataReader, nameOrIndex)
            {
                _dataReader = dataReader is OracleDataReaderEx ?
                    ((OracleDataReaderEx)dataReader).DataReader :
                    (OracleDataReader)dataReader;

                _fieldType = _dataReader.GetProviderSpecificFieldType(Index);

                if (_fieldType != typeof(OracleBlob)
#if !MANAGED
 && _fieldType != typeof(OracleXmlType)
#endif
)
                    _fieldType = _dataReader.GetFieldType(Index);
            }

            private readonly Type _fieldType;

            public override Type GetFieldType(int index)
            {
                return _fieldType;
            }

            public override object GetValue(object o, int index)
            {
#if !MANAGED
                if (_fieldType == typeof(OracleXmlType))
                {
                    var xml = _dataReader.GetOracleXmlType(Index);
                    return MappingSchema.ConvertToXmlDocument(xml);
                }
#endif
                if (_fieldType == typeof(OracleBlob))
                {
                    var blob = _dataReader.GetOracleBlob(Index);
                    return MappingSchema.ConvertToStream(blob);
                }

                return _dataReader.IsDBNull(index) ? null :
                    _dataReader.GetValue(Index);
            }

            public override Boolean GetBoolean(object o, int index) { return MappingSchema.ConvertToBoolean(GetValue(o, Index)); }
            public override Char GetChar(object o, int index) { return MappingSchema.ConvertToChar(GetValue(o, Index)); }
            public override Guid GetGuid(object o, int index) { return MappingSchema.ConvertToGuid(GetValue(o, Index)); }

            [CLSCompliant(false)]
            public override SByte GetSByte(object o, int index) { return (SByte)_dataReader.GetDecimal(Index); }
            [CLSCompliant(false)]
            public override UInt16 GetUInt16(object o, int index) { return (UInt16)_dataReader.GetDecimal(Index); }
            [CLSCompliant(false)]
            public override UInt32 GetUInt32(object o, int index) { return (UInt32)_dataReader.GetDecimal(Index); }
            [CLSCompliant(false)]
            public override UInt64 GetUInt64(object o, int index) { return (UInt64)_dataReader.GetDecimal(Index); }

            public override Decimal GetDecimal(object o, int index) { return OracleDecimal.SetPrecision(_dataReader.GetOracleDecimal(Index), 28).Value; }

            public override Boolean? GetNullableBoolean(object o, int index) { return MappingSchema.ConvertToNullableBoolean(GetValue(o, Index)); }
            public override Char? GetNullableChar(object o, int index) { return MappingSchema.ConvertToNullableChar(GetValue(o, Index)); }
            public override Guid? GetNullableGuid(object o, int index) { return MappingSchema.ConvertToNullableGuid(GetValue(o, Index)); }

            [CLSCompliant(false)]
            public override SByte? GetNullableSByte(object o, int index) { return _dataReader.IsDBNull(index) ? null : (SByte?)_dataReader.GetDecimal(Index); }
            [CLSCompliant(false)]
            public override UInt16? GetNullableUInt16(object o, int index) { return _dataReader.IsDBNull(index) ? null : (UInt16?)_dataReader.GetDecimal(Index); }
            [CLSCompliant(false)]
            public override UInt32? GetNullableUInt32(object o, int index) { return _dataReader.IsDBNull(index) ? null : (UInt32?)_dataReader.GetDecimal(Index); }
            [CLSCompliant(false)]
            public override UInt64? GetNullableUInt64(object o, int index) { return _dataReader.IsDBNull(index) ? null : (UInt64?)_dataReader.GetDecimal(Index); }

            public override Decimal? GetNullableDecimal(object o, int index) { return _dataReader.IsDBNull(index) ? (decimal?)null : OracleDecimal.SetPrecision(_dataReader.GetOracleDecimal(Index), 28).Value; }
        }
    }


    public interface IOracleDataReader
    {
        int Depth { get; }
        long FetchSize { get; set; }
        int FieldCount { get; }
        bool HasRows { get; }
        int HiddenFieldCount { get; }
        int InitialLOBFetchSize { get; }
        int InitialLONGFetchSize { get; }
        bool IsClosed { get; }
        int RecordsAffected { get; }
        long RowSize { get; }
        int VisibleFieldCount { get; }
        void Close();
        void Dispose();
        bool GetBoolean(int i);
        byte GetByte(int i);
        long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length);
        char GetChar(int i);
        long GetChars(int i, long fieldOffset, char[] buffer, int bufferOffset, int length);
        DbDataReader GetData(int i);
        string GetDataTypeName(int i);
        DateTime GetDateTime(int i);
        decimal GetDecimal(int i);
        double GetDouble(int i);
        Type GetFieldType(int i);
        float GetFloat(int i);
        Guid GetGuid(int i);
        short GetInt16(int i);
        int GetInt32(int i);
        long GetInt64(int i);
        string GetName(int i);
        OracleBFile GetOracleBFile(int i);
        OracleBinary GetOracleBinary(int i);
        OracleBlob GetOracleBlob(int i);
        OracleBlob GetOracleBlobForUpdate(int i);
        OracleBlob GetOracleBlobForUpdate(int i, int wait);
        OracleClob GetOracleClob(int i);
        OracleClob GetOracleClobForUpdate(int i);
        OracleClob GetOracleClobForUpdate(int i, int wait);
        OracleDate GetOracleDate(int i);
        OracleDecimal GetOracleDecimal(int i);
        OracleIntervalDS GetOracleIntervalDS(int i);
        OracleIntervalYM GetOracleIntervalYM(int i);
        OracleRef GetOracleRef(int i);
        OracleString GetOracleString(int i);
        OracleTimeStamp GetOracleTimeStamp(int i);
        OracleTimeStampLTZ GetOracleTimeStampLTZ(int i);
        OracleTimeStampTZ GetOracleTimeStampTZ(int i);
        object GetOracleValue(int i);
        int GetOracleValues(object[] values);
        OracleXmlType GetOracleXmlType(int i);
        int GetOrdinal(string name);
        Type GetProviderSpecificFieldType(int ordinal);
        object GetProviderSpecificValue(int ordinal);
        int GetProviderSpecificValues(object[] values);
        DataTable GetSchemaTable();
        string GetString(int i);
        TimeSpan GetTimeSpan(int i);
        object GetValue(int i);
        int GetValues(object[] values);
        XmlReader GetXmlReader(int i);
        bool IsDBNull(int i);
        bool NextResult();
        bool Read();
    }

}
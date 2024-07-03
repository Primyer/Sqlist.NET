using NpgsqlTypes;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace Sqlist.NET
{
    public partial class NpgsqlTypeMapper : TypeMapper
    {
        private static readonly Dictionary<string, NpgsqlDbType> NameDbTypePairs = new()
        {
            ["bigint"] = NpgsqlDbType.Bigint,
            ["int8"] = NpgsqlDbType.Bigint,
            ["bigserial"] = NpgsqlDbType.Bigint,
            ["serial8"] = NpgsqlDbType.Bigint,
            ["double"] = NpgsqlDbType.Double,
            ["double precision"] = NpgsqlDbType.Double,
            ["float8"] = NpgsqlDbType.Double,
            ["integer"] = NpgsqlDbType.Integer,
            ["int"] = NpgsqlDbType.Integer,
            ["int4"] = NpgsqlDbType.Integer,
            ["serial"] = NpgsqlDbType.Integer,
            ["serial4"] = NpgsqlDbType.Integer,
            ["decimal"] = NpgsqlDbType.Numeric,
            ["numeric"] = NpgsqlDbType.Numeric,
            ["real"] = NpgsqlDbType.Real,
            ["float4"] = NpgsqlDbType.Real,
            ["smallint"] = NpgsqlDbType.Smallint,
            ["int2"] = NpgsqlDbType.Smallint,
            ["smallserial"] = NpgsqlDbType.Smallint,
            ["serial2"] = NpgsqlDbType.Smallint,
            ["money"] = NpgsqlDbType.Money,
            ["bool"] = NpgsqlDbType.Boolean,
            ["boolean"] = NpgsqlDbType.Boolean,
            ["box"] = NpgsqlDbType.Box,
            ["circle"] = NpgsqlDbType.Circle,
            ["line"] = NpgsqlDbType.Line,
            ["lseg"] = NpgsqlDbType.LSeg,
            ["path"] = NpgsqlDbType.Path,
            ["point"] = NpgsqlDbType.Point,
            ["polygon"] = NpgsqlDbType.Polygon,
            ["bpchar"] = NpgsqlDbType.Char,
            ["text"] = NpgsqlDbType.Text,
            ["varchar"] = NpgsqlDbType.Varchar,
            ["character varying"] = NpgsqlDbType.Varchar,
            ["name"] = NpgsqlDbType.Name,
            ["citext"] = NpgsqlDbType.Citext,
            ["char"] = NpgsqlDbType.InternalChar,
            ["charachter"] = NpgsqlDbType.InternalChar,
            ["bytea"] = NpgsqlDbType.Bytea,
            ["date"] = NpgsqlDbType.Date,
            ["time"] = NpgsqlDbType.Time,
            ["time without time zone"] = NpgsqlDbType.Time,
            ["timetz"] = NpgsqlDbType.TimeTz,
            ["time with time zone"] = NpgsqlDbType.TimeTz,
            ["timestamp"] = NpgsqlDbType.Timestamp,
            ["timestamp without time zone"] = NpgsqlDbType.Timestamp,
            ["timestamptz"] = NpgsqlDbType.TimestampTz,
            ["timestamp  with time zone"] = NpgsqlDbType.TimestampTz,
            ["interval"] = NpgsqlDbType.Interval,
            ["inet"] = NpgsqlDbType.Inet,
            ["cidr"] = NpgsqlDbType.Cidr,
            ["macaddr"] = NpgsqlDbType.MacAddr,
            ["macaddr8"] = NpgsqlDbType.MacAddr8,
            ["bit"] = NpgsqlDbType.Bit,
            ["varbit"] = NpgsqlDbType.Varbit,
            ["bit varying"] = NpgsqlDbType.Varbit,
            ["tsvector"] = NpgsqlDbType.TsVector,
            ["tsquery"] = NpgsqlDbType.TsQuery,
            ["regconfig"] = NpgsqlDbType.Regconfig,
            ["uuid"] = NpgsqlDbType.Uuid,
            ["xml"] = NpgsqlDbType.Xml,
            ["json"] = NpgsqlDbType.Json,
            ["jsonb"] = NpgsqlDbType.Jsonb,
            ["jsonpath"] = NpgsqlDbType.JsonPath,
            ["hstore"] = NpgsqlDbType.Hstore,
            ["refcursor"] = NpgsqlDbType.Refcursor,
            ["oidvector"] = NpgsqlDbType.Oidvector,
            ["int2vector"] = NpgsqlDbType.Int2Vector,
            ["oid"] = NpgsqlDbType.Oid,
            ["xid"] = NpgsqlDbType.Xid,
            ["xid8"] = NpgsqlDbType.Xid8,
            ["cid"] = NpgsqlDbType.Cid,
            ["regtype"] = NpgsqlDbType.Regtype,
            ["tid"] = NpgsqlDbType.Tid,
            ["pg_lsn"] = NpgsqlDbType.PgLsn,
            ["unknown"] = NpgsqlDbType.Unknown,
            ["geometry"] = NpgsqlDbType.Geometry,
            ["geodetic"] = NpgsqlDbType.Geography,
            ["ltree"] = NpgsqlDbType.LTree,
            ["lquery"] = NpgsqlDbType.LQuery,
            ["ltxtquery"] = NpgsqlDbType.LTxtQuery,
            ["int4range"] = NpgsqlDbType.IntegerRange,
            ["int8range"] = NpgsqlDbType.BigIntRange,
            ["numrange"] = NpgsqlDbType.NumericRange,
            ["tsrange"] = NpgsqlDbType.TimestampRange,
            ["tstzrange"] = NpgsqlDbType.TimestampTzRange,
            ["daterange"] = NpgsqlDbType.DateRange,
            ["int4multirange"] = NpgsqlDbType.IntegerMultirange,
            ["int8multirange"] = NpgsqlDbType.BigIntMultirange,
            ["nummultirange"] = NpgsqlDbType.NumericMultirange,
            ["tsmultirange"] = NpgsqlDbType.TimestampMultirange,
            ["tstzmultirange"] = NpgsqlDbType.TimestampTzMultirange,
            ["datemultirange"] = NpgsqlDbType.DateMultirange,
            ["array"] = NpgsqlDbType.Array,
            ["range"] = NpgsqlDbType.Range,
            ["multirange"] = NpgsqlDbType.Multirange
        };

        private static readonly Dictionary<NpgsqlDbType, string> TypeNames = NameDbTypePairs
            .GroupBy(d => d.Value)
            .Select(g => g.OrderBy(d => d.Key).First())
            .ToDictionary(d => d.Value, d => d.Key);

        private static readonly Dictionary<NpgsqlDbType, Type> ClrTypes = new()
        {
            [NpgsqlDbType.Bigint] = typeof(long),
            [NpgsqlDbType.Double] = typeof(double),
            [NpgsqlDbType.Integer] = typeof(int),
            [NpgsqlDbType.Numeric] = typeof(decimal),
            [NpgsqlDbType.Real] = typeof(float),
            [NpgsqlDbType.Smallint] = typeof(short),
            [NpgsqlDbType.Money] = typeof(decimal),
            [NpgsqlDbType.Boolean] = typeof(bool),
            [NpgsqlDbType.Box] = typeof(NpgsqlBox),
            [NpgsqlDbType.Circle] = typeof(NpgsqlCircle),
            [NpgsqlDbType.Line] = typeof(NpgsqlLine),
            [NpgsqlDbType.LSeg] = typeof(NpgsqlLSeg),
            [NpgsqlDbType.Path] = typeof(NpgsqlPath),
            [NpgsqlDbType.Point] = typeof(NpgsqlPoint),
            [NpgsqlDbType.Polygon] = typeof(NpgsqlPolygon),
            [NpgsqlDbType.Char] = typeof(string),
            [NpgsqlDbType.Text] = typeof(string),
            [NpgsqlDbType.Varchar] = typeof(string),
            [NpgsqlDbType.Name] = typeof(string),
            [NpgsqlDbType.Citext] = typeof(string),
            [NpgsqlDbType.InternalChar] = typeof(byte),
            [NpgsqlDbType.Bytea] = typeof(byte[]),
            [NpgsqlDbType.Date] = typeof(DateTime),
            [NpgsqlDbType.Time] = typeof(TimeSpan),
            [NpgsqlDbType.TimeTz] = typeof(TimeSpan),
            [NpgsqlDbType.Timestamp] = typeof(DateTime),
            [NpgsqlDbType.TimestampTz] = typeof(DateTimeOffset),
            [NpgsqlDbType.Interval] = typeof(TimeSpan),
            [NpgsqlDbType.Inet] = typeof(IPAddress),
            [NpgsqlDbType.Cidr] = typeof(ValueTuple<IPAddress, int>),
            [NpgsqlDbType.MacAddr] = typeof(PhysicalAddress),
            [NpgsqlDbType.MacAddr8] = typeof(PhysicalAddress),
            [NpgsqlDbType.Bit] = typeof(BitArray),
            [NpgsqlDbType.Varbit] = typeof(BitArray),
            [NpgsqlDbType.TsVector] = typeof(NpgsqlTsVector),
            [NpgsqlDbType.TsQuery] = typeof(NpgsqlTsQuery),
            [NpgsqlDbType.Regconfig] = typeof(string),
            [NpgsqlDbType.Uuid] = typeof(Guid),
            [NpgsqlDbType.Xml] = typeof(string),
            [NpgsqlDbType.Json] = typeof(string),
            [NpgsqlDbType.Jsonb] = typeof(string),
            [NpgsqlDbType.JsonPath] = typeof(string),
            [NpgsqlDbType.Hstore] = typeof(IDictionary<string, string>),
            [NpgsqlDbType.Oidvector] = typeof(uint[]),
            [NpgsqlDbType.Oid] = typeof(uint),
            [NpgsqlDbType.Xid] = typeof(uint),
            [NpgsqlDbType.Xid8] = typeof(uint),
            [NpgsqlDbType.Cid] = typeof(uint),
            [NpgsqlDbType.IntegerRange] = typeof(NpgsqlRange<int>),
            [NpgsqlDbType.BigIntRange] = typeof(NpgsqlRange<long>),
            [NpgsqlDbType.NumericRange] = typeof(NpgsqlRange<decimal>),
            [NpgsqlDbType.TimestampRange] = typeof(NpgsqlRange<DateTime>),
            [NpgsqlDbType.TimestampTzRange] = typeof(NpgsqlRange<DateTimeOffset>),
            [NpgsqlDbType.DateRange] = typeof(NpgsqlRange<DateTime>),
            [NpgsqlDbType.IntegerMultirange] = typeof(NpgsqlRange<int>[]),
            [NpgsqlDbType.BigIntMultirange] = typeof(NpgsqlRange<long>[]),
            [NpgsqlDbType.NumericMultirange] = typeof(NpgsqlRange<decimal>[]),
            [NpgsqlDbType.TimestampMultirange] = typeof(NpgsqlRange<DateTime>[]),
            [NpgsqlDbType.TimestampTzMultirange] = typeof(NpgsqlRange<DateTimeOffset>[]),
            [NpgsqlDbType.DateMultirange] = typeof(NpgsqlRange<DateTime>[]),
            [NpgsqlDbType.Array] = typeof(Array),
            [NpgsqlDbType.Range] = typeof(Range),
            [NpgsqlDbType.Multirange] = typeof(Range[])
        };

        private static readonly Dictionary<Type, NpgsqlDbType> ClrDbTypePairs = new()
        {
            [typeof(long)] = NpgsqlDbType.Bigint,
            [typeof(double)] = NpgsqlDbType.Double,
            [typeof(int)] = NpgsqlDbType.Integer,
            [typeof(decimal)] = NpgsqlDbType.Numeric,
            [typeof(float)] = NpgsqlDbType.Real,
            [typeof(short)] = NpgsqlDbType.Smallint,
            [typeof(bool)] = NpgsqlDbType.Boolean,
            [typeof(NpgsqlBox)] = NpgsqlDbType.Box,
            [typeof(NpgsqlCircle)] = NpgsqlDbType.Circle,
            [typeof(NpgsqlLine)] = NpgsqlDbType.Line,
            [typeof(NpgsqlLSeg)] = NpgsqlDbType.LSeg,
            [typeof(NpgsqlPath)] = NpgsqlDbType.Path,
            [typeof(NpgsqlPoint)] = NpgsqlDbType.Point,
            [typeof(NpgsqlPolygon)] = NpgsqlDbType.Polygon,
            [typeof(string)] = NpgsqlDbType.Text,
            [typeof(byte)] = NpgsqlDbType.InternalChar,
            [typeof(byte[])] = NpgsqlDbType.Bytea,
            [typeof(DateTime)] = NpgsqlDbType.Timestamp,
            [typeof(DateTimeOffset)] = NpgsqlDbType.TimestampTz,
            [typeof(TimeSpan)] = NpgsqlDbType.Interval,
            [typeof(IPAddress)] = NpgsqlDbType.Inet,
            [typeof(ValueTuple<IPAddress, int>)] = NpgsqlDbType.Cidr,
            [typeof(PhysicalAddress)] = NpgsqlDbType.MacAddr,
            [typeof(BitArray)] = NpgsqlDbType.Bit,
            [typeof(NpgsqlTsVector)] = NpgsqlDbType.TsVector,
            [typeof(NpgsqlTsQuery)] = NpgsqlDbType.TsQuery,
            [typeof(Guid)] = NpgsqlDbType.Uuid,
            [typeof(IDictionary<string, string>)] = NpgsqlDbType.Hstore,
            [typeof(uint[])] = NpgsqlDbType.Oidvector,
            [typeof(uint)] = NpgsqlDbType.Oid,
            [typeof(NpgsqlRange<int>)] = NpgsqlDbType.IntegerRange,
            [typeof(NpgsqlRange<long>)] = NpgsqlDbType.BigIntRange,
            [typeof(NpgsqlRange<decimal>)] = NpgsqlDbType.NumericRange,
            [typeof(NpgsqlRange<DateTime>)] = NpgsqlDbType.TimestampRange,
            [typeof(NpgsqlRange<DateTimeOffset>)] = NpgsqlDbType.TimestampTzRange,
            [typeof(NpgsqlRange<DateTime>)] = NpgsqlDbType.DateRange,
            [typeof(NpgsqlRange<int>[])] = NpgsqlDbType.IntegerMultirange,
            [typeof(NpgsqlRange<long>[])] = NpgsqlDbType.BigIntMultirange,
            [typeof(NpgsqlRange<decimal>[])] = NpgsqlDbType.NumericMultirange,
            [typeof(NpgsqlRange<DateTime>[])] = NpgsqlDbType.TimestampMultirange,
            [typeof(NpgsqlRange<DateTimeOffset>[])] = NpgsqlDbType.TimestampTzMultirange,
            [typeof(NpgsqlRange<DateTime>[])] = NpgsqlDbType.DateMultirange,
            [typeof(Array)] = NpgsqlDbType.Array,
            [typeof(Range)] = NpgsqlDbType.Range,
            [typeof(Range[])] = NpgsqlDbType.Multirange
        };


        /// <summary>
        ///     Initializes a new instance of the <see cref="NpgsqlTypeMapper"/> class.
        /// </summary>
        private NpgsqlTypeMapper()
        { }

        public static NpgsqlTypeMapper Instance => new();

        /// <inheritdoc />
        public override string TypeName(DbType dbType)
        {
            return TypeName(ClrDbTypePairs[FromDbType(dbType)]);
        }

        /// <inheritdoc />
        public override Type GetType(string typeName)
        {
            var type = ClrTypes[NameDbTypePairs[NormalizeType(typeName)]];

            return typeName.Contains('[')
                ? type.MakeArrayType()
                : type;
        }

        public virtual string TypeName(NpgsqlDbType dbType)
        {
            try
            {
                return TypeNames[dbType];
            }
            catch (InvalidCastException)
            {
                throw new NotSupportedException(dbType + " is not supported by PostgreSQL.");
            }
        }

        public static NpgsqlDbType GetNpgsqlDbType(string type)
        {
            var normalized = NormalizeType(type);

            return type.Contains('[')
                ? NpgsqlDbType.Array | NameDbTypePairs[normalized]
                : NameDbTypePairs[normalized];
        }

        private static string NormalizeType(string type)
        {
            var index = type.IndexOfAny(['(', '[']);
            if (index != -1)
                return !type.StartsWith("char") || !CharTypeRegex().IsMatch(type) ? type[..(index)].Trim() : "varchar";
            else
                return type;
        }

        [GeneratedRegex(@"(char|character)\s*\(\d+\)")]
        private static partial Regex CharTypeRegex();
    }
}

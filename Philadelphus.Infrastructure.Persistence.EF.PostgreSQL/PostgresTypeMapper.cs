using NpgsqlTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL
{
    /// <summary>
    /// Класс-помощник для преобразования типов данных .NET, PostgreSQL, Npgsql
    /// </summary>
    internal static class PostgresTypeMapper
    {
        private static readonly Dictionary<string, Type> PostgresToDotNetMap = new()
        {
            // Числовые типы
            { "smallint", typeof(short) },
            { "int2", typeof(short) },
            { "integer", typeof(int) },
            { "int4", typeof(int) },
            { "bigint", typeof(long) },
            { "int8", typeof(long) },
            { "decimal", typeof(decimal) },
            { "numeric", typeof(decimal) },
            { "real", typeof(float) },
            { "float4", typeof(float) },
            { "double precision", typeof(double) },
            { "float8", typeof(double) },
            { "serial", typeof(int) },
            { "bigserial", typeof(long) },
        
            // Денежные типы
            { "money", typeof(decimal) },
        
            // Строковые типы
            { "character", typeof(string) },
            { "char", typeof(string) },
            { "character varying", typeof(string) },
            { "varchar", typeof(string) },
            { "text", typeof(string) },
            { "citext", typeof(string) }, // case-insensitive text
        
            // Бинарные типы
            { "bytea", typeof(byte[]) },
        
            // Логический тип
            { "boolean", typeof(bool) },
            { "bool", typeof(bool) },
        
            // Дата и время
            { "date", typeof(DateTime) },
            { "time", typeof(TimeSpan) },
            { "time without time zone", typeof(TimeSpan) },
            { "time with time zone", typeof(DateTimeOffset) },
            { "timetz", typeof(DateTimeOffset) },
            { "timestamp", typeof(DateTime) },
            { "timestamp without time zone", typeof(DateTime) },
            { "timestamp with time zone", typeof(DateTimeOffset) },
            { "timestamptz", typeof(DateTimeOffset) },
            { "interval", typeof(TimeSpan) },
        
            // UUID
            { "uuid", typeof(Guid) },
        
            // Геометрические типы
            { "point", typeof(NpgsqlPoint) },
            { "line", typeof(NpgsqlLine) },
            { "lseg", typeof(NpgsqlLSeg) },
            { "box", typeof(NpgsqlBox) },
            { "path", typeof(NpgsqlPath) },
            { "polygon", typeof(NpgsqlPolygon) },
            { "circle", typeof(NpgsqlCircle) },
        
            // Сетевые типы
            { "inet", typeof(NpgsqlInet) },
            { "cidr", typeof(NpgsqlCidr) },
        
            // JSON типы
            { "json", typeof(string) },
            { "jsonb", typeof(string) },
        
            // Массивы (базовые примеры)
            { "integer[]", typeof(int[]) },
            { "text[]", typeof(string[]) },
            { "uuid[]", typeof(Guid[]) },
        
            // Диапазонные типы
            { "int4range", typeof(NpgsqlRange<int>) },
            { "int8range", typeof(NpgsqlRange<long>) },
            { "numrange", typeof(NpgsqlRange<decimal>) },
            { "tsrange", typeof(NpgsqlRange<DateTime>) },
            { "tstzrange", typeof(NpgsqlRange<DateTimeOffset>) },
            { "daterange", typeof(NpgsqlRange<DateTime>) },
        
            // XML
            { "xml", typeof(string) },
        
            // OID
            { "oid", typeof(uint) },
        
            // Другие типы
            { "bit", typeof(BitArray) },
            { "varbit", typeof(BitArray) },
            { "tsvector", typeof(string) },
            { "tsquery", typeof(string) }
        };

        private static readonly Dictionary<Type, string> DotNetToPostgresMap = new()
        {
            { typeof(short), "smallint" },
            { typeof(int), "integer" },
            { typeof(long), "bigint" },
            { typeof(decimal), "numeric" },
            { typeof(float), "real" },
            { typeof(double), "double precision" },
            { typeof(string), "text" },
            { typeof(byte[]), "bytea" },
            { typeof(bool), "boolean" },
            { typeof(DateTime), "timestamp without time zone" },
            { typeof(DateTimeOffset), "timestamp with time zone" },
            { typeof(TimeSpan), "interval" },
            { typeof(Guid), "uuid" },
            { typeof(NpgsqlPoint), "point" },
            { typeof(NpgsqlLine), "line" },
            { typeof(NpgsqlLSeg), "lseg" },
            { typeof(NpgsqlBox), "box" },
            { typeof(NpgsqlPath), "path" },
            { typeof(NpgsqlPolygon), "polygon" },
            { typeof(NpgsqlCircle), "circle" },
            { typeof(NpgsqlInet), "inet" },
            { typeof(NpgsqlCidr), "cidr" },
            { typeof(int[]), "integer[]" },
            { typeof(string[]), "text[]" },
            { typeof(Guid[]), "uuid[]" },
            { typeof(NpgsqlRange<int>), "int4range" },
            { typeof(NpgsqlRange<long>), "int8range" },
            { typeof(NpgsqlRange<decimal>), "numrange" },
            { typeof(NpgsqlRange<DateTime>), "tsrange" },
            { typeof(NpgsqlRange<DateTimeOffset>), "tstzrange" }
        };

        /// <summary>
        /// Преобразовать тип PosgreSQL к типу .NET
        /// </summary>
        /// <param name="postgresType">PosgreSQL тип</param>
        /// <returns>Результат выполнения операции.</returns>
        public static Type ToDotNetType(string postgresType)
        {
            var normalizedType = postgresType.ToLowerInvariant();
            return PostgresToDotNetMap.TryGetValue(normalizedType, out var type)
                ? type
                : typeof(object);
        }

        /// <summary>
        /// Преобразовать тип .NET к типу PosgreSQL
        /// </summary>
        /// <param name="dotNetType">.NET тип</param>
        /// <returns>Результат выполнения операции.</returns>
        public static string ToPostgresType(Type dotNetType)
        {
            return DotNetToPostgresMap.TryGetValue(dotNetType, out var pgType)
                ? pgType
                : "text";
        }

        /// <summary>
        /// Преобразовать тип .NET к типу Npgsql
        /// </summary>
        /// <param name="dotNetType">.NET тип</param>
        /// <returns>Результат выполнения операции.</returns>
        public static NpgsqlDbType ToNpgsqlDbType(Type dotNetType)
        {
            return dotNetType switch
            {
                _ when dotNetType == typeof(short) => NpgsqlDbType.Smallint,
                _ when dotNetType == typeof(int) => NpgsqlDbType.Integer,
                _ when dotNetType == typeof(long) => NpgsqlDbType.Bigint,
                _ when dotNetType == typeof(decimal) => NpgsqlDbType.Numeric,
                _ when dotNetType == typeof(float) => NpgsqlDbType.Real,
                _ when dotNetType == typeof(double) => NpgsqlDbType.Double,
                _ when dotNetType == typeof(string) => NpgsqlDbType.Text,
                _ when dotNetType == typeof(byte[]) => NpgsqlDbType.Bytea,
                _ when dotNetType == typeof(bool) => NpgsqlDbType.Boolean,
                _ when dotNetType == typeof(DateTime) => NpgsqlDbType.Timestamp,
                _ when dotNetType == typeof(DateTimeOffset) => NpgsqlDbType.TimestampTz,
                _ when dotNetType == typeof(TimeSpan) => NpgsqlDbType.Interval,
                _ when dotNetType == typeof(Guid) => NpgsqlDbType.Uuid,
                _ when dotNetType == typeof(NpgsqlPoint) => NpgsqlDbType.Point,
                _ when dotNetType == typeof(NpgsqlLine) => NpgsqlDbType.Line,
                _ when dotNetType == typeof(NpgsqlLSeg) => NpgsqlDbType.LSeg,
                _ when dotNetType == typeof(NpgsqlBox) => NpgsqlDbType.Box,
                _ when dotNetType == typeof(NpgsqlPath) => NpgsqlDbType.Path,
                _ when dotNetType == typeof(NpgsqlPolygon) => NpgsqlDbType.Polygon,
                _ when dotNetType == typeof(NpgsqlCircle) => NpgsqlDbType.Circle,
                _ when dotNetType == typeof(NpgsqlInet) => NpgsqlDbType.Inet,
                _ when dotNetType == typeof(NpgsqlCidr) => NpgsqlDbType.Cidr,
                _ when dotNetType == typeof(int[]) => NpgsqlDbType.Array | NpgsqlDbType.Integer,
                _ when dotNetType == typeof(string[]) => NpgsqlDbType.Array | NpgsqlDbType.Text,
                _ when dotNetType == typeof(Guid[]) => NpgsqlDbType.Array | NpgsqlDbType.Uuid,
                _ when dotNetType.IsGenericType && dotNetType.GetGenericTypeDefinition() == typeof(NpgsqlRange<>) => NpgsqlDbType.Range,
                _ => NpgsqlDbType.Text
            };
        }

        /// <summary>
        /// Преобразовать значение .NET в значение БД
        /// </summary>
        /// <param name="value">Значение .NET</param>
        /// <param name="targetType">Тип .NET</param>
        /// <returns>Результат выполнения операции.</returns>
        public static object ConvertValue(object value, Type targetType)
        {
            if (value == null || targetType == null)
                return value;

            // Если значение уже нужного типа
            if (targetType.IsInstanceOfType(value))
                return value;

            // Обработка nullable типов
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // Преобразование строки в нужный тип
            if (value is string stringValue)
            {
                return underlyingType switch
                {
                    _ when underlyingType == typeof(Guid) => Guid.Parse(stringValue),
                    _ when underlyingType == typeof(DateTime) => DateTime.Parse(stringValue),
                    _ when underlyingType == typeof(int) => int.Parse(stringValue),
                    _ when underlyingType == typeof(long) => long.Parse(stringValue),
                    _ when underlyingType == typeof(decimal) => decimal.Parse(stringValue),
                    _ when underlyingType == typeof(double) => double.Parse(stringValue),
                    _ when underlyingType == typeof(float) => float.Parse(stringValue),
                    _ when underlyingType == typeof(bool) => bool.Parse(stringValue),
                    _ when underlyingType == typeof(short) => short.Parse(stringValue),
                    _ when underlyingType == typeof(byte) => byte.Parse(stringValue),
                    _ when underlyingType == typeof(sbyte) => sbyte.Parse(stringValue),
                    _ when underlyingType == typeof(uint) => uint.Parse(stringValue),
                    _ when underlyingType == typeof(ulong) => ulong.Parse(stringValue),
                    _ when underlyingType == typeof(ushort) => ushort.Parse(stringValue),
                    _ when underlyingType == typeof(TimeSpan) => TimeSpan.Parse(stringValue),
                    _ => Convert.ChangeType(stringValue, underlyingType)
                };
            }

            // Преобразование других типов
            try
            {
                return Convert.ChangeType(value, underlyingType);
            }
            catch
            {
                return value;
            }
        }
    }
}

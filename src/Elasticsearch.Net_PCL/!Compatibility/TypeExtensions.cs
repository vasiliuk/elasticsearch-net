using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


public static class TypeExtensions
{
    public static bool IsAssignableFrom(this Type type, Type c)
    {
        return type.GetTypeInfo().IsAssignableFrom(c.GetTypeInfo());
    }

    private static readonly Dictionary<Type, TypeCode> _typeCodeTable =
            new Dictionary<Type, TypeCode>()
            {
                { typeof( Boolean ), TypeCode.Boolean },
                { typeof( Char ), TypeCode.Char },
                { typeof( Byte ), TypeCode.Byte },
                { typeof( Int16 ), TypeCode.Int16 },
                { typeof( Int32 ), TypeCode.Int32 },
                { typeof( Int64 ), TypeCode.Int64 },
                { typeof( SByte ), TypeCode.SByte },
                { typeof( UInt16 ), TypeCode.UInt16 },
                { typeof( UInt32 ), TypeCode.UInt32 },
                { typeof( UInt64 ), TypeCode.UInt64 },
                { typeof( Single ), TypeCode.Single },
                { typeof( Double ), TypeCode.Double },
                { typeof( DateTime ), TypeCode.DateTime },
                { typeof( Decimal ), TypeCode.Decimal },
                { typeof( String ), TypeCode.String },
            };

    public static TypeCode GetTypeCode(this Type type)
    {
        if (type == null) return TypeCode.Empty;

        TypeCode result;
        return _typeCodeTable.TryGetValue(type, out result) ? result : TypeCode.Object;
    }

    public static FieldInfo GetField(this Type type, string name)
    {
        return type.GetTypeInfo().GetDeclaredField(name);
    }
}


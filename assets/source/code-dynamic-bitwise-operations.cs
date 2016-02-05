// code-dynamic-bitwise-operations.cs - Example of using expression trees to
// dynamically construct bitwise enumeration operations.
// 
// Written in 2016 by Etienne de Martel <edemartel@gmail.com>
// 
// To the extent possible under law, the author(s) have dedicated all copyright
// and related and neighboring rights to this software to the public domain
// worldwide. This software is distributed without any warranty.
//
// You should have received a copy of the CC0 Public Domain Dedication along with
// this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.

public static class DynamicBitwise
{
    public delegate Enum Operation(Enum value, Enum flag);

    public static Operation BuildOperation(Type enumType, Func<Expression, Expression, Expression> bitwise)
    {
        var valueParam = Expression.Parameter(typeof(Enum), "value");
        var flagParam = Expression.Parameter(typeof(Enum), "flag");

        var changeType = typeof(Convert).GetMethod("ChangeType", new[] { typeof(object), typeof(Type) });
        var underlyingType = Enum.GetUnderlyingType(enumType);

        var underlyingValue = Expression.Call(changeType, valueParam, Expression.Constant(underlyingType));
        var convertedValue = Expression.Convert(underlyingValue, underlyingType);

        var underlyingFlag = Expression.Call(changeType, flagParam, Expression.Constant(underlyingType));
        var convertedFlag = Expression.Convert(underlyingFlag, underlyingType);
        
        var result = bitwise(convertedValue, convertedFlag);
        
        var method = typeof(Enum).GetMethod("ToObject", new[] { typeof(Type), underlyingType });
        var resultValue = Expression.Call(method, Expression.Constant(enumType), result);
        var convertedResultValue = Expression.Convert(resultValue, typeof(Enum));
        
        return Expression.Lambda<Operation>(convertedResultValue, valueParam, flagParam).Compile();
    }

    public static Operation BuildAddFlag(Type enumType)
    {
        return BuildOperation(enumType, Expression.Or);
    }

    public static Operation BuildRemoveFlag(Type enumType)
    {
        return BuildOperation(enumType, (v, f) => Expression.And(v, Expression.Not(f)));
    }
}
// Copyright (c) 2016, Etienne de Martel
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the copyright holder nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

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
---
layout: post
title: "Code: Reflection bitwise operations in C#"
description: "Having fun with expression trees"
category: Coding
tags: [code]
---
{% include JB/setup %}

As some of you probably already know, I'm a programmer in life. More specifically, I'm a tool programmer in the video games industry. That means I spend most of my professional time creating tools to increase the productivity of game developers. And sometimes I come across interesting things, and I figured I could start sharing some of them. Needless to say, this article will be incomprehensible for the non-programmer, so feel free to skip this category if you lack the technical skills. And just like Quick shots, I make no guarantee about frequency of posts.

I work mostly in C#, and I also do C++, so that's the two most common things you'll see here.

<!-- more -->

One of our core features in our tools is a property grid. There is no standard way to do one in WPF, none of the available libraries were suitable for our needs, so we ended up writing one. For the unaware, a property grid displays the properties of objects as a list of names and editors. For enumerations, we normally show a drop down list, but if the enumeration is tagged with the `[Flags]` attribute, then we show a list of checkboxes for each flag.

Checking a checkbox does essentially the following:

    int currentValue = (int)Convert.ChangeType(Value, typeof(int));
    int newValue = currentValue | (int)Convert.ChangeType(Flag, typeof(int));
    if(currentValue != newValue)
        Value = Enum.ToObject(PropertyType, newValue);

Unchecking it works similarly, but with `newValue = currentValue & ~(int)Flag;` instead. `Flag` and `Value` are `Enum` objects, `PropertyType` is the `Type` of the enumeration. This works fine, except for when the enumeration's underlying type is not `int`, which happens surprisingly often on our case. The issue is we need to use reflection here, since we only know the underlying type at runtime, and we want to do bitwise operation on those. But how do you do bitwise operations via reflection? Well, it's surprisingly easy if you know your expression trees. Which is what I ended doing.

The gist of it is that we dynamically generate the code above using expression trees, which is a more readable way of emitting IL.

First we declare a delegate type for the operation:

    public delegate Enum Operation(Enum value, Enum flag);

Next we setup a function to build that delegate. This function takes the type of the enum, and a delegate that selects either | or &~ so we avoid copy/pasting the same method twice.

    public static Operation BuildOperation(Type enumType, Func<Expression, Expression, Expression> bitwise)
    {

Our method takes two parameters, so we declare two parameter expressions accordingly. The names are optional but help with debugging:

	    var valueParam = Expression.Parameter(typeof(Enum), "value");
	    var flagParam = Expression.Parameter(typeof(Enum), "flag");

Then we have to the following things for each parameter: we call `Convert.ChangeType` to get the underlying value, and then we cast it. This implies getting the method for `Convert.ChangeType` using reflection, and then building two expressions: first a method call, and then a cast. Notice how we pass the second parameter to `Expression.Call`: this takes in expressions, not values, so we have to wrap it into a constant. Also, `changeType` below could probably be a readonly static field, since it never changes.

	    var changeType = typeof(Convert).GetMethod("ChangeType", new[] { typeof(object), typeof(Type) });
        var underlyingType = Enum.GetUnderlyingType(enumType);

        var underlyingValue = Expression.Call(changeType, valueParam, Expression.Constant(underlyingType));
        var convertedValue = Expression.Convert(underlyingValue, underlyingType);

        var underlyingFlag = Expression.Call(changeType, flagParam, Expression.Constant(underlyingType));
        var convertedFlag = Expression.Convert(underlyingFlag, underlyingType);

Then we do the actual bitwise operation. More on that later.

	    var result = bitwise(convertedValue, convertedFlag);

Then we select the appropriate overload for `Enum.ToObject` using the same method as for `Convert.ChangeType`. The overload is selected by passing in the underlying type as the second element of the parameter array.

	    var method = typeof(Enum).GetMethod("ToObject", new[] {typeof (Type), underlyingType});

Same as before, we call the method and cast the result

	    var resultValue = Expression.Call(method, Expression.Constant(enumType), result);
	    var convertedResultValue = Expression.Convert(resultValue, typeof(Enum));

The final expression serves as the lambda's return statement, so we create a lambda with it, passing in the two initial parameter expressions for the delegate's parameters.

	    return Expression.Lambda<Operation>(convertedResultValue, valueParam, flagParam).Compile(); 
    }

Example usage goes thus. Since this could potentially be slow, you should really cache the delegates somewhere.

    var addFlag = BuildOperation(enumType, Expression.Or);
    var removeFlag = BuildOperation(enumType, (v, f) => Expression.And(v, Expression.Not(f)));

And that's it, really. You can use the same general method to do loads of other things, like arithmetic. I really like expression trees overall, as they allow for pretty neat tricks without sacrificing too much readability or performance.

[You can grab the full source code here](/assets/source/code-dynamic-bitwise-operations.cs).
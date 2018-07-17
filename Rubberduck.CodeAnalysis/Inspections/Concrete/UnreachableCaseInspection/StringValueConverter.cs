﻿using Rubberduck.Parsing.Grammar;
using Rubberduck.Parsing.PreProcessing;
using System;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Rubberduck.Inspections.Concrete.UnreachableCaseInspection
{
    public delegate bool StringToValueConversion<T>(string value, out T result, string typeName = null);

    public class StringValueConverter
    {
        public static bool TryConvertString(string valueText, out long value, string typeName = null)
        {
            value = default;
            typeName = typeName ?? Tokens.Long;

            if (valueText.Equals(Tokens.True) || valueText.Equals(Tokens.False))
            {
                value = valueText.Equals(Tokens.True) ? -1 : 0;
                return true;
            }

            if ((typeName == Tokens.Byte
                    || typeName == Tokens.Integer
                    || typeName == Tokens.Long
                    || typeName == Tokens.LongLong)
                && long.TryParse(valueText, out var integralValue))
            {
                value = integralValue;
                return true;
            }

            if (typeName == Tokens.Currency && decimal.TryParse(valueText, NumberStyles.Any, CultureInfo.InvariantCulture, out var decimalValue))
            {
                value = Convert.ToInt64(decimalValue);
                return true;
            }

            if (double.TryParse(valueText, NumberStyles.Any, CultureInfo.InvariantCulture, out var rationalValue))
            {
                value = Convert.ToInt64(rationalValue);
                return true;
            }

            return false;
        }

        public static bool TryConvertString(string valueText, out double value, string typeName = null)
        {
            value = default;
            if (valueText.Equals(Tokens.True) || valueText.Equals(Tokens.False))
            {
                value = valueText.Equals(Tokens.True) ? -1 : 0;
                return true;
            }
            if (double.TryParse(valueText, NumberStyles.Any, CultureInfo.InvariantCulture, out var rational))
            {
                value = rational;
                return true;
            }
            return false;
        }

        public static bool TryConvertString(string valueText, out decimal value, string typeName = null)
        {
            value = default;
            if (valueText.Equals(Tokens.True) || valueText.Equals(Tokens.False))
            {
                value = valueText.Equals(Tokens.True) ? -1 : 0;
                return true;
            }

            if (decimal.TryParse(valueText, NumberStyles.Any, CultureInfo.InvariantCulture, out var decimalValue))
            {
                value = decimalValue;
                return true;
            }

            return false;
        }

        public static bool TryConvertString(string valueText, out bool value, string typeName = null)
        {
            value = default;
            if (valueText.Equals(Tokens.True) || valueText.Equals(Tokens.False))
            {
                value = valueText.Equals(Tokens.True);
                return true;
            }
            if (double.TryParse(valueText, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleValue))
            {
                value = Math.Abs(doubleValue) >= double.Epsilon;
                return true;
            }
            return false;
        }

        public static bool TryConvertString(string valueText, out DateValueIComparableDecorator value, string typeName = null)
        {
            value = default;
            if (valueText.Count(f => f == '#') == 2 
                && valueText.StartsWith("#") 
                && valueText.EndsWith("#"))
            {
                try
                {
                    var literal = new DateLiteralExpression(new ConstantExpression(new StringValue(valueText)));
                    value = new DateValueIComparableDecorator((DateValue)literal.Evaluate());
                    return true;
                }
                catch (SyntaxErrorException)
                {
                }
                catch (Exception)
                {
                    //even though a SyntaxErrorException is thrown, this catch block
                    //seems to be needed(?)
                }
            }
            return false;
        }

        public static bool TryConvertString(string valueText, out string value, string typeName = null)
        {
            value = valueText;
            return true;
        }
    }
}

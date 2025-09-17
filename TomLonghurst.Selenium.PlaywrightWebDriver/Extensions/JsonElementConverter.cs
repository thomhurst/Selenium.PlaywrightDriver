using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace TomLonghurst.Selenium.PlaywrightWebDriver.Extensions;

public static class JsonElementConverter
{
    public static object? ConvertToNativeType(object? value)
    {
        if (value is not JsonElement jsonElement)
        {
            return value;
        }

        return jsonElement.ValueKind switch
        {
            JsonValueKind.String => jsonElement.GetString(),
            JsonValueKind.Number => ConvertNumber(jsonElement),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => null,
            JsonValueKind.Array => ConvertArray(jsonElement),
            JsonValueKind.Object => ConvertObject(jsonElement),
            _ => value
        };
    }

    private static object ConvertNumber(JsonElement jsonElement)
    {
        // Try to parse as int first, then double
        if (jsonElement.TryGetInt32(out var intValue))
        {
            return intValue;
        }
        
        if (jsonElement.TryGetInt64(out var longValue))
        {
            return longValue;
        }
        
        if (jsonElement.TryGetDouble(out var doubleValue))
        {
            return doubleValue;
        }
        
        // Fallback to decimal if needed
        if (jsonElement.TryGetDecimal(out var decimalValue))
        {
            return decimalValue;
        }
        
        // If all else fails, return the raw number as string
        return jsonElement.GetRawText();
    }

    private static object?[] ConvertArray(JsonElement jsonElement)
    {
        return jsonElement.EnumerateArray()
            .Select(element => ConvertToNativeType(element))
            .ToArray();
    }

    private static Dictionary<string, object?> ConvertObject(JsonElement jsonElement)
    {
        var result = new Dictionary<string, object?>();
        
        foreach (var property in jsonElement.EnumerateObject())
        {
            result[property.Name] = ConvertToNativeType(property.Value);
        }
        
        return result;
    }
}
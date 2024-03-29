﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Nodes;

namespace PowerOverlay;

public interface IApplicationJson
{
    public JsonNode ToJson();
}
public static class JsonStringExtension
{
    public static string ToLowerCamelCase(this string name)
    {
        if (String.IsNullOrEmpty(name)) return String.Empty;
        if (name.Length == 1) return name.ToLower();
        return String.Concat(name.Substring(0, 1).ToLower(), name.Substring(1));
    }

    public static JsonObject AddLowerCamel(this JsonObject node, string propertyName, JsonNode? value)
    {
        node[propertyName.ToLowerCamelCase()] = value;
        return node;
    }
    public static JsonObject AddLowerCamelValue<T>(this JsonObject node, string propertyName, T value)
    {
        node[propertyName.ToLowerCamelCase()] = JsonValue.Create<T>(value);
        return node;
    }

    public static JsonArray ToJson<T>(this IEnumerable<T> collection)
        where T : IApplicationJson
    {
        return new JsonArray(collection.Select(x => x.ToJson()).ToArray());
    }

    public static void TryGet<T>(this JsonObject o, string name, Action<T> f)
        where T : class
    {
        if (o.ContainsKey(name))
        {
            if (typeof(T) == typeof(JsonObject)) {
                f((o[name]!.AsObject() as T)!);
            }
            else if (typeof(T) == typeof(JsonArray))
            {
                f((o[name]!.AsArray() as T)!);
            }
            else if (typeof(T) == typeof(string))
            {
                var val = o[name]!.GetValue<string>();
                if (!String.IsNullOrWhiteSpace(val)) f((val as T)!);
            }
            else
            {
                f(o[name]!.GetValue<T>());
            }
        }
    }
    public static void TryGetValue<T>(this JsonObject o, string name, Action<T> f)
        where T : struct
    {
        if (o.ContainsKey(name))
        {
            f(o[name]!.GetValue<T>());
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Nodes;

namespace overlay_popup;

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

    public static JsonArray ToJson<T>(this IEnumerable<T> collection)
        where T : IApplicationJson
    {
        return new JsonArray(collection.Select(x => x.ToJson()).ToArray());
    }
}
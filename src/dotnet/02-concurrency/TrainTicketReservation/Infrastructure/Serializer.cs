﻿using System.Dynamic;
using System.Text.Json;

namespace TrainTicketReservation.Infrastructure;

public static class Serializer
{
    public static IObjectSerializer Instance { get; set; } =new ObjectSerializer();
}
public interface IObjectSerializer
{
    object? Deserialize(ReadOnlySpan<byte> span, Type t);
    JsonElement Parse(ReadOnlySpan<byte> span);
    byte[] SerializeToUtf8Bytes(object? t);
}
class ObjectSerializer : IObjectSerializer
{
    public static JsonSerializerOptions Options = new();
    private static JsonElement Empty = JsonSerializer.Deserialize<JsonElement>("{}");
    public object? Deserialize(ReadOnlySpan<byte> span, Type t)
    {
        return JsonSerializer.Deserialize(span, t, Options);
    }

    public JsonElement Parse(ReadOnlySpan<byte> span)
    {
        if(span.Length == 0) return Empty;
        return JsonSerializer.Deserialize<JsonElement>(span, Options);
    }

    public byte[] SerializeToUtf8Bytes(object? t)
    {
        return t == null ? Array.Empty<byte>() : JsonSerializer.SerializeToUtf8Bytes(t, t.GetType(), Options);
    }
}
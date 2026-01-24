using System.Text.Json.Serialization;

namespace ddchttp.web;

[JsonSerializable(typeof(ErrorResult))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}

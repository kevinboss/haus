namespace Haus.Ws;

public sealed record ZoneUpdate(
    string? Name = null,
    double? Latitude = null,
    double? Longitude = null,
    double? Radius = null,
    bool? Passive = null,
    string? Icon = null);

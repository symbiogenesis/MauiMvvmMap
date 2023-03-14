namespace MauiMvvmMap.Extensions;

using Microsoft.Maui.Maps;

public static class MapSpanExtensions
{
    public static (Location SouthWest, Location NorthEast) ToBounds(this MapSpan self)
    {
        var halfLat = self.LatitudeDegrees / 2d;
        var halfLong = self.LongitudeDegrees / 2d;

        var southWest = new Location(self.Center.Latitude - halfLat, self.Center.Longitude - halfLong);
        var northEast = new Location(self.Center.Latitude + halfLat, self.Center.Longitude + halfLong);

        return (southWest, northEast);
    }
}

namespace JadwalSholat.Core.Models;

/// <summary>One of the bundled cities whose 1-year prayer time dataset ships with the app.</summary>
public sealed record City(string Id, string Name, string Region, double Latitude, double Longitude)
{
    /// <summary>The 5 major Indonesian cities whose schedules are pre-fetched (see Requirement.md), keyed by their api.myquran.com city id.</summary>
    public static readonly IReadOnlyList<City> Bundled =
    [
        new("1301", "Jakarta", "DKI Jakarta", -6.2088, 106.8456),
        new("1219", "Bandung", "Jawa Barat", -6.9175, 107.6191),
        new("1433", "Semarang", "Jawa Tengah", -6.9932, 110.4203),
        new("1505", "Yogyakarta", "DI Yogyakarta", -7.7956, 110.3695),
        new("1638", "Surabaya", "Jawa Timur", -7.2575, 112.7521),
    ];

    public static City? FindById(string id) => Bundled.FirstOrDefault(c => c.Id == id);

    /// <summary>Nearest bundled city to a coordinate (haversine), used for geolocation auto-detect.</summary>
    public static City Nearest(double latitude, double longitude)
    {
        return Bundled
            .Select(c => (City: c, Distance: HaversineKm(latitude, longitude, c.Latitude, c.Longitude)))
            .OrderBy(x => x.Distance)
            .First().City;
    }

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371.0;
        double dLat = DegreesToRadians(lat2 - lat1);
        double dLon = DegreesToRadians(lon2 - lon1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
}

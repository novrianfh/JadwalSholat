namespace JadwalSholat.Core.Models;

/// <summary>One of the bundled cities whose 1-year prayer time dataset ships with the app.</summary>
public sealed record City(string Id, string Name, string Region, double Latitude, double Longitude)
{
    /// <summary>All 38 provincial capitals plus a set of other major Indonesian cities, keyed by their api.myquran.com city id.</summary>
    public static readonly IReadOnlyList<City> Bundled =
    [
        // Ibukota provinsi
        new("0119", "Banda Aceh", "Aceh", 5.5483, 95.3238),
        new("0228", "Medan", "Sumatera Utara", 3.5952, 98.6722),
        new("0314", "Padang", "Sumatera Barat", -0.9492, 100.3543),
        new("0412", "Pekanbaru", "Riau", 0.5071, 101.4478),
        new("0507", "Tanjungpinang", "Kepulauan Riau", 0.9186, 104.4453),
        new("0610", "Jambi", "Jambi", -1.6101, 103.6131),
        new("0816", "Palembang", "Sumatera Selatan", -2.9761, 104.7754),
        new("0907", "Pangkal Pinang", "Bangka Belitung", -2.1316, 106.1169),
        new("0710", "Bengkulu", "Bengkulu", -3.7928, 102.2608),
        new("1014", "Bandar Lampung", "Lampung", -5.4292, 105.2610),
        new("1301", "Jakarta", "DKI Jakarta", -6.2088, 106.8456),
        new("1106", "Serang", "Banten", -6.1149, 106.1503),
        new("1219", "Bandung", "Jawa Barat", -6.9175, 107.6191),
        new("1433", "Semarang", "Jawa Tengah", -6.9932, 110.4203),
        new("1505", "Yogyakarta", "DI Yogyakarta", -7.7956, 110.3695),
        new("1638", "Surabaya", "Jawa Timur", -7.2575, 112.7521),
        new("1709", "Denpasar", "Bali", -8.6705, 115.2126),
        new("1810", "Mataram", "Nusa Tenggara Barat", -8.5833, 116.1167),
        new("1922", "Kupang", "Nusa Tenggara Timur", -10.1772, 123.6070),
        new("2013", "Pontianak", "Kalimantan Barat", -0.0263, 109.3425),
        new("2214", "Palangka Raya", "Kalimantan Tengah", -2.2096, 113.9213),
        new("2113", "Banjarmasin", "Kalimantan Selatan", -3.3186, 114.5944),
        new("2310", "Samarinda", "Kalimantan Timur", -0.5022, 117.1536),
        new("2401", "Tanjung Selor", "Kalimantan Utara", 2.8386, 117.3691),
        new("2914", "Manado", "Sulawesi Utara", 1.4748, 124.8421),
        new("2506", "Gorontalo", "Gorontalo", 0.5435, 123.0568),
        new("2813", "Palu", "Sulawesi Tengah", -0.8917, 119.8707),
        new("3003", "Mamuju", "Sulawesi Barat", -2.6786, 118.8887),
        new("2622", "Makassar", "Sulawesi Selatan", -5.1477, 119.4327),
        new("2717", "Kendari", "Sulawesi Tenggara", -3.9985, 122.5130),
        new("3110", "Ambon", "Maluku", -3.6954, 128.1814),
        new("3211", "Sofifi", "Maluku Utara", 0.7397, 127.4956),
        new("3403", "Manokwari", "Papua Barat", -0.8615, 134.0620),
        new("3413", "Sorong", "Papua Barat Daya", -0.8762, 131.2558),
        new("3329", "Jayapura", "Papua", -2.5337, 140.7181),
        new("3317", "Nabire", "Papua Tengah", -3.3667, 135.4833),
        new("3308", "Wamena", "Papua Pegunungan", -4.0847, 138.9450),
        new("3315", "Merauke", "Papua Selatan", -8.4672, 140.3325),

        // Kota besar lain
        new("1221", "Bekasi", "Jawa Barat", -6.2383, 106.9756),
        new("1225", "Depok", "Jawa Barat", -6.4025, 106.7942),
        new("1107", "Tangerang", "Banten", -6.1783, 106.6319),
        new("1108", "Tangerang Selatan", "Banten", -6.2884, 106.7180),
        new("1222", "Bogor", "Jawa Barat", -6.5971, 106.8060),
        new("1223", "Cimahi", "Jawa Barat", -6.8841, 107.5413),
        new("1224", "Cirebon", "Jawa Barat", -6.7063, 108.5571),
        new("1634", "Malang", "Jawa Timur", -7.9666, 112.6326),
        new("1434", "Surakarta", "Jawa Tengah", -7.5755, 110.8243),
        new("1624", "Sidoarjo", "Jawa Timur", -7.4478, 112.7183),
        new("0506", "Batam", "Kepulauan Riau", 1.0456, 104.0305),
        new("2308", "Balikpapan", "Kalimantan Timur", -1.2379, 116.8529),
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

namespace BackendIntegrador.Infrastructure.Services.GemeloDigital;

public static class GemeloClimateMath
{
    public static decimal CalculateThi(decimal tempCelsius, decimal humedadRelativa)
    {
        var t = (double)tempCelsius;
        var rh = (double)humedadRelativa;
        return (decimal)((1.8 * t + 32) - ((0.55 - 0.0055 * rh) * (1.8 * t - 26)));
    }

    public static int CountConsecutiveHotDays(IReadOnlyList<(DateOnly Fecha, decimal ThiMax)> orderedByDate, decimal threshold)
    {
        var maxStreak = 0;
        var current = 0;
        foreach (var (_, thi) in orderedByDate.OrderBy(x => x.Fecha))
        {
            if (thi >= threshold)
            {
                current++;
                maxStreak = Math.Max(maxStreak, current);
            }
            else
            {
                current = 0;
            }
        }

        return maxStreak;
    }

    public static int CountTrailingHotDays(IReadOnlyList<(DateOnly Fecha, decimal ThiMax)> orderedByDate, decimal threshold)
    {
        var streak = 0;
        foreach (var (_, thi) in orderedByDate.OrderByDescending(x => x.Fecha))
        {
            if (thi >= threshold)
                streak++;
            else
                break;
        }

        return streak;
    }
}

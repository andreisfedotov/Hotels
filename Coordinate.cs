public record Coordinate(double Latilude, double Longitude)
{
    public static bool TryParse(string input, out Coordinate? coordinate)
    {
        coordinate = default;
        var splitArray = input.Split(',', 2);
        if (splitArray.Length != 2) return false;
        if (!double.TryParse(splitArray[0], out var lat)) return false;
        if (!double.TryParse(splitArray[1], out var lon)) return false;
        coordinate = new(lat, lon);
        return true;
    }

    public static async ValueTask<Coordinate?> BindAsync(HttpContext context,
        ParameterInfo parameter)
    {
        var input = context.GetRouteValue(parameter.Name!) as string ?? string.Empty;
        TryParse(input, out var coordinate);
        return coordinate;
    }
}
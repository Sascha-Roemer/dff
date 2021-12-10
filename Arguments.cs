namespace dff;

public static class Arguments
{
    public static IEnumerable<string> GetAll(this string[] args, string key, int offset = 0)
    {
        var found = false;
        var skip = 0;
        foreach(var e in args)
        {
            if (offset > skip++) continue;
            if (!found && e == key) found = true;
            else if (found && !e.StartsWith("-")) yield return e;
            else if (found) break;
        }
    }

    public static string GetOne(this string[] args, string key, int offset = 0)
    =>
    args.GetAll(key, offset).FirstOrDefault();

    public static bool IsCommand(this string[] args, string name, params string[] keys)
    =>
    args.Length > 0 && args[0] == name
    && args.Intersect(keys).Count() == keys.Count();

    public static int GetInt(this string[] args, string key, int defaultValue, int offset = 0)
    {
        var value = args.GetOne(key, offset);
        if (value != null && int.TryParse(value, out var result))
        {
            return result;
        }
        return defaultValue;
    }
}
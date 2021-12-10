namespace dff;

public class SizeDiagram
{
    const int k = 1000;
    const int m = 1000000;
    const int g = 1000000000;
    const long t = 1000000000000;
    private int _width;
    private int _height;

    public SizeDiagram(int width = 120, int height = 16)
    {
        _width = width;
        _height = height;
    }

    public void PrintSizeDiagram(Dictionary<long, int> countGroupedBySize)
    {
        #region Test with fake values
        /*
        countGroupedBySize.Clear();
        countGroupedBySize[1]=1000; // 1
        countGroupedBySize[10]=100; // 10
        countGroupedBySize[100]=1000; // 100
        countGroupedBySize[1000]=100; // 1k
        countGroupedBySize[10000]=1000; // 10k
        countGroupedBySize[100000]=100; // 100k
        countGroupedBySize[1000000]=1000; // 1m
        countGroupedBySize[10000000]=100; // 10m
        countGroupedBySize[100000000]=1000; // 100m
        countGroupedBySize[500000000]=100; // 500m
        countGroupedBySize[1000000000]=1000; // 1g
        countGroupedBySize[5000000000]=100; // 5g
        countGroupedBySize[10000000000]=1000; // 10g
        countGroupedBySize[50000000000]=100; // 50g
        countGroupedBySize[100000000000]=1000; // 100g
        countGroupedBySize[500000000000]=100; // 500g
        countGroupedBySize[1000000000000]=1000; // 1t
        countGroupedBySize[2000000000000]=100; // 2t
        countGroupedBySize[4000000000000]=1000; // 4t
        */
        #endregion
        
        #region Scale size
        var div = 1d;
        var data = new double[_width];
        var min = 0d;
        var maxValue = 1*t;
        var max = LogPos(maxValue / div);
        foreach(var e in countGroupedBySize)
        {
            if (e.Key == 0) continue;
            if (e.Key > maxValue) continue;
            var index = Scale(LogPos(e.Key / div), min, max, _width);
            data[index] += e.Value;
        }
        #endregion

        #region Scale count
        min = LogPos(data.Min());
        max = LogPos(data.Max());

        for (var i = 0; i < data.Length; i++)
        {
            data[i] = Scale(LogPos(data[i]), min, max, _height);
        }
        #endregion

        #region Output values
        Console.WriteLine();
        for(var i = _height-1; i >= 0; i--)
        {
            Console.Write("|");
            for(var e = 0; e < _width; e++)
            {
                Console.Write((data[e] == i && i > 0) ? "." : data[e] > i ? "|" : " ");
            }
            Console.WriteLine();
        }
        #endregion

        #region Output scale
        int itemWidth = GetItemWidth(maxValue, _width);
        Console.WriteLine("+" + Repeat('-', _width, ',', itemWidth) + "->");
        Console.WriteLine("0".PadRight(itemWidth * 2 + 1) + CreateScale(maxValue, _width, GetShortValue));
        Console.WriteLine("Average: ".PadRight(itemWidth * 2 + 1) + CreateScale(maxValue, _width, (e) => SumCountBySize(e, countGroupedBySize)));
        #endregion
    }

    private string Repeat(char value, int width, char separator, int interval)
    {
        var sb = new StringBuilder();
        for(var i = 1; i <= width; i++)
        {
            sb.Append(i % interval == 0 ? separator : value);
        }
        return sb.ToString();
    }

    private static string CreateScale(long max, int width, Func<long, string> getValue)
    {
        var sb = new StringBuilder();
        long i = 1000;
        int itemWidth = GetItemWidth(max, width);
        while (i <= max)
        {
            sb.Append(getValue(i).PadLeft(itemWidth));
            i = i * 10;
        }
        return sb.ToString();
    }

    private static int GetItemWidth(long max, int width) => (int)(width / LogPos(max));

    private static string GetShortValue(long value)
    {
        if (value < 1000) return value.ToString();
        if (value < 1000000) return (value/1000).ToString() + "k";
        if (value < 1000000000) return (value/1000000).ToString() + "m";
        if (value < 1000000000000) return (value/1000000000).ToString() + "g";
        if (value < 1000000000000000) return (value/1000000000000).ToString() + "t";
        return (value/1000000000000000).ToString() + "e";
    }

    private static string SumCountBySize(long value, Dictionary<long, int> countGroupedBySize) =>
        countGroupedBySize.Where(e => e.Key > (value / 10) && e.Key < value).Sum(e => e.Value).ToString();

    private static int Scale(double value, double min, double max, int scaleMax)
    {
        if (value < 0) return 0;
        if (min < 0) min = 0;
        if (max < 0) max = 0;

        return (int)((value-min) / (max-min) * (scaleMax -1));
    }

    private static double LogPos(double value) => value < 1 ? 0 : Math.Log(value, 10);
}
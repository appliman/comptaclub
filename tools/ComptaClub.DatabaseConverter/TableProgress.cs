namespace ComptaClub.DatabaseConverter;

internal sealed class TableProgress
{
    private const int BAR_WIDTH = 24;

    public void Report(string name, long copied, long total, bool completed = false)
    {
        var proportion = total == 0 ? 1d : Math.Clamp((double)copied / total, 0d, 1d);
        var filled = (int)Math.Floor(proportion * BAR_WIDTH);
        var bar = new string('█', filled) + new string('░', BAR_WIDTH - filled);
        var line = $"{name,-30} [{bar}] {copied}/{total}";

        if (Console.IsOutputRedirected || completed)
        {
            Console.WriteLine(line);
        }
        else
        {
            Console.Write($"\r{line.PadRight(Console.WindowWidth > 0 ? Console.WindowWidth - 1 : line.Length)}");
        }
    }
}

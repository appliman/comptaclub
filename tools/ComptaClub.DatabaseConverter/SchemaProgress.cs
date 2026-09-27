namespace ComptaClub.DatabaseConverter;

internal static class SchemaProgress
{
    public static async Task RunAsync(string provider, Func<Task> migration, CancellationToken cancellationToken)
    {
        var label = $"Création et mise à jour du schéma {provider}";
        Console.WriteLine($"{label} : en cours...");
        var task = migration();
        var frames = new[] { '|', '/', '-', '\\' };
        var frame = 0;

        while (!task.IsCompleted)
        {
            if (!Console.IsOutputRedirected)
            {
                Console.Write($"\r{label} : {frames[frame++ % frames.Length]}");
            }

            await Task.WhenAny(task, Task.Delay(150, cancellationToken));
            cancellationToken.ThrowIfCancellationRequested();
        }

        await task;
        Console.WriteLine($"\r{label} : terminé.");
    }
}

using ComptaClub.DatabaseConverter;

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, args) =>
{
    args.Cancel = true;
    cancellation.Cancel();
};

return await new ConversionConsole().RunAsync(cancellation.Token);

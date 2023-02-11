using ComptaClub.Requests.Documents;

namespace ComptaClub.Handlers.Documents;

internal class GetDocumentContentRequestHandler : IRequestHandler<Requests.Documents.GetDocumentContentRequest, long>
{
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public GetDocumentContentRequestHandler(IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<long> Handle(GetDocumentContentRequest request, CancellationToken cancellationToken)
    {
        var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = from dc in db.DocumentsContents
                      where dc.DocumentId == request.DocumentId
                      select dc.Content;

        long size = 0;

        await db.Database.OpenConnectionAsync();
        using var command = query.CreateDbCommand();
        using var reader = await command.ExecuteReaderAsync(System.Data.CommandBehavior.SequentialAccess, cancellationToken);
        var doc = await reader.ReadAsync(cancellationToken);
        if (doc)
        {
            using var str = reader.GetStream(0);
            int bufferSize = 1024;
            byte[] buffer = new byte[bufferSize];
            int pos = 0;
            while ((pos = str.Read(buffer, 0, bufferSize)) > 0)
            {
                request.Output.Write(buffer, 0, pos);
                size+= pos;
            }
            str.Close();
        }

        return size;
    }
}

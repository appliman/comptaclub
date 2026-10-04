using ClosedXML.Excel;
using ComptaClub.Contracts.Models.Members;

namespace ComptaClub.Handlers.Members;

internal sealed class ImportMembersExcelRequestHandler(IMediator mediator)
    : IRequestHandler<ImportMembersExcelRequest, ImportMembersExcelResult>
{
    public async Task<ImportMembersExcelResult> Handle(ImportMembersExcelRequest request, CancellationToken cancellationToken)
    {
        using var _workbook = new XLWorkbook(request.Content);
        var _result = new ImportMembersExcelResult();
        var _sheet = _workbook.Worksheets.FirstOrDefault()
            ?? throw new InvalidDataException("Le fichier Excel ne contient aucune feuille.");
        var _header = _sheet.FirstRowUsed()
            ?? throw new InvalidDataException("Le fichier Excel est vide.");
        var _columns = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var _cell in _header.CellsUsed())
        {
            if (!_columns.TryAdd(_cell.GetString().Trim(), _cell.Address.ColumnNumber))
            {
                throw new InvalidDataException("Le fichier Excel contient des en-têtes répétés.");
            }
        }
        foreach (var _name in new[] { "Numéro de licence", "Prénom", "Nom", "Email", "Type de licence" })
        {
            if (!_columns.ContainsKey(_name))
            {
                _result.AddErrorBrokenRule("File", $"La colonne « {_name} » est requise.");
            }
        }
        if (_result.HasError)
        {
            return _result;
        }
        foreach (var _row in _sheet.RowsUsed().Where(row => row.RowNumber() > _header.RowNumber()))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var _member = await mediator.Send(new CreateMemberRequest(), cancellationToken);
            _member.Email = _row.Cell(_columns["Email"]).GetString().Trim();
            _member.Name = $"{_row.Cell(_columns["Prénom"]).GetString()} {_row.Cell(_columns["Nom"]).GetString()}".Trim();
            _member.LicenseNumber = _row.Cell(_columns["Numéro de licence"]).GetString().Trim();
            _member.LicenseTypeName = _row.Cell(_columns["Type de licence"]).GetString().Trim();
            var _existing = string.IsNullOrWhiteSpace(_member.LicenseNumber) ? null
                : await mediator.Send(new GetMemberByFilterRequest(filter =>
                {
                    filter.LicenseNumber = _member.LicenseNumber;
                    filter.MemberState = null;
                }), cancellationToken);
            if (_existing is not null)
            {
                _result.AddWarningBrokenRule($"Row{_row.RowNumber()}", "Ce membre est déjà importé.");
                continue;
            }
            var _saved = await mediator.Send(new SaveEntityRequest<MemberData>(_member), cancellationToken);
            if (!_saved.HasError)
            {
                _result.ImportedIds.Add(_member.Id);
            }
            foreach (var _rule in _saved.ErrorBrokenRuleList)
            {
                foreach (var _message in _rule.MessageList)
                {
                    _result.AddErrorBrokenRule($"Row{_row.RowNumber()}.{_rule.PropertyName}", _message);
                }
            }
        }
        return _result;
    }
}

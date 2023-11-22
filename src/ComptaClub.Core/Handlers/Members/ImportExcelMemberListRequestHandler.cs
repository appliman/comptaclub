using ClosedXML.Excel;

using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Members;

namespace ComptaClub.Handlers.Members;

public class ImportExcelMemberListRequestHandler : IRequestHandler<ImportExcelMemberListRequest, IEnumerable<MemberData>?>
{
	private readonly IMediator _mediator;

	public ImportExcelMemberListRequestHandler(IMediator mediator)
	{
		_mediator = mediator;
	}

	public async Task<IEnumerable<MemberData>?> Handle(ImportExcelMemberListRequest request, CancellationToken cancellationToken)
	{
		XLWorkbook? workbook = null;
		if (request.ContentStream is not null)
		{
			workbook = new XLWorkbook(request.ContentStream);
		}
		else if (request.FileName is not null)
		{
			workbook = new XLWorkbook(request.FileName);
		}

		if (workbook == null)
		{
			return null;
		}

		var worksheet = workbook!.Worksheet(1);
		bool rowHeader = true;
		int emailColumnIndex = 0;
		int firstNameColumnIndex = 0;
		int lastNameColumnIndex = 0;
		int licenseNumberColumnIndex = 0;
		int licenseTypeColumnIndex = 0;
		int rowId = 1;

		var result = new List<MemberData>();
		foreach (IXLRow row in worksheet.RowsUsed())
		{
			if (rowHeader)
			{
				var columnIndex = 1;
				foreach (IXLCell cell in row.CellsUsed())
				{
					if ($"{cell.Value}".Equals("Numéro de licence", StringComparison.InvariantCultureIgnoreCase))
					{
						licenseNumberColumnIndex = columnIndex;
					}
					if ($"{cell.Value}".Equals("Prénom", StringComparison.InvariantCultureIgnoreCase))
					{
						firstNameColumnIndex = columnIndex;
					}
					if ($"{cell.Value}".Equals("Nom", StringComparison.InvariantCultureIgnoreCase))
					{
						lastNameColumnIndex = columnIndex;
					}
					if ($"{cell.Value}".Equals("Email", StringComparison.InvariantCultureIgnoreCase))
					{
						emailColumnIndex = columnIndex;
					}
					if ($"{cell.Value}".Equals("Type de licence", StringComparison.InvariantCultureIgnoreCase))
					{
						licenseTypeColumnIndex = columnIndex;
					}
					columnIndex++;
				}
				rowHeader = false;
			}
			else
			{
				var member = await _mediator.Send(new CreateMemberRequest());

				member.Email = $"{row.Cell(emailColumnIndex).Value}".Trim();
				member.Name = $"{row.Cell(firstNameColumnIndex).Value} {row.Cell(lastNameColumnIndex).Value}".Trim();
				member.LicenseNumber = $"{row.Cell(licenseNumberColumnIndex).Value}".Trim();
				member.LicenseTypeName = $"{row.Cell(licenseTypeColumnIndex).Value}".Trim();

				var existing = await _mediator.Send(new GetMemberByFilterRequest(f => f.LicenseNumber = member.LicenseNumber));
				if (existing != null)
				{
					continue;
				}

				existing = await _mediator.Send(new GetMemberByFilterRequest(f =>
				{
					f.LicenseNumber = member.Email;
					f.Name = member.Name;
				}));
				if (existing != null)
				{
					continue;
				}

				var saveResult = await _mediator.Send(new SaveEntityRequest<MemberData>(member));
				if (!saveResult.HasError)
				{
					result.Add(member);
				}

				rowId++;
			}
		}
		return result;
	}
}

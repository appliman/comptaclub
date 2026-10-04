namespace ComptaClub.Contracts.Models.Members;

public sealed record ImportMembersExcelRequest(Stream Content) : IRequest<ImportMembersExcelResult>;

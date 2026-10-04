using ComptaClub.Datas.Enums;

namespace ComptaClub.Blazor.Services.Mcp;

public sealed record MemberInput(string Name, string Email, MemberState State, string? LicenseNumber = null, string? LicenseTypeName = null);

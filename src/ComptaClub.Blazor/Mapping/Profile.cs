using ComptaClub.Contracts.Models.Exercices;

namespace ComptaClub.Blazor.Mapping;

public static class Profile
{
    public static ViewModels.Account ToViewModel(Datas.AccountData source) => new()
    {
        Id = source.Id,
        Code = source.Code,
        Label = source.Label,
        ParentAccountId = source.ParentAccountId,
        Direction = source.Direction,
        CreationDate = source.CreationDate,
        Level = source.Level,
        Children = ToViewModels(source.Children)
    };

    public static Datas.AccountData ToData(ViewModels.Account source) => new()
    {
        Id = source.Id,
        Code = source.Code,
        Label = source.Label,
        ParentAccountId = source.ParentAccountId,
        Direction = source.Direction,
        CreationDate = source.CreationDate,
        Level = source.Level,
        Children = source.Children.Select(ToData).ToList()
    };

    public static ViewModels.Exercice ToViewModel(Datas.ExerciceData source) => new()
    {
        Id = source.Id,
        Code = source.Code,
        Label = source.Label,
        InitialAmount = source.InitialAmount,
        BalanceAmount = source.BalanceAmount,
        StartDate = source.StartDate.FromDayId(),
        EndDate = source.EndDate.FromDayId(),
        CreationDate = source.CreationDate.FromDayId(),
        ClosedDate = source.ClosedDate.FromDayId(),
        LastEntryId = source.LastEntryId,
        Active = source.Active,
        ExerciceState = source.ExerciceState
    };

    public static Datas.ExerciceData ToData(ViewModels.Exercice source) => new()
    {
        Id = source.Id,
        Code = source.Code,
        Label = source.Label,
        InitialAmount = source.InitialAmount,
        BalanceAmount = source.BalanceAmount,
        StartDate = source.StartDate.ToDayId(),
        EndDate = source.EndDate.ToDayId(),
        CreationDate = source.CreationDate.ToDayId(),
        ClosedDate = source.ClosedDate.ToDayId(),
        LastEntryId = source.LastEntryId,
        Active = source.Active,
        ExerciceState = source.ExerciceState
    };

    public static ViewModels.Exercice ToViewModel(CreateExerciceRequest source) => new()
    {
        Code = source.Code,
        Label = source.Label,
        InitialAmount = source.InitialAmount,
        Active = source.Active
    };

    public static ViewModels.User ToViewModel(Datas.UserData source) => new()
    {
        Id = source.Id,
        Name = source.Name,
        Email = source.Email,
        CreationDate = source.CreationDate.FromDayId()
    };

    public static Datas.UserData ToData(ViewModels.User source) => new()
    {
        Id = source.Id,
        Name = source.Name,
        Email = source.Email,
        CreationDate = source.CreationDate.ToDayId()
    };

    public static ViewModels.Document ToViewModel(Datas.DocumentData source) => new()
    {
        Id = source.Id,
        CreationDate = source.CreationDate.FromDayId(),
        LastUpdate = source.LastUpdate.FromDayId(),
        FileName = source.FileName,
        Description = source.Description!,
        MimeType = source.MimeType,
        Size = source.Size,
        UserOwnerId = source.UserOwnerId
    };

    public static Datas.DocumentData ToData(ViewModels.Document source) => new()
    {
        Id = source.Id,
        CreationDate = source.CreationDate.ToDayId(),
        LastUpdate = source.LastUpdate.ToDayId(),
        FileName = source.FileName,
        Description = source.Description,
        MimeType = source.MimeType,
        Size = source.Size,
        UserOwnerId = source.UserOwnerId
    };

    public static ViewModels.IncomeStatement ToViewModel(Datas.IncomeStatementData source) => new()
    {
        Id = source.Id,
        ExerciceId = source.ExerciceId,
        CreationDate = source.CreationDate,
        CreditTotal = source.CreditTotal,
        DebitTotal = source.DebitTotal,
        Description = source.Description
    };

    public static ViewModels.IncomeStatementItem ToViewModel(Datas.IncomeStatementItemData source) => new()
    {
        Id = source.Id,
        IncomeStatementId = source.IncomeStatementId,
        ParentIncomeStatementItemId = source.ParentIncomeStatementItemId,
        AccountId = source.AccountId,
        Amount = source.Amount,
        Code = source.Code,
        Label = source.Label,
        Direction = source.Direction,
        Level = source.Level
    };

    public static List<ViewModels.Account> ToViewModels(IEnumerable<Datas.AccountData> source) => source.Select(ToViewModel).ToList();
    public static List<ViewModels.Exercice> ToViewModels(IEnumerable<Datas.ExerciceData> source) => source.Select(ToViewModel).ToList();
    public static List<ViewModels.User> ToViewModels(IEnumerable<Datas.UserData> source) => source.Select(ToViewModel).ToList();
    public static List<ViewModels.Document> ToViewModels(IEnumerable<Datas.DocumentData> source) => source.Select(ToViewModel).ToList();
    public static List<ViewModels.IncomeStatement> ToViewModels(IEnumerable<Datas.IncomeStatementData> source) => source.Select(ToViewModel).ToList();
    public static List<ViewModels.IncomeStatementItem> ToViewModels(IEnumerable<Datas.IncomeStatementItemData> source) => source.Select(ToViewModel).ToList();
}
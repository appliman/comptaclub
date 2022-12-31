using System;

using Azure.Core;

namespace ComptaClub.Blazor.Mapping;

public class Profile : AutoMapper.Profile
{
    public Profile()
    {
        CreateMap<Datas.AccountData, ViewModels.Account>()
            .ReverseMap();

        CreateMap<Datas.BankData, ViewModels.Bank>()
            .ReverseMap();

        CreateMap<Datas.ExerciceData, ViewModels.Exercice>()
            .ReverseMap();

        CreateMap<Requests.CreateExerciceRequest, ViewModels.Exercice>();

        CreateMap<Datas.EntryData, ViewModels.Entry>()
            .ForMember(d => d.Balance, opt => opt.MapFrom(s => new Models.Balance(s.BalanceValue)));

        CreateMap<ViewModels.Entry, Datas.EntryData>()
            .ForMember(d => d.BalanceValue, opt => opt.MapFrom(s => s.Balance!.Amount));

        CreateMap<ViewModels.AccountDirection, Datas.AccountDirection>()
             .ReverseMap();
    }
}

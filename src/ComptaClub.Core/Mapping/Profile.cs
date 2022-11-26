using System;

using Azure.Core;

namespace ComptaClub.Mapping;

public class Profile : AutoMapper.Profile
{
    public Profile()
    {
        CreateMap<Datas.Account, Models.Account>()
            .ReverseMap();

        CreateMap<Datas.Bank, Models.Bank>()
            .ReverseMap();

        CreateMap<Datas.Exercice, Models.Exercice>()
            .ReverseMap();

        CreateMap<Requests.CreateExerciceRequest, Models.Exercice>();

        CreateMap<Datas.Entry, Models.Entry>()
            .ForMember(d => d.Balance, opt => opt.MapFrom(s => new Models.Balance(s.BalanceValue)));

        CreateMap<Models.Entry, Datas.Entry>()
            .ForMember(d => d.BalanceValue, opt => opt.MapFrom(s => s.Balance!.Amount));
    }
}

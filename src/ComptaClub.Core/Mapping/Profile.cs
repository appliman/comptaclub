using System;

using Azure.Core;

namespace ComptaClub.Mapping;

public class Profile : AutoMapper.Profile
{
    public Profile()
    {
        CreateMap<Datas.Account, Models.Account>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.RowKey))
            .ForMember(d => d.Code, opt => opt.MapFrom(s => s.PartitionKey))
            .ReverseMap();

        CreateMap<Datas.Bank, Models.Bank>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.RowKey))
            .ForMember(d => d.Code, opt => opt.MapFrom(s => s.PartitionKey))
            .ReverseMap();

        CreateMap<Datas.Exercice, Models.Exercice>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.RowKey))
            .ForMember(d => d.Code, opt => opt.MapFrom(s => s.PartitionKey))
            .ReverseMap();

        CreateMap<Requests.CreateExerciceRequest, Models.Exercice>();

        CreateMap<Datas.Entry, Models.Entry>()
            .ForMember(d => d.CreationDate, opt => opt.MapFrom(s => s.Timestamp!.Value.LocalDateTime.ToDayId()))
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.RowKey))
            .ForMember(d => d.Balance, opt => opt.MapFrom(s => new Models.Balance(s.BalanceValue)));

        CreateMap<Models.Entry, Datas.Entry>()
            .ForMember(d => d.RowKey, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.PartitionKey, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.BalanceValue, opt => opt.MapFrom(s => s.Balance!.Amount));
    }
}

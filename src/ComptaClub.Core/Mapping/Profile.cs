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

        CreateMap<Requests.CreateExercice, Models.Exercice>();
    }
}

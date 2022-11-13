using System;

namespace ComptaClub.Mapping;

public class Profile : AutoMapper.Profile
{
    public Profile()
    {
        CreateMap<Datas.Account, Models.Account>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.RowKey))
            .ForMember(d => d.Code, opt => opt.MapFrom(s => s.PartitionKey));

        CreateMap<Models.Account, Datas.Account>()
            .ForMember(d => d.PartitionKey, opt => opt.MapFrom(s => s.Code))
            .ForMember(d => d.RowKey, opt => opt.MapFrom(s => s.Id));


        CreateMap<Datas.Bank, Models.Bank>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.RowKey))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.PartitionKey))
            .ReverseMap();
    }
}

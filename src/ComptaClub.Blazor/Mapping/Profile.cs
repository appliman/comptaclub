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
            .ForMember(d => d.InitialAmount, opt => opt.MapFrom(s => s.InitialAmount / 1000000m))
            .ForMember(d => d.BalanceAmount, opt => opt.MapFrom(s => s.BalanceAmount / 1000000m))
            .ForMember(d => d.StartDate, opt => opt.MapFrom(s => s.StartDate.FromDayId()))
            .ForMember(d => d.EndDate, opt => opt.MapFrom(s => s.EndDate.FromDayId()))
            .ForMember(d => d.CreationDate, opt => opt.MapFrom(s => s.CreationDate.FromDayId()))
            .ForMember(d => d.ClosedDate, opt => opt.MapFrom(s => s.ClosedDate.FromDayId()));


		CreateMap<ViewModels.Exercice, Datas.ExerciceData>()
            .ForMember(d => d.InitialAmount, opt => opt.MapFrom(s => Convert.ToInt64(s.InitialAmount * 1000000)))
            .ForMember(d => d.BalanceAmount, opt => opt.MapFrom(s => Convert.ToInt64(s.BalanceAmount * 1000000)))
            .ForMember(d => d.StartDate, opt => opt.MapFrom(s => s.StartDate.ToDayId()))
			.ForMember(d => d.EndDate, opt => opt.MapFrom(s => s.EndDate.ToDayId()))
			.ForMember(d => d.CreationDate, opt => opt.MapFrom(s => s.CreationDate.ToDayId()))
			.ForMember(d => d.ClosedDate, opt => opt.MapFrom(s => s.ClosedDate.ToDayId()));

        CreateMap<Requests.CreateExerciceRequest, ViewModels.Exercice>();

        CreateMap<Datas.EntryData, ViewModels.Entry>()
            .ForMember(d => d.Amount, opt => opt.MapFrom(s => s.Amount / 1000000m))
            .ForMember(d => d.CreationDate, opt => opt.MapFrom(s => s.CreationDate.FromDayId()))
            .ForMember(d => d.ValueDate, opt => opt.MapFrom(s => s.ValueDate.FromDayId()));

		CreateMap<ViewModels.Entry, Datas.EntryData>()
            .ForMember(d => d.Amount, opt => opt.MapFrom(s => Convert.ToInt64(s.Amount * 1000000)))
			.ForMember(d => d.ValueDate, opt => opt.MapFrom(s => s.ValueDate.ToDayId()))
			.ForMember(d => d.CreationDate, opt => opt.MapFrom(s => s.CreationDate.ToDayId()));

	}
}

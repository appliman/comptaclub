using System;

using Azure.Core;
using ComptaClub.Requests.Exercices;

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
            .ForMember(d => d.StartDate, opt => opt.MapFrom(s => s.StartDate.FromDayId()))
            .ForMember(d => d.EndDate, opt => opt.MapFrom(s => s.EndDate.FromDayId()))
            .ForMember(d => d.CreationDate, opt => opt.MapFrom(s => s.CreationDate.FromDayId()))
            .ForMember(d => d.ClosedDate, opt => opt.MapFrom(s => s.ClosedDate.FromDayId()));


		CreateMap<ViewModels.Exercice, Datas.ExerciceData>()
            .ForMember(d => d.StartDate, opt => opt.MapFrom(s => s.StartDate.ToDayId()))
			.ForMember(d => d.EndDate, opt => opt.MapFrom(s => s.EndDate.ToDayId()))
			.ForMember(d => d.CreationDate, opt => opt.MapFrom(s => s.CreationDate.ToDayId()))
			.ForMember(d => d.ClosedDate, opt => opt.MapFrom(s => s.ClosedDate.ToDayId()));

        CreateMap<CreateExerciceRequest, ViewModels.Exercice>();

        CreateMap<Datas.EntryData, ViewModels.Entry>()
            .ForMember(d => d.CreationDate, opt => opt.MapFrom(s => s.CreationDate.FromDayId()))
            .ForMember(d => d.ValueDate, opt => opt.MapFrom(s => s.ValueDate.FromDayId()));

		CreateMap<ViewModels.Entry, Datas.EntryData>()
			.ForMember(d => d.ValueDate, opt => opt.MapFrom(s => s.ValueDate.ToDayId()))
			.ForMember(d => d.CreationDate, opt => opt.MapFrom(s => s.CreationDate.ToDayId()));

        CreateMap<Datas.MemberData, ViewModels.Member>()
            .ForMember(d => d.CreationDate, opt => opt.MapFrom(s => s.CreationDate.FromDayId()));

        CreateMap<ViewModels.Member, Datas.MemberData>()
            .ForMember(d => d.CreationDate, opt => opt.MapFrom(s => s.CreationDate.ToDayId()));

		CreateMap<Datas.UserData, ViewModels.User>()
        	.ForMember(d => d.CreationDate, opt => opt.MapFrom(s => s.CreationDate.FromDayId()));

		CreateMap<ViewModels.User, Datas.UserData>()
			.ForMember(d => d.CreationDate, opt => opt.MapFrom(s => s.CreationDate.ToDayId()));

        CreateMap<Datas.DocumentData, ViewModels.Document>()
            .ForMember(d => d.CreationDate, opt => opt.MapFrom(s => s.CreationDate.FromDayId()))
            .ForMember(d => d.LastUpdate, opt => opt.MapFrom(s => s.LastUpdate.FromDayId()));

        CreateMap<ViewModels.Document, Datas.DocumentData>()
            .ForMember(d => d.CreationDate, opt => opt.MapFrom(s => s.CreationDate.ToDayId()))
            .ForMember(d => d.LastUpdate, opt => opt.MapFrom(s => s.LastUpdate.ToDayId()));

        CreateMap<Datas.IncomeStatementData, ViewModels.IncomeStatement>();
		CreateMap<Datas.IncomeStatementItemData, ViewModels.IncomeStatementItem>();
	}
}

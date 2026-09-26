using ComptaClub.Blazor.Mapping;
using ComptaClub.Datas;
using ComptaClub.Enums;
using ComptaClub.Extensions;

namespace ComptaClub.Tests;

[TestClass]
public class MappingProfileTests
{
    [TestMethod]
    public void Exercice_dates_and_fields_round_trip()
    {
        var source = new ExerciceData
        {
            Id = Guid.NewGuid(),
            Code = "2026",
            Label = "Exercice 2026",
            InitialAmount = 1200,
            BalanceAmount = 1550,
            StartDate = new DateTime(2026, 1, 1).ToDayId(),
            EndDate = new DateTime(2026, 12, 31).ToDayId(),
            CreationDate = new DateTime(2025, 12, 1).ToDayId(),
            ClosedDate = new DateTime(2027, 1, 15).ToDayId(),
            LastEntryId = Guid.NewGuid(),
            Active = true,
            ExerciceState = ExerciceState.Closed
        };

        var viewModel = Profile.ToViewModel(source);
        var result = Profile.ToData(viewModel);

        Assert.AreEqual(new DateTime(2026, 1, 1), viewModel.StartDate);
        Assert.AreEqual(new DateTime(2027, 1, 15), viewModel.ClosedDate);
        Assert.AreEqual(source.Id, result.Id);
        Assert.AreEqual(source.Code, result.Code);
        Assert.AreEqual(source.Label, result.Label);
        Assert.AreEqual(source.InitialAmount, result.InitialAmount);
        Assert.AreEqual(source.BalanceAmount, result.BalanceAmount);
        Assert.AreEqual(source.StartDate, result.StartDate);
        Assert.AreEqual(source.EndDate, result.EndDate);
        Assert.AreEqual(source.CreationDate, result.CreationDate);
        Assert.AreEqual(source.ClosedDate, result.ClosedDate);
        Assert.AreEqual(source.LastEntryId, result.LastEntryId);
        Assert.AreEqual(source.Active, result.Active);
        Assert.AreEqual(source.ExerciceState, result.ExerciceState);
    }

    [TestMethod]
    public void Account_hierarchy_is_copied_without_sharing_children()
    {
        var parent = new AccountData
        {
            Id = Guid.NewGuid(),
            Code = "7",
            Label = "Produits",
            Direction = AccountDirection.Credit,
            CreationDate = 42,
            Children =
            [
                new AccountData
                {
                    Id = Guid.NewGuid(),
                    Code = "701",
                    Label = "Ventes",
                    Direction = AccountDirection.Credit
                }
            ]
        };

        var viewModel = Profile.ToViewModel(parent);
        var result = Profile.ToData(viewModel);

        Assert.AreEqual(parent.Id, result.Id);
        Assert.AreEqual(parent.CreationDate, result.CreationDate);
        Assert.AreEqual(parent.Children[0].Id, result.Children.Single().Id);
        Assert.AreEqual(parent.Children[0].Code, result.Children.Single().Code);
        Assert.AreNotSame(parent.Children[0], result.Children[0]);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Datas;
using ComptaClub.Results;

namespace ComptaClub.Blazor.ViewModels;

public class Entry : IMetaEntity
{
    public int RowIndex { get; set; }
    public Guid Id { get; set; }
    public Enums.MetaEntity MetaEntity => Enums.MetaEntity.Entry;
    public string PartNumber { get; set; } = null!;
    public string Label { get; set; } = null!;
    public Guid BankId { get; set; }
    public Guid ExerciceId { get; set; }
    public Guid AccountId { get; set; }
    public Guid? UserCreatorId { get; set; }
    public Guid? MemberId { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime ValueDate { get; set; }
    public Enums.PaymentType PaymentType { get; set; }
	public Enums.AccountDirection AccountDirection { get; set; }
	public string? ExtraInfos { get; set; }
	public long Balance { get; set; }
	public long Amount { get; set; }
    public string? ImportId { get; set; }
    public List<Member> AssociatedMemberList { get; set; } = new();
}

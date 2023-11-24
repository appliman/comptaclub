using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Datas;
using ComptaClub.Results;

namespace ComptaClub.Blazor.ViewModels;

public class EntryRow : IMetaEntity
{
    public Guid Id { get => Entity.Id; set => Entity.Id = value; }
    public Datas.EntryData Entity { get; set; } = default!;
	public bool IsSelected { get; set; } = false;
	public MetaEntity MetaEntity => MetaEntity.Entry;
	public int RowIndex { get; set; }
    public List<MemberRow> AssociatedMemberList { get; set; } = new();
}

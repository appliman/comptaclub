using ComptaClub.Blazor.Extensions;
using ComptaClub.Datas;

namespace ComptaClub.Blazor.ViewModels
{
    public class Account : IMetaEntity
    {
        public int RowIndex { get; set; }
        public Guid Id { get; set; }
        public Enums.MetaEntity MetaEntity => Enums.MetaEntity.Account;
        public string Code { get; set; } = null!;
        public string Label { get; set; } = null!;
        public string CodeAndLabel => $"{Code} {Label}";
        public Guid? ParentAccountId { get; set; }
        public Enums.AccountDirection Direction { get; set; }
        public int CreationDate { get; set; }
        public List<Account> Children { get; set; } = new();
        public int Level { get; set; } = -1;
        public decimal Total { get; set; }
        public decimal DeepTotal 
        {
            get
            {
                if (Children.Any())
                {
                    return this.DeepSum(i => i.Total);
                }
                return Total;
            }
        }
    }
}

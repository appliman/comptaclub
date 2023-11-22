using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Contracts.Models.Users;

public class UserListFilterOptions
{
    public DeletedState DeletedState { get; set; } = DeletedState.Undeleted;
}

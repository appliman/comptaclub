using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Contracts.Models.Banks;

public record GetActiveBankRequest : IRequest<BankData?>
{

}

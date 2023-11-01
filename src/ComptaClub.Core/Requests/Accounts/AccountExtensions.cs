using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Requests.Accounts;
public static class AccountExtensions
{
	public static async Task<AccountData?> GetAccountById(this IMediator mediator, Guid accountId)
	{
		var account = await mediator.Send(new GetAccountByFilterRequest(f => f.SetById(accountId)));
		return account;
	}
}

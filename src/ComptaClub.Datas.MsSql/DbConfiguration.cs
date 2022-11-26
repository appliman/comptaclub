using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Datas;

public class DbConfiguration
{
	public string ConnectionString { get; set; } = null!;
	public string EnvironmentName { get; set; } = null!;
}

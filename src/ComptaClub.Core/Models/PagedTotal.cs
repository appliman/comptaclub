using System;
using System.Collections.Generic;
using System.Text;

namespace ComptaClub.Models
{
	public class PagedTotal
	{
		public PagedTotal()
		{
			ExtraProperties = new Dictionary<string, object>();
		}
		public int RowCount { get; set; }
		public Dictionary<string, object> ExtraProperties { get; set; }
	}
}

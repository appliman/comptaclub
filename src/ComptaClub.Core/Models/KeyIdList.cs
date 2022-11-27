using System;
using System.Collections.Generic;
using System.Text;

namespace ComptaClub.Models;

public class KeyIdList
{
	public KeyIdList()
	{
		KeyList = new List<object>();
		PropertyName = "Id";
	}
	public List<object> KeyList { get; set; }
	public string PropertyName { get; set; }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ComptaClub.Contracts.Models;

public class KeyIdList
{
    public KeyIdList()
    {
        KeyList = new List<Guid>();
        PropertyName = "Id";
    }
    public List<Guid> KeyList { get; set; }
    public string PropertyName { get; set; }
}

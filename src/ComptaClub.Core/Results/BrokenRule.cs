using System.Collections.Generic;
using System.Linq;

namespace ComptaClub.Results
{
    public enum Severity
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }
    public class BrokenRule
    {
        public BrokenRule()
        {
            Severity = Severity.Error;
        }

        public BrokenRule(string propertyName, string message)
        {
            PropertyName = propertyName;
            MessageList.Add(message);
            Severity = Severity.Error;
        }

        public string PropertyName { get; set; } = null!;
        public List<string> MessageList { get; set; } = new();
        public Severity Severity { get; set; }
    }
}

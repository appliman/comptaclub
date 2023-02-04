using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OFXNet.Enums;

namespace ComptaClub.Import;

public class OfxTransactionImport
{
    public string TransType { get; set; } = null!;

    public DateTime Date { get; set; }

    public decimal Amount { get; set; }

    public string TransactionID { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateTime TransactionInitializationDate { get; set; }

    public DateTime FundAvaliabilityDate { get; set; }

    public string Memo { get; set; } = null!; 

    public string IncorrectTransactionID { get; set; } = null!; 

    public string TransactionCorrectionAction { get; set; } = null!;

    public string ServerTransactionID { get; set; } = null!;

    public string CheckNum { get; set; } = null!;

    public string ReferenceNumber { get; set; } = null!;

    public string Sic { get; set; } = null!;

    public string PayeeID { get; set; } = null!;

    public string AccountID { get; set; } = null!;
    public string AccountKey { get; set; } = null!;
    public string AccountType { get; set; } = null!;
    public string? BankID { get; set; }
    public string? BranchID { get; set; }
    public string BankAccountType { get; set; } = null!;

    public string? Currency { get; set; }

}

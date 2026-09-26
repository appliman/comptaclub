using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


using OFXNet.Utils;

namespace ComptaClub.Import;

public static class OfxParser
{
    public static IEnumerable<OfxTransactionImport> ParseFromFile(string fileName)
    {
        var content = File.ReadAllText(fileName);
        return ParseFromContent(content);
    }
    public static IEnumerable<OfxTransactionImport> ParseFromContent(string content)
    {
        var import = OFXDocumentParser.Import(content);
        var result = new List<OfxTransactionImport>();
        foreach (var item in import.Transactions)
        {
            var importTransaction = new OfxTransactionImport();
            importTransaction.AccountID = import.Account!.AccountID.Cleanup();
            importTransaction.AccountKey = import.Account!.AccountKey.Cleanup();
            importTransaction.AccountType = $"{import.Account!.AccountType}";
            importTransaction.Amount = item.Amount;
            importTransaction.BankAccountType = $"{import.Account!.BankAccountType}";
            importTransaction.BankID = import.Account!.BankID.CleanupNullable();
            importTransaction.Currency = item.Currency.CleanupNullable();
            importTransaction.CheckNum = item.CheckNum.Cleanup();
            importTransaction.Date = item.Date;
            importTransaction.FundAvaliabilityDate = item.FundAvaliabilityDate;
            importTransaction.IncorrectTransactionID = item.IncorrectTransactionID.Cleanup();
            importTransaction.Memo = item.Memo.Cleanup();
            importTransaction.Name = item.Name.Cleanup();
            importTransaction.PayeeID = item.PayeeID.Cleanup();
            importTransaction.ReferenceNumber = item.ReferenceNumber.Cleanup();
            importTransaction.TransactionID = item.TransactionID.Cleanup();
            importTransaction.ServerTransactionID = item.ServerTransactionID.Cleanup();
            importTransaction.Sic = item.Sic.Cleanup();
            importTransaction.TransactionCorrectionAction = $"{item.TransactionCorrectionAction}";
            importTransaction.TransactionID = item.TransactionID.Cleanup();
            importTransaction.TransactionInitializationDate =  item.TransactionInitializationDate;
            importTransaction.TransType = $"{item.TransType}";
            result.Add(importTransaction);
        }
        return result;
    }

    public static string Cleanup(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }
        input = input.Replace("\t", "").Trim();
        return input;
    }

    public static string? CleanupNullable(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }
        input = input.Replace("\t", "").Trim();
        return input;
    }

}

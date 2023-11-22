using ComptaClub.Contracts.Results;

namespace ComptaClub.Blazor.Pages.Components;

public class CustomValidator : ComponentBase
{
    private ValidationMessageStore? _messageStore;

    [CascadingParameter]
    public EditContext? CurrentEditContext { get; set; }

    protected override void OnInitialized()
    {
        if (CurrentEditContext == null)
        {
            throw new InvalidOperationException();
        }
        _messageStore = new(CurrentEditContext);
        CurrentEditContext.OnValidationRequested += (s, arg) => _messageStore.Clear();
    }

    public void DisplayErrors(Dictionary<string, List<string>> errors)
    {
        foreach (var error in errors)
        {
            _messageStore!.Add(CurrentEditContext!.Field(error.Key), error.Value);
        }
        CurrentEditContext!.NotifyValidationStateChanged();
    }

    public void DisplayErrors(CommandResult validationResult)
    {
        foreach (var brokenRule in validationResult.ErrorBrokenRuleList)
        {
            foreach (var error in brokenRule.MessageList)
            {
                _messageStore!.Add(CurrentEditContext!.Field(brokenRule.PropertyName), error);
            }
        }
        CurrentEditContext!.NotifyValidationStateChanged();
    }

    public void DisplayErrors(IList<BrokenRule> brokenRuleList)
    {
        foreach (var brokenRule in brokenRuleList)
        {
            foreach (var error in brokenRule.MessageList)
            {
                _messageStore!.Add(CurrentEditContext!.Field(brokenRule.PropertyName), error);
            }
        }
        CurrentEditContext!.NotifyValidationStateChanged();
    }

    public void DisplayError(string error)
    {
         _messageStore!.Add(CurrentEditContext!.Field("All"), error);
        CurrentEditContext.NotifyValidationStateChanged();
    }

}

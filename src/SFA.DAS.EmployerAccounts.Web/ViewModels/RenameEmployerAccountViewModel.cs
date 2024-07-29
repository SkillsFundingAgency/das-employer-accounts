namespace SFA.DAS.EmployerAccounts.Web.ViewModels;

public class RenameEmployerAccountViewModel : ViewModelBase
{
    public string CurrentName { get; set; }
    private string _newName;
    public string NewName
    {
        get
        {
            return ChangeAccountName.GetValueOrDefault() ? _newName : LegalEntityName;
        }
        set
        {
            _newName = value;
        }
    }
    public bool?  ChangeAccountName { get; set; }
    public string NewNameError => GetErrorMessage(nameof(NewName));
    public string LegalEntityName { get; set; }
    public bool NameConfirmed { get; set; }
}
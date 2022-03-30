namespace Didimo.Networking
{
    public class AccountStatusQuery : GetQuery<AccountStatusResponse>
    {
        protected override string URL => $"{base.URL}/accounts/default/status";

    }
}
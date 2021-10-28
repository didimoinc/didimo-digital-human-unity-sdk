namespace Didimo.Networking
{
    public class ProfileQuery : GetQuery<ProfileResponse>
    {
        protected override string URL => $"{base.URL}/profile";
    }
}
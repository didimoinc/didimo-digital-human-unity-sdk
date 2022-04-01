namespace Didimo.Networking
{
    public class ListQuery : GetQuery<ListResponse>
    {
        protected override string URL => $"{base.URL}/didimos";
    }
}
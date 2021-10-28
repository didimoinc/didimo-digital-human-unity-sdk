namespace Didimo.Networking
{
    public class DidimoDetailsQuery : GetQuery<DidimoDetailsResponse>
    {
        public string DidimoKey { get; }
        protected override string URL => $"{base.URL}/didimos/{DidimoKey}";

        public DidimoDetailsQuery(string didimoKey) { DidimoKey = didimoKey; }
    }
}
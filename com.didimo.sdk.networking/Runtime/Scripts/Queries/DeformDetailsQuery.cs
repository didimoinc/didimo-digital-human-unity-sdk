namespace Didimo.Networking
{
    public class DeformDetailsQuery : GetQuery<DidimoDetailsResponse>
    {
        public string DidimoKey { get; }
        protected override string URL => $"{base.URL}/assets/{DidimoKey}";

        public DeformDetailsQuery(string didimoKey) { DidimoKey = didimoKey; }
    }
}
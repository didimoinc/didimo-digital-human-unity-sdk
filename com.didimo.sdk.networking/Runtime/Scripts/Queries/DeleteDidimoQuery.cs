namespace Didimo.Networking
{
    public class DeleteQuery : DeleteQuery<DidimoEmptyResponse>
    {
        public string DidimoKey { get; }
        protected override string URL => $"{base.URL}/didimos/{DidimoKey}";

        public DeleteQuery(string didimoKey) { DidimoKey = didimoKey; }
    }
}
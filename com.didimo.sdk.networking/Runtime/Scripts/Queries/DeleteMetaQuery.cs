namespace Didimo.Networking
{
    public class DeleteMetaQuery : DeleteQuery<DidimoEmptyResponse>
    {
        public string DidimoKey { get; }
        public string Key { get; }

        protected override string URL => $"{base.URL}/didimos/{DidimoKey}/meta_data/{Key}";

        public DeleteMetaQuery(string didimoKey, string key)
        {
            DidimoKey = didimoKey;
            Key = key;
        }
    }
}
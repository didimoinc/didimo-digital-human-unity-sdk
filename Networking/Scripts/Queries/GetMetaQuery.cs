namespace Didimo.Networking
{
    public class GetMetaQuery : GetQuery<MetaDataResponse>
    {
        public string DidimoKey { get; }
        public string MetaName { get; }
        protected override string URL => $"{base.URL}/didimos/{DidimoKey}/meta_data/{MetaName}";

        public GetMetaQuery(string didimoKey, string metaName)
        {
            DidimoKey = didimoKey;
            MetaName = metaName;
        }
    }
}
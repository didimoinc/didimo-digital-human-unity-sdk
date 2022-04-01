using System.Collections.Generic;

namespace Didimo.Networking
{
    public class UpdateMetaQuery : PutQuery<MetaDataResponse>
    {
        private string DidimoKey { get; }
        private string Key { get; }

        protected override string URL => $"{base.URL}/didimos/{DidimoKey}/meta_data/{Key}";

        public UpdateMetaQuery(string didimoKey, string key, string value) : base(key, new Dictionary<string, string> {{"value", value}})
        {
            this.DidimoKey = didimoKey;
            this.Key = key;
        }
    }
}
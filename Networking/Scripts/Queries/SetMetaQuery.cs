using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Didimo.Networking
{
    public class SetMetaQuery : PostQuery<DidimoResponse>
    {
        public string DidimoKey { get; }
        protected override string URL => $"{base.URL}/didimos/{DidimoKey}/meta_data/";

        public SetMetaQuery(string didimoKey, string key, string value) : base(new Dictionary<string, string> {{"name", key}, {"value", value}}) { DidimoKey = didimoKey; }
    }
}
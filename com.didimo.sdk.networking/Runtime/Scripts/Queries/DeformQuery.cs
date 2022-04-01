using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Didimo.Networking
{
    public class DeformQuery : Query<DeformResponse>
    {
        private const          string INPUT_TYPE_HEADER    = "input_type";
        private const          string INPUT_TYPE           = "vertex_deform";
        private const          string DEFORM_MATRIX_HEADER = "template_deformation";
        private const          string DEFORM_ASSET_HEADER  = "user_asset";
        public static readonly string DeformedAssetName    = DEFORM_ASSET_HEADER + ".obj";

        public string DidimoKey { get; }
        public byte[] DeformationMatrix { get; }
        public byte[] DeformationData { get; }
        protected override string URL => $"{base.URL}/assets";

        public DeformQuery(string didimoKey, byte[] deformationMatrix, byte[] deformationData)
        {
            DidimoKey = didimoKey;
            DeformationData = deformationData;
            DeformationMatrix = deformationMatrix;
        }

        protected override UnityWebRequest CreateRequest(Uri uri)
        {
            WWWForm form = new WWWForm();
            form.AddBinaryData(DEFORM_MATRIX_HEADER, DeformationMatrix, DEFORM_MATRIX_HEADER);
            form.AddBinaryData(DEFORM_ASSET_HEADER, DeformationData, DEFORM_ASSET_HEADER);
            form.AddField(INPUT_TYPE_HEADER, INPUT_TYPE);
            UnityWebRequest request = UnityWebRequest.Post(uri, form);

            return request;
        }
    }
}
Shader "Hidden/Didimo/SRP/Utility/EncodeToAlpha"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Base("Base", Color) = (1,1,1,1)
        _Invert("Invert", Float) = 0
        _Metallic("Metallic", Float) = 0
    }
    SubShader
    {
       Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
          
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;               
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            uniform float3 _Base;
            uniform int _Invert;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);               
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float sampleAlpha = _Invert == 0 ? col.r : 1-col.r;               
                return float4(_Base.r,_Base.g,_Base.b,sampleAlpha);
            }
            ENDCG
        }
    }
}

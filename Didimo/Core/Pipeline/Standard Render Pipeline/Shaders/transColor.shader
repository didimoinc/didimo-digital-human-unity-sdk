
/// by Tom Sirdevan for Didimo

Shader "Didimo/SRP/transColor"
{
  Properties
  {
    mainColor ("Color", Color) = (0.083, 0.058, 0.044)
    opacitySampler ("Opacity Map", 2D) = "white" {}
    zBias ("Z Bias", Range(-1,1)) = 0
  }
  SubShader
  {
    Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
    LOD 100

    Pass
    {
      Tags { 
        "LightMode"="Always"
      }

      ZWrite Off
      Blend SrcAlpha OneMinusSrcAlpha
      Cull Off

      CGPROGRAM
      #include "didimoCommon.cginc"
      #pragma vertex didimoVert
      #pragma fragment frag
      
      fixed3 mainColor;
      sampler2D opacitySampler;

      fixed4 frag(v2f i) : SV_Target
      {
        return fixed4(mainColor, tex2D(opacitySampler, i.uv).r);
      }
      ENDCG
    }
  }

  Fallback "Standard"
}

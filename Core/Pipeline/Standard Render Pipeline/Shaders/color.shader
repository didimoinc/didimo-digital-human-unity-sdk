
/// by Tom Sirdevan for Didimo

Shader "Didimo/DRP/color"
{
  Properties
  {
    baseColor ("Color", Color) = (1,1,1)
    zBias ("Z Bias", Range(-1,1)) = 0
  }
  SubShader
  {
    Tags { 
      "RenderType" = "Opaque" 
    }
    LOD 100

    Pass
    {
      Tags { 
        "LightMode"="Always"
      }

      CGPROGRAM
      #include "didimoCommon.cginc"
      #pragma vertex didimoVert
      #pragma fragment frag
      
      fixed3 baseColor;

      fixed4 frag(v2f i) : SV_Target
      {
        return fixed4(baseColor, 1);
      }
      ENDCG
    }
  }

  Fallback "Standard"
}


/// by Tom Sirdevan for Didimo

Shader "Didimo/SRP/texture"
{
  Properties
  {
    colorSampler ("Color Map", 2D) = "white" {}
    [Toggle] grayScale ("Gray Scale", Float) = 0
    invert ("Invert", Range(0,1)) = 0
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
      
      sampler2D colorSampler;
      float grayScale;
      half invert;

      fixed4 frag(v2f i) : SV_Target
      {
        half4 colorMap = tex2D(colorSampler, i.uv);
        if(grayScale)
        {
          half a = colorMap.r;
          a = lerp(a, 1 - a, invert);
          return fixed4(a, a, a, 1);
        }
        return colorMap;
      }
      ENDCG
    }
  }

  Fallback "Standard"
}



/// by Tom Sirdevan for Didimo

Shader "Didimo/SRP/simpleHair"
{
  Properties
  {
    /// diffuse
    diffColor ("Color", Color) = (0.132, 0.109, 0.091, 1)    
    directDiffInt ("Direct Diffuse", Range(0,2)) = 1
    indirectDiffInt ("Indirect Diffuse", Range(0,2)) = 1

    /// geometry
    opacity ("Opacity", Range(0,1)) = 0.6
    zBias ("Z Bias", Range(-1,1)) = 0
  }
  SubShader
  {
    Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
    LOD 100

    Pass
    {
      Tags { 
        "LightMode"="ForwardBase"
      }

      ZWrite Off
      Blend SrcAlpha OneMinusSrcAlpha
      Cull Off

      CGPROGRAM
      #pragma multi_compile_fwdbase
      #include "didimoCommon.cginc"
      #include "didimoLighting.cginc"
      #pragma vertex didimoVertFwdBase
      #pragma fragment frag

      fixed3 diffColor;
      float opacity;

      fixed4 _LightColor0;

      fixed4 frag(v2fbase i) : SV_Target
      {
        float3 tN = float3(0, 0, 1);

        half3x3 tS = half3x3(i.wT, i.wB, i.wN);

        float3 tV = normalize(mul(tS, i.wV));

        half3 wN = normalize(mul(tN, tS));

        float3 tD = getLightDir(i.wP, tS);
				UNITY_LIGHT_ATTENUATION(attenuation, i, i.wP);

        half3 diffuse = half3(0, 0, 0);

        diffuse = evalLambert(tD, tN) * _LightColor0 * attenuation;

				#ifdef VERTEXLIGHT_ON
					diffuse += i.vertexLighting;
				#endif

        diffuse *= directDiffInt;

        diffuse += evalIndDiffuse(wN, i.wP);

        diffuse *= diffColor;

        return fixed4(diffuse, opacity);
      }
      ENDCG
    }
  }
  Fallback "Standard"
}

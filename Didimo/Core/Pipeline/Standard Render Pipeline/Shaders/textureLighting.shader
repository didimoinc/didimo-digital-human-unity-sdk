
/// by Tom Sirdevan for Didimo

Shader "Didimo/SRP/textureLighting"
{
  Properties
  {
    colorSampler ("Color Map", 2D) = "white" {} 
    roughness ("Roughness", Range(0,1)) = 0.099
    fresnel ("Fresnel", Range(0,1)) = 0.69

    normalSampler ("Normal Map", 2D) = "blue" {}
    height ("Height", Range(0,5)) = 1
    zBias ("Z Bias", Range(-1,1)) = 0

    directDiffInt ("Direct Diffuse Intensity", Range(0,1)) = 1
    indirectDiffInt ("Indirect Diffuse Intensity", Range(0,1)) = 1
    directSpecInt ("Direct Specular", Range(0,1)) = 1
    indirectSpecInt ("Indirect Specular", Range(0,1)) = 1
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
        "LightMode"="ForwardBase"
      }
      Cull Back
      CGPROGRAM
      #pragma multi_compile_fwdbase
      #include "didimoCommon.cginc"
      #include "didimoLighting.cginc"
      #pragma vertex didimoVertFwdBase
      #pragma fragment frag
      
      sampler2D colorSampler;
      half roughness;
      half fresnel;

      sampler2D normalSampler;
      half height;

      fixed4 _LightColor0;

      fixed4 frag(v2fbase i) : SV_Target
      {
        half3 tN = getTsNormal(normalSampler, i.uv, height);

        half3x3 tS = half3x3(i.wT, i.wB, i.wN);

        half3 wN = normalize(mul(tN, tS));

        half3 tV = normalize(mul(tS, i.wV));  

        /// direct lighting
        half3 diffuse = half3(0, 0, 0);
        half3 spec = half3(0, 0, 0);
  
				float3 tD = getLightDir(i.wP, tS);
        UNITY_LIGHT_ATTENUATION(attenuation, i, i.wP);

        diffuse = evalLambert(tD, tN) * _LightColor0 * attenuation;
        spec = evalPhong(tD, tN, tV, roughness) * _LightColor0 * attenuation;

        diffuse *= directDiffInt;
        spec *= directSpecInt;

        /// indirect lighting
        diffuse += evalIndDiffuse(wN, i.wP);
        spec += evalIndSpec(wN, i.wV, roughness) * indirectSpecInt;

        diffuse *= tex2D(colorSampler, i.uv);

        spec *= evalFresnel(fresnel, tN, tV);

        return fixed4(diffuse + spec, 1);
      }
      ENDCG
    }

    Pass
    {
      Tags { 
        "LightMode"="ForwardAdd"
      }
      Blend One One // additive blending 
      Cull Back
      CGPROGRAM
      #pragma multi_compile_fwdadd_fullshadows
      #include "didimoCommon.cginc"
      #include "didimoLighting.cginc"
      #pragma vertex didimoVertFwdAdd
      #pragma fragment frag
      
      sampler2D colorSampler;
      half roughness;
      half fresnel;

      sampler2D normalSampler;
      half height;

      fixed4 _LightColor0;

      fixed4 frag(v2fadd i) : SV_Target
      {
        half3 tN = getTsNormal(normalSampler, i.uv, height);

        half3x3 tS = half3x3(i.wT, i.wB, i.wN);

        half3 tV = normalize(mul(tS, i.wV));  

        half3 diffuse = half3(0, 0, 0);
        half3 spec = half3(0, 0, 0);
  
				float3 tD = getLightDir(i.wP, tS);
        UNITY_LIGHT_ATTENUATION(attenuation, i, i.wP);

        diffuse = evalLambert(tD, tN) * _LightColor0 * attenuation;
        spec = evalPhong(tD, tN, tV, roughness) * _LightColor0 * attenuation;

        diffuse *= directDiffInt;
        spec *= directSpecInt;

        diffuse *= tex2D(colorSampler, i.uv);

        spec *= evalFresnel(fresnel, tN, tV);

        return fixed4(diffuse + spec, 1);
      }
      ENDCG
    }

  }

  Fallback "Standard"
}

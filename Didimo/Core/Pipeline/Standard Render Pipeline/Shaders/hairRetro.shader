
/// by Tom Sirdevan for Didimo

Shader "Didimo/SRP/hairRetro"
{
  Properties
  {
    /// diffuse
    colorSampler ("Color Map", 2D) = "white" {}
    colorTint ("Tint", Color) = (0, 0, 0, 0)

    /// spec
    specColor ("Specular Color", Color) = (0.557, 0.499, 0.393)
    spec1Int ("Specular 1 Intensity", Range(0,2)) = 1
    specExp1 ("Specular 1 Spread", Range(0,1)) = 0.4
    spec2Int ("Specular 2 Intensity", Range(0,2)) = 1
    specExp2 ("Specular 2 Spread", Range(0,1)) = 0.5
    specShiftSampler ("Specular Shift Map", 2D) = "white" {}

    /// geometry
    normalMap ("Normal Map", 2D) = "blue" {}
    height ("Height", Range(0,5)) = 1.38   
    transThresh ("Opacity Threshold", Range(0,1)) = 0.308

    zBias ("Z Bias", Range(-1,1)) = 0

    directDiffInt ("Direct Diffuse Intensity", Range(0,1)) = 1
    indirectDiffInt ("Indirect Diffuse Intensity", Range(0,1)) = 1
    directSpecInt ("Direct Specular", Range(0,1)) = 1
    indirectSpecInt ("Indirect Specular", Range(0,1)) = 1    
  }
  SubShader
  {
    Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
    LOD 100

    /// alpha cutout
    Pass
    {
      Tags {
        "LightMode"="Always"
      }
      AlphaToMask On /// alpha-to-coverage pass
      Cull Off
      CGPROGRAM
      #include "didimoCommon.cginc"
      #include "didimoLighting.cginc"
      #pragma vertex didimoVert
      #pragma fragment frag

      fixed3 diffColor;

      fixed3 specColor;
      float spec1Int;
      float specExp1;
      float spec2Int;
      float specExp2;
      sampler2D specShiftSampler;

      sampler2D normalMap;
      half height;

      sampler2D opacitySampler;
      half opacityThreshold;

      fixed4 _LightColor0;
      
      fixed4 frag(v2f i) : SV_Target
      {
        fixed opacityMap = tex2D(opacitySampler, i.uv).r;

        clip(opacityMap - opacityThreshold);

        return fixed4(diffColor, 1);
      }
      ENDCG
    }

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
      #define DIDIMO_HAIR      
      #include "didimoCommon.cginc"
      #include "didimoLighting.cginc"
      #pragma vertex didimoVertFwdBase
      #pragma fragment frag

      sampler2D colorSampler;
      fixed4 colorTint;

      fixed3 specColor;
      float spec1Int;
      float specExp1;
      float spec2Int;
      float specExp2;
      sampler2D specShiftSampler;

      sampler2D normalMap;
      half height;

      fixed4 _LightColor0;

      fixed4 frag(v2fbase i) : SV_Target
      {
        fixed specShiftMap = tex2D(specShiftSampler, i.uv).r;

        half3 tN = getTsNormal(normalMap, i.uv, height);

        half3x3 tS = half3x3(i.wT, i.wB, i.wN);

        half3 tV = normalize(mul(tS, i.wV));

        half _specExp1 = lerp(80, 40, specExp1);
        half _specExp2 = lerp(40, 10, specExp2);

        half3 diffuse = half3(0, 0, 0);
        half3 spec1 = half3(0, 0, 0);
				half3 spec2 = half3(0, 0, 0);
        fixed shadowSum = 0;

				float3 tD = getLightDir(i.wP, tS);
				UNITY_LIGHT_ATTENUATION(attenuation, i, i.wP);

        evalHairBrdf(tD, _LightColor0, tN, tV, _specExp1, _specExp2, specShiftMap, diffuse, spec1, spec2);

        diffuse *= attenuation;
        spec1 *= attenuation;
        spec2 *= attenuation;

				#ifdef VERTEXLIGHT_ON
					diffuse += i.vertexLighting;
				#endif

        diffuse *= directDiffInt;
        spec1 *= directSpecInt;
        spec2 *= directSpecInt;

        /// indirect 
        half3 wN = normalize(mul(tN, tS));
        diffuse += evalIndDiffuse(wN, i.wP);

        half3 indSpec = evalIndSpec(wN, i.wV, 0.9); /// TODO: also shouldn't wN be biased towards the hair 
        spec1 += indSpec;
        spec2 += indSpec;

        fixed4 colorMap = tex2D(colorSampler, i.uv);
        fixed3 diffColor = lerp(colorMap.rgb, colorTint.rgb, colorTint.a);

        diffuse *= diffColor;
        spec1 *= specColor * spec1Int;
        spec2 *= diffColor * spec2Int; // * specularNoise

        fixed3 result = diffuse + spec1 + spec2;
        // result *= colorMap.a;

        return fixed4(result, colorMap.a);
      }
      ENDCG
    }

    Pass
    {
      Tags { 
        "LightMode"="ForwardAdd"
      }
      ZWrite Off
      Blend SrcAlpha OneMinusSrcAlpha
      Cull Off
      CGPROGRAM
      #pragma multi_compile_fwdadd_fullshadows
      #define DIDIMO_HAIR
      #include "didimoCommon.cginc"
      #include "didimoLighting.cginc"
      #pragma vertex didimoVertFwdAdd
      #pragma fragment frag

      sampler2D colorSampler;
      fixed4 colorTint;

      fixed3 specColor;
      float spec1Int;
      float specExp1;
      float spec2Int;
      float specExp2;
      sampler2D specShiftSampler;

      sampler2D normalMap;
      half height;

      fixed4 _LightColor0;

      fixed4 frag(v2fadd i) : SV_Target
      {
        fixed specShiftMap = tex2D(specShiftSampler, i.uv).r;

        half3 tN = getTsNormal(normalMap, i.uv, height);

        half3x3 tS = half3x3(i.wT, i.wB, i.wN);

        half3 tV = normalize(mul(tS, i.wV));

        half _specExp1 = lerp(80, 40, specExp1);
        half _specExp2 = lerp(40, 10, specExp2);

        half3 diffuse = half3(0, 0, 0);
        half3 spec1 = half3(0, 0, 0);
				half3 spec2 = half3(0, 0, 0);
        fixed shadowSum = 0;

				float3 tD = getLightDir(i.wP, tS);
        UNITY_LIGHT_ATTENUATION(attenuation, i, i.wP);
        evalHairBrdf(tD, _LightColor0, tN, tV, _specExp1, _specExp2, specShiftMap, diffuse, spec1, spec2);

        diffuse *= attenuation;
        spec1 *= attenuation;
        spec2 *= attenuation;

        diffuse *= directDiffInt;
        spec1 *= directSpecInt;
        spec2 *= directSpecInt;

        fixed4 colorMap = tex2D(colorSampler, i.uv);
        fixed3 diffColor = lerp(colorMap.rgb, colorTint.rgb, colorTint.a);

        diffuse *= diffColor;
        spec1 *= specColor * spec1Int;
        spec2 *= diffColor * spec2Int; // * specularNoise

        fixed3 result = diffuse + spec1 + spec2;
        // result *= colorMap.a;

        return fixed4(result, colorMap.a);
      }
      ENDCG
    }

  }
  Fallback "Standard"
}

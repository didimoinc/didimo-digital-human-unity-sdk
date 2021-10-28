
/// by Tom Sirdevan for Didimo

Shader "Didimo/SRP/eye"
{
	Properties
	{
		/// diffuse
		colorSampler ("Color Map", 2D) = "white" {}

		_darken ("Darken", Range(0,1)) = 0.35

		/// spec
		irisSpecInt ("Iris Spec Intensity", Range(0,2)) = 0.2
		irisRough ("Iris Roughness", Range(0,1)) = 0.703
		fresnel ("Cornea Fresnel", Range(0,1)) = 0.7

		/// geo
		normalSampler ("Normal Map", 2D) = "blue" {}
		height ("Height", Range(0,1)) = 0.916
		refrSize ("Refraction Size", Range(0,1)) = 0.389
		refrAmount ("Refraction Amount", Range(0,1)) = 0.258
		uvScale ("UV Scale", Range(0.1, 2)) = 1.04

		zBias ("Z Bias", Range(-1,1)) = -0.3 //0

    directDiffInt ("Direct Diffuse Intensity", Range(0,1)) = 1
    indirectDiffInt ("Indirect Diffuse Intensity", Range(0,1)) = 1
    directSpecInt ("Direct Specular", Range(0,1)) = 1
    indirectSpecInt ("Indirect Specular", Range(0,1)) = 1
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Tags { 
				"LightMode"="ForwardBase"
			}

			Cull Back

			CGPROGRAM
			#pragma multi_compile_fwdbase
			#define DIDIMO_EYE
			#include "didimoCommon.cginc"
			#include "didimoLighting.cginc"
			#pragma vertex didimoVertFwdBase
			#pragma fragment frag

			/// diffuse
			sampler2D colorSampler;
			half _darken;

			/// spec
			half irisSpecInt;
			half irisRough;
			half fresnel;

			/// geo
			sampler2D normalSampler;
			half height;
			half refrSize;
			half refrAmount;
			half uvScale;

			fixed4 _LightColor0;

			fixed4 frag(v2fbase i) : SV_Target
			{
				float3x3 tS = float3x3(i.wT, i.wB, i.wN);

				half3 tV = normalize(mul(tS, i.wV));

				/// scale uvs
				half2 uv = i.uv - half2(0.5, 0.5);
				uv *= uvScale;
				uv += half2(0.5, 0.5);

				/// refraction
				half irisMask = 0;
				half refrHeight = 0;
				half radius = lerp(0, 0.3, refrSize);
				half2 center = half2(0.5, 0.5);
				if(pow(uv.x - center.x, 2) + pow(uv.y - center.y, 2) < pow(radius, 2)) /// inside circle test
				{
					refrHeight = 1 - (length(uv - center) / radius);
					irisMask = 1;
				}
				// float a = irisMask;  // irisMask refrHeight
				// return fixed4(a, a, a, 1);

				half _refrAmount = lerp(0, 0.5, refrAmount);
				half3 refrVec = -half3(tV.x, tV.y, tV.z);
				half3 offset = refrVec * refrHeight * _refrAmount;
				uv += offset.xy;
				// return fixed4(uv, 0, 1);

				half3 tN = tex2D(normalSampler, uv).xyz;
				tN.x = 1 - tN.x;
				// tN.y = 1 - tN.y;
				tN = (tN * 2.0 - 1.0);
				tN *= height;
				tN.z += 1.0 - height;
				tN = normalize(tN);

				half3 wN = normalize(mul(tN, tS)); /// world normal

				/// cornea normals
				const half3 ctN = half3(0, 0, 1);
				half3 cwN = normalize(mul(ctN, tS));

        /// direct lighting
        half3 diffuse = half3(0, 0, 0);
        half3 irisSpec = half3(0, 0, 0);
				half3 corneaSpec = half3(0, 0, 0);

				float3 tD = getLightDir(i.wP, tS);
				UNITY_LIGHT_ATTENUATION(attenuation, i, i.wP);	

				evalEyeBrdf(tD, _LightColor0, tN, ctN, tV, irisRough, diffuse, irisSpec, corneaSpec);

        diffuse *= attenuation;
        irisSpec *= attenuation;
				corneaSpec *= attenuation;

				#ifdef VERTEXLIGHT_ON
					diffuse += i.vertexLighting;
				#endif

        diffuse *= directDiffInt;
        irisSpec *= directSpecInt;
				corneaSpec *= directSpecInt;

				/// indirect lighting
				diffuse += evalIndDiffuse(wN, i.wP);
				corneaSpec += evalIndSpec(cwN, i.wV, 0);

				half3 colorMap = tex2D(colorSampler, uv).xyz;
				diffuse *= colorMap * (1 -_darken);

				irisSpec *= irisSpecInt * colorMap * irisMask * refrHeight * 5;

				corneaSpec *= evalFresnel(fresnel, ctN, tV);

				half sideDampen = min(1, pow(abs(dot(ctN, tV)), 10) * 10);
				// return fixed4(sideDampen, sideDampen, sideDampen, 1);
				corneaSpec *= sideDampen;

				return fixed4(diffuse + irisSpec + corneaSpec, 1.0);
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
			#define DIDIMO_EYE
			#include "didimoCommon.cginc"
			#include "didimoLighting.cginc"
			#pragma vertex didimoVertFwdAdd
			#pragma fragment frag

			/// diffuse
			sampler2D colorSampler;
			half _darken;

			/// spec
			half irisSpecInt;
			half irisRough;
			half fresnel;

			/// geo
			sampler2D normalSampler;
			half height;
			half refrSize;
			half refrAmount;
			half uvScale;

			fixed4 _LightColor0;

			fixed4 frag(v2fadd i) : SV_Target
			{
				float3x3 tS = float3x3(i.wT, i.wB, i.wN);

				half3 tV = normalize(mul(tS, i.wV));

				/// scale uvs
				half2 uv = i.uv - half2(0.5, 0.5);
				uv *= uvScale;
				uv += half2(0.5, 0.5);

				/// refraction
				half irisMask = 0;
				half refrHeight = 0;
				half radius = lerp(0, 0.3, refrSize);
				half2 center = half2(0.5, 0.5);
				if(pow(uv.x - center.x, 2) + pow(uv.y - center.y, 2) < pow(radius, 2)) /// inside circle test
				{
					refrHeight = 1 - (length(uv - center) / radius);
					irisMask = 1;
				}
				// float a = irisMask;  // irisMask refrHeight
				// return fixed4(a, a, a, 1);

				half _refrAmount = lerp(0, 0.5, refrAmount);
				half3 refrVec = -half3(tV.x, tV.y, tV.z);
				half3 offset = refrVec * refrHeight * _refrAmount;
				uv += offset.xy;
				// return fixed4(uv, 0, 1);

				half3 tN = tex2D(normalSampler, uv).xyz;
				tN.x = 1 - tN.x;
				// tN.y = 1 - tN.y;
				tN = (tN * 2.0 - 1.0);
				tN *= height;
				tN.z += 1.0 - height;
				tN = normalize(tN);

				/// cornea normals
				const half3 ctN = half3(0, 0, 1);
				half3 cwN = normalize(mul(ctN, tS));

        /// direct lighting
        half3 diffuse = half3(0, 0, 0);
        half3 irisSpec = half3(0, 0, 0);
				half3 corneaSpec = half3(0, 0, 0);

				float3 tD = getLightDir(i.wP, tS);
				UNITY_LIGHT_ATTENUATION(attenuation, i, i.wP);	

				evalEyeBrdf(tD, _LightColor0, tN, ctN, tV, irisRough, diffuse, irisSpec, corneaSpec);

        diffuse *= attenuation;
        irisSpec *= attenuation;
				corneaSpec *= attenuation;

        diffuse *= directDiffInt;
        irisSpec *= directSpecInt;
				corneaSpec *= directSpecInt;

				half3 colorMap = tex2D(colorSampler, uv).xyz;
				diffuse *= colorMap * (1 - _darken);

				irisSpec *= irisSpecInt * colorMap * irisMask * refrHeight * 5;

				corneaSpec *= evalFresnel(fresnel, ctN, tV);

				half sideDampen = min(1, pow(abs(dot(ctN, tV)), 10) * 10);
				// return fixed4(sideDampen, sideDampen, sideDampen, 1);
				corneaSpec *= sideDampen;

				return fixed4(diffuse + irisSpec + corneaSpec, 1.0);
			}
			ENDCG
		}

	}
	Fallback "Standard"
}

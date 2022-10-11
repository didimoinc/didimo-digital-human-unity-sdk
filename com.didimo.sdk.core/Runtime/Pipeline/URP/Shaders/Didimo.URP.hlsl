
/// NOTE: if using Unlit mode:
/// SSAO with depth normals doesn't work (keyword _SCREEN_SPACE_OCCLUSION doesn't seem to make a difference), but SSAO with just depth does
/// for shadows you need to add Boolean Keywords:
/// _MAIN_LIGHT_SHADOWS_CASCADE, Global Multi-Compile
/// _SHADOWS_SOFT, Global Multi-Compile
/// _ADDITIONAL_LIGHT_SHADOWS, Global Multi-Compile


#ifndef DIDIMO_INCLUDED
#define DIDIMO_INCLUDED

#include "../../Common/Didimo.Common.hlsl"
#ifndef SHADERGRAPH_PREVIEW 

half getMainShadow(float3 wP)
{
	float4 shadowCoord = TransformWorldToShadowCoord(wP);
	ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
	half4 shadowParams = GetMainLightShadowParams();
	return SampleShadowmap(TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture), shadowCoord, shadowSamplingData, shadowParams, false);    
}

#endif

void evalDiffuse(in half3 tN, in half3 tD, half sssAdd, in half3 lightColor, inout half3 diffuse, inout half3 psuedoSss, inout half3 transmission, in half shadowValue)
{
	half lambert = dot(tN, tD);
	//diffuse += lightColor * max(0, lambert);    
	psuedoSss += lightColor * clamp(lambert + sssAdd, 0, 1) * shadowValue;
	half tLambert = dot(-tN, tD) ;

	//float LdotN = dot(lTd, tN);
	//float rimDot = pow(max(0.0, 1.0 - dot(tEtoV, tN)), transmissionHaloSharpness);
	

	transmission += lightColor * max(0, pow(tLambert,3.0));
}

#define TRANSMAP_ADJUST_AS_ITS_TOO_BRIGHT_IN_THE_WRONG_PLACES
void PsuedoSSS_float(in half3x3 tS, in float3 wP, in float3 tN, in half4 ssUv, in half3 baseColor, in half3 sssColor, in half sssAdd, in half3 transColor, in half transMap, out half3 outColor)
{
#ifdef SHADERGRAPH_PREVIEW

	outColor = baseColor;

#else

	half3 diffuse = half3(0, 0, 0);
	half3 psuedoSss = half3(0, 0, 0);
	half3 transmission = half3(0, 0, 0);

	Light mainLight = GetMainLight();

	float3 tD = normalize(mul(tS, mainLight.direction));
	half ShadowAtten = getMainShadow(wP);
    #ifdef TRANSMAP_ADJUST_AS_ITS_TOO_BRIGHT_IN_THE_WRONG_PLACES
    transMap = max(0, transMap - 0.19);
    transMap = transMap * transMap;
    #endif

	evalDiffuse(tN, tD, sssAdd, mainLight.color * mainLight.distanceAttenuation , diffuse, psuedoSss, transmission, ShadowAtten);

	int pixelLightCount = GetAdditionalLightsCount();
	for (int i = 0; i < pixelLightCount; ++i)
	{
		Light light = GetAdditionalLight(i, wP);
		tD = normalize(mul(tS, light.direction));

		evalDiffuse(tN, tD, sssAdd, light.color * light.distanceAttenuation , diffuse, psuedoSss, transmission, light.shadowAttenuation);
	}
    
	half3 wN = normalize(mul(tN, tS));

	half3 indDiff = SampleSH(wN);
	psuedoSss += indDiff * 0.5 * sssAdd;

	psuedoSss *= sssColor * baseColor;

	half ao = lerp(1, SampleAmbientOcclusion(ssUv.xy), 0.5);
	psuedoSss *= ao;

	const half3 LuminanceVector = { 0.299f, 0.587f, 0.114f };
	half lum = min(dot(diffuse, LuminanceVector), 1);

	half lumSss = min(dot(psuedoSss, LuminanceVector), 1);

	transmission *= transColor * transMap * ao;

	// outColor = lerp(psuedoSss, lumSss, (lum + lumSss) * 0.5);
	outColor = lerp(psuedoSss, lumSss, lum) + transmission;

#endif
}

/*


float GetBlinnPhongIntensity(const in C_Ray ray, const in C_Material mat, const in vec3 vLightDir, const in vec3 vNormal)
{
	vec3 vHalf = normalize(vLightDir - ray.vDir);
	float fNdotH = max(0.0, dot(vHalf, vNormal));

	float shadowDot = max(0.0, dot(vNormal, vLightDir));

	float fSpecPower = exp2(mat.fSmoothness);
	float fSpecIntensity = fSpecPower * mat.fSpecularIntensity;

	fSpecIntensity *=  pow(fNdotH, fSpecPower);
	fSpecIntensity *= blinn_specular_energy_conservation(fSpecPower);
	//stop weird highlights but perhaps messes with fresnel
	#ifdef SHADOWBYDOT
	fSpecIntensity *= shadowDot;
	#endif
	return fSpecIntensity ;
}
*/



/*
def smoothFuncPlot(x, v):
	t = [v[0],v[2]]
	p = [v[1],v[3]]

	if x < t[0]:
		d = x / t[0]
		return smoothLerp(d, 0.0, p[0])
	elif x < t[1]:
		d = (x - t[0]) / (t[1]-t[0])
		return smoothLerp(d, p[0], p[1])
	elif x < 1.0:
		d = (x - t[1]) / (1.0-t[1])
		return smoothLerp(d, p[1], 1.0)
	else:
		return 1.0
*/



/*
void DeskinMatrix_half(in half4x4 skinMatrix, in int matrixIdx, out half4x4 invMatrix)
{
	invMatrix = _SkinMatrices[matrixIdx];
	invMatrix = transpose(invMatrix);
}*/



/*
#ifdef EYE_SHADOW_COLOUR

							in float3 eyeShadowColor,
#endif
*/

half evalBlinn(in half3 lightDir, in half3 normal, in half3 viewVec, in float Shadow, in float shininess = 5)
{
	half3 halfVec = normalize(lightDir + viewVec); // n.b. it's '+' in unity due to handiness
	//float dotNH = dot(normal, halfVec);
	float dotNH = max(0, dot(normal, halfVec));
	half specTerm = pow(dotNH, shininess);
	specTerm *= max(0, dot(lightDir, normal));
	return specTerm * Shadow;
}


float3 SampleNormal(in float2 uv, in half3x3 tS, in SamplerState ss, in Texture2D normalMapSmapler, in float normalMapIntensity)
{
	float3 normal_map_sample = UnpackNormal(normalMapSmapler.Sample(ss, uv));
	half3 normal_uv = normal_map_sample;
	half3 wsnormal = mul(normal_uv, tS);
	wsnormal = lerp(tS[2], wsnormal, normalMapIntensity);
	return normalize(wsnormal);
	//return normal_uv;
}


/*
radianceOcclusion = min(1.0, radianceOcclusion + scatter);
	albedo = mix(albedo, m_scatter_colour.rgb, scatterColourFactor);
*/


void Eye_float(in half3x3 tS,
	in float3 wV,
	in float3 wP,
	in float2 uv,
	in float2 distortUV,
	in half4 ssUv,
	in float refrHeight,
	in half3 baseColor,
	in Texture2D corneaNormalMap,
	in float corneaNormalStrength,
	in Texture2D irisNormalMap,
	in float irisNormalStrength,
	in float3 concaveNormal,
	in SamplerState ss,
	in float EyeShininess,
	in float SpecularIntensity,
	in float IrisShininess,
	in float IrisSpecularIntensity,
	in float3 sphericalWorldNormal,
	in float eyeLidao,
	in float aoStrength,
	in float scattering,
	in float envContribScale,
	out half3 outColor)
{
	const half3 ctN = half3(0, 0, 1);
#ifdef SHADERGRAPH_PREVIEW

	outColor = baseColor;

#else

	half3 itN = SampleNormal(distortUV, tS, ss, irisNormalMap, irisNormalStrength);
	half3 tN = SampleNormal(uv, tS, ss, corneaNormalMap, corneaNormalStrength);

	Light mainLight = GetMainLight();
	half ShadowAtten = getMainShadow(wP);
	wV = normalize(wV);

	float3 tV = mul(tS, wV);
	float3 tD = normalize(mul(tS, mainLight.direction));
	float3 wD = mainLight.direction;
	float highlightShadow = eyeLidao;

	float NdotL = dot(itN, tD);

	NdotL = (NdotL + scattering) / (1.0 + scattering);

	float3 diffuse = max(0.0, NdotL) * mainLight.color * mainLight.distanceAttenuation * ShadowAtten;

	float3 irisSpec = evalBlinn(tD, itN, tV, highlightShadow, IrisShininess) * mainLight.color * mainLight.distanceAttenuation * ShadowAtten;
	float3 corneaSpec = evalBlinn(wD, tN, wV, highlightShadow, EyeShininess) * mainLight.color * mainLight.distanceAttenuation * ShadowAtten;

	int pixelLightCount = GetAdditionalLightsCount();
	for (int i = 0; i < pixelLightCount; ++i)
	{
		Light light = GetAdditionalLight(i, wP);
		tD = normalize(mul(tS, light.direction));
		float NdotL = dot(itN, tD);
		NdotL = (NdotL + scattering) / (1.0 + scattering);
        float shadow = light.shadowAttenuation;
		diffuse += max(0.00, NdotL) * light.color * light.distanceAttenuation * shadow;
		irisSpec += evalBlinn(tD, itN, tV, highlightShadow, IrisShininess) * light.color * light.distanceAttenuation * shadow;
		corneaSpec += evalBlinn(light.direction, tN, wV, highlightShadow, EyeShininess) * light.color * light.distanceAttenuation * shadow;
	}

	irisSpec *= refrHeight * 4;

	half3 wN = normalize(mul(itN, tS));

	diffuse += SampleSH(wN);
	//baseColor *= 1 - _DidimoEyeDarken;
	diffuse *= baseColor;

	/// temper iris by the base color luminance
	const half3 LuminanceVector = { 0.299f, 0.587f, 0.114f };
	half invLum = 1 - dot(baseColor, LuminanceVector);
	irisSpec *= invLum;
	float3 totalSpec = max(float3(0.0, 0.0, 0.0), (corneaSpec * SpecularIntensity) + (irisSpec * IrisSpecularIntensity)); //

	outColor = diffuse + totalSpec;

	half ao = eyeLidao;// SampleAmbientOcclusion(ssUv.xy) *

	outColor = lerp(outColor, outColor * ao, aoStrength);

	/// reflection
	half3 reflDir = normalize(-reflect(tV, tN));
	reflDir = normalize(mul(reflDir, tS));
	half3 indirectSpec = GlossyEnvironmentReflection(reflDir, 0, 1) * ao * envContribScale;
	half f = 1.0 - abs(dot(ctN, tV));
	f *= f * f;
	half fresnel = lerp(1, f, 0.995);
	outColor += indirectSpec * fresnel;
#ifdef TEST_FRESNEL
	outColor = float3(f, f, f);
#endif
#endif
}

//#define TEST_AS_PHONG_BLINN
half evalKajiyaKay(half3 normal, half3 tangent, half3 halfVec, half roughness, half shadow)
{
	half  dotTH = dot(tangent, halfVec);
	
	half  dirAtten = smoothstep(-1.0, 0.0, dotTH);
#ifdef TEST_AS_PHONG_BLINN
	return max(0, pow(dotNH, roughness));
#else
	half  sinTH = sqrt(1 - dotTH * dotTH);

	return max(0, dirAtten * pow(sinTH, roughness)) * shadow;
#endif
}

float3 decodeNormal(float3 v)
{
	return 2.0 * (v - 0.5);
}

float3 decodeNormal2(float3 v)
{
	return normalize(2.0 * (v - 0.5));
}


/*

void evalDiffuse(in half3 tN, in half3 tD, half sssAdd, in half3 lightColor, inout half3 diffuse, inout half3 psuedoSss, inout half3 transmission)
{
	half lambert = dot(tN, tD);
	//diffuse += lightColor * max(0, lambert);
	psuedoSss += lightColor * clamp(lambert + sssAdd, 0, 1);
	half tLambert = dot(-tN, tD);   
	transmission += lightColor * max(0, tLambert * tLambert * tLambert);
}
*/


#define DOUBLE_SIDED_LIGHTING
float doLightDot(in float3 normal, in float3 lightdirection, in float rimDot, in float transmissionFactor)
{
	float dp = dot(normal, lightdirection);

#ifdef DOUBLE_SIDED_LIGHTING  	
	return  max(0, dp) + abs(min(0,dp)) * rimDot * transmissionFactor;
#else
	return max(0.0, dp);
#endif        
}

float3 calculateDiffuseColour(in float3 normal, in float scatter, in float transmissionFactor, float3 lightdirection, in float3 lightcol, in float rimDot)
{
	float inv_scatter = max(0.0, 1.0 - scatter);
	float dp = clamp((doLightDot(normal, lightdirection, rimDot, transmissionFactor) + scatter) * inv_scatter, 0.0, 1.0);

	float3 diffcol = lightcol;
	return diffcol * dp;
}

#define USE_FLOW_MAP
#define WORLD_SPACE_FLOW_MAP

void evalHairBrdf( in half3 lTd, in half3 lColor, in half3 tN, in half3 tT, in half3 tEtoV, 
				   in half roughness1, in half roughness2, in half specShift, in half specShift2, in half scatter, in half transmissionFactor, in half transmissionHaloSharpness,
				   inout half3 diffuse, inout half3 spec1, inout half3 spec2)
{	
	float LdotN = dot(lTd, tN);
	float rimDot = pow(max(0.0, 1.0 - dot(tEtoV, tN)), transmissionHaloSharpness);
	diffuse += calculateDiffuseColour(tN, scatter, transmissionFactor, lTd, lColor, rimDot);
	//max(0, LdotN)* lColor;
	float shadow = max(0, LdotN );
	
	half3 halfVec = normalize(lTd + tEtoV); 
	float specShiftAddition = 0.0;

	half3 T1 = normalize(tT - (tN * (specShift + specShiftAddition )));
	half3 T2 = normalize(tT - (tN * (specShift2+ specShiftAddition )));
	spec1 += max(0, evalKajiyaKay(tN, T1, halfVec, roughness1, shadow ) * lColor);
	spec2 += max(0, evalKajiyaKay(tN, T2, halfVec, roughness2, shadow ) * lColor);
}


void Hair_float( in half3x3 tS, in float3 wP, in float3 tP, in half3 tN, in half3 tT, in float3 tV, in float2 uv, 
				 in half3 baseColor, in half specExp1, in half specExp2, in half envRough, in half envSpecMul, 
				 in half specShift, in half specShift2, in half flowMultiply, in half specMultiply, in half rootTipPos, in float AO, in float SSSfactor, in float transmissionFactor, in float transmissionHaloSharpness,
				 out half3 outColor)
{
	half3 wN = normalize(mul(tN, tS));
	half3 wT = normalize(mul(tT, tS));
	//tT = -cross(tN, tT);
	
#ifdef SHADERGRAPH_PREVIEW

	outColor = baseColor;

#else
	float3 tEtoV = normalize(tV - tP);

	baseColor = min(baseColor, half3(1.0, 1.0, 1.0));
	half3 diffuse = half3(0, 0, 0);
	half3 spec1 = half3(0, 0, 0);
	half3 spec2 = half3(0, 0, 0);

	Light mainLight = GetMainLight();
	half ShadowAtten = getMainShadow(wP);

	half3 tD = normalize(mul(tS, mainLight.direction)); //light direction in tangent space
	half rough1 = specExp1;
	half rough2 = specExp2;


     evalHairBrdf(tD, (mainLight.color * mainLight.distanceAttenuation * ShadowAtten).rgb, tN, tT, tEtoV,
                      rough1, rough2, specShift , specShift2, SSSfactor, transmissionFactor, transmissionHaloSharpness,
				      diffuse, spec1, spec2);

	int pixelLightCount = GetAdditionalLightsCount();
	for (int i = 0; i < pixelLightCount; ++i)
	{
		Light light = GetAdditionalLight(i, wP);
		tD = normalize(mul(tS, light.direction));
		evalHairBrdf(tD, light.color * light.distanceAttenuation * light.shadowAttenuation, tN, tT, tEtoV, rough1, rough2, specShift, specShift2, SSSfactor, transmissionFactor, transmissionHaloSharpness,
							diffuse, spec1, spec2);
	}

	half3 indDiff = SampleSH(wN);
	diffuse += indDiff * 0.7;
	diffuse *= baseColor;

	//environmental reflection - N.B. non anisotropic!
#define HAIR_ENV_SPEC
#ifdef HAIR_ENV_SPEC


	const half3 ctN = half3(0, 0, 1);
	float3 t1 = normalize(wT * specShift);

	half3 reflDir = normalize(-reflect(tV, normalize(ctN + t1 * 0.05f) ));//
	//half3 reflDir = normalize(-reflect(tV, ctN));//
	reflDir = normalize(mul(reflDir, tS));
	half3 indirectSpec = GlossyEnvironmentReflection(reflDir,  envRough, 1.0) * envSpecMul;//wP.xyz,
	half f = clamp(1.0 - abs(dot(ctN, tV)), 0.0, 1.0);
	f *= f * f;
	half fresnel = lerp(1, f, 0.995);
 
	spec1 += indirectSpec * fresnel;

#endif

	const half3 LuminanceVector = { 0.299f, 0.587f, 0.114f };
	half lum = min(dot(baseColor , LuminanceVector), 1);
	half3 lumCol = half3(lum, lum, lum);

	spec1 *= lumCol;
	spec2 *= baseColor;

	half rootDampen = min(lerp(0, 3, uv.y), 1);

#ifdef TEST_NORMALS
	outColor = lerp(perturbedFlow, diffuse + (spec1 + spec2) * specMultiply, 0.1);
#else
	outColor = (diffuse + (spec1 + spec2) * specMultiply) * AO;
#endif
#endif

	

}

void Background_float(in half3x3 tS, in float3 wP, in float3 tN, in half3 baseColor, in half3 globalIllum, out half3 outColor)
{
#ifdef SHADERGRAPH_PREVIEW

	outColor = baseColor;

#else

	half3 diffuse = half3(0, 0, 0);

	Light mainLight = GetMainLight();

	float3 tD = normalize(mul(tS, mainLight.direction));

	// half ShadowAtten = getMainShadow(wP);

	diffuse = max(0, dot(tN, tD)) * mainLight.color * mainLight.distanceAttenuation;// * ShadowAtten;

	int pixelLightCount = GetAdditionalLightsCount();
	for (int i = 0; i < pixelLightCount; ++i)
	{
		Light light = GetAdditionalLight(i, wP);
		tD = normalize(mul(tS, light.direction));

		diffuse += max(0, dot(tN, tD)) * light.color * light.distanceAttenuation;// * light.shadowAttenuation;
	}

	half3 wN = normalize(mul(tN, tS));

	diffuse += globalIllum;

	diffuse *= baseColor;

	outColor = diffuse;

#endif
}

void Ssao_half(in half4 ssUv, out half outAo)
{
#ifdef SHADERGRAPH_PREVIEW
	outAo = 1;
#else
	outAo = SampleAmbientOcclusion(ssUv.xy);
#endif
}

#endif // DIDIMO_INCLUDED

// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers d3d11 gles
#define UNITY_SAMPLE_FULL_SH_PER_PIXEL 1

#include "UnityCG.cginc"
#include "UnityStandardUtils.cginc"

/// exposed uniforms
half directDiffInt;
half indirectDiffInt;
half directSpecInt;
half indirectSpecInt;

// /// for testing
// samplerCUBE irradCube; 
// samplerCUBE radCube; 

float remapTo01(float v, float fMin, float fMax)
{
  return clamp((v - fMin) / (fMax - fMin), 0, 1);
}

/// returns tangent space light direction
float3 getLightDir(float3 wP, float3x3 tS)
{
  half3 tD;
  if(_WorldSpaceLightPos0.w <= 0) /// directional light
  {
    tD = normalize(mul(tS, _WorldSpaceLightPos0.xyz));
  } 
  else // point or spot light
  {
    float3 lD = _WorldSpaceLightPos0.xyz - wP.xyz;
    tD = normalize(mul(tS, lD));
  }
  return tD;
}


///
/// indirect
///
half3 evalIndDiffuse(half3 wN, float3 wP)
{
  // wN *= half3(-1, 1, 1); /// flip it to match Maya
  half3 indirectDiff = ShadeSHPerPixel(wN, 0, wP);
  
  return indirectDiff.rgb * indirectDiffInt; 
}

half3 evalIndSpec(half3 wN, half3 wV, half roughness)
{
  half3 reflDir = normalize(-reflect(wV, wN));
  // reflDir *= half3(-1, 1, 1); /// flip it to match Maya
  half dotDown = 1 - max(0, dot(wN, half3(0, -1, 0)));

  half4 radSample = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflDir, lerp(0, 9, roughness));
  half3 indirectSpec = DecodeHDR(radSample, unity_SpecCube0_HDR);

  return indirectSpec * indirectSpecInt * dotDown;
}


/// common brdfs
half evalLambert(half3 tlD, half3 tN)
{
  return max(0, dot(tlD, tN));
}

half evalPhong(half3 tlD, half3 tN, half3 tV, fixed roughness)
{
  half3 reflVec = normalize(reflect(-tlD, tN));
  float dotRV = max(0, dot(reflVec, tV));
  return pow(dotRV, (1 - roughness) * 100);
}

half evalBlinn(half3 tlD, half3 tN, half3 tV, fixed roughness)
{
  half3 halfVec = normalize(tlD + tV);
  float dotNH = max(0, dot(tN, halfVec));
  half specTerm = pow(dotNH, max(0.01, (1 - roughness)) * 100);
  return specTerm;
}

half evalAniso(half3 tlD, half3 tN, half3 tV, fixed roughness, fixed angle, fixed minTerm)
{
  fixed sX = angle;
  fixed sY = 1.0 - angle;

  half3 halfVec = normalize(tlD + tV);

  half3 upVec = half3(0, 1, 0);

  half3 B  = normalize(cross(upVec, tN));
  half3 N  = normalize(tN) * sX ;
  half3 T  = cross(N, B) / sY; 

  half dotTH = dot(T, halfVec);
  half dotBH = dot(B, halfVec);
  half dotNH = max(0, dot(N, halfVec));

  half specTerm = dotNH * dotNH / (dotTH * dotTH + dotBH * dotBH);
  
  roughness = max(0.0001, 1 - roughness);

  specTerm = pow(specTerm, roughness);

  specTerm = max((specTerm - minTerm) * dot(tlD, tN), 0);

  return specTerm; 
}

float evalFresnel(float amount, half3 tN, half3 tV)
{

    float f = 1.0 - abs(dot(tN, tV));
    float f2 = f * f;
    return max(0, lerp(1.0, f2 * f2 * f, amount));
}


#ifdef DIDIMO_SKIN
  half evalSkinSpec(half3 tlD, half3 tN, half3 tV, fixed roughness)
  {
    half3 halfVec = normalize(tlD + tV);
    float dotNH = max(0, dot(tN, halfVec));
    half specTerm = 0;
    specTerm += pow(dotNH, (1 - roughness) * 70); // * 0.5;
    specTerm += pow(dotNH, (1 - roughness) * 40); // * 0.5;
    return specTerm;
  }
#endif

#ifdef DIDIMO_EYE
  void evalEyeBrdf(in half3 lTd, in half3 lColor, in half3 tN, in half3 ctN, in half3 tV, in fixed irisRough, inout fixed3 diffuse, inout half3 irisSpec, inout half3 corneaSpec)
  {
    diffuse += evalLambert(lTd, tN) * lColor;

    // half3 halfVec = normalize(lTd + tV);
    // float dotNH = max(0, dot(tN, halfVec));
    // irisSpec = pow(dotNH, 0.01);
    irisSpec += evalBlinn(lTd, tN, tV, irisRough) * lColor;

    // corneaSpec += evalAniso(lTd, ctN, tV, 0.001, 0.5, 1) * lColor;
  }
#endif

#ifdef DIDIMO_HAIR
  half evalKajiyaKay(half3 tangent, half3 halfVec, half roughness)
  {
    half  dotTH    = dot(tangent, halfVec);
    half  sinTH    = sqrt(1 - dotTH * dotTH);
    half  dirAtten = smoothstep(-1.0, 0.0, dotTH);
    return max(0, dirAtten * pow(sinTH, roughness));
  }

  void evalHairBrdf(in half3 lTd, in half3 lColor, in half3 tN, in half3 tV, in fixed roughness1, in fixed roughness2, in fixed specShift, inout fixed3 diffuse, inout half3 spec1, inout half3 spec2)
  {
    diffuse += max(0, evalLambert(lTd, tN) * lColor);

    const half3 tB = normalize(half3(0, 1, 0));
    half3 halfVec = normalize(lTd + tV); 

    half primaryShift   = specShift - 0.25;
    half secondaryShift = primaryShift - clamp(dot(lTd, tB), 0, 1) * 0.25;
    
    half3 T1 = normalize(tB + tN * primaryShift);
    half3 T2 = normalize(tB + tN * secondaryShift);
        
    spec1 += max(0, evalKajiyaKay(T1, halfVec, roughness1) * lColor);
    spec2 += max(0, evalKajiyaKay(T2, halfVec, roughness2) * lColor);
  }
#endif


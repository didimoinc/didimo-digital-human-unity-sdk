
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members vertexLighting)
#pragma exclude_renderers d3d11
#include "UnityCG.cginc"
#include "AutoLight.cginc"

/// uniforms
float zBias = 0;

sampler2D zBiasSampler;

/// tangent (uv) space
half3 getTsNormal(sampler2D normalMap, half2 uv, half height)
{
  half3 tN = tex2D(normalMap, uv).xyz;
  tN = (tN * 2.0 - 1.0);
  tN *= height;
  tN.z += 1.0 - height;
  tN = normalize(tN);
  return tN;
}

struct appdata
{
  float4 vertex : POSITION;  /// object space position
  float2 uv : TEXCOORD0; /// texture space uv
  float4 oT : TANGENT;   /// object space tangent
  float3 oN : NORMAL;    /// object space normal
};

struct v2f
{
  float4 pos : SV_POSITION; /// clip space vertex
  float2 uv : TEXCOORD0;   /// texture space uv
};

v2f didimoVert(appdata v) /// simple
{
  v2f o;
  o.uv = v.uv;
  float3 wP = mul(unity_ObjectToWorld, v.vertex);
  o.pos = mul(UNITY_MATRIX_VP, float4(wP, 1));
  return o;
}

/// forward base
struct v2fbase
{
  float4 pos : SV_POSITION; /// clip space vertex
  float2 uv : TEXCOORD0;   /// texture space uv
  float3 wT : TANGENT;     /// world space tangent
  float3 wB : TEXCOORD1;   /// world space binormal
  float3 wN : NORMAL;      /// world space normal
  float3 wV : TEXCOORD2;   /// world space view
  float3 wP : TEXCOORD3;   /// world space position
  
  LIGHTING_COORDS(4,6)

  #ifdef VERTEXLIGHT_ON
    float3 vertexLighting : TEXCOORD7;
  #endif
};

v2fbase didimoVertFwdBase(appdata v) 
{
  v2fbase o;
  
  o.uv = v.uv;

  // /// debug: flip handedness for comparing with maya viewport shaders
  // float3 oT = normalize(float3(-v.oT.x, v.oT.y, v.oT.z)); 
  // float3 oB = cross(v.oN, v.oT.xyz) * v.oT.w;
  // oB = normalize(float3(-oB.x, oB.y, oB.z));
  // float3 oN = normalize(float3(-v.oN.x, v.oN.y, v.oN.z));

  float3 oT = v.oT;
  float3 oB = cross(v.oN, v.oT.xyz) * v.oT.w;
  float3 oN = v.oN;

  float4x4 wIt = transpose(unity_WorldToObject); 
  o.wT = mul(wIt, oT);
  o.wB = mul(wIt, oB);
  o.wN = mul(wIt, oN);

  o.wP = mul(unity_ObjectToWorld, v.vertex);
  o.wV = normalize(_WorldSpaceCameraPos - o.wP);

  #ifdef VERTEXLIGHT_ON
    for(int index = 0; index < 4; index++)
    {  
      o.vertexLighting = Shade4PointLights(unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                unity_4LightAtten0, o.wP, o.wN);
    }
  #endif

  float zBiasMap = tex2Dlod(zBiasSampler, float4(v.uv, 0, 0)).x;
  o.pos = mul(UNITY_MATRIX_VP, float4(o.wP + o.wV * zBias * zBiasMap, 1));

  TRANSFER_VERTEX_TO_FRAGMENT(o);

  return o;
}

/// forward add
struct v2fadd
{
  float4 pos : SV_POSITION; /// clip space vertex
  float2 uv : TEXCOORD0;   /// texture space uv
  float3 wT : TANGENT;     /// world space tangent
  float3 wB : TEXCOORD1;   /// world space binormal
  float3 wN : NORMAL;      /// world space normal
  float3 wV : TEXCOORD2;   /// world space view
  float3 wP : TEXCOORD3;   /// world space position
  
  LIGHTING_COORDS(4,6)
};

v2fadd didimoVertFwdAdd(appdata v)
{
  v2fadd o;
  
  o.uv = v.uv;

  float3 oT = v.oT;
  float3 oB = cross(v.oN, v.oT.xyz) * v.oT.w;
  float3 oN = v.oN;

  float4x4 wIt = transpose(unity_WorldToObject); 
  o.wT = mul(wIt, oT);
  o.wB = mul(wIt, oB);
  o.wN = mul(wIt, oN);

  o.wP = mul(unity_ObjectToWorld, v.vertex);
  o.wV = normalize(_WorldSpaceCameraPos - o.wP);

  float zBiasMap = tex2Dlod(zBiasSampler, float4(v.uv, 0, 0)).x;
  o.pos = mul(UNITY_MATRIX_VP, float4(o.wP + o.wV * zBias * zBiasMap, 1));

  TRANSFER_VERTEX_TO_FRAGMENT(o);

  return o;
}

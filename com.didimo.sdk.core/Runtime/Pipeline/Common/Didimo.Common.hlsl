
#ifndef DIDIMO_COMMON_INCLUDED
#define DIDIMO_COMMON_INCLUDED


void VertexSphereUnion_float(in float3 pos, in float3 sc, in float sr, out float3 opos)
{
	float3 spos = pos - sc;
	float l = length(spos);
	if (l < sr)
	{
		float3 n = spos / l;
		opos = sc + n * sr;
		return;
	}
	opos = pos;
}


float2 hash2( float2 p )
{        
    return frac(sin(float2(dot(p,float2(127.1,311.7)),dot(p,float2(269.5,183.3))))*43758.5453);
}

float GenerateRandom(float3 input)
{
    float3 prime1 = float3(419.0,853.0,1009.0);
    float3 prime2 = float3(2243.0,503.0,2053.0);
    return frac(sin(dot(input, prime1) * dot(input, prime2) )*43758.5453);
}


void LODFade_float(out float LOD)
{
    LOD = 0;
}

void GenerateRandomFloat_float(float3 input, out float ret)
{
    ret = GenerateRandom(input);
}

float2 RotateZ(const in float2 vPos, const in float fAngle)
{
	const float DEG_TO_RAD = 0.01745329251;
	const float fAngleRad = fAngle * DEG_TO_RAD;
	float s = sin(fAngleRad);
	float c = cos(fAngleRad);

	return float2(c * vPos.x + s * vPos.y, -s * vPos.x + c * vPos.y);
}

float distanceToElipse(in float2 pt, in float2 c, in float2 r, in float rot)
{
	float2 rpt = RotateZ(pt - c, rot);
	return sqrt((rpt.x * rpt.x) / (r.x * r.x) + (rpt.y * rpt.y) / (r.y * r.y));
}

float distanceToSuperEllipse(in float2 pt, in float2 c, in float2 r, in float rot, in float shapePow)
{
	float2 rpt = RotateZ(pt - c, rot);
	return sqrt(pow(rpt.x * rpt.x, shapePow) / (r.x * r.x) + pow(rpt.y * rpt.y, shapePow) / (r.y * r.y));
}



void decodePackedVector_float(in float3 v, out float3 outV)
{
	outV = (v + float3(1.0, 1.0, 1.0)) * 0.5;
}


void gauss_float(in float x, in float a, in float b, float c, out float res)
{
	res = (1.0f / a * sqrt(2.0f * PI)) * exp(-0.5f * (((c - b) * (x - b)) / (a * a)));
}

void multiChoice_float(int choice, in float4 a, in float4 b, in float4 c, in float4 d, out float4 r)
{
	switch (choice)
	{
	case 0:r = a; break;
	case 1:r = b; break;
	case 2:r = c; break;
	default:r = d; break;
	}
}




void multi_or_float(in bool A, in  bool B, in  bool C, out bool outval)
{
    outval = A || B || C;
}

void multi_or_half(in bool A, in  bool B, in  bool C, out bool outval)
{
    outval = A || B || C;
}

void diffuse_or_emission(bool ChooseDiffuse, in float3 Diffuse, in float3 Emission, out float3 ODiffuse, out float3 OEmission)
{
    if (ChooseDiffuse)
    {
        ODiffuse = Diffuse;
        OEmission = float3(0.0, 0.0, 0.0);
    }
    else
    {
        ODiffuse = Emission;//float3(0.0,0.0,0.0);
        OEmission = Emission;
    }
}

void diffuse_or_emission_float(bool choice, in float3 Diffuse, in float3 Emission, out float3 ODiffuse, out float3 OEmission)
{
    diffuse_or_emission(choice, Diffuse, Emission, ODiffuse, OEmission);
}

void diffuse_or_emission_half(bool choice, in float3 Diffuse, in float3 Emission, out float3 ODiffuse, out float3 OEmission)
{
    diffuse_or_emission(choice, Diffuse, Emission, ODiffuse, OEmission);
}


void Quadratic_float(in float4 x, in float a, in float b, in float c, out float4 res)
{
	res = (a * x * x) + (b * x) + c;
}



float smoothLerp(in float x, in float a, in float b)
{
	float v = x * x * (3.0 - 2.0 * x);
	return a + (b - a) * v;
}

#define TRANSPOSE_MATRIX_CONVERT
#ifdef TRANSPOSE_MATRIX_CONVERT
void convertMat4ToMat3_float(in float4x4 mat, out float3x3 outmat)
{
	outmat = float3x3(mat[0][0], mat[0][1], mat[0][2],
					  mat[1][0], mat[1][1], mat[1][2],
					  mat[2][0], mat[2][1], mat[2][2]);
}

void convertMat4ToMat3_half(half3x3 mat, out half3x3 outmat)
{
	outmat = half3x3 (mat[0][0], mat[0][1], mat[0][2],
					mat[1][0], mat[1][1], mat[1][2],
					mat[2][0], mat[2][1], mat[2][2]);
}
#else
void convertMat4ToMat3_float(in float4x4 mat, out float3x3 outmat)
{
	outmat = float3x3(mat[0][0], mat[1][0], mat[2][0],
					  mat[0][1], mat[1][1], mat[2][1],
					  mat[0][2], mat[1][2], mat[2][2]);
}

void convertMat4ToMat3_half(half3x3 mat, out half3x3 outmat)
{
	outmat = half3x3 (mat[0][0], mat[1][0], mat[2][0],
		mat[0][1], mat[1][1], mat[2][1],
		mat[0][2], mat[1][2], mat[2][2]);
}
#endif

float gaussCurve(float x, float a, float b, float c)
{
    float e = 2.71828;
    float x_b = x - b;
    x_b *= x_b;
    float _2c2 = 2.0 * c * c;

    return a * pow(e, -(x_b / _2c2));
}


void Gauss_half(in float4 x, in float a, in float b, in float c, out float4 res)
{
    float4 xb = x - float4(b, b, b, b);
    res = exp(xb);// a* exp((xb * xb) / (2 * c * c));	
}

float innerfunc(in float x, in  float a, in  float b, in  float c)
{
    return pow(x, a + (x + b) * c);
}

float Sigmoid(in float x, in float a, in float b, in float c)
{
    return (1 / 1 + exp(-x * a - c) + b);
}

void Sigmoid_half(in float4 x, in float a, in float b, in float c, out float4 res)
{
    res = float4(Sigmoid(x.x, a, b, c), Sigmoid(x.y, a, b, c), Sigmoid(x.z, a, b, c), Sigmoid(x.w, a, b, c));
}


void PowCurveTwoStep_half(in float4 x, in float a, in float b, in float c, out float4 res)
{
    //+\left(y+a\right)\cdot d
    res = float4(pow(x.x, a + (x.x + b) * c),
        pow(x.y, a + (x.y + b) * c),
        pow(x.z, a + (x.z + b) * c),
        pow(x.w, a + (x.w + b) * c));

    //res = pow(x, b);
}

void CentredScale_half(in float2 uv, in float2 centre, in float2 scale, out float2 result)
{
    result = ((uv - centre) * scale) + centre;
}


float ScatterDiffuse(in float NdotL, float scatter)
{
    //m_scatter_colour.rgb 
    return (1.0 - pow(NdotL, 0.5)) * scatter;
}


void EyeAOCalculations_float(in float2 uv,
    in float2 centre,
    in float2 radii,
    in float rotation,
    in float radPow,
    in float shapePow,
    out float AO)
{
    AO = pow(clamp((distanceToSuperEllipse(uv, centre, radii, rotation, shapePow)), 0.0, 1.0), radPow);
    //AO = pow(clamp((distanceToEllipse(uv, centre, radii, rotation)), 0.0, 1.0), radPow);
}


void EyeAOBezierCalculations_float(in float4x4 parameters,
    in float radPow,
    out float AO)
{

    AO = 1.0;

}


////////////////////////////////////////////////////////////////
//Eye specific stuff
////////////////////////////////////////////////////////////////


//takes input from 0..1 and reforms it in to an envelope between x = 0..t[0]..t[1]..1 with output values in range 0..p[0]..p[1]..1
//TODO: replace this curve with a texture lookup perhaps
float linearStepedLerp(in float x, in float3 t, in float3 p)
{
	if (x < t[0])
	{
		float d = x / t[0];
		return (d * (p[0]));
	}
	else if (x < t[1])
	{
		float d = (x - t[0]) / (t[1] - t[0]);
		return lerp(p[0], p[1], d);
	}
	else if (x < t[2])
	{
		float d = (x - t[1]) / (t[2] - t[1]);
		return lerp(p[1], p[2], d);
	}
	else if (x < 1.0)
	{
		float d = (x - t[2]) / (1.0 - t[2]);
		return lerp(p[2], 1.0, d);
	}
	else
		return x;
}

void EyeBaseUVAdjust_float(in float2 uv, in float3 t, float3 p, out float2 res2)
{
	//+\left(y+a\right)\cdot d
	float2 abvec = uv - float2(0.5, 0.5);
	float dist = length(abvec);
	abvec /= dist;
	//float fval = gaussCurve(1.0 - dist, a, b, c);
	float fval = linearStepedLerp(dist, t, p);

	res2 = float2(0.5, 0.5) + abvec * fval;
	//res2 = uv;
}


void PrepareEyeUv_float(in float3 tV, in float2 uv, in float eyeRefracion, out float2 refrUv, out float refrHeight, out float3 concaveNormal) {
	const half3 ctN = half3(0, 0, 1);

#ifdef SHADERGRAPH_PREVIEW

	concaveNormal = ctN;
	refrUv = uv;
	refrHeight = 0;

#else

	const float radius = 0.12;
	const half2 center = half2(0.5, 0.5);

	half3 diffuse = half3(0, 0, 0);
	half3 irisSpec = half3(0, 0, 0);

	float irisMask = 0;
	refrHeight = 0;

	concaveNormal = ctN;

	half fromCenterX = uv.x - center.x;
	fromCenterX *= fromCenterX;

	half fromCenterY = uv.y - center.y;
	fromCenterY *= fromCenterY;

	if (fromCenterX + fromCenterY < radius * radius) /// inside circle test
	{
		refrHeight = 1 - (length(uv - center) / radius);
		irisMask = 1;

		half2 newXy = center - uv;

		half3 _concaveNormal = normalize(half3(newXy / radius, 0.5));
		concaveNormal = normalize(lerp(concaveNormal, _concaveNormal, refrHeight * 15));
	}

	tV = normalize(tV);

	// refrHeight *= refrHeight;

	half2 offset = tV.xy * tV.z * refrHeight * eyeRefracion * 0.4;

	refrUv = uv - offset;
#endif

}


//#define USE_dFD
void calcSDFAlpha_half(in float2 uv, in float dist, in float cutoff, in float smoothing, in float antialiasfactor, in float gamma, out float res)
{
#ifdef USE_dFD
    float2 dpdx = ddx(uv);
    float2 dpdy = ddy(uv);
    float m = length(float2(length(dpdx), length(dpdy)));
#else
    float m = 0.4;

#endif
    antialiasfactor = 1.0;
    float smoothval = max(smoothing, pow(abs(m) * antialiasfactor, gamma));

    //float distanceChange = m_derivative * 0.5; //fwidth(dist)
    float antialiasedCutoff = smoothstep(cutoff - smoothval, cutoff + smoothval, dist);

    res = antialiasedCutoff;
}

void calcSDFAlpha_float(in float2 uv, in float dist, in float cutoff, in float smoothing, in float antialiasfactor, in float gamma, out float res)
{
#ifdef USE_dFD
    float2 dpdx = ddx(uv);
    float2 dpdy = ddy(uv);
    float m = length(float2(length(dpdx), length(dpdy)));
#else
    float m = 0.4;

#endif

    float smoothval = max(smoothing, pow(abs(m) * antialiasfactor, gamma));

    //float distanceChange = m_derivative * 0.5; //fwidth(dist)
    float antialiasedCutoff = smoothstep(cutoff - smoothval, cutoff + smoothval, dist);
    res = antialiasedCutoff;
}

void MatrixRotateXYZ_float(in float3 o, out float4x4 m)
{
    float crx = cos(o.x);
    float srx = sin(o.x);
    float cry = cos(o.y);
    float sry = sin(o.y);
    float crz = cos(o.z);
    float srz = sin(o.z);
    //this may have 'flipped' y rotation - need to establish 'cannon' here. 
    //RotateY and SetRotateY are flipped respective to each other 	
    m[0][0] = crz * cry;    m[0][1] = crz * -sry * -srx + srz * crx;        m[0][2] = crz * -sry * crx + srz * srx;          m[0][3] = 0;
    m[1][0] = -srz * cry;   m[1][1] = -srz * -sry * -srx + crz * crx;       m[1][2] = -srz * -sry * crx + crz * srx;         m[1][3] = 0;
    m[2][0] = sry;          m[2][1] = cry * -srx;                           m[2][2] = cry * crx;                             m[2][3] = 0;
    m[3][0] = 0;            m[3][1] = 0;                                    m[3][2] = 0;                                     m[3][3] = 1;
}

void MatrixRotateXYZ_half(in float3 o, out half4x4 m)
{
    float crx = cos(o.x);
    float srx = sin(o.x);
    float cry = cos(o.y);
    float sry = sin(o.y);
    float crz = cos(o.z);
    float srz = sin(o.z);
    //this may have 'flipped' y rotation - need to establish 'cannon' here. 
    //RotateY and SetRotateY are flipped respective to each other 	
    m[0][0] = crz * cry;    m[0][1] = crz * -sry * -srx + srz * crx;        m[0][2] = crz * -sry * crx + srz * srx;          m[0][3] = 0;
    m[1][0] = -srz * cry;   m[1][1] = -srz * -sry * -srx + crz * crx;       m[1][2] = -srz * -sry * crx + crz * srx;         m[1][3] = 0;
    m[2][0] = sry;          m[2][1] = cry * -srx;                           m[2][2] = cry * crx;                             m[2][3] = 0;
    m[3][0] = 0;            m[3][1] = 0;                                    m[3][2] = 0;                                     m[3][3] = 1;
}



void RefracionDirection_float(in float internalIoR, in float3 normalW, in float3 cameraW, out float3 output)
{
    float airIoR = 1.00029;
    float n = airIoR / internalIoR;
    float facing = dot(normalW, cameraW);
    float w = n * facing;
    float k = sqrt(1 + (w - n) * (w + n));
    output = -normalize((w - k) * normalW - n * cameraW);
}



void IrisUVMask_float(in float IrisUVRadius, in float2 UV, in float3 LimbusUVWidth, out float3 m)
{
    // Iris Mask with Limbus Ring falloff
    UV = UV - float2(0.5f, 0.5f);

    float3 r;
    float uvl = length(UV);
    r = (float3(uvl, uvl, uvl) - (float3(IrisUVRadius, IrisUVRadius, IrisUVRadius) - LimbusUVWidth)) / LimbusUVWidth;
    m = saturate(float3(1.0, 1.0, 1.0) - r);
    m = smoothstep(0, 1, m);
}


float2 DeriveTangents(in float3 EyeDirectionWorld, in float3 TangentBasisX, in float3 ScaledRefracionDirection)
{
    float dp = dot(TangentBasisX, EyeDirectionWorld);
    float3 scaleEDW = EyeDirectionWorld * dp;
    float3 TD2 = normalize(TangentBasisX - scaleEDW);

    float3 cp = cross(TD2, EyeDirectionWorld);

    return float2(dot(ScaledRefracionDirection, TD2), dot(cp, ScaledRefracionDirection));
}

void ScalePupils_float(in float2 UV, float PupilScale, float2 PupilShift, out float2 UVscaled)
{
    // Scale UVs from from unit circle in or out from center
// float2 UV, float PupilScale

    float ShiftMask = pow(saturate(2.0 * ((distance(float2(0.5, 0.5), UV) - 0.45) / -0.5)), 0.7);
    PupilShift.x *= ShiftMask * -0.1;
    PupilShift.y *= ShiftMask * 0.1;
    float2 UVshifted = UV + float2(PupilShift.x, PupilShift.y);
    float2 UVcentered = UVshifted - float2(0.5, 0.5);
    float UVlength = length(UVcentered);
    // UV on circle at distance 0.5 from the center, in direction of original UV
    float2 UVmax = normalize(UVcentered) * 0.5;

    UVscaled = lerp(UVmax, float2(0.0, 0.0), saturate((1.0 - UVlength * 2.0) * PupilScale)) + float2(0.5, 0.5);
}

void SphereMask_float(in float2 input, in float2 centre, in float radius, in float hardness, out float result)
{
    float d = distance(input, centre);
    result = ((d - radius) - 1.0) / (hardness - 1);
}

void EyeRefracion_float(in float2 UV, in float DepthPlaneOffset, in float3 MidPlaneDisplacement, 
                         in float InputDepthScale, in float3 camVec, in float3 PixelNormalWS, 
                         in float3 EyeDirectionWorld, in float InternalIoR, in float3 LimbusUVWidth, 
                         in float3 TangentBasisX, float IrisUVRadius, 
                         out float2 OutputRefracedUV, out float OutputTransparency, out float3 OutputIrisUVMask)
{
    float3 offset = max(MidPlaneDisplacement, float3(0.0, 0.0, 0.0)) * InputDepthScale;

    float camDot = dot(camVec, EyeDirectionWorld);
    float3 ScaledRefracedOffsetDirection = offset / lerp(0.325, 1.0, camDot * camDot);
    float2 c1 = IrisUVRadius * float2(-1.0, 1.0);
    float2 UVIrisScaled = c1 * DeriveTangents(EyeDirectionWorld, TangentBasisX, ScaledRefracedOffsetDirection);
    OutputTransparency = length(UVIrisScaled) - IrisUVRadius;
    IrisUVMask_float(IrisUVRadius, UV, LimbusUVWidth, OutputIrisUVMask);
    OutputRefracedUV = lerp(UVIrisScaled + UV, UV, OutputIrisUVMask.x);
}

#endif
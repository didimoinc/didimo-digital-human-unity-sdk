
#ifndef DIDIMO_HLSL_INCLUDED
#define DIDIMO_HLSL_INCLUDED

#define TAP_COUNT  8
#define HALF_TAP_COUNT 4
#define TOTAL_TAP_COUNT  (TAP_COUNT * TAP_COUNT)



void MultiTapUVGenerator3x3(in float2 uv, in float TS,
    out float2 uv1, out float2 uv2, out float2 uv3,
    out float2 uv4, out float2 uv5, out float2 uv6,
    out float2 uv7, out float2 uv8, out float2 uv9    
)
{
    float y = uv.y - TS;
    uv1 = float2(uv.x - TS, y);
    uv2 = float2(uv.x, y);
    uv3 = float2(uv.x + TS, y);    
    uv4 = float2(uv.x - TS, uv.y);
    uv5 = float2(uv.x, uv.y);
    uv6 = float2(uv.x + TS, uv.y);
    y = uv.y + TS;
    uv7 = float2(uv.x - TS, y); 
    uv8 = float2(uv.x, y);
    uv9 = float2(uv.x + TS, y);        
}

void CombineAverage5x5_float(
    in float4 m00, in float4 m01, in float4 m02, in float4 m03, in float4 m04,
    in float4 m10, in float4 m11, in float4 m12, in float4 m13, in float4 m14,
    in float4 m20, in float4 m21, in float4 m22, in float4 m23, in float4 m24,
    in float4 m30, in float4 m31, in float4 m32, in float4 m33, in float4 m34,
    in float4 m40, in float4 m41, in float4 m42, in float4 m43, in float4 m44,
    out float4 result)
{
    result = (m00 + m01 + m02 + m03 + m04 +
        m10 + m11 + m12 + m13 + m14 +
        m20 + m21 + m22 + m23 + m24 +
        m30 + m31 + m32 + m33 + m34 +
        m40 + m41 + m42 + m43 + m44) / 25.0;
}


void MultiTapUVGenerator5x5_float(in float2 uv, in float TS, 
                            out float2 uv1, out float2 uv2, out float2 uv3, out float2 uv4, out float2 uv5, 
                            out float2 uv6, out float2 uv7, out float2 uv8 , out float2 uv9, out float2 uv10,
                            out float2 uv11, out float2 uv12, out float2 uv13, out float2 uv14, out float2 uv15, 
                            out float2 uv16, out float2 uv17, out float2 uv18, out float2 uv19, out float2 uv20,
                            out float2 uv21, out float2 uv22, out float2 uv23, out float2 uv24, out float2 uv25)
{
    float TS2 = TS * 2.0;
    float y =  uv.y - TS2;
    uv1 = float2(uv.x - TS2, y);
    uv2 = float2(uv.x - TS, y);
    uv3 = float2(uv.x, y);
    uv4 = float2(uv.x + TS, y);
    uv5 = float2(uv.x + TS2, y);
    y = uv.y - TS;
    uv6 = float2(uv.x - TS2, y);
    uv7 = float2(uv.x - TS, y);
    uv8 = float2(uv.x, y);
    uv9 = float2(uv.x + TS, y);
    uv10 = float2(uv.x + TS2, y);
    y = uv.y;
    uv11 = float2(uv.x - TS2, y);
    uv12 = float2(uv.x - TS, y);
    uv13 = float2(uv.x, y);
    uv14 = float2(uv.x + TS, y);
    uv15 = float2(uv.x + TS2, y);
    y = uv.y + TS;
    uv16 = float2(uv.x - TS2, y);
    uv17 = float2(uv.x - TS, y);
    uv18 = float2(uv.x, y);
    uv19 = float2(uv.x + TS, y);
    uv20 = float2(uv.x + TS2, y);
    y = uv.y + TS2;
    uv21 = float2(uv.x - TS2, y);
    uv22 = float2(uv.x - TS, y);
    uv23 = float2(uv.x, y);
    uv24 = float2(uv.x + TS, y);
    uv25 = float2(uv.x + TS2, y);
}

void MutliTapScreen_float(Texture2D tex, SamplerState SS, in float2 uv, in float TapSize, out float4 OutColour)
{       
    float3 col = float4(0.0, 0.0, 0.0, 0.0);
    //col += tex.Sample(SS, uv);
    //col += shadergraph_SampleSceneColor(uv);
    
    float2 tl = uv - float2(TapSize * HALF_TAP_COUNT, TapSize * HALF_TAP_COUNT);
    float2 cuv = tl;
    
    for (int y = 0; y < TAP_COUNT; ++y)
    {
        cuv.x = tl.x;
        for (int x = 0; x < TAP_COUNT; ++x)
        {            
            //col += shadergraph_SampleSceneColor(cuv);
            //col += tex.Sample(SS, cuv);
            cuv.x += TapSize;            
        }
        cuv.y += TapSize;
    }
    col.r *= 10000000.0;
    OutColour = float4(col, 1.0);
}

#endif
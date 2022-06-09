

        struct Bindings_VertexIn
        {
            float3 WorldSpaceNormal;
            float3 TangentSpaceNormal;
            float3 WorldSpaceTangent;
            float3 WorldSpaceBiTangent;
            float3 TangentSpaceViewDirection;
            float3 WorldSpacePosition;
            float3 TangentSpacePosition;
            half4 uv0;
            half4 uv1;
        };

        struct SurfaceDescriptionInputs
        {
            float3 WorldSpaceNormal;
            float3 TangentSpaceNormal;
            float3 WorldSpaceTangent;
            float3 WorldSpaceBiTangent;
            float3 WorldSpaceViewDirection;
            float3 TangentSpaceViewDirection;
            float3 WorldSpacePosition;
            float3 TangentSpacePosition;
            float4 uv0;
            float4 uv1;
        };

        void SG_MainHairSubGraph_float(
                                 UnityTexture2D specShiftMap, UnityTexture2D normalMap,UnityTexture2D albedoMap, UnityTexture2D rootToTipMap, UnityTexture2D flowMap, UnityTexture2D AOMapUnique, UnityTexture2D OpacityMap, UnityTexture2D AOMap,
                                 float alphaClipThreshold, float specExp1, float specExp2, float EnvRough, float specShift, float specShift2, float flowMultiplier, float specMultiply,
                                 float anisoHighlightRotation, float testNormalsBool, float testTangentsBool, float useFlowMapBool,
                                 float AOFactor, float AOStrength, float alphaPower, float alphaLODbias, float useUniqueAOBool,
                                 float4 colour, float envSpecMul, float flowMapRotation, float meshFlowRotation, float SDFSmoothing,
                                 float SDFAAFactor, float SDFGamma, float SDFToggleBool, float scatterFactor, float transmissionStrength, float transmissionHaloSharpness,
                                 SurfaceDescriptionInputs IN, out float AlphaClip_1, out float3 Colour_2, out float Alpha_3);

struct Bindings_TangentBasis
{
       float3 WorldSpaceNormal;
       float3 WorldSpaceTangent;
       float3 WorldSpaceBiTangent;
};


void Unity_Multiply_half3_half3(half3 A, half3 B, out half3 Out)
{
    Out = A * B;
}

void Unity_Add_float3(float3 A, float3 B, out float3 Out)
{
    Out = A + B;
}

void Unity_MatrixConstruction_Row_float(float4 M0, float4 M1, float4 M2, float4 M3, out float4x4 Out4x4, out float3x3 Out3x3, out float2x2 Out2x2)
{
    Out4x4 = float4x4(M0.x, M0.y, M0.z, M0.w, M1.x, M1.y, M1.z, M1.w, M2.x, M2.y, M2.z, M2.w, M3.x, M3.y, M3.z, M3.w);
    Out3x3 = float3x3(M0.x, M0.y, M0.z, M1.x, M1.y, M1.z, M2.x, M2.y, M2.z);
    Out2x2 = float2x2(M0.x, M0.y, M1.x, M1.y);
}


void Unity_MatrixTranspose_float3x3(float3x3 In, out float3x3 Out)
{
    Out = transpose(In);
}

void Unity_Rotate_About_Axis_Degrees_float(float3 In, float3 Axis, float Rotation, out float3 Out)
{
    Rotation = radians(Rotation);

    float s = sin(Rotation);
    float c = cos(Rotation);
    float one_minus_c = 1.0 - c;

    Axis = normalize(Axis);

    float3x3 rot_mat = { one_minus_c * Axis.x * Axis.x + c,            one_minus_c * Axis.x * Axis.y - Axis.z * s,     one_minus_c * Axis.z * Axis.x + Axis.y * s,
                              one_minus_c * Axis.x * Axis.y + Axis.z * s,   one_minus_c * Axis.y * Axis.y + c,              one_minus_c * Axis.y * Axis.z - Axis.x * s,
                              one_minus_c * Axis.z * Axis.x - Axis.y * s,   one_minus_c * Axis.y * Axis.z + Axis.x * s,     one_minus_c * Axis.z * Axis.z + c
    };

    Out = mul(rot_mat, In);
}

void Unity_Multiply_float3x3_float3(float3x3 A, float3 B, out float3 Out)
{
    Out = mul(A, B);
}

void Unity_Subtract_float(float A, float B, out float Out)
{
    Out = A - B;
}

void Unity_Multiply_float_float(float A, float B, out float Out)
{
    Out = A * B;
}

void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Branch_float3(float Predicate, float3 True, float3 False, out float3 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Normalize_float3(float3 In, out float3 Out)
        {
            Out = normalize(In);
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
        Out = A * B;
        }
        
        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }

        

        void SG_TangentBasis(Bindings_TangentBasis IN, out float3x3 OutMatrix3_1)
        {
            float4x4 _MatrixConstruction_ab739f6004d14c549a9f3e97eaca8647_var4x4_4;
            float3x3 _MatrixConstruction_ab739f6004d14c549a9f3e97eaca8647_var3x3_5;
            float2x2 _MatrixConstruction_ab739f6004d14c549a9f3e97eaca8647_var2x2_6;
            Unity_MatrixConstruction_Row_float((float4(IN.WorldSpaceTangent, 1.0)), (float4(IN.WorldSpaceBiTangent, 1.0)), (float4(IN.WorldSpaceNormal, 1.0)), float4 (0, 0, 0, 0), _MatrixConstruction_ab739f6004d14c549a9f3e97eaca8647_var4x4_4, _MatrixConstruction_ab739f6004d14c549a9f3e97eaca8647_var3x3_5, _MatrixConstruction_ab739f6004d14c549a9f3e97eaca8647_var2x2_6);
            OutMatrix3_1 = _MatrixConstruction_ab739f6004d14c549a9f3e97eaca8647_var3x3_5;
        }


        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };

        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
             float4 texCoord1;
             float3 viewDirectionWS;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif

            float3 sh;
        };

     
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 interp0 : INTERP0;
             float3 interp1 : INTERP1;
             float4 interp2 : INTERP2;
             float4 interp3 : INTERP3;
             float4 interp4 : INTERP4;
             float3 interp5 : INTERP5;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float AlphaClipThreshold;
        };



#define TEXTURE_DECL(NAME) texture2D NAME; SamplerState sampler##NAME; float4 NAME##_TexelSize;
        // To make the Unity shader SRP Batcher compatible, declare all
        // properties related to a Material in a a single CBUFFER block with 
        // the name UnityPerMaterial.
        CBUFFER_START(UnityPerMaterial)
            // The following line declares the _BaseColor variable, so that you
            // can use it in the fragment shader.



        TEXTURE_DECL(specShiftMap)
        TEXTURE_DECL(normalMap)
        TEXTURE_DECL(albedoMap)
        TEXTURE_DECL(rootToTipMap)
        TEXTURE_DECL(flowMap)
        TEXTURE_DECL(OpacityMap)
        TEXTURE_DECL(AOMap)
        TEXTURE_DECL(AOMapUnique)
        float4 colour;
        float specExp1;
        float specExp2;
        float EnvRough;
        float specShift;
        float specShift2;
        float flowMultiplier;
        float specMultiply;
        float alphaClipThreshold;
        float anisoHighlightRotation;
        float AOFactor;
        float AOStrength;
        float alphaPower;
        float alphaLODbias;
        float envSpecMul;
        float flowMapRotation;
        float meshFlowRotation;
        float SDFSmoothing;
        float SDFAAFactor;
        float SDFGamma;
        float scatterFactor;
        float transmissionStrength;
        float transmissionHaloSharpness;
        float testNormalsBool;
        float testTangentsBool;
        float useFlowMapBool;
        float useUniqueAOBool;
        float SDFToggleBool;
        CBUFFER_END

        struct Bindings_MainHairSubGraph
        {
            float3 WorldSpaceNormal;
            float3 TangentSpaceNormal;
            float3 WorldSpaceTangent;
            float3 WorldSpaceBiTangent;
            float3 TangentSpaceViewDirection;
            float3 WorldSpacePosition;
            float3 TangentSpacePosition;
            half4 uv0;
            half4 uv1;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN) //
        {
            SurfaceDescription surface = (SurfaceDescription)0;
                                    
            UnityTexture2D specShiftMap_ = UnityBuildTexture2DStructNoScale(specShiftMap);
            UnityTexture2D normalMap_ = UnityBuildTexture2DStructNoScale(normalMap);
            UnityTexture2D albedoMap_ = UnityBuildTexture2DStructNoScale(albedoMap);
            UnityTexture2D rootToTipMap_ = UnityBuildTexture2DStructNoScale(rootToTipMap);
            UnityTexture2D flowMap_ = UnityBuildTexture2DStructNoScale(flowMap);
            UnityTexture2D AOMapUnique_ = UnityBuildTexture2DStructNoScale(AOMapUnique);
            UnityTexture2D OpacityMap_ = UnityBuildTexture2DStructNoScale(OpacityMap);
            UnityTexture2D AOMap_ = UnityBuildTexture2DStructNoScale(AOMap);
      
      
            SG_MainHairSubGraph_float(specShiftMap_, normalMap_, albedoMap_, rootToTipMap_, flowMap_, AOMapUnique_, OpacityMap_, AOMap_,
                        alphaClipThreshold, specExp1, specExp2, EnvRough, specShift, specShift2, flowMultiplier, specMultiply,
                        anisoHighlightRotation, testNormalsBool, testTangentsBool, useFlowMapBool,
                        AOFactor, AOStrength, alphaPower, alphaLODbias, useUniqueAOBool,
                        colour, envSpecMul, flowMapRotation, meshFlowRotation, SDFSmoothing,
                        SDFAAFactor, SDFGamma, SDFToggleBool, scatterFactor, transmissionStrength, transmissionHaloSharpness,
                        IN, surface.AlphaClipThreshold, surface.BaseColor, surface.AlphaClipThreshold);
           
            return surface;
        }

        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
            // use bitangent on the fly like in hdrp
            // IMPORTANT! If we ever support Flip on double sided materials ensure bitangent and tangent are NOT flipped.
            float crossSign = (input.tangentWS.w > 0.0 ? 1.0 : -1.0)* GetOddNegativeScale();
            float3 bitang = crossSign * cross(input.normalWS.xyz, input.tangentWS.xyz);
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
            // to pr               eserve mikktspace compliance we use same scale renormFactor as was used on the normal.
            // This                is explained in section 2.2 in "surface gradient based bump mapping framework"
            output.WorldSpaceTangent = renormFactor * input.tangentWS.xyz;
            output.WorldSpaceBiTangent = renormFactor * bitang;
        
            output.WorldSpaceViewDirection = normalize(input.viewDirectionWS);
            float3x3 tangentSpaceTransform = float3x3(output.WorldSpaceTangent, output.WorldSpaceBiTangent, output.WorldSpaceNormal);
            output.TangentSpaceViewDirection = mul(tangentSpaceTransform, output.WorldSpaceViewDirection);
            output.WorldSpacePosition = input.positionWS;
            output.TangentSpacePosition = float3(0.0f, 0.0f, 0.0f);
            output.uv0 = input.texCoord0;
            output.uv1 = input.texCoord1;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }

      
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyz =  input.normalWS;
            output.interp2.xyzw =  input.tangentWS;
            output.interp3.xyzw =  input.texCoord0;
            output.interp4.xyzw =  input.texCoord1;
            output.interp5.xyz =  input.viewDirectionWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.normalWS = input.interp1.xyz;
            output.tangentWS = input.interp2.xyzw;
            output.texCoord0 = input.interp3.xyzw;
            output.texCoord1 = input.interp4.xyzw;
            output.viewDirectionWS = input.interp5.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        


        void SG_RemapNormal2_2b6375e425b07084da8462e68a0157b8_float(float3 Vector3_942441e1fae041e0bdf53ccda1848564, out float3 Normals_1)
        {
            float3 _Property_32a8f23bd88741fd815d47022c986244_Out_0 = Vector3_942441e1fae041e0bdf53ccda1848564;
            float _Split_9211f61c93e94a9eac40db454e8afa1b_R_1 = _Property_32a8f23bd88741fd815d47022c986244_Out_0[0];
            float _Split_9211f61c93e94a9eac40db454e8afa1b_G_2 = _Property_32a8f23bd88741fd815d47022c986244_Out_0[1];
            float _Split_9211f61c93e94a9eac40db454e8afa1b_B_3 = _Property_32a8f23bd88741fd815d47022c986244_Out_0[2];
            float _Split_9211f61c93e94a9eac40db454e8afa1b_A_4 = 0;
            float _Subtract_ba09f9385df94b3bafa0f7c2c01bf4ab_Out_2;
            Unity_Subtract_float(_Split_9211f61c93e94a9eac40db454e8afa1b_R_1, 0.5, _Subtract_ba09f9385df94b3bafa0f7c2c01bf4ab_Out_2);
            float _Multiply_2cf540c875044890aa19cf1c519f7c62_Out_2;
            Unity_Multiply_float_float(_Subtract_ba09f9385df94b3bafa0f7c2c01bf4ab_Out_2, 2, _Multiply_2cf540c875044890aa19cf1c519f7c62_Out_2);
            Normals_1 = (_Multiply_2cf540c875044890aa19cf1c519f7c62_Out_2.xxx);
        }

   
     void SG_MainHairSubGraph_float(
                                 UnityTexture2D specShiftMap, UnityTexture2D normalMap,UnityTexture2D albedoMap, UnityTexture2D rootToTipMap, UnityTexture2D flowMap, UnityTexture2D AOMapUnique, UnityTexture2D OpacityMap, UnityTexture2D AOMap,
                                 float alphaClipThreshold, float specExp1, float specExp2, float EnvRough, float specShift, float specShift2, float flowMultiplier, float specMultiply,
                                 float anisoHighlightRotation, float testNormalsBool, float testTangentsBool, float useFlowMapBool, 
                                 float AOFactor, float AOStrength, float alphaPower, float alphaLODbias, float useUniqueAOBool, 
                                 float4 colour, float envSpecMul, float flowMapRotation, float meshFlowRotation, float SDFSmoothing, 
                                 float SDFAAFactor, float SDFGamma, float SDFToggleBool, float scatterFactor, float transmissionStrength, float transmissionHaloSharpness, 
                                 SurfaceDescriptionInputs IN, out float AlphaClip_1, out float3 Colour_2, out float Alpha_3)
     {                
        Bindings_TangentBasis tanbasis;
        tanbasis.WorldSpaceNormal = IN.WorldSpaceNormal;
        tanbasis.WorldSpaceTangent = IN.WorldSpaceTangent;
        tanbasis.WorldSpaceBiTangent = IN.WorldSpaceBiTangent;
        float3x3 tanBasisMat;
        SG_TangentBasis(tanbasis, tanBasisMat);
        float3x3 TransposeTanBasisMat;
        Unity_MatrixTranspose_float3x3(tanBasisMat, TransposeTanBasisMat);
                
        float3 rotateAboutAxisVec3;
        Unity_Rotate_About_Axis_Degrees_float(IN.WorldSpaceTangent, IN.WorldSpaceNormal, anisoHighlightRotation, rotateAboutAxisVec3);
        float3 _Multiply_fd0603abb5784be9b1ae02785ab0177a_Out_2;
        Unity_Multiply_float3x3_float3(tanBasisMat, rotateAboutAxisVec3, _Multiply_fd0603abb5784be9b1ae02785ab0177a_Out_2);        
        float3 _RotateAboutAxis_1a9c8caaf41f42d5b73e575c8d878695_Out_3;
        Unity_Rotate_About_Axis_Degrees_float(_Multiply_fd0603abb5784be9b1ae02785ab0177a_Out_2, float3 (0, 0, 1), meshFlowRotation, _RotateAboutAxis_1a9c8caaf41f42d5b73e575c8d878695_Out_3);
        float3x3 mat3x3;
        convertMat4ToMat3_float(UNITY_MATRIX_M, mat3x3);        
        float4 flowMapSample = SAMPLE_TEXTURE2D(flowMap.tex, flowMap.samplerstate, flowMap.GetTransformedUV(IN.uv0.xy));        
        float3 remapedNormal;
        SG_RemapNormal2_2b6375e425b07084da8462e68a0157b8_float((flowMapSample.xyz), remapedNormal);
        float3 _Multiply_81bf44ff723446d8b348d7830fe6531b_Out_2;
        Unity_Multiply_float3x3_float3(mat3x3, remapedNormal, _Multiply_81bf44ff723446d8b348d7830fe6531b_Out_2);
        float3 normalRotatedAboutAxis;
        Unity_Rotate_About_Axis_Degrees_float(_Multiply_81bf44ff723446d8b348d7830fe6531b_Out_2, IN.WorldSpaceNormal, anisoHighlightRotation, normalRotatedAboutAxis);
        float3 _Multiply_a4fee9b8c8854600afab94ac7130d1cf_Out_2;
        Unity_Multiply_float3x3_float3(tanBasisMat, normalRotatedAboutAxis, _Multiply_a4fee9b8c8854600afab94ac7130d1cf_Out_2);
        float _Property_a77093edd0024d2f96c55b643f6d529c_Out_0 = flowMapRotation;
        float3 _RotateAboutAxis_e5a8a4ea41314dc9a596767ac4e4d20d_Out_3;
        Unity_Rotate_About_Axis_Degrees_float(_Multiply_a4fee9b8c8854600afab94ac7130d1cf_Out_2, float3 (0, 0, 1), _Property_a77093edd0024d2f96c55b643f6d529c_Out_0, _RotateAboutAxis_e5a8a4ea41314dc9a596767ac4e4d20d_Out_3);
        float _Property_95e5f2edf5f14e7ea8e31d6e957ef8b8_Out_0 = flowMultiplier;
        float3 _Lerp_8668370d7be54b0a85a05636a86cda2f_Out_3;
        Unity_Lerp_float3(_RotateAboutAxis_1a9c8caaf41f42d5b73e575c8d878695_Out_3, _RotateAboutAxis_e5a8a4ea41314dc9a596767ac4e4d20d_Out_3, (_Property_95e5f2edf5f14e7ea8e31d6e957ef8b8_Out_0.xxx), _Lerp_8668370d7be54b0a85a05636a86cda2f_Out_3);
        float3 _Branch_ac0521a913584cababd1d2c6a0d26fad_Out_3;
        Unity_Branch_float3(useFlowMapBool, _Lerp_8668370d7be54b0a85a05636a86cda2f_Out_3, _RotateAboutAxis_1a9c8caaf41f42d5b73e575c8d878695_Out_3, _Branch_ac0521a913584cababd1d2c6a0d26fad_Out_3);
        float3 _Multiply_55a85f3f90db4195a8fb991954b8c304_Out_2;
        Unity_Multiply_float3x3_float3(TransposeTanBasisMat, _Branch_ac0521a913584cababd1d2c6a0d26fad_Out_3, _Multiply_55a85f3f90db4195a8fb991954b8c304_Out_2);
        float3 _Normalize_7f48a916e7e848af9e08e5ec2c154e0c_Out_1;
        Unity_Normalize_float3(_Multiply_55a85f3f90db4195a8fb991954b8c304_Out_2, _Normalize_7f48a916e7e848af9e08e5ec2c154e0c_Out_1);
        float3 _Add_6b7b1f42e52d4d5e82b7acacfe696f7b_Out_2;
        _Add_6b7b1f42e52d4d5e82b7acacfe696f7b_Out_2 = _Normalize_7f48a916e7e848af9e08e5ec2c154e0c_Out_1 + float3(1, 1, 1);
        float3 _Multiply_9147c43d534841f9a741a0f81b9949f2_Out_2;
        Unity_Multiply_float3_float3(_Add_6b7b1f42e52d4d5e82b7acacfe696f7b_Out_2, float3(0.5, 0.5, 0.5), _Multiply_9147c43d534841f9a741a0f81b9949f2_Out_2);
                
        float4 albedoMapSample = SAMPLE_TEXTURE2D(albedoMap.tex, albedoMap.samplerstate, albedoMap.GetTransformedUV(IN.uv0.xy));
        float4 _Multiply_2975ea5f41bb4680a74280e106ac2c02_Out_2;
        Unity_Multiply_float4_float4(colour, albedoMapSample, _Multiply_2975ea5f41bb4680a74280e106ac2c02_Out_2);
        float4 Color_a426fac7938d4054afefc4ba96e8e91b = IsGammaSpace() ? float4(0.009433985, 0.009433985, 0.009433985, 0) : float4(SRGBToLinear(float3(0.009433985, 0.009433985, 0.009433985)), 0);
        float4 _Add_4c90deb4e3184939a6948966cb92c7eb_Out_2;
        Unity_Add_float4(_Multiply_2975ea5f41bb4680a74280e106ac2c02_Out_2, Color_a426fac7938d4054afefc4ba96e8e91b, _Add_4c90deb4e3184939a6948966cb92c7eb_Out_2);
                
        float4 rootToTipSample = SAMPLE_TEXTURE2D(rootToTipMap.tex, rootToTipMap.samplerstate, rootToTipMap.GetTransformedUV(IN.uv0.xy));


        float4 AOMapUniqueSample = SAMPLE_TEXTURE2D(AOMapUnique.tex, AOMapUnique.samplerstate, AOMapUnique.GetTransformedUV(IN.uv1.xy));              
        float4 AOMapSample = SAMPLE_TEXTURE2D(AOMap.tex, AOMap.samplerstate, AOMap.GetTransformedUV(IN.uv0.xy));        
        float AOResult;
        Unity_Branch_float(useUniqueAOBool, AOMapUniqueSample.r, AOMapSample.r, AOResult);
        float _Property_6eb4f0e1c9bd428aab62642ba46f7056_Out_0 = AOFactor;
        float _Power_b9505bf771844e2b80c026aa62cd1267_Out_2;
        Unity_Power_float(AOResult, _Property_6eb4f0e1c9bd428aab62642ba46f7056_Out_0, _Power_b9505bf771844e2b80c026aa62cd1267_Out_2);
        
        float3 hairColourResult;
        Hair_float(tanBasisMat, IN.WorldSpacePosition, IN.TangentSpacePosition, IN.TangentSpaceNormal, _Branch_ac0521a913584cababd1d2c6a0d26fad_Out_3, IN.TangentSpaceViewDirection, (IN.uv0.xy), (_Add_4c90deb4e3184939a6948966cb92c7eb_Out_2.xyz), specExp1, specExp2, EnvRough, envSpecMul, specShift, specShift2, flowMultiplier, specMultiply, (rootToTipSample).x, _Power_b9505bf771844e2b80c026aa62cd1267_Out_2, scatterFactor, transmissionStrength, transmissionHaloSharpness, hairColourResult);
        float3 hairColourBranchResult;
        Unity_Branch_float3(testNormalsBool, IN.WorldSpaceNormal, hairColourResult, hairColourBranchResult);
        float3 tangentTestResult;
        Unity_Branch_float3(testTangentsBool, _Multiply_9147c43d534841f9a741a0f81b9949f2_Out_2, hairColourBranchResult, tangentTestResult);
             
        float4 OpacityMapSample = SAMPLE_TEXTURE2D(OpacityMap.tex, OpacityMap.samplerstate, OpacityMap.GetTransformedUV(IN.uv0.xy));
                
        float _calcSDFAlphaCustomFunction;
        calcSDFAlpha_float(float2 (0, 0), OpacityMapSample.r, alphaClipThreshold, SDFSmoothing, SDFAAFactor, SDFGamma, _calcSDFAlphaCustomFunction);
        
        float _Add_6173736479a640478cc85569c8f5d145_Out_2;
        Unity_Add_float(OpacityMapSample.g, 0.0, _Add_6173736479a640478cc85569c8f5d145_Out_2);
        float _Multiply_a83b03889fb44eff9fb38ae293a43425_Out_2;
        Unity_Multiply_float_float(_calcSDFAlphaCustomFunction, _Add_6173736479a640478cc85569c8f5d145_Out_2, _Multiply_a83b03889fb44eff9fb38ae293a43425_Out_2);
        float _Clamp_ee237f78f1e24d10a2e919b7f6567c9c_Out_3;
        Unity_Clamp_float(_Multiply_a83b03889fb44eff9fb38ae293a43425_Out_2, 0, 1, _Clamp_ee237f78f1e24d10a2e919b7f6567c9c_Out_3);
        float _Property_9b91636a824c42bca23b05676c15b3ca_Out_0 = alphaLODbias;
        #if defined(SHADER_API_GLES) && (SHADER_TARGET < 30)
          float4 OpacityMapSample2 = float4(0.0f, 0.0f, 0.0f, 1.0f);
        #else
          //UnityBuildSamplerStateStruct(SamplerState_Trilinear_Repeat).samplerstate
          //float4 OpacityMapSample2 = SAMPLE_TEXTURE2D_LOD(OpacityMap.tex, SamplerState_Trilinear_Repeat, OpacityMap.GetTransformedUV(IN.uv0.xy), _Property_9b91636a824c42bca23b05676c15b3ca_Out_0);
         float4 OpacityMapSample2 = SAMPLE_TEXTURE2D(OpacityMap.tex, OpacityMap.samplerstate, OpacityMap.GetTransformedUV(IN.uv0.xy));
        #endif
      
        float OpacityResult;
        Unity_Branch_float(SDFToggleBool, _Clamp_ee237f78f1e24d10a2e919b7f6567c9c_Out_3, OpacityMapSample2.r, OpacityResult);
        
        float alphaResult;
        Unity_Power_float(OpacityResult, alphaPower, alphaResult);
        AlphaClip_1 = alphaClipThreshold;
        Colour_2 = tangentTestResult;
        Alpha_3 = alphaResult;
     }
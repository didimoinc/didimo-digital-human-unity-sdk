        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD1
        #define VARYINGS_NEED_VIEWDIRECTION_WS
        #define FEATURES_GRAPH_VERTEX
        #define _ALPHATEST_ON 1

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

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
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        half4 _HairColor;
        float4 _Opacity_TexelSize;
        float4 _Albedo_TexelSize;
        float4 _flowMap_TexelSize;
        float4 _rootToTip_TexelSize;
        half _specExp1;
        half _specExp2;
        half _EnvRough;
        half _EnvSpecularScale;
        half _specShift;
        half _specShift2;
        half _flowMultiplier;
        half _hairAlbedoMultiply;
        half _specMultiply;
        half _alphaClipThreshold;
        half _AnisoHighlightRotation;
        half _TestNormals;
        half _TestTangents;
        half _UseUniqueAOMap;
        float4 _AOMap_TexelSize;
        float4 _AOMapUnique_TexelSize;
        half _AOFactor;
        half _AOStrength;
        half _AlphaLODbias;
        half _hairScaleNudge;
        half _TangentFlowMapRotation;
        half _MeshTangentRotation;
        half _UseFlowMap;
        half _TransmissionStrength;
        half _TransmissionHaloSharpness;
        half _ScaterFactor;
        half _SDF_toggle;
        half _SDF_smoothing;
        half _SDF_AAFactor;
        half _SDF_gamma;
        half _AlphaPower;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        SAMPLER(SamplerState_Trilinear_Repeat);
        TEXTURE2D(_NormalMap);
        SAMPLER(sampler_NormalMap);
        float4 _NormalMap_TexelSize;
        TEXTURE2D(_Opacity);
        SAMPLER(sampler_Opacity);
        TEXTURE2D(_SpecShift);
        SAMPLER(sampler_SpecShift);
        float4 _SpecShift_TexelSize;
        TEXTURE2D(_Albedo);
        SAMPLER(sampler_Albedo);
        TEXTURE2D(_flowMap);
        SAMPLER(sampler_flowMap);
        TEXTURE2D(_rootToTip);
        SAMPLER(sampler_rootToTip);
        TEXTURE2D(_AOMap);
        SAMPLER(sampler_AOMap);
        TEXTURE2D(_AOMapUnique);
        SAMPLER(sampler_AOMapUnique);
        
        // Graph Includes
        #include "Packages/com.didimo.sdk.core/Runtime/Pipeline/URP/Shaders/Didimo.URP.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Multiply_half3_half3(half3 A, half3 B, out half3 Out)
        {
            Out = A * B;
        }
              
        void Unity_MatrixConstruction_Row_float (float4 M0, float4 M1, float4 M2, float4 M3, out float4x4 Out4x4, out float3x3 Out3x3, out float2x2 Out2x2)
        {
            Out4x4 = float4x4(M0.x, M0.y, M0.z, M0.w, M1.x, M1.y, M1.z, M1.w, M2.x, M2.y, M2.z, M2.w, M3.x, M3.y, M3.z, M3.w);
            Out3x3 = float3x3(M0.x, M0.y, M0.z, M1.x, M1.y, M1.z, M2.x, M2.y, M2.z);
            Out2x2 = float2x2(M0.x, M0.y, M1.x, M1.y);
        }

        float3x3 Unity_MatrixConstruction_Row_float_3x3(float3 M0, float3 M1, float3 M2)
        {            
            return float3x3(M0.x, M0.y, M0.z, M1.x, M1.y, M1.z, M2.x, M2.y, M2.z);            
        }
        
        struct Bindings_TangentBasis
        {
            float3 WorldSpaceNormal;
            float3 WorldSpaceTangent;
            float3 WorldSpaceBiTangent;
        };
        
        float3x3 SG_TangentBasis(Bindings_TangentBasis IN)
        {            
            return Unity_MatrixConstruction_Row_float_3x3(IN.WorldSpaceTangent.xyz, IN.WorldSpaceBiTangent.xyz, IN.WorldSpaceNormal.xyz);
        }

        
        float3 Unity_Rotate_About_Axis_Degrees_float(float3 In, float3 Axis, float Rotation)
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
        
            return mul(rot_mat,  In);
        } 
    
        float3 branch(float Predicate, float3 True, float3 False)
        {
            return Predicate ? True : False;
        }
                  
        float branch(float Predicate, float True, float False)
        {
            return Predicate ? True : False;
        }
                                
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
     
        float3 SG_RemapNormal2(float3 invec)
        {
            return (invec[0] - 0.5) * 2;            
        }
            
        float3x3 SG_TangentBasis(Bindings_MainHairSubGraph IN)
        {
            return Unity_MatrixConstruction_Row_float_3x3(IN.WorldSpaceTangent.xyz, IN.WorldSpaceBiTangent.xyz, IN.WorldSpaceNormal.xyz);
        }

        float3x3 SG_TangentBasis(SurfaceDescriptionInputs IN)
        {
            return Unity_MatrixConstruction_Row_float_3x3(IN.WorldSpaceTangent.xyz, IN.WorldSpaceBiTangent.xyz, IN.WorldSpaceNormal.xyz);
        }


        float3 packNormal(float3 normal)
        {
            return (normal + float3(1, 1 , 1)) * 0.5;               
        }
     
        void SG_MainHairSubGraph_float(
        float SpecExp1, float SpecExp2, float EnvRough, float SpecShift, float SpecShift2, 
        float FlowMultipler, float SpecMultiply, UnityTexture2D SpecShiftTex, UnityTexture2D NormalTex, float AlphaClipThreshold, UnityTexture2D AlbedoTex, UnityTexture2D RootToTipTex, UnityTexture2D FlowMapTex,
        float AnisoHighlightRotation, float TestNormalsBool, float TestTangentsBool, float UseFlowMapBool, float AOFactor, float AOStrength, UnityTexture2D OpacityTex, float AlphaPower, float AlphaLODBias, 
        float UseUniqueAOBool, UnityTexture2D AOMapTex, UnityTexture2D AOMapUniqueTex, float4 Colour, float EnvSpecMul, float FlowMapRotation, float MeshFlowRotation, float SDFSmoothing, float SDFAAFactor, float SDFGamma, 
        float SDFToggleBool, float ScatterFactor, float TransmissionStrength, float TransmissionHaloSharpness, 
        SurfaceDescriptionInputs IN, out float AlphaClip, out float3 OutColour, out float OutAlpha)
        {
            float3x3 tangentBasis3x3 = SG_TangentBasis(IN);
            float3x3 tangentBasis3x3_t = transpose(tangentBasis3x3);
            float3 MeshTangentRotated_WS = Unity_Rotate_About_Axis_Degrees_float(IN.WorldSpaceTangent, IN.WorldSpaceNormal, AnisoHighlightRotation);
            float3 MeshTangentRotated_TS = mul(tangentBasis3x3, MeshTangentRotated_WS);        
            MeshTangentRotated_TS = Unity_Rotate_About_Axis_Degrees_float(MeshTangentRotated_TS, float3 (0, 0, 1), MeshFlowRotation);
            float3x3 modelMatrix3x3;
            convertMat4ToMat3_float(UNITY_MATRIX_M, modelMatrix3x3);        
            float4 FlowMapTexSample = SAMPLE_TEXTURE2D(FlowMapTex.tex, FlowMapTex.samplerstate, FlowMapTex.GetTransformedUV(IN.uv0.xy));
        
            float3 FlowMapTangent_TS = mul(modelMatrix3x3, SG_RemapNormal2(FlowMapTexSample.xyz));
            FlowMapTangent_TS = Unity_Rotate_About_Axis_Degrees_float(FlowMapTangent_TS, IN.WorldSpaceNormal, AnisoHighlightRotation);
            FlowMapTangent_TS = mul(tangentBasis3x3, FlowMapTangent_TS);        
            FlowMapTangent_TS= Unity_Rotate_About_Axis_Degrees_float(FlowMapTangent_TS, float3 (0, 0, 1), FlowMapRotation);
        
            float3 TangentResult = lerp(MeshTangentRotated_TS, FlowMapTangent_TS, (FlowMultipler.xxx));
            TangentResult = branch(UseFlowMapBool, TangentResult, MeshTangentRotated_TS);
            float3 TangentResultTangentSpace =  packNormal(normalize(mul(tangentBasis3x3_t, TangentResult)));            
             float4 AbledoTexSample = SAMPLE_TEXTURE2D(AlbedoTex.tex, AlbedoTex.samplerstate, AlbedoTex.GetTransformedUV(IN.uv0.xy));
    
            float3 offsetColour = float3(0.009433985, 0.009433985, 0.009433985);           
            float3 albedoColour = (Colour.rgb * AbledoTexSample) +  (IsGammaSpace() ? offsetColour : SRGBToLinear(offsetColour.rgb));
        
            float4 RootToTipSample = SAMPLE_TEXTURE2D(RootToTipTex.tex, RootToTipTex.samplerstate, RootToTipTex.GetTransformedUV(IN.uv0.xy));             
            float4 AOMapUniqueTexSample = SAMPLE_TEXTURE2D(AOMapUniqueTex.tex, AOMapUniqueTex.samplerstate, AOMapUniqueTex.GetTransformedUV(IN.uv1.xy));        
            float4 AOMapTexSample = SAMPLE_TEXTURE2D(AOMapTex.tex, AOMapTex.samplerstate, AOMapTex.GetTransformedUV(IN.uv0.xy));
            float AOResult = branch(UseUniqueAOBool, AOMapUniqueTexSample.r, AOMapTexSample.r);
            AOResult = lerp(1.0f, pow(AOResult, AOFactor), AOStrength);
            
        
            float3 hairFunctionColourResult;
            Hair_float(tangentBasis3x3, IN.WorldSpacePosition, IN.TangentSpacePosition, IN.TangentSpaceNormal, TangentResult, IN.TangentSpaceViewDirection, IN.uv0.xy, albedoColour, SpecExp1, SpecExp2, EnvRough, EnvSpecMul, SpecShift, SpecShift2, FlowMultipler, SpecMultiply, (RootToTipSample.rgba).x, AOResult, ScatterFactor, TransmissionStrength, TransmissionHaloSharpness, hairFunctionColourResult);            
            OutColour = branch(TestTangentsBool, TangentResultTangentSpace, branch(TestNormalsBool, IN.WorldSpaceNormal, hairFunctionColourResult));
            float4 OpacityTexSample = SAMPLE_TEXTURE2D(OpacityTex.tex, OpacityTex.samplerstate, OpacityTex.GetTransformedUV(IN.uv0.xy));
            
            float SDFAlphaResult;
            calcSDFAlpha_float(float2 (0, 0), OpacityTexSample.r, AlphaClipThreshold, SDFSmoothing, SDFAAFactor, SDFGamma, SDFAlphaResult);                        
            SDFAlphaResult = clamp(SDFAlphaResult * OpacityTexSample.g, 0, 1);
    
            #if defined(SHADER_API_GLES) && (SHADER_TARGET < 30)
              float OpacitySampleLOD = 0.0f;
            #else
              float OpacitySampleLOD = SAMPLE_TEXTURE2D_LOD(OpacityTex.tex, UnityBuildSamplerStateStruct(SamplerState_Trilinear_Repeat).samplerstate, OpacityTex.GetTransformedUV(IN.uv0.xy), AlphaLODBias).r;
            #endif
    
            float WhichAlpha = branch(SDFToggleBool, SDFAlphaResult, OpacitySampleLOD);
            OutAlpha = pow(WhichAlpha, AlphaPower);
            AlphaClip = AlphaClipThreshold;
        
        }


         // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            half3 Normal;
            half3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            half _Property_87332f423a654fc58db45e080aa8c656_Out_0 = _hairScaleNudge;
            half3 _Multiply_272a1dfd302a429cb91114d47fd727e3_Out_2;
            Unity_Multiply_half3_half3(IN.ObjectSpaceNormal, (_Property_87332f423a654fc58db45e080aa8c656_Out_0.xxx), _Multiply_272a1dfd302a429cb91114d47fd727e3_Out_2);
            float3 _Add_06ae16e20f45417e8d8697ec13ce2250_Out_2 = IN.ObjectSpacePosition + _Multiply_272a1dfd302a429cb91114d47fd727e3_Out_2;
            description.Position = _Add_06ae16e20f45417e8d8697ec13ce2250_Out_2;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }


        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
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
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        void Unity_MatrixConstruction_Row_float (float4 M0, float4 M1, float4 M2, float4 M3, out float4x4 Out4x4, out float3x3 Out3x3, out float2x2 Out2x2)
        {
        Out4x4 = float4x4(M0.x, M0.y, M0.z, M0.w, M1.x, M1.y, M1.z, M1.w, M2.x, M2.y, M2.z, M2.w, M3.x, M3.y, M3.z, M3.w);
        Out3x3 = float3x3(M0.x, M0.y, M0.z, M1.x, M1.y, M1.z, M2.x, M2.y, M2.z);
        Out2x2 = float2x2(M0.x, M0.y, M1.x, M1.y);
        }
        
        struct Bindings_TangentBasis_9299e8973405e7742ba8aebaec0f00a3_float
        {
        float3 WorldSpaceNormal;
        float3 WorldSpaceTangent;
        float3 WorldSpaceBiTangent;
        };
        
        void SG_TangentBasis_9299e8973405e7742ba8aebaec0f00a3_float(Bindings_TangentBasis_9299e8973405e7742ba8aebaec0f00a3_float IN, out float3x3 OutMatrix3_1)
        {
        float4x4 _MatrixConstruction_ab739f6004d14c549a9f3e97eaca8647_var4x4_4;
        float3x3 _MatrixConstruction_ab739f6004d14c549a9f3e97eaca8647_var3x3_5;
        float2x2 _MatrixConstruction_ab739f6004d14c549a9f3e97eaca8647_var2x2_6;
        Unity_MatrixConstruction_Row_float((float4(IN.WorldSpaceTangent, 1.0)), (float4(IN.WorldSpaceBiTangent, 1.0)), (float4(IN.WorldSpaceNormal, 1.0)), float4 (0, 0, 0, 0), _MatrixConstruction_ab739f6004d14c549a9f3e97eaca8647_var4x4_4, _MatrixConstruction_ab739f6004d14c549a9f3e97eaca8647_var3x3_5, _MatrixConstruction_ab739f6004d14c549a9f3e97eaca8647_var2x2_6);
        OutMatrix3_1 = _MatrixConstruction_ab739f6004d14c549a9f3e97eaca8647_var3x3_5;
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
        
            Out = mul(rot_mat,  In);
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
        
        struct Bindings_RemapNormal2_2b6375e425b07084da8462e68a0157b8_float
        {
        };
        
        
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
        
        struct Bindings_MainHairSubGraph_8fa39b8404808394cb967f29bc5756e2_float
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
        struct Bindings_TangentBasis
        {
            float3 WorldSpaceNormal;
            float3 WorldSpaceTangent;
            float3 WorldSpaceBiTangent;
        };

          void SG_RemapNormal2(float3 invec, out float3 Normals_1)
        {

        float _Split_9211f61c93e94a9eac40db454e8afa1b_R_1 = invec[0];
        float _Subtract_ba09f9385df94b3bafa0f7c2c01bf4ab_Out_2;
        Unity_Subtract_float(_Split_9211f61c93e94a9eac40db454e8afa1b_R_1, 0.5, _Subtract_ba09f9385df94b3bafa0f7c2c01bf4ab_Out_2);
        float _Multiply_2cf540c875044890aa19cf1c519f7c62_Out_2;
        Unity_Multiply_float_float(_Subtract_ba09f9385df94b3bafa0f7c2c01bf4ab_Out_2, 2, _Multiply_2cf540c875044890aa19cf1c519f7c62_Out_2);
        Normals_1 = (_Multiply_2cf540c875044890aa19cf1c519f7c62_Out_2.xxx);
        }
        

        void SG_TangentBasis(Bindings_TangentBasis IN, out float3x3 OutMatrix3_1)
        {
            float4x4 _MatrixConstruction_ab739f6004d14c549a9f3e97eaca8647_var4x4_4;
            float3x3 _MatrixConstruction_ab739f6004d14c549a9f3e97eaca8647_var3x3_5;
            float2x2 _MatrixConstruction_ab739f6004d14c549a9f3e97eaca8647_var2x2_6;
            Unity_MatrixConstruction_Row_float((float4(IN.WorldSpaceTangent, 1.0)), (float4(IN.WorldSpaceBiTangent, 1.0)), (float4(IN.WorldSpaceNormal, 1.0)), float4 (0, 0, 0, 0), _MatrixConstruction_ab739f6004d14c549a9f3e97eaca8647_var4x4_4, _MatrixConstruction_ab739f6004d14c549a9f3e97eaca8647_var3x3_5, _MatrixConstruction_ab739f6004d14c549a9f3e97eaca8647_var2x2_6);
            OutMatrix3_1 = _MatrixConstruction_ab739f6004d14c549a9f3e97eaca8647_var3x3_5;
        }
        



        
        //float SpecExp1, float SpecExp2, float EnvRough, float SpecShift, float SpecShift2, 
        //float FlowMultipler, float SpecMultiply, UnityTexture2D SpecShiftTex, UnityTexture2D NormalTex, 
        //float AlphaClipThreshold, UnityTexture2D AlbedoTex, UnityTexture2D RootToTipTex, UnityTexture2D FlowMapTex, 
        //float AnisoHighlightRotation, float TestNormalsBool, float TestTangentsBool, float UseFlowMapBool, float AOFactor, UnityTexture2D OpacityTex, float AlphaPower, float AlphaLODbias, 
        //float UseUniqueAOBool, UnityTexture2D AOMapTex, UnityTexture2D AOMapUniqueTex, float4 Colour, float EnvSpecMul, float flowMapRotation, float MeshFlowRotation, float SDFSmoothing, float SDFAAFactor, float SDFGamma, 
        //float SDFToggleBool, float ScatterFactor, float TransmissionStrength, float TransmissionHaloSharpness, 
        //Bindings_MainHairSubGraph_8fa39b8404808394cb967f29bc5756e2_float IN, out float AlphaClip_1, out float3 Colour_2, out float Alpha_3)
        void SG_MainHairSubGraph_8fa39b8404808394cb967f29bc5756e2_float(
        float SpecExp1, float SpecExp2, float EnvRough, float SpecShift, float SpecShift2, 
        float FlowMultipler, float SpecMultiply, UnityTexture2D SpecShiftTex, UnityTexture2D NormalTex, float AlphaClipThreshold, UnityTexture2D AlbedoTex, UnityTexture2D RootToTipTex, UnityTexture2D FlowMapTex,
        float AnisoHighlightRotation, float TestNormalsBool, float TestTangentsBool, float UseFlowMapBool, float AOFactor, UnityTexture2D OpacityTex, float AlphaPower, float AlphaLODBias, 
        float UseUniqueAOBool, UnityTexture2D AOMapTex, UnityTexture2D AOMapUniqueTex, float4 Colour, float EnvSpecMul, float FlowMapRotation, float MeshFlowRotation, float SDFSmoothing, float SDFAAFactor, float SDFGamma, 
        float SDFToggleBool, float ScatterFactor, float TransmissionStrength, float TransmissionHaloSharpness, 
        Bindings_MainHairSubGraph_8fa39b8404808394cb967f29bc5756e2_float IN, out float AlphaClip_1, out float3 Colour_2, out float Alpha_3)
        {
        float _Property_6b1a2906e2b94f339fd3c3869fe68f9e_Out_0 = AlphaClipThreshold;
        float _Property_b5e1480417d3448cbd7ca8c40fdd131a_Out_0 = TestTangentsBool;
        Bindings_TangentBasis_9299e8973405e7742ba8aebaec0f00a3_float _TangentBasis_99b609fdefef4cb48c6f9370b7ee1a0e;
        _TangentBasis_99b609fdefef4cb48c6f9370b7ee1a0e.WorldSpaceNormal = IN.WorldSpaceNormal;
        _TangentBasis_99b609fdefef4cb48c6f9370b7ee1a0e.WorldSpaceTangent = IN.WorldSpaceTangent;
        _TangentBasis_99b609fdefef4cb48c6f9370b7ee1a0e.WorldSpaceBiTangent = IN.WorldSpaceBiTangent;
        float3x3 _TangentBasis_99b609fdefef4cb48c6f9370b7ee1a0e_OutMatrix3_1;
        SG_TangentBasis_9299e8973405e7742ba8aebaec0f00a3_float(_TangentBasis_99b609fdefef4cb48c6f9370b7ee1a0e, _TangentBasis_99b609fdefef4cb48c6f9370b7ee1a0e_OutMatrix3_1);
        float3x3 _MatrixTranspose_0edde2d1aeb94af482eb85dad33d4ff2_Out_1;
        Unity_MatrixTranspose_float3x3(_TangentBasis_99b609fdefef4cb48c6f9370b7ee1a0e_OutMatrix3_1, _MatrixTranspose_0edde2d1aeb94af482eb85dad33d4ff2_Out_1);
        float _Property_e829df8a49604fc1bf5e524224b2f8c0_Out_0 = UseFlowMapBool;
        float _Property_6757a3a2a2b84dc5b3cc0bc18fec7957_Out_0 = AnisoHighlightRotation;
        float3 _RotateAboutAxis_1b78f4780c3a49d1a6edbdfe469741e9_Out_3;
        Unity_Rotate_About_Axis_Degrees_float(IN.WorldSpaceTangent, IN.WorldSpaceNormal, _Property_6757a3a2a2b84dc5b3cc0bc18fec7957_Out_0, _RotateAboutAxis_1b78f4780c3a49d1a6edbdfe469741e9_Out_3);
        float3 _Multiply_fd0603abb5784be9b1ae02785ab0177a_Out_2;
        Unity_Multiply_float3x3_float3(_TangentBasis_99b609fdefef4cb48c6f9370b7ee1a0e_OutMatrix3_1, _RotateAboutAxis_1b78f4780c3a49d1a6edbdfe469741e9_Out_3, _Multiply_fd0603abb5784be9b1ae02785ab0177a_Out_2);
        float _Property_67445c3ce1c9444989a012f7c548baac_Out_0 = MeshFlowRotation;
        float3 _RotateAboutAxis_1a9c8caaf41f42d5b73e575c8d878695_Out_3;
        Unity_Rotate_About_Axis_Degrees_float(_Multiply_fd0603abb5784be9b1ae02785ab0177a_Out_2, float3 (0, 0, 1), _Property_67445c3ce1c9444989a012f7c548baac_Out_0, _RotateAboutAxis_1a9c8caaf41f42d5b73e575c8d878695_Out_3);
        float3x3 _convertMat4ToMat3CustomFunction_354ef7dce6854cba8b2e3596212c5d72_outvec_1;
        convertMat4ToMat3_float(UNITY_MATRIX_M, _convertMat4ToMat3CustomFunction_354ef7dce6854cba8b2e3596212c5d72_outvec_1);
        UnityTexture2D _Property_de82f1af52b04af0911952805fa0eacc_Out_0 = FlowMapTex;
        float4 _SampleTexture2D_b8ee74575d524d01ace9efb6a6e9a436_RGBA_0 = SAMPLE_TEXTURE2D(_Property_de82f1af52b04af0911952805fa0eacc_Out_0.tex, _Property_de82f1af52b04af0911952805fa0eacc_Out_0.samplerstate, _Property_de82f1af52b04af0911952805fa0eacc_Out_0.GetTransformedUV(IN.uv0.xy));
        float _SampleTexture2D_b8ee74575d524d01ace9efb6a6e9a436_R_4 = _SampleTexture2D_b8ee74575d524d01ace9efb6a6e9a436_RGBA_0.r;
        float _SampleTexture2D_b8ee74575d524d01ace9efb6a6e9a436_G_5 = _SampleTexture2D_b8ee74575d524d01ace9efb6a6e9a436_RGBA_0.g;
        float _SampleTexture2D_b8ee74575d524d01ace9efb6a6e9a436_B_6 = _SampleTexture2D_b8ee74575d524d01ace9efb6a6e9a436_RGBA_0.b;
        float _SampleTexture2D_b8ee74575d524d01ace9efb6a6e9a436_A_7 = _SampleTexture2D_b8ee74575d524d01ace9efb6a6e9a436_RGBA_0.a;
        Bindings_RemapNormal2_2b6375e425b07084da8462e68a0157b8_float _RemapNormal2_e4c6bba6fce3484191195ad499213d21;
        float3 _RemapNormal2_e4c6bba6fce3484191195ad499213d21_Normals_1;
        SG_RemapNormal2((_SampleTexture2D_b8ee74575d524d01ace9efb6a6e9a436_RGBA_0.xyz), _RemapNormal2_e4c6bba6fce3484191195ad499213d21_Normals_1);
        float3 _Multiply_81bf44ff723446d8b348d7830fe6531b_Out_2;
        Unity_Multiply_float3x3_float3(_convertMat4ToMat3CustomFunction_354ef7dce6854cba8b2e3596212c5d72_outvec_1, _RemapNormal2_e4c6bba6fce3484191195ad499213d21_Normals_1, _Multiply_81bf44ff723446d8b348d7830fe6531b_Out_2);
        float3 _RotateAboutAxis_bb8b3d7eb753440e874238357b350530_Out_3;
        Unity_Rotate_About_Axis_Degrees_float(_Multiply_81bf44ff723446d8b348d7830fe6531b_Out_2, IN.WorldSpaceNormal, _Property_6757a3a2a2b84dc5b3cc0bc18fec7957_Out_0, _RotateAboutAxis_bb8b3d7eb753440e874238357b350530_Out_3);
        float3 _Multiply_a4fee9b8c8854600afab94ac7130d1cf_Out_2;
        Unity_Multiply_float3x3_float3(_TangentBasis_99b609fdefef4cb48c6f9370b7ee1a0e_OutMatrix3_1, _RotateAboutAxis_bb8b3d7eb753440e874238357b350530_Out_3, _Multiply_a4fee9b8c8854600afab94ac7130d1cf_Out_2);
        float _Property_a77093edd0024d2f96c55b643f6d529c_Out_0 = FlowMapRotation;
        float3 _RotateAboutAxis_e5a8a4ea41314dc9a596767ac4e4d20d_Out_3;
        float _Property_95e5f2edf5f14e7ea8e31d6e957ef8b8_Out_0 = FlowMultipler;
        Unity_Rotate_About_Axis_Degrees_float(_Multiply_a4fee9b8c8854600afab94ac7130d1cf_Out_2, float3 (0, 0, 1), _Property_a77093edd0024d2f96c55b643f6d529c_Out_0, _RotateAboutAxis_e5a8a4ea41314dc9a596767ac4e4d20d_Out_3);
      
        float3 _Lerp_8668370d7be54b0a85a05636a86cda2f_Out_3;
        Unity_Lerp_float3(_RotateAboutAxis_1a9c8caaf41f42d5b73e575c8d878695_Out_3, _RotateAboutAxis_e5a8a4ea41314dc9a596767ac4e4d20d_Out_3, (_Property_95e5f2edf5f14e7ea8e31d6e957ef8b8_Out_0.xxx), _Lerp_8668370d7be54b0a85a05636a86cda2f_Out_3);
        float3 _Branch_ac0521a913584cababd1d2c6a0d26fad_Out_3;
        Unity_Branch_float3(_Property_e829df8a49604fc1bf5e524224b2f8c0_Out_0, _Lerp_8668370d7be54b0a85a05636a86cda2f_Out_3, _RotateAboutAxis_1a9c8caaf41f42d5b73e575c8d878695_Out_3, _Branch_ac0521a913584cababd1d2c6a0d26fad_Out_3);
        float3 _Multiply_55a85f3f90db4195a8fb991954b8c304_Out_2;
        Unity_Multiply_float3x3_float3(_MatrixTranspose_0edde2d1aeb94af482eb85dad33d4ff2_Out_1, _Branch_ac0521a913584cababd1d2c6a0d26fad_Out_3, _Multiply_55a85f3f90db4195a8fb991954b8c304_Out_2);
        float3 _Normalize_7f48a916e7e848af9e08e5ec2c154e0c_Out_1;
        Unity_Normalize_float3(_Multiply_55a85f3f90db4195a8fb991954b8c304_Out_2, _Normalize_7f48a916e7e848af9e08e5ec2c154e0c_Out_1);
        float3 _Add_6b7b1f42e52d4d5e82b7acacfe696f7b_Out_2;
        Unity_Add_float3(_Normalize_7f48a916e7e848af9e08e5ec2c154e0c_Out_1, float3(1, 1, 1), _Add_6b7b1f42e52d4d5e82b7acacfe696f7b_Out_2);
        float3 _Multiply_9147c43d534841f9a741a0f81b9949f2_Out_2;
        Unity_Multiply_float3_float3(_Add_6b7b1f42e52d4d5e82b7acacfe696f7b_Out_2, float3(0.5, 0.5, 0.5), _Multiply_9147c43d534841f9a741a0f81b9949f2_Out_2);
        float _Property_fe24b67262284f7dafbfdc716c687c39_Out_0 = TestNormalsBool;
        float4 _UV_518992b1b5f34744891eea6fcbd3d1bf_Out_0 = IN.uv0;
        float4 _Property_cba9a5289c1743f9a3315daf05cd9cdb_Out_0 = Colour;
        UnityTexture2D _Property_d0aaf0b5021d4e78a3d2155275eb4ae3_Out_0 = AlbedoTex;
        float4 _SampleTexture2D_6d80892c9aa2410b96cdbf438abed2c2_RGBA_0 = SAMPLE_TEXTURE2D(_Property_d0aaf0b5021d4e78a3d2155275eb4ae3_Out_0.tex, _Property_d0aaf0b5021d4e78a3d2155275eb4ae3_Out_0.samplerstate, _Property_d0aaf0b5021d4e78a3d2155275eb4ae3_Out_0.GetTransformedUV(IN.uv0.xy));
        float _SampleTexture2D_6d80892c9aa2410b96cdbf438abed2c2_R_4 = _SampleTexture2D_6d80892c9aa2410b96cdbf438abed2c2_RGBA_0.r;
        float _SampleTexture2D_6d80892c9aa2410b96cdbf438abed2c2_G_5 = _SampleTexture2D_6d80892c9aa2410b96cdbf438abed2c2_RGBA_0.g;
        float _SampleTexture2D_6d80892c9aa2410b96cdbf438abed2c2_B_6 = _SampleTexture2D_6d80892c9aa2410b96cdbf438abed2c2_RGBA_0.b;
        float _SampleTexture2D_6d80892c9aa2410b96cdbf438abed2c2_A_7 = _SampleTexture2D_6d80892c9aa2410b96cdbf438abed2c2_RGBA_0.a;
        float4 _Multiply_2975ea5f41bb4680a74280e106ac2c02_Out_2;
        Unity_Multiply_float4_float4(_Property_cba9a5289c1743f9a3315daf05cd9cdb_Out_0, _SampleTexture2D_6d80892c9aa2410b96cdbf438abed2c2_RGBA_0, _Multiply_2975ea5f41bb4680a74280e106ac2c02_Out_2);
        float4 Color_a426fac7938d4054afefc4ba96e8e91b = IsGammaSpace() ? float4(0.009433985, 0.009433985, 0.009433985, 0) : float4(SRGBToLinear(float3(0.009433985, 0.009433985, 0.009433985)), 0);
        float4 _Add_4c90deb4e3184939a6948966cb92c7eb_Out_2;
        Unity_Add_float4(_Multiply_2975ea5f41bb4680a74280e106ac2c02_Out_2, Color_a426fac7938d4054afefc4ba96e8e91b, _Add_4c90deb4e3184939a6948966cb92c7eb_Out_2);
        float _Property_2e3f711b91df4616a4e1009a8400f9c5_Out_0 = SpecExp1;
        float _Property_a7dc524f7d7f41b3a73e252340371abb_Out_0 = SpecExp2;
        float _Property_828930696e0d4bdb8025f32f65f8bea3_Out_0 = EnvRough;
        float _Property_fc25933395f1442e93301c53b6cd4556_Out_0 = EnvSpecMul;
        float _Property_58122324692b48478a497d6033b4c55b_Out_0 = SpecShift;        
        float _Property_a57a072cf73f4867b13bd93dd90eb41f_Out_0 = SpecShift2;
        float _Property_3cd608b8e5a3405e936c5244c2c22ea9_Out_0 = FlowMultipler;
        float _Property_b53f61254eb14eb5bd04afb66aee1d02_Out_0 = SpecMultiply;
        UnityTexture2D _Property_90f566a85eb348a7b74b81ac9b3e2d3f_Out_0 = RootToTipTex;
        float4 _SampleTexture2D_0890fc352230432888afc5ef918f2cf5_RGBA_0 = SAMPLE_TEXTURE2D(_Property_90f566a85eb348a7b74b81ac9b3e2d3f_Out_0.tex, _Property_90f566a85eb348a7b74b81ac9b3e2d3f_Out_0.samplerstate, _Property_90f566a85eb348a7b74b81ac9b3e2d3f_Out_0.GetTransformedUV(IN.uv0.xy));
        float _SampleTexture2D_0890fc352230432888afc5ef918f2cf5_R_4 = _SampleTexture2D_0890fc352230432888afc5ef918f2cf5_RGBA_0.r;
        float _SampleTexture2D_0890fc352230432888afc5ef918f2cf5_G_5 = _SampleTexture2D_0890fc352230432888afc5ef918f2cf5_RGBA_0.g;
        float _SampleTexture2D_0890fc352230432888afc5ef918f2cf5_B_6 = _SampleTexture2D_0890fc352230432888afc5ef918f2cf5_RGBA_0.b;
        float _SampleTexture2D_0890fc352230432888afc5ef918f2cf5_A_7 = _SampleTexture2D_0890fc352230432888afc5ef918f2cf5_RGBA_0.a;
        float _Property_0a96a5cd4e13453192c3a24fbd6f0b07_Out_0 = UseUniqueAOBool;
        UnityTexture2D _Property_cf0bfb6f352f4591ae3471657ac237e4_Out_0 = AOMapUniqueTex;
        float4 _SampleTexture2D_084ebc65b0fa4b45a573abda2b741bf1_RGBA_0 = SAMPLE_TEXTURE2D(_Property_cf0bfb6f352f4591ae3471657ac237e4_Out_0.tex, _Property_cf0bfb6f352f4591ae3471657ac237e4_Out_0.samplerstate, _Property_cf0bfb6f352f4591ae3471657ac237e4_Out_0.GetTransformedUV(IN.uv1.xy));
        float _SampleTexture2D_084ebc65b0fa4b45a573abda2b741bf1_R_4 = _SampleTexture2D_084ebc65b0fa4b45a573abda2b741bf1_RGBA_0.r;
        float _SampleTexture2D_084ebc65b0fa4b45a573abda2b741bf1_G_5 = _SampleTexture2D_084ebc65b0fa4b45a573abda2b741bf1_RGBA_0.g;
        float _SampleTexture2D_084ebc65b0fa4b45a573abda2b741bf1_B_6 = _SampleTexture2D_084ebc65b0fa4b45a573abda2b741bf1_RGBA_0.b;
        float _SampleTexture2D_084ebc65b0fa4b45a573abda2b741bf1_A_7 = _SampleTexture2D_084ebc65b0fa4b45a573abda2b741bf1_RGBA_0.a;
        UnityTexture2D _Property_bbacd12b90264212aeab48258cc23d0a_Out_0 = AOMapTex;
        float4 _SampleTexture2D_9b8794ca61224a598291a9c4722279b0_RGBA_0 = SAMPLE_TEXTURE2D(_Property_bbacd12b90264212aeab48258cc23d0a_Out_0.tex, _Property_bbacd12b90264212aeab48258cc23d0a_Out_0.samplerstate, _Property_bbacd12b90264212aeab48258cc23d0a_Out_0.GetTransformedUV(IN.uv0.xy));
        float _SampleTexture2D_9b8794ca61224a598291a9c4722279b0_R_4 = _SampleTexture2D_9b8794ca61224a598291a9c4722279b0_RGBA_0.r;
        float _SampleTexture2D_9b8794ca61224a598291a9c4722279b0_G_5 = _SampleTexture2D_9b8794ca61224a598291a9c4722279b0_RGBA_0.g;
        float _SampleTexture2D_9b8794ca61224a598291a9c4722279b0_B_6 = _SampleTexture2D_9b8794ca61224a598291a9c4722279b0_RGBA_0.b;
        float _SampleTexture2D_9b8794ca61224a598291a9c4722279b0_A_7 = _SampleTexture2D_9b8794ca61224a598291a9c4722279b0_RGBA_0.a;
        float _Branch_9ecbff7ea7d24e76937a3318551bc79b_Out_3;
        Unity_Branch_float(_Property_0a96a5cd4e13453192c3a24fbd6f0b07_Out_0, _SampleTexture2D_084ebc65b0fa4b45a573abda2b741bf1_R_4, _SampleTexture2D_9b8794ca61224a598291a9c4722279b0_R_4, _Branch_9ecbff7ea7d24e76937a3318551bc79b_Out_3);
        float _Property_6eb4f0e1c9bd428aab62642ba46f7056_Out_0 = AOFactor;
        float _Power_b9505bf771844e2b80c026aa62cd1267_Out_2;
        Unity_Power_float(_Branch_9ecbff7ea7d24e76937a3318551bc79b_Out_3, _Property_6eb4f0e1c9bd428aab62642ba46f7056_Out_0, _Power_b9505bf771844e2b80c026aa62cd1267_Out_2);
        float _Property_1351a9146b26414599a4a3d05da01917_Out_0 = ScatterFactor;
        float _Property_730581a92f0047da8e7af1b6b27bfa15_Out_0 = TransmissionStrength;
        float _Property_5ca5b4c60b694e92b05419543edbcb9a_Out_0 = TransmissionHaloSharpness;
        float3 _HairCustomFunction_4ea6a9d29116492bb5443a098eb2017f_outColor_7;
        Hair_float(_TangentBasis_99b609fdefef4cb48c6f9370b7ee1a0e_OutMatrix3_1, IN.WorldSpacePosition, IN.TangentSpacePosition, IN.TangentSpaceNormal, _Branch_ac0521a913584cababd1d2c6a0d26fad_Out_3, IN.TangentSpaceViewDirection, (_UV_518992b1b5f34744891eea6fcbd3d1bf_Out_0.xy), (_Add_4c90deb4e3184939a6948966cb92c7eb_Out_2.xyz), _Property_2e3f711b91df4616a4e1009a8400f9c5_Out_0, _Property_a7dc524f7d7f41b3a73e252340371abb_Out_0, _Property_828930696e0d4bdb8025f32f65f8bea3_Out_0, _Property_fc25933395f1442e93301c53b6cd4556_Out_0, _Property_58122324692b48478a497d6033b4c55b_Out_0, _Property_a57a072cf73f4867b13bd93dd90eb41f_Out_0, _Property_3cd608b8e5a3405e936c5244c2c22ea9_Out_0, _Property_b53f61254eb14eb5bd04afb66aee1d02_Out_0, (_SampleTexture2D_0890fc352230432888afc5ef918f2cf5_RGBA_0).x, _Power_b9505bf771844e2b80c026aa62cd1267_Out_2, _Property_1351a9146b26414599a4a3d05da01917_Out_0, _Property_730581a92f0047da8e7af1b6b27bfa15_Out_0, _Property_5ca5b4c60b694e92b05419543edbcb9a_Out_0, _HairCustomFunction_4ea6a9d29116492bb5443a098eb2017f_outColor_7);
        float3 _Branch_74b6b654e0ba4caabd32d0ccd64b2a32_Out_3;
        Unity_Branch_float3(_Property_fe24b67262284f7dafbfdc716c687c39_Out_0, IN.WorldSpaceNormal, _HairCustomFunction_4ea6a9d29116492bb5443a098eb2017f_outColor_7, _Branch_74b6b654e0ba4caabd32d0ccd64b2a32_Out_3);
        float3 _Branch_2fb8551b11644b90aafe5386f3a7ea87_Out_3;
        Unity_Branch_float3(_Property_b5e1480417d3448cbd7ca8c40fdd131a_Out_0, _Multiply_9147c43d534841f9a741a0f81b9949f2_Out_2, _Branch_74b6b654e0ba4caabd32d0ccd64b2a32_Out_3, _Branch_2fb8551b11644b90aafe5386f3a7ea87_Out_3);
        float _Property_a909180168704bc797e84f0cd117c638_Out_0 = SDFToggleBool;
        UnityTexture2D _Property_5bd501ccbd134c96bf4cae88423b3553_Out_0 = OpacityTex;
        float4 _SampleTexture2D_57d2f8dd9a5044ab911ed506bcb28dc9_RGBA_0 = SAMPLE_TEXTURE2D(_Property_5bd501ccbd134c96bf4cae88423b3553_Out_0.tex, _Property_5bd501ccbd134c96bf4cae88423b3553_Out_0.samplerstate, _Property_5bd501ccbd134c96bf4cae88423b3553_Out_0.GetTransformedUV(IN.uv0.xy));
        float _SampleTexture2D_57d2f8dd9a5044ab911ed506bcb28dc9_R_4 = _SampleTexture2D_57d2f8dd9a5044ab911ed506bcb28dc9_RGBA_0.r;
        float _SampleTexture2D_57d2f8dd9a5044ab911ed506bcb28dc9_G_5 = _SampleTexture2D_57d2f8dd9a5044ab911ed506bcb28dc9_RGBA_0.g;
        float _SampleTexture2D_57d2f8dd9a5044ab911ed506bcb28dc9_B_6 = _SampleTexture2D_57d2f8dd9a5044ab911ed506bcb28dc9_RGBA_0.b;
        float _SampleTexture2D_57d2f8dd9a5044ab911ed506bcb28dc9_A_7 = _SampleTexture2D_57d2f8dd9a5044ab911ed506bcb28dc9_RGBA_0.a;
        float _Property_b453a13264354b9cbd0f75d3fde9c7a8_Out_0 = AlphaClipThreshold;
        float _Property_3f6e684b93ba44d887cd645f44e7ee53_Out_0 = SDFSmoothing;
        float _Property_2cf820a4e9194b3ea25f3644ed96795b_Out_0 = SDFAAFactor;
        float _Property_8ede62c6989b4f9aa8008d32d9949767_Out_0 = SDFGamma;
        float _calcSDFAlphaCustomFunction_88d6ada0ce1d4afaa80960d34c037e04_New_0;
        calcSDFAlpha_float(float2 (0, 0), _SampleTexture2D_57d2f8dd9a5044ab911ed506bcb28dc9_R_4, _Property_b453a13264354b9cbd0f75d3fde9c7a8_Out_0, _Property_3f6e684b93ba44d887cd645f44e7ee53_Out_0, _Property_2cf820a4e9194b3ea25f3644ed96795b_Out_0, _Property_8ede62c6989b4f9aa8008d32d9949767_Out_0, _calcSDFAlphaCustomFunction_88d6ada0ce1d4afaa80960d34c037e04_New_0);
        float Slider_6ebd5e9dcadc42b393274992462bd42d = 0;
        float _Add_6173736479a640478cc85569c8f5d145_Out_2;
        Unity_Add_float(_SampleTexture2D_57d2f8dd9a5044ab911ed506bcb28dc9_G_5, Slider_6ebd5e9dcadc42b393274992462bd42d, _Add_6173736479a640478cc85569c8f5d145_Out_2);
        float _Multiply_a83b03889fb44eff9fb38ae293a43425_Out_2;
        Unity_Multiply_float_float(_calcSDFAlphaCustomFunction_88d6ada0ce1d4afaa80960d34c037e04_New_0, _Add_6173736479a640478cc85569c8f5d145_Out_2, _Multiply_a83b03889fb44eff9fb38ae293a43425_Out_2);
        float _Clamp_ee237f78f1e24d10a2e919b7f6567c9c_Out_3;
        Unity_Clamp_float(_Multiply_a83b03889fb44eff9fb38ae293a43425_Out_2, 0, 1, _Clamp_ee237f78f1e24d10a2e919b7f6567c9c_Out_3);
        float _Property_9b91636a824c42bca23b05676c15b3ca_Out_0 = AlphaLODBias;
        #if defined(SHADER_API_GLES) && (SHADER_TARGET < 30)
          float4 _SampleTexture2DLOD_ce92fc8562264a75b3a8083636864582_RGBA_0 = float4(0.0f, 0.0f, 0.0f, 1.0f);
        #else
          float4 _SampleTexture2DLOD_ce92fc8562264a75b3a8083636864582_RGBA_0 = SAMPLE_TEXTURE2D_LOD(_Property_5bd501ccbd134c96bf4cae88423b3553_Out_0.tex, UnityBuildSamplerStateStruct(SamplerState_Trilinear_Repeat).samplerstate, _Property_5bd501ccbd134c96bf4cae88423b3553_Out_0.GetTransformedUV(IN.uv0.xy), _Property_9b91636a824c42bca23b05676c15b3ca_Out_0);
        #endif
        float _SampleTexture2DLOD_ce92fc8562264a75b3a8083636864582_R_5 = _SampleTexture2DLOD_ce92fc8562264a75b3a8083636864582_RGBA_0.r;
        float _SampleTexture2DLOD_ce92fc8562264a75b3a8083636864582_G_6 = _SampleTexture2DLOD_ce92fc8562264a75b3a8083636864582_RGBA_0.g;
        float _SampleTexture2DLOD_ce92fc8562264a75b3a8083636864582_B_7 = _SampleTexture2DLOD_ce92fc8562264a75b3a8083636864582_RGBA_0.b;
        float _SampleTexture2DLOD_ce92fc8562264a75b3a8083636864582_A_8 = _SampleTexture2DLOD_ce92fc8562264a75b3a8083636864582_RGBA_0.a;
        float _Branch_77176c49b2e047359b88fb6f8a1c5152_Out_3;
        Unity_Branch_float(_Property_a909180168704bc797e84f0cd117c638_Out_0, _Clamp_ee237f78f1e24d10a2e919b7f6567c9c_Out_3, _SampleTexture2DLOD_ce92fc8562264a75b3a8083636864582_R_5, _Branch_77176c49b2e047359b88fb6f8a1c5152_Out_3);
        float _Property_6e52d610af6e4dbcb20468654e31b716_Out_0 = AlphaPower;
        float _Power_acb18f87651c403f82ea9f04f3b946f3_Out_2;
        Unity_Power_float(_Branch_77176c49b2e047359b88fb6f8a1c5152_Out_3, _Property_6e52d610af6e4dbcb20468654e31b716_Out_0, _Power_acb18f87651c403f82ea9f04f3b946f3_Out_2);
        AlphaClip_1 = _Property_6b1a2906e2b94f339fd3c3869fe68f9e_Out_0;
        Colour_2 = _Branch_2fb8551b11644b90aafe5386f3a7ea87_Out_3;
        Alpha_3 = _Power_acb18f87651c403f82ea9f04f3b946f3_Out_2;
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
            float3 _Add_06ae16e20f45417e8d8697ec13ce2250_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, _Multiply_272a1dfd302a429cb91114d47fd727e3_Out_2, _Add_06ae16e20f45417e8d8697ec13ce2250_Out_2);
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
        
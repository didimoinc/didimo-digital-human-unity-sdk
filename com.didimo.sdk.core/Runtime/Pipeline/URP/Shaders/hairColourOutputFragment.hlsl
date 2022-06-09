 // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float AlphaClipThreshold;
        };

      
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
                                
            SG_MainHairSubGraph_float(_specExp1, _specExp2, _EnvRough, _specShift, _specShift2, _flowMultiplier, _specMultiply, UnityBuildTexture2DStructNoScale(_SpecShift), UnityBuildTexture2DStructNoScale(_NormalMap), _alphaClipThreshold, UnityBuildTexture2DStructNoScale(_Albedo), UnityBuildTexture2DStructNoScale(_rootToTip), UnityBuildTexture2DStructNoScale(_flowMap), _AnisoHighlightRotation, _TestNormals, _TestTangents, _UseFlowMap, _AOFactor, _AOStrength, UnityBuildTexture2DStructNoScale(_Opacity), _AlphaPower, _AlphaLODbias, _UseUniqueAOMap, UnityBuildTexture2DStructNoScale(_AOMap), UnityBuildTexture2DStructNoScale(_AOMapUnique), _HairColor, _EnvSpecularScale, _TangentFlowMapRotation, _MeshTangentRotation, _SDF_smoothing, _SDF_AAFactor, _SDF_gamma, _SDF_toggle, _ScaterFactor, _TransmissionStrength, _TransmissionHaloSharpness, IN, surface.AlphaClipThreshold, surface.BaseColor, surface.Alpha);
            
            return surface;
        }
       
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
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
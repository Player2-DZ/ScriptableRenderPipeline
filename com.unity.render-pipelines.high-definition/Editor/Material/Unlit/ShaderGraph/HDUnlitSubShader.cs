using System.Collections.Generic;
using Data.Util;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering.HighDefinition
{
    [FormerName("UnityEditor.Experimental.Rendering.HDPipeline.HDUnlitSubShader")]
    class HDUnlitSubShader : IHDUnlitSubShader
    {
        Pass m_PassMETA = new Pass()
        {
            Name = "META",
            LightMode = "META",
            TemplateName = "HDUnlitPass.template",
            MaterialName = "Unlit",
            ShaderPassName = "SHADERPASS_LIGHT_TRANSPORT",
            CullOverride = "Cull Off",
            Includes = new List<string>()
            {
                "#include \"Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassLightTransport.hlsl\"",
            },
            RequiredFields = new List<string>()
            {
                "AttributesMesh.normalOS",
                "AttributesMesh.tangentOS",     // Always present as we require it also in case of Variants lighting
                "AttributesMesh.uv0",
                "AttributesMesh.uv1",
                "AttributesMesh.color",
                "AttributesMesh.uv2",           // SHADERPASS_LIGHT_TRANSPORT always uses uv2
            },
            PixelShaderSlots = new List<int>()
            {
                HDUnlitMasterNode.ColorSlotId,
                HDUnlitMasterNode.AlphaSlotId,
                HDUnlitMasterNode.AlphaThresholdSlotId,
                HDUnlitMasterNode.EmissionSlotId
            },
            VertexShaderSlots = new List<int>()
            {
                //HDUnlitMasterNode.PositionSlotId
            },
            UseInPreview = false
        };

        Pass m_SceneSelectionPass = new Pass()
        {
            Name = "SceneSelectionPass",
            LightMode = "SceneSelectionPass",
            TemplateName = "HDUnlitPass.template",
            MaterialName = "Unlit",
            ShaderPassName = "SHADERPASS_DEPTH_ONLY",
            ColorMaskOverride = "ColorMask 0",
            CullOverride = HDSubShaderUtilities.defaultCullMode,
            ZWriteOverride = HDSubShaderUtilities.zWriteOn,
            ExtraDefines = new List<string>()
            {
                "#define SCENESELECTIONPASS",
                "#pragma editor_sync_compilation",
            },
            Includes = new List<string>()
            {
                "#include \"Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassDepthOnly.hlsl\"",
            },
            PixelShaderSlots = new List<int>()
            {
                HDUnlitMasterNode.AlphaSlotId,
                HDUnlitMasterNode.AlphaThresholdSlotId
            },
            VertexShaderSlots = new List<int>()
            {
                HDUnlitMasterNode.PositionSlotId
            },
            UseInPreview = false,
        };

        Pass m_PassDepthForwardOnly = new Pass()
        {
            Name = "DepthForwardOnly",
            LightMode = "DepthForwardOnly",
            TemplateName = "HDUnlitPass.template",
            MaterialName = "Unlit",
            ShaderPassName = "SHADERPASS_DEPTH_ONLY",
            ZWriteOverride = HDSubShaderUtilities.zWriteOn,
            // Caution: When using MSAA we have normal and depth buffer bind.
            // Mean unlit object need to not write in it (or write 0) - Disable color mask for this RT
            // This is not a problem in no MSAA mode as there is no buffer bind
            ColorMaskOverride = "ColorMask 0 0",
            CullOverride = HDSubShaderUtilities.defaultCullMode,

            ExtraDefines = new List<string>()
            {
                "#pragma multi_compile _ WRITE_MSAA_DEPTH"
                // Note we don't need to define WRITE_NORMAL_BUFFER
            },

            Includes = new List<string>()
            {
                "#include \"Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassDepthOnly.hlsl\"",
            },
            PixelShaderSlots = new List<int>()
            {
                HDUnlitMasterNode.AlphaSlotId,
                HDUnlitMasterNode.AlphaThresholdSlotId
            },

            VertexShaderSlots = new List<int>()
            {
                HDUnlitMasterNode.PositionSlotId
            },
            UseInPreview = false,

            OnGeneratePassImpl = (IMasterNode node, ref Pass pass) =>
            {
                HDSubShaderUtilities.SetStencilStateForDepth(ref pass);
            }
        };

        Pass m_PassMotionVectors = new Pass()
        {
            Name = "MotionVectors",
            LightMode = "MotionVectors",
            TemplateName = "HDUnlitPass.template",
            MaterialName = "Unlit",
            ShaderPassName = "SHADERPASS_MOTION_VECTORS",
            CullOverride = HDSubShaderUtilities.defaultCullMode,
            ZWriteOverride = HDSubShaderUtilities.zWriteOn,
            // Caution: When using MSAA we have motion vector, normal and depth buffer bind.
            // Mean unlit object need to not write in it (or write 0) - Disable color mask for this RT
            // This is not a problem in no MSAA mode as there is no buffer bind
            ColorMaskOverride = "ColorMask 0 1",

            ExtraDefines = new List<string>()
            {
                "#pragma multi_compile _ WRITE_MSAA_DEPTH"
                // Note we don't need to define WRITE_NORMAL_BUFFER
            },
            Includes = new List<string>()
            {
                "#include \"Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassMotionVectors.hlsl\"",
            },
            RequiredFields = new List<string>()
            {
                "FragInputs.positionRWS",
            },
            PixelShaderSlots = new List<int>()
            {
                HDUnlitMasterNode.AlphaSlotId,
                HDUnlitMasterNode.AlphaThresholdSlotId
            },
            VertexShaderSlots = new List<int>()
            {
                HDUnlitMasterNode.PositionSlotId
            },
            UseInPreview = false,

            OnGeneratePassImpl = (IMasterNode node, ref Pass pass) =>
            {
                HDSubShaderUtilities.SetStencilStateForMotionVector(ref pass);
            }
        };

        Pass m_PassDistortion = new Pass()
        {
            Name = "DistortionVectors",
            LightMode = "DistortionVectors",
            TemplateName = "HDUnlitPass.template",
            MaterialName = "Unlit",
            ShaderPassName = "SHADERPASS_DISTORTION",
            CullOverride = HDSubShaderUtilities.defaultCullMode,
            ZWriteOverride = HDSubShaderUtilities.zWriteOff,
            Includes = new List<string>()
            {
                "#include \"Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassDistortion.hlsl\"",
            },
            PixelShaderSlots = new List<int>()
            {
                HDUnlitMasterNode.AlphaSlotId,
                HDUnlitMasterNode.AlphaThresholdSlotId,
                HDUnlitMasterNode.DistortionSlotId,
                HDUnlitMasterNode.DistortionBlurSlotId,
            },
            VertexShaderSlots = new List<int>()
            {
                HDUnlitMasterNode.PositionSlotId
            },
            UseInPreview = true,

            OnGeneratePassImpl = (IMasterNode node, ref Pass pass) =>
            {
                HDSubShaderUtilities.SetStencilStateForDistortionVector(ref pass);
                var masterNode = node as HDUnlitMasterNode;
                if (masterNode.distortionDepthTest.isOn)
                {
                    pass.ZTestOverride = "ZTest LEqual";
                }
                else
                {
                    pass.ZTestOverride = "ZTest Always";
                }
                if (masterNode.distortionMode == DistortionMode.Add)
                {
                    pass.BlendOverride = "Blend One One, One One";
                    pass.BlendOpOverride = "BlendOp Add, Add";
                }
                else if (masterNode.distortionMode == DistortionMode.Multiply)
                {
                    pass.BlendOverride = "Blend DstColor Zero, DstAlpha Zero";
                    pass.BlendOpOverride = "BlendOp Add, Add";
                }
                else // (masterNode.distortionMode == DistortionMode.Replace)
                {
                    pass.BlendOverride = "Blend One Zero, One Zero";
                    pass.BlendOpOverride = "BlendOp Add, Add";
                }
            }
        };

        Pass m_PassShadowCaster = new Pass()
        {
            Name = "ShadowCaster",
            LightMode = "ShadowCaster",
            TemplateName = "HDUnlitPass.template",
            MaterialName = "Unlit",
            ShaderPassName = "SHADERPASS_SHADOWS",
            ColorMaskOverride = "ColorMask 0",
            ZClipOverride = HDSubShaderUtilities.zClipShadowCaster,
            CullOverride = HDSubShaderUtilities.defaultCullMode,
            ZWriteOverride = HDSubShaderUtilities.zWriteOn,
            Includes = new List<string>()
            {
                "#include \"Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassDepthOnly.hlsl\"",
            },
            PixelShaderSlots = new List<int>()
            {
                HDUnlitMasterNode.AlphaSlotId,
                HDUnlitMasterNode.AlphaThresholdSlotId,
            },
            VertexShaderSlots = new List<int>()
            {
                HDUnlitMasterNode.PositionSlotId
            },
            UseInPreview = false,
        };

        Pass m_PassForwardOnly = new Pass()
        {
            Name = "ForwardOnly",
            LightMode = "ForwardOnly",
            TemplateName = "HDUnlitPass.template",
            MaterialName = "Unlit",
            ShaderPassName = "SHADERPASS_FORWARD_UNLIT",
            CullOverride = HDSubShaderUtilities.defaultCullMode,
            ZTestOverride = HDSubShaderUtilities.zTestTransparent,
            ZWriteOverride = HDSubShaderUtilities.ZWriteDefault,
            ExtraDefines = new List<string>()
            {
                "#pragma multi_compile _ DEBUG_DISPLAY"
            },
            Includes = new List<string>()
            {
                "#include \"Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassForwardUnlit.hlsl\"",
            },
            PixelShaderSlots = new List<int>()
            {
                HDUnlitMasterNode.ColorSlotId,
                HDUnlitMasterNode.AlphaSlotId,
                HDUnlitMasterNode.AlphaThresholdSlotId,
                HDUnlitMasterNode.EmissionSlotId
            },
            VertexShaderSlots = new List<int>()
            {
                HDUnlitMasterNode.PositionSlotId
            },
            UseInPreview = true,

            OnGeneratePassImpl = (IMasterNode node, ref Pass pass) =>
            {
                HDSubShaderUtilities.SetStencilStateForForward(ref pass);
                HDSubShaderUtilities.SetBlendModeForForward(ref pass);
            }
        };

        Pass m_PassRaytracingIndirect = new Pass()
        {
            Name = "IndirectDXR",
            LightMode = "IndirectDXR",
            TemplateName = "HDUnlitRaytracingPass.template",
            MaterialName = "Unlit",
            ShaderPassName = "SHADERPASS_RAYTRACING_INDIRECT",
            ExtraDefines = new List<string>()
            {
            },
            Includes = new List<string>()
            {
                "#include \"Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassRaytracingIndirect.hlsl\"",
            },
            PixelShaderSlots = new List<int>()
            {
                HDUnlitMasterNode.ColorSlotId,
                HDUnlitMasterNode.AlphaSlotId,
                HDUnlitMasterNode.AlphaThresholdSlotId,
                HDUnlitMasterNode.EmissionSlotId
            },
            VertexShaderSlots = new List<int>()
            {
                HDLitMasterNode.PositionSlotId
            },
            UseInPreview = false
        };

        Pass m_PassRaytracingVisibility = new Pass()
        {
            Name = "VisibilityDXR",
            LightMode = "VisibilityDXR",
            TemplateName = "HDUnlitRaytracingPass.template",
            MaterialName = "Unlit",
            ShaderPassName = "SHADERPASS_RAYTRACING_VISIBILITY",
            Includes = new List<string>()
            {
                "#include \"Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassRaytracingVisibility.hlsl\"",
            },
            PixelShaderSlots = new List<int>()
            {
                HDUnlitMasterNode.ColorSlotId,
                HDUnlitMasterNode.AlphaSlotId,
                HDUnlitMasterNode.AlphaThresholdSlotId,
                HDUnlitMasterNode.EmissionSlotId
            },
            VertexShaderSlots = new List<int>()
            {
                HDLitMasterNode.PositionSlotId
            },
            UseInPreview = false
        };

        Pass m_PassRaytracingForward = new Pass()
        {
            Name = "ForwardDXR",
            LightMode = "ForwardDXR",
            TemplateName = "HDUnlitRaytracingPass.template",
            MaterialName = "Unlit",
            ShaderPassName = "SHADERPASS_RAYTRACING_FORWARD",
            ExtraDefines = new List<string>()
            {
            },
            Includes = new List<string>()
            {
                "#include \"Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassRaytracingForward.hlsl\"",
            },
            PixelShaderSlots = new List<int>()
            {
                HDUnlitMasterNode.ColorSlotId,
                HDUnlitMasterNode.AlphaSlotId,
                HDUnlitMasterNode.AlphaThresholdSlotId,
                HDUnlitMasterNode.EmissionSlotId
            },
            VertexShaderSlots = new List<int>()
            {
                HDLitMasterNode.PositionSlotId
            },
            UseInPreview = false
        };

        Pass m_PassRaytracingGBuffer = new Pass()
        {
            Name = "GBufferDXR",
            LightMode = "GBufferDXR",
            TemplateName = "HDUnlitRaytracingPass.template",
            MaterialName = "Unlit",
            ShaderPassName = "SHADERPASS_RAYTRACING_GBUFFER",
            ExtraDefines = new List<string>()
            {
            },
            Includes = new List<string>()
            {
                "#include \"Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderpassRaytracingGBuffer.hlsl\"",
            },
            PixelShaderSlots = new List<int>()
            {
                HDUnlitMasterNode.ColorSlotId,
                HDUnlitMasterNode.AlphaSlotId,
                HDUnlitMasterNode.AlphaThresholdSlotId,
                HDUnlitMasterNode.EmissionSlotId
            },
            VertexShaderSlots = new List<int>()
            {
                HDLitMasterNode.PositionSlotId
            },
            UseInPreview = false
        };

        public int GetPreviewPassIndex() { return 0; }

        private static ActiveFields GetActiveFieldsFromMasterNode(AbstractMaterialNode iMasterNode, Pass pass)
        {
            var activeFields = new ActiveFields();
            var baseActiveFields = activeFields.baseInstance;

            HDUnlitMasterNode masterNode = iMasterNode as HDUnlitMasterNode;
            if (masterNode == null)
            {
                return activeFields;
            }

            if (masterNode.alphaTest.isOn && pass.PixelShaderUsesSlot(HDUnlitMasterNode.AlphaThresholdSlotId))
            {
                baseActiveFields.Add("AlphaTest");
            }

            if (masterNode.surfaceType != SurfaceType.Opaque)
            {
                if (masterNode.transparencyFog.isOn)
                {
                    baseActiveFields.Add("AlphaFog");
                }
            }

            if (masterNode.addPrecomputedVelocity.isOn)
            {
                baseActiveFields.Add("AddPrecomputedVelocity");
            }

            return activeFields;
        }

        private static bool GenerateShaderPassUnlit(HDUnlitMasterNode masterNode, Pass pass, GenerationMode mode, ShaderGenerator result, List<string> sourceAssetDependencyPaths)
        {
            if (mode == GenerationMode.ForReals || pass.UseInPreview)
            {
                pass.OnGeneratePass(masterNode);

                // apply master node options to active fields
                var activeFields = GetActiveFieldsFromMasterNode(masterNode, pass);

                // use standard shader pass generation
                bool vertexActive = masterNode.IsSlotConnected(HDUnlitMasterNode.PositionSlotId);
                return HDSubShaderUtilities.GenerateShaderPass(masterNode, pass, mode, activeFields, result, sourceAssetDependencyPaths, vertexActive);
            }
            else
            {
                return false;
            }
        }

        public string GetSubshader(IMasterNode iMasterNode, GenerationMode mode, List<string> sourceAssetDependencyPaths = null)
        {
            if (sourceAssetDependencyPaths != null)
            {
                // HDUnlitSubShader.cs
                sourceAssetDependencyPaths.Add(AssetDatabase.GUIDToAssetPath("1c44ec077faa54145a89357de68e5d26"));
                // HDSubShaderUtilities.cs
                sourceAssetDependencyPaths.Add(AssetDatabase.GUIDToAssetPath("713ced4e6eef4a44799a4dd59041484b"));
            }

            var masterNode = iMasterNode as HDUnlitMasterNode;

            var subShader = new ShaderGenerator();
            subShader.AddShaderChunk("SubShader", true);
            subShader.AddShaderChunk("{", true);
            subShader.Indent();
            {
                // Add tags at the SubShader level
                int queue = HDRenderQueue.ChangeType(masterNode.renderingPass, masterNode.sortPriority, masterNode.alphaTest.isOn);
                HDSubShaderUtilities.AddTags(subShader, HDRenderPipeline.k_ShaderTagName, HDRenderTypeTags.HDUnlitShader, queue);

                // For preview only we generate the passes that are enabled
                bool opaque = (masterNode.surfaceType == SurfaceType.Opaque);
                bool transparent = !opaque;
                bool distortionActive = transparent && masterNode.distortion.isOn;

                GenerateShaderPassUnlit(masterNode, m_PassShadowCaster, mode, subShader, sourceAssetDependencyPaths);
                GenerateShaderPassUnlit(masterNode, m_PassMETA, mode, subShader, sourceAssetDependencyPaths);
                GenerateShaderPassUnlit(masterNode, m_SceneSelectionPass, mode, subShader, sourceAssetDependencyPaths);

                GenerateShaderPassUnlit(masterNode, m_PassDepthForwardOnly, mode, subShader, sourceAssetDependencyPaths);
                GenerateShaderPassUnlit(masterNode, m_PassMotionVectors, mode, subShader, sourceAssetDependencyPaths);

                if (distortionActive)
                {
                    GenerateShaderPassUnlit(masterNode, m_PassDistortion, mode, subShader, sourceAssetDependencyPaths);
                }

                GenerateShaderPassUnlit(masterNode, m_PassForwardOnly, mode, subShader, sourceAssetDependencyPaths);
            }
            subShader.Deindent();
            subShader.AddShaderChunk("}", false);

#if ENABLE_RAYTRACING
            if (mode == GenerationMode.ForReals)
            {
                subShader.AddShaderChunk("SubShader", false);
                subShader.AddShaderChunk("{", false);
                subShader.Indent();
                {
                    GenerateShaderPassUnlit(masterNode, m_PassRaytracingIndirect, mode, subShader, sourceAssetDependencyPaths);
                    GenerateShaderPassUnlit(masterNode, m_PassRaytracingVisibility, mode, subShader, sourceAssetDependencyPaths);
                    GenerateShaderPassUnlit(masterNode, m_PassRaytracingForward, mode, subShader, sourceAssetDependencyPaths);
                    GenerateShaderPassUnlit(masterNode, m_PassRaytracingGBuffer, mode, subShader, sourceAssetDependencyPaths);
                }
                subShader.Deindent();
                subShader.AddShaderChunk("}", false);
            }
#endif

            subShader.AddShaderChunk(@"CustomEditor ""UnityEditor.Rendering.HighDefinition.HDUnlitGUI""");

            return subShader.GetShaderString(0);
        }

        public bool IsPipelineCompatible(RenderPipelineAsset renderPipelineAsset)
        {
            return renderPipelineAsset is HDRenderPipelineAsset;
        }
    }
}

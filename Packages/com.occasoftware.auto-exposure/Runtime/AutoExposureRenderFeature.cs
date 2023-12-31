using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OccaSoftware.Exposure.Runtime
{
#pragma warning disable 0618
    public class AutoExposureRenderFeature : ScriptableRendererFeature
    {
        private const string profilerTag = "Auto Exposure";
        private const string cmdBufferName = "AEPass";

        /// <summary>
        /// Conducts Auto Exposure using the Fragment Shader approach.
        /// </summary>
        class AutoExposureFragmentRenderPass : ScriptableRenderPass
        {
            private RenderTargetIdentifier source;

            private RenderTargetHandle target;
            private RenderTargetHandle[] downscaledSource = new RenderTargetHandle[3];
            private RenderTexture perFrameDataRt = null;

            private Material calculateExposure = null;
            private Material applyExposure = null;
            private Material blitData = null;
            private Material blitScreen = null;

            private const string calculateExposurePath = "OccaSoftware/AutoExposure/FragmentCalculateExposure";
            private const string applyExposurePath = "OccaSoftware/AutoExposure/FragmentApply";
            private const string blitDataPath = "OccaSoftware/AutoExposure/FragmentBlitData";
            private const string blitScreenPath = "OccaSoftware/AutoExposure/BlitScreen";

            private Dictionary<Camera, RenderTexture> kvp = new Dictionary<Camera, RenderTexture>();

            private AutoExposure autoExposure;

            private bool isFirst = true;

            private const string persistentDataId = "_AutoExposureDataPrevious";
            private const string outputId = "_AutoExposureResults";
            private const string perFrameDataId = "_AutoExposureData";
            private const string downscaleId = "_DownscaleResults";

            public AutoExposureFragmentRenderPass()
            {
                kvp = new Dictionary<Camera, RenderTexture>();
                target.Init(outputId);
                for (int i = 0; i < downscaledSource.Length; i++)
                {
                    downscaledSource[i].Init(downscaleId + i);
                }
            }

            public void Setup(AutoExposure autoExposure)
            {
                this.autoExposure = autoExposure;
            }

            private void SetupMaterials()
            {
                GetShaderAndSetupMaterial(calculateExposurePath, ref calculateExposure);
                GetShaderAndSetupMaterial(applyExposurePath, ref applyExposure);
                GetShaderAndSetupMaterial(blitDataPath, ref blitData);
                GetShaderAndSetupMaterial(blitScreenPath, ref blitScreen);
            }

            /// <summary>
            /// Grabs the shader from path string and creates the material.
            /// If the material is already assigned, does nothing.
            /// If the path is null or invalid, does nothing.
            /// </summary>
            /// <param name="path">The shader path</param>
            /// <param name="material">The material to setup.</param>
            private void GetShaderAndSetupMaterial(string path, ref Material material)
            {
                if (material != null)
                    return;

                Shader s = Shader.Find(path);
                if (s != null)
                {
                    material = CoreUtils.CreateEngineMaterial(s);
                }
                else
                {
                    Debug.Log("Missing shader reference at " + path);
                }
            }

            private void ClearRenderTexture(RenderTexture renderTexture)
            {
                RenderTexture rt = RenderTexture.active;
                RenderTexture.active = renderTexture;
                GL.Clear(true, true, Color.black);
                RenderTexture.active = rt;
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                SetupMaterials();

                RenderTextureDescriptor targetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
                targetDescriptor.colorFormat = RenderTextureFormat.DefaultHDR;
                targetDescriptor.msaaSamples = 1;
                cmd.GetTemporaryRT(target.id, targetDescriptor);
                cmd.SetRenderTarget(target.id);
                cmd.ClearRenderTarget(true, true, Color.black);

                // Luminance Setup
                RenderTextureDescriptor luminanceDescriptor = renderingData.cameraData.cameraTargetDescriptor;
                luminanceDescriptor.colorFormat = RenderTextureFormat.DefaultHDR;
                luminanceDescriptor.width = 1;
                luminanceDescriptor.height = 1;
                luminanceDescriptor.depthBufferBits = 0;

                if (perFrameDataRt == null)
                {
                    perFrameDataRt = new RenderTexture(luminanceDescriptor);
                    perFrameDataRt.name = renderingData.cameraData.camera + " Per Frame Luminance Data";
                    perFrameDataRt.Create();
                }

                ClearRenderTexture(perFrameDataRt);

                if (!kvp.TryGetValue(renderingData.cameraData.camera, out RenderTexture _))
                {
                    RenderTexture rt = new RenderTexture(luminanceDescriptor);
                    rt.name = renderingData.cameraData.camera + " Permanent Luminance Data";
                    rt.Create();

                    ClearRenderTexture(rt);
                    kvp.Add(renderingData.cameraData.camera, rt);
                }

                RenderTextureDescriptor tempDescriptor = renderingData.cameraData.cameraTargetDescriptor;
                tempDescriptor.colorFormat = RenderTextureFormat.DefaultHDR;
                for (int i = 0; i < downscaledSource.Length; i++)
                {
                    tempDescriptor.width /= 2;
                    tempDescriptor.height /= 2;
                    cmd.GetTemporaryRT(downscaledSource[i].id, tempDescriptor);
                    cmd.SetRenderTarget(downscaledSource[i].id);
                    cmd.ClearRenderTarget(true, true, Color.black);
                }
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                UnityEngine.Profiling.Profiler.BeginSample(profilerTag);
                CommandBuffer cmd = CommandBufferPool.Get(cmdBufferName);

                source = renderingData.cameraData.renderer.cameraColorTarget;

                /*
                Steps:
                Get and write the Auto Exposure parameters.
                Load the previous data to the global texture defines.

                Write to the current data texture.
                
                Write to the output target using the data texture vars.
                Write to the screen.

                Write to the persistent data texture for next frame.
                */

                // Setup
                RenderTargetIdentifier persistentDataRTIdentifier = new RenderTargetIdentifier(kvp[renderingData.cameraData.camera]);
                RenderTargetIdentifier perFrameDataRTIdentifier = new RenderTargetIdentifier(perFrameDataRt);

                // Load the previous Luminance
                cmd.SetGlobalTexture(persistentDataId, persistentDataRTIdentifier);
                WriteShaderParams(ref cmd, renderingData.cameraData.isSceneViewCamera);

                // Downscale
                cmd.SetRenderTarget(downscaledSource[0].Identifier());
                cmd.SetGlobalTexture("_Source", source);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, blitScreen);

                cmd.SetRenderTarget(downscaledSource[1].Identifier());
                cmd.SetGlobalTexture("_Source", downscaledSource[0].Identifier());
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, blitScreen);

                cmd.SetRenderTarget(downscaledSource[2].Identifier());
                cmd.SetGlobalTexture("_Source", downscaledSource[1].Identifier());
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, blitScreen);

                // Calculate the auto exposure data (by rendering to the _AutoExposureData texture).
                cmd.SetGlobalTexture("_Source", downscaledSource[2].Identifier());
                cmd.SetRenderTarget(perFrameDataRTIdentifier);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, calculateExposure);

                // Apply the Exposure
                cmd.SetRenderTarget(target.Identifier());
                cmd.SetGlobalTexture("_Source", source);
                cmd.SetGlobalTexture(perFrameDataId, perFrameDataRTIdentifier);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, applyExposure);

                cmd.SetRenderTarget(source);
                cmd.SetGlobalTexture("_Source", target.Identifier());
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, blitScreen);

                // Write to the previous luminance for next frame
                cmd.SetRenderTarget(persistentDataRTIdentifier);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, blitData);

                // Reset target
                cmd.SetRenderTarget(source);

                // Execute and Clear
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
                UnityEngine.Profiling.Profiler.EndSample();
            }

            /// <summary>
            /// Sets global shader variables corresponding to the property values in the interpolated auto exposure volume stack component.
            /// </summary>
            /// <param name="cmd"></param>
            /// <param name="isSceneView"></param>
            private void WriteShaderParams(ref CommandBuffer cmd, bool isSceneView)
            {
                // Non-Compute-specific
                cmd.SetGlobalInteger(ShaderParams._SampleCount, autoExposure.sampleCount.value);
                cmd.SetGlobalFloat(ShaderParams._Response, autoExposure.response.value);
                cmd.SetGlobalInteger(ShaderParams._ClampingEnabled, (int)autoExposure.clampingEnabled.value);
                cmd.SetGlobalFloat(ShaderParams._ClampingBracket, autoExposure.clampingBracket.value);
                cmd.SetGlobalInteger(ShaderParams._AnimateSamplePositions, (int)autoExposure.animateSamplePositions.value);

                cmd.SetGlobalInteger(ShaderParams._IsFirstFrame, isFirst ? 1 : 0);

                AutoExposureAdaptationMode adaptationMode = autoExposure.adaptationMode.value;
                // Progressive rendering doesn't perform accurately for scene view because of how deltatime works in editor.
                // So, we force the adaptation mode to instant.
                if (isSceneView)
                    adaptationMode = AutoExposureAdaptationMode.Instant;

                // Progressive rendering doesn't work for game view when play mode is off.
                // So, we force the adaptation mode to instant.
                if (!isSceneView && !Application.isPlaying)
                    adaptationMode = AutoExposureAdaptationMode.Instant;

                // General

                cmd.SetGlobalInteger(ShaderParams._MeteringMaskMode, (int)autoExposure.meteringMaskMode.value);
                cmd.SetGlobalTexture(ShaderParams._MeteringMaskTexture, autoExposure.meteringMaskTexture.value);
                cmd.SetGlobalFloat(ShaderParams._MeteringProceduralFalloff, autoExposure.meteringProceduralFalloff.value);
                cmd.SetGlobalInteger(ShaderParams._AdaptationMode, (int)adaptationMode);
                cmd.SetGlobalFloat(ShaderParams._FixedCompensation, autoExposure.evCompensation.value);
                cmd.SetGlobalFloat(ShaderParams._DarkToLightSpeed, autoExposure.darkToLightSpeed.value);
                cmd.SetGlobalFloat(ShaderParams._LightToDarkSpeed, autoExposure.lightToDarkSpeed.value);
                cmd.SetGlobalFloat(ShaderParams._EvMin, autoExposure.evMin.value);
                cmd.SetGlobalFloat(ShaderParams._EvMax, autoExposure.evMax.value);
                cmd.SetGlobalTexture(ShaderParams._ExposureCompensationCurve, autoExposure.compensationCurveParameter.value.GetTexture());
            }

            public override void OnCameraCleanup(CommandBuffer cmd)
            {
                isFirst = false;

                for (int i = 0; i < downscaledSource.Length; i++)
                {
                    cmd.ReleaseTemporaryRT(downscaledSource[i].id);
                }
            }

            private static class ShaderParams
            {
                public static int _SampleCount = Shader.PropertyToID("_SampleCount");
                public static int _Response = Shader.PropertyToID("_Response");
                public static int _ClampingEnabled = Shader.PropertyToID("_ClampingEnabled");
                public static int _ClampingBracket = Shader.PropertyToID("_ClampingBracket");
                public static int _AnimateSamplePositions = Shader.PropertyToID("_AnimateSamplePositions");
                public static int _IsFirstFrame = Shader.PropertyToID("_IsFirstFrame");

                public static int _MeteringMaskMode = Shader.PropertyToID("_MeteringMaskMode");
                public static int _MeteringMaskTexture = Shader.PropertyToID("_MeteringMaskTexture");
                public static int _MeteringProceduralFalloff = Shader.PropertyToID("_MeteringProceduralFalloff");
                public static int _AdaptationMode = Shader.PropertyToID("_AdaptationMode");
                public static int _FixedCompensation = Shader.PropertyToID("_FixedCompensation");
                public static int _DarkToLightSpeed = Shader.PropertyToID("_DarkToLightSpeed");
                public static int _LightToDarkSpeed = Shader.PropertyToID("_LightToDarkSpeed");
                public static int _EvMin = Shader.PropertyToID("_EvMin");
                public static int _EvMax = Shader.PropertyToID("_EvMax");
                public static int _ExposureCompensationCurve = Shader.PropertyToID("_ExposureCompensationCurve");
            }
        }

        /// <summary>
        /// Conducts Auto Exposure using the Compute Shader approach. Requires SM4.5+.
        /// </summary>
        class AutoExposureComputeRenderPass : ScriptableRenderPass
        {
            private const string shaderName = "os-AutoExposureCompute";

            private ComputeShader shader;
            private ComputeBuffer constDataBuffer;

            private int mainKernel;
            private int updateKernel;
            private int rtKernel;
            private bool isFirst = true;

            private uint threadGroupSizeX;
            private uint threadGroupSizeY;
            private RenderTargetHandle aeHandle;
            private const string handleId = "AutoExposureHandle";

            private Dictionary<Camera, ComputeBuffer> camBufferPairs = new Dictionary<Camera, ComputeBuffer>();
            private AutoExposure autoExposure;

            public AutoExposureComputeRenderPass()
            {
                aeHandle.Init(handleId);

                constDataBuffer = new ComputeBuffer(1, sizeof(int) * 3 + sizeof(float) * 7 + sizeof(uint) * 2);

#if UNITY_EDITOR
				UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += OnAssemblyReload;
#endif
            }

            public void Setup(AutoExposure autoExposure)
            {
                this.autoExposure = autoExposure;
            }

            public bool LoadComputeShader()
            {
                shader = (ComputeShader)Resources.Load(shaderName);
                if (shader == null)
                    return false;

                return true;
            }

            private void OnAssemblyReload()
            {
                foreach (KeyValuePair<Camera, ComputeBuffer> entry in camBufferPairs)
                {
                    entry.Value.Release();
                }

                constDataBuffer.Release();
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                Camera cam = renderingData.cameraData.camera;
                RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;

                SetupCameraBufferPair(cam);
                SetupRenderTexture(descriptor);
                SetupKernel();

                void SetupCameraBufferPair(Camera cam)
                {
                    if (!camBufferPairs.TryGetValue(cam, out ComputeBuffer buffer))
                    {
                        camBufferPairs.Add(cam, new ComputeBuffer(1, sizeof(uint) * 2 + sizeof(float) * 2));
                    }
                }

                void SetupRenderTexture(RenderTextureDescriptor descriptor)
                {
                    descriptor.enableRandomWrite = true;
                    descriptor.msaaSamples = 1;
                    cmd.GetTemporaryRT(aeHandle.id, descriptor);
                }

                void SetupKernel()
                {
                    mainKernel = shader.FindKernel("AutoExposure");
                    updateKernel = shader.FindKernel("UpdateTargetLum");
                    rtKernel = shader.FindKernel("AdjustExposure");
                    shader.GetKernelThreadGroupSizes(mainKernel, out threadGroupSizeX, out threadGroupSizeY, out _);
                }
            }

            private struct ConstantData
            {
                public ConstantData(
                    float evMin,
                    float evMax,
                    float evCompensation,
                    int adaptationMode,
                    float darkToLightInterp,
                    float lightToDarkInterp,
                    float deltaTime,
                    uint screenSizeX,
                    uint screenSizeY,
                    int isFirstFrame,
                    int meteringMaskMode,
                    float meteringProceduralFalloff
                )
                {
                    this.evMin = evMin;
                    this.evMax = evMax;
                    this.evCompensation = evCompensation;
                    this.adaptationMode = adaptationMode;
                    this.darkToLightInterp = darkToLightInterp;
                    this.lightToDarkInterp = lightToDarkInterp;
                    this.deltaTime = deltaTime;
                    this.screenSizeX = screenSizeX;
                    this.screenSizeY = screenSizeY;
                    this.isFirstFrame = isFirstFrame;
                    this.meteringMaskMode = meteringMaskMode;
                    this.meteringProceduralFalloff = meteringProceduralFalloff;
                }

                public float evMin;
                public float evMax;
                public float evCompensation;
                public int adaptationMode;
                public float darkToLightInterp;
                public float lightToDarkInterp;
                public float deltaTime;
                public uint screenSizeX;
                public uint screenSizeY;
                public int isFirstFrame;
                public int meteringMaskMode;
                public float meteringProceduralFalloff;
            };

            private static class ShaderParams
            {
                public static int MeteringMaskTexture = Shader.PropertyToID("MeteringMaskTexture");
                public static int Constants = Shader.PropertyToID("Constants");
                public static int Data = Shader.PropertyToID("Data");
                public static int _AutoExposureTarget = Shader.PropertyToID("_AutoExposureTarget");

                public static int ExposureCompensationCurve = Shader.PropertyToID("ExposureCompensationCurve");
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                UnityEngine.Profiling.Profiler.BeginSample(profilerTag);
                CommandBuffer cmd = CommandBufferPool.Get(cmdBufferName);

                Camera cam = renderingData.cameraData.camera;
                RenderTargetIdentifier source = renderingData.cameraData.renderer.cameraColorTarget;
                RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
                desc.width = (int)(ScalableBufferManager.widthScaleFactor * desc.width);
                desc.height = (int)(ScalableBufferManager.heightScaleFactor * desc.height);
                bool isSceneView = renderingData.cameraData.isSceneViewCamera;
                bool xrRendering = renderingData.cameraData.xrRendering;
                ExecuteComputeShader(cmd);

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);

                void ExecuteComputeShader(CommandBuffer cmd)
                {
                    cmd.Blit(source, aeHandle.Identifier());
                    SetupConstantData();

                    int groupsX = GetGroupCount(desc.width, threadGroupSizeX);
                    int groupsY = GetGroupCount(desc.height, threadGroupSizeY);

                    RenderTargetIdentifier meteringMaskIdentifier = new RenderTargetIdentifier(autoExposure.meteringMaskTexture.value);
                    cmd.SetComputeTextureParam(shader, mainKernel, ShaderParams.MeteringMaskTexture, meteringMaskIdentifier);

                    cmd.SetComputeBufferParam(shader, mainKernel, ShaderParams.Constants, constDataBuffer);
                    cmd.SetComputeBufferParam(shader, mainKernel, ShaderParams.Data, camBufferPairs[cam]);
                    cmd.SetComputeTextureParam(shader, mainKernel, ShaderParams._AutoExposureTarget, aeHandle.id);

                    int computeShaderPassCount = 1;
                    if (xrRendering && XRGraphics.stereoRenderingMode == XRGraphics.StereoRenderingMode.SinglePassInstanced)
                    {
                        computeShaderPassCount = 2;
                    }
                    cmd.DispatchCompute(shader, mainKernel, groupsX, groupsY, computeShaderPassCount);

                    RenderTargetIdentifier exposureCompensationCurveId = new RenderTargetIdentifier(
                        autoExposure.compensationCurveParameter.value.GetTexture()
                    );
                    cmd.SetComputeTextureParam(shader, updateKernel, ShaderParams.ExposureCompensationCurve, exposureCompensationCurveId);
                    cmd.SetComputeBufferParam(shader, updateKernel, ShaderParams.Constants, constDataBuffer);
                    cmd.SetComputeBufferParam(shader, updateKernel, ShaderParams.Data, camBufferPairs[cam]);
                    cmd.DispatchCompute(shader, updateKernel, 1, 1, 1);

                    cmd.SetComputeBufferParam(shader, rtKernel, ShaderParams.Constants, constDataBuffer);
                    cmd.SetComputeBufferParam(shader, rtKernel, ShaderParams.Data, camBufferPairs[cam]);
                    cmd.SetComputeTextureParam(shader, rtKernel, ShaderParams._AutoExposureTarget, aeHandle.id);
                    cmd.DispatchCompute(shader, rtKernel, groupsX, groupsY, computeShaderPassCount);

                    cmd.Blit(aeHandle.Identifier(), source);

                    int GetGroupCount(int textureDimension, uint groupSize)
                    {
                        return Mathf.CeilToInt((textureDimension + groupSize - 1) / groupSize);
                    }

                    void SetupConstantData()
                    {
                        AutoExposureAdaptationMode adaptationMode = autoExposure.adaptationMode.value;

                        // Progressive rendering doesn't perform accurately for scene view because of how deltatime works in editor.
                        // So, we force the adaptation mode to instant.
                        if (isSceneView)
                            adaptationMode = AutoExposureAdaptationMode.Instant;

                        // Progressive rendering doesn't work for game view when play mode is off.
                        // So, we force the adaptation mode to instant.
                        if (!isSceneView && !Application.isPlaying)
                            adaptationMode = AutoExposureAdaptationMode.Instant;

                        ConstantData constants = new ConstantData(
                            autoExposure.evMin.value,
                            autoExposure.evMax.value,
                            autoExposure.evCompensation.value,
                            (int)adaptationMode,
                            autoExposure.darkToLightSpeed.value,
                            autoExposure.lightToDarkSpeed.value,
                            Time.deltaTime,
                            (uint)desc.width,
                            (uint)desc.height,
                            isFirst ? 1 : 0,
                            (int)autoExposure.meteringMaskMode.value,
                            autoExposure.meteringProceduralFalloff.value
                        );

                        cmd.SetBufferData(constDataBuffer, new ConstantData[] { constants });
                    }
                }
                UnityEngine.Profiling.Profiler.EndSample();
            }

            public override void OnCameraCleanup(CommandBuffer cmd)
            {
                isFirst = false;
                cmd.ReleaseTemporaryRT(aeHandle.id);
            }
        }

        private AutoExposure autoExposure;
        private AutoExposureComputeRenderPass computeRenderPass = null;
        private AutoExposureFragmentRenderPass fragmentRenderPass = null;
        private const RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        private bool DeviceSupportsComputeShaders()
        {
            const int _COMPUTE_SHADER_LEVEL = 45;
            if (SystemInfo.graphicsShaderLevel >= _COMPUTE_SHADER_LEVEL)
                return true;

            return false;
        }

        private bool DeviceHasXRSinglePassInstancedRenderingEnabled()
        {
            if (XRGraphics.enabled && XRGraphics.stereoRenderingMode == XRGraphics.StereoRenderingMode.SinglePassInstanced)
                return true;

            return false;
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
			UnityEditor.SceneManagement.EditorSceneManager.activeSceneChanged += Recreate;
			UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode += Recreate;
#endif
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += Recreate;
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
			UnityEditor.SceneManagement.EditorSceneManager.activeSceneChanged -= Recreate;
			UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode -= Recreate;
#endif
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= Recreate;
        }

        private void Recreate(UnityEngine.SceneManagement.Scene current, UnityEngine.SceneManagement.Scene next)
        {
            Create();
        }

        public override void Create()
        {
            Setup();
        }

        /// <summary>
        /// Clears the two render passes.
        /// Initializing the pass is handled later, during AddRenderPasses.
        /// </summary>
        internal void Setup()
        {
            computeRenderPass = null;
            fragmentRenderPass = null;
            warningReported = false;
        }

        /// <summary>
        /// Get the Auto Exposure component from the Volume Manager stack.
        /// </summary>
        /// <returns>If Auto Exposure component is null or inactive, returns false.</returns>
        internal bool RegisterAutoExposureStackComponent()
        {
            autoExposure = VolumeManager.instance.stack.GetComponent<AutoExposure>();
            if (autoExposure == null)
                return false;

            return autoExposure.IsActive();
        }

        private void InitializeCompute()
        {
            if (computeRenderPass == null)
            {
                computeRenderPass = new AutoExposureComputeRenderPass();
                computeRenderPass.renderPassEvent = renderPassEvent;
            }

            if (fragmentRenderPass != null)
            {
                fragmentRenderPass = null;
            }
        }

        private void InitializeFragment()
        {
            if (fragmentRenderPass == null)
            {
                fragmentRenderPass = new AutoExposureFragmentRenderPass();
                fragmentRenderPass.renderPassEvent = renderPassEvent;
            }

            if (computeRenderPass != null)
            {
                computeRenderPass = null;
            }
        }

        private bool warningReported = false;

        /// <summary>
        /// Validates the auto exposure stack component.
        /// Validates the relevant render pass.
        /// Sets up the relevant render pass and enqueues it.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="renderingData"></param>
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (IsExcludedCameraType(renderingData.cameraData.camera.cameraType))
                return;

            bool isActive = RegisterAutoExposureStackComponent();
            if (!isActive)
                return;

            if (autoExposure.renderingMode.value == AutoExposureRenderingMode.Compute && DeviceSupportsComputeShaders())
            {
                if (DeviceHasXRSinglePassInstancedRenderingEnabled())
                {
                    if (!warningReported)
                    {
                        Debug.LogWarning(
                            "Rendering mode is set to compute, but Auto Exposure's compute mode is incompatible with XR SPI. Switch to Fragment mode for Auto Exposure to work."
                        );
                        warningReported = true;
                    }

                    return;
                }

                InitializeCompute();

                bool hasValidComputeShader = computeRenderPass.LoadComputeShader();
                if (!hasValidComputeShader)
                    return;

                computeRenderPass.Setup(autoExposure);
                renderer.EnqueuePass(computeRenderPass);
            }
            else
            {
                InitializeFragment();
                fragmentRenderPass.Setup(autoExposure);
                renderer.EnqueuePass(fragmentRenderPass);
            }

            bool IsExcludedCameraType(CameraType type)
            {
                switch (type)
                {
                    case CameraType.Preview:
                        return true;
                    case CameraType.Reflection:
                        return true;
                    default:
                        return false;
                }
            }
        }
    }
#pragma warning restore 0618
}

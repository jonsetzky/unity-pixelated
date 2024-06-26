using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace Pixelated
{
    [DisallowMultipleRendererFeature("Pixelate")]
    public class PixelRenderFeature : ScriptableRendererFeature
    {
        private RTHandle m_ColorHandle = null;
        private RTHandle m_DepthHandle = null;

        [Range(1f, 15f)]
        public float m_PixelDensity = 6.0f;

        private RenderPassEvent m_RPEvent = RenderPassEvent.AfterRenderingSkybox;

        PixelRenderPass m_RenderPass = null;

        public override void AddRenderPasses(
            ScriptableRenderer renderer,
            ref RenderingData renderingData
        )
        {
            if (
                renderingData.cameraData.cameraType == CameraType.Game
                || renderingData.cameraData.cameraType == CameraType.SceneView
            )
                renderer.EnqueuePass(m_RenderPass);
        }

        private Vector2Int Scale(Vector2Int size)
        {
            float scale = Mathf.Lerp(
                1.0f,
                0.01f,
                Mathf.Pow(1 - Mathf.Clamp01(m_PixelDensity / 15.0f), 1f / 2f)
            );
            return new Vector2Int(
                Mathf.RoundToInt(size.x * scale),
                Mathf.RoundToInt(size.y * scale)
            );
        }

        public override void SetupRenderPasses(
            ScriptableRenderer renderer,
            in RenderingData renderingData
        )
        {
            if (
                renderingData.cameraData.cameraType == CameraType.Game
                || renderingData.cameraData.cameraType == CameraType.SceneView
            )
            {
                if (m_ColorHandle == null)
                {
                    m_ColorHandle = RTHandles.Alloc(
                        Scale,
                        filterMode: FilterMode.Point,
                        wrapMode: TextureWrapMode.Clamp,
                        depthBufferBits: DepthBits.None,
                        name: "_RTColor"
                    );
                }
                if (m_DepthHandle == null)
                {
                    m_DepthHandle = RTHandles.Alloc(
                        Scale,
                        filterMode: FilterMode.Point,
                        wrapMode: TextureWrapMode.Clamp,
                        depthBufferBits: DepthBits.Depth24,
                        name: "_RTDepth"
                    );
                }

                m_RenderPass.Setup(m_ColorHandle, m_DepthHandle);
                m_RenderPass.SetTarget(
                    renderer.cameraColorTargetHandle,
                    renderer.cameraDepthTargetHandle
                );

                // Calling ConfigureInput with the ScriptableRenderPassInput.Color argument
                // ensures that the opaque texture is available to the Render Pass.
                m_RenderPass.ConfigureInput(ScriptableRenderPassInput.None);
            }
        }

        public override void Create()
        {
            // if (!(QualitySettings.renderPipeline is UniversalRenderPipelineAsset))
            // {
            //     Debug.LogError("Current render pipeline asset is not URP");
            //     return;
            // }

            // UniversalRenderPipelineAsset urpAsset =
            //     GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

            m_RenderPass = new PixelRenderPass(m_RPEvent);
        }

        protected override void Dispose(bool disposing)
        {
            m_ColorHandle?.Release();
            m_ColorHandle = null;
            m_DepthHandle?.Release();
            m_DepthHandle = null;
        }
    }
}

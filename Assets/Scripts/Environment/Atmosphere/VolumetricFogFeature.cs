using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

// Renderer Feature: añade un pase de pantalla completa que calcula niebla volumétrica por raymarching usando el buffer de profundidad.
public class VolumetricFogFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Shader shader;
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        [Header("Calidad")]
        public float maxDistance = 60f;
        [Range(8, 64)] public int steps = 24;

        [Header("Forma de la niebla")]
        public float baseHeight = 0f;
        public float heightFalloff = 0.15f;
        public float noiseScale = 0.08f;
        [Range(0f, 1f)] public float noiseAmount = 0.6f;
        public float windSpeed = 0.3f;

        [Header("Luz")]
        [Range(0f, 0.9f)] public float anisotropy = 0.45f;
        public float ambientIntensity = 1f;
        public float moonScatter = 0.6f;
        public float lanternScatter = 1f;
    }

    public Settings settings = new Settings();

    private Material material;
    private VolumetricFogPass fogPass;

    public override void Create()
    {
        Shader shader = settings.shader != null ? settings.shader : Shader.Find("Hidden/Llanos/VolumetricFog");
        if (shader == null)
        {
            Debug.LogWarning("VolumetricFogFeature: no se encontró el shader Hidden/Llanos/VolumetricFog. Asígnalo en el campo Shader.");
            return;
        }

        material = CoreUtils.CreateEngineMaterial(shader);
        fogPass = new VolumetricFogPass(material);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (material == null || fogPass == null) return;

        CameraType cameraType = renderingData.cameraData.cameraType;
        if (cameraType != CameraType.Game && cameraType != CameraType.SceneView) return;

        fogPass.renderPassEvent = settings.renderPassEvent;
        fogPass.UpdateMaterial(settings);
        fogPass.ConfigureInput(ScriptableRenderPassInput.Depth);
        renderer.EnqueuePass(fogPass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(material);
    }

    private class VolumetricFogPass : ScriptableRenderPass
    {
        private const string passName = "Llanos Volumetric Fog";
        private const int maxLights = 4;

        private static readonly int maxDistanceId = Shader.PropertyToID("_VolFogMaxDistance");
        private static readonly int stepsId = Shader.PropertyToID("_VolFogSteps");
        private static readonly int baseHeightId = Shader.PropertyToID("_VolFogBaseHeight");
        private static readonly int heightFalloffId = Shader.PropertyToID("_VolFogHeightFalloff");
        private static readonly int noiseScaleId = Shader.PropertyToID("_VolFogNoiseScale");
        private static readonly int noiseAmountId = Shader.PropertyToID("_VolFogNoiseAmount");
        private static readonly int windId = Shader.PropertyToID("_VolFogWind");
        private static readonly int anisotropyId = Shader.PropertyToID("_VolFogAnisotropy");
        private static readonly int ambientId = Shader.PropertyToID("_VolFogAmbient");
        private static readonly int moonScatterId = Shader.PropertyToID("_VolFogMoonScatter");
        private static readonly int lanternScatterId = Shader.PropertyToID("_VolFogLanternScatter");
        private static readonly int lightPosRangeId = Shader.PropertyToID("_VolFogLightPosRange");
        private static readonly int lightColorId = Shader.PropertyToID("_VolFogLightColor");
        private static readonly int lightCountId = Shader.PropertyToID("_VolFogLightCount");

        private readonly Material material;
        private readonly Vector4[] lightPosRange = new Vector4[maxLights];
        private readonly Vector4[] lightColor = new Vector4[maxLights];

        public VolumetricFogPass(Material material)
        {
            this.material = material;
        }

        public void UpdateMaterial(Settings settings)
        {
            material.SetFloat(maxDistanceId, settings.maxDistance);
            material.SetFloat(stepsId, settings.steps);
            material.SetFloat(baseHeightId, settings.baseHeight);
            material.SetFloat(heightFalloffId, settings.heightFalloff);
            material.SetFloat(noiseScaleId, settings.noiseScale);
            material.SetFloat(noiseAmountId, settings.noiseAmount);
            material.SetFloat(windId, settings.windSpeed);
            material.SetFloat(anisotropyId, settings.anisotropy);
            material.SetFloat(ambientId, settings.ambientIntensity);
            material.SetFloat(moonScatterId, settings.moonScatter);
            material.SetFloat(lanternScatterId, settings.lanternScatter);

            int count = Mathf.Min(FogLight.activeLights.Count, maxLights);
            for (int i = 0; i < maxLights; i++)
            {
                if (i < count)
                {
                    lightPosRange[i] = FogLight.activeLights[i].GetPositionRange();
                    lightColor[i] = FogLight.activeLights[i].GetColor();
                }
                else
                {
                    lightPosRange[i] = Vector4.zero;
                    lightColor[i] = Vector4.zero;
                }
            }
            material.SetVectorArray(lightPosRangeId, lightPosRange);
            material.SetVectorArray(lightColorId, lightColor);
            material.SetFloat(lightCountId, count);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            // No se puede leer del backbuffer
            if (resourceData.isActiveTargetBackBuffer) return;

            TextureHandle source = resourceData.activeColorTexture;

            TextureDesc destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = "_VolumetricFogColor";
            destinationDesc.clearBuffer = false;
            TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

            RenderGraphUtils.BlitMaterialParameters parameters = new(source, destination, material, 0);
            renderGraph.AddBlitPass(parameters, passName);

            resourceData.cameraColor = destination;
        }
    }
}
using UnityEngine;
using UnityEngine.Rendering;

public partial class CustomRenderPipeline : RenderPipeline {

	CameraRenderer renderer;

	CameraBufferSettings cameraBufferSettings;

	bool useDynamicBatching, useGPUInstancing, useLightsPerObject;

	ShadowSettings shadowSettings;

	PostFXSettings postFXSettings;

	int colorLUTResolution;

	public CustomRenderPipeline (
		CameraBufferSettings cameraBufferSettings,
		bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher,
		bool useLightsPerObject, ShadowSettings shadowSettings,
		PostFXSettings postFXSettings, int colorLUTResolution, Shader cameraRendererShader
	) {
		this.colorLUTResolution = colorLUTResolution;
		//this.allowHDR = allowHDR;
		this.cameraBufferSettings = cameraBufferSettings;
		this.postFXSettings = postFXSettings;
		this.shadowSettings = shadowSettings;
		this.useDynamicBatching = useDynamicBatching;
		this.useGPUInstancing = useGPUInstancing;
		this.useLightsPerObject = useLightsPerObject;
		GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
		GraphicsSettings.lightsUseLinearIntensity = true;
		InitializeForEditor();
		renderer = new CameraRenderer(cameraRendererShader);
	}

	protected override void Render (ScriptableRenderContext context, Camera[] cameras) {
		foreach (Camera camera in cameras) {
			renderer.Render(
				context, camera, cameraBufferSettings,
				useDynamicBatching, useGPUInstancing, useLightsPerObject,
				shadowSettings, postFXSettings, colorLUTResolution
			);
		}
	}

	protected override void Dispose (bool disposing) {
		base.Dispose(disposing);
		DisposeForEditor();
		renderer.Dispose();
	}
}
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditorForRenderPipeline(typeof(Light), typeof(CustomRenderPipelineAsset))]
public class CustomLightEditor : LightEditor {

	static GUIContent renderingLayerMaskLabel =
		new GUIContent("Rendering Layer Mask", "Functional version of above property.");

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		RenderingLayerMaskDrawer.Draw(
			settings.renderingLayerMask, renderingLayerMaskLabel
		);
		
		if (
			!settings.lightType.hasMultipleDifferentValues &&
			(LightType)settings.lightType.enumValueIndex == LightType.Spot
		)
		{
			settings.DrawInnerAndOuterSpotAngle();
		}

		settings.ApplyModifiedProperties();

		var light = target as Light;
		if (light.cullingMask != -1) {
			EditorGUILayout.HelpBox(
				light.type == LightType.Directional ?
					"Culling Mask only affects shadows." :
					"Culling Mask only affects shadow unless Lights Per Objects is on.",
				MessageType.Warning
			);
		}
	}
}
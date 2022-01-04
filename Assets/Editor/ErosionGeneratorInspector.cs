using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(ErosionGenerator)), CanEditMultipleObjects]
public class ErosionGeneratorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        var generator = this.target as ErosionGenerator;

        EditorGUI.BeginChangeCheck();
        generator._t = EditorGUILayout.Slider("Talus angle", generator._t, 0.01f, .025f);
        if (EditorGUI.EndChangeCheck())
            generator.ExecuteThermalErosion();

        var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        EditorGUILayout.LabelField("Number of iterations", style);

        EditorGUI.BeginChangeCheck();
        generator._iterationsThermal = EditorGUILayout.IntSlider("Thermal", generator._iterationsThermal, 0, 200);
        if (EditorGUI.EndChangeCheck())
            generator.ExecuteThermalErosion();

        EditorGUI.BeginChangeCheck();
        generator._iterationsHydraulic = EditorGUILayout.IntSlider("Hydraulic", generator._iterationsHydraulic, 0, 25);
        if (EditorGUI.EndChangeCheck())
            generator.ExecuteHudraulicErosion();

        if (GUILayout.Button("Regenerate Erosion"))
            generator.ExecuteAllErosion();

        EditorGUILayout.Toggle("Print execution time", generator._showTime);
    }
}

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NoiseGenerator)), CanEditMultipleObjects]
public class NoiseGeneratorInspector : Editor
{
    private string _buttonText = "Switch to Diamond Square noise";
    private bool _isPerlin = true;
    public override void OnInspectorGUI()
    {
        var generator = this.target as NoiseGenerator;

        if (GUILayout.Button(_buttonText))
        {
            if (_isPerlin)
            {
                generator.UsedNoise = NoiseGenerator.NoiseFunction.DIAMONDSQUARE;
                _buttonText = "Switch to Perlin noise";
            }
            else
            {
                generator.UsedNoise = NoiseGenerator.NoiseFunction.PERLIN;
                _buttonText = "Switch to Diamond Square noise";
            }
            _isPerlin = !_isPerlin;
        }

        EditorGUI.BeginChangeCheck();
        if (_isPerlin)
        {
            generator._scale = EditorGUILayout.Slider("Scale", generator._scale, 10, 30);
            generator._offsetX = EditorGUILayout.Slider("X offset", generator._offsetX, 0, 100);
            generator._offsetY = EditorGUILayout.Slider("Y offset", generator._offsetY, 0, 100);
        }
        else
        {
            generator._roughness = EditorGUILayout.Slider("Roughness", generator._roughness, 0, 2);

            var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            EditorGUILayout.LabelField("Corner values", style);
            generator._topRightValue = EditorGUILayout.Slider("Top-right value", generator._topRightValue, 0, 1);
            generator._topLeftValue = EditorGUILayout.Slider("Top-left value", generator._topLeftValue, 0, 1);
            generator._bottomRightValue = EditorGUILayout.Slider("Bottom-right value", generator._bottomRightValue, 0, 1);
            generator._bottomLeftValue = EditorGUILayout.Slider("Bottom-left value", generator._bottomLeftValue, 0, 1);

            if (GUILayout.Button("Regenerate terrain"))
                generator.RegenerateTerrain();
        }
        generator._width = EditorGUILayout.IntSlider("Width", generator._width,0, 512);
        generator._height = EditorGUILayout.IntSlider("Height", generator._height, 0, 512);
        EditorGUILayout.LabelField("Only 256x256 and 512x512 are supported");

        if (EditorGUI.EndChangeCheck())
            generator.RegenerateTerrain();

        generator._showTime = EditorGUILayout.Toggle("Print execution time", generator._showTime);

    }
}
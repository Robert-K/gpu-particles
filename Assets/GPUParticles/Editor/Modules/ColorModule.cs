using UnityEngine;
using UnityEditor;

public class ColorModule : Module
{
    SerializedProperty colorMode;
    SerializedProperty color;
    SerializedProperty colorOverLife;
    SerializedProperty steps;

    Object[] targets;

    public ColorModule(SerializedObject serializedObject, Object[] targets)
    {
        this.targets = targets;

        colorMode = serializedObject.FindProperty("colorMode");
        color = serializedObject.FindProperty("color");
        colorOverLife = serializedObject.FindProperty("colorOverLife");
        steps = serializedObject.FindProperty("steps");
    }

    public override void Draw()
    {
        DrawGUI("Color", DrawContent);
    }

    private void DrawContent()
    {
        EditorGUILayout.PropertyField(colorMode, new GUIContent("Color Mode"));
        switch (colorMode.enumValueIndex)
        {
            case (int)ColorMode.Constant:
                EditorGUILayout.PropertyField(color, new GUIContent("Color"));
                break;
            case (int)ColorMode.OverLife:
                EditorGUILayout.PropertyField(colorOverLife, new GUIContent("Gradient"));
                steps.intValue = Mathf.Max(1, EditorGUILayout.IntField(new GUIContent("Steps"), steps.intValue));
                for (int i = 0; i < targets.Length; i++)
                {
                    GPUParticleEmitter temp = (GPUParticleEmitter)targets[i];
                    temp.UpdateColorOverLifeTexture();
                }
                break;
        }
    }
}

using UnityEngine;
using UnityEditor;

namespace GPUParticles
{
    public class SizeModule : Module
    {
        SerializedProperty sizeMode;
        SerializedProperty size;
        SerializedProperty sizeOverLife;
        SerializedProperty sizeSteps;
        //SerializedProperty sizeVariation;

        Object[] targets;

        public SizeModule(SerializedObject serializedObject, Object[] targets)
        {
            this.targets = targets;

            sizeMode = serializedObject.FindProperty("sizeMode");
            size = serializedObject.FindProperty("size");
            sizeOverLife = serializedObject.FindProperty("sizeOverLife");
            sizeSteps = serializedObject.FindProperty("sizeSteps");
            //sizeVariation = serializedObject.FindProperty("sizeVariation");
        }

        public override void Draw()
        {
            DrawGUI("Size", DrawContent);
        }

        private void DrawContent()
        {
            EditorGUILayout.PropertyField(sizeMode, new GUIContent("Size Mode"));
            switch (sizeMode.enumValueIndex)
            {
                case (int)SizeMode.Constant:
                    size.floatValue = Mathf.Max(0f, EditorGUILayout.FloatField(new GUIContent("Size"), size.floatValue));
                    break;
                case (int)SizeMode.OverLife:
                    EditorGUILayout.CurveField(sizeOverLife, Color.green, new Rect(0, 0, 1, 1), new GUIContent("Size Over Life"));
                    sizeSteps.intValue = Mathf.Max(2, EditorGUILayout.IntField(new GUIContent("Steps"), sizeSteps.intValue));
                    for (int i = 0; i < targets.Length; i++)
                    {
                        GPUParticleEmitter temp = (GPUParticleEmitter)targets[i];
                        temp.UpdateSizeOverLifeBuffer();
                    }
                    break;
            }
            //sizeVariation.floatValue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Variation (%)"), sizeVariation.floatValue), 0f, 100f);
        }
    }
}
using UnityEngine;
using UnityEditor;

namespace GPUParticles
{
    public class LifetimeModule : Module
    {
        SerializedProperty minLifetime;
        SerializedProperty maxLifetime;

        public LifetimeModule(SerializedObject serializedObject)
        {
            minLifetime = serializedObject.FindProperty("minLifetime");
            maxLifetime = serializedObject.FindProperty("maxLifetime");
        }

        public override void Draw()
        {
            DrawGUI("Lifetime", DrawContent);
        }

        private void DrawContent()
        {
            EditorGUILayout.PropertyField(minLifetime, new GUIContent("Min Lifetime"));
            EditorGUILayout.PropertyField(maxLifetime, new GUIContent("Max Lifetime"));
        }
    }
}
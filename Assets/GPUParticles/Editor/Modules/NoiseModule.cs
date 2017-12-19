using UnityEngine;
using UnityEditor;

namespace GPUParticles
{
    public class NoiseModule : Module
    {
        SerializedProperty enableNoise;

        SerializedProperty convergence;
        SerializedProperty convergenceFrequency;
        SerializedProperty convergenceAmplitude;
        SerializedProperty viscosity;

        public NoiseModule(SerializedObject serializedObject)
        {
            enableNoise = serializedObject.FindProperty("enableNoise");

            convergence = serializedObject.FindProperty("convergence");
            convergenceFrequency = serializedObject.FindProperty("convergenceFrequency");
            convergenceAmplitude = serializedObject.FindProperty("convergenceAmplitude");
            viscosity = serializedObject.FindProperty("viscosity");
        }

        public override void Draw()
        {
            enableNoise.boolValue = DrawGUI(enableNoise.boolValue, "Noise", DrawContent);
        }

        private void DrawContent()
        {
            EditorGUILayout.PropertyField(convergence, new GUIContent("Convergence"));
            EditorGUILayout.PropertyField(convergenceFrequency, new GUIContent("Convergence Frequency"));
            EditorGUILayout.PropertyField(convergenceAmplitude, new GUIContent("Convergence Amplitude"));
            EditorGUILayout.PropertyField(viscosity, new GUIContent("Viscosity"));
        }
    }
}
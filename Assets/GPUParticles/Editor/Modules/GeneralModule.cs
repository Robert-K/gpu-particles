using UnityEngine;
using UnityEditor;

namespace GPUParticles
{
    public class GeneralModule : Module
    {
        SerializedProperty maxParticles;
        SerializedProperty simulationSpace;
        SerializedProperty simulationParent;
        SerializedProperty timeScale;

        Object[] targets;

        public GeneralModule(SerializedObject serializedObject, Object[] targets)
        {
            this.targets = targets;

            maxParticles = serializedObject.FindProperty("maxParticles");
            simulationSpace = serializedObject.FindProperty("simulationSpace");
            simulationParent = serializedObject.FindProperty("simulationParent");
            timeScale = serializedObject.FindProperty("timeScale");
        }

        public override void Draw()
        {
            DrawGUI("General", DrawContent);
        }

        private void DrawContent()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            maxParticles.intValue = Mathf.Max(1, EditorGUILayout.IntField("Max Particles", maxParticles.intValue));
            EditorGUILayout.LabelField("(" + Mathf.CeilToInt((float)maxParticles.intValue / GPUParticleEmitter.THREAD_COUNT) * GPUParticleEmitter.THREAD_COUNT + " in Buffer)", Styles.textRight, GUILayout.Width(EditorGUIUtility.labelWidth / 2f));
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    GPUParticleEmitter temp = (GPUParticleEmitter)targets[i];
                    temp.DispatchInit();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(simulationSpace, new GUIContent("Simulation Space"));
            if (simulationSpace.enumValueIndex == (int)SimulationSpace.Custom)
            {
                EditorGUILayout.PropertyField(simulationParent, new GUIContent("Simulation Parent"));
            }

            timeScale.floatValue = Mathf.Max(0f, EditorGUILayout.FloatField("Time Scale", timeScale.floatValue));
        }
    }
}
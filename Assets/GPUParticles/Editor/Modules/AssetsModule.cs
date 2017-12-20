using UnityEngine;
using UnityEditor;

namespace GPUParticles
{
    public class AssetsModule : Module
    {
        private bool darkSkin;

        SerializedProperty computeShader;
        SerializedProperty renderMaterial;

        public AssetsModule(SerializedObject serializedObject)
        {
            darkSkin = EditorPrefs.GetBool("DarkSkin");

            computeShader = serializedObject.FindProperty("computeShader");
            renderMaterial = serializedObject.FindProperty("renderMaterial");
        }

        public override void Draw()
        {
            DrawGUI("Assets", DrawContent);
        }

        private void DrawContent()
        {
            EditorGUILayout.PropertyField(computeShader, new GUIContent("Compute Shader"));
            EditorGUILayout.PropertyField(renderMaterial, new GUIContent("Render Material"));
            EditorGUI.BeginChangeCheck();
            darkSkin = EditorGUILayout.Toggle(new GUIContent("Dark Skin"), darkSkin);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool("DarkSkin", darkSkin);
                if (darkSkin) headerColor = new Color(0.15f, 0.15f, 0.15f);
                else headerColor = new Color(0.63f, 0.63f, 0.63f);
            }
        }
    }
}
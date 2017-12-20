using UnityEngine;
using UnityEditor;
using System;

namespace GPUParticles
{
    public abstract class Module
    {
        public static Color headerColor = new Color(0.15f, 0.15f, 0.15f);

        public abstract void Draw();

        protected void DrawGUI(string name, Action drawContent)
        {
            EditorGUI.DrawRect(EditorGUILayout.BeginHorizontal(Styles.moduleHeader), headerColor);
            Rect foldoutRect = GUILayoutUtility.GetRect(40f, 16f);
            EditorPrefs.SetBool(name + " Foldout", EditorGUI.Foldout(foldoutRect, EditorPrefs.GetBool(name + " Foldout"), name, true));
            EditorGUILayout.EndHorizontal();
            if (EditorPrefs.GetBool(name + " Foldout"))
            {
                EditorGUILayout.BeginVertical("Box");
                drawContent();
                EditorGUILayout.EndVertical();
            }
        }

        protected bool DrawGUI(bool enabled, string name, Action drawContent)
        {
            Rect toggleRect = EditorGUILayout.BeginHorizontal(Styles.moduleHeader);
            EditorGUI.DrawRect(toggleRect, headerColor);
            toggleRect.position += new Vector2(3f, 1f);
            toggleRect.size = new Vector2(16f, 16f);
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            enabled = GUI.Toggle(toggleRect, enabled, string.Empty);
            GUI.color = Color.white;
            Rect foldoutRect = GUILayoutUtility.GetRect(40f, 16f);
            EditorPrefs.SetBool(name + " Foldout", EditorGUI.Foldout(foldoutRect, EditorPrefs.GetBool(name + " Foldout"), name, true));
            EditorGUILayout.EndHorizontal();
            if (EditorPrefs.GetBool(name + " Foldout"))
            {
                EditorGUI.BeginDisabledGroup(!enabled);
                EditorGUILayout.BeginVertical("Box");
                drawContent();
                EditorGUILayout.EndVertical();
                EditorGUI.EndDisabledGroup();
            }

            return enabled;
        }
    }
}
using UnityEngine;
using UnityEditor;

namespace GPUParticles
{
    public class Style
    {
        public static GUIStyle groupHeader, subtitle, link, controls;

        public static Texture2D logo;

        static Style()
        {
            groupHeader = new GUIStyle(GUI.skin.GetStyle("Toolbar"))
            {
                padding = new RectOffset(32, 10, 0, 0),
                margin = new RectOffset(0, 0, 3, 3),
            };

            subtitle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
            };

            link = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(10, 10, 0, 0),
            };
            link.normal.textColor = new Color(0.07f, 0.61f, 1.0f);

            controls = new GUIStyle(GUI.skin.GetStyle("Command"))
            {
                alignment = TextAnchor.MiddleCenter,
            };

            logo = (Texture2D)EditorGUIUtility.Load("Logo.png");
        }
    }
}

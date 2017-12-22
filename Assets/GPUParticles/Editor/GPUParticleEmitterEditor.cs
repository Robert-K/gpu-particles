using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace GPUParticles
{
    [CustomEditor(typeof(GPUParticleEmitter)), CanEditMultipleObjects]
    public class GPUParticleEmitterEditor : Editor
    {
        private static GPUParticleEmitterEditor instance;

        private List<Module> modules;

        private Texture2D logo;

        public static GPUParticleEmitterEditor GetInstance()
        {
            return instance;
        }

        private void OnEnable()
        {
            instance = this;

            if (logo == null)
            {
                logo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/GPUParticles/Resources/Logo.png");
            }

            modules = new List<Module>
        {
            new SystemModule(targets),
            new GeneralModule(serializedObject, targets),
            new ColorModule(serializedObject, targets),
            new SizeModule(serializedObject, targets),
            new LifetimeModule(serializedObject),
            new EmissionModule(serializedObject),
            new InheritVelocityModule(serializedObject),
            new NoiseModule(serializedObject),
            new ConstantInfluenceModule(serializedObject),
            new AssetsModule(serializedObject),
        };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField(new GUIContent(logo), Styles.title, GUILayout.Height(EditorGUIUtility.singleLineHeight * 4f));
            EditorGUILayout.LabelField("Version " + GPUParticleEmitter.VERSION + " (C) Robert Kossessa 2017", Styles.title);

            GUI.color = Color.white;

            foreach (Module cur in modules)
            {
                cur.Draw();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
using UnityEngine;
using UnityEditor;

namespace GPUParticles
{
    public class SystemModule : Module
    {
        Object[] targets;

        public SystemModule(Object[] targets)
        {
            this.targets = targets;
        }

        public override void Draw()
        {
            DrawGUI("System", DrawContent);
        }

        private void DrawContent()
        {
            foreach (GPUParticleEmitter cur in targets)
            {
                EditorGUILayout.BeginHorizontal();
                if (Application.isPlaying)
                {
                    EditorGUILayout.LabelField(cur.name + ": " + cur.GetAliveCount() + " / " + cur.maxParticles + " Particles alive");
                    GPUParticleEmitterEditor.GetInstance().Repaint();
                }
                else
                {
                    EditorGUILayout.LabelField(cur.name + ": 0 / " + cur.maxParticles + " Particles alive");
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}

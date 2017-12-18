using UnityEngine;
using UnityEditor;

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
        bool update = EditorPrefs.GetBool("UpdateParticleCount");
        EditorPrefs.SetBool("UpdateParticleCount", EditorGUILayout.ToggleLeft(new GUIContent("Constantly Update Particle Count", "Disable this or close this module if you are experiencing performance issues."), update));

        EditorGUILayout.Space();

        foreach (GPUParticleEmitter cur in targets)
        {
            EditorGUILayout.BeginHorizontal();
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField(cur.name + ": " + cur.GetAliveCount() + " / " + cur.maxParticles + " Particles alive");
            }
            else
            {
                EditorGUILayout.LabelField(cur.name + ": 0 / " + cur.maxParticles + " Particles alive");
            }
            EditorGUILayout.EndHorizontal();
            if (update) GPUParticleEmitterEditor.GetInstance().Repaint();
        }
    }
}

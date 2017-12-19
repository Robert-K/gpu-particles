using UnityEngine;
using UnityEditor;

namespace GPUParticles
{
    public class InheritVelocityModule : Module
    {
        SerializedProperty enableInheritVelocity;

        SerializedProperty inheritVelocity;
        SerializedProperty extrapolation;

        public InheritVelocityModule(SerializedObject serializedObject)
        {
            enableInheritVelocity = serializedObject.FindProperty("enableInheritVelocity");

            inheritVelocity = serializedObject.FindProperty("inheritVelocity");
            extrapolation = serializedObject.FindProperty("extrapolation");
        }

        public override void Draw()
        {
            enableInheritVelocity.boolValue = DrawGUI(enableInheritVelocity.boolValue, "Inherit Velocity", DrawContent);
        }

        private void DrawContent()
        {
            EditorGUILayout.PropertyField(inheritVelocity, new GUIContent("Velocity Multiplier", "Multiplies the emitter's transform velocity and then applies it to the particles on spawn"));

            EditorGUILayout.Slider(extrapolation, 0f, 1f, new GUIContent("Emission Extrapolation", "Extrapolates emission position based on velocity. Stretches particle puffs at high velocities to make them less noticable."));
        }
    }
}
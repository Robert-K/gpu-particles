using UnityEngine;
using UnityEditor;

namespace GPUParticles
{
    public class ConstantInfluenceModule : Module
    {
        SerializedProperty enableConstantInfluence;

        SerializedProperty constantVelocity;
        SerializedProperty constantForce;
        SerializedProperty linearDrag;

        public ConstantInfluenceModule(SerializedObject serializedObject)
        {
            enableConstantInfluence = serializedObject.FindProperty("enableConstantInfluence");

            constantVelocity = serializedObject.FindProperty("constantVelocity");
            constantForce = serializedObject.FindProperty("constantForce");
            linearDrag = serializedObject.FindProperty("linearDrag");
        }

        public override void Draw()
        {
            enableConstantInfluence.boolValue = DrawGUI(enableConstantInfluence.boolValue, "Constant Influence", DrawContent);
        }

        private void DrawContent()
        {
            EditorGUILayout.PropertyField(constantVelocity, new GUIContent("Constant Velocity", "Moves the particles at a constant velocity."));
            EditorGUILayout.PropertyField(constantForce, new GUIContent("Constant Force", "Constantly accelerates the particles."));
            EditorGUILayout.PropertyField(linearDrag, new GUIContent("Linear Drag", "Similar to air resistance, but linear."));
        }
    }
}
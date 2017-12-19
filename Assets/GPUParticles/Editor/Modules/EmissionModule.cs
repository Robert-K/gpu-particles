using UnityEngine;
using UnityEditor;

namespace GPUParticles
{
    public class EmissionModule : Module
    {
        SerializedProperty enableEmission;

        SerializedProperty emissionRate;

        SerializedProperty emissionShape;
        SerializedProperty sphereRadius;
        SerializedProperty boxSize;
        SerializedProperty coneRadius;
        SerializedProperty coneAngle;
        SerializedProperty coneLength;
        SerializedProperty meshType;
        SerializedProperty mesh;
        SerializedProperty meshRenderer;
        SerializedProperty skinnedMeshRenderer;
        SerializedProperty circleRadius;
        SerializedProperty edgeStart;
        SerializedProperty edgeEnd;
        SerializedProperty edgeRadius;

        SerializedProperty directionType;
        SerializedProperty direction;

        SerializedProperty minInitialSpeed;
        SerializedProperty maxInitialSpeed;

        public EmissionModule(SerializedObject serializedObject)
        {
            enableEmission = serializedObject.FindProperty("enableEmission");

            emissionRate = serializedObject.FindProperty("emissionRate");
            emissionShape = serializedObject.FindProperty("emissionShape");
            sphereRadius = serializedObject.FindProperty("sphereRadius");
            boxSize = serializedObject.FindProperty("boxSize");
            coneRadius = serializedObject.FindProperty("coneRadius");
            coneAngle = serializedObject.FindProperty("coneAngle");
            coneLength = serializedObject.FindProperty("coneLength");
            meshType = serializedObject.FindProperty("meshType");
            mesh = serializedObject.FindProperty("mesh");
            meshRenderer = serializedObject.FindProperty("meshRenderer");
            skinnedMeshRenderer = serializedObject.FindProperty("skinnedMeshRenderer");
            circleRadius = serializedObject.FindProperty("circleRadius");
            edgeStart = serializedObject.FindProperty("edgeStart");
            edgeEnd = serializedObject.FindProperty("edgeEnd");
            edgeRadius = serializedObject.FindProperty("edgeRadius");

            directionType = serializedObject.FindProperty("directionType");
            direction = serializedObject.FindProperty("direction");

            minInitialSpeed = serializedObject.FindProperty("minInitialSpeed");
            maxInitialSpeed = serializedObject.FindProperty("maxInitialSpeed");
        }

        public override void Draw()
        {
            enableEmission.boolValue = DrawGUI(enableEmission.boolValue, "Emission", DrawContent);
        }

        private void DrawContent()
        {
            EditorGUILayout.PropertyField(emissionRate, new GUIContent("Particles/sec"));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(emissionShape, new GUIContent("Emission Shape"));
            switch (emissionShape.enumValueIndex)
            {
                case (int)EmissionShape.Sphere:
                    EditorGUILayout.PropertyField(sphereRadius, new GUIContent("Radius"));
                    break;
                case (int)EmissionShape.Box:
                    EditorGUILayout.PropertyField(boxSize, new GUIContent("Size"));
                    break;
                case (int)EmissionShape.Cone:
                    EditorGUILayout.PropertyField(coneRadius, new GUIContent("Radius"));
                    EditorGUILayout.PropertyField(coneAngle, new GUIContent("Angle"));
                    EditorGUILayout.PropertyField(coneLength, new GUIContent("Length"));
                    break;
                case (int)EmissionShape.Mesh:
                    EditorGUILayout.PropertyField(meshType, new GUIContent("Type"));
                    EditorGUILayout.PropertyField(mesh, new GUIContent("Mesh"));
                    break;
                case (int)EmissionShape.MeshRenderer:
                    EditorGUILayout.PropertyField(meshType, new GUIContent("Type"));
                    EditorGUILayout.PropertyField(meshRenderer, new GUIContent("Mesh Renderer"));
                    break;
                case (int)EmissionShape.SkinnedMeshRenderer:
                    EditorGUILayout.PropertyField(meshType, new GUIContent("Type"));
                    EditorGUILayout.PropertyField(skinnedMeshRenderer, new GUIContent("Skinned Mesh Renderer"));
                    break;
                case (int)EmissionShape.Circle:
                    EditorGUILayout.PropertyField(circleRadius, new GUIContent("Radius"));
                    break;
                case (int)EmissionShape.Edge:
                    EditorGUILayout.PropertyField(edgeStart, new GUIContent("Start"));
                    EditorGUILayout.PropertyField(edgeEnd, new GUIContent("End"));
                    EditorGUILayout.PropertyField(edgeRadius, new GUIContent("Radius"));
                    break;
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(directionType, new GUIContent("Direction Type"));
            switch (directionType.enumValueIndex)
            {
                case (int)DirectionType.Uniform:
                    EditorGUILayout.PropertyField(direction, new GUIContent("Direction"));
                    break;
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(minInitialSpeed, new GUIContent("Min Initial Speed"));
            EditorGUILayout.PropertyField(maxInitialSpeed, new GUIContent("Max Initial Speed"));
        }
    }
}
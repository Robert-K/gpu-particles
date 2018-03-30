using UnityEngine;
using UnityEditor;
using System;

namespace GPUParticles
{
    [CustomEditor(typeof(GPUParticles)), CanEditMultipleObjects]
    public class GPUParticlesEditor : Editor
    {
        #region Serialized Properties

        #region General Properties
        SerializedProperty maxParticles;
        SerializedProperty simulationSpace;
        SerializedProperty timeScale;
        SerializedProperty randomSeed;
        #endregion

        #region Emission Properties
        SerializedProperty enableEmission;

        SerializedProperty emissionMode;
        SerializedProperty emissionFrequency;
        SerializedProperty emissionCount;
        SerializedProperty emissionThrottle;
        SerializedProperty lifespan;
        SerializedProperty lifespanRandomness;
        SerializedProperty emissionShape;
        SerializedProperty sphereRadius;
        SerializedProperty boxSize;
        SerializedProperty edgeStart;
        SerializedProperty edgeEnd;
        #endregion

        #region Velocity Properties
        SerializedProperty inheritVelocity;
        SerializedProperty initialVelocity;
        SerializedProperty speedRandomness;
        SerializedProperty directionSpread;
        SerializedProperty acceleration;
        SerializedProperty drag;
        SerializedProperty effectors;
        #endregion

        #region Noise Properties
        SerializedProperty enableNoise;

        SerializedProperty noiseAmplitude;
        SerializedProperty noiseFrequency;
        SerializedProperty noiseEvolution;
        #endregion

        #region Color Properties
        SerializedProperty colorMode;
        SerializedProperty color;
        SerializedProperty colorEvolution;
        SerializedProperty colorSteps;
        SerializedProperty colorRandomness;
        #endregion

        #region Size Properties
        SerializedProperty sizeMode;
        SerializedProperty size;
        SerializedProperty sizeEvolution;
        SerializedProperty sizeSteps;
        SerializedProperty sizeRandomness;
        #endregion

        #region Rendering Properties
        SerializedProperty enableRendering;

        SerializedProperty particleMesh;
        SerializedProperty particleMaterial;
        SerializedProperty castShadows;
        SerializedProperty receiveShadows;
        #endregion

        #endregion

        #region Private Variables

        private static bool drawDefaultInspector = false;

        private static bool drawEmissionHandles = true;

        #endregion

        #region Private Methods

        private void Header()
        {
            GUI.DrawTexture(GUILayoutUtility.GetRect(0f, 60f), Style.logo, ScaleMode.ScaleToFit);
            GUILayout.Label("\u00A9 Robert Kossessa 2017", Style.subtitle);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("GitHub", Style.link))
            {
                Application.OpenURL("https://github.com/Robert-K/gpu-particles");
            }
            if (GUILayout.Button("Kosro.de", Style.link))
            {
                Application.OpenURL("https://kosro.de");
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void ControlGroup(string name, Action contentCallback)
        {
            EditorGUILayout.BeginHorizontal(Style.groupHeader);
            Rect foldoutRect = GUILayoutUtility.GetRect(40f, 16f);
            EditorPrefs.SetBool(name + " Foldout", EditorGUI.Foldout(foldoutRect, EditorPrefs.GetBool(name + " Foldout"), name, true));
            EditorGUILayout.EndHorizontal();
            if (EditorPrefs.GetBool(name + " Foldout"))
            {
                EditorGUILayout.BeginVertical("Box");
                contentCallback();
                EditorGUILayout.EndVertical();
            }
        }
        private bool ControlGroup(bool enabled, string name, Action contentCallback)
        {
            Rect toggleRect = EditorGUILayout.BeginHorizontal(Style.groupHeader);
            toggleRect.position += new Vector2(3f, 0f);
            toggleRect.size = new Vector2(16f, 16f);
            Color prevColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            enabled = GUI.Toggle(toggleRect, enabled, string.Empty);
            GUI.color = prevColor;
            Rect foldoutRect = GUILayoutUtility.GetRect(40f, 16f);
            EditorPrefs.SetBool(name + " Foldout", EditorGUI.Foldout(foldoutRect, EditorPrefs.GetBool(name + " Foldout"), name, true));
            EditorGUILayout.EndHorizontal();
            if (EditorPrefs.GetBool(name + " Foldout"))
            {
                EditorGUI.BeginDisabledGroup(!enabled);
                EditorGUILayout.BeginVertical("Box");
                contentCallback();
                EditorGUILayout.EndVertical();
                EditorGUI.EndDisabledGroup();
            }

            return enabled;
        }

        private void DrawSystem()
        {
            EditorGUILayout.LabelField(new GUIContent("GameObject"), new GUIContent("Particle Count"));
            for (int i = targets.Length - 1; i >= 0; i--)
            {
                GPUParticles temp = (GPUParticles)targets[i];
                EditorGUILayout.LabelField(new GUIContent(temp.gameObject.name), new GUIContent(temp.AliveCount + " / " + temp.maxParticles));
            }
        }
        private void DrawGeneralProperties()
        {
            EditorGUI.BeginChangeCheck();
            maxParticles.intValue = Mathf.Max(1, EditorGUILayout.DelayedIntField(new GUIContent("Max Particles"), maxParticles.intValue));
            bool changed = EditorGUI.EndChangeCheck();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(simulationSpace, new GUIContent("Simulation Space"));

            EditorGUILayout.Space();

            timeScale.floatValue = Mathf.Max(0f, EditorGUILayout.FloatField(new GUIContent("Time Scale"), timeScale.floatValue));

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(randomSeed, new GUIContent("Random Seed"));
            if (changed || EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    ((GPUParticles)targets[i]).DispatchInit();
                }
            }
        }
        private void DrawEmissionProperties()
        {
            EditorGUILayout.PropertyField(emissionMode, new GUIContent("Emission Mode"));
            if (emissionMode.enumValueIndex == (int)EmissionMode.Frequency)
            {
                EditorGUI.BeginChangeCheck();
                emissionFrequency.floatValue = Mathf.Max(0f, EditorGUILayout.FloatField(new GUIContent("Frequency (Hz)"), emissionFrequency.floatValue));
                if (EditorGUI.EndChangeCheck())
                {
                    for (int i = 0; i < targets.Length; i++)
                    {
                        ((GPUParticles)targets[i]).ResetEmissionInterval();
                    }
                }

                emissionCount.intValue = Mathf.Max(0, EditorGUILayout.IntField(new GUIContent("Count"), emissionCount.intValue));
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(emissionThrottle, new GUIContent("Throttle"));
                if (EditorGUI.EndChangeCheck())
                {
                    for (int i = 0; i < targets.Length; i++)
                    {
                        ((GPUParticles)targets[i]).ResetEmissionInterval();
                    }
                }
            }

            EditorGUILayout.Space();

            lifespan.floatValue = Mathf.Max(0.01f, EditorGUILayout.FloatField(new GUIContent("Lifespan"), lifespan.floatValue));
            EditorGUILayout.PropertyField(lifespanRandomness, new GUIContent("Randomness"));

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
                case (int)EmissionShape.Edge:
                    EditorGUILayout.PropertyField(edgeStart, new GUIContent("Start"));
                    EditorGUILayout.PropertyField(edgeEnd, new GUIContent("End"));
                    break;
            }

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            drawEmissionHandles = EditorGUILayout.Toggle(new GUIContent("Draw Handles"), drawEmissionHandles);
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
        }
        private void DrawVelocityProperties()
        {
            EditorGUILayout.PropertyField(inheritVelocity, new GUIContent("Inherit Velocity"));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(initialVelocity, new GUIContent("Initial Velocity"));
            EditorGUILayout.PropertyField(speedRandomness, new GUIContent("Randomness"));
            EditorGUILayout.PropertyField(directionSpread, new GUIContent("Direction Spread"));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(acceleration, new GUIContent("Acceleration"));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(drag, new GUIContent("Drag"));

            EditorGUILayout.Space();

            EditorGUI.indentLevel++;
            if (EditorGUILayout.PropertyField(effectors, new GUIContent("Effectors")))
            {
                GPUParticles temp = (GPUParticles)target;
                EditorGUI.BeginChangeCheck();
                effectors.arraySize = EditorGUILayout.DelayedIntField(new GUIContent("Size"), effectors.arraySize);
                GPUParticles.Effector[] array = temp.effectors;
                for (int i = 0; i < array.Length; i++)
                {
                    EditorGUILayout.LabelField(new GUIContent("Effector " + i));
                    EditorGUI.indentLevel++;
                    array[i].position = EditorGUILayout.Vector3Field(new GUIContent("Position"), array[i].position);
                    array[i].force = EditorGUILayout.FloatField(new GUIContent("Force"), array[i].force);
                    EditorGUI.indentLevel--;
                }
                if (EditorGUI.EndChangeCheck())
                {
                    temp.UpdateEffectorsBuffer();
                }
            }
            EditorGUI.indentLevel--;
        }
        private void DrawNoiseProperties()
        {
            EditorGUILayout.PropertyField(noiseAmplitude, new GUIContent("Amplitude"));
            EditorGUILayout.PropertyField(noiseFrequency, new GUIContent("Frequency"));
            EditorGUILayout.PropertyField(noiseEvolution, new GUIContent("Evolution"));
        }
        private void DrawColorProperties()
        {
            EditorGUILayout.PropertyField(colorMode, new GUIContent("Mode"));
            switch (colorMode.enumValueIndex)
            {
                case (int)ColorMode.Constant:
                    EditorGUILayout.PropertyField(color, new GUIContent("Color"));
                    EditorGUILayout.PropertyField(colorRandomness, new GUIContent("Randomness"));
                    break;
                case (int)ColorMode.OverLife:
                    EditorGUILayout.PropertyField(colorEvolution, new GUIContent("Evolution"));
                    EditorGUI.BeginChangeCheck();
                    colorSteps.intValue = Mathf.Max(2, EditorGUILayout.IntField(new GUIContent("Steps"), colorSteps.intValue));
                    if (EditorGUI.EndChangeCheck() || GUILayout.Button("Update"))
                    {
                        for (int i = 0; i < targets.Length; i++)
                        {
                            ((GPUParticles)targets[i]).UpdateColorEvolutionTexture();
                        }
                    }
                    break;
            }
        }
        private void DrawSizeProperties()
        {
            EditorGUILayout.PropertyField(sizeMode, new GUIContent("Mode"));
            switch (sizeMode.enumValueIndex)
            {
                case (int)SizeMode.Constant:
                    EditorGUILayout.PropertyField(size, new GUIContent("Size"));
                    EditorGUILayout.PropertyField(sizeRandomness, new GUIContent("Randomness"));
                    break;
                case (int)SizeMode.OverLife:
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(size, new GUIContent("Size"));
                    EditorGUILayout.CurveField(sizeEvolution, Color.green, new Rect(0, 0, 1, 1), new GUIContent("Evolution"));
                    sizeSteps.intValue = Mathf.Max(2, EditorGUILayout.DelayedIntField(new GUIContent("Steps"), sizeSteps.intValue));
                    if (EditorGUI.EndChangeCheck())
                    {
                        for (int i = 0; i < targets.Length; i++)
                        {
                            ((GPUParticles)targets[i]).UpdateSizeEvolutionBuffer();
                        }
                    }
                    break;
            }
        }
        private void DrawRenderingProperties()
        {
            EditorGUILayout.PropertyField(particleMesh, new GUIContent("Particle Mesh"));

            if (GUILayout.Button(new GUIContent("Update")))
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    ((GPUParticles)targets[i]).UpdateMeshBuffer();
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(particleMaterial, new GUIContent("Material"));

            EditorGUILayout.PropertyField(castShadows, new GUIContent("Cast Shadows"));

            EditorGUILayout.PropertyField(receiveShadows, new GUIContent("Receive Shadows"));
        }
        private void DrawDebug()
        {
            drawDefaultInspector = EditorGUILayout.Toggle(new GUIContent("Default Inspector"), drawDefaultInspector);
        }

        #endregion

        #region MonoBehaviour Methods

        private void OnEnable()
        {
            EditorApplication.update += Repaint;

            maxParticles = serializedObject.FindProperty("maxParticles");
            simulationSpace = serializedObject.FindProperty("simulationSpace");
            timeScale = serializedObject.FindProperty("timeScale");
            randomSeed = serializedObject.FindProperty("randomSeed");

            enableEmission = serializedObject.FindProperty("enableEmission");

            emissionMode = serializedObject.FindProperty("emissionMode");
            emissionFrequency = serializedObject.FindProperty("emissionFrequency");
            emissionCount = serializedObject.FindProperty("emissionCount");
            emissionThrottle = serializedObject.FindProperty("emissionThrottle");
            lifespan = serializedObject.FindProperty("lifespan");
            lifespanRandomness = serializedObject.FindProperty("lifespanRandomness");
            emissionShape = serializedObject.FindProperty("emissionShape");
            sphereRadius = serializedObject.FindProperty("sphereRadius");
            boxSize = serializedObject.FindProperty("boxSize");
            edgeStart = serializedObject.FindProperty("edgeStart");
            edgeEnd = serializedObject.FindProperty("edgeEnd");

            inheritVelocity = serializedObject.FindProperty("inheritVelocity");
            initialVelocity = serializedObject.FindProperty("initialVelocity");
            speedRandomness = serializedObject.FindProperty("speedRandomness");
            directionSpread = serializedObject.FindProperty("directionSpread");
            acceleration = serializedObject.FindProperty("acceleration");
            drag = serializedObject.FindProperty("drag");
            effectors = serializedObject.FindProperty("effectors");

            enableNoise = serializedObject.FindProperty("enableNoise");

            noiseAmplitude = serializedObject.FindProperty("noiseAmplitude");
            noiseFrequency = serializedObject.FindProperty("noiseFrequency");
            noiseEvolution = serializedObject.FindProperty("noiseEvolution");

            colorMode = serializedObject.FindProperty("colorMode");
            color = serializedObject.FindProperty("color");
            colorEvolution = serializedObject.FindProperty("colorEvolution");
            colorSteps = serializedObject.FindProperty("colorSteps");
            colorRandomness = serializedObject.FindProperty("colorRandomness");

            sizeMode = serializedObject.FindProperty("sizeMode");
            size = serializedObject.FindProperty("size");
            sizeEvolution = serializedObject.FindProperty("sizeEvolution");
            sizeSteps = serializedObject.FindProperty("sizeSteps");
            sizeRandomness = serializedObject.FindProperty("sizeRandomness");

            enableRendering = serializedObject.FindProperty("enableRendering");

            particleMesh = serializedObject.FindProperty("particleMesh");
            particleMaterial = serializedObject.FindProperty("particleMaterial");
            castShadows = serializedObject.FindProperty("castShadows");
            receiveShadows = serializedObject.FindProperty("receiveShadows");
        }

        public override void OnInspectorGUI()
        {
            if (drawDefaultInspector)
            {
                DrawDefaultInspector();
                EditorGUILayout.Space();
                drawDefaultInspector = EditorGUILayout.Toggle("Default Inspector", drawDefaultInspector);
                return;
            }

            serializedObject.Update();

            Header();

            ControlGroup("System", DrawSystem);
            ControlGroup("General", DrawGeneralProperties);
            enableEmission.boolValue = ControlGroup(enableEmission.boolValue, "Emission", DrawEmissionProperties);
            ControlGroup("Velocity", DrawVelocityProperties);
            enableNoise.boolValue = ControlGroup(enableNoise.boolValue, "Noise", DrawNoiseProperties);
            ControlGroup("Color", DrawColorProperties);
            ControlGroup("Size", DrawSizeProperties);
            enableRendering.boolValue = ControlGroup(enableRendering.boolValue, "Rendering", DrawRenderingProperties);
            ControlGroup("Debug", DrawDebug);

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (drawEmissionHandles)
            {


                GPUParticles temp = (GPUParticles)target;

                switch (emissionShape.enumValueIndex)
                {
                    case (int)EmissionShape.Sphere:
                        Handles.color = Color.yellow;
                        temp.sphereRadius = Handles.RadiusHandle(temp.transform.rotation, temp.transform.position, temp.sphereRadius);
                        break;
                    case (int)EmissionShape.Box:
                        temp.boxSize = Handles.ScaleHandle(temp.boxSize, temp.transform.position, temp.transform.rotation, HandleUtility.GetHandleSize(temp.transform.position) * 1.3f);
                        Handles.matrix *= temp.transform.localToWorldMatrix;
                        Handles.color = Color.yellow;
                        Handles.DrawWireCube(Vector3.zero, temp.boxSize);
                        break;
                    case (int)EmissionShape.Edge:
                        Vector3 start = temp.transform.TransformPoint(temp.edgeStart);
                        Vector3 end = temp.transform.TransformPoint(temp.edgeEnd);
                        temp.edgeStart = temp.transform.InverseTransformPoint(Handles.PositionHandle(start, temp.transform.rotation));
                        temp.edgeEnd = temp.transform.InverseTransformPoint(Handles.PositionHandle(end, temp.transform.rotation));
                        Handles.color = Color.yellow;
                        Handles.DrawLine(start, end);
                        break;
                }
            }
        }

        private void OnDisable()
        {
            EditorApplication.update -= Repaint;
        }

        #endregion
    }
}
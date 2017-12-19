using UnityEngine;
using System.Runtime.InteropServices;

namespace GPUParticles
{
    public class GPUParticleEmitter : MonoBehaviour
    {
        public const string VERSION = "0.5.7";

        #region Module Variables

        #region General
        public const int THREAD_COUNT = 256;
        public int maxParticles = 100000;
        public SimulationSpace simulationSpace = SimulationSpace.Local;
        public Transform simulationParent;
        public float timeScale = 1f;
        #endregion

        #region Color
        public ColorMode colorMode;
        public Color color = Color.white;
        public Gradient colorOverLife;
        public int steps = 16;
        private Texture2D colorOverLifeTexture;
        #endregion

        #region Lifetime
        public float minLifetime = 3f;
        public float maxLifetime = 5f;
        #endregion

        #region Emission
        public bool enableEmission = true;
        public float emissionRate = 15000f;
        public float minInitialSpeed = 0.1f;
        public float maxInitialSpeed = 0.5f;
        public EmissionShape emissionShape;
        public float sphereRadius = 0.2f;
        public Vector3 boxSize;
        public float coneRadius;
        public float coneAngle;
        public float coneLength;
        public MeshType meshType;
        public Mesh emissionMesh;
        public MeshRenderer meshRenderer;
        public SkinnedMeshRenderer skinnedMeshRenderer;
        public float circleRadius;
        public Vector3 edgeStart;
        public Vector3 edgeEnd;
        public float edgeRadius;
        public DirectionType directionType;
        public Vector3 direction;
        #endregion

        #region Inherit Velocity
        public bool enableInheritVelocity = true;
        public float inheritVelocity = 0.3f;
        public float extrapolation = 0f;
        private Vector3 previousPositon;
        #endregion

        #region Noise
        public bool enableNoise = true;
        public float convergence = 150f;
        public float convergenceFrequency = 10f;
        public float convergenceAmplitude = 200f;
        public float viscosity = 0.1f;
        #endregion

        #region Constant Influence
        public bool enableConstantInfluence = true;
        public Vector3 constantVelocity;
        public new Vector3 constantForce = Vector3.up;
        public float linearDrag = 1f;
        #endregion

        #region Rendering
        public RenderingMethod renderingMethod = RenderingMethod.Billboard;
        public Mesh particleMesh;
        public Material particleMaterial;
        #endregion

        #region Assets
        public ComputeShader computeShader;
        public Material renderMaterial;
        #endregion

        #endregion

        #region Other Variables

        private int initKernel, emitKernel, updateKernel;
        private struct Particle
        {
            public bool alive;
            public Vector3 position;
            public Vector3 velocity;
            public Vector2 life; //x = age, y = lifetime
            public Color color;
        }
        private ComputeBuffer particles, dead, quads, counter;
        private int bufferSize = 100096;
        private int groupCount;
        private int[] counterArray;
        private int deadCount = 0;

        #endregion

        #region Unity Functions

        private void Awake()
        {
            Camera.main.depthTextureMode = DepthTextureMode.Depth;

            DispatchInit();
        }

        private void Update()
        {
            DispatchUpdate();

            DispatchEmit(Mathf.RoundToInt(Time.deltaTime * emissionRate * timeScale));
        }

        private void OnRenderObject()
        {
            switch (renderingMethod)
            {
                case RenderingMethod.Billboard:
                    renderMaterial.SetBuffer("particles", particles);
                    renderMaterial.SetBuffer("quads", quads);

                    renderMaterial.SetPass(0);

                    Graphics.DrawProcedural(MeshTopology.Quads, 6, dead.count);
                    break;
                case RenderingMethod.Mesh:
                    int alive = GetAliveCount();
                    Particle[] tmp = new Particle[maxParticles];
                    particles.GetData(tmp);

                    int l = 0;
                    for (int i = 0; i < Mathf.CeilToInt(alive / 1000f); i++)
                    {
                        Matrix4x4[] matrices = new Matrix4x4[Mathf.CeilToInt(alive / 1000f)];
                        int j = 0;
                        for (int k = 0; k < Mathf.Min(1000, maxParticles); k++, l++)
                        {
                            if (tmp[l].alive)
                            {
                                matrices[j++] = Matrix4x4.Translate(tmp[l].position);
                            }
                        }
                        Graphics.DrawMeshInstanced(particleMesh, 0, particleMaterial, matrices);
                    }

                    break;
            }
        }

        private void OnDestroy()
        {
            ReleaseBuffers();
        }

        #endregion

        #region Public Functions

        public int GetAliveCount()
        {
            return bufferSize - deadCount;
        }

        public void UpdateColorOverLifeTexture()
        {
            steps = Mathf.Max(1, steps);

            colorOverLifeTexture = new Texture2D(steps, 1);

            for (int i = 0; i < steps; i++)
            {
                colorOverLifeTexture.SetPixel(i, 0, colorOverLife.Evaluate(1f / steps * i));
            }
            colorOverLifeTexture.Apply();
        }

        public void DispatchInit()
        {
            ReleaseBuffers();

            UpdateColorOverLifeTexture();

            previousPositon = transform.position;

            initKernel = computeShader.FindKernel("Init");
            emitKernel = computeShader.FindKernel("Emit");
            updateKernel = computeShader.FindKernel("Update");

            groupCount = Mathf.CeilToInt((float)maxParticles / THREAD_COUNT);
            bufferSize = groupCount * THREAD_COUNT;

            particles = new ComputeBuffer(bufferSize, Marshal.SizeOf(typeof(Particle)));
            dead = new ComputeBuffer(bufferSize, sizeof(int), ComputeBufferType.Append);
            dead.SetCounterValue(0);
            counter = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
            counterArray = new int[] { 0, 1, 0, 0 };

            computeShader.SetBuffer(initKernel, "particles", particles);
            computeShader.SetBuffer(initKernel, "dead", dead);

            computeShader.Dispatch(initKernel, groupCount, 1, 1);

            quads = new ComputeBuffer(6, Marshal.SizeOf(typeof(Vector3)));
            quads.SetData(new[]
            {
            new Vector3(-0.5f,0.5f),
            new Vector3(0.5f,0.5f),
            new Vector3(0.5f,-0.5f),
            new Vector3(0.5f,-0.5f),
            new Vector3(-0.5f,-0.5f),
            new Vector3(-0.5f,0.5f)
        });
        }
        public void DispatchEmit(int count)
        {
            if (enableEmission)
            {
                count = Mathf.Min(count, maxParticles - (bufferSize - deadCount));

                if (count > 0)
                {
                    Vector3 velocity = (transform.position - previousPositon) / Time.deltaTime;
                    previousPositon = transform.position;

                    computeShader.SetBuffer(emitKernel, "particles", particles);
                    computeShader.SetBuffer(emitKernel, "alive", dead);

                    computeShader.SetVector("seeds", new Vector3(Random.Range(1f, 10000f), Random.Range(1f, 10000f), Random.Range(1f, 10000f)));
                    computeShader.SetVector("initialSpeedRange", new Vector2(minInitialSpeed, maxInitialSpeed));
                    computeShader.SetVector("inheritedPosition", transform.position);
                    computeShader.SetVector("lifeRange", new Vector2(minLifetime, maxLifetime));
                    computeShader.SetVector("time", new Vector2(Time.deltaTime, Time.time));
                    computeShader.SetInt("colorMode", (int)colorMode);
                    computeShader.SetVector("color", color);

                    computeShader.SetInt("emissionShape", (int)emissionShape);
                    if (emissionShape == EmissionShape.Sphere)
                    {
                        computeShader.SetFloat("radius", Mathf.Max(0.01f, sphereRadius));
                    }
                    else if (emissionShape == EmissionShape.Box)
                    {
                        computeShader.SetVector("boxSize", boxSize);
                    }
                    else if (emissionShape == EmissionShape.Edge)
                    {
                        computeShader.SetVector("edgeStart", edgeStart);
                        computeShader.SetVector("edgeEnd", edgeEnd);
                        computeShader.SetFloat("radius", Mathf.Max(0.01f, edgeRadius));
                    }

                    computeShader.SetInt("directionType", (int)directionType);
                    if (directionType == DirectionType.Uniform)
                    {
                        computeShader.SetVector("direction", direction);
                    }

                    if (enableInheritVelocity)
                    {
                        computeShader.SetVector("inheritedVelocity", velocity * inheritVelocity);
                        computeShader.SetFloat("extrapolation", extrapolation);
                    }

                    computeShader.Dispatch(emitKernel, count, 1, 1);
                }
            }
        }

        #endregion

        #region Private Functions

        private int GetDeadCount()
        {
            return deadCount;
        }
        private void SetDeadCount()
        {
            if (dead == null || counter == null || counterArray == null)
            {
                deadCount = bufferSize;
                return;
            }
            counter.SetData(counterArray);
            ComputeBuffer.CopyCount(dead, counter, 0);
            counter.GetData(counterArray);
            deadCount = counterArray[0];
        }

        private void DispatchUpdate()
        {
            if (timeScale > 0)
            {
                computeShader.SetBuffer(updateKernel, "particles", particles);
                computeShader.SetBuffer(updateKernel, "dead", dead);

                if ((int)colorMode == 1)
                {
                    computeShader.SetInt("colorMode", (int)colorMode);
                    computeShader.SetTexture(emitKernel, "colorOverLife", colorOverLifeTexture);
                    computeShader.SetTexture(updateKernel, "colorOverLife", colorOverLifeTexture);
                    computeShader.SetInt("steps", steps);
                }

                if (enableNoise)
                {
                    computeShader.SetFloat("viscosity", viscosity);
                    computeShader.SetFloat("convergence", convergence + Mathf.PerlinNoise(Time.time * convergenceFrequency, Mathf.PingPong(Time.time * convergenceFrequency, 1.0f)) * convergenceAmplitude);
                }
                else
                {
                    computeShader.SetFloat("viscosity", 0f);
                }

                if (enableConstantInfluence)
                {
                    computeShader.SetVector("constantVelocity", constantVelocity);
                    computeShader.SetVector("constantForce", constantForce);
                    computeShader.SetFloat("linearDrag", linearDrag);
                }
                else
                {
                    computeShader.SetVector("constantVelocity", Vector3.zero);
                    computeShader.SetVector("constantForce", Vector3.zero);
                    computeShader.SetFloat("linearDrag", 0f);
                }

                computeShader.SetVector("time", new Vector2(Time.deltaTime * timeScale, Time.time));

                computeShader.Dispatch(updateKernel, groupCount, 1, 1);

                SetDeadCount();
            }
        }

        private void ReleaseBuffers()
        {
            if (particles != null)
            {
                particles.Release();
            }
            if (dead != null)
            {
                dead.Release();
            }
            if (counter != null)
            {
                counter.Release();
            }
            if (quads != null)
            {
                quads.Release();
            }
        }

        #endregion
    }
}

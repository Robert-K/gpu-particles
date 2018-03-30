using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;

namespace GPUParticles
{
    [AddComponentMenu("GPU Particle Emitter")]
    public class GPUParticles : MonoBehaviour
    {
        #region Public Properties

        #region General Properties

        public uint maxParticles = 100000;

        public SimulationSpace simulationSpace = SimulationSpace.World;

        public float timeScale = 1f;

        public int randomSeed = 0;

        #endregion

        #region Emission Properties

        public bool enableEmission = true;

        public EmissionMode emissionMode = EmissionMode.Throttle;

        public float emissionFrequency = 20000f;

        public uint emissionCount = 1;

        [Range(0f, 1f)]
        public float emissionThrottle = 1f;

        public float lifespan = 5f;

        [Range(0f, 1f)]
        public float lifespanRandomness = 0f;

        public EmissionShape emissionShape = EmissionShape.Box;

        public float sphereRadius = 1f;

        public Vector3 boxSize = Vector3.one;

        public Vector3 edgeStart = Vector3.one;

        public Vector3 edgeEnd = -Vector3.one;

        #endregion

        #region Velocity Properties

        public float inheritVelocity = 0.5f;

        public Vector3 initialVelocity = Vector3.one;

        [Range(0f, 1f)]
        public float speedRandomness = 0.75f;

        [Range(0f, 1f)]
        public float directionSpread = 0.5f;

        public Vector3 acceleration = Vector3.down;

        public float drag = 0.1f;

        [System.Serializable]
        public struct Effector
        {
            public Vector3 position;
            public float force;
        }

        public Effector[] effectors;

        #endregion

        #region Turbulent Noise Properties

        public bool enableNoise = true;

        public float noiseAmplitude = 0.1f;

        public float noiseFrequency = 2f;

        public float noiseEvolution = 1f;

        #endregion

        #region Color Properties

        public ColorMode colorMode = ColorMode.Constant;

        public Color color = Color.white;

        public Gradient colorEvolution;

        public int colorSteps = 16;

        [Range(0f, 1f)]
        public float colorRandomness = 0f;

        #endregion

        #region Size Properties

        public SizeMode sizeMode = SizeMode.Constant;

        public float size = 1f;

        public AnimationCurve sizeEvolution = AnimationCurve.EaseInOut(0.5f, 1f, 1f, 0f);

        public int sizeSteps = 16;

        [Range(0f, 1f)]
        public float sizeRandomness = 0.5f;

        #endregion

        #region Rendering Properties

        public bool enableRendering = true;

        public Mesh particleMesh;

        public Material particleMaterial;

        public ShadowCastingMode castShadows = ShadowCastingMode.Off;

        public bool receiveShadows = false;

        #endregion

        public uint AliveCount { get { return (uint)bufferSize - deadCount; } }

        #endregion

        #region Private Variables

        private float time, deltaTime;

        private float emissionInterval;

        private struct Particle
        {
            public bool alive;
            public Vector3 position;
            public Vector3 velocity;
            public float age;
            public float lifespan;
            public Color color;
            public float size;
        }
        private int stride = Marshal.SizeOf(typeof(Particle));

        private struct MeshData
        {
            public Vector3 vertex;
            public Vector2 uv;

            public MeshData(Vector3 vertex, Vector2 uv)
            {
                this.vertex = vertex;
                this.uv = uv;
            }
        }

        private Texture2D colorEvolutionTexture;

        private ComputeBuffer particlesBuffer, deadIDsBuffer, counterArgsBuffer, effectorsBuffer, meshBuffer, sizeEvolutionBuffer;
        private const int GROUP_SIZE = 256;
        private int bufferSize = 100096;
        private int groupCount;
        private uint[] counterArgsArray;
        private uint deadCount = 100096;
        private MeshTopology meshTopology = MeshTopology.Quads;
        private int vertexCount = 6;

        [SerializeField, HideInInspector]
        private Mesh defaultMesh;

        [SerializeField, HideInInspector]
        private ComputeShader computeShader;

        private int initKernel, emitKernel, updateKernel;

        #endregion

        #region Public Methods

        public void DispatchInit()
        {
            ReleaseBuffers();

            Random.InitState(randomSeed);

            groupCount = Mathf.CeilToInt((float)maxParticles / GROUP_SIZE);
            bufferSize = groupCount * GROUP_SIZE;

            UpdateColorEvolutionTexture();
            UpdateSizeEvolutionBuffer();
            UpdateEffectorsBuffer();
            UpdateMeshBuffer();

            particlesBuffer = new ComputeBuffer(bufferSize, stride);
            deadIDsBuffer = new ComputeBuffer(bufferSize, sizeof(uint), ComputeBufferType.Append);
            deadIDsBuffer.SetCounterValue(0);

            counterArgsArray = new uint[] { 0, 1, 0, 0 };
            counterArgsBuffer = new ComputeBuffer(1, 4 * sizeof(uint), ComputeBufferType.IndirectArguments);

            initKernel = computeShader.FindKernel("Init");
            emitKernel = computeShader.FindKernel("Emit");
            updateKernel = computeShader.FindKernel("Update");

            computeShader.SetBuffer(initKernel, "particles", particlesBuffer);
            computeShader.SetBuffer(initKernel, "deadIDsAppend", deadIDsBuffer);

            computeShader.Dispatch(initKernel, groupCount, 1, 1);

            deadCount = (uint)bufferSize;
        }

        public void DispatchEmit(uint count)
        {
            count = (uint)Mathf.Min(count, maxParticles - AliveCount);
            if (count > 0)
            {
                computeShader.SetFloat("time", time);
                computeShader.SetInt("seed", randomSeed);

                computeShader.SetInt("emissionShape", (int)emissionShape);
                switch (emissionShape)
                {
                    case EmissionShape.Sphere:
                        computeShader.SetFloat("sphereRadius", sphereRadius);
                        break;
                    case EmissionShape.Box:
                        computeShader.SetVector("boxSize", boxSize);
                        break;
                    case EmissionShape.Edge:
                        computeShader.SetVector("edgeStart", edgeStart);
                        computeShader.SetVector("edgeEnd", edgeEnd);
                        break;
                }

                if (colorMode == ColorMode.Constant)
                {
                    computeShader.SetVector("color", color);
                }
                else
                {
                    computeShader.SetVector("color", colorEvolution.Evaluate(0f));
                }

                if (sizeMode == SizeMode.Constant)
                {
                    computeShader.SetFloat("size", size);
                    computeShader.SetFloat("sizeRandomness", sizeRandomness);
                }
                else
                {
                    computeShader.SetFloat("size", sizeEvolution.Evaluate(0f) * size);
                    computeShader.SetFloat("sizeRandomness", 0f);
                }

                computeShader.SetFloat("lifespan", lifespan);
                computeShader.SetFloat("lifespanRandomness", lifespanRandomness);

                computeShader.SetVector("initialVelocity", initialVelocity);
                computeShader.SetFloat("speedRandomness", speedRandomness);
                computeShader.SetFloat("directionSpread", directionSpread);

                computeShader.SetBuffer(emitKernel, "particles", particlesBuffer);
                computeShader.SetBuffer(emitKernel, "deadIDsConsume", deadIDsBuffer);

                computeShader.Dispatch(emitKernel, (int)count, 1, 1);
            }
        }

        public void DispatchUpdate()
        {
            computeShader.SetFloat("time", time);
            computeShader.SetFloat("deltaTime", deltaTime);
            computeShader.SetInt("seed", randomSeed);

            computeShader.SetFloat("drag", 1f - drag * deltaTime);
            computeShader.SetVector("acceleration", acceleration * deltaTime);

            if (colorMode == ColorMode.OverLife)
            {
                computeShader.SetTexture(updateKernel, "colorEvolutionTexture", colorEvolutionTexture);
                computeShader.SetBool("colorEvolution", true);
            }
            else computeShader.SetBool("colorEvolution", false);

            if (sizeMode == SizeMode.OverLife)
            {
                computeShader.SetBuffer(updateKernel, "sizeEvolutionBuffer", sizeEvolutionBuffer);
                computeShader.SetFloat("size", size);
                computeShader.SetInt("sizeSteps", sizeSteps);
                computeShader.SetBool("sizeEvolution", true);
            }
            else computeShader.SetBool("sizeEvolution", false);

            computeShader.SetBuffer(updateKernel, "effectors", effectorsBuffer);
            computeShader.SetInt("effectorCount", effectors.Length);

            computeShader.SetBuffer(updateKernel, "particles", particlesBuffer);
            computeShader.SetBuffer(updateKernel, "deadIDsAppend", deadIDsBuffer);

            computeShader.Dispatch(updateKernel, GROUP_SIZE, 1, 1);

            UpdateDeadCount();
        }

        public void ResetEmissionInterval()
        {
            emissionInterval = 0f;
        }

        public void UpdateEffectorsBuffer()
        {
            ReleaseBuffer(effectorsBuffer);
            if (effectors.Length > 0)
            {
                effectorsBuffer = new ComputeBuffer(effectors.Length, Marshal.SizeOf(typeof(Effector)));
                effectorsBuffer.SetData(effectors);
            }
            else
            {
                effectorsBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(Effector)));
            }
        }

        public void UpdateMeshBuffer()
        {
            if (particleMesh == null)
                particleMesh = defaultMesh;

            meshTopology = particleMesh.GetTopology(0);
            vertexCount = particleMesh.vertexCount;
            MeshData[] data = new MeshData[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                data[i].vertex = particleMesh.vertices[i];
                data[i].uv = particleMesh.uv[i];
            }
            ReleaseBuffer(meshBuffer);
            meshBuffer = new ComputeBuffer(vertexCount, Marshal.SizeOf(typeof(MeshData)));
            meshBuffer.SetData(data);
        }

        public void UpdateColorEvolutionTexture()
        {
            colorEvolutionTexture = new Texture2D(colorSteps, 1);

            for (int i = 0; i < colorSteps; i++)
            {
                colorEvolutionTexture.SetPixel(i, 0, colorEvolution.Evaluate(1f / colorSteps * i));
            }
            colorEvolutionTexture.Apply();
        }

        public void UpdateSizeEvolutionBuffer()
        {
            if (sizeEvolutionBuffer != null) sizeEvolutionBuffer.Release();
            sizeEvolutionBuffer = new ComputeBuffer(sizeSteps, sizeof(float));

            float[] temp = new float[sizeSteps];
            for (int i = 0; i < sizeSteps; i++)
            {
                temp[i] = sizeEvolution.Evaluate(1f / sizeSteps * i);
            }

            sizeEvolutionBuffer.SetData(temp);
        }

        #endregion

        #region Private Methods

        private void UpdateDeadCount()
        {
            if (deadIDsBuffer == null || counterArgsBuffer == null || counterArgsArray == null)
            {
                deadCount = (uint)bufferSize;
                return;
            }
            counterArgsBuffer.SetData(counterArgsArray);
            ComputeBuffer.CopyCount(deadIDsBuffer, counterArgsBuffer, 0);
            counterArgsBuffer.GetData(counterArgsArray);
            deadCount = counterArgsArray[0];
        }

        private void UpdateTime()
        {
            time = Time.time * timeScale;
            deltaTime = Time.deltaTime * timeScale;
        }

        private void EmissionUpdate()
        {
            if (enableEmission)
            {
                emissionInterval -= deltaTime;

                float emissionRate;
                if (emissionMode == EmissionMode.Frequency)
                {
                    emissionRate = 1f / emissionFrequency * timeScale;
                }
                else
                {
                    emissionRate = 1f / (emissionThrottle * maxParticles / lifespan);
                }

                uint count = 0;
                while (emissionInterval < emissionRate)
                {
                    emissionInterval += emissionRate;
                    count++;
                }

                count *= emissionCount;

                DispatchEmit(count);
            }
        }

        private void ReleaseBuffers()
        {
            ReleaseBuffer(particlesBuffer);
            ReleaseBuffer(deadIDsBuffer);
            ReleaseBuffer(counterArgsBuffer);
            ReleaseBuffer(effectorsBuffer);
            ReleaseBuffer(sizeEvolutionBuffer);
            ReleaseBuffer(meshBuffer);
        }

        private void ReleaseBuffer(ComputeBuffer buffer)
        {
            if (buffer != null) buffer.Release();
            buffer = null;
        }

        #endregion

        #region MonoBehaviour Methods

        private void Awake()
        {
            DispatchInit();
        }

        private void Update()
        {
            UpdateTime();

            EmissionUpdate();

            DispatchUpdate();
        }

        private void OnRenderObject()
        {
            if (enableRendering)
            {
                particleMaterial.SetBuffer("particles", particlesBuffer);
                particleMaterial.SetBuffer("mesh", meshBuffer);
                particleMaterial.SetPass(0);
                Graphics.DrawProcedural(meshTopology, vertexCount, bufferSize);
            }
        }

        private void OnDestroy()
        {
            ReleaseBuffers();
        }

        #endregion
    }
}

namespace GPUParticles
{
    public enum SimulationSpace
    {
        World,
        Local,
    }

    public enum ColorMode
    {
        Constant,
        OverLife,
    }

    public enum SizeMode
    {
        Constant,
        OverLife,
    }

    public enum EmissionShape
    {
        Sphere,
        Box,
        Edge,
    }

    public enum EmissionMode
    {
        Frequency,
        Throttle
    }

    public enum RenderingMethod
    {
        DrawProcedural,
        Instancing,
        MeshBatching,
    }
}

namespace GPUParticles
{
    public enum SimulationSpace
    {
        Local,
        World,
        Custom,
    }
    public enum ColorMode
    {
        Constant,
        OverLife,
        Random,
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
        Cone,
        Mesh,
        MeshRenderer,
        SkinnedMeshRenderer,
        Circle,
        Edge,
    }
    public enum DirectionType
    {
        Outwards,
        Uniform
    }
    public enum MeshType
    {
        Vertex,
        Edge,
        Triangle,
    }
    public enum RenderingMethod
    {
        Billboard,
        Mesh,
    }
}
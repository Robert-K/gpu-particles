#ifndef PARTICLE_STRUCT
#define PARTICLE_STRUCT

struct Particle
{
    bool alive;
    float3 position;
    float3 velocity;
    float2 life; //x = age, y = lifetime
    float4 color;
    float size;
};

#endif
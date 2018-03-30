Shader "Custom/Billboard Particles"
{
	Properties
	{
		_MainTex("Particle Sprite", 2D) = "white" {}
	_SizeMul("Size Multiplier", Float) = 1
	}

		SubShader
	{
		Pass
	{
		Cull Back
		Lighting Off
		Zwrite Off

		//Blend SrcAlpha OneMinusSrcAlpha
		//Blend One OneMinusSrcAlpha
		Blend One One
		//Blend OneMinusDstColor One

		LOD 200

		Tags
	{
		"RenderType" = "Transparent"
		"Queue" = "Transparent"
		"IgnoreProjector" = "True"
	}

		CGPROGRAM

#pragma target 5.0
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"
#include "./Common.cginc"

		struct MeshData
	{
		float3 vertex;
		float2 uv;
	};

		uniform sampler2D _MainTex;
	float _SizeMul;

	StructuredBuffer<Particle> particles;
	StructuredBuffer<MeshData> mesh;

	struct v2f
	{
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
		float4 col : COLOR;
	};

	v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
	{
		v2f o;

		float3 q = mesh[id].vertex;

		//*BILLBOARD:*/ o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(particles[inst].position, 1.0f)) + float4(q, 0.0f) * _SizeMul * particles[inst].size);
		o.pos = mul(UNITY_MATRIX_VP, float4(particles[inst].position, 1.0f) + float4(q, 0.0f) * _SizeMul * particles[inst].size);

		o.uv = mesh[id].uv;

		o.col = particles[inst].alive * particles[inst].color;

		return o;
	}

	fixed4 frag(v2f i) : COLOR
	{
		return tex2D(_MainTex, i.uv) * i.col * i.col.a;
	}

		ENDCG
	}
	}
}
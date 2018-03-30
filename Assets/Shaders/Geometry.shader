// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "GPU Particles/GS Billboard"
{
	Properties
	{
		_SpriteTex("Base (RGB)", 2D) = "white" {}
	_Size("Size", Range(0, 3)) = 0.5
	}

		SubShader
	{
		Pass
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 200

		Blend One One
		Cull Off
		Zwrite Off
		Lighting Off
		Fog {Mode Off}

		CGPROGRAM
#pragma target 5.0
#pragma vertex VS_Main
#pragma fragment FS_Main
#pragma geometry GS_Main
#include "UnityCG.cginc" 
#include "Common.cginc"

		// **************************************************************
		// Data structures												*
		// **************************************************************
		struct GS_INPUT
	{
		float4	pos		: POSITION;
		float2  tex0	: TEXCOORD0;
		float4 col		: COLOR;
	};

	struct FS_INPUT
	{
		float4	pos		: POSITION;
		float2  tex0	: TEXCOORD0;
		float4 col		: COLOR;
	};


	// **************************************************************
	// Vars															*
	// **************************************************************

	float _Size;
	float4x4 _VP;
	sampler2D _SpriteTex;
	StructuredBuffer<Particle> particles;

	// **************************************************************
	// Shader Programs												*
	// **************************************************************

	// Vertex Shader ------------------------------------------------
	GS_INPUT VS_Main(uint id : SV_VertexID, uint inst : SV_InstanceID)
	{
		GS_INPUT output = (GS_INPUT)0;

		output.pos = float4(particles[inst].position,particles[inst].size);
		output.tex0 = float2(0, 0);
		output.col = particles[inst].color;

		return output;
	}

	// Geometry Shader -----------------------------------------------------
	[maxvertexcount(4)]
	void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
	{
		float3 up = float3(0, 1, 0);
		float3 look = _WorldSpaceCameraPos - p[0].pos;
		look.y = 0;
		look = normalize(look);
		float3 right = cross(up, look);

		float halfS = 0.5f * _Size * p[0].pos.w;

		float4 v[4];
		v[0] = float4(p[0].pos + halfS * right - halfS * up, 1.0f);
		v[1] = float4(p[0].pos + halfS * right + halfS * up, 1.0f);
		v[2] = float4(p[0].pos - halfS * right - halfS * up, 1.0f);
		v[3] = float4(p[0].pos - halfS * right + halfS * up, 1.0f);

		FS_INPUT pIn;

		pIn.col = p[0].col;

		pIn.pos = UnityObjectToClipPos(v[0]);
		pIn.tex0 = float2(1.0f, 0.0f);
		triStream.Append(pIn);

		pIn.pos = UnityObjectToClipPos(v[1]);
		pIn.tex0 = float2(1.0f, 1.0f);
		triStream.Append(pIn);

		pIn.pos = UnityObjectToClipPos(v[2]);
		pIn.tex0 = float2(0.0f, 0.0f);
		triStream.Append(pIn);

		pIn.pos = UnityObjectToClipPos(v[3]);
		pIn.tex0 = float2(0.0f, 1.0f);
		triStream.Append(pIn);
	}



	// Fragment Shader -----------------------------------------------
	float4 FS_Main(FS_INPUT input) : COLOR
	{
		return tex2D(_SpriteTex,input.tex0) * input.col;
	}

		ENDCG
	}
	}
}
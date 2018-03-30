Shader "GPU Particles/DrawProcedural"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_SizeMul("Size Multiplier", Float) = 1
	}
		SubShader
		{
			Tags
			{
				"RenderType" = "Transparent"
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
			}

			LOD 200

			Pass
			{
			Cull Back
			Lighting Off
			Zwrite Off

			//Blend SrcAlpha OneMinusSrcAlpha
			//Blend One OneMinusSrcAlpha
			Blend One One
			//Blend OneMinusDstColor One

			CGPROGRAM
			#pragma target 4.5
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Common.cginc"

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
				float4 col : COLOR;
			};

			struct MeshData
			{
				float3 vertex;
				float2 uv;
			};

			sampler2D _MainTex;
			float _SizeMul;

			StructuredBuffer<Particle> particles;
			StructuredBuffer<MeshData> mesh;

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				o.pos = mul(UNITY_MATRIX_VP, float4(particles[inst].position + mesh[id].vertex, 0.0) * particles[inst].size * _SizeMul);

				o.uv = mesh[id].uv;

				o.col = particles[inst].alive * particles[inst].color;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return tex2D(_MainTex, i.uv) * i.col;
			}
			ENDCG
		}
		}
}

Shader "Custom/VisualizeParticles"
{
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		_ScaleX("Scale X", Float) = 1.0
		_ScaleY("Scale Y", Float) = 1.0
	}
		SubShader{
			Tags { "Queue" = "Transparent"}

			Pass {
				Cull Back
				Zwrite Off
				Blend One One

				CGPROGRAM

				#pragma vertex vert  
				#pragma fragment frag

				uniform sampler2D _MainTex;
				uniform float _ScaleX;
				uniform float _ScaleY;

				#include "./Common.cginc"

				StructuredBuffer<Particle> particles;
				StructuredBuffer<float3> quads;

				struct v2f {
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;
				};

				v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
				{
					v2f o;

					float3 worldPos = particles[inst].position;
					float3 quad = quads[id];

					o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPos, 1.0f)) + float4(quad, 0.0f) * float4(_ScaleX, _ScaleY, 1.0, 1.0));

					o.uv = quad + 0.5f;

					o.color = particles[inst].alive * particles[inst].color;

					return o;
				}

				float4 frag(v2f i) : SV_Target
				{
					fixed4 c = tex2D(_MainTex, float2(i.uv.xy)) * i.color;
					c.rgb *= c.a;
					return c;
				}

				ENDCG
			}
		}
}

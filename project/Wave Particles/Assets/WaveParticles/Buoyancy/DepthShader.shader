Shader "WaveParticles/DepthShader"
{
	Properties
	{
		[HideInInspector] _WaveParticles_Velocity_x("_WaveParticles_Velocity_x", Float) = 0.0125
		[HideInInspector] _WaveParticles_Velocity_y("_WaveParticles_Velocity_y", Float) = 0.0125
		[HideInInspector] _WaveParticles_Velocity_z("_WaveParticles_Velocity_z", Float) = 0.0125
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 100

		Pass
		{
			Blend One One
			BlendOp Add
			Cull Off
			ZWrite Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float3 worldPos : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 worldNormal : NORMAL;
			};

			float _WaveParticles_Velocity_x;
			float _WaveParticles_Velocity_y;
			float _WaveParticles_Velocity_z;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			    o.worldNormal = UnityObjectToWorldNormal(v.normal);
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				// sample the texture
				// TODO: make this a function of water-surface velocity
				float3 velocity = float3(_WaveParticles_Velocity_x, _WaveParticles_Velocity_y, _WaveParticles_Velocity_z);

				float depth = 0;
				float waterPushed = 0;
				float waterSucked = 0;
				if (i.worldPos.y < 0) {
					depth = i.worldPos.y;
					if (i.worldNormal.y < 0) {
						depth *= -1;
						waterPushed = -1 * dot(velocity, float3(0, 1, 0)) * (1.0/depth);
					}
					else {
						waterSucked = -1 * dot(velocity, float3(0, 1, 0))* (-1.0 / depth);
					}
				}
				float4 col = float4(waterPushed, depth, waterSucked, 1);
				return col;
			}
			ENDCG
		}
	}
}

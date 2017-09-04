Shader "WaveParticles/WaveShader" {
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Cube("Cube Map", Cube) = "white" {}
		_MurkColor("Murk Color", Color) = (0.5, 0.5, 0.5, 1)
		_ReflectionColor("Reflection Color", Color) = (0.5, 0.5, 0.5, 1)
		_Emission("Emission", Range(-1, 1)) = 0

		[HideInInspector] [HDR] _FieldTex("Texture", 2D) = "white" {}
		[HideInInspector] _VertexEnabled("Vertex Enabled", Int) = 1
		[HideInInspector] _UnitX("UnitX", Float) = 0.0125
		[HideInInspector] _UnitY("UnitY", Float) = 0.0125
		[HideInInspector] _HoriResInverse("_HoriResInverse", Float) = 0.0
		[HideInInspector] _VertResInverse("_VertResInverse", Float) = 0.0
	}
		SubShader{
			Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
			LOD 200

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma multi_compile __ SHOW_DEBUG_TEXTURE_ON

			//TODO workout how to correctly handle alpha blending
			#pragma surface surf Standard fullforwardshadows vertex:vert alpha

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			#include "DisplaceVertices.cginc"

			sampler2D _MainTex;
			sampler2D _FieldTex;
			int _VertexEnabled;
			float _UnitX;
			float _UnitY;
			float _HoriResInverse;
			float _VertResInverse;
			float _Emission;


			half _Glossiness;
			half _Metallic;
			fixed4 _MurkColor;
			fixed4 _ReflectionColor;

			struct Input {
				float2 uv_MainTex;
				float3 worldRefl;
				float3 worldNormal;
			};

			void vert(inout appdata_full v) {
				if (_VertexEnabled) {
					// Find the position in the texture where displacement information is located.
					float2 tex_position = v.texcoord.xy + float2(_HoriResInverse * 0.5, _VertResInverse * 0.5);
					// Look up the diplacement value for the current vertex
					float4 displacement = tex2Dlod(_FieldTex, float4(tex_position, 0, 0));

					v.vertex.xyz += displacement.xyz;

					{
						// Calculate the normal of the current vertex
						float4 displacement_north = tex2Dlod(_FieldTex, float4(tex_position + float2(0.00, _VertResInverse), 0, 0));
						float4 displacement_east = tex2Dlod(_FieldTex, float4(tex_position + float2(_HoriResInverse, 0.00), 0, 0));
						float4 displacement_south = tex2Dlod(_FieldTex, float4(tex_position + float2(0.00, -_VertResInverse), 0, 0));
						float4 displacement_west = tex2Dlod(_FieldTex, float4(tex_position + float2(-_HoriResInverse, 0.00), 0, 0));
						v.normal = calculateNormal(
							displacement_north.y, displacement_east.y, displacement_south.y, displacement_west.y, displacement.y,
							_UnitX, _UnitY);
					}
				}
			}

			samplerCUBE _Cube;

			void surf(Input IN, inout SurfaceOutputStandard o) {
				// Albedo comes from a texture tinted by color
				// fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
				const float gridSize = 0.125;
				const float gridThickness = 0.0125;

				const float cosAngle = dot(normalize(IN.worldRefl), normalize(IN.worldNormal));
				const float normalReflectance = 0.001;

				const float proportionOpaque = 1 - _ReflectionColor.a;

				// Using Schlick's equation
				const float proportionReflected = normalReflectance + ((1 - normalReflectance) * pow((1 - cosAngle), 5));
				
				fixed4 c;
				// Determine whether or not to show debug texture
				#if SHOW_DEBUG_TEXTURE_ON
					float2 texIndex;
					texIndex = IN.uv_MainTex + float2(_HoriResInverse * 0.5, _VertResInverse * 0.5);
					c = tex2D(_FieldTex, texIndex);
				#else
					float alpha = proportionOpaque;
					fixed4 color = _MurkColor;
					c = color;
					o.Emission = (texCUBE(_Cube, IN.worldRefl).rgb * _ReflectionColor.rgb * proportionReflected) * (1 - proportionOpaque) + fixed3(_Emission, _Emission, _Emission);
				#endif

				o.Albedo = c.rgb;
				o.Alpha = c.a;
				o.Metallic = 0;
				o.Smoothness = 0;
			}
			ENDCG
		}
		CustomEditor "WaveShaderEditor"
		FallBack "Diffuse"
}

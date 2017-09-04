Shader "Unlit/1DFunction"
{
	Properties
	{
		[HDR] _MainTex("Texture", 2D) = "white" {}
		_HoriRes("Horizontal Resolution", Int) = 80
		_VertRes("Vertical Resolution", Int) = 80
		_Width("Width", Float) = 4.0
		_Height("Height", Float) = 4.0
		_ParticleRadii("Particle Radii", Float) = 0.2
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 100

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					float4 vertex : SV_POSITION;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float _HoriRes;
				float _VertRes;
				float _Width;
				float _Height;
				float _ParticleRadii;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					//// sample the texture
					const float kernelWidth = ceil((_ParticleRadii / _Width) * _HoriRes);
					const float kernelHeight = ceil((_ParticleRadii / _Height) * _VertRes);
					const float unitX = _Width / _HoriRes;
					const float unitY = _Height / _VertRes;

					float4 color = float4(0, 0, 0, 1);

					for (float x = 0; x < kernelWidth; x++) {
						float2 coords = i.uv + float2(x / _HoriRes, 0);
						float4 newVal = tex2D(_MainTex, coords);

						float x_component = ((kernelWidth / 2) - x) * unitX;
						float abs_diff = x_component;
						float relativePixelDistance = (3.141 * abs_diff) / _ParticleRadii;

						float y_displacement_factor = 0.5f * (cos(relativePixelDistance) + 1);
						newVal *= y_displacement_factor;
						color += newVal;
					}
					return color;
				}
				ENDCG
			}

			GrabPass
			{
				"_MainTex"
			}

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					float4 vertex : SV_POSITION;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float _HoriRes;
				float _VertRes;
				float _Width;
				float _Height;
				float _ParticleRadii;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					//// sample the texture
					const float kernelWidth = ceil((_ParticleRadii / _Width) * _HoriRes);
					const float kernelHeight = ceil((_ParticleRadii / _Height) * _VertRes);
					const float unitX = _Width / _HoriRes;
					const float unitY = _Height / _VertRes;

					float4 color = float4(0, 0, 0, 1);

					for (float y = 0; y < kernelHeight; y++) {
						float2 coords = i.uv + float2(0, y / _VertRes);
						float4 newVal = tex2D(_MainTex, coords);

						float y_component = ((kernelHeight / 2) - y) * unitY;
						float abs_diff = y_component;
						float relativePixelDistance = (3.141 * abs_diff) / _ParticleRadii;

						float y_displacement_factor = 0.5f * (cos(relativePixelDistance) + 1);
						newVal *= y_displacement_factor;
						color += newVal;
					}

					return color;
				}
				ENDCG
			}
		}
}


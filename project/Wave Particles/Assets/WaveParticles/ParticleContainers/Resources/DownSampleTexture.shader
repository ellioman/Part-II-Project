// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/DownSampleTexture"
{
	Properties
	{
		[HDR] _MainTex("Texture", 2D) = "white" {}
		_antiAliasFactor("Anti Alias Factor", Int) = 1
		_textureWidth("Texture Width", Int) = 100
		_textureHeight("Texture Height", Int) = 100
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			int _antiAliasFactor;
			int _textureHeight;
			int _textureWidth;

			fixed4 frag (v2f i) : SV_Target
			{
				float2 pixelOffset = float2(1.0f / (_textureWidth * _antiAliasFactor), 1.0f / (_textureHeight * _antiAliasFactor));
				float4 col = tex2D(_MainTex, i.uv);
				for (int x = 0; x < _antiAliasFactor; x++) {
					for (int y = 0; y < _antiAliasFactor; y++) {
						col += tex2D(_MainTex, i.uv + (float2(x, y) * pixelOffset));
					}
				}
				col *= 1.0f/(_antiAliasFactor * _antiAliasFactor);
				col.a = 1.0f;
				return col;
			}
			ENDCG
		}
	}
}

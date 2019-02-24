Shader "Unlit/DrawpDepth"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ZParams ("ZParams (near,far,0,0)", Vector) = (0,1,0,0) 
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			ZWrite On
			ZTest Always
			ColorMask 0
		
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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _ZParams;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			float frag (v2f i) : SV_Depth
			{
				float4 col = tex2D(_MainTex, i.uv);
			
				// _ZBufferParams:
				// x = 1-f/n
				// y = f/n
				// z = x/far
				// w = y/far
				
				float g_Afx_zNear = _ZParams.x;
				float g_Afx_zFar = _ZParams.y;
				
				float f1 = (-1) * g_Afx_zFar * g_Afx_zNear * 1.0;
				float xD = g_Afx_zFar - g_Afx_zNear;
				
				const float4 kDecodeDot = float4(1.0, 1/255.0, 1/65025.0, 1/16581375.0);
				float depth = dot( col, kDecodeDot );
				
				// decode to linear in inch:
				depth = f1/(depth * xD -g_Afx_zFar);
				
				// to meters (Unity):
				depth = depth * 2.54 / 100.0;
				
				// to Unity zBuffer (inverse of LinearEyeDepth):
				depth = (1.0/depth - _ZBufferParams.w) / _ZBufferParams.z;
				
				return depth;
			}
			ENDCG
		}
	}
}

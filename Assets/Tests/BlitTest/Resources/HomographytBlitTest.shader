Shader "Hidden/HomographytBlitTest"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	
	SubShader
	{
		CGINCLUDE
		
		#include "UnityCG.cginc"

		struct ToVert
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct ToFrag
		{
			float2 uv : TEXCOORD0;
			float4 vertex : SV_POSITION;
		};
		
		sampler2D _MainTex;
		float4x4 _Homography;
		
		
		ToFrag Vert( ToVert v )
		{
			ToFrag o;
			o.vertex = UnityObjectToClipPos( v.vertex );
			o.vertex = mul( _Homography, o.vertex );
			o.uv = v.uv;
			return o;
		}
		
		fixed4 Frag( ToFrag i ) : SV_Target
		{
			return tex2D( _MainTex, i.uv );
		}
			
		ENDCG
		
		
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		
		
		// Pass 0: Clear background.
		Pass
		{
			Material {
				Ambient[_ClearColor]
			}
			
			SetTexture[_MainTex] {
				constantColor [_ClearColor]
				Combine constant, constant
			}
		}
		
		// Pass 1: Draw image.
		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			ENDCG
		}
	}
}

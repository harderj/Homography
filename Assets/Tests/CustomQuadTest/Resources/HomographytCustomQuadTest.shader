Shader "Hidden/HomographytCustomQuadTest"
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
		
		
		float4x4 _Matrix;
		
		
		ToFrag Vert( ToVert v )
		{
			ToFrag o;
			o.vertex = v.vertex;
			//o.vertex = UnityObjectToClipPos( v.vertex );
			o.vertex = mul( _Matrix, o.vertex );
			o.uv = v.uv;
			return o;
		}
		
		fixed4 Frag( ToFrag i ) : SV_Target
		{
			return fixed4( i.uv, 0, 1 );
		}
			
		ENDCG
		
		
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		
		// Pass 0: Draw image.
		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			ENDCG
		}
	}
}

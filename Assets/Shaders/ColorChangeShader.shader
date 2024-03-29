Shader "ColorChangeShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
	}
	
	// Shader code pasted into all further CGPROGRAM blocks
	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};
	
	sampler2D _MainTex;
	half4 _MainTex_ST;

 
	
	v2f vert( appdata_img v ) 
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		return o;
	} 
	
	half4 frag(v2f i) : SV_Target 
	{
		half2 coords = i.uv;
		coords = (coords - 0.5) * 2.0;		
		
	 
		half4 color = tex2D (_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv  , _MainTex_ST));

				// NINTENDOFILTER
		const half oneOver7 = 1.0 / 8.0;
		const half oneOver3 = 1.0 / 3.0;


		color.r = floor(color.r * 7.99) * oneOver7;
		color.g = floor(color.g * 7.99) * oneOver7;
		color.b = floor(color.b * 3.99) * oneOver3;

				//color.b=0.2;


		return color;
	}

	ENDCG 
	
Subshader {
 Pass {
	  ZTest Always Cull Off ZWrite Off

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      ENDCG
  }
  
}

Fallback off
	
} // shader

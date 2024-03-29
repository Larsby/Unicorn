// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// https://alastaira.wordpress.com/2015/08/07/unity-shadertoys-a-k-a-converting-glsl-shaders-to-cghlsl/

Shader "MegaRainBow" {


	Properties{
	               	  _MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader{
	 
				Blend One OneMinusSrcAlpha
		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 screenCoord : TEXCOORD1;
			};

			v2f vert (appdata v){
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.screenCoord.xy = ComputeScreenPos(o.vertex);
				return o;
			}


			sampler2D _MainTex;
fixed4 SampleSpriteTexture (float2 uv)
{
    fixed4 color = tex2D (_MainTex, uv);
    return color;
}


			fixed4 frag (v2f i) : SV_Target{
				
     fixed4 c = SampleSpriteTexture (i.uv);



        float2 z = i.uv.xy;                            
        
        float2 uv = i.uv*9.0;
		z = uv - float2(1,0);  
		uv.x -= 4.2;                          
		uv.y -= 3.0;                         


		uv *= float2(-z.y,z.x) / dot(uv,uv);
    	uv =   log(length(uv))*float2(.5, -.5) + _Time.y/2.
     	+ atan2(uv.y, uv.x)/6.3 * float2(15, 1);        
    	c = .5+.5*sin(6.*3.14159*uv.y+float4(0,2.1,-2.1,0));
                 // try also U.x
  		c /= max(c.x,max(c.y,c.z)); // saturates the rainbow

  		c.a = 1.0f;//SampleSpriteTexture (IN.texcoord).a;


		// NINTENDOFILTER
		//const half oneOver7 = 1.0 / 8.0;
		//const half oneOver3 = 1.0 / 3.0;


		//c.r = floor(c.r * 7.99) * oneOver7;
		//c.g = floor(c.g * 7.99) * oneOver7;
		//c.b = floor(c.b * 3.99) * oneOver3;

  return c;
}

 
    	 
 

 
				
			ENDCG
		}
	}
}

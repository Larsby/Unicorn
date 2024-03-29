// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// https://alastaira.wordpress.com/2015/08/07/unity-shadertoys-a-k-a-converting-glsl-shaders-to-cghlsl/

Shader "IceCream" {
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


			#define sizeVaer 1.0
			#define spirals 1.0
			#define time _Time.y

			#define PI 3.141593
			#define TPI PI*2.0

			#define blue float4(172.0/255.0, 218.0/255.0, 205.0/255.0, 1)
			#define pink float4(240.0/255.0, 180.0/255.0, 180.0/255.0, 1)
			#define white float4(140.0/255.0, 280.0/255.0, 180.0/255.0, 1)

		 


			fixed4 frag (v2f i) : SV_Target{
				
		 
     			fixed4 c = SampleSpriteTexture (i.uv);

			    float size = (sizeVaer + sin(time))*1.0;
			   // float2 uv = (IN.vertex.xy / _ScreenParams.xy-0.25*2.f)  ; 
			     float2 uv =  i.screenCoord.xy-0.25*2.f  ; 

			         
			    float d = sqrt(uv.x*uv.x+uv.y*uv.y)*size*2.0f; // Distance fom center
			    float a = atan2(uv.x, uv.y)/TPI*spirals; // Angle from center
			    
			    float v = frac(d + a - time); // Spirals!
			    
			    if (v<0.25) {
			    	c = blue;
			    } else if (v>0.5 && v<0.75) {
			    	c = pink;
			    } else {
			    	c = white;
			    }

			    c.r=v; //modification for gradients
			    c.g-=v; //modification for gradients

			    return c;
			}

 
				
			ENDCG
		}
	}
}

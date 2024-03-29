// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ShaderToy/TestJAO"{
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
				
 		//	float2 uv = i.uv;
		//	return float4(uv.y,uv.x,uv.x, uv.x);


		  fixed4 c = SampleSpriteTexture (i.uv);



                 float tau=6.28318;
                  
          //   float2 uv = (IN.texcoord.xy / _ScreenParams.xy )*500.00;;
       //    float2 uv = (IN.vertex.xy / _ScreenParams.xy )*5.00;;
        float2 uv = i.uv;
                uv.y+=_Time.y;


              fixed3 rainbow = sqrt(.5+sin((uv.y+float3(0,2,1)/3.)*tau)*.5);

         c.rgb = rainbow.rgb;

         float resolution = 3.4f;

int colr = c.r*resolution;
c.r = colr/resolution;;

int colg = c.g*resolution;
c.g = colg/resolution;;

int colb = c.b*resolution;
c.b = colb/resolution;

  

    c.rgb *= c.a;
    return c;


			}
				
			ENDCG
		}
	}
}

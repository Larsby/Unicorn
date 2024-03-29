// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// https://alastaira.wordpress.com/2015/08/07/unity-shadertoys-a-k-a-converting-glsl-shaders-to-cghlsl/

Shader "Custom/iceCreamOriginal" {
        Properties {
               	[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
                _LC("LC", Color) = (1,0,0,0)
        }
    SubShader
    {

          Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma target 2.0
            #pragma vertex SpriteVert
            #pragma fragment SpriteFrag
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #pragma multi_compile _ PIXELSNAP_ON

            #include "UnityCG.cginc"
            		

			struct appdata_t
			{
			    float4 vertex   : POSITION;
			    float4 color    : COLOR;
			    float2 texcoord : TEXCOORD0;
			    UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
			    float4 vertex   : SV_POSITION;
			    fixed4 color    : COLOR;
			    float2 texcoord : TEXCOORD0;
			 
			    UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f SpriteVert(appdata_t IN)
			{
			    v2f OUT;

			    UNITY_SETUP_INSTANCE_ID (IN);
			    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

			    OUT.vertex = UnityObjectToClipPos(IN.vertex);
			    OUT.texcoord = IN.texcoord;
			    OUT.color = IN.color ;

			    return OUT;
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

			fixed4 SpriteFrag(v2f IN) : SV_Target
			{
			    fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
			    float size = (sizeVaer + sin(time))*1.0;
			    float2 uv = (IN.vertex.xy / _ScreenParams.xy-0.25*2.f)  ; 
			 	uv.x *= _ScreenParams.x/_ScreenParams.y; // Aspect ratio
			   
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
    FallBack "Diffuse"
}




				 



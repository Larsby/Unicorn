// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ShaderToy/cooltruck"{
	   Properties{
                      _MainTex ("Base (RGB)", 2D) = "white" {}
    }
    SubShader{
     //   Tags { "Queue" = "Transparent" }
             Blend SrcAlpha OneMinusSrcAlpha   

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
				// I learned a very cool effect from Main Sequence Star
// https://www.shadertoy.com/view/4dXGR4#
// Thanks a lot.


   // float2 uv = i.screenCoord.xy * _ScreenParams.xy / _ScreenParams.xy;
    float2 uv = i.uv;
    float aspect = _ScreenParams.x / _ScreenParams.y;
    uv.x = uv.x * aspect;
    float2 p = uv - float2(0.5 * aspect, 0.5);
    float3 col = float3(.3, .30, .30);
    
    // you can apply square root if you want.
    float sqR = dot(p, p) * 8.0;
    
    // f(x) = (1 - sqrt(abs(1 - x))) / x; 
    // lim(x->0)f(x) = 0.5
    // lim(x->1)f(x) = 1.0
    // lim(x->2)f(x) = 0
    // lim(x->infinite)f(x) < 0, therefore scale can be used as the alpha value.
    float scale = (1.0 - sqrt(abs(1.0 - sqR)))/(sqR);
    
    float3 Color = float3(uv,0.5+0.5*sin(_Time.y)); // this is the default color when you create a new shader.
    col += Color * scale;
    float4 returncol = float4(col,scale);
    returncol.a=col;
    return returncol; // scale affects nothing here.
}

 
				
			ENDCG
		}
	}
}

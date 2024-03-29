// https://alastaira.wordpress.com/2015/08/07/unity-shadertoys-a-k-a-converting-glsl-shaders-to-cghlsl/

Shader "FastColor" {
        Properties {
                _MainTex ("Base (RGB)", 2D) = "white" {}
                _LC("LC", Color) = (1,0,0,0)
               
 
        }
       
        SubShader {
                Tags { "Queue" = "Geometry" }
               
                Pass {
                   
                        GLSLPROGRAM
                        #include "UnityCG.glslinc" 
                        #ifdef VERTEX
                       
                        varying vec2 TextureCoordinate;
                       
                        void main()
                        {
                                gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
                                TextureCoordinate = gl_MultiTexCoord0.xy;
                        }
                       
                        #endif
                       
                        #ifdef FRAGMENT

                        precision lowp float;                  
             
                        
                        varying vec2 TextureCoordinate;


 

#define CI vec3(.3,.5,.6)
#define CO vec3(.2)
#define CM vec3(.0)
#define CE vec3(.8,.7,.5)

float metaball(vec2 p, float r)
{
	return r / dot(p, p);
}

vec3 samplef(in vec2 uv)
{
	float t0 = sin(_Time.y * 1.9) * .46;
	float t1 = sin(_Time.y * 2.4) * .49;
	float t2 = cos(_Time.y * 1.4) * .57;

	float r = metaball(uv + vec2(t0, t2), .33) *
			  metaball(uv - vec2(t0, t1), .27) *
			  metaball(uv + vec2(t1, t2), .59);

	vec3 c = (r > .4 && r < .7)
			  ? (vec3(step(.1, r*r*r)) * CE)
			  : (r < .9 ? (r < .7 ? CO: CM) : CI);

	return c;
}

 



                        void main()
                        {
                        /*
                        	vec2 uv = (TextureCoordinate*2)-1.0;

    vec3 col = vec3(0);

 
    col += samplef(uv);
 
    
    gl_FragColor = vec4(clamp(col, .2, 1.), 1);

    gl_FragColor.r += 0.2;
    */

    	float time=_Time.y*0.010;
	//vec2 uv = (fragCoord.xy / iResolution.xx-0.5)*8.0;
	vec2 uv = TextureCoordinate;
    vec2 uv0=uv;
	float i0=1.0;
	float i1=1.0;
	float i2=1.0;
	float i4=0.0;
	for(int s=0;s<7;s++)
	{
		vec2 r;
		r=vec2(cos(uv.y*i0-i4+time/i1),sin(uv.x*i0-i4+time/i1))/i2;
        r+=vec2(-r.y,r.x)*0.3;
		uv.xy+=r;
        
		i0*=1.93;
		i1*=1.15;
		i2*=1.7;
		i4+=0.05+0.1*time*i1;
	}
    float r=sin(uv.x-time)*0.5+0.5;
    float b=sin(uv.y+time)*0.5+0.5;
    float g=sin((uv.x+uv.y+sin(time*0.5))*0.5)*0.5+0.5;
	gl_FragColor = vec4(r,g,b,1.0);
  	 					 	gl_FragColor.r += (TextureCoordinate.x);
gl_FragColor = mix(gl_FragColor,vec4(0.2,0.5,0.2,1.0),0.5);
                       }
                       
                        #endif
                       
                        ENDGLSL
                }
        }
}




				 
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


 

#define RM_FACTOR   0.9
#define RM_ITERS     9

float plasma(vec3 r) {

float time = _Time.y * 0.5f;

	float mx = r.x + time / 0.130;
	mx += 20.0 * sin((r.y + mx) / 20.0 + time / 0.810);
	float my = r.y - time / 0.200;
	my += 30.0 * cos(r.x / 23.0 + time/ 0.710);
	return r.z - (sin(mx / 7.0) * 2.25 + sin(my / 3.0) * 2.25 + 5.5);
}

float scene(vec3 r) {
	return plasma(r);
}

float raymarch(vec3 pos, vec3 dir) {
	float dist = 0.0;
	float dscene;

	for (int i = 0; i < RM_ITERS; i++) {
		dscene = scene(pos + dist * dir);
		if (abs(dscene) < 0.1)
			break;
		dist += RM_FACTOR * dscene;
	}

	return dist;
}

//void mainImage(out vec4 fragColor, in vec2 fragCoord) 
       void main()
{

 	vec2 fragCoord =  TextureCoordinate.xy ;
		vec3 iResolution = _ScreenParams.xyz * 0.00059 ;

		fragCoord.x -= 0.5;

	float c, s;
	float vfov = 3.14159 / 2.3;

	vec3 cam = vec3(0.0, 0.0, 30.0);

	vec2 uv = (fragCoord.xy / iResolution.xy) ;//- 0.5;
	uv.x *= iResolution.x / iResolution.y;
	uv.y -= 1.0;

	vec3 dir = vec3(0.0, 0.0, -1.0);

	float xrot = vfov * length(uv);

	c = cos(xrot);
	s = sin(xrot);
	dir = mat3(1.0, 0.0, 0.0,
	           0.0,   c,  -s,
	           0.0,   s,   c) * dir;

	c = normalize(uv).x;
	s = normalize(uv).y;
	dir = mat3(  c,  -s, 0.0,
	             s,   c, 0.0,
	           0.0, 0.0, 1.0) * dir;

	//c = cos(0.15);
	//s = sin(-0.85);
	c = cos(1.25);
	s = sin(1.5);
	dir = mat3(  c, 0.0,   s,
	           0.0, 1.0, 0.0,
	            -s, 0.0,   c) * dir;

	float dist = raymarch(cam, dir);
	vec3 pos = cam + dist * dir;
	//FF 73 96 FF
	gl_FragColor.rgb = mix(
		vec3(0.0, 1.0, 0.4),
		mix(
			vec3(1.0, 1.0, 0.8f),
		//	vec3(0x73/255.0f, 0x96/255.0f, 0xFF/255.0f),
			vec3(1.0,0.5,1.0),
			pos.z / 20.0
		),
		1.0 / (dist / 80.0)
	);
}

 



 
                       
                        #endif
                       
                        ENDGLSL
                }
        }
}




				 
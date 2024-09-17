// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Clouds/Alpha" {
Properties {
	//not sure how useful a tint color will be as it should be in the mesh colors
	
	//Need to have these clouds fade in, and possibly fade out?
	_FadeStart("Fade Start", Float) = 90
	_FadeDistance("Fade Out Distance", Float) = 30
	
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Volume Texture", 2D) = "white" {}
	_MaskTex ("Mask Texture", 2D) = "white" {}
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha OneMinusSrcAlpha
	//AlphaTest Greater .01
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off

	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_particles
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _MaskTex;
			
			float _FadeStart;
			float _FadeDistance;
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};

			struct v2f {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};
			
			float4 _MainTex_ST;
			float4 _MaskTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				
				float3 viewPos = mul(UNITY_MATRIX_MV, v.vertex);
                float dist = length(viewPos);
                
                float fade = 1-saturate((dist-_FadeStart)/_FadeDistance);
                
                /*
                float nfadeout = saturate(dist / _FadeOutDistNear);
                float ffadeout = 1 - saturate(max(dist - _FadeOutDistFar,0) * 0.2);
                */
				
				o.color = v.color;
				o.color.a = v.color.a*fade;
				
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.texcoord1 = TRANSFORM_TEX(v.texcoord1,_MaskTex);
				return o;
			}

			//sampler2D _CameraDepthTexture;
			
			fixed4 frag (v2f i) : COLOR
			{
				/*
				//Soft particles could somehow be used here to some use...well...possibly.
				#ifdef SOFTPARTICLES_ON
				float sceneZ = LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
				float partZ = i.projPos.z;
				float fade = saturate (_InvFade * (sceneZ-partZ));
				i.color.a *= fade;
				#endif
				*/
				
				i.color.a *= tex2D(_MainTex, i.texcoord).a;
				i.color.a *= tex2D(_MaskTex, i.texcoord1).a;
				
				return 2.0f * i.color; // * tex2D(_MainTex, i.texcoord);
			}
			ENDCG 
		}
	}	
}
}

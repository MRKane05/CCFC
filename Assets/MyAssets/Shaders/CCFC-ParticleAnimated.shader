// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "CCFC/Particles/Atmosphere" {
	Properties {
		_MainTex ("Particle Mask Texture", 2D) = "white" {}
		_TexMap ("Particle Anim Texture", 2D) = "white" {}
		_TexAnim ("TexAnim Speed", Vector) = (1.0, 1.0, 1.0, 1.0)
	}

	Category {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend One OneMinusSrcColor
		ColorMask RGB
		Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
		
		// ---- Fragment program cards
		SubShader {
			Pass {
			
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_particles

				#include "UnityCG.cginc"

				sampler2D _MainTex;
				sampler2D _TexMap;
				fixed4 _TexAnim;
				float4 _TexOffset;
				fixed4 _TintColor;
				
				struct appdata_t {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					//float2 texcoord1 : TEXCOORD1;
				};

				float4 _MainTex_ST;
				
				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.color = v.color;
					o.texcoord.xy = TRANSFORM_TEX(v.texcoord,_MainTex);
					//o.texcoord.zw = o.texcoord.xy;
					//o.texcoord += _TexOffset;
					//o.texcoord1 = TRANSFORM_TEX(v.texcoord, _MainTex) + _TexAnim*_Time.yy;
					_TexOffset = _TexOffset + _TexAnim*_Time.yyyy;
					
					return o;
				}
				
				fixed4 frag (v2f i) : COLOR
				{

					half4 prev = i.color * tex2D(_MainTex, i.texcoord);
					prev *= tex2D(_TexMap, i.texcoord.xy + _TexOffset.xy).a +  tex2D(_TexMap, i.texcoord.xy + _TexOffset.zw).a;
					//prev *= tex2D(_TexMap, i.texcoord.xy).a + tex2D(_TexMap, i.texcoord.zw).a;
					prev.rgb *= prev.a;
					return prev;
				}
				ENDCG 
			}
		} 	
	}
}
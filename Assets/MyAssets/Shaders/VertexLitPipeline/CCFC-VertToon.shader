Shader "CCFC/VertexLit/CCFC-VertToon"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_AircraftCol ("Aircraft Color", Color) = (0, 1, 0, 1)
		_ShadingSteps("Shading Steps", float) = 4
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			#pragma target 2.0
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;

			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float nl : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _ShadingSteps;
			fixed4 _AircraftCol;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				float3 worldNormal = normalize(UnityObjectToWorldNormal(v.normal));
				float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
				// NdotL calculation (Vertex level)
				o.nl =(dot(worldNormal, lightDir) + 1.0f)*0.5f;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
			col.rgb = lerp(_AircraftCol.rgb, col.rgb, col.a);
				//Graduate and apply our lighting
				float shading = (floor(i.nl * _ShadingSteps + 0.5f) / _ShadingSteps);
				col.rgb = col.rgb * shading;

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				//return fixed4(shading, shading, shading, 1.0);
				
				return col;
			}
			ENDCG
		}
	}
}

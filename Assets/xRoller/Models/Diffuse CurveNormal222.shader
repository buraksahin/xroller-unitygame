Shader "Infinite Runner/Diffuse CurveNormal222" {
	Properties {
		_MainTex ("Main Texture", 2D) = "black" {}
		_FadeOutColorFirst ("Fade Out Color First", Color) = (0, 0, 0, 0)  //erste Farbe
		_FadeOutColor ("Fade Out Color", Color) = (0, 0, 0, 0)
		_NearCurve ("Near Curve", Vector) = (0, 0, 0, 0)
		_FarCurve ("Far Curve", Vector) = (0, 0, 0, 0)
		_Dist ("Distance Mod", Float) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
        Pass { // pass 0

			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog             
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma multi_compile_fwdbase LIGHTMAP_OFF LIGHTMAP_ON
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc" // for _LightColor0
			#ifndef SHADOWS_OFF		
			#include "AutoLight.cginc"	
			#endif
						
			uniform sampler2D _MainTex;
			uniform half4 _MainTex_ST;
			uniform half4 	_MainTex_TexelSize;
			// uniform sampler2D unity_Lightmap;
			// uniform half4 unity_LightmapST;
			uniform fixed4 _FadeOutColor;
			uniform fixed4 _FadeOutColorFirst;
			uniform fixed4 _NearCurve;
			uniform fixed4 _FarCurve;
			uniform fixed _Dist;

			//uniform float4 _LightColor0;
			
			struct v2f 
			{
				fixed4 diff : COLOR0;
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				half2 uvLM : TEXCOORD1;
				half distanceSquared : TEXCOORD2;
				//#ifndef SHADOWS_OFF
		        LIGHTING_COORDS(3,4)
				//#endif
               //Used to pass fog amount around number should be a free texcoord.
                UNITY_FOG_COORDS(5)
			};
						
			v2f vert(appdata_full v)
			{
				v2f o;

				// Apply the curve
                float4 pos = mul(UNITY_MATRIX_MV, v.vertex); 
                o.distanceSquared = pos.z * pos.z * _Dist;
                pos.x += (_NearCurve.x - max(1.0 - o.distanceSquared / _FarCurve.x, 0.0) * _NearCurve.x);
                pos.y += (_NearCurve.y - max(1.0 - o.distanceSquared / _FarCurve.y, 0.0) * _NearCurve.y);
                o.pos = mul(UNITY_MATRIX_P, pos); 
				o.uv = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				o.uvLM = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				
				//neu
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));  //unnötig ?
				o.diff = nl*_LightColor0;										//unnötig ?
				
                // only evaluate ambient
                //o.diff.rgb = ShadeSH9(half4(worldNormal,0.1)); //(worldNormal,1)   je niedriger 2.Parameter -> desto mehr Detail
                //o.diff.rgb *=4; 
                o.diff.rgb += ShadeSH9(half4(worldNormal,0.5f));
                o.diff.a = 0.1f;  //1

				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1.0-o.uv.y;
				#endif
				
				#ifndef SHADOWS_OFF			  	
      			TRANSFER_VERTEX_TO_FRAGMENT(o);
				#endif


                //Compute fog amount from clip space position.
                UNITY_TRANSFER_FOG(o,o.pos);

				return o;
			}
			
			fixed4 frag(v2f i) : COLOR
			{

                fixed4 col = tex2D(_MainTex, i.uv) *_FadeOutColorFirst;     
				col *= i.diff;     //heller -> FadeOut nach hinten wirkt transparent



                //Apply fog to color (additive pass are automatically handled)
                UNITY_APPLY_FOG(i.fogCoord, col); 


                //col *= i.diff*4;     //heller
                return col;
			}
			
			ENDCG
        } // end pass
	} 
	//FallBack "Diffuse"
}
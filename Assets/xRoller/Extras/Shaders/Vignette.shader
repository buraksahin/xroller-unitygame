Shader "SkyboxPlus/Vignette" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
	_VignettePower("VignettePower", Range(0.0,6.0)) = 5.5
	}
		SubShader
	{
		Pass
	{

CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"

	uniform sampler2D _MainTex;
	uniform float _VignettePower;

	struct v2f
	{
		float2 texcoord : TEXCOORD0;
	};

	float4 frag(v2f_img i) : COLOR
	{
		float4 renderTex = tex2D(_MainTex, i.uv);
		float2 dist = (i.uv - 0.5f) * 1.25f;
		dist.x = 1 - dot(dist, dist)  * _VignettePower;
		renderTex *= dist.x;
		return renderTex;

	}

		ENDCG
	}
	}
}
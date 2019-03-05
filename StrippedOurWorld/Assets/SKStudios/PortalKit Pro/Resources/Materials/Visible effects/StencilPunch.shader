﻿Shader "Custom/StencilPunch" {
	Properties{
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		ZWrite Off
		ColorMask 0


Stencil{
		Ref 4
		Comp always
		Pass replace
	}
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0

		sampler2D _MainTex;

	struct Input {
		float2 uv_MainTex;
	};

	void surf(Input IN, inout SurfaceOutputStandard o) {
		
	}
	ENDCG
	}
		FallBack "Diffuse"
}
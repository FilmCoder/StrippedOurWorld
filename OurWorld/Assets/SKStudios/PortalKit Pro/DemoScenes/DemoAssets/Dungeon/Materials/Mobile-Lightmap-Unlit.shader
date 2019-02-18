// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit shader. Simplest possible textured shader.
// - SUPPORTS lightmap
// - no lighting
// - no per-material color

Shader "Mobile/Unlit Cutout (Supports Lightmap)" {
	Properties{
		_MainTex("Base (RGBA)", 2D) = "white" {}
	}

		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		// Non-lightmapped
		Pass{
		Tags{ "LightMode" = "Vertex" }
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
		// make fog work
#pragma multi_compile_fog

#include "UnityCG.cginc"

		struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		UNITY_FOG_COORDS(1)
			float4 vertex : SV_POSITION;
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		UNITY_TRANSFER_FOG(o,o.vertex);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		// sample the texture
		fixed4 col = tex2D(_MainTex, i.uv);
		if(col.a < 0.5)
			discard;
	// apply fog
	UNITY_APPLY_FOG(i.fogCoord, col);
	return col;
	}
		ENDCG
	}

		// Lightmapped
		Pass
	{
		Tags{ "LIGHTMODE" = "VertexLM" "RenderType" = "Opaque" }

		CGPROGRAM

#pragma vertex vert
#pragma fragment frag
#pragma target 2.0
#include "UnityCG.cginc"
#pragma multi_compile_fog
#define USING_FOG (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))

		// uniforms
		float4 _MainTex_ST;

	// vertex shader input data
	struct appdata
	{
		float3 pos : POSITION;
		float3 uv1 : TEXCOORD1;
		float3 uv0 : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	// vertex-to-fragment interpolators
	struct v2f
	{
		float2 uv0 : TEXCOORD0;
		float2 uv1 : TEXCOORD1;
#if USING_FOG
		fixed fog : TEXCOORD2;
#endif
		float4 pos : SV_POSITION;
		UNITY_VERTEX_OUTPUT_STEREO
	};

	// vertex shader
	v2f vert(appdata IN)
	{
		v2f o;
		UNITY_SETUP_INSTANCE_ID(IN);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

		// compute texture coordinates
		o.uv0 = IN.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
		o.uv1 = IN.uv0.xy * _MainTex_ST.xy + _MainTex_ST.zw;

		// fog
#if USING_FOG
		float3 eyePos = UnityObjectToViewPos(float4(IN.pos, 1));
		float fogCoord = length(eyePos.xyz);  // radial fog distance
		UNITY_CALC_FOG_FACTOR_RAW(fogCoord);
		o.fog = saturate(unityFogFactor);
#endif

		// transform position
		o.pos = UnityObjectToClipPos(IN.pos);
		return o;
	}

	// textures
	sampler2D _MainTex;

	// fragment shader
	fixed4 frag(v2f IN) : SV_Target
	{
		fixed4 col, tex;

	// Fetch lightmap
	half4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, IN.uv0.xy);
	col.rgb = DecodeLightmap(bakedColorTex);

	// Fetch color texture
	tex = tex2D(_MainTex, IN.uv1.xy);
	if(tex.a < 0.5)
			discard;
	col.rgb = tex.rgb * col.rgb;
	col.a = 1;

	return col;
	}

		ENDCG
	}
	}
}

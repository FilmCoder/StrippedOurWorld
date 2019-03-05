// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/PlaceholderShader"
{
	Properties
	{
		_Cube("Cubemap", CUBE) = "" {}
		_Alpha("Alpha", Float) = 1
		//_EnvBoxStart("Env Box Start", Vector) = (0, 0, 0)
		_EnvBoxSize("Env Box Size", Vector) = (5, 5, 5)
		_EnvBoxPos("Env Box Pos", Vector) = (0, 0, 0)
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 100

			//env map parameters

//Pass
//{
	CGPROGRAM

	#include "UnityCG.cginc"
	#include "Assets/SKStudios/Common/Resources/Materials/Visible Effects/CustomRenderer.cginc"
	#pragma surface surf Lambert

	samplerCUBE _Cube;
	//float _Alpha;

	struct Input {
		float2 uv_MainTex;
		float3 worldRefl;
		float3 worldPos;
		float3 worldNormal;
	};

	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float3 objectOrigin : TEXCOORD1;
		float4 vertex : SV_POSITION;
	};


	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		o.objectOrigin = mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0));
		return o;
	}

	void surf(Input IN, inout SurfaceOutput o)
	{
		//fixed3 normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		//float3 rVec = bpcem(IN.worldRefl, EnvBoxMax, EnvBoxMin, EnvBoxPos, IN.worldPos);
		//o.Emission = texCUBE(_Cube, IN.worldRefl * 4).rgb;
		float3 rVec = bpcem(IN.worldPos, IN.worldNormal, float3(0, 0, 0), _EnvBoxPos);

		o.Emission = texCUBE(_Cube, rVec).rgb;
		o.Alpha = _Alpha;
	}
	ENDCG
		//}
		}
}

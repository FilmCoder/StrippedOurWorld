// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/DepthToAlpha"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _CameraDepthNormalsTexture;
			uniform sampler2D _EffectTex;			//Texture rendered by effect camera
			uniform float4x4 _EffectCW; 			//CameraToWorldMatrix of effect camera
			uniform float4x4 _DestinationVP;		//VP matrix of the final rendering camera
			uniform float4x4 _EffectTransform;		//Matrix transforming from effect cam to main cam
			
			struct appdata
			{
				float4 vertex 	: POSITION;
				float2 uv 		: TEXCOORD1;
			};
			
			struct v2f {
				float4 pos 		: SV_POSITION;
				float4 scrPos	: TEXCOORD0;
				float2 uv		: TEXCOORD1;
				float4 vertex	: TEXCOORD2;
				float3 wdir		: TEXCOORD3;
			};

			v2f vert(appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);	
				o.scrPos = ComputeScreenPos(o.pos);			
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				float4 clip = float4(o.vertex.xy, 0.0, 1.0);
				o.wdir = (mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos);
				return o;
			}

			//Calculate world coords given screen UV and depth info
			float4 WorldSpacePositionFromDepth(float depth, float2 uv){
    			float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
		        float3 vpos = float3((uv * 2 - 1) / p11_22, -1) * depth;
		        float4 wpos = mul(_EffectCW, float4(vpos, 1));
				wpos.xyz += float4(_WorldSpaceCameraPos, 0) / _ProjectionParams.z;
				wpos -= float4(_WorldSpaceCameraPos, 0);
				//Properly scale the output
				return wpos * _ProjectionParams.z;
			}

			//This likely needs to be updated at some point to account for UNITY_REVERSED_Z
			fixed4 frag (v2f i) : SV_Target
			{
				half3 normal;
				float depth;
				DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, i.uv), depth, normal);
				normal = mul((float3x3)_EffectCW, normal);
				//Get world space position from depth texture
				float4 wpos = WorldSpacePositionFromDepth(depth, i.uv);
				wpos = mul(_EffectTransform, float4(wpos.xyz, 1));
				float4 cpos = mul(_DestinationVP, float4(wpos.xyz, 1));
				cpos.xyz /= cpos.w;
				half4 col = half4(tex2D(_EffectTex, i.uv).rgb, cpos.z);
				return col;
			}
			ENDCG
		}
	}
}


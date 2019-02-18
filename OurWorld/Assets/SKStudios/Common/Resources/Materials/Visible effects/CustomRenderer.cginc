#pragma shader_feature _ VR
#pragma multi_compile_fog
//#pragma multi_compile_fwdbase
#include "UnityPBSLighting.cginc"
//Normal map info
uniform sampler2D _BumpMap;
uniform half4 _BumpMap_ST;
uniform float _BumpScale;

uniform float _CustomRenderInFront;
uniform float _PerfectReflection;

uniform float _YFlipOverride;
uniform float _XFlipOverride;

uniform float _Forward = 0;

//Mask texture, used for cutout effects
uniform sampler2D _AlphaTexture;
uniform float _Alpha = 1;
uniform float _Mask = 1;


//Textures for both eyes. If non-stereo rendering is being employed, only the right channel is used.
sampler2D _LeftEyeTexture;
sampler2D _RightEyeTexture;

//Temporary
uniform sampler2D _WorldPosTex;

uniform float4 _LeftEyeTexture_TexelSize;
uniform float4 _RightEyeTexture_TexelSize;

//Offsets for both eyes. This defines where, in normalized screen-space coordinates, the Custom Renderer will actually render.
//X and Y are used for positioning, while Z and W define the size of the renderer.
uniform float4 _LeftDrawPos;
uniform float4 _RightDrawPos;

uniform float _VR;

uniform float4x4 _InverseReflectionMatrix;
uniform sampler2D _CameraDepthTexture;
struct SK_Extended_SurfaceOutputStandardSpecular
{
	fixed3 Albedo;      // diffuse color
	fixed3 Specular;    // specular color
	float3 Normal;      // tangent space normal, if written
	half3 Emission;
	half Smoothness;    // 0=rough, 1=smooth
	half Occlusion;     // occlusion (default 1)
	fixed Alpha;        // alpha for transparencies
	fixed CustomRenderAlpha;
};

inline half4 LightingCustom(SK_Extended_SurfaceOutputStandardSpecular s, float3 viewDir, UnityGI gi)
{
	s.Normal = normalize(s.Normal);

	// energy conservation
	half oneMinusReflectivity;
	s.Albedo = EnergyConservationBetweenDiffuseAndSpecular(s.Albedo, s.Specular, /*out*/ oneMinusReflectivity);

	// shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
	// this is necessary to handle transparency in physically correct way - only diffuse component gets affected by alpha
	half outputAlpha;
	s.Albedo = PreMultiplyAlpha(s.Albedo, s.Alpha, oneMinusReflectivity, /*out*/ outputAlpha);

	half4 c = UNITY_BRDF_PBS(s.Albedo, s.Specular, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);
	c.a = outputAlpha;

	float alphaScalar = 1;//1 - s.Specular;
	return c * alphaScalar;
}

inline half4 LightingCustom_Deferred(SK_Extended_SurfaceOutputStandardSpecular s, float3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2)
{
	// energy conservation
	half oneMinusReflectivity;
	s.Albedo = EnergyConservationBetweenDiffuseAndSpecular(s.Albedo, s.Specular, /*out*/ oneMinusReflectivity);

	half4 c = UNITY_BRDF_PBS(s.Albedo, s.Specular, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);

	UnityStandardData data;
	data.diffuseColor = s.Albedo;
	data.occlusion = s.Occlusion;
	data.specularColor = s.Specular;
	data.smoothness = s.Smoothness;
	data.normalWorld = s.Normal;

	UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

	half4 emission = half4(s.Emission + c.rgb, 1);
	float alphaScalar = 1;//s.Alpha;
	return emission * alphaScalar;
}

inline void LightingCustom_GI(
	SK_Extended_SurfaceOutputStandardSpecular s,
	UnityGIInput data,
	inout UnityGI gi)
{
#if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
	gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
#else
	Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.Normal, s.Specular);
	gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal, g);
#endif

	float alphaScalar = 1;//s.Alpha;

	gi.light.color *= alphaScalar;
#ifdef DIRLIGHTMAP_SEPARATE
#ifdef LIGHTMAP_ON
	gi.light2.color *= alphaScalar;
#endif
#ifdef DYNAMICLIGHTMAP_ON
	gi.light3.color *= alphaScalar;
#endif
#endif

	
}

float2 GetEyeCoords(float2 sUV, float2 normal){
	float4 eyeCoords = _VR && unity_StereoEyeIndex == 0 ? _LeftDrawPos : _RightDrawPos;
	#if UNITY_SINGLE_PASS_STEREO
		if (_VR) {
			//In single pass stereo rendering, the screenspace coordinates must be divided in half to account for the doublewidth render.
			eyeCoords /= float4(2, 1, 2, 1);
			//Likewise, the right eye must be shifted to the right by half the width of the final render so that it is on the right part of the screen.
			if(unity_StereoEyeIndex != 0){
				eyeCoords += 0.5;
			}
		}
	#endif
	//Fix the coordinates so that they are accurate to screenspace.... space.
	eyeCoords.xy = eyeCoords.xy / eyeCoords.zw;

	if (_YFlipOverride)
		sUV.y = 1 - sUV.y;

	#if NormalMap
		//Determine output distortion from normal map
		sUV.x -= (normal.r /** (1 - normal.b)*/) / 10 * _BumpScale;
		sUV.y += (normal.g /** (1 - normal.b)*/) / 10 * _BumpScale;
	#endif

	return (sUV / eyeCoords.zw) - eyeCoords.xy;
}


float4 GetDepth(float4 sUV, float3 normal/*, float inDepth, out float depth*/){
	float2 uv = GetEyeCoords(sUV, normal);
	float4 col =  tex2D(_RightEyeTexture, uv);
	//Correct the worldspace position to account for the fact that the data is coming in transformed.
	float4 wpos = mul(_InverseReflectionMatrix, float4(col.xyz, 1));	
	//Get the depth of the screen
	float screenDepth = tex2D(_CameraDepthTexture, sUV.xy);
	//Convert from world to object space
	float4 opos = mul(unity_WorldToObject, float4(wpos.xyz, 1));
	//Convert from object to clip space
	float4 cpos = UnityObjectToClipPos(opos.xyz);
	//Perspective divide
	cpos.xyz /= cpos.w;
	//depth = cpos.z; 
	return float4(wpos.xyz, 1);
}

//Returns the result of the Custom Renderer portion. 
float4 CustomRenderResult(float2 alphaUV, float2 sUV, float3 normal/*, out float depth*/) {
	
	fixed4 alpha = tex2D(_AlphaTexture, alphaUV);

	if (!_Mask) {
		alpha = 0;
	}

//#if 0
	#if UNITY_SINGLE_PASS_STEREO
		if (_VR) {
			//In single pass stereo rendering, the screenspace coordinates must be divided in half to account for the doublewidth render.
			_LeftDrawPos /= float4(2, 1, 2, 1);
			_RightDrawPos /= float4(2, 1, 2, 1);
			//Likewise, the right eye must be shifted to the right by half the width of the final render so that it is on the right part of the screen.
			_RightDrawPos.x += 0.5;
		}
	#endif

//#endif
	//Fix the coordinates so that they are accurate to screenspace.... space.
//#if 0
	_LeftDrawPos.xy /= _LeftDrawPos.zw;
	_RightDrawPos.xy /= _RightDrawPos.zw;
//#endif

	//On some systems, for some reason Unity fails to accurately detect if the UV is flipped. This override patches that.
//#if 0
	if (_YFlipOverride)
		sUV.y = 1 - sUV.y;
//#endif

	//Init the output
	fixed4 col = fixed4(0.0, 0.0, 0.0, 0.0);
//#if 0
	//#if NormalMap
	// sUV.x -= (normal.r * (1 - normal.b)) / 10 * _BumpScale;
		//Determine output distortion from normal map
		sUV.x -= (normal.r) / 10 * _BumpScale;
		sUV.y += (normal.g ) / 10 * _BumpScale;
	//#endif
//#endif




//#if 0
	#if UNITY_SINGLE_PASS_STEREO
		if (_VR) {
			if (unity_StereoEyeIndex == 0)
			{
				sUV = (sUV / _LeftDrawPos.zw) - _LeftDrawPos.xy;
				col = tex2D(_LeftEyeTexture, sUV);
			}
			else {

				sUV = (sUV / _RightDrawPos.zw) - _RightDrawPos.xy;
				col = tex2D(_RightEyeTexture, sUV);
			}
		}
		else {
			sUV = (sUV / _RightDrawPos.zw) - _RightDrawPos.xy;
			col = tex2D(_RightEyeTexture, sUV);
		}

	#else
		if (_VR) {
			if (unity_StereoEyeIndex == 0)
			{
				sUV = (sUV / _LeftDrawPos.zw) - _LeftDrawPos.xy;
				col = tex2D(_LeftEyeTexture, sUV);
			//	col = _LeftDrawPos;
			}
			else
			{
				sUV = (sUV / _RightDrawPos.zw) - _RightDrawPos.xy;
				col = tex2D(_RightEyeTexture, sUV);
			//	col = _RightDrawPos;
			}
		}
		else {
			sUV = (sUV / _RightDrawPos.zw) - _RightDrawPos.xy;
			col = tex2D(_RightEyeTexture, sUV);
		}
		//col = half4(sUV, 1, 1);
	#endif
	//depth = 1;
//#endif
/*
	float2 uv = GetEyeCoords(sUV, normal);
	float4 col = 0;
	if (_VR && unity_StereoEyeIndex) 
		col = tex2D(_LeftEyeTexture, uv);
	else
		col = tex2D(_RightEyeTexture, uv);
		*/
	//Alpha from mask, multiplied by alpha value
	//depth = col.a;
		
		col.a = max(0, (1 - alpha.r));
	//col.a = 0;
		//col.a = 1;

		//col = float4(sUV.x, sUV.y, 1, 1);
	return col;
}

const fixed4 _EnvBoxPos;
const fixed4 _EnvBoxSize;

//World vertex pos, world normal, local normal, and origin of the object
float3 bpcem(float3 worldPos, float3 worldNormal, float3 normal, float3 objectOrigin) {
	float3 dir = worldPos - _WorldSpaceCameraPos;                            // pos fragment relativo a pos camera
	float3 worldNorm = worldNormal;
	worldNorm.xy -= normal;
	float3 rdir = reflect(dir, worldNorm);                             // vettore riflesso da normale
	float3 opos = (objectOrigin - (_EnvBoxSize / 2));
	//BPCEM
	float3 nrdir = normalize(rdir);                                            // vettore riflesso normalizzato
	//float3 rbmax = (_EnvBoxStart + _EnvBoxSize - IN.worldPos) / nrdir;            // AABB max value +...
	float3 rbmax = (opos + _EnvBoxSize - worldPos) / nrdir;
	//float3 rbmin = (_EnvBoxStart - IN.worldPos) / nrdir;                          // AABB min value +...
	float3 rbmin = (opos - worldPos) / nrdir;
	float3 rbminmax = (nrdir > 0.0f) ? rbmax : rbmin;                                 // ...?
	float fa = min(min(rbminmax.x, rbminmax.y), rbminmax.z);                    // ...?
	float3 posonbox = worldPos + nrdir * fa;                                   // ...?
	//rdir = posonbox - (_EnvBoxStart + _EnvBoxSize / 2);                            // ...? - posizione (centro) del box
	rdir = posonbox - (opos + _EnvBoxSize / 2);
	//PBCEM end
	return rdir;
}
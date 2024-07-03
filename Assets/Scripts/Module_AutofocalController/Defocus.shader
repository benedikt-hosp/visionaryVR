Shader "Hidden/Defocus"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "black" {}
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	sampler2D _MainTex, _CameraDepthTexture, _defocusTexture, _blurredTex;
	float4 _MainTex_TexelSize; // Vector4(1 / width, 1 / height, width, height), with and height in px
	// maybe also useful: unity_CameraWorldClipPlanes[6]
	float _OpticalPower, _CocConstant, _BokehRadius;
	int _downscaleFactor;
	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
	};

	v2f vert (appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
	}
	ENDCG


	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass // 0 coc pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			half frag (v2f i) : SV_Target
			{   
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);                
				//depth = LinearEyeDepth(depth);
				
				depth = Linear01Depth(depth);
				float4 viewDir = mul (unity_CameraInvProjection, float4 (i.uv * 2.0 - 1.0, 1.0, 1.0));
				float3 viewPos = (viewDir.xyz / viewDir.w) * depth;
				depth = length(viewPos);
				
				float defocus = (_OpticalPower*depth - 1) /depth;
				return abs(defocus);
			}
			ENDCG
		}
		Pass // 1 pre filter pass
		// downscaling of coc texture
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			half4 frag (v2f i) : SV_Target
			{
				// downsample defocus values
				float4 o = _MainTex_TexelSize.xyxy * float2(-0.5, 0.5).xxyy;
				half coc0 = tex2D(_defocusTexture, i.uv + o.xy).r;
				half coc1 = tex2D(_defocusTexture, i.uv + o.zy).r;
				half coc2 = tex2D(_defocusTexture, i.uv + o.xw).r;
				half coc3 = tex2D(_defocusTexture, i.uv + o.zw).r;
				//TODO texture gathering?
				//half coc = (coc0 + coc1 + coc2 + coc3) * 0.25; //mean of all coc values 
				// use max(abs(coc)) value:
				half cocMin = min(min(min(coc0, coc1), coc2), coc3);
				half cocMax = max(max(max(coc0, coc1), coc2), coc3);
				half coc = cocMax >= -cocMin ? cocMax : cocMin;
				coc*= _CocConstant; // from defocus to coc in texels (TODO: does it work correctly?)
				return half4(tex2D(_MainTex, i.uv).rgb, coc);
			}
			ENDCG
		}
		Pass // 2 convolution pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#define BOKEH_KERNEL_XLARGE
			#if defined(BOKEH_KERNEL_MEDIUM)
				static const int kernelSampleCount = 22;
				static const float2 kernel[kernelSampleCount] = {
					float2(0, 0),
					float2(0.53333336, 0),
					float2(0.3325279, 0.4169768),
					float2(-0.11867785, 0.5199616),
					float2(-0.48051673, 0.2314047),
					float2(-0.48051673, -0.23140468),
					float2(-0.11867763, -0.51996166),
					float2(0.33252785, -0.4169769),
					float2(1, 0),
					float2(0.90096885, 0.43388376),
					float2(0.6234898, 0.7818315),
					float2(0.22252098, 0.9749279),
					float2(-0.22252095, 0.9749279),
					float2(-0.62349, 0.7818314),
					float2(-0.90096885, 0.43388382),
					float2(-1, 0),
					float2(-0.90096885, -0.43388376),
					float2(-0.6234896, -0.7818316),
					float2(-0.22252055, -0.974928),
					float2(0.2225215, -0.9749278),
					float2(0.6234897, -0.7818316),
					float2(0.90096885, -0.43388376),
				};
			#endif
			#if defined(BOKEH_KERNEL_LARGE)
				static const int kernelSampleCount = 71;
				static const float2 kernel[kernelSampleCount] = {
					float2(0,0),
					float2(0.2758621,0),
					float2(0.1719972,0.21567768),
					float2(-0.061385095,0.26894566),
					float2(-0.24854316,0.1196921),
					float2(-0.24854316,-0.11969208),
					float2(-0.061384983,-0.2689457),
					float2(0.17199717,-0.21567771),
					float2(0.51724136,0),
					float2(0.46601835,0.22442262),
					float2(0.32249472,0.40439558),
					float2(0.11509705,0.50427306),
					float2(-0.11509704,0.50427306),
					float2(-0.3224948,0.40439552),
					float2(-0.46601835,0.22442265),
					float2(-0.51724136,0),
					float2(-0.46601835,-0.22442262),
					float2(-0.32249463,-0.40439564),
					float2(-0.11509683,-0.5042731),
					float2(0.11509732,-0.504273),
					float2(0.32249466,-0.40439564),
					float2(0.46601835,-0.22442262),
					float2(0.7586207,0),
					float2(0.7249173,0.22360738),
					float2(0.6268018,0.4273463),
					float2(0.47299224,0.59311354),
					float2(0.27715522,0.7061801),
					float2(0.056691725,0.75649947),
					float2(-0.168809,0.7396005),
					float2(-0.3793104,0.65698475),
					float2(-0.55610836,0.51599306),
					float2(-0.6834936,0.32915324),
					float2(-0.7501475,0.113066405),
					float2(-0.7501475,-0.11306671),
					float2(-0.6834936,-0.32915318),
					float2(-0.5561083,-0.5159932),
					float2(-0.37931028,-0.6569848),
					float2(-0.16880904,-0.7396005),
					float2(0.056691945,-0.7564994),
					float2(0.2771556,-0.7061799),
					float2(0.47299215,-0.59311366),
					float2(0.62680185,-0.4273462),
					float2(0.72491735,-0.22360711),
					float2(1,0),
					float2(0.9749279,0.22252093),
					float2(0.90096885,0.43388376),
					float2(0.7818315,0.6234898),
					float2(0.6234898,0.7818315),
					float2(0.43388364,0.9009689),
					float2(0.22252098,0.9749279),
					float2(0,1),
					float2(-0.22252095,0.9749279),
					float2(-0.43388385,0.90096885),
					float2(-0.62349,0.7818314),
					float2(-0.7818317,0.62348956),
					float2(-0.90096885,0.43388382),
					float2(-0.9749279,0.22252093),
					float2(-1,0),
					float2(-0.9749279,-0.22252087),
					float2(-0.90096885,-0.43388376),
					float2(-0.7818314,-0.6234899),
					float2(-0.6234896,-0.7818316),
					float2(-0.43388346,-0.900969),
					float2(-0.22252055,-0.974928),
					float2(0,-1),
					float2(0.2225215,-0.9749278),
					float2(0.4338835,-0.90096897),
					float2(0.6234897,-0.7818316),
					float2(0.78183144,-0.62348986),
					float2(0.90096885,-0.43388376),
					float2(0.9749279,-0.22252086),
				};
			#endif
			#if defined(BOKEH_KERNEL_XLARGE)
				static const int kernelSampleCount = 132;
				static const float2 kernel[kernelSampleCount] = {
					float2(0,0),
					float2(0.16666667,0),
					float2(0.083333333,0.14433757),
					float2(-0.083333333,0.14433757),
					float2(-0.16666667,0),
					float2(-0.083333333,-0.14433757),
					float2(0.083333333,-0.14433757),
					float2(0.33333333,0),
					float2(0.28867513,0.16666667),
					float2(0.16666667,0.28867513),
					float2(0,0.33333333),
					float2(-0.16666667,0.28867513),
					float2(-0.28867513,0.16666667),
					float2(-0.33333333,0),
					float2(-0.28867513,-0.16666667),
					float2(-0.16666667,-0.28867513),
					float2(0,-0.33333333),
					float2(0.16666667,-0.28867513),
					float2(0.28867513,-0.16666667),
					float2(0.5,0),
					float2(0.47290862,0.16234973),
					float2(0.39457025,0.30710636),
					float2(0.27347408,0.41858324),
					float2(0.12274274,0.48470013),
					float2(-0.041289673,0.49829225),
					float2(-0.20084771,0.45788666),
					float2(-0.33864079,0.36786196),
					float2(-0.43973688,0.2379737),
					float2(-0.49318065,0.082297295),
					float2(-0.49318065,-0.082297295),
					float2(-0.43973688,-0.2379737),
					float2(-0.33864079,-0.36786196),
					float2(-0.20084771,-0.45788666),
					float2(-0.041289673,-0.49829225),
					float2(0.12274274,-0.48470013),
					float2(0.27347408,-0.41858324),
					float2(0.39457025,-0.30710636),
					float2(0.47290862,-0.16234973),
					float2(0.66666667,0),
					float2(0.64572211,0.16579326),
					float2(0.58420445,0.32116912),
					float2(0.48597908,0.45636474),
					float2(0.35721786,0.56288528),
					float2(0.20601133,0.63403768),
					float2(0.041860346,0.66535115),
					float2(-0.12492088,0.65485817),
					float2(-0.28385286,0.60321803),
					float2(-0.42494933,0.5136755),
					float2(-0.53934466,0.39185683),
					float2(-0.61985099,0.24541637),
					float2(-0.6614098,0.083555489),
					float2(-0.6614098,-0.083555489),
					float2(-0.61985099,-0.24541637),
					float2(-0.53934466,-0.39185683),
					float2(-0.42494933,-0.5136755),
					float2(-0.28385286,-0.60321803),
					float2(-0.12492088,-0.65485817),
					float2(0.041860346,-0.66535115),
					float2(0.20601133,-0.63403768),
					float2(0.35721786,-0.56288528),
					float2(0.48597908,-0.45636474),
					float2(0.58420445,-0.32116912),
					float2(0.64572211,-0.16579326),
					float2(0.83333333,0),
					float2(0.81627495,0.16774877),
					float2(0.76579818,0.32862988),
					float2(0.68396953,0.47605685),
					float2(0.5741391,0.60399399),
					float2(0.44080334,0.70720355),
					float2(0.28942104,0.78146011),
					float2(0.12618981,0.8237236),
					float2(-0.042207641,0.83226376),
					float2(-0.20887711,0.80673093),
					float2(-0.36699513,0.74817045),
					float2(-0.51008832,0.65897978),
					float2(-0.63229844,0.5428104),
					float2(-0.72862218,0.4044183),
					float2(-0.79511605,0.24946927),
					float2(-0.82905777,0.084306935),
					float2(-0.82905777,-0.084306935),
					float2(-0.79511605,-0.24946927),
					float2(-0.72862218,-0.4044183),
					float2(-0.63229844,-0.5428104),
					float2(-0.51008832,-0.65897978),
					float2(-0.36699513,-0.74817045),
					float2(-0.20887711,-0.80673093),
					float2(-0.042207641,-0.83226376),
					float2(0.12618981,-0.8237236),
					float2(0.28942104,-0.78146011),
					float2(0.44080334,-0.70720355),
					float2(0.5741391,-0.60399399),
					float2(0.68396953,-0.47605685),
					float2(0.76579818,-0.32862988),
					float2(0.81627495,-0.16774877),
					float2(1,0),
					float2(0.9863613,0.16459459),
					float2(0.94581724,0.32469947),
					float2(0.87947375,0.47594739),
					float2(0.78914051,0.61421271),
					float2(0.67728157,0.73572391),
					float2(0.54694816,0.83716648),
					float2(0.40169542,0.91577333),
					float2(0.24548549,0.96940027),
					float2(0.082579345,0.99658449),
					float2(-0.082579345,0.99658449),
					float2(-0.24548549,0.96940027),
					float2(-0.40169542,0.91577333),
					float2(-0.54694816,0.83716648),
					float2(-0.67728157,0.73572391),
					float2(-0.78914051,0.61421271),
					float2(-0.87947375,0.47594739),
					float2(-0.94581724,0.32469947),
					float2(-0.9863613,0.16459459),
					float2(-1,0),
					float2(-0.9863613,-0.16459459),
					float2(-0.94581724,-0.32469947),
					float2(-0.87947375,-0.47594739),
					float2(-0.78914051,-0.61421271),
					float2(-0.67728157,-0.73572391),
					float2(-0.54694816,-0.83716648),
					float2(-0.40169542,-0.91577333),
					float2(-0.24548549,-0.96940027),
					float2(-0.082579345,-0.99658449),
					float2(0.082579345,-0.99658449),
					float2(0.24548549,-0.96940027),
					float2(0.40169542,-0.91577333),
					float2(0.54694816,-0.83716648),
					float2(0.67728157,-0.73572391),
					float2(0.78914051,-0.61421271),
					float2(0.87947375,-0.47594739),
					float2(0.94581724,-0.32469947),
					float2(0.9863613,-0.16459459),
				};
			#endif

			half Weigh (half coc, half radius) {
				// test:
				//if (coc > radius)
				//{
					//	return half(1.0);
				//}
				//else
				//{
					//	return half(0.0);
				//}
				return saturate((coc - radius + 3) / 6); // TODO what if we change the 2 here
			}

			half4 frag (v2f i) : SV_Target
			{   
				half3 bgColor = 0, fgColor = 0;
				half bgWeight = 0, fgWeight = 0;

				//float coc = _CocConstant * tex2D(_defocusTexture, i.uv).r;
				//coc = clamp(coc, -1, 1); // clamp between -1 and 1
				half coc = tex2D(_defocusTexture, i.uv).r;
				for (int k = 0; k < kernelSampleCount; k++)
				{
					float2 o = kernel[k] * _BokehRadius;
					half radius = length(o);
					o *= _MainTex_TexelSize.xy; // from texel coords to uv coords
					half4 s = tex2D(_MainTex, i.uv + o);
					half coc_sample = tex2D(_defocusTexture, i.uv + o).r; // what exactly are we doing here
					half bgw = Weigh(max(0,min(coc_sample,coc)), radius);
					//half bgw = Weigh(max(0,min(coc_sample,coc_sample)), radius);

					bgColor += s.rgb * bgw;
					bgWeight += bgw;

					half fgw = Weigh(-coc_sample, radius);
					fgColor += s.rgb * fgw;
					fgWeight += fgw;

				}
				bgColor *= 1 / (bgWeight + (bgWeight == 0)); // devided by 1 in case Weight is 1
				fgColor *= 1 / (fgWeight + (fgWeight == 0)); 
				// combining fg and bg:
				half bgfg_interp = min(1, fgWeight * 3.14159265359 /kernelSampleCount);
				//half bgfg_interp = min(1, fgWeight /kernelSampleCount);
				half3 color = lerp(bgColor, fgColor, bgfg_interp);
				
				return half4(color, bgfg_interp); // pass bgfg_interp for usage in combine pass
			}
			ENDCG
		}
		
		Pass // 3 poster filter pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			half4 frag (v2f i) : SV_Target
			{
				float4 o = _MainTex_TexelSize.xyxy * float2(-0.5, 0.5).xxyy;
				half4 s =
				tex2D(_MainTex, i.uv + o.xy) +
				tex2D(_MainTex, i.uv + o.zy) +
				tex2D(_MainTex, i.uv + o.xw) +
				tex2D(_MainTex, i.uv + o.zw);
				return s * 0.25;
			}
			ENDCG
		}

		Pass // 4 combine pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			half4 frag (v2f i) : SV_Target
			{
				half4 source = tex2D(_MainTex, i.uv);
				half coc = tex2D(_defocusTexture, i.uv).r;
				half4 blurred = tex2D(_blurredTex, i.uv);

				half dofStrength = smoothstep(0.1, 1.0, abs(coc)); //TODO os smoothstep ok? with the 0.1?
				half3 color = lerp(
				source.rgb, blurred.rgb,
				dofStrength + blurred.a - dofStrength * blurred.a
				);
				return half4(color, source.a);
			}
			ENDCG
		}
		Pass // 5 depth pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			half4 frag (v2f i) : SV_Target
			{   
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);                
				depth = LinearEyeDepth(depth);
				//depth = Linear01Depth(depth);
				return 0.15*depth*half4(1,1,1,1);
			}
			ENDCG
		}
		Pass // 6 distance pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			half4 frag (v2f i) : SV_Target
			{   
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);                
				//depth = LinearEyeDepth(depth);
				
				depth = Linear01Depth(depth);
				float4 viewDir = mul (unity_CameraInvProjection, float4 (i.uv * 2.0 - 1.0, 1.0, 1.0));
				float3 viewPos = (viewDir.xyz / viewDir.w) * depth;
				float distance = length(viewPos);
				
				return 0.15*distance*half4(1,1,1,1);
			}
			ENDCG
		}
	}
}
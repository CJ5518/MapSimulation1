Shader "Landon/USA-TintOverlay" {
    Properties {
        _MainTex ("Base Color (rgb) Lines (a)", 2D) = "grey" {}
		[NoScaleOffset][Normal] _Bump ("Normal Map (rgb)", 2D) = "bump" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		[NoScaleOffset] _State ("State Mask(r) nothing yet (gba)", 2D) = "grey" {}

		[HDR] _Lines ("State Line Color (rgb) Intensity (a)", Color) = (0.5,0.5,0.5,1)
		
		[NoScaleOffset] _Data1 ("Data 1 (rgba)", 2D) = "grey" {}
		[NoScaleOffset] _Data2 ("Data 2 (rgba)", 2D) = "grey" {}

		[HDR] _Color1 ("Data 1 channel r Color (rgb) Intensity (a)", Color) = (0.5,0.5,0.5,1)
		[HDR] _Color2 ("Data 1 channel g Color (rgb) Intensity (a)", Color) = (0.5,0.5,0.5,1)
		[HDR] _Color3 ("Data 1 channel b Color (rgb) Intensity (a)", Color) = (0.5,0.5,0.5,1)
		[HDR] _Color4 ("Data 1 channel a Color (rgb) Intensity (a)", Color) = (0.5,0.5,0.5,0)
		[HDR] _Color5 ("Data 2 channel r Color (rgb) Intensity (a)", Color) = (0.5,0.5,0.5,0)
		[HDR] _Color6 ("Data 2 channel g Color (rgb) Intensity (a)", Color) = (0.5,0.5,0.5,0)
		[HDR] _Color7 ("Data 2 channel b Color (rgb) Intensity (a)", Color) = (0.5,0.5,0.5,0)
		[HDR] _Color8 ("Data 2 channel a Color (rgb) Intensity (a)", Color) = (0.5,0.5,0.5,0)
    }
    SubShader {
        Tags { "RenderType"="Opaque"}
        //LOD 400
		//ColorMask RGBA
        CGPROGRAM
        #pragma surface surf Standard // fullforwardshadows
        #pragma target 3.0


        sampler2D _MainTex, _Bump, _State, _Data1, _Data2;
        struct Input {
			float2 uv_MainTex;
		};
        fixed _Glossiness, _Metallic;
        fixed4 _Lines, _Color1, _Color2, _Color3, _Color4, _Color5, _Color6, _Color7, _Color8;


        void surf (Input IN, inout SurfaceOutputStandard o) {
			//sample textures
			fixed4 main = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 state = tex2D (_State, IN.uv_MainTex);
			fixed4 data1 = tex2D (_Data1, IN.uv_MainTex);
			fixed4 data2 = tex2D (_Data2, IN.uv_MainTex);
			//color mask areas
			fixed c1 = data1.r * _Color1.a;
			fixed c2 = data1.g * _Color2.a;
			fixed c3 = data1.b * _Color3.a;
			fixed c4 = data1.a * _Color4.a;
			fixed c5 = data2.r * _Color5.a;
			fixed c6 = data2.g * _Color6.a;
			fixed c7 = data2.b * _Color7.a;
			fixed c8 = data2.a * _Color8.a;
			//multiply colors
			fixed3 c1m = saturate(1-c1 + _Color1.rgb);
			fixed3 c2m = saturate(1-c2 + _Color2.rgb);
			fixed3 c3m = saturate(1-c3 + _Color3.rgb);
			fixed3 c4m = saturate(1-c4 + _Color4.rgb);
			fixed3 c5m = saturate(1-c5 + _Color5.rgb);
			fixed3 c6m = saturate(1-c6 + _Color6.rgb);
			fixed3 c7m = saturate(1-c7 + _Color7.rgb);
			fixed3 c8m = saturate(1-c8 + _Color8.rgb);
			//add colors
			fixed3 c1a = c1 * _Color1.rgb;
			fixed3 c2a = c2 * _Color2.rgb;
			fixed3 c3a = c3 * _Color3.rgb;
			fixed3 c4a = c4 * _Color4.rgb;
			fixed3 c5a = c5 * _Color5.rgb;
			fixed3 c6a = c6 * _Color6.rgb;
			fixed3 c7a = c7 * _Color7.rgb;
			fixed3 c8a = c8 * _Color8.rgb;
			//apply colors
			fixed3 mult = 1 * c1m * c2m * c3m * c4m * c5m * c6m * c7m * c8m;
			fixed3 add = c1a + c2a + c3a + c4a + c5a + c6a + c7a + c8a;
			fixed3 color = main.rgb * mult + add;
			//brighten hovered State
			color = (state.r * 0.2 + 0.8) * color + state.r * 0.05;
			//add state lines over top
			o.Albedo = lerp(color.rgb, _Lines.rgb, (1 - main.a) * _Lines.a);

			o.Normal = UnpackNormal (tex2D (_Bump, IN.uv_MainTex));
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
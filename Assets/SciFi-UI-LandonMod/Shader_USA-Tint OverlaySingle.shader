Shader "Landon/USA-TintOverlaySingle" {
    Properties {
        _MainTex ("Base Color (rgb) Lines (a)", 2D) = "grey" {}
		[NoScaleOffset][Normal] _Bump ("Normal Map (rgb)", 2D) = "bump" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		[NoScaleOffset] _State ("State Mask(r) nothing yet (gba)", 2D) = "grey" {}

		[HDR] _Lines ("State Line Color (rgb) Intensity (a)", Color) = (0.5,0.5,0.5,1)
		
		[NoScaleOffset] _Data ("Data 1 (rgba)", 2D) = "grey" {}

		[HDR] _C1 ("Data 1 channel r Color (rgb) Intensity (a)", Color) = (1,0,0,1)
		[HDR] _C2 ("Data 1 channel g Color (rgb) Intensity (a)", Color) = (0.5,0.5,0.5,0)
		[HDR] _C3 ("Data 1 channel b Color (rgb) Intensity (a)", Color) = (0.5,0.5,0.5,0)
		[HDR] _C4 ("Data 1 channel a Color (rgb) Intensity (a)", Color) = (0.5,0.5,0.5,0)
    }
    SubShader {
        Tags { "Queue" = "Geometry" "RenderType"="Opaque"}
        //LOD 400	ColorMask RGBA
        CGPROGRAM
        #pragma surface surf Standard // fullforwardshadows
        #pragma target 3.0


        sampler2D _MainTex, _Bump, _State, _Data;
        struct Input {
			float2 uv_MainTex;
		};
        fixed _Glossiness, _Metallic;
        fixed4 _Lines, _C1, _C2, _C3, _C4;


        void surf (Input IN, inout SurfaceOutputStandard o) {
			//sample textures
			fixed4 main = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 state = tex2D (_State, IN.uv_MainTex);
			fixed4 data = tex2D (_Data, IN.uv_MainTex);
			//color mask areas
			fixed c1 = data.r * _C1.a;
			fixed c2 = data.g * _C2.a;
			fixed c3 = data.b * _C3.a;
			fixed c4 = data.a * _C4.a;
			fixed colorAreas = c1 + c2 + c3 + c4;
			//add colors
			fixed3 sum = c1 * _C1.rgb + c2 * _C2.rgb + c3 * _C3.rgb + c4 * _C4.rgb;
			//apply colors
			fixed3 color = main.rgb * (1 - colorAreas) + sum;

			//brighten hovered State
			color = (state.r * 0.2 + 0.8) * color + state.r * 0.05;
			//add state lines over top
			o.Albedo = lerp(color.rgb, _Lines.rgb, (1 - main.a) * _Lines.a);

			o.Normal = UnpackNormal (tex2D (_Bump, IN.uv_MainTex));
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;


			//multiply colors  
			//fixed3 c1m = saturate(1-c1 + _C1.rgb);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
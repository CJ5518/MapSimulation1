Shader "Landon/USA-Tint" {
    Properties {
        _MainTex ("Data (rgba)", 2D) = "black" {}
		[NoScaleOffset] _SecondTex ("Data 2 (rgba)", 2D) = "black" {}
		[NoScaleOffset][Normal] _BumpMap ("Normal Map", 2D) = "bump" {}
		[NoScaleOffset] _BaseTex ("Base Texture (rgb)", 2D) = "white" {}
		[NoScaleOffset] _Details ("State Lines (r) nothing yet (gba)", 2D) = "white" {}
		[NoScaleOffset] _State ("State Mask(r) nothing yet (gba)", 2D) = "black" {}

		[HDR] _StateLines ("Lines Color (rgb) Intensity (a)", Color) = (1,1,1,1)

		[HDR] _Color1 ("_Color1 Color in first position (rgb) Intensity (a)", Color) = (1,0,0,1)		// 1001 in sim 
		[HDR] _Color2 ("_Color2 Color in second position (rgb) Intensity (a)", Color) = (0,1,0,1)		// 0101 in sim
		[HDR] _Color3 ("_Color3 Color in third position (rgb) Intensity (a)", Color) = (0,0,0,1)				// 0011 in sim
		[HDR] _Color4 ("_Color4 Color in fourth position (rgb) Intensity (a)", Color) = (0,0,0.5,1)	// 0000 in sim
		
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_TextureLerpValue ("Texture lerp value", Range(0,1)) = 0.0
    }
    SubShader {
        Tags { "RenderType"="Opaque"}
        //LOD 400
		//ColorMask RGBA


        CGPROGRAM
        #pragma surface surf Standard // fullforwardshadows
        #pragma target 3.0  // (shader model 3.0 for nicer looking lighting)


        sampler2D _MainTex, _SecondTex, _BumpMap, _BaseTex, _Details, _State;

        struct Input {
            float2 uv_MainTex;
        };

        fixed _Glossiness, _Metallic, _TextureLerpValue;
        fixed4 _StateLines, _Color1, _Color2, _Color3, _Color4;


        void surf (Input IN, inout SurfaceOutputStandard o) {
			//Well named variables
            fixed4 m = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 b = tex2D (_BaseTex, IN.uv_MainTex);
			fixed4 d = tex2D (_Details, IN.uv_MainTex);
			fixed4 secondTextureColor = tex2D(_SecondTex, IN.uv_MainTex);
			fixed4 s = tex2D (_State, IN.uv_MainTex);

			m = lerp(m.rgba, secondTextureColor.rgba, _TextureLerpValue);
			fixed data1 = m.r * _Color1.a;
			fixed data2 = m.g * _Color2.a;
			fixed data3 = m.b * _Color3.a;
			fixed data4 = m.a * _Color4.a;

			//option 1
			//First for any data being shown (use intensity) lerp baseTex to black
            fixed3 color = lerp(b.rgb, fixed3(0,0,0), saturate(data1 + data2 + data3 + data4));
			
			//option 2
			//First multiply color for shown data
			//fixed3 color = b.rgb * saturate(1-data1 + _Color1.rgb) * saturate(1-data2 + _Color2.rgb) * saturate(1-data3 + _Color3.rgb) * saturate(1-data4 + _Color4.rgb);
			

			//Extra darkness!!!! for the look I want.
			fixed3 dark = saturate(1-data1 + _Color1.rgb) * saturate(1-data2 + _Color2.rgb) * saturate(1-data3 + _Color3.rgb) * saturate(1-data4 + _Color4.rgb);
			color *= dark * dark * dark;
			


			//second add color for shown data
			color += (data1 * _Color1.rgb) + (data2 * _Color2.rgb) + (data3 * _Color3.rgb) + (data4 * _Color4.rgb);
			//brighten hovered State
			color = (s.r * 0.2 + 0.8) * color + s.r * 0.05;
			//add state lines over top
			o.Albedo = lerp(color.rgb, _StateLines.rgb, d.r * _StateLines.a);



			o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_MainTex));
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
			//o.Emission = (1-c.a) * _CGlow * 50;
            //o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
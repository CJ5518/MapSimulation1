Shader "Landon/USA-Tint_SingleChannels" {
    Properties {
        _MainTex ("Base Texture (rgb)", 2D) = "white" {}
		[NoScaleOffset] _BumpMap ("Normal Map", 2D) = "bump" {}
		[NoScaleOffset] _Details ("State Lines (r) nothing yet (gba)", 2D) = "white" {}
		[NoScaleOffset] _State ("State Mask(r) nothing yet (gba)", 2D) = "grey" {}

		_StateLines ("Lines Color (rgb) Intensity (a)", Color) = (1,1,1,1)

		_DataS ("Susceptible Data",2D) = "black" {}
		_DataE ("Exposed Data",2D) = "black" {}
		_DataI ("Infected Data",2D) = "black" {}
		_DataV ("Vaccinated Data",2D) = "black" {}
		_DataR ("Recovered Data",2D) = "black" {}

		_Susceptible ("Susceptible Color (rgb) Intensity (a)", Color) = (1,1,1,1)
		_Exposed ("Exposed Color (rgb) Intensity (a)", Color) = (1,0,0,1)
		_Infected ("Infected Color (rgb) Intensity (a)", Color) = (0,1,0,1)
		_Vaccinated ("Vaccinated Color (rgb) Intensity (a)", Color) = (0,0,1,1)
		_Recovered ("Recovered Color (rgb) Intensity (a)", Color) = (0,0,0,1)

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader {
        Tags { "RenderType"="Opaque"}
        //LOD 400
		//ColorMask RGBA


        CGPROGRAM
        #pragma surface surf Standard // fullforwardshadows
        #pragma target 3.0  // (shader model 3.0 for nicer looking lighting)


        sampler2D _MainTex, _BumpMap, _Details, _State, _DataS, _DataE, _DataI, _DataV, _DataR;
        struct Input {
            float2 uv_MainTex;
        };
        fixed _Glossiness, _Metallic;
        fixed4 _StateLines, _Susceptible, _Exposed, _Infected, _Vaccinated, _Recovered;


        void surf (Input IN, inout SurfaceOutputStandard o) {

            fixed4 m = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 d = tex2D (_Details, IN.uv_MainTex);
			fixed4 s = tex2D (_State, IN.uv_MainTex);

			fixed sus = tex2D(_DataS, IN.uv_MainTex).a * _Susceptible.a;
			fixed exp = tex2D(_DataE, IN.uv_MainTex).a * _Exposed.a;
			fixed inf = tex2D(_DataI, IN.uv_MainTex).a * _Infected.a;
			fixed vac = tex2D(_DataV, IN.uv_MainTex).a * _Vaccinated.a;
			fixed rec = tex2D(_DataR, IN.uv_MainTex).a * _Recovered.a;

			//First for any data being shown, lerp baseTex to black
            fixed3 color = lerp(m.rgb, fixed3(0,0,0), exp + inf + rec);
			
			//second add color for shown data
			color += exp * _Exposed.rgb + inf * 2 * _Infected.rgb + rec * _Recovered.rgb + vac * _Vaccinated.rgb;


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
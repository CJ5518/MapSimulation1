// Based on cician's shader from https://forum.unity3d.com/threads/simple-optimized-blur-shader.185327/#post-1267642

Shader "Landon/UI/Blur" {
    Properties {
        _Size ("Blur", Range(0, 30)) = 1
        [HideInInspector] _MainTex ("Image (draws over blur)", 2D) = "white" {}
        _AdditiveColor ("Blur Add Color", Color) = (0, 0, 0, 1)
        _MultiplyColor ("Blur Mult Color", Color) = (1, 1, 1, 1)
    }

    Category {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Opaque" }


        SubShader {
            // Horizontal blur
            GrabPass {
                "_HBlur"
            }
            //ZTest Off
            Cull Off
            Lighting Off
            ZWrite Off
            ZTest [unity_GUIZTestMode]
            Blend SrcAlpha OneMinusSrcAlpha

            Pass {          
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma fragmentoption ARB_precision_hint_fastest
                #include "UnityCG.cginc"

                struct appdata_t {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
					float4  color : COLOR;
                };

                struct v2f {
                    float4 vertex : POSITION;
                    float4 uvgrab : TEXCOORD0;
                    float2 uvmain : TEXCOORD1;
					float4  color : COLOR;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;

                v2f vert (appdata_t v) {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);

                    #if UNITY_UV_STARTS_AT_TOP
                    float scale = -1.0;
                    #else
                    float scale = 1.0;
                    #endif

                    o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y * scale) + o.vertex.w) * 0.5;
                    o.uvgrab.zw = o.vertex.zw;

                    o.uvmain = TRANSFORM_TEX(v.texcoord, _MainTex);

					o.color = v.color;

                    return o;
                }

                sampler2D _HBlur;
                float4 _HBlur_TexelSize;
                float _Size;
                fixed4 _AdditiveColor, _MultiplyColor;

                half4 frag( v2f i ) : COLOR {   
                    half4 image = tex2D(_MainTex, i.uvmain) * i.color;

					half4 sum = half4(0,0,0,0);
                    #define G(weight,X,Y) tex2Dproj( _HBlur, UNITY_PROJ_COORD(float4(i.uvgrab.x + _HBlur_TexelSize.x * X * _Size, i.uvgrab.y + _HBlur_TexelSize.y * Y * _Size, i.uvgrab.z, i.uvgrab.w))) * weight

																	sum += G(0.14, -1, 3);	sum += G(0.22, 0, 3);	sum += G(0.14, 1, 3);
											sum += G(0.22, -2, 2);	sum += G(0.43, -1, 2);	sum += G(0.57, 0, 2);	sum += G(0.43, 1, 2);	sum += G(0.22, 2, 2);
					sum += G(0.14, -3, 1);	sum += G(0.43, -2, 1);	sum += G(0.78, -1, 1);	sum += G(1, 0, 1);		sum += G(0.78, 1, 1);	sum += G(0.43, 2, 1);	sum += G(0.14, 3, 1);
					sum += G(0.22, -3, 0);	sum += G(0.57, -2, 0);	sum += G(1, -1, 0);		sum += G(1, 0, 0);		sum += G(1, 1, 0);		sum += G(0.57, 2, 0);	sum += G(0.22, 3, 0);
					sum += G(0.14, -3, -1);	sum += G(0.43, -2, -1);	sum += G(0.78, -1, -1);	sum += G(1, 0, -1);		sum += G(0.78, 1, -1);	sum += G(0.43, 2, -1);	sum += G(0.14, 3, -1);
											sum += G(0.22, -2, -2);	sum += G(0.43, -1, -2);	sum += G(0.57, 0, -2);	sum += G(0.43, 1, -2);	sum += G(0.22, 2, -2);
																	sum += G(0.14, -1, -3);	sum += G(0.22, 0, -3);	sum += G(0.14, 1, -3);
					
					sum /= 11.7;

					half4 result = sum * _MultiplyColor + _AdditiveColor;
					result.a = image.a;

                    return result;
                }
                ENDCG
            }
        }
    }
}
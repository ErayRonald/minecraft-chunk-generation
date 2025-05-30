Shader "Custom/WaterShader" {
    Properties {
        _MainTex ("Water Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Transparency ("Transparency", Range(0,1)) = 0.5
        _ScrollSpeed ("Scroll Speed", Float) = 0.05
    }
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Transparency;
            float _ScrollSpeed;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float2 uv = i.uv;
                uv.x += _Time.y * _ScrollSpeed;
                fixed4 col = tex2D(_MainTex, uv) * _Color;
                col.a = _Transparency;
                return col;
            }
            ENDCG
        }
    }
}
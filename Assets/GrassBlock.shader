Shader "Custom/GrassBlock"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DirtTex ("Dirt Texture", 2D) = "white" {}
        _GrassTex ("Grass Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        sampler2D _DirtTex;
        sampler2D _GrassTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
        };

        void surf(Input IN, inout SurfaceOutput o)
        {
            float3 worldNormal = normalize(IN.worldNormal);
            float grassBlend = smoothstep(0.5, 0.7, worldNormal.y);
            float3 dirtColor = tex2D(_DirtTex, IN.uv_MainTex).rgb;
            float3 grassColor = tex2D(_GrassTex, IN.uv_MainTex).rgb;
            o.Albedo = lerp(dirtColor, grassColor, grassBlend);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
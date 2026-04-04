Shader "UI/InvertColorWithBorder"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _BorderSize ("Border Size", Float) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" }

        GrabPass { "_BackgroundTex" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _BackgroundTex;

            float _BorderSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                 v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);

    o.screenPos = ComputeGrabScreenPos(o.vertex);

    o.uv = v.uv;
    return o;
            }

            float Luminance(float3 col)
            {
                return dot(col, float3(0.299, 0.587, 0.114));
            }

            fixed4 frag (v2f i) : SV_Target
            {
    float alpha = tex2D(_MainTex, i.uv).a;
    if (alpha < 0.01) discard;

    float2 screenUV = i.screenPos.xy / i.screenPos.w;
    float3 bg = tex2D(_BackgroundTex, screenUV).rgb;

    float lum = dot(bg, float3(0.299, 0.587, 0.114));
    float inv = 1 - lum;

    return float4(inv, inv, inv, 1);
            }
            ENDCG
        }
    }
}
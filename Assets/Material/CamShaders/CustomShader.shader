Shader "ImageEffect/CustomShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        [Toggle] _Reverse ("Reverse", Float) = 0
        [Toggle] _BlackWhite ("BlackWhite", Float) = 0

        [Toggle] _Contrast ("Contrast", Float) = 0
        _ContrastThreshold ("Contrast Threshold", Range(0,1)) = 0.5

        [Toggle] _Posterize ("Posterize", Float) = 0
        _PosterizeSteps ("Posterize Steps", Range(2,32)) = 4

        [Toggle] _Pixelate ("Pixelate", Float) = 0
        _PixelSize ("Pixel Size", Range(32,1024)) = 256

        [Toggle] _Vignette ("Vignette", Float) = 0
        _VignetteStrength ("Vignette Strength", Range(0,2)) = 1

        [Toggle] _Edge ("Edge Detect", Float) = 0
        _EdgeStrength ("Edge Strength", Range(0,5)) = 1

        [Toggle] _Noise ("Noise", Float) = 0
        _NoiseStrength ("Noise Strength", Range(0,1)) = 0.1

        [Toggle] _CRT ("CRT Scanline", Float) = 0
        _CRTStrength ("CRT Strength", Range(0,1)) = 0.2
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            float _Reverse;
            float _BlackWhite;

            float _Contrast;
            float _ContrastThreshold;

            float _Posterize;
            float _PosterizeSteps;

            float _Pixelate;
            float _PixelSize;

            float _Vignette;
            float _VignetteStrength;

            float _Edge;
            float _EdgeStrength;

            float _Noise;
            float _NoiseStrength;

            float _CRT;
            float _CRTStrength;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float random(float2 uv)
            {
                return frac(sin(dot(uv,float2(12.9898,78.233))) * 43758.5453);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Pixelate
                if(_Pixelate > 0.5)
                {
                    uv = floor(uv * _PixelSize) / _PixelSize;
                }

                fixed4 col = tex2D(_MainTex, uv);

                float luminance = dot(col.rgb, float3(0.2126,0.7152,0.0722));

                // Reverse
                if(_Reverse > 0.5)
                    col.rgb = 1 - col.rgb;

                // BlackWhite
                if(_BlackWhite > 0.5)
                    col.rgb = luminance;

                // Contrast
                if(_Contrast > 0.5)
                {
                    float bw = luminance > _ContrastThreshold ? 1 : 0;
                    col.rgb = bw;
                }

                // Posterize
                if(_Posterize > 0.5)
                    col.rgb = floor(col.rgb * _PosterizeSteps) / _PosterizeSteps;

                // Edge Detect
                if(_Edge > 0.5)
                {
                    float2 offset = float2(1.0/_ScreenParams.x, 1.0/_ScreenParams.y);

                    float c1 = tex2D(_MainTex, uv + float2(offset.x,0)).r;
                    float c2 = tex2D(_MainTex, uv - float2(offset.x,0)).r;
                    float c3 = tex2D(_MainTex, uv + float2(0,offset.y)).r;
                    float c4 = tex2D(_MainTex, uv - float2(0,offset.y)).r;

                    float edge = abs(c1-c2) + abs(c3-c4);
                    col.rgb = lerp(col.rgb, edge.xxx, _EdgeStrength);
                }

                // Noise
                if(_Noise > 0.5)
                {
                    float n = random(uv + _Time.y);
                    col.rgb += n * _NoiseStrength;
                }

                // CRT scanline
                if(_CRT > 0.5)
                {
                    float scan = sin(i.uv.y * _ScreenParams.y * 1.5);
                    col.rgb *= 1 - (_CRTStrength * (scan * 0.5 + 0.5));
                }

                // Vignette
                if(_Vignette > 0.5)
                {
                    float dist = distance(i.uv,float2(0.5,0.5));
                    float vig = smoothstep(0.8,0.5,dist);
                    col.rgb *= lerp(1,vig,_VignetteStrength);
                }

                return col;
            }

            ENDCG
        }
    }
}
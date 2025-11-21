// ClippedMarble.shader (Unlit 버전으로 테스트)
Shader "Custom/ClippedMarble"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _ClipYPosition ("Clip Y Position", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float _ClipYPosition;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex); // 월드 포지션 계산
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float clipValue = i.worldPos.y - _ClipYPosition;
                
                // 픽셀 버리기 로직
                if (clipValue < 0.0) 
                {
                    discard;
                }

                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                return col;
            }
            ENDCG
        }
    }
    // FallBack 쉐이더도 문제가 있을 수 있으니 "Standard"로 변경하거나 아예 제거합니다.
    FallBack "Standard" // Fallback을 Standard로 변경
}
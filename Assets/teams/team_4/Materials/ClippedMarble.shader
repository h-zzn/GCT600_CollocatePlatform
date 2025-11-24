// ClippedMarble.shader - VR Stereo Fixed Version
Shader "Custom/ClippedMarble"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        
        // Bowl information passed from C#
        _BowlCenter ("Bowl Center", Vector) = (0,0,0,0)
        _BowlUp ("Bowl Up Direction", Vector) = (0,1,0,0)
        _BowlRadius ("Bowl Radius", Float) = 0.15
        _BowlDepth ("Bowl Depth", Float) = 0.08
        _BowlWallHeight ("Bowl Wall Height", Float) = 0.03
        _MarbleRadius ("Marble Radius", Float) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _BowlCenter;
            float3 _BowlUp;  // 그릇의 위쪽 방향 (회전 정보)
            float _BowlRadius;
            float _BowlDepth;
            float _BowlWallHeight;
            float _MarbleRadius;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                float4 pos : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // Calculate clip height based on camera position (ported from C#)
            float CalculateClipHeight(float3 cameraPos, float3 marbleCenter)
            {
                // Bowl의 Up 방향 정규화
                float3 bowlUp = normalize(_BowlUp);
                
                // Bowl의 rim Y 위치 (Bowl의 up 방향 고려)
                float3 bowlRimPoint = _BowlCenter.xyz + bowlUp * _BowlWallHeight;
                
                // 카메라 높이 계산 (Bowl의 up 방향 기준)
                float3 camToBowlRim = cameraPos - bowlRimPoint;
                float cameraHeight = dot(camToBowlRim, bowlUp);  // Up 방향으로의 투영
                
                float3 camToMarble = normalize(marbleCenter - cameraPos);
                float verticalAngle = asin(-dot(camToMarble, bowlUp)) * 57.2958; // Bowl 기준 수직 각도
                
                // CASE 1: Camera is much higher than bowl
                if (cameraHeight > _BowlDepth * 2.0)
                {
                    float viewFactor = saturate(verticalAngle / 70.0);
                    float clipOffset = lerp(_MarbleRadius * 0.9, _MarbleRadius * 0.2, viewFactor);
                    // Marble center에서 bowl up 방향으로 offset
                    return dot(marbleCenter - _BowlCenter.xyz, bowlUp) - clipOffset;
                }
                
                // CASE 2: Camera is much lower than bowl
                if (cameraHeight < -_BowlDepth)
                {
                    return dot(marbleCenter - _BowlCenter.xyz, bowlUp) + _MarbleRadius * 0.9;
                }
                
                // CASE 3: Side view - simplified for rotation
                // Bowl의 right/forward 방향 계산
                float3 bowlForward = abs(bowlUp.y) > 0.9 ? float3(0, 0, 1) : float3(0, 1, 0);
                float3 bowlRight = normalize(cross(bowlUp, bowlForward));
                bowlForward = cross(bowlRight, bowlUp);
                
                // Camera를 Bowl의 local space로 변환
                float3 camToBowl = cameraPos - _BowlCenter.xyz;
                float2 camHorizontal = float2(dot(camToBowl, bowlRight), dot(camToBowl, bowlForward));
                float horizontalDist = length(camHorizontal);
                
                if (horizontalDist < 0.001)
                {
                    return dot(marbleCenter - _BowlCenter.xyz, bowlUp) - _MarbleRadius * 0.5;
                }
                
                float2 horizontalDir = normalize(camHorizontal);
                float3 nearestRimPoint = _BowlCenter.xyz + 
                    bowlRight * (-horizontalDir.x * _BowlRadius) +
                    bowlForward * (-horizontalDir.y * _BowlRadius) +
                    bowlUp * _BowlWallHeight;
                
                float heightDifference = cameraHeight;
                
                // Ray-sphere intersection
                float3 rimToMarble = marbleCenter - nearestRimPoint;
                float3 rayDir = normalize(rimToMarble);
                
                float3 oc = nearestRimPoint - marbleCenter;
                float a = 1.0;
                float b = 2.0 * dot(oc, rayDir);
                float c = dot(oc, oc) - _MarbleRadius * _MarbleRadius;
                float discriminant = b * b - 4.0 * a * c;
                
                if (discriminant < 0.0)
                {
                    return dot(marbleCenter - _BowlCenter.xyz, bowlUp) + _MarbleRadius;
                }
                
                float t = (-b - sqrt(discriminant)) / (2.0 * a);
                if (t < 0.0) 
                {
                    t = (-b + sqrt(discriminant)) / (2.0 * a);
                }
                
                float3 occlusionPoint = nearestRimPoint + t * rayDir;
                
                // Bowl의 up 방향 기준으로 높이 계산
                float occlusionHeight = dot(occlusionPoint - _BowlCenter.xyz, bowlUp);
                
                // Adjust based on camera height
                if (heightDifference <= 0.0)
                {
                    float heightFactor = saturate(-heightDifference / _BowlDepth);
                    float additionalOcclusion = heightFactor * _MarbleRadius * 0.5;
                    return occlusionHeight + additionalOcclusion;
                }
                else
                {
                    float heightFactor = saturate(heightDifference / (_BowlDepth * 2.0));
                    float lessOcclusion = heightFactor * _MarbleRadius * 0.4;
                    return occlusionHeight - lessOcclusion;
                }
            }

            v2f vert (appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                o.pos = UnityObjectToClipPos(v.vertex);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // KEY FIX: Use per-eye camera position
                // _WorldSpaceCameraPos is automatically different for each eye in VR
                // Shader에서 Marble 위치를 실시간으로 받아옴:
                float3 cameraPos = _WorldSpaceCameraPos;
                
                // Get marble center from object-to-world matrix
                float3 marbleCenter = float3(unity_ObjectToWorld[0].w, 
                                             unity_ObjectToWorld[1].w, 
                                             unity_ObjectToWorld[2].w);
                
                // Calculate clip height for this eye
                // 매 프레임마다 현재 위치 기준으로 계산!
                float clipHeight = CalculateClipHeight(cameraPos, marbleCenter);
                
                // Bowl의 up 방향 기준으로 clip
                float3 bowlUp = normalize(_BowlUp);
                float pixelHeight = dot(i.worldPos.xyz - _BowlCenter.xyz, bowlUp);
                float clipValue = pixelHeight - clipHeight;
                
                // Discard pixels below clip plane
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
    FallBack "Standard"
}

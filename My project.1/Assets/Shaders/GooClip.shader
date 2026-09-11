Shader "Custom/GooClip"
{
    Properties
    {
        _Color("Color", Color) = (0, 0.8, 0.1, 1)
        _ClipPlanePosition("Clip Plane Position (World)", Vector) = (0, 0, 0, 0)
        _ClipPlaneNormal("Clip Plane Normal (World)", Vector) = (0, 1, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _ClipPlanePosition;
                float4 _ClipPlaneNormal;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float side = dot(IN.positionWS - _ClipPlanePosition.xyz, _ClipPlaneNormal.xyz);
                clip(-side); // discard anything on the far side the normal points toward (above the liquid surface)

                Light mainLight = GetMainLight();
                float3 normal = normalize(IN.normalWS);
                float ndotl = saturate(dot(normal, mainLight.direction));
                float3 lighting = mainLight.color * ndotl + 0.35; // flat ambient fudge so the shadow side isn't pure black

                float3 color = _Color.rgb * lighting;
                return half4(color, _Color.a);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}

Shader "Custom/DOSStyleShader"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _UseTexture ("Use Texture", Range(0, 1)) = 0
        _ColorLevels ("Color Levels", Range(2, 32)) = 8
        _FlatShading ("Flat Shading", Range(0, 1)) = 1
        [Space(10)]
        [Header(Lighting Settings)]
        _LightLevels ("Light Steps", Range(1, 32)) = 4
        _ShadowIntensity ("Shadow Darkness", Range(0, 1)) = 0.6
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
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                nointerpolation float3 flatNormal : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _UseTexture;
            float _ColorLevels;
            float _FlatShading;
            float _LightLevels;
            float _ShadowIntensity;
            
            // Quantizes a value to a specific number of levels
            float Quantize(float value, float levels)
            {
                return floor(value * (levels - 1) + 0.5) / (levels - 1);
            }
            
            // Quantizes a color to a specific number of levels per channel
            float3 QuantizeColor(float3 color, float levels)
            {
                return float3(
                    Quantize(color.r, levels),
                    Quantize(color.g, levels),
                    Quantize(color.b, levels)
                );
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                // Calculate per-vertex normal
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                // Store flat normal (same for all vertices in the triangle)
                // This uses the first vertex normal of each triangle for the entire face
                o.flatNormal = UnityObjectToWorldNormal(v.normal);
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Choose between flat shading (per-face) and regular shading
                float3 normal = lerp(i.worldNormal, i.flatNormal, _FlatShading);
                normal = normalize(normal);
                
                // Either use texture or base color based on the UseTexture parameter
                fixed4 texColor = tex2D(_MainTex, i.uv);
                fixed4 col = lerp(_Color, texColor, _UseTexture);
                
                // Basic lighting calculation (directional light only for simplicity)
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float ndotl = max(0, dot(normal, lightDir));
                
                // Calculate ambient light term to control shadow darkness
                float ambient = 1.0 - _ShadowIntensity;
                
                // Quantize lighting to get stepped/banded lighting with custom number of levels
                float lightIntensity = Quantize(ndotl, _LightLevels);
                
                // Apply lighting to the texture color with ambient term to control darkness
                // This ensures shadows are never completely black unless ShadowIntensity is 1
                col.rgb *= lerp(ambient, 1.0, lightIntensity);
                
                // Quantize the final color to reduce color depth
                col.rgb = QuantizeColor(col.rgb, _ColorLevels);
                
                return col;
            }
            ENDCG
        }
    }
}
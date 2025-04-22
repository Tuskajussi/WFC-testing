Shader "Custom/VertexColorWithLightingAndShadows"
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "white" {}
        _Color("Vertex Color", Color) = (1,1,1,1)
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }

            Pass
            {
                Tags { "LightMode" = "ForwardBase" }

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fog

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                    float4 color : COLOR;
                };

                struct v2f
                {
                    float4 pos : POSITION;
                    float3 normal : NORMAL;
                    float4 color : COLOR;
                    float4 worldPos : TEXCOORD0;  // World position for shadowing
                    float3 worldNormal : TEXCOORD1;  // World normal for lighting
                };

                // Vertex shader
                v2f vert(appdata v)
                {
                    v2f o;

                    // Convert the vertex position to clip space
                    o.pos = UnityObjectToClipPos(v.vertex);

                    // Pass the vertex color to the fragment shader
                    o.color = v.color;

                    // Transform the vertex normal into world space
                    o.worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);

                    // Transform the vertex position into world space (for shadowing)
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                    return o;
                }

                // Fragment shader
                half4 frag(v2f i) : SV_Target
                {
                    // Lighting calculations
                    float3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz); // World-space light direction
                    float3 normal = normalize(i.worldNormal); // Normalize the world normal
                    float diff = max(0.0, dot(normal, worldLightDir)); // Lambertian diffuse lighting

                    // Apply lighting to the vertex color
                    half4 litColor = i.color * diff;

                    // Return the final color with lighting and shadows applied
                    return litColor;
                }
                ENDCG
            }
        }
            Fallback "Diffuse"
}

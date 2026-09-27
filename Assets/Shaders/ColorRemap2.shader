    Shader "Custom/ColorRemap2"
    {
        Properties
        {
            _MainTex ("Sprite Texture", 2D) = "white" {}
            _Color ("Tint", Color) = (1,1,1,1)
            
            _SrcColorMap ("Source Color Map", 2D) = "white" {}
            _DstColorMap ("Dest Color Map", 2D) = "white" {}
        }
     
        SubShader
        {
            Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
     
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
     
            Pass
            {
                Tags { "LightMode" = "Universal2D" }
     
                HLSLPROGRAM
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
     
                #pragma vertex UnlitVertex
                #pragma fragment UnlitFragment
     
                #pragma multi_compile _ DEBUG_DISPLAY
     
                struct Attributes
                {
                    float3 positionOS   : POSITION;
                    float4 color        : COLOR;
                    float2 uv           : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };
     
                struct Varyings
                {
                    float4  positionCS  : SV_POSITION;
                    half4   color       : COLOR;
                    float2  uv          : TEXCOORD0;
                    UNITY_VERTEX_OUTPUT_STEREO
                };
     
                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);
                half4 _MainTex_ST;
                
                TEXTURE2D(_SrcColorMap);
                SAMPLER(sampler_SrcColorMap);
                half4 _SrcColorMap_ST;
                float4 _SrcColorMap_TexelSize;
                
                TEXTURE2D(_DstColorMap);
                SAMPLER(sampler_DstColorMap);
                half4 _DstColorMap_ST;
     
                Varyings UnlitVertex(Attributes v)
                {
                    Varyings o = (Varyings)0;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
     
                    o.positionCS = TransformObjectToHClip(v.positionOS);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    o.color = v.color;
                    return o;
                }
     
                half4 UnlitFragment(Varyings i) : SV_Target
                {
                    float4 mainCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                    float paletteWidth = _SrcColorMap_TexelSize.z;
                    int paletteWidthI = int(paletteWidth);
                    
                    float bestFit = 999.0;
                    int bestFitPixel = 0;
                    
                    for (int i = 0; i < paletteWidthI; i++)
                    {
                        float4 thisCol = SAMPLE_TEXTURE2D(_SrcColorMap, sampler_SrcColorMap, float2(i/paletteWidth, 0.5));
                        float4 dist = abs(mainCol - thisCol);
                        float distF = dist.x + dist.y + dist.z + dist.z;
                        
                        if (distF < bestFit)
                        {
                            bestFit = distF;
                            bestFitPixel = i;
                        }
                    }
                    
                    float4 newCol = SAMPLE_TEXTURE2D(_DstColorMap, sampler_DstColorMap, float2(bestFitPixel/paletteWidth, 0.5));
     
                    return newCol * mainCol.a;
                }
                ENDHLSL
            }
            
        }
     
        Fallback "Sprites/Default"
    }
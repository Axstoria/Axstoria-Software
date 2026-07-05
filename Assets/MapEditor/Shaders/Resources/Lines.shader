Shader "Custom/Lines"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Overlay" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 position : POSITION;
                float4 color    : COLOR;
            };

            struct Varyings
            {
                float4 pos   : SV_POSITION;
                float4 color : COLOR;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.pos   = TransformObjectToHClip(v.position.xyz);
                o.color = v.color;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                return i.color;
            }
            ENDHLSL
        }
    }

    // Built-in pipeline fallback
    SubShader
    {
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            Cull Off
            Fog { Mode Off }
            BindChannels
            {
                Bind "vertex", vertex
                Bind "color", color
            }
        }
    }
}

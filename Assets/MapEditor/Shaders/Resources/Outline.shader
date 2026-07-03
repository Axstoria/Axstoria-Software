Shader "Custom/Outline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1, .5, 0, 1)
        _Outline      ("Outline width", Range(0, 1)) = .05
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "DisableBatching" = "True" }
        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            ZWrite On
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float  _Outline;
                float4 _OutlineColor;
            CBUFFER_END

            struct Attributes
            {
                float4 position : POSITION;
                float3 normal   : NORMAL;
            };

            struct Varyings
            {
                float4 pos   : SV_POSITION;
                float4 color : COLOR;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                v.position.xyz *= (1 + _Outline);
                o.pos   = TransformObjectToHClip(v.position.xyz);
                o.color = _OutlineColor;
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
        Tags { "DisableBatching" = "True" }
        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode" = "Always" }
            Cull Front
            ZWrite On
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f     { float4 pos : POSITION; float4 color : COLOR; };

            uniform float  _Outline;
            uniform float4 _OutlineColor;

            v2f vert(appdata v)
            {
                v2f o;
                v.vertex *= (1 + _Outline);
                o.pos   = UnityObjectToClipPos(v.vertex);
                o.color = _OutlineColor;
                return o;
            }
            half4 frag(v2f i) : COLOR { return i.color; }
            ENDCG
        }
    }

    Fallback "Diffuse"
}

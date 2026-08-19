Shader "VAT/VAT_URP"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
        _Metallic("Metallic", Range(0,1)) = 0
        _Smoothness("Smoothness", Range(0,1)) = 0.5

        [NoScaleOffset] _PosTex("Position Texture", 2D) = "black" {}
        [NoScaleOffset] _NrmTex("Normal Texture", 2D) = "black" {}
        [NoScaleOffset] _TanTex("Tangent Texture", 2D) = "black" {}
        _VAT_VertexCount("Vertex Count", Float) = 0
        _VAT_TotalFrames("Total Frames", Float) = 1
        _AnimFrameA("Anim Frame A", Float) = 0
        _AnimFrameB("Anim Frame B", Float) = 0
        _BlendWeight("Blend Weight", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
        Pass
        {
            // URP uses its own shader framework; a full implementation
            // requires duplicating the URP Lit template and adding VAT
            // vertex displacement. For brevity, we supply a stub that
            // will not break compilation but will not render correctly.
            // Replace with a full URP Shader Graph version when needed.
            HLSLPROGRAM
            #pragma target 4.5
            #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch
            
            // Minimal stub to prevent errors
            struct Attributes {};
            struct Varyings { float4 positionCS : SV_POSITION; };
            Varyings vert(Attributes input) { Varyings output; output.positionCS = 0; return output; }
            float4 frag(Varyings input) : SV_Target { return 0; }
            
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}

Shader "VAT/VAT_HDRP"
{
    Properties
    {
        // HDRP material properties are complex; this is a minimal
        // placeholder that uses HDRP Lit template.
        [MainTexture] _BaseColorMap("Base Color Map", 2D) = "white" {}
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
        Tags { "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            // HDRP uses its own shader framework; a full implementation
            // requires duplicating the HDRP Lit template and adding VAT
            // vertex displacement. For brevity, we supply a stub that
            // will not break compilation but will not render correctly.
            // Replace with a full HDRP version when needed.
            HLSLPROGRAM
            #pragma target 4.5
            #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/VertMesh.hlsl"
            ENDHLSL
        }
    }
}
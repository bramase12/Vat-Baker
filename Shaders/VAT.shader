Shader "VAT/VAT"
{
    Properties
    {
        [MainTexture] _MainTex ("Base Map", 2D) = "white" {}
        [MainColor]   _Color    ("Base Color", Color) = (1,1,1,1)
        _Glossiness   ("Smoothness", Range(0,1)) = 0.5
        _Metallic     ("Metallic",   Range(0,1)) = 0.0

        [NoScaleOffset] _BumpMap    ("Normal Map", 2D) = "bump" {}
        _BumpScale                 ("Normal Scale", Float) = 1.0

        [NoScaleOffset] _EmissionMap   ("Emission Map", 2D) = "white" {}
        [HDR]           _EmissionColor ("Emission Color", Color) = (0,0,0,1)

        [NoScaleOffset] _OcclusionMap    ("Occlusion Map", 2D) = "white" {}
        _OcclusionStrength            ("Occlusion Strength", Range(0,1)) = 1.0

        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5

        [NoScaleOffset] _PosTex ("Position Texture", 2D) = "black" {}
        [NoScaleOffset] _NrmTex ("Normal Texture",   2D) = "black" {}
        [NoScaleOffset] _TanTex ("Tangent Texture",  2D) = "black" {}

        _VAT_VertexCount  ("Vertex Count",    Float) = 1
        _VAT_TotalFrames  ("Total Frames",    Float) = 1
        _VAT_TexWidth     ("Texture Width",   Float) = 256
        _VAT_RowsPerFrame ("Rows Per Frame",  Float) = 1
        _AnimFrameA       ("Anim Frame A",    Float) = 0
        _AnimFrameB       ("Anim Frame B",    Float) = 0
        _BlendWeight      ("Blend Weight",    Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard vertex:vert addshadow fullforwardshadows
        #pragma target 4.5
        #pragma multi_compile_instancing
        #pragma shader_feature _ALPHATEST_ON

        sampler2D _MainTex, _BumpMap, _EmissionMap, _OcclusionMap;
        half _Glossiness, _Metallic, _BumpScale, _OcclusionStrength;
        half4 _Color, _EmissionColor;
        float _Cutoff;

        sampler2D _PosTex, _NrmTex, _TanTex;
        float _VAT_VertexCount, _VAT_TotalFrames, _VAT_TexWidth, _VAT_RowsPerFrame;
        float _AnimFrameA, _AnimFrameB, _BlendWeight;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv1 : TEXCOORD1;
        };

        float2 PackedUV(float vertexU, float frame)
        {
            float idx = vertexU * _VAT_VertexCount - 0.5;
            float row = floor(idx / _VAT_TexWidth);
            float col = idx - row * _VAT_TexWidth;
            float y = frame * _VAT_RowsPerFrame + row;
            return float2((col + 0.5) / _VAT_TexWidth, (y + 0.5) / (_VAT_RowsPerFrame * _VAT_TotalFrames));
        }

        float3 SamplePos(float u, float f) { return tex2Dlod(_PosTex, float4(PackedUV(u,f),0,0)).rgb; }
        float3 SampleNrm(float u, float f) { return tex2Dlod(_NrmTex, float4(PackedUV(u,f),0,0)).rgb * 2 - 1; }
        float4 SampleTan(float u, float f) { float4 t = tex2Dlod(_TanTex, float4(PackedUV(u,f),0,0)); t.xyz = t.xyz * 2 - 1; return t; }

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_SETUP_INSTANCE_ID(v);
            o.uv_MainTex = v.texcoord;
            o.uv1 = v.texcoord1;

            if (length(v.texcoord1) < 0.0001 && _VAT_VertexCount > 1) return;

            float u = v.texcoord1.x;
            float3 posA = SamplePos(u, _AnimFrameA);
            float3 posB = SamplePos(u, _AnimFrameB);
            float3 posOS = lerp(posA, posB, _BlendWeight);

            float3 nrmA = SampleNrm(u, _AnimFrameA);
            float3 nrmB = SampleNrm(u, _AnimFrameB);
            v.normal = lerp(nrmA, nrmB, _BlendWeight);

            float4 tanA = SampleTan(u, _AnimFrameA);
            float4 tanB = SampleTan(u, _AnimFrameB);
            v.tangent = lerp(tanA, tanB, _BlendWeight);

            v.vertex.xyz = posOS;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

            #if _ALPHATEST_ON
                clip(o.Alpha - _Cutoff);
            #endif

            o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_MainTex), _BumpScale);
            o.Emission = tex2D(_EmissionMap, IN.uv_MainTex).rgb * _EmissionColor.rgb;
            o.Occlusion = lerp(1.0, tex2D(_OcclusionMap, IN.uv_MainTex).r, _OcclusionStrength);
        }
        ENDCG
    }

    FallBack "Diffuse"
}
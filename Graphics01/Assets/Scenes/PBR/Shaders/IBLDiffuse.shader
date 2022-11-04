Shader "Custom/IBLDiffuse"
{
    Properties
    {
        _CubeMap("cubemap",CUBE)=""{}
        _sampleDelta("sampleDelta",float)=1
    }
 
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
 
        Pass
        {
            Name "IBLDiffuse"
            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

 
            void getIrradiance(float3 normal, half sampleDelta ,samplerCUBE cubecolor,out float3 irrdiance)
            {
                float3 up = half3(0,1,0);
                float3 right = cross(up,normal);
                up = cross(normal, right);
                half CountSample = 0;
            
                for (float phi = 0.0; phi < 2.0 *3.14; phi +=sampleDelta)
                {
                    for (float theta = 0.0; phi < 0.5 *3.14; theta +=sampleDelta)
                    {
                        // spherical to cartesian (in tangent space)
                        float3 tangentSample = float3(sin(theta) * cos(phi),  sin(theta) * sin(phi), cos(theta));
                        //tangent tp world
                        float3 sampleVec = tangentSample.x * right + tangentSample.y * up + tangentSample.z * normal;
                        irrdiance += texCUBE(cubecolor,sampleVec)* cos(theta) * sin(theta) ;
                        CountSample++;
                    }
                }
                irrdiance = PI *irrdiance*(1.0/half(CountSample));
            }
            
            struct Attributes
            {
                float4 positionOS       : POSITION;
                float2 uv               : TEXCOORD0;
                float3 Normal            :NORMAL;
            };
 
            struct Varyings
            {
                float4 positionCS       : SV_POSITION;
                float2 uv               : TEXCOORD0;
                float3 Normal      : TEXCOORD1;
            };
 
            CBUFFER_START(UnityPerMaterial)
            half _sampleDelta;
            CBUFFER_END
            samplerCUBE _CubeMap;
            
 
            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(v.Normal);
                o.Normal = normalInput.normalWS;
 
                return o;
            }
 
            half4 frag(Varyings i) : SV_Target
            {
                half3 WorldNormal = normalize(i.Normal);
                half3  irrdiance = half3(0,0,0);
                half4 cubecolor = texCUBE(_CubeMap,normalize(WorldNormal));
                getIrradiance(WorldNormal,_sampleDelta,_CubeMap,irrdiance);
                half4 color = texCUBE(_CubeMap,normalize(WorldNormal));
         

                return half4(color.rgb,1);
            }
            ENDHLSL
        }
    }
}

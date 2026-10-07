Shader "Unlit/ConvolutionFilterShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FilterTex ("Filter Texture", 2D) = "white" {}
        _FilterSize ("Filter Size", Vector) = (0,0,0,0)
        _MainTexCoef ("Texture Coefficient", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"}
        LOD 100
        ZTest Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            ZTest Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            sampler2D _FilterTex;
            float4 _MainTexCoef;
            float4 _FilterSize;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float k = 0;
                float result = float4(0,0,0,0);
                while(k < _FilterSize.x)
                {
                    float j = 0;
                    while (j < _FilterSize.y)
                    {
                        fixed4 filCol = tex2D(_FilterTex, float2(k/_FilterSize.x,j/_FilterSize.y));
                        fixed4 col = tex2D(_MainTex, i.uv);
                        result += fixed4(filCol.r * col.r, filCol.g*col.g, filCol.b*col.b, filCol.a*col.a);
                        
                        j = j+1;
                    }
                    k = k+1;
                }

                

                return result/(_FilterSize.x * _FilterSize.y);
            }
            ENDCG
        }
    }
}

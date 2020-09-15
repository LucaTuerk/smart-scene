Shader "SmartScene/VisibilityShader"
{
    Properties
    { 
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderQueue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            bool _greyScale;

            struct appdata
            {
                float4 vertex : POSITION;
                uint vertexid : SV_VertexID;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : TEXCOORD0;
            };


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col;
              
                    col = lerp(float4(1,0,0,0.2), float4(0,1,0,1), i.color.r);
                    if (i.color.r == 0)
                    {
                        col = float4(0,0,0,0);
                    }

                    if( _greyScale ) 
                        col = lerp(float4(0,0,0,0), float4(1,1,1,1), i.color.r);
                        
                return col;
            }
            ENDCG
        }
    }
}

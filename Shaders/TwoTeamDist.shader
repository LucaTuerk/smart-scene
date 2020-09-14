Shader "SmartScene/TwoTeamDist"
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

            sampler2D _Color;
            float _maxDistance;
            float _showDistance;
            bool _showRed;
            bool _showBlue;
            bool _showMeeting;
            bool _Overlap;

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
                float red = i.color.r * _maxDistance;
                float blue = i.color.b * _maxDistance; 

                float4 outCol = float4(0,0,0,0);
                if ( red < _showDistance && _showRed &&  i.color.r != 0.0f && ( _Overlap || red < blue ) ) {
                    outCol += float4(1,0,0,0.5);
                }
                if ( blue < _showDistance && _showBlue && i.color.b != 0.0f && ( _Overlap || blue < red ) ) {
                    outCol += float4(0,0,1,0.5);
                }
                if ( _showMeeting ) {
                    outCol = float4 (0,0,0,0);
                    if ( 
                        (red - blue) < _showDistance &&
                        (blue - red) < _showDistance && 
                        red != 0.0f && blue != 0.0f
                    ) {
                        outCol = float4 (0, 1, 0, 0.5);
                    }
                }
                return i.color.a < 0.5f ? float4(1,1,1,1) : float4(outCol.rgb, min( outCol.a, 0.5f) );
            }
            ENDCG
        }
    }
}

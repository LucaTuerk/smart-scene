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
                float4 outCol = float4(0,0,0,0);
                if ( i.color.r * _maxDistance < _showDistance && _showRed) {
                    outCol += float4(1,0,0,0.5);
                }
                if ( i.color.b * _maxDistance < _showDistance && _showBlue) {
                    outCol += float4(0,0,1,0.5);
                }
                if ( _showMeeting ) {
                    outCol = float4 (0,0,0,0);
                    if ( abs ( i.color.r * _maxDistance - i.color.b * _maxDistance) < _showDistance ) {
                        outCol = float4 (1, 0, 1, 1);
                    }
                }

                return outCol;
            }
            ENDCG
        }
    }
}

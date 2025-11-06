Shader "UI/CutoutHole"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _HoleCenter ("Hole Center", Vector) = (0.5, 0.5, 0, 0)
        _HoleRadius ("Hole Radius", Float) = 0.2
        _Feather ("Feather (Blur Edge)", Float) = 0.02
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float4 _Color;
            float4 _HoleCenter;
            float _HoleRadius;
            float _Feather;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 delta = uv - _HoleCenter.xy;
                delta.x *= aspect;
                float dist = length(delta);

                float hole = smoothstep(_HoleRadius - _Feather, _HoleRadius, dist); // 0 inside, 1 outside
                fixed4 col = tex2D(_MainTex, uv) * _Color;
                col.a *= hole;

                return col;
            }
            ENDCG
        }
    }
}

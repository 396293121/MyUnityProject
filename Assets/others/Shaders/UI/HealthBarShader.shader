Shader "UI/HealthBar"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Metallic ("金属度", Range(0,1)) = 0.8
        _RimColor ("边缘光颜色", Color) = (1,0.5,0,1)
        _FlowSpeed ("流光速度", Float) = 1.0
        _LiquidColor ("液体颜色", Color) = (1,0,0,1)
        _Cutoff ("血量阈值", Range(0,1)) = 1.0
    }

    SubShader
    {
        Tags { 
            "Queue"="Transparent" 
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 viewDir : TEXCOORD1;
                float3 normal : TEXCOORD2;
            };

            sampler2D _MainTex;
            float _Metallic;
            float4 _RimColor;
            float _FlowSpeed;
            float4 _LiquidColor;
            float _Cutoff;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
                o.normal = v.normal;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 基础颜色
                fixed4 col = tex2D(_MainTex, i.uv) * _LiquidColor;
                
                // 金属质感
                float metallic = _Metallic * 0.5;
                col.rgb *= (1 + metallic);
                
                // 边缘光（菲涅尔效应）
                float rim = 1 - saturate(dot(i.viewDir, i.normal));
                col.rgb += _RimColor.rgb * pow(rim, 3) * _RimColor.a;
                
                // 流光效果
                float flow = sin((_Time.y * _FlowSpeed) + i.uv.x * 10) * 0.5 + 0.5;
                col.rgb += flow * 0.3 * (1 - _Metallic);
                
                // 血量裁剪
                clip(i.uv.x - (1 - _Cutoff));
                
                return col;
            }
            ENDCG
        }
    }
}

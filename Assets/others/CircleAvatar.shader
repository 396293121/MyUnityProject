Shader "Custom/CircleAvatar"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Radius ("Radius", Range(0, 0.5)) = 0.45
        _Softness ("Softness", Range(0, 0.1)) = 0.005
        _BorderWidth ("Border Width", Range(0, 0.1)) = 0.02
        _BorderColor ("Border Color", Color) = (1,1,1,1)
        _AspectRatio ("Aspect Ratio", Float) = 1.0
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Radius;
            float _Softness;
            float _BorderWidth;
            fixed4 _BorderColor;
            float _AspectRatio;
            
            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 调整UV比例
                float2 aspectUV = i.uv;
                aspectUV.x = (aspectUV.x - 0.5) * _AspectRatio + 0.5;
                
                // 计算中心点距离
                float2 center = float2(0.5, 0.5);
                float dist = distance(aspectUV, center);
                
                // 计算透明度
                float alpha = smoothstep(_Radius + _Softness, _Radius, dist);
                
                // 边框计算
                float border = smoothstep(_Radius - _BorderWidth, _Radius, dist) - 
                               smoothstep(_Radius, _Radius + _Softness, dist);
                
                // 采样纹理
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // 组合结果
                fixed4 final = col * alpha;
                final.rgb = lerp(final.rgb, _BorderColor.rgb, border);
                final.a = max(alpha, border);
                
                return final;
            }
            ENDCG
        }
    }
    
    CustomEditor "CircleAvatarShaderEditor"
}
Shader"FitImageToSquare" {
    Properties {
        _MainTex ("Dont use this, Used by code", 2D) = "white" {}
    }
 
    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment SimpleBlit
 
            #include "UnityCG.cginc"
 
            float _Aspect;
            UNITY_DECLARE_TEX2D(_MainTex);
            float4 _MainTex_TexelSize;
 
            struct v2f_yo
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
 
            v2f_yo vert(appdata_img v)
            {
                v2f_yo o;
                o.pos = UnityObjectToClipPos(v.vertex);                
                
                if (_Aspect > 1.0)
                {
                o.uv = v.texcoord * float2(1.0, _Aspect) - float2(0.0, (_Aspect - 1.0) * 0.5); 
                }
                else
                {
                    o.uv = v.texcoord * float2(1.0 / _Aspect, 1.0) - float2((1.0 / _Aspect - 1.0) * 0.5, 0.0);
                }
 
                return o;
            }
 
            fixed4 SimpleBlit(v2f_yo i) : SV_Target
            {
                if (i.uv.x < 0.0 || i.uv.x > 1.0 || i.uv.y < 0.0 || i.uv.y > 1.0)
                {
                    return 0;
                }
                else
                {
                    return UNITY_SAMPLE_TEX2D(_MainTex, i.uv);
                }
            }
            ENDCG
        }
    }
    Fallback Off
}
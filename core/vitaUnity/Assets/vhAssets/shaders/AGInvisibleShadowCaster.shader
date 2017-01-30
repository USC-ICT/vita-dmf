Shader "AG/Invisible Shadow Caster" {
    Subshader
    {
        Pass
        {
            Tags{ "LightMode" = "ShadowCaster" }
            ZWrite ON

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f
            {
                V2F_SHADOW_CASTER;
            };

            v2f vert(appdata_full v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER(o);
                return o;
            }

            float4 frag(v2f i) : COLOR
            {
                SHADOW_CASTER_FRAGMENT(i);
            }

            ENDCG
        }
    }

    Fallback off
}

Shader "Custom/ScanlineQuest" {

    Properties {

        [HDR]_ScanColor("Scan Color", Color) = (0,2,0,1)

        _LineWidth("Line Width", Float) = 0.05

        _Speed("Speed", Float) = 1.0

        _Boost("Quest Brightness Boost", Float) = 2.0 // Multiplicador de força

    }

    SubShader {

        // Mudamos o Queue para garantir que desenhe depois de tudo

        Tags { "RenderType"="Transparent" "Queue"="Transparent+500" }

        

        // Blend SrcAlpha OneMinusSrcAlpha é mais visível no Quest que o Additive

        Blend SrcAlpha OneMinusSrcAlpha 

        ZWrite Off

        Cull Off



        Pass {

            CGPROGRAM

            #pragma vertex vert

            #pragma fragment frag

            #include "UnityCG.cginc"



            struct v2f {

                float4 pos : SV_POSITION;

                float3 worldPos : TEXCOORD0;

            };



            float4 _ScanColor;

            float _LineWidth, _Speed, _Boost;

            float _MinY, _MaxY;



            v2f vert (appdata_base v) {

                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);

                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                return o;

            }



            fixed4 frag (v2f i) : SV_Target {

                float t = sin(_Time.y * _Speed) * 0.5 + 0.5;

                float currentLineY = lerp(_MinY, _MaxY, t);

                

                float dist = abs(i.worldPos.y - currentLineY);

                

                // Usamos um cálculo de intensidade mais agressivo para VR

                float intensity = smoothstep(_LineWidth, 0, dist);

                

                if(intensity <= 0) discard; // Economiza processamento no Quest



                fixed4 col = _ScanColor;

                col.a = intensity * _Boost; // Força o Alpha para ser bem visível

                

                return col;

            }

            ENDCG

        }

    }

}
//AlphaBlendedColorTexture.shader (Forked from AlphaBlendedColorHard)
//Created by Aaron C Gaudette on 28.07.16
//A transparent alpha-blended shader

Shader "Custom/Transparent/Alpha Blended Color Texture"{
	Properties{
		_Color("Color",Color) = (1,1,1,1)
		_MainTex("Texture",2D) = "white" {}
	}
	Category{
		Tags{"RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True"}
		
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaTest Greater .01
		Lighting Off ZWrite Off
		
		SubShader{
			Pass{
				CGPROGRAM
				
				#pragma vertex vert
				#pragma fragment frag
				
				fixed4 _Color;
				sampler2D _MainTex;
				float4 _MainTex_ST;
				
				struct VertexInput{
					half4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};
				struct VertexOutput{
					half4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
				};
				
				VertexOutput vert(VertexInput input){
					VertexOutput output;
					output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
					output.uv = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;
					return output;
				}
				fixed4 frag(VertexOutput output) : COLOR{
					return _Color * tex2D(_MainTex, output.uv).rgba;
				}
				
				ENDCG
			}
		}
	}
}
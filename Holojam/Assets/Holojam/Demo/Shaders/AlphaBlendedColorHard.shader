//AlphaBlendedColorHard.shader (Updated from AlphaBlendedColor on 21.08.14)
//Created by Aaron C Gaudette on 11.07.14
//A transparent alpha-blended shader

Shader "Custom/Transparent/Alpha Blended Color Hard"{
	Properties{
		_Color("Color",Color) = (1,1,1,1)
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
				
				struct VertexInput{
					half4 vertex : POSITION;
				};
				struct VertexOutput{
					half4 pos : SV_POSITION;
				};
				
				VertexOutput vert(VertexInput input){
					VertexOutput output;
					output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
					return output;
				}
				fixed4 frag(VertexOutput output) : COLOR{
					return _Color;
				}
				
				ENDCG
			}
		}
	}
}
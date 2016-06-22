Shader "Custom/CrayonShader" {
	Properties {
		_Detail ("Detail", 2D) = "gray" {}
		_Color ("Main Color", COLOR) = (1,1,1,1)
	}

	Category {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
	
		BindChannels {
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}
	
		SubShader {
			Pass {
				SetTexture [_Detail] {
					combine texture * Constant ConstantColor[_Color] 
				}
			}
		}
	}
}



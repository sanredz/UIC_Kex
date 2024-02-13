Shader "ShirtColor/Diffuse" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_ShirtColor ("Shirt Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_ColorMatchAlpha ("Color Match Alpha", Float) = 0.95
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 200

CGPROGRAM
#pragma surface surf Lambert

sampler2D _MainTex;
fixed4 _Color;
fixed4 _ShirtColor;
float _ColorMatchAlpha;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
//	if( c.r > .48f && c.r < .76f &&
//		c.g > .49f && c.g < .80f &&
//		c.b > .50f && c.b < .84f){
//			c *= _ShirtColor;
//
//		}
		
	if( c.r <= .50f &&
		c.g > .50f && 
		c.b <= .50f){
			c.a = 1.0f;
			c = _ShirtColor;
		}
		
//	if(c.a == _ColorMatchAlpha){
//		c *= _ShirtColor;
//	}

	//if( c.r > .86f && c.r < .90f &&
	//	c.g > .18f && c.g < .20f &&
	//	c.b > .37f && c.b < .40f){
	//		c *= _ShirtColor;
	//	}
	
	o.Albedo = c.rgb;
	o.Alpha = c.a;
	//o.Alpha = 1.0f;
}
ENDCG
}

Fallback "VertexLit"
}

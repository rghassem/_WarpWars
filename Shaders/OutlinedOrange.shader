Shader "Custom/OutlinedOrange" {

Properties {
    _Color ("Color", Color) = (1,1,1)
}

SubShader {
    Color [_Color]
    Pass {}
} 
	FallBack "Diffuse"
}




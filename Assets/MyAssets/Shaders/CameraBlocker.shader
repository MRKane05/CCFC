Shader "Custom/CameraBlocker" {
	Properties { 
	_Mask ("Culling Mask", 2D) = "white" {} 
	} 
	SubShader  { 
		Tags {"Queue" = "Background"} 
		Blend SrcAlpha OneMinusSrcAlpha 
		Lighting Off 
		ZWrite On 
		ZTest Always 
		Alphatest LEqual 0
		//this needs to be mapped to screen coords
		Pass { 
			SetTexture [_Mask] { combine texture }
		} 
	} 

}

Shader "Unlit/Transparent"
{
    SubShader{
        

        Tags {"Queue" = "Geometry-10" }

        Lighting Off

        ZTest Always
        ZWrite On

  
        ColorMask 0

        Pass {}
    }
}
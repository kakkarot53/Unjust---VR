Shader "CustomRenderTexture/Mask"
{
     SubShader
     {
         Tags{"Queue" = "Transparent+1"}
        Pass
        {
            Name "Mask"
            Blend One Zero
        }
    }
}

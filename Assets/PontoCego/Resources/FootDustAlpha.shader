Shader "PontoCego/Particula alpha pintada"
{
 Properties { [PerRendererData] _MainTex("Alpha",2D)="white"{} _Color("Cor",Color)=(1,1,1,1) }
 SubShader
 {
  Tags {"Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True"}
  Cull Off Lighting Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass
  {
   CGPROGRAM
   #pragma vertex SpriteVert
   #pragma fragment TintAlpha
   #pragma multi_compile_instancing
   #pragma multi_compile _ PIXELSNAP_ON
   #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
   #include "UnitySprites.cginc"
   fixed4 TintAlpha(v2f IN):SV_Target {fixed4 color=IN.color;color.a*=SampleSpriteTexture(IN.texcoord).a;return color;}
   ENDCG
  }
 }
}

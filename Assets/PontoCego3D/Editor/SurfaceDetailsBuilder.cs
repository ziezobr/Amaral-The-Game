using UnityEditor;using UnityEngine;
namespace PontoCego3D.Editor
{
 public static partial class DistrictBuilder
 {
  static void SurfaceDetails()
  {
   var texture=new Texture2D(128,128,TextureFormat.RGBA32,false){name="Alvenaria / reboco e juntas",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Bilinear};
   for(int y=0;y<128;y++)for(int x=0;x<128;x++){bool mortar=y%16==0||(x+(y/16%2)*16)%32==0;float noise=((x*17+y*13)%11)/160f;float v=mortar?.81f:.94f+noise;texture.SetPixel(x,y,new Color(v,v,v));}texture.Apply();string path=Gen+"/Materials/Masonry.asset";AssetDatabase.DeleteAsset(path);AssetDatabase.CreateAsset(texture,path);
   foreach(var key in new[]{"Riberal / creme","PADARIA PÃO DE MINAS","MARTINS / OFICINA","Galpão / tijolo"})if(mats.TryGetValue(key,out var material)){material.mainTexture=texture;material.mainTextureScale=new Vector2(5,2);EditorUtility.SetDirty(material);}
   foreach(var at in new[]{new Vector3(-22,0,-22),new Vector3(22,0,-22)})
   {
    var roof=Mat("Telhado cerâmico",new Color(.46f,.20f,.12f));Box("Telhado / beiral",at+Vector3.up*4.9f,new Vector3(24.4f,.18f,20.4f),roof);
    for(int i=-10;i<=10;i++)Box("Telhado / friso",at+new Vector3(i*1.1f,5.02f,0),new Vector3(.06f,.06f,20.2f),Mat("Junta do telhado",new Color(.31f,.12f,.07f)));
    Part(PrimitiveType.Cylinder,"Caixa d'água",at+new Vector3(6,5.65f,-5),new Vector3(2.2f,.65f,2.2f),Mat("Caixa d'água azul",new Color(.10f,.30f,.44f)));
    Box("Unidade de ar no telhado",at+new Vector3(-5,5.4f,-5),new Vector3(2,.7f,1.6f),Mat("Equipamentos do telhado",new Color(.55f,.58f,.56f)));
   }
  }
 }
}

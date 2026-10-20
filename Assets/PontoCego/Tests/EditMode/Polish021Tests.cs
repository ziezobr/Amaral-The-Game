using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace PontoCego.Tests
{
    public sealed class Polish021Tests
    {
        [TestCase("adryan-child-v1")][TestCase("enemy-variety-v1")]
        public void ImportedAtlasKeepsOriginalPixelGrid(string name){string path="Assets/PontoCego/UrbanArt/References/"+name+".png";var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);Assert.That(texture.width,Is.EqualTo(1536));Assert.That(texture.height,Is.EqualTo(1024));Assert.That(((TextureImporter)AssetImporter.GetAtPath(path)).npotScale,Is.EqualTo(TextureImporterNPOTScale.None));}
        [TestCase(0,-1,0)][TestCase(-1,-1,1)][TestCase(-1,0,2)][TestCase(-1,1,3)][TestCase(0,1,4)][TestCase(1,1,5)][TestCase(1,0,6)][TestCase(1,-1,7)]
        public void EightDirectionsRemainStable(float x,float y,int expected)=>Assert.That(ReferenceActorAnimation.DirectionIndex(new Vector2(x,y)),Is.EqualTo(expected));
        [Test]public void ThreeFrameCyclesHavePassingPoseBetweenStrides(){Assert.That(new[]{ReferenceActorAnimation.CycleFrame(0,3),ReferenceActorAnimation.CycleFrame(1,3),ReferenceActorAnimation.CycleFrame(2,3),ReferenceActorAnimation.CycleFrame(3,3)},Is.EqualTo(new[]{0,1,2,1}));}
        [Test]public void StaminaUpgradeIsPerMotorAndCapped(){var settings=ScriptableObject.CreateInstance<MovementSettings>();try{settings.sprintCapacity=4;var model=new MotorModel(settings);model.SetStaminaUpgrade(1);Assert.That(model.StaminaCapacity,Is.EqualTo(4.8f).Within(.001f));model.SetStaminaUpgrade(99);Assert.That(model.StaminaCapacity,Is.EqualTo(8).Within(.001f));Assert.That(settings.sprintCapacity,Is.EqualTo(4));Assert.That(model.Stamina,Is.LessThanOrEqualTo(model.StaminaCapacity));}finally{Object.DestroyImmediate(settings);}}
        [Test]public void NewGameStartsWithFiftyReaisAndExistingBalanceSurvives(){Assert.That(new CampaignSave().currency,Is.EqualTo(50));Assert.That(JsonUtility.FromJson<CampaignSave>("{\"currency\":937}").currency,Is.EqualTo(937));}
        [Test]public void OldSaveDefaultsToNoStaminaUpgrade(){var save=JsonUtility.FromJson<CampaignSave>("{\"currency\":100}");Assert.That(save.staminaLevel,Is.Zero);}
        [TestCase(.0f,FoodKind.Banana)][TestCase(.039f,FoodKind.Banana)][TestCase(.04f,FoodKind.OrangeJuice)][TestCase(.069f,FoodKind.OrangeJuice)][TestCase(.07f,FoodKind.Steak)][TestCase(.079f,FoodKind.Steak)]
        public void RareFoodTableIsDeterministic(float roll,FoodKind expected)=>Assert.That(CommonEnemyFoodDrop.FoodForRoll(roll),Is.EqualTo(expected));
        [TestCase(.08f)][TestCase(.5f)][TestCase(.999f)]public void MostCommonEnemiesHaveNoRareDrop(float roll)=>Assert.That(CommonEnemyFoodDrop.FoodForRoll(roll),Is.Null);
        [Test]public void FlashStunOpensShieldDefense(){Assert.That(EnemyVariety.ShieldMultiplier(EnemyArchetype.Armored,false),Is.EqualTo(.4f));Assert.That(EnemyVariety.ShieldMultiplier(EnemyArchetype.Armored,true),Is.EqualTo(1));}
        [Test]public void RadarProjectionIsIndependentOfScreenResolution(){var rect=new Rect(-100,-100,200,200);Assert.That(CityRadar2D.Project(Vector2.zero,rect,new Vector2(320,224)),Is.EqualTo(new Vector2(160,112)));var view=CityRadar2D.LocalView(new Rect(-110,-102,330,210),new Vector2(0,0),208f/134);Assert.That(view.Contains(Vector2.zero));Assert.That(view.width/view.height,Is.EqualTo(208f/134).Within(.001f));}
        [Test]public void RadarClipsLongOffscreenLines(){Vector2 a=new Vector2(-10000,50),b=new Vector2(10000,50);Assert.That(CityRadar2D.ClipSegment(ref a,ref b,new Rect(0,0,320,224)));Assert.That(a.x,Is.EqualTo(0).Within(.01f));Assert.That(b.x,Is.EqualTo(320).Within(.01f));a=new Vector2(-100,-100);b=new Vector2(-10,-10);Assert.That(CityRadar2D.ClipSegment(ref a,ref b,new Rect(0,0,320,224)),Is.False);}
        [Test]public void ShopDescriptionsCoverEveryItem(){for(int i=0;i<EquipmentShop.ProductCount;i++){Assert.That(EquipmentShop.ProductName(i),Is.Not.Empty);Assert.That(EquipmentShop.Description(i).Length,Is.GreaterThan(30));}Assert.That(EquipmentShop.Description(10),Does.Contain("20%"));}
    }
}

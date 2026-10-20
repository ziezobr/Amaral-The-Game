using NUnit.Framework;using UnityEngine;
namespace PontoCego.Tests
{
    public sealed class Features025Tests
    {
        [Test]public void DifficultyIncreasesLifeDamageAndPace(){Assert.That(DifficultyRules.EnemyLife(GameDifficulty.Story),Is.LessThan(DifficultyRules.EnemyLife(GameDifficulty.Normal)));Assert.That(DifficultyRules.EnemyLife(GameDifficulty.Hard),Is.GreaterThan(1));Assert.That(DifficultyRules.Incoming(GameDifficulty.Hard),Is.GreaterThan(DifficultyRules.Incoming(GameDifficulty.Story)));Assert.That(DifficultyRules.EnemyPace(GameDifficulty.Hard),Is.GreaterThan(DifficultyRules.EnemyPace(GameDifficulty.Normal)));}
        [Test]public void NormalRetainsAuthoredBalance(){Assert.That(DifficultyRules.EnemyLife(GameDifficulty.Normal),Is.EqualTo(1));Assert.That(DifficultyRules.Incoming(GameDifficulty.Normal),Is.EqualTo(1));Assert.That(new CampaignSave().difficulty,Is.EqualTo(1));}
        [Test]public void MapCoordinatesInvertGuiYAndClamp(){var bounds=new Rect(-110,-102,330,210);Assert.That(MapWaypoint.FromMap(Vector2.zero,bounds),Is.EqualTo(new Vector2(-110,108)));Assert.That(MapWaypoint.FromMap(Vector2.one,bounds),Is.EqualTo(new Vector2(220,-102)));Assert.That(MapWaypoint.FromMap(new Vector2(-3,2),bounds),Is.EqualTo(bounds.min));}
        [Test]public void UpdatesRequireNewerVersionAndRejectZipTraversal(){Assert.That(GameAutoUpdater.Newer("v0.25.0","0.24.0"),Is.True);Assert.That(GameAutoUpdater.Newer("v0.24.0","0.24.0"),Is.False);Assert.That(GameAutoUpdater.Newer("v0.9.0","0.24.0"),Is.False);Assert.Throws<System.IO.InvalidDataException>(()=>GameAutoUpdater.SafeEntry("C:/UpdateStage","../../Windows/bad.exe"));}
    }
}

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace PontoCego.Editor
{
    public static class BuildProject
    {
        [MenuItem("Ponto Cego/1 • Gerar fases urbanas")]
        public static void Build()=>UrbanSceneBuilder.Build();
        [MenuItem("Ponto Cego/2 • Build Windows")]
        public static void BuildWindows()
        {
            var output = System.Environment.GetEnvironmentVariable("PONTO_RELEASE_OUTPUT") ?? "Builds/Demo0250";
            Directory.CreateDirectory(output);
            PlayerSettings.bundleVersion="0.25.0";
            EditorBuildSettings.scenes=EditorBuildSettings.scenes.OrderBy(s=>s.path.EndsWith("00_Menu.unity")?0:1).ToArray();
            Debug.Log("BUILD_START_SCENE: "+EditorBuildSettings.scenes.First(s=>s.enabled).path);
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled&&s.path.StartsWith(UrbanSceneBuilder.Root+"/Scenes/")).Select(s=>s.path).OrderBy(p=>p.EndsWith("00_Menu.unity")?0:1).ToArray(),
                locationPathName=Path.Combine(output,"PontoCego.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Windows build failed: "+report.summary.result);
            File.Copy("Packaging/PontoCegoUpdater.exe",Path.Combine(output,"PontoCegoUpdater.exe"),true);
            Debug.Log("PONTO_CEGO_WINDOWS_BUILD_OK");
        }
        public static void BuildAll(){Build();BuildWindows();}
        public static void OpenForUser()
        {
            void Open()
            {
                EditorSceneManager.OpenScene(UrbanSceneBuilder.Root+"/Scenes/00_Menu.unity");
                Selection.activeGameObject=GameObject.Find("Amaral");
                if(SceneView.lastActiveSceneView){SceneView.lastActiveSceneView.in2DMode=true;SceneView.lastActiveSceneView.Frame(new Bounds(Vector3.zero,new Vector3(36,24,1)),true);}
                EditorApplication.ExecuteMenuItem("Window/General/Game");Debug.Log("PONTO_CEGO_URBAN_READY");
            }
            Open();
        }
    }
}

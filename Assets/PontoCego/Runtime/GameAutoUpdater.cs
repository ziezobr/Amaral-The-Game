using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;
namespace PontoCego
{
    /// <summary>Release updates run only in the menu. Verify, safely stage, exit, apply and restart.</summary>
    public sealed class GameAutoUpdater:MonoBehaviour
    {
        const string Api="https://api.github.com/repos/ziezobr/Amaral-The-Game/releases/latest";
        const string Prefix="https://github.com/ziezobr/Amaral-The-Game/releases/download/";
        [Serializable]sealed class Asset{public string name,browser_download_url;}
        [Serializable]sealed class Release{public string tag_name;public Asset[] assets;}
        public string Status{get;private set;}="Atualizador pronto.";
        public bool Busy{get;private set;}public bool Ready{get;private set;}public float Progress{get;private set;}
        string packageRoot,targetRoot;bool postponed;float readyAt;UnityWebRequest active;
        public static bool Newer(string remote,string current)=>Version.TryParse(remote.TrimStart('v','V'),out var r)&&Version.TryParse(current.TrimStart('v','V'),out var c)&&r>c;
        public static string SafeEntry(string root,string entry)
        {
            string prefix=Path.GetFullPath(root)+Path.DirectorySeparatorChar;
            string path=Path.GetFullPath(Path.Combine(root,entry));
            if(!path.StartsWith(prefix,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Caminho inválido no pacote.");return path;
        }
        bool Automatic{get=>PlayerPrefs.GetInt("PontoCego.AutoUpdate",1)==1;set{PlayerPrefs.SetInt("PontoCego.AutoUpdate",value?1:0);PlayerPrefs.Save();}}
        IEnumerator Start(){if(Application.isBatchMode||!GameSession.CanPersist)yield break;yield return new WaitForSecondsRealtime(2);StartCoroutine(Check());}
        void Update(){if(active!=null)Progress=active.downloadProgress;if(Ready&&Automatic&&!postponed&&Time.unscaledTime-readyAt>8)Apply();}
        public void CheckNow(){if(!Busy&&!Ready)StartCoroutine(Check());}
        IEnumerator Check()
        {
            Busy=true;Status="Verificando atualizações...";Release release=null;
            using(var request=UnityWebRequest.Get(Api))
            {
                request.SetRequestHeader("User-Agent","Amaral-The-Game-Updater");request.timeout=25;yield return request.SendWebRequest();
                if(request.result!=UnityWebRequest.Result.Success){Status="Sem conexão com o GitHub. Você pode jogar normalmente.";Busy=false;yield break;}
                try{release=JsonUtility.FromJson<Release>(request.downloadHandler.text);}catch{}
            }
            if(release==null||!Newer(release.tag_name,Application.version)){Status="Versão "+Application.version+" atualizada.";Busy=false;yield break;}
            var zip=release.assets?.FirstOrDefault(a=>a.name.EndsWith("Windows-Portable.zip",StringComparison.Ordinal));
            var sums=release.assets?.FirstOrDefault(a=>a.name=="SHA256SUMS.txt");
            if(zip==null||sums==null||!zip.browser_download_url.StartsWith(Prefix,StringComparison.Ordinal)||!sums.browser_download_url.StartsWith(Prefix,StringComparison.Ordinal)){Status="Pacote de atualização ainda não disponível.";Busy=false;yield break;}
            string expected=null;
            using(var request=UnityWebRequest.Get(sums.browser_download_url))
            {
                request.timeout=25;yield return request.SendWebRequest();
                if(request.result==UnityWebRequest.Result.Success){var line=request.downloadHandler.text.Split('\n').FirstOrDefault(l=>l.TrimEnd().EndsWith(zip.name,StringComparison.Ordinal));if(line!=null)expected=line.Split(new[]{' ','\t'},StringSplitOptions.RemoveEmptyEntries)[0].ToLowerInvariant();}
            }
            if(expected==null||expected.Length!=64){Status="Não foi possível validar o pacote.";Busy=false;yield break;}
            string cache=Path.Combine(Application.temporaryCachePath,"update-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(cache);
            string archive=Path.Combine(cache,"game.zip");Status="Baixando "+release.tag_name+"...";
            using(var request=new UnityWebRequest(zip.browser_download_url,UnityWebRequest.kHttpVerbGET))
            {
                request.downloadHandler=new DownloadHandlerFile(archive);request.timeout=300;active=request;yield return request.SendWebRequest();active=null;
                if(request.result!=UnityWebRequest.Result.Success){Status="Download interrompido. Tente novamente no menu.";Busy=false;yield break;}
            }
            try
            {
                string actual;using(var hash=SHA256.Create())using(var stream=File.OpenRead(archive))actual=BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","").ToLowerInvariant();
                if(actual!=expected)throw new InvalidDataException("Checksum diferente.");
                string stage=Path.Combine(cache,"payload");Directory.CreateDirectory(stage);
                using(var file=ZipFile.OpenRead(archive))foreach(var entry in file.Entries){if(string.IsNullOrEmpty(entry.Name))continue;string output=SafeEntry(stage,entry.FullName);Directory.CreateDirectory(Path.GetDirectoryName(output));entry.ExtractToFile(output);}
                string exe=Directory.GetFiles(stage,"PontoCego.exe",SearchOption.AllDirectories).Single();packageRoot=Path.GetDirectoryName(exe);targetRoot=Directory.GetParent(Application.dataPath).FullName;
                if(!File.Exists(Path.Combine(packageRoot,"UnityPlayer.dll")))throw new InvalidDataException("Pacote incompleto.");
                Ready=true;readyAt=Time.unscaledTime;Status="Atualização pronta. O jogo reiniciará em 8 segundos.";
            }
            catch(Exception error){Status="Atualização não aplicada: "+error.Message;}
            Busy=false;
        }
        public void Apply()
        {
            if(!Ready||!FindAnyObjectByType<MainMenu>())return;
            try
            {
                string runner=Path.Combine(Path.GetDirectoryName(packageRoot),"PontoCegoUpdaterRunner.exe");
                File.Copy(Path.Combine(targetRoot,"PontoCegoUpdater.exe"),runner,true);
                string Enc(string value)=>Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value));
                string args=Enc(packageRoot)+" "+Enc(targetRoot)+" "+System.Diagnostics.Process.GetCurrentProcess().Id;
                PlayerPrefs.Save();System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(runner,args){UseShellExecute=false,CreateNoWindow=true});Application.Quit();
            }
            catch(Exception error){postponed=true;Status="Não foi possível reiniciar: "+error.Message;}
        }
        void OnGUI()
        {
            if(Application.isBatchMode||!GameSession.CanPersist)return;var old=GUI.matrix;GUI.matrix=UiTheme.SafeMatrix();CleanGui.Round(new Rect(18,590,420,94),UiTheme.Surface);
            GUI.Label(new Rect(30,598,395,37),Status+(Busy&&Progress>0?" "+Mathf.RoundToInt(Progress*100)+"%":""),UiTheme.Label(12,Color.white));
            if(GUI.Button(new Rect(30,642,188,30),Ready?"ATUALIZAR AGORA":"VERIFICAR",GamePhoneView.ButtonStyle)){if(Ready)Apply();else CheckNow();}
            if(GUI.Button(new Rect(228,642,198,30),Ready?"DEPOIS":Automatic?"AUTOMÁTICO: SIM":"AUTOMÁTICO: NÃO",GamePhoneView.ButtonStyle)){if(Ready){postponed=true;Status="Atualização pronta para aplicar pelo menu.";}else Automatic=!Automatic;}
            GUI.matrix=old;
        }
        void OnDestroy(){active?.Abort();}
    }
}

using System;
using System.IO;
using System.Diagnostics;
using System.Text;
internal static class UpdaterHelper
{
    static void CopyTree(string source,string target)
    {
        foreach(string file in Directory.GetFiles(source,"*",SearchOption.AllDirectories))
        {
            string relative=file.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar);
            string destination=Path.GetFullPath(Path.Combine(target,relative));
            if(!destination.StartsWith(Path.GetFullPath(target)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException();
            Directory.CreateDirectory(Path.GetDirectoryName(destination));File.Copy(file,destination,true);
        }
    }
    static int Main(string[] args)
    {
        if(args.Length<3)return 2;
        string source=Encoding.UTF8.GetString(Convert.FromBase64String(args[0]));
        string target=Encoding.UTF8.GetString(Convert.FromBase64String(args[1]));
        string backup=Path.Combine(Path.GetDirectoryName(source),"previous-version");
        try
        {
            if(!Directory.Exists(source)||!File.Exists(Path.Combine(source,"PontoCego.exe"))||!File.Exists(Path.Combine(target,"PontoCego.exe")))return 3;
            try{using(var game=Process.GetProcessById(int.Parse(args[2])))if(!game.WaitForExit(60000))return 4;}catch(ArgumentException){}
            Directory.CreateDirectory(backup);
            foreach(string file in Directory.GetFiles(source,"*",SearchOption.AllDirectories))
            {
                string relative=file.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar);string previous=Path.Combine(target,relative);
                if(File.Exists(previous)){string destination=Path.Combine(backup,relative);Directory.CreateDirectory(Path.GetDirectoryName(destination));File.Copy(previous,destination,true);}
            }
            CopyTree(source,target);
            if(args.Length<4||args[3]!="--no-restart")Process.Start(new ProcessStartInfo(Path.Combine(target,"PontoCego.exe")){WorkingDirectory=target,UseShellExecute=true});
            return 0;
        }
        catch(Exception error)
        {
            try{if(Directory.Exists(backup))CopyTree(backup,target);File.WriteAllText(Path.Combine(backup,"update-error.txt"),error.ToString());}catch{}
            return 1;
        }
    }
}

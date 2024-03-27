using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using UnityEditor;
using Debug = UnityEngine.Debug;

public class ScriptBatch
{
    private static string _directory = "/home/over/Projects/WebGame/RPO";
    private static string _serverDirectory = _directory + "/Server/server";
    private static string _webGlDirectory = _directory + "/webgl";
    private static string _user = "root";
    private static string _ip = "147.45.104.194";
    
    
    [MenuItem("MyTools/Server/Restart",false, 1)]
    public static void RestartServer()
    {
        ProcessStartInfo psi = new ProcessStartInfo("ssh", $"{_user}@{_ip} \"screen -S server -p 0 -X stuff '^C ./server\\n'\"")
        {
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = psi })
        {
            process.Start();
            process.WaitForExit();
            Debug.Log($"Server start");
        }
        
        psi = new ProcessStartInfo("ssh", $"{_user}@{_ip} \"screen -S web -p 0 -X stuff '^C docker compose up --build\\n'\"")
        {
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = psi })
        {
            process.Start();
            process.WaitForExit();
            Debug.Log($"Web start");
        }
    }
    
    
    [MenuItem("MyTools/Build/All", false, 1)]
    public static void BuildAllGame()
    {
        BuildWebGlGame();
        BuildServerGame();
    }
    
    [MenuItem("MyTools/Build/WebGl")]
    public static void BuildWebGlGame()
    {
        string[] levels = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
        BuildPipeline.BuildPlayer(levels, _webGlDirectory, BuildTarget.WebGL, BuildOptions.None);
    }
    
    [MenuItem("MyTools/Build/Server")]
    public static void BuildServerGame()
    {
        string[] levels = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
        BuildPipeline.BuildPlayer(levels, _serverDirectory, BuildTarget.StandaloneLinux64, BuildOptions.EnableHeadlessMode);
    }
        
    [MenuItem("MyTools/Upload/All",false, 1)]
    public static void UploadAllGame()
    {
        UploadServerGame();
        UploadWebGlGame();
    }
    
    [MenuItem("MyTools/Upload/Server")]
    public static void UploadServerGame()
    {
        Directory.SetCurrentDirectory(Path.Combine(_serverDirectory, ".."));
        string sftpCommand = $"put -r {Directory.GetCurrentDirectory()}";

        ProcessStartInfo psi = new ProcessStartInfo("sftp", $"{_user}@{_ip}")
        {
            RedirectStandardInput = true,
            UseShellExecute = false,
            // CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = psi })
        {
            process.Start();

            using (StreamWriter sw = process.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    Debug.Log("Sending SFTP command...");
                    sw.WriteLine(sftpCommand);
                    sw.WriteLine("exit");
                    Debug.Log("SFTP command sent.");
                }
            }

            process.WaitForExit();
            Debug.Log($"SFTP process exited with code: {process.ExitCode}");
        }
    }
    
    [MenuItem("MyTools/Upload/WebGl")]
    public static void UploadWebGlGame()
    {
        string sftpCommand = $"put -r {_webGlDirectory} /{_user}/unity-docker-example";

        ProcessStartInfo psi = new ProcessStartInfo("sftp", $"{_user }@{_ip}")
        {
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = psi })
        {
            process.Start();

            using (StreamWriter sw = process.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    Debug.Log("Sending SFTP command...");
                    sw.WriteLine(sftpCommand);
                    sw.WriteLine("exit");
                    Debug.Log("SFTP command sent.");
                }
            }

            process.WaitForExit();
            Debug.Log($"SFTP process exited with code: {process.ExitCode}");
        }
    }

    [MenuItem("MyTools/Build&Upload",false, 1)]
    public static void BuildUploadGame()
    {
        BuildAllGame();
        UploadAllGame();
        RestartServer();
    }
}
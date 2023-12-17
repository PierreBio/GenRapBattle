using UnityEngine;
using System.Diagnostics;

public class PythonServerController : MonoBehaviour
{
    // Le chemin vers votre excutable Python (remplacez 'python' si ncessaire)
    private static string pythonExecutable = Application.streamingAssetsPath + "/PythonInstall/python.exe";

    // Le chemin complet vers le fichier server.py
    public string pythonScriptPath = Application.streamingAssetsPath + "/server.py";

    // Le chemin complet vers le fichier model_letters.h5
    public string finalModelPath = Application.streamingAssetsPath + "/model_rap.pt";

    string arguments; // Encadrer le chemin avec des guillemets

    // Stocke la rfrence du processus du serveur Python pour pouvoir l'arrter plus tard si ncessaire
    private Process pythonProcess;

    [SerializeField] private bool displayConsole = true;
    [SerializeField] private ConsoleToGUI buildLogger;

    void Start()
    {
        arguments = "\"" + finalModelPath + "\""; // Encadrer le chemin avec des guillemets

        // Dmarrer le serveur Python dans un processus spar
        StartPythonServer();
    }

    void OnApplicationQuit()
    {
        // Arrter le serveur Python lorsque l'application Unity se ferme
        StopPythonServer();
    }

    void StartPythonServer()
    {
        UnityEngine.Debug.Log("Lancement du processus");
        if (displayConsole) buildLogger.Log(pythonExecutable, "", LogType.Log);
        if (displayConsole) buildLogger.Log(pythonScriptPath, "", LogType.Log);

        // Configurer le processus
        ProcessStartInfo startInfo = new ProcessStartInfo(pythonExecutable, "\"" + pythonScriptPath + "\" " + arguments);

        UnityEngine.Debug.Log("Nouveau process lanc");

        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardInput = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true; // Ajoutez cette ligne pour rediriger la sortie d'erreur (stderr) vers Unity
        startInfo.CreateNoWindow = true;

        // Dmarrer le processus
        pythonProcess = new Process();
        pythonProcess.StartInfo = startInfo;
        pythonProcess.OutputDataReceived += new DataReceivedEventHandler(OnPythonOutput);
        pythonProcess.ErrorDataReceived += new DataReceivedEventHandler(OnPythonError); // Grer la sortie d'erreur
        pythonProcess.Start();
        pythonProcess.BeginOutputReadLine();
        pythonProcess.BeginErrorReadLine(); // Commencer  lire les sorties d'erreur
    }

    void OnPythonOutput(object sender, DataReceivedEventArgs e)
    {
        // Grer la sortie standard (stdout) du processus Python
        if (!string.IsNullOrEmpty(e.Data))
        {
            UnityEngine.Debug.Log("Python Output: " + e.Data);
            if (displayConsole) buildLogger.Log(e.Data, "", LogType.Log);
        }
    }

    void OnPythonError(object sender, DataReceivedEventArgs e)
    {
        // Grer la sortie d'erreur (stderr) du processus Python
        if (!string.IsNullOrEmpty(e.Data))
        {
            UnityEngine.Debug.LogWarning("Python Error: " + e.Data);
            if (displayConsole) buildLogger.Log(e.Data, "", LogType.Log);
        }
    }


    void StopPythonServer()
    {
        // Arrter le processus Python s'il est en cours d'excution
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            pythonProcess.StandardInput.WriteLine("exit"); // Envoyer 'exit' pour terminer proprement le script Python
            pythonProcess.WaitForExit(1000); // Attendre au maximum 1 seconde que le processus se termine
            if (!pythonProcess.HasExited)
            {
                pythonProcess.Kill(); // Forcer l'arrt du processus Python s'il n'a pas rpondu  l'arrt normal
            }
            pythonProcess.Close();
            pythonProcess.Dispose();
        }
    }
}
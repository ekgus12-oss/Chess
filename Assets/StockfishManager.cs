using UnityEngine;
using System.Diagnostics;
using System.IO;

public class StockfishManager : MonoBehaviour
{
    private Process stockfishProcess;
    private StreamWriter sw;
    private StreamReader sr;

    public string stockfishPath = "Assets/StreamingAssets/stockfish.exe";

    void Start() { StartStockfish(); }

    void StartStockfish()
    {
        try
        {
            stockfishProcess = new Process();
            stockfishProcess.StartInfo.FileName = stockfishPath;
            stockfishProcess.StartInfo.UseShellExecute = false;
            stockfishProcess.StartInfo.RedirectStandardInput = true;
            stockfishProcess.StartInfo.RedirectStandardOutput = true;
            stockfishProcess.StartInfo.CreateNoWindow = true;
            stockfishProcess.Start();

            sw = stockfishProcess.StandardInput;
            sr = stockfishProcess.StandardOutput;

            SendUCIMessage("uci");
            SendUCIMessage("isready");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("스톡피시 실행 실패: " + e.Message);
        }
    }

    // [중요] 4가지 난이도 설정 로직
    public void SetDifficultyCustom(string mode)
    {
        switch (mode)
        {
            case "Teacher": // 친절한 선생님 (낮은 실력, 실수 유발)
                SendUCIMessage("setoption name Skill Level value 0");
                break;
            case "Defender": // 견고한 수비 (높은 실력, 무승부 지향)
                SendUCIMessage("setoption name Skill Level value 15");
                SendUCIMessage("setoption name Contempt value -20"); // 소극적
                break;
            case "Attacker": // 공격적인 (가끔 실수하지만 몰아붙임)
                SendUCIMessage("setoption name Skill Level value 10");
                SendUCIMessage("setoption name Contempt value 50"); // 공격적
                break;
            case "Balance": // 밸런스형 (표준적인 실력)
                SendUCIMessage("setoption name Skill Level value 5");
                SendUCIMessage("setoption name Contempt value 0");
                break;
        }
    }

    public void SendUCIMessage(string message)
    {
        if (stockfishProcess != null && !stockfishProcess.HasExited)
        {
            sw.WriteLine(message);
            sw.Flush();
        }
    }

    public string GetBestMove(string fen)
    {
        sw.WriteLine("position fen " + fen);
        sw.WriteLine("go movetime 1000");
        sw.Flush();

        string line;
        while ((line = sr.ReadLine()) != null)
        {
            if (line.StartsWith("bestmove"))
            {
                string[] parts = line.Split(' ');
                return parts.Length >= 2 ? parts[1] : null;
            }
        }
        return null;
    }

    void OnApplicationQuit() { if (stockfishProcess != null) stockfishProcess.Kill(); }
}
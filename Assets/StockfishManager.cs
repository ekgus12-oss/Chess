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
    // StockfishManager.cs 안에 추가하세요

    public float GetEvaluation(string fen)
    {
        if (stockfishProcess == null || stockfishProcess.HasExited) return 0;

        // 스톡피쉬에게 현재 FEN 상황을 분석하라고 명령 (깊이 10정도면 충분히 빠르고 정확함)
        stockfishProcess.StandardInput.WriteLine("position fen " + fen);
        stockfishProcess.StandardInput.WriteLine("go depth 10");

        string line;
        float evalScore = 0;

        // 스톡피쉬의 출력 메시지 중에서 score cp를 찾음
        while (!(line = stockfishProcess.StandardOutput.ReadLine()).StartsWith("bestmove"))
        {
            if (line.Contains("score cp"))
            {
                // "score cp 15" 같은 문자열에서 15만 추출
                string[] parts = line.Split(' ');
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i] == "cp")
                    {
                        float.TryParse(parts[i + 1], out evalScore);
                        break;
                    }
                }
            }
        }
        return evalScore;
    }
}
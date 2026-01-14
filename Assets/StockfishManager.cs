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

    // [추가] ChatManager에서 봇의 멘탈에 따라 호출할 함수
    public void SetSkillLevel(int level)
    {
        // 스톡피시 Skill Level 공식 범위는 0~20입니다.
        level = Mathf.Clamp(level, 0, 20);
        SendUCIMessage($"setoption name Skill Level value {level}");

        // 낮은 레벨일 때 더 인간적인 실수를 유도하기 위해 노이즈 추가 (선택사항)
        if (level < 5)
        {
            SendUCIMessage("setoption name Skill Level Maximum Error 500");
            SendUCIMessage("setoption name Skill Level Probability 100");
        }
    }

    // 기존의 커스텀 난이도 설정
    public void SetDifficultyCustom(string mode)
    {
        switch (mode)
        {
            case "Teacher":
                SetSkillLevel(0);
                break;
            case "Defender":
                SetSkillLevel(15);
                SendUCIMessage("setoption name Contempt value -20");
                break;
            case "Attacker":
                SetSkillLevel(10);
                SendUCIMessage("setoption name Contempt value 50");
                break;
            case "Balance":
                SetSkillLevel(5);
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
        SendUCIMessage("position fen " + fen);
        SendUCIMessage("go movetime 4000");

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

    public float GetEvaluation(string fen)
    {
        if (stockfishProcess == null || stockfishProcess.HasExited) return 0;

        SendUCIMessage("position fen " + fen);
        SendUCIMessage("go depth 10");

        string line;
        float evalScore = 0;

        // "bestmove"가 나올 때까지 출력물을 읽어 분석 점수를 찾습니다.
        while ((line = sr.ReadLine()) != null && !line.StartsWith("bestmove"))
        {
            if (line.Contains("score cp"))
            {
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

    void OnApplicationQuit()
    {
        if (stockfishProcess != null && !stockfishProcess.HasExited)
            stockfishProcess.Kill();
    }
}
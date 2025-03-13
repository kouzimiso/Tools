using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        // フォルダと設定を定義
        string helpFolder = "help";
        string[] targetFolders = { "folder1", "folder2", "folder3" };
        Dictionary<string, object> pathMatchSetting = new Dictionary<string, object>
        {
            { "filter", "*.ini" },
            { "ignore_string", new List<string> { "AJCD+", "DJCD+" } }
        };

        // フォルダを比較
        var result = CompareFolders(pathMatchSetting, helpFolder, targetFolders);
        
        // 結果をCSVに保存
        File.WriteAllLines("result.csv", result);
        
        // 終了メッセージ
        Console.WriteLine("比較処理が完了しました。result.csv を確認してください。");
    }

    static List<string> CompareFolders(Dictionary<string, object> settings, string helpFolder, string[] folders)
    {
        var result = new List<string>();
        var fileGroups = GetMatchingFiles(settings, folders);
        
        foreach (var group in fileGroups)
        {
            var helpFile = Path.Combine(helpFolder, group.Key + ".csv");
            var helpData = File.Exists(helpFile) ? ParseCsv(helpFile) : new Dictionary<string, string>();
            result.AddRange(CompareFiles(group.Value, helpData));
        }
        return result;
    }

    static Dictionary<string, List<string>> GetMatchingFiles(Dictionary<string, object> settings, string[] folders)
    {
        var fileGroups = new Dictionary<string, List<string>>();
        string filter = settings.ContainsKey("filter") ? settings["filter"].ToString() : "*.*";
        var ignoreList = settings.ContainsKey("ignore_string") ? (List<string>)settings["ignore_string"] : new List<string>();

        foreach (var folder in folders)
        {
            if (!Directory.Exists(folder)) continue;
            var files = Directory.GetFiles(folder, filter).Where(f => !ignoreList.Any(i => f.Contains(i)));
            foreach (var file in files)
            {
                string name = Path.GetFileName(file);
                if (!fileGroups.ContainsKey(name))
                    fileGroups[name] = new List<string>();
                fileGroups[name].Add(file);
            }
        }
        return fileGroups;
    }

    static List<string> CompareFiles(List<string> files, Dictionary<string, string> helpData)
    {
        var result = new List<string>();
        var data = files.Select(ParseIniFile).ToList();
        var allKeys = data.SelectMany(d => d.Keys).Distinct();

        foreach (var key in allKeys)
        {
            var values = data.Select(d => d.ContainsKey(key) ? d[key] : "N/A").ToArray();
            string helpText = helpData.ContainsKey(key) ? helpData[key] : "";
            result.Add(key+","+helpText+","+string.Join(",", values)+"}");
        }
        return result;
    }

    static Dictionary<string, string> ParseIniFile(string path)
    {
        var result = new Dictionary<string, string>();
        foreach (var line in File.ReadLines(path))
        {
            if (line.StartsWith(";") || !line.Contains("=")) continue;
            var parts = line.Split(new[] { '=' }, 2);
            result[parts[0].Trim()] = parts[1].Trim();
        }
        return result;
    }

    static Dictionary<string, string> ParseCsv(string path)
    {
        var result = new Dictionary<string, string>();
        foreach (var line in File.ReadLines(path).Skip(1))
        {
            var parts = line.Split(',');
            if (parts.Length < 3) continue;
            result[parts[1].Trim()] = parts[2].Trim();
        }
        return result;
    }
}

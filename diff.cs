using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ConfigDiff
{
    public static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: ConfigDiff <help_folder> <config_folder1> <config_folder2> ...");
            return;
        }

        string helpFolder = args[0];
        string[] configFolders = args.Skip(1).ToArray();

        List<DiffResult> results = CompareConfigs(helpFolder, configFolders);

        WriteResultsToCsv("result.csv", results);
    }

    public static List<DiffResult> CompareConfigs(string helpFolder, string[] configFolders)
    {
        List<DiffResult> results = new List<DiffResult>();

        var allConfigFiles = configFolders.SelectMany(folder => Directory.GetFiles(folder)).Select(Path.GetFileName).Distinct();

        foreach (var configFile in allConfigFiles)
        {
            string helpFilePath = Path.Combine(helpFolder, configFile + ".csv");
            Dictionary<string, HelpItem> helpData = LoadHelpData(helpFilePath);

            Dictionary<string, Dictionary<string, List<string>>>[] configData = configFolders
                .Select(folder => LoadConfigData(Path.Combine(folder, configFile)))
                .ToArray();

            results.AddRange(CompareFiles(configFile, helpData, configData));
        }

        return results;
    }

    public static Dictionary<string, HelpItem> LoadHelpData(string filePath)
    {
        Dictionary<string, HelpItem> helpData = new Dictionary<string, HelpItem>();

        if (!File.Exists(filePath))
        {
            return helpData;
        }

        foreach (string line in File.ReadLines(filePath).Skip(1))
        {
            string[] parts = line.Split(',');
            if (parts.Length >= 4)
            {
                string group = parts[0].Trim('"');
                string key = parts[1].Trim('"');
                string help = parts[2].Trim('"');
                string defaultValue = parts[3].Trim('"');

                helpData[GetKey(group, key)] = new HelpItem(group, key, help, defaultValue);
            }
        }

        return helpData;
    }

    public static Dictionary<string, Dictionary<string, List<string>>> LoadConfigData(string filePath)
    {
        Dictionary<string, Dictionary<string, List<string>>> configData = new Dictionary<string, Dictionary<string, List<string>>>();
        string currentGroup = "";

        if (!File.Exists(filePath))
        {
            return configData;
        }

        foreach (string line in File.ReadLines(filePath))
        {
            string trimmedLine = line.Trim();
            if (trimmedLine.StartsWith(";"))
            {
                continue;
            }

            if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
            {
                currentGroup = trimmedLine.Substring(1, trimmedLine.Length - 2);
                if (!configData.ContainsKey(currentGroup))
                {
                    configData[currentGroup] = new Dictionary<string, List<string>>();
                }
                continue;
            }

            string[] parts = trimmedLine.Split('=', 2);
            if (parts.Length == 2)
            {
                string key = parts[0].Trim();
                string value = parts[1].Trim();

                if (!configData.ContainsKey(currentGroup))
                {
                    configData[currentGroup] = new Dictionary<string, List<string>>();
                }

                if (!configData[currentGroup].ContainsKey(key))
                {
                    configData[currentGroup][key] = new List<string>();
                }
                configData[currentGroup][key].Add(value);
            }
        }

        return configData;
    }

    public static List<DiffResult> CompareFiles(string fileName, Dictionary<string, HelpItem> helpData, Dictionary<string, Dictionary<string, List<string>>>[] configData)
    {
        List<DiffResult> results = new List<DiffResult>();

        var allKeys = configData.SelectMany(data => data.Keys.SelectMany(group => data[group].Keys).Select(key => new { Group = data.Keys.FirstOrDefault(g => data[g].ContainsKey(key)), Key = key })).Distinct();

        foreach (var item in allKeys)
        {
            string group = item.Group ?? "";
            string key = item.Key;
            string fullKey = GetKey(group, key);

            HelpItem helpItem = helpData.GetValueOrDefault(fullKey);

            List<List<string>> values = configData.Select(data => data.GetValueOrDefault(group)?.GetValueOrDefault(key) ?? new List<string>()).ToList();
            bool hasDiff = !values.Skip(1).All(v => v.SequenceEqual(values[0]));

            results.Add(new DiffResult(fileName, group, key, helpItem?.Help, hasDiff, helpItem?.DefaultValue, values));
        }

        for (int i = 0; i < configData.Length; i++)
        {
            if (configData[i].Count == 0)
            {
                results.Add(new DiffResult(fileName, "", "", "設定ファイルが存在しません", true, "", new List<List<string>>()));
            }
        }

        return results;
    }

    public static void WriteResultsToCsv(string filePath, List<DiffResult> results)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("Group,Item,Help,Diff,Default,Value1,Value2,Value3");

            foreach (var result in results)
            {
                string values = string.Join(",", result.Values.Select(v => string.Join(";", v)));
                writer.WriteLine("result.Group+","+result.Key+","+result.Help+","+result.HasDiff+","+result.DefaultValue+","+values);
            }
        }
    }

    public static string GetKey(string group, string key)
    {
        return string.IsNullOrEmpty(group) ? key : group+"."+key;
    }

    public class HelpItem
    {
        public string Group { get; }
        public string Key { get; }
        public string Help { get; }
        public string DefaultValue { get; }

        public HelpItem(string group, string key, string help, string defaultValue)
        {
            Group = group;
            Key = key;
            Help = help;
            DefaultValue = defaultValue;
        }
    }

    public class DiffResult
    {
        public string FileName { get; }
        public string Group { get; }
        public string Key { get; }
        public string Help { get; }
        public bool HasDiff { get; }
        public string DefaultValue { get; }
        public List<List<string>> Values { get; }

        public DiffResult(string fileName, string group, string key, string help, bool hasDiff, string defaultValue, List<List<string>> values)
        {
            FileName = fileName;
            Group = group;
            Key = key;
            Help = help;
            HasDiff = hasDiff;
            DefaultValue = defaultValue;
            Values = values;
        }
    }
}
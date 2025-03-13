using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("フォルダパスを指定してください。");
            return;
        }

        string folderPath = args[0];
        string checkEndSearchWord = args.Length > 1 ? args[1] : null;

        // 指定されたフォルダ内のファイルを検索
        List<string> filesWithDuplicates = SearchFilesWithDuplicateCharacters(folderPath, checkEndSearchWord);

        if (filesWithDuplicates.Count > 0)
        {
            Console.WriteLine("重複が見つかったファイル:");
            foreach (var file in filesWithDuplicates)
            {
                Console.WriteLine(file);
            }

            Console.WriteLine("重複を削除しますか？ (y/n)");
            string userInput = Console.ReadLine();
            if (userInput.ToLower() == "y")
            {
                foreach (var file in filesWithDuplicates)
                {
                    RemoveDuplicateCharactersFromFile(file, checkEndSearchWord);
                    Console.WriteLine(file+"の重複が削除されました。");
                }
            }
            else
            {
                Console.WriteLine("操作はキャンセルされました。");
            }
        }
        else
        {
            Console.WriteLine("重複する文字列が見つかりませんでした。");
        }
    }

    static List<string> SearchFilesWithDuplicateCharacters(string folderPath, string checkEndSearchWord)
    {
        List<string> filesWithDuplicates = new List<string>();
        foreach (string filePath in Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories))
        {
            // UTF-8 BOMでファイルを読み込む
            string content = File.ReadAllText(filePath, Encoding.UTF8);

            // checkEndSearchWordが指定されている場合は、その部分までの範囲を抽出
            string contentToCheck = string.IsNullOrEmpty(checkEndSearchWord) ? content : GetContentUpToWord(content, checkEndSearchWord);

            // contentToCheckがnullでないかをチェック
            if (!string.IsNullOrEmpty(contentToCheck) && HasDuplicateCharacters(contentToCheck))
            {
                filesWithDuplicates.Add(filePath);
            }
        }
        return filesWithDuplicates;
    }

    static string GetContentUpToWord(string content, string checkEndSearchWord)
    {
        int endIndex = content.IndexOf(checkEndSearchWord);
        if (endIndex == -1) return null;
        return content.Substring(0, endIndex);
    }

    static bool HasDuplicateCharacters(string content)
    {
        // 重複する文字列を正規表現でチェック
        return Regex.IsMatch(content, @"(・ｿ)\1{1,}");
    }

    static void RemoveDuplicateCharactersFromFile(string filePath, string checkEndSearchWord)
    {
        string content = File.ReadAllText(filePath, Encoding.UTF8);

        string contentToCheck = string.IsNullOrEmpty(checkEndSearchWord) ? content : GetContentUpToWord(content, checkEndSearchWord);

        if (contentToCheck == null)
        {
            Console.WriteLine(filePath+ "には終了文字が見つかりませんでした。スキップします。");
            return;
        }

        // 重複する文字列を削除
        string fixedContent = Regex.Replace(contentToCheck, @"(・ｿ)\1{1,}", "$1");

        if (!string.IsNullOrEmpty(checkEndSearchWord))
        {
            int endIndex = content.IndexOf(checkEndSearchWord);
            fixedContent += content.Substring(endIndex);
        }

        File.WriteAllText(filePath, fixedContent, Encoding.UTF8);
    }
}

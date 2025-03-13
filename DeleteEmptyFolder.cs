using System;
using System.IO;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        // 引数が正しいか確認
        if (args.Length == 0)
        {
            Console.WriteLine("使用方法: DeleteEmptyFolder.exe <フォルダパス>");
            return;
        }

        // 指定されたフォルダパスを取得
        string folderPath = args[0];

        // フォルダが存在するか確認
        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine("指定されたフォルダが見つかりません: " + folderPath);
            return;
        }

        // 空のフォルダリストを作成
        List<string> emptyFolders = new List<string>();

        // 空のフォルダを再帰的に探索
        FindEmptyFolders(folderPath, emptyFolders);

        // 空のフォルダがない場合は終了
        if (emptyFolders.Count == 0)
        {
            Console.WriteLine("空のフォルダは見つかりませんでした。");
            return;
        }

        // 見つかった空のフォルダを表示
        Console.WriteLine("空のフォルダ一覧:");
        foreach (var folder in emptyFolders)
        {
            Console.WriteLine(folder);
        }

        // 削除の確認をする
        Console.Write("\n空のフォルダを削除しますか？ (y/n): ");
        string confirm = Console.ReadLine();

        if (confirm.ToLower() == "y")
        {
            // 削除処理
            foreach (var folder in emptyFolders)
            {
                try
                {
                    Directory.Delete(folder);
                    Console.WriteLine("削除しました: " + folder);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("削除できませんでした: " + folder);
                    Console.WriteLine("エラー: " + ex.Message);
                }
            }
        }
        else
        {
            Console.WriteLine("削除はキャンセルされました。");
        }
    }

    /// <summary>
    /// 指定したディレクトリ以下の空のフォルダを再帰的に検索します。
    /// </summary>
    /// <param name="path">検索するフォルダパス</param>
    /// <param name="emptyFolders">空のフォルダを格納するリスト</param>
    static void FindEmptyFolders(string path, List<string> emptyFolders)
    {
        // サブフォルダを取得
        string[] subDirs = Directory.GetDirectories(path);

        // 各サブフォルダについて再帰的にチェック
        foreach (var subDir in subDirs)
        {
            FindEmptyFolders(subDir, emptyFolders);
        }

        // フォルダが空かどうか確認
        if (Directory.GetFiles(path).Length == 0 && Directory.GetDirectories(path).Length == 0)
        {
            emptyFolders.Add(path);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: DeleteFiles.exe <target_folder> <pattern1> [<pattern2> ...]");
            return;
        }

        string targetFolder = args[0];
        var patterns = args.Skip(1).Distinct().ToList(); // 重複パターンを削除

        if (!Directory.Exists(targetFolder))
        {
            Console.WriteLine("指定されたフォルダが存在しません: {0}", targetFolder);
            return;
        }

        List<string> itemsToDelete = new List<string>();

        Console.WriteLine("検索対象フォルダ: {0}", targetFolder);
        Console.WriteLine("検索パターン:");
        foreach (var pattern in patterns)
        {
            Console.WriteLine("  {0}", pattern);
        }

        foreach (var pattern in patterns)
        {
            // パターン名を正しく表示
            Console.WriteLine("パターン '{0}' で検索中...", pattern);

            try
            {
                // ワイルドカードをサポートする検索を実行
                var files = Directory.EnumerateFiles(targetFolder, pattern, SearchOption.AllDirectories);
                var dirs = Directory.EnumerateDirectories(targetFolder, pattern, SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    Console.WriteLine("見つかったファイル: {0}", file);
                    itemsToDelete.Add(file);
                }

                foreach (var dir in dirs)
                {
                    Console.WriteLine("見つかったフォルダ: {0}", dir);
                    itemsToDelete.Add(dir);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("検索エラー: {0}", ex.Message);
            }
        }

        if (itemsToDelete.Count == 0)
        {
            Console.WriteLine("該当するファイルやフォルダは見つかりませんでした。");
            return;
        }

        Console.WriteLine("リストアップされたファイルやフォルダ:");
        foreach (var item in itemsToDelete)
        {
            Console.WriteLine(item);
        }

        // ユーザー確認
        Console.Write("削除しますか？ (y/n): ");
        string confirm = Console.ReadLine();
        if (!confirm.Equals("y", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("削除はキャンセルされました。");
            return;
        }

        // 削除処理
        foreach (var item in itemsToDelete)
        {

            try
            {
                // 隠し属性と読み取り専用属性を解除
                if (File.Exists(item))
                {
                    var attributes = File.GetAttributes(item);
                    if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden ||
                        (attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(item, attributes & ~FileAttributes.Hidden & ~FileAttributes.ReadOnly);
                    }

                    File.Delete(item);
                    Console.WriteLine("削除: {0}", item);
                }
                else if (Directory.Exists(item))
                {
                    var attributes = File.GetAttributes(item);
                    if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden ||
                        (attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(item, attributes & ~FileAttributes.Hidden & ~FileAttributes.ReadOnly);
                    }

                    Directory.Delete(item, true);
                    Console.WriteLine("削除: {0}", item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("削除エラー: {0} - {1}", item, ex.Message);
            }
        }

        Console.WriteLine("削除が完了しました。");
    }
}

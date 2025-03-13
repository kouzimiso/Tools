using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        // 引数の確認
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: Program.exe <target_folder> <LF->CRLF(true)/CRLF->LF(false)> <pattern1> [<pattern2> ...]");
            return;
        }

        // 引数1: フォルダパス
        string targetFolder = args[0];

        // 引数2: LF->CRLF(true) か CRLF->LF(false) かの選択
        bool convertToCRLF = bool.Parse(args[1]);

        // 引数3以降: ファイルフィルター（例: "*.txt", "*.cs"）
        var patterns = args.Skip(2).ToList();

        // 対象フォルダの存在確認
        if (!Directory.Exists(targetFolder))
        {
            Console.WriteLine("指定されたフォルダが存在しません: {0}", targetFolder);
            return;
        }

        // 処理対象のファイルリスト
        List<string> filesToConvert = new List<string>();

        Console.WriteLine("検索対象フォルダ: {0}", targetFolder);
        Console.WriteLine("検索パターン:");
        foreach (var pattern in patterns)
        {
            Console.WriteLine("  {0}", pattern);
        }

        // 指定されたパターンに一致するファイルを検索
        foreach (var pattern in patterns)
        {
            try
            {
                // 再帰的にファイルを検索 (サブフォルダ含む)
                var files = Directory.EnumerateFiles(targetFolder, pattern, SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    // ファイルのエンコーディングを判別して読み込み
                    Encoding encoding;
                    string content = TryReadFileWithAutoDetectEncoding(file, out encoding);

                    if (content == null)
                    {
                        Console.WriteLine("ファイルの読み込みに失敗しました: {0}", file);
                        continue;
                    }

                    // 改行コードのチェック
                    bool containsLF = content.Contains("\n") && !content.Contains("\r\n");
                    bool containsCRLF = content.Contains("\r\n");

                    // 指定した変換条件に一致する場合にリストアップ
                    if ((convertToCRLF && containsLF) || (!convertToCRLF && containsCRLF))
                    {
                        Console.WriteLine("変換対象のファイル: {0}", file);
                        filesToConvert.Add(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("検索エラー: {0}", ex.Message);
            }
        }

        // ファイルが見つからなかった場合
        if (filesToConvert.Count == 0)
        {
            Console.WriteLine("該当するファイルは見つかりませんでした。");
            return;
        }

        // ユーザーに変換確認
        Console.Write("これらのファイルの改行コードを変換しますか？ (y/n): ");
        string confirm = Console.ReadLine();
        if (!confirm.Equals("y", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("変換はキャンセルされました。");
            return;
        }

        // ファイルの変換処理
        foreach (var file in filesToConvert)
        {
            try
            {
                Encoding encoding;
                string content = TryReadFileWithAutoDetectEncoding(file, out encoding);

                if (content == null)
                {
                    Console.WriteLine("エンコーディングの判別に失敗しました: {0}", file);
                    continue;
                }

                // LF->CRLF または CRLF->LF の変換
                if (convertToCRLF)
                {
                    content = content.Replace("\n", "\r\n"); // LF -> CRLF
                }
                else
                {
                    content = content.Replace("\r\n", "\n"); // CRLF -> LF
                }

                // 変換結果を元のエンコーディングで上書き保存
                File.WriteAllText(file, content, encoding);
                Console.WriteLine("変換完了: {0}", file);
            }
            catch (Exception ex)
            {
                Console.WriteLine("変換エラー: {0} - {1}", file, ex.Message);
            }
        }

        Console.WriteLine("全ての変換が完了しました。");
    }

    // ファイルのエンコーディングを簡易的に判別して読み込む
    static string TryReadFileWithAutoDetectEncoding(string filePath, out Encoding encoding)
    {
        // ファイルのバイトを読み取る
        byte[] buffer = File.ReadAllBytes(filePath);

        // UTF-8 BOMが存在するかチェック
        if (buffer.Length >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
        {
            encoding = Encoding.UTF8;
            return Encoding.UTF8.GetString(buffer);
        }

        // バイト列がShift-JISとして正しいか確認する
        try
        {
            string content = Encoding.GetEncoding("Shift_JIS").GetString(buffer);
            encoding = Encoding.GetEncoding("Shift_JIS");
            return content;
        }
        catch
        {
            // Shift-JIS でのデコードが失敗したら UTF-8 にフォールバック
            try
            {
                string content = Encoding.UTF8.GetString(buffer);
                encoding = Encoding.UTF8;
                return content;
            }
            catch
            {
                encoding = null;
                return null;
            }
        }
    }
}

using System;
using System.IO;
using System.Windows.Forms;

namespace FileMover
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // 引数が正しく指定されているかを確認
            if (args.Length != 5)
            {
                Console.WriteLine("Usage: FileMover.exe <IncludeModifiedFiles> <OperationType> <TargetFolder> <CompairFolder> <MoveFolder>");
                return;
            }

            bool includeModifiedFiles = bool.Parse(args[0]);
            string operationType = args[1].ToLower();
            string targetFolder = args[2];
            string compairFolder = args[3];
            string moveFolder = args[4];

            if (!Directory.Exists(targetFolder) || !Directory.Exists(compairFolder))
            {
                Console.WriteLine("指定されたフォルダが存在しません。");
                return;
            }

            if (operationType != "move" && operationType != "copy")
            {
                Console.WriteLine("操作タイプは 'move' または 'copy' でなければなりません。");
                return;
            }

            bool isMoveOperation = operationType == "move";

            // ファイルを移動またはコピーする処理を実行
            MoveOrCopyFiles(compairFolder, targetFolder, moveFolder, isMoveOperation, includeModifiedFiles);
            Console.WriteLine("処理が完了しました。");

            // ユーザーの入力を待つ
            Console.WriteLine("Enterキーを押すと終了します...");
            Console.ReadLine();
        }

        // ファイルの移動またはコピー処理を行う
        static void MoveOrCopyFiles(string compairFolder, string targetFolder, string destinationFolder, bool isMoveOperation, bool includeModifiedFiles)
        {
            var targetFiles = Directory.GetFiles(targetFolder, "*", SearchOption.AllDirectories);
            var targetDirs = Directory.GetDirectories(targetFolder, "*", SearchOption.AllDirectories);

            foreach (var file in targetFiles)
            {
                string relativePath = file.Substring(targetFolder.Length + 1);
                string correspondingSourceFile = Path.Combine(compairFolder, relativePath);
                string destFilePath = Path.Combine(destinationFolder, relativePath);

                if (!File.Exists(correspondingSourceFile) ||
                    (includeModifiedFiles && IsFileModified(file, correspondingSourceFile)))
                {
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));

                        if (File.Exists(destFilePath))
                        {
                            File.Delete(destFilePath);
                        }

                        if (isMoveOperation)
                        {
                            Console.WriteLine(string.Format("Moved file: {0} -> {1}", file, destFilePath));
                            File.Move(file, destFilePath);
                        }
                        else
                        {
                            Console.WriteLine(string.Format("Copied file: {0} -> {1}", file, destFilePath));
                            File.Copy(file, destFilePath);
                        }
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine(string.Format("Error processing file {0} to {1}: {2}", file, destFilePath, ex.Message));
                    }
                }
            }

            foreach (var dir in targetDirs)
            {
                string relativePath = dir.Substring(targetFolder.Length + 1);
                string correspondingSourceDir = Path.Combine(compairFolder, relativePath);
                string destDirPath = Path.Combine(destinationFolder, relativePath);

                if (!Directory.Exists(correspondingSourceDir))
                {
                    try
                    {
                        if (isMoveOperation)
                        {
                            Console.WriteLine(string.Format("Moved directory: {0} -> {1}", dir, destDirPath));
                            Directory.Move(dir, destDirPath);
                        }
                        else
                        {
                            CopyDirectory(dir, destDirPath);
                            Console.WriteLine(string.Format("Copied directory: {0} -> {1}", dir, destDirPath));
                        }
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine(string.Format("Error processing directory {0} to {1}: {2}", dir, destDirPath, ex.Message));
                    }
                }
            }
        }

        static bool IsFileModified(string targetFile, string sourceFile)
        {
            FileInfo targetInfo = new FileInfo(targetFile);
            FileInfo sourceInfo = new FileInfo(sourceFile);
            return targetInfo.Length != sourceInfo.Length || targetInfo.LastWriteTime != sourceInfo.LastWriteTime;
        }

        static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (var subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }
    }
}

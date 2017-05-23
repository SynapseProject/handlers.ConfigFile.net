using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

using Alphaleonis.Win32.Filesystem;


namespace Synapse.Handlers.FileUtil
{
    public class CopyUtil
    {
        public bool OverwriteExisting { get; set; } = true;
        public bool IncludeSubdirectories { get; set; } = true;
        public bool PurgeDestination { get; set; } = false;
        public bool Verbose { get; set; } = true;

        public String CallbackLabel { get; set; }
        public Action<string, string> Callback { get; set; }

        public FileTransaction Transaction { get; set; } = new FileTransaction();

        public CopyUtil() { }

        public CopyUtil(CopyFileHandlerConfig config)
        {
            OverwriteExisting = config.OverwriteExisting;
            IncludeSubdirectories = config.IncludeSubdirectories;
            PurgeDestination = config.PurgeDestination;
            Verbose = config.Verbose;
        }

        public void Copy(String source, String destination, String callbackLabel = null, Action<string, string> callback = null, bool dryRun = false)
        {
            Callback = callback;
            CallbackLabel = callbackLabel;

            if (dryRun)
                CallbackLabel = CallbackLabel + " - DryRun";

            if (PurgeDestination)
            {
                Callback?.Invoke(CallbackLabel, "Purging Directory [" + destination + "]");
                if (!dryRun && Directory.Exists(destination))
                    Directory.Delete(destination, true);
            }

            DoAction(FileAction.Copy, source, destination, dryRun);
        }

        public void Move(String source, String destination, String callbackLabel = null, Action<string, string> callback = null, bool dryRun = false)
        {
            Callback = callback;
            CallbackLabel = callbackLabel;

            if (dryRun)
                CallbackLabel = CallbackLabel + " - DryRun";

            if (PurgeDestination)
            {
                Callback?.Invoke(CallbackLabel, "Purging Directory [" + destination + "]");
                if (!dryRun && Directory.Exists(destination))
                    Directory.Delete(destination, true);
            }

            DoAction(FileAction.Move, source, destination, dryRun);
        }

        private void DoAction(FileAction action, String source, String destination, bool dryRun = false)
        {
            FileType sourceType = GetType(source);
            FileType destinationType = GetType(destination);

            try
            {
                CopyOptions copyOptions = CopyOptions.FailIfExists;
                MoveOptions moveOptions = MoveOptions.CopyAllowed | MoveOptions.WriteThrough;

                if (OverwriteExisting)
                {
                    copyOptions = CopyOptions.None;
                    moveOptions |= MoveOptions.ReplaceExisting;
                }

                String dirPath = Path.GetDirectoryName(destination);
                if (!Directory.Exists(dirPath))
                {
                    if (!dryRun)
                    {
                        CreateDirectory(dirPath);
                        Callback?.Invoke(CallbackLabel, "Directory [" + dirPath + "] Created.");
                    }
                    else
                        Callback?.Invoke(CallbackLabel, "Directory [" + dirPath + "] Will Be Created.");
                }

                CopyMoveResult result = null;
                switch (sourceType)
                {
                    case FileType.File:
                        if (destinationType == FileType.Directory)
                        {
                            String fileName = Path.GetFileName(source);
                            destination = Path.Combine(destination, fileName);
                        }

                        if (!dryRun)
                        {
                            if (action == FileAction.Copy)
                                result = CopyFile(source, destination, copyOptions);
                            else
                                result = MoveFile(source, destination, moveOptions);
                        }
                        break;
                    case FileType.Directory:
                        if (destinationType == FileType.File)
                        {
                            Callback?.Invoke(CallbackLabel, "Unable To Copy Directory [" + source + "] Into Existing File [" + destination + "]");
                            break;
                        }

                        if (IncludeSubdirectories == false)
                        {
                            // Only Grab Files and Top-Level Directories From Source Directory
                            DirectoryInfo dirInfo = new DirectoryInfo(source);
                            FileInfo[] files = dirInfo.GetFiles();
                            foreach (FileInfo file in files)
                                DoAction(action, file.FullName, destination, dryRun);

                            DirectoryInfo[] sourceDirs = dirInfo.GetDirectories();
                            foreach (DirectoryInfo sourceDir in sourceDirs)
                            {
                                DirectoryInfo destDir = new DirectoryInfo(Path.Combine(destination, sourceDir.Name));
                                if (!destDir.Exists)
                                {
                                    if (!dryRun)
                                    {
                                        CreateDirectory(destDir.FullName);
                                        Callback?.Invoke(CallbackLabel, "Directory [" + destDir.FullName + "] Created.");
                                    }
                                    else
                                        Callback?.Invoke(CallbackLabel, "Directory [" + destDir.FullName + "] Will Be Created.");
                                }
                            }
                        }
                        else
                        {
                            if (dryRun && Verbose)
                            {
                                ListContents(source);
                            }

                            if (!dryRun)
                            {
                                if (action == FileAction.Copy)
                                    result = CopyDirectory(source, destination, copyOptions);
                                else
                                    result = MoveDirectory(source, destination, moveOptions);
                            }
                        }
                        break;
                }

                String message = String.Empty;
                if (dryRun)
                    message = String.Format("{0} {1}: [{2}] To [{3}].", (action == FileAction.Move ? "Will Move" : "Will Copy"), sourceType, source, destination);
                else if (result.ErrorCode == 0)
                    message = String.Format("{0} {1}: [{2}] To [{3}].", (action == FileAction.Move ? "Moved" : "Copied"), sourceType, source, destination);
                else
                    message = String.Format("ERROR : {0} - Source: [{1}], Destination[{2}]", result.ErrorMessage, result.Source, result.Destination);

                Callback?.Invoke(CallbackLabel, message);
            }
            catch (Exception ex)
            {
                Callback?.Invoke(CallbackLabel, "Copy Failed On - Source:[" + source + "] [to] Destination:[" + destination + "]");
                throw ex;
            }
        }

        public static FileType GetType(String path)
        {
            FileType type = FileType.Unknown;
            FileInfo info = new FileInfo(path);
            if (info.Exists)
            {
                if (info.Attributes.HasFlag(System.IO.FileAttributes.Directory))
                    type = FileType.Directory;
                else
                    type = FileType.File;
            }
            else
            {
                DirectoryInfo dInfo = new DirectoryInfo(path);
                if (dInfo.Exists)
                    type = FileType.Directory;
                else if (path.Trim().EndsWith(@"\") || path.Trim().EndsWith(@"/"))
                    type = FileType.Directory;
            }
            return type;
        }


        public CopyMoveProgressResult CopyMoveProgressHandler(long totalFileSize, long totalBytesTransferred,
            long streamSize, long streamBytesTransferred, int streamNumber,
            CopyMoveProgressCallbackReason callbackReason, object userData)
        {
            if (userData != null && Verbose)
            {
                string[] files = userData.ToString().Split('|');
                Callback?.Invoke(CallbackLabel, string.Format("Copied File: [{0}] to [{1}]", files[0], files[1]));
            }

            return CopyMoveProgressResult.Continue;
        }

        private void CreateDirectory(String dirPath)
        {
            if (Transaction.IsStarted)
                Directory.CreateDirectoryTransacted(Transaction.Kernal, dirPath);
            else
                Directory.CreateDirectory(dirPath);
        }

        private CopyMoveResult CopyFile(String source, String destination, CopyOptions options)
        {
            CopyMoveResult result = null;
            if (Transaction.IsStarted)
                result = File.CopyTransacted(Transaction.Kernal, source, destination, options, CopyMoveProgressHandler, null, PathFormat.FullPath);
            else
                result = File.Copy(source, destination, options, CopyMoveProgressHandler, null, PathFormat.FullPath);
            return result;
        }

        private CopyMoveResult MoveFile(String source, String destination, MoveOptions options)
        {
            CopyMoveResult result = null;
            if (Transaction.IsStarted)
                result = File.MoveTransacted(Transaction.Kernal, source, destination, options, CopyMoveProgressHandler, null, PathFormat.FullPath);
            else
                result = File.Move(source, destination, options, CopyMoveProgressHandler, null, PathFormat.FullPath);
            return result;
        }

        private CopyMoveResult CopyDirectory(String source, String destination, CopyOptions options)
        {
            CopyMoveResult result = null;
            if (Transaction.IsStarted)
                result = Directory.CopyTransacted(Transaction.Kernal, source, destination, options, CopyMoveProgressHandler, null, PathFormat.FullPath);
            else
                result = Directory.Copy(source, destination, options, CopyMoveProgressHandler, null, PathFormat.FullPath);
            return result;
        }

        private CopyMoveResult MoveDirectory(String source, String destination, MoveOptions options)
        {
            CopyMoveResult result = null;
            if (Transaction.IsStarted)
                result = Directory.MoveTransacted(Transaction.Kernal, source, destination, options, CopyMoveProgressHandler, null, PathFormat.FullPath);
            else
                result = Directory.Move(source, destination, options, CopyMoveProgressHandler, null, PathFormat.FullPath);
            return result;
        }

        private void ListContents(String path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            if (dir.Exists)
                ListContents(dir);
        }

        private void ListContents(DirectoryInfo dir)
        {
            if (dir != null)
            {
                DirectoryInfo[] dirs = dir.GetDirectories();
                foreach (DirectoryInfo d in dirs)
                {
                    ListContents(d);
                }

                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    Callback?.Invoke(CallbackLabel, string.Format("Will Copy File: [{0}] to [{1}]", files[0], files[1]));
                }
            }
        }

        private String Indent(int depth, int indention, String msg)
        {
            return msg.PadLeft((msg.Length + (depth * indention)), ' ');
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphaleonis.Win32.Filesystem;

namespace Synapse.Handlers.FileUtil
{
    public class CopyUtil
    {
        public bool OverwriteExisting { get; set; } = true;
        public bool IncludeSubdirectories { get; set; } = true;
        public bool MaintainAttributes { get; set; } = true;
        public bool PurgeDestination { get; set; } = false;
        public bool Verbose { get; set; } = true;

        public String CallbackLabel { get; set; }
        public Action<string, string> Callback { get; set; }

        public CopyUtil() { }

        public CopyUtil(CopyFileHandlerConfig config)
        {
            OverwriteExisting = config.OverwriteExisting;
            IncludeSubdirectories = config.IncludeSubdirectories;
            MaintainAttributes = config.MaintainAttributes;
            PurgeDestination = config.PurgeDestination;
            Verbose = config.Verbose;
        }

        public void Copy(String source, String destination, String callbackLabel = null, Action<string, string> callback = null, bool dryRun = false)
        {
            DoAction(FileAction.Copy, source, destination, callbackLabel, callback, dryRun);
        }

        public void Move(String source, String destination, String callbackLabel = null, Action<string, string> callback = null, bool dryRun = false)
        {
            DoAction(FileAction.Move, source, destination, callbackLabel, callback, dryRun);
        }

        private void DoAction(FileAction action, String source, String destination, String callbackLabel = null, Action<string, string> callback = null, bool dryRun = false)
        {
            Callback = callback;
            CallbackLabel = callbackLabel;

            if (dryRun)
                CallbackLabel = CallbackLabel + " - DryRun";

            FileType sourceType = GetType(source);
            FileType destinationType = GetType(destination);

            try
            {
                CopyOptions copyOptions = CopyOptions.FailIfExists;
                MoveOptions moveOptions = MoveOptions.None;

                if (OverwriteExisting)
                {
                    copyOptions = CopyOptions.None;
                    moveOptions = MoveOptions.ReplaceExisting;
                }

                String dirPath = Path.GetDirectoryName(destination);
                if (!Directory.Exists(dirPath) && !dryRun)
                    Directory.CreateDirectory(dirPath);

                switch (sourceType)
                {
                    case FileType.File:
                        if (destinationType == FileType.Directory)
                        {
                            String fileName = Path.GetFileName(source);
                            destination = Path.Combine(destination, fileName);
                        }

                        if (action == FileAction.Copy)
                        {
                            if (!dryRun)
                                File.Copy(source, destination, copyOptions, MaintainAttributes, CopyMoveProgressHandler, null, PathFormat.FullPath);
                            Callback?.Invoke(CallbackLabel, "Copied File [" + source + "] To File [" + destination + "].");
                        }
                        else
                        {
                            if (!dryRun)
                               File.Move(source, destination, moveOptions, CopyMoveProgressHandler, null, PathFormat.FullPath);
                            Callback?.Invoke(CallbackLabel, "Moved File [" + source + "] To File [" + destination + "].");
                        }
                        break;
                    case FileType.Directory:
                        if (destinationType == FileType.File)
                        {
                            Callback?.Invoke(CallbackLabel, "Unable To Copy Directory [" + source + "] Into Existing File [" + destination + "]");
                            break;
                        }

                        if (action == FileAction.Copy)
                        {
                            if (Verbose)
                            {
                                Callback?.Invoke(CallbackLabel, "Contents of Directory [" + source + "]");
                                ListContents(source);
                            }
                            if (!dryRun)
                                Directory.Copy(source, destination, copyOptions, CopyMoveProgressHandler, null, PathFormat.FullPath);
                            Callback?.Invoke(CallbackLabel, "Copied Directory [" + source + "] To Directory [" + destination + "].");
                        }
                        else
                        {
                            if (Verbose)
                            {
                                Callback?.Invoke(CallbackLabel, "Contents of Directory [" + source + "]");
                                ListContents(source);
                            }
                            if (!dryRun)
                                Directory.Move(source, destination, moveOptions, CopyMoveProgressHandler, null, PathFormat.FullPath);
                            Callback?.Invoke(CallbackLabel, "Moved Directory [" + source + "] To Directory [" + destination + "].");
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                callback?.Invoke(callbackLabel, "Copy Failed On - Source:[" + source + "] [to] Destination:[" + destination + "]");
                throw ex;
            }
        }

        private FileType GetType(String path)
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
                if (path.Trim().EndsWith(@"\") || path.Trim().EndsWith(@"/"))
                    type = FileType.Directory;
            }

            return type;
        }

        public CopyMoveProgressResult CopyMoveProgressHandler(long totalFileSize, long totalBytesTransferred,
            long streamSize, long streamBytesTransferred, int streamNumber,
            CopyMoveProgressCallbackReason callbackReason, object userData)
        {
            if (userData != null)
            {
                string[] files = userData.ToString().Split('|');
                Callback?.Invoke(CallbackLabel, string.Format("Copied file: {0}  [to]  {1}", files[0], files[1]));
            }

            return CopyMoveProgressResult.Continue;
        }

        private void ListContents(String path, int depth = 1, int indention = 2)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            if (dir.Exists)
                ListContents(dir, depth, indention);
        }

        private void ListContents(DirectoryInfo dir, int depth = 1, int indention = 2)
        {
            if (dir != null)
            {
                DirectoryInfo[] dirs = dir.GetDirectories();
                foreach (DirectoryInfo d in dirs)
                {
                    Callback?.Invoke(CallbackLabel, Indent(depth, indention, d.FullName));
                    ListContents(d, depth + 1, indention);
                }

                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    Callback?.Invoke(CallbackLabel, Indent(depth, indention, file.FullName));
                }
            }
        }

        private String Indent(int depth, int indention, String msg)
        {
            return msg.PadLeft((msg.Length + (depth * indention)), ' ');
        }

    }
}

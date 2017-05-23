using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

using Alphaleonis.Win32.Filesystem;


namespace Synapse.Handlers.FileUtil
{
    public class DeleteUtil
    {
        public bool Recursive { get; set; } = true;
        public bool IgnoreReadOnly { get; set; } = true;
        public bool Verbose { get; set; } = true;
        public bool FailIfMissing { get; set; } = true;

        public String CallbackLabel { get; set; }
        public Action<string, string> Callback { get; set; }

        public FileTransaction Transaction { get; set; } = new FileTransaction();

        public DeleteUtil() { }

        public DeleteUtil(DeleteFileHandlerConfig config)
        {
            Recursive = config.Recursive;
            Verbose = config.Verbose;
            FailIfMissing = config.FailIfMissing;
            IgnoreReadOnly = config.IgnoreReadOnly;
        }

        public void Delete(String target, String callbackLabel = null, Action<string, string> callback = null, bool dryRun = false)
        {
            Callback = callback;
            CallbackLabel = callbackLabel;

            if (dryRun)
                CallbackLabel = CallbackLabel + " - DryRun";

            bool exists = Exists(target);

            if (exists)
            {
                FileType type = GetType(target);
                if (!dryRun)
                {
                    switch (type)
                    {
                        case FileType.Directory:
                            {
                                if (Transaction.IsStarted)
                                    Directory.DeleteTransacted(Transaction.Kernal, target, Recursive, IgnoreReadOnly);
                                else
                                    Directory.Delete(target, Recursive, IgnoreReadOnly);
                                break;
                            }

                        case FileType.File:
                            {
                                if (Transaction.IsStarted)
                                    File.DeleteTransacted(Transaction.Kernal, target, IgnoreReadOnly);
                                else
                                    File.Delete(target, IgnoreReadOnly);
                                break;
                            }
                    }
                }

                if (type != FileType.Unknown)
                {
                    String message = String.Empty;
                    message = String.Format("{0} {1}: [{2}].", (dryRun ? "Will Delete" : "Deleted"), type, target);
                    Callback?.Invoke(CallbackLabel, message);
                }
            }
            else
            {
                String message = "Target [" + target + "] Does Not Exist.";
                Callback?.Invoke(CallbackLabel, message);
                if (FailIfMissing)
                    throw new Exception(message);
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

        public static bool Exists(String path)
        {
            FileInfo info = new FileInfo(path);
            if (info.Exists)
                return true;
            else
            {
                DirectoryInfo dInfo = new DirectoryInfo(path);
                return dInfo.Exists;
            }
        }

    }
}

using System;
using System.Collections.Generic;
using Alphaleonis.Win32.Filesystem;

namespace Synapse.Filesystem
{
    public class WindowsSynapseDirectory : SynapseDirectory
    {
        private DirectoryInfo dirInfo = null;

        public WindowsSynapseDirectory() { }
        public WindowsSynapseDirectory(string fullPath)
        {
            FullName = fullPath;
        }

        public override String FullName
        {
            get { return dirInfo.FullName; }
            set { dirInfo = new DirectoryInfo( value ); }
        }
        public override String Name { get { return dirInfo?.Name; } }
        public override String Parent { get { return dirInfo?.Parent?.FullName; } }
        public override String Root { get { return dirInfo?.Root?.FullName; } }

        public override SynapseDirectory Create(string childDirName = null, String callbackLabel = null, Action<string, string> callback = null)
        {
            if (childDirName == null || childDirName == FullName)
            {
                if ( !Directory.Exists( FullName ) )
                    Directory.CreateDirectory( FullName );
                callback?.Invoke( callbackLabel, $"Directory [{FullName}] Was Created." );
                return this;
            }
            else
            {
                String childDirNameString = PathCombine( FullName, childDirName );
                WindowsSynapseDirectory synDir = new WindowsSynapseDirectory( childDirNameString );
                synDir.Create(null, callbackLabel, callback);
                return synDir;
            }
        }

        public override SynapseFile CreateFile(string fullName, String callbackLabel = null, Action<string, string> callback = null)
        {
            return new WindowsSynapseFile(fullName);
        }

        public override void Delete(string dirName = null, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            if ( dirName == null )
            {
                Directory.Delete( FullName, true );
                if (verbose)
                    Logger.Log($"Directory [{FullName}] Was Deleted.", callbackLabel, callback);
            }
            else
            {
                Directory.Delete( dirName, true );
                if (verbose)
                    Logger.Log($"Directory [{dirName}] Was Deleted.", callbackLabel, callback);
            }

            dirInfo = null;
        }

        public override bool Exists(string dirName = null)
        {
            if ( dirName == null )
                return Directory.Exists( FullName );
            else
                return Directory.Exists( dirName );
        }

        public override IEnumerable<SynapseDirectory> GetDirectories()
        {
            String[] directories = Directory.GetDirectories( FullName );

            List<SynapseDirectory> synDirs = new List<SynapseDirectory>();
            foreach (string dir in directories)
            {
                SynapseDirectory synDir = new WindowsSynapseDirectory( Path.Combine( FullName, dir ) );
                synDirs.Add( synDir );
            }

            return synDirs;
        }

        public override IEnumerable<SynapseFile> GetFiles()
        {
            String[] files = Directory.GetFiles( FullName );
            List<SynapseFile> synFiles = new List<SynapseFile>();
            foreach (string file in files)
            {
                SynapseFile synFile = new WindowsSynapseFile( Path.Combine( FullName, file ) );
                synFiles.Add( synFile );
            }

            return synFiles;
        }

        public override string PathCombine(params string[] paths)
        {
            return Path.Combine( paths );
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphaleonis.Win32.Filesystem;

namespace Synapse.Filesystem
{
    public class WindowsSynapseFile : SynapseFile
    {
        private System.IO.FileStream fileStream;
        private bool isStreamOpen = false;

        public override string Name
        {
            get
            {
                return Path.GetFileName( this.FullName );
            }
        }

        public override string FullName { get; set; }

        public WindowsSynapseFile() : base() { }
        public WindowsSynapseFile(string fullName) : base( fullName ) { }

        public override System.IO.Stream OpenStream(AccessType access, String callbackLabel = null, Action<string, string> callback = null)
        {
            if ( !isStreamOpen )
            {
                fileStream = File.Open( FullName, System.IO.FileMode.OpenOrCreate, access == AccessType.Read ? System.IO.FileAccess.Read : System.IO.FileAccess.Write );
                isStreamOpen = true;
                callback?.Invoke( callbackLabel, $"File Stream [{FullName}] Has Been Opened." );
            }
            else
                callback?.Invoke( callbackLabel, $"File Stream [{FullName}] Is Already Open." );
            return fileStream;
        }

        public override void CloseStream(String callbackLabel = null, Action<string, string> callback = null)
        {
            if ( isStreamOpen )
            {
                fileStream.Close();
                isStreamOpen = false;
                callback?.Invoke( callbackLabel, $"File Stream [{FullName}] Has Been Closed." );
            }
            else
                callback?.Invoke( callbackLabel, $"File Stream [{FullName}] Is Already Closed." );

        }

        public override SynapseFile Create(string fileName = null, String callbackLabel = null, Action<string, string> callback = null)
        {
            if ( fileName == null || fileName == FullName)
            {
                fileStream = File.Open( FullName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write );
                isStreamOpen = true;    // Opens Stream as Write By Default
                callback?.Invoke( callbackLabel, $"File [{FullName}] Was Created." );
                return this;
            }
            else
            {
                WindowsSynapseFile synFile = new WindowsSynapseFile( fileName );
                synFile.Create( null, callbackLabel, callback );
                return synFile;
            }
        }

        public override SynapseDirectory CreateDirectory(string dirName, String callbackLabel = null, Action<string, string> callback = null)
        {
            return new WindowsSynapseDirectory(dirName);
        }

        public override void Delete(string fileName = null, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            if ( fileName == null || fileName == FullName )
            {
                File.Delete( FullName );
                if (verbose)
                    callback?.Invoke( callbackLabel, $"File [{FullName}] Was Deleted." );
            }
            else
            {
                File.Delete( fileName );
                if (verbose)
                    callback?.Invoke( callbackLabel, $"File [{fileName}] Was Deleted." );
            }
        }

        public override bool Exists(string fileName = null)
        {
            if ( fileName == null )
                return File.Exists( FullName );
            else
                return File.Exists( fileName );
        }

    }
}

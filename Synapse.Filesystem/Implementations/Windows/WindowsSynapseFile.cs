using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Filesystem
{
    public class WindowsSynapseFile : SynapseFile
    {
        private FileStream fileStream;
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

        public override Stream OpenStream(String callbackLabel = null, Action<string, string> callback = null)
        {
            if ( !isStreamOpen )
            {
                fileStream = File.Open( FullName, FileMode.OpenOrCreate );
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
                if ( !File.Exists( FullName ) )
                {
                    fileStream = File.Open( FullName, FileMode.OpenOrCreate );
                    isStreamOpen = true;
                }
                callback?.Invoke( callbackLabel, $"File [{FullName}] Was Created." );
                return this;
            }
            else
            {
                WindowsSynapseFile synFile = new WindowsSynapseFile( fileName );
                synFile.Create( fileName, callbackLabel, callback );
                return synFile;
            }
        }

        public override void Delete(string fileName = null, String callbackLabel = null, Action<string, string> callback = null)
        {
            if ( fileName == null )
            {
                File.Delete( FullName );
                callback?.Invoke( callbackLabel, $"File [{FullName}] Was Deleted." );
            }
            else
            {
                File.Delete( fileName );
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

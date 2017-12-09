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

        public override Stream OpenStream()
        {
            if ( !isStreamOpen )
            {
                fileStream = File.Open( FullName, FileMode.OpenOrCreate );
                isStreamOpen = true;
            }
            return fileStream;
        }

        public override void CloseStream()
        {
            fileStream.Close();
            isStreamOpen = false;
        }

        public override SynapseFile Create(string fileName = null)
        {
            if ( fileName == null || fileName == FullName)
            {
                if ( !File.Exists( FullName ) )
                {
                    fileStream = File.Open( FullName, FileMode.OpenOrCreate );
                    isStreamOpen = true;
                }
                return this;
            }
            else
            {
                WindowsSynapseFile synFile = new WindowsSynapseFile( fileName );
                synFile.Create( fileName );
                return synFile;
            }
        }

        public override void Delete(string fileName = null)
        {
            if ( fileName == null )
                File.Delete( FullName );
            else
                File.Delete( fileName );
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

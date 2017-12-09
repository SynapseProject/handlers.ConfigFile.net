using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Synapse.Filesystem
{
    public abstract class SynapseFile
    {
        public abstract String Name { get; }
        public abstract String FullName { get; set; }

        public SynapseFile() { }

        public SynapseFile(string fullName)
        {
            FullName = fullName;
        }

        public abstract SynapseFile Create(string fileName = null);
        public abstract void Delete(string fileName = null);
        public abstract bool Exists(string fileName = null);

        public abstract Stream OpenStream();
        public abstract void CloseStream();

        public void CopyTo(SynapseFile file, bool overwrite = true)
        {
            Stream source = this.OpenStream();
            Stream target = file.OpenStream();

            source.CopyTo( target );

            this.CloseStream();
            file.CloseStream();

            Console.WriteLine( $"Copied File [{this.FullName}] to [{file.FullName}]." );
        }

        public void MoveTo(SynapseFile file, bool overwrite = true)
        {
            CopyTo( file );
            this.Delete();
            Console.WriteLine( $"Moved File [{this.FullName}] to [{file.FullName}]." );

        }
    }
}

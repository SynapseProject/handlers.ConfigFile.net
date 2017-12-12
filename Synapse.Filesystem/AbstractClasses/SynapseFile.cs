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

        public abstract SynapseFile Create(string fileName = null, String callbackLabel = null, Action<string, string> callback = null);
        public abstract void Delete(string fileName = null, String callbackLabel = null, Action<string, string> callback = null);
        public abstract bool Exists(string fileName = null);

        public abstract Stream OpenStream(AccessType access, String callbackLabel = null, Action<string, string> callback = null);
        public abstract void CloseStream(String callbackLabel = null, Action<string, string> callback = null);

        public void CopyTo(SynapseFile file, bool overwrite = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            Stream source = this.OpenStream(AccessType.Read);
            Stream target = file.OpenStream( AccessType.Write);

            source.CopyTo( target );

            this.CloseStream();
            file.CloseStream();

            callback?.Invoke( callbackLabel, $"Copied File [{this.FullName}] to [{file.FullName}]." );
        }

        public void MoveTo(SynapseFile file, bool overwrite = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            CopyTo( file );
            this.Delete();
            callback?.Invoke( callbackLabel, $"Moved File [{this.FullName}] to [{file.FullName}]." );

        }
    }
}

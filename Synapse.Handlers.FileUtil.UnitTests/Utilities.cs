using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace Synapse.Handlers.FileUtil.UnitTests
{
    public static class Utilities
    {
        public static void SetupTestFiles(string source = null, string destination = null)
        {
            CleanupTestFiles( destination );
            if( source == null )    // if there is nothing to copy over just make sure dir exists
            {
                if( !Directory.Exists( destination ) )
                {
                    Directory.CreateDirectory( destination );
                }
            }
            else
            {
                DirectoryCopy( source, destination, true );
            }
        }
        public static void CleanupTestFiles( string destination )
        {
            if( destination != null && Directory.Exists( destination ) )
                Directory.Delete( destination, true );
        }
        public static void DirectoryCopy(string source, string destination, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo( source );

            if( !dir.Exists )
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + source );
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if( !Directory.Exists( destination ) )
            {
                Directory.CreateDirectory( destination );
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach( FileInfo file in files )
            {
                string temppath = Path.Combine( destination, file.Name );
                file.CopyTo( temppath, false );
            }

            // If copying subdirectories, copy them and their contents to new location.
            if( copySubDirs )
            {
                foreach( DirectoryInfo subdir in dirs )
                {
                    string temppath = Path.Combine( destination, subdir.Name );
                    DirectoryCopy( subdir.FullName, temppath, copySubDirs );
                }
            }
        }
    }
}

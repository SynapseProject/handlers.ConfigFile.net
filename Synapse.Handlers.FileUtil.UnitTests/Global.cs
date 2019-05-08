using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;
namespace Synapse.Handlers.FileUtil.UnitTests
{
    [SetUpFixture]
    public class Global
    {
        public static string Root = null;
        public static string PlansRoot = null;
        public static string TestFilesRoot = null;
        public static string WorkingDirectory = null;
        public static string SourceDir1 = null;
        public static string SourceDir2 = null;
        public static string DestDir1 = null;
        public static string DestDir2 = null;

        [OneTimeSetUp]
        public void Init()
        {
            Root = Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location );
            Directory.SetCurrentDirectory( $@"{Root}\..\.." );
            Root = Directory.GetCurrentDirectory();
            PlansRoot = $@"{Root}\Plans";
            TestFilesRoot = $@"{Root}\TestFiles";
            WorkingDirectory = $@"{Root}\WorkingDir";
            SourceDir1 = $@"{WorkingDirectory}\Source1";
            SourceDir2 = $@"{WorkingDirectory}\Source2";
            DestDir1 = $@"{WorkingDirectory}\Dest1";
            DestDir2 = $@"{WorkingDirectory}\Dest2";
        }
        public static void SetupTestFiles()
        {
            CleanupTestFiles();
            DirectoryCopy( TestFilesRoot, WorkingDirectory, true );
        }
        public static void CleanupTestFiles()
        {
            if( Directory.Exists( WorkingDirectory ) )
                Directory.Delete( WorkingDirectory, true );
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

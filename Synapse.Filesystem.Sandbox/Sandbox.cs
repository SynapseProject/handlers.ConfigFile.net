using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphaleonis.Win32.Filesystem;

namespace Synapse.Filesystem
{
    public class Sandbox
    {
        public static void Main(string[] args)
        {

            //String sourceFile = @"C:\Temp\Source\YamlDotNet.xml";
            //String targetFile = @"C:\Temp\Destination\YamlDotNet.xml";
            ////sourceFile = @"\\localhost\C$\Temp\Source\YamlDotNet.xml";
            ////targetFile = @"\\localhost\C$\Temp\Destination\YamlDotNet.xml";

            //WindowsSynapseFile synSourceFile = new WindowsSynapseFile( sourceFile );
            //WindowsSynapseFile synTargetFile = new WindowsSynapseFile( targetFile );
            //synSourceFile.CopyTo( synTargetFile );
            //synTargetFile.Delete();
            //synSourceFile.MoveTo( synTargetFile );


            String sourceDir = @"C:\Temp\Source";
            String targetDir = @"C:\Temp\Destination";
            //sourceDir = @"\\localhost\C$\Temp\Source";
            //targetDir = @"\\localhost\C$\Temp\Destination";

            WindowsSynapseDirectory synSourceDir = new WindowsSynapseDirectory( sourceDir );
            WindowsSynapseDirectory synTargetDir = new WindowsSynapseDirectory( targetDir );
            synSourceDir.CopyTo( synTargetDir, true, true, "Sandbox", ConsoleWriter );
            Console.WriteLine( "================================" );
            synTargetDir.Clear( null, "Sandbox", ConsoleWriter );
            Console.WriteLine( "================================" );
            synSourceDir.MoveTo( synTargetDir, true, true, "Sandbox", ConsoleWriter );




            Console.WriteLine( "Press <ENTER> To Continue..." );
            Console.ReadLine();
        }

        private static void ConsoleWriter(string label, string message)
        {
            Console.WriteLine( $"{label} - {message}" );
        }
    }
}

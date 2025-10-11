using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Ganss.IO;

namespace DotMake.SvgSprite.Cli.Commands
{
    internal class FileCliCommandBase
    {
        protected static readonly bool FileSystemIgnoreCase = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        protected static readonly StringComparer FileSystemStringComparer = FileSystemIgnoreCase
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
        protected static readonly StringComparison FileSystemStringComparison = FileSystemIgnoreCase
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        protected const string PathPatternsDescription = "Patterns in paths are supported:" +
                                                         "\n- ? matches a single character" +
                                                         "\n- * matches zero or more characters" +
                                                         "\n- ** matches zero or more recursive directories, e.g. a\\**\\x matches a\\x, a\\b\\x, a\\b\\c\\x, etc." +
                                                         "\n- [...] matches a set of characters, syntax is the same as character groups in Regex." +
                                                         "\n- {group1,group2,...} matches any of the pattern groups. Groups can contain groups and patterns, e.g. {a\\b,{c,d}*}.";

        protected static CliExitInfo CheckInputFiles(IEnumerable<string> inputFiles, bool skipDuplicates, out List<IFileInfo> inputFileInfos)
        {
            var inputFileDuplicateCheck = skipDuplicates
                ? new HashSet<string>(FileSystemStringComparer)
                : null;
            inputFileInfos = new List<IFileInfo>();

            var index = 1; 
            foreach (var inputFile in inputFiles)
            {
                var emptyGlob = true;

                Console.WriteLine($"Input files group {index}:");

                foreach (var fileSystemInfo in Glob.Expand(inputFile, FileSystemIgnoreCase))
                {
                    if (emptyGlob)
                        emptyGlob = false;

                    var exitInfo = CheckInputFile(fileSystemInfo, inputFileInfos, inputFileDuplicateCheck);
                    if (exitInfo != null)
                        return exitInfo;
                }

                //if emptyGlob is true here, means glob expand was empty above
                //this can happen when pattern in path returns nothing or no pattern in path and file does not exist
                //so treat the input as regular path here
                if (emptyGlob)
                {
                    var fileSystemInfo = CreateFileSystemInfo(inputFile);
                    var exitInfo = CheckInputFile(fileSystemInfo, inputFileInfos, inputFileDuplicateCheck);
                    if (exitInfo != null)
                        return exitInfo;
                }

                index++;
            }

            //should not happen but just in case
            if (inputFileInfos.Count == 0)
                return new CliExitInfo(3, "No input files found!");

            return null; //success
        }

        protected static CliExitInfo CheckOutputFiles(IEnumerable<string> outputFiles, bool skipDuplicates, bool overwrite, out List<IFileInfo> outputFileInfos)
        {
            var outputFileDuplicateCheck = skipDuplicates
                ? new HashSet<string>(FileSystemStringComparer)
                : null;
            outputFileInfos = new List<IFileInfo>();

            var index = 1;
            foreach (var outputFile in outputFiles)
            {
                Console.WriteLine($"Output files group {index}:");

                var fileSystemInfo = CreateFileSystemInfo(outputFile);
                var exitInfo = CheckOutputFile(fileSystemInfo, outputFileInfos, outputFileDuplicateCheck, overwrite: overwrite);
                if (exitInfo != null)
                    return exitInfo;

                index++;
            }

            //should not happen but just in case
            if (outputFileInfos.Count == 0)
                return new CliExitInfo(3, "No output files found!");

            return null; //success
        }

        protected static CliExitInfo CheckInputFile(string inputFile, out IFileInfo inputFileInfo)
        {
            var inputFileInfos = new List<IFileInfo>();
            inputFileInfo = null;

            Console.WriteLine("Input file:");

            var fileSystemInfo = CreateFileSystemInfo(inputFile);
            var exitInfo = CheckInputFile(fileSystemInfo, inputFileInfos, forSingle: true);
            if (exitInfo != null)
                return exitInfo;

            inputFileInfo = inputFileInfos[0];
            return null; //success
        }

        protected static CliExitInfo CheckOutputFile(string outputFile, bool overwrite, out IFileInfo outputFileInfo)
        {
            var outputFileInfos = new List<IFileInfo>();
            outputFileInfo = null;

            Console.WriteLine("Output file:");

            var fileSystemInfo = CreateFileSystemInfo(outputFile);
            var exitInfo = CheckOutputFile(fileSystemInfo, outputFileInfos, forSingle: true, overwrite: overwrite);
            if (exitInfo != null)
                return exitInfo;

            outputFileInfo = outputFileInfos[0];
            return null; //success
        }

        protected static CliExitInfo CheckOutputDirectory(string outputDirectory, out IDirectoryInfo outputDirectoryInfo)
        {
            var outputDirectoryInfos = new List<IDirectoryInfo>();
            outputDirectoryInfo = null;

            Console.WriteLine("Output directory:");

            var fileSystemInfo = CreateFileSystemInfo(outputDirectory);
            var exitInfo = CheckOutputDirectory(fileSystemInfo, outputDirectoryInfos, forSingle: true);
            if (exitInfo != null)
                return exitInfo;

            outputDirectoryInfo = outputDirectoryInfos[0];
            return null; //success
        }

        private static CliExitInfo CheckInputFile(IFileSystemInfo fileSystemInfo, List<IFileInfo> fileInfos, HashSet<string> fileDuplicateCheck = null, bool forSingle = false)
        {
            var fileInfo = fileSystemInfo as IFileInfo;

            if (fileInfo == null)
            {
                Console.WriteLine($"  {fileSystemInfo.FullName} => NOT A FILE!");
                return new CliExitInfo(2, forSingle ? "The input is not a file!" : "One of the inputs is not a file!");
            }

            if (!fileInfo.Exists)
            {
                Console.WriteLine($"  {fileInfo.FullName} => NOT EXISTS!");
                return new CliExitInfo(1, forSingle ? "The input file does not exist!" : "One of the input files does not exist!");
            }

            if (fileDuplicateCheck != null)
            {
                var key = fileInfo.Name + fileInfo.Length + fileInfo.LastWriteTimeUtc.Ticks;
                if (!fileDuplicateCheck.Add(key))
                {
                    Console.WriteLine($"  {fileInfo.FullName} => DUPLICATE SKIPPED!");
                    return null; //success
                }
            }

            Console.WriteLine($"  {fileInfo.FullName}");
            fileInfos.Add(fileInfo);

            return null; //success
        }

        private static CliExitInfo CheckOutputFile(IFileSystemInfo fileSystemInfo, List<IFileInfo> fileInfos, HashSet<string> fileDuplicateCheck = null, bool forSingle = false, bool overwrite = false)
        {
            var fileInfo = fileSystemInfo as IFileInfo;

            if (fileInfo == null)
            {
                Console.WriteLine($"  {fileSystemInfo.FullName} => NOT A FILE!");
                return new CliExitInfo(2, forSingle ? "The output is not a file!" : "One of the outputs is not a file!");
            }

            if (fileInfo.Exists && !overwrite)
            {
                Console.WriteLine($"  {fileInfo.FullName} => ALREADY EXISTS!");
                return new CliExitInfo(1, forSingle ? "The output file already exists!" : "One of the output files already exists!");
            }

            if (fileDuplicateCheck != null)
            {
                var key = fileInfo.FullName;
                if (!fileDuplicateCheck.Add(key))
                {
                    Console.WriteLine($"  {fileInfo.FullName} => DUPLICATE SKIPPED!");
                    return null; //success
                }
            }

            Console.WriteLine($"  {fileInfo.FullName}");
            fileInfos.Add(fileInfo);

            return null; //success
        }

        private static CliExitInfo CheckOutputDirectory(IFileSystemInfo fileSystemInfo, List<IDirectoryInfo> directoryInfos, bool forSingle = false)
        {
            if (fileSystemInfo is IFileInfo fileInfo && fileInfo.Exists)
            {
                Console.WriteLine($"  {fileSystemInfo.FullName} => NOT A DIRECTORY!");
                return new CliExitInfo(2, forSingle ? "The output is not a directory!" : "One of the outputs is not a directory!");
            }

            var directoryInfo = fileSystemInfo as IDirectoryInfo;
            if (directoryInfo == null)
            {
                var fileSystem = new FileSystem();
                directoryInfo = fileSystem.DirectoryInfo.New(fileSystemInfo.FullName);
            }

            Console.WriteLine($"  {directoryInfo.FullName}");
            directoryInfos.Add(directoryInfo);

            return null; //success
        }


        private static IFileSystemInfo CreateFileSystemInfo(string path)
        {
            //we can't create IFileSystemInfo instance if not enumerating a directory so use this as a workaround

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var fileSystem = new FileSystem();

            return fileSystem.Directory.Exists(path) || path.EndsWith(Path.DirectorySeparatorChar)
                ? fileSystem.DirectoryInfo.New(path)
                : fileSystem.FileInfo.New(path);
        }
    }
}

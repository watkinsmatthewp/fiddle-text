using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeProject;

namespace FiddleText.Backend
{
    public class TextFiddler
    {
        public TextFiddlerConfig Config { get; private set; }

        // Ctor
        public TextFiddler(TextFiddlerConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            Config = config;
        }

        #region Public methods

        public ProcessFileResult[] ProcessPath(string path)
        {
            try
            {
                // Maybe it's a file path?
                return new ProcessFileResult[] { ProcessFile(path, true) };
            }
            catch (FileNotFoundException)
            {
                // Maybe it's a directory path?
                return ProcessDirectory(path, "*.*", SearchOption.AllDirectories);
            }
        }
        
        public ProcessFileResult[] ProcessDirectory(string dirPath, string searchPattern, SearchOption searchOption)
        {
            ValidateDirectoryPath(dirPath);

            ConcurrentBag<ProcessFileResult> fileResults = new ConcurrentBag<ProcessFileResult>();

            ParallelOptions options = new ParallelOptions();
            if (Config.RunMultiThreaded)
            {
                options.MaxDegreeOfParallelism = 1;
            }

            Parallel.ForEach(FastDirectoryEnumerator.EnumerateFiles(dirPath, searchPattern, searchOption), options, file => 
            {
                fileResults.Add(ProcessFile(file.Path, false));
            });

            return fileResults.OrderBy(f => f.OriginalFilePath).ToArray();
        }

        public ProcessFileResult ProcessFile(string inputFilePath)
        {
            return ProcessFile(inputFilePath, true);
        }

        #endregion

        #region Helpers

        private ProcessFileResult ProcessFile(string inputFilePath, bool checkForExistence)
        {            
            Stopwatch watch = new Stopwatch();
            watch.Start();

            ProcessFileResult fileResult = new ProcessFileResult()
            {
                OriginalFilePath = inputFilePath
            };
            
            try
            {
                ValidateInputFilePath(inputFilePath, checkForExistence);
                fileResult.OutputFilePath = GetDestinationFilePath(inputFilePath);

                string tempDestinationOutputFilePath = Path.GetTempFileName();

                EvaluateAndWriteToTempFile(fileResult, tempDestinationOutputFilePath);
                MoveTempFileToDestination(fileResult, tempDestinationOutputFilePath);
            }
            catch (Exception e)
            {
                fileResult.Exception = e;
            }
            finally
            {
                watch.Stop();
                fileResult.Duration = watch.Elapsed;
            }

            return fileResult;
        }

        private static void MoveTempFileToDestination(ProcessFileResult fileResult, string tempDestinationOutputFilePath)
        {
            // Bring over the temp file
            if (fileResult.OutputFilePath == fileResult.OriginalFilePath)
            {
                // Delete the original file so the move doesn't fail
                string tmpCopyPath = Path.Combine(Path.GetDirectoryName(fileResult.OriginalFilePath), Path.GetFileName(tempDestinationOutputFilePath));
                File.Move(tempDestinationOutputFilePath, tmpCopyPath);
                File.Delete(fileResult.OriginalFilePath);
                File.Move(tmpCopyPath, fileResult.OutputFilePath);
            }
            else
            {
                // Move to the new directory
                File.Move(tempDestinationOutputFilePath, fileResult.OutputFilePath);
            }
        }

        private void EvaluateAndWriteToTempFile(ProcessFileResult fileResult, string outputTempFilePath)
        {
            using (FileStream writeStream = new FileStream(outputTempFilePath, FileMode.OpenOrCreate))
            {
                using (StreamWriter fileWriter = new StreamWriter(writeStream))
                {
                    using (FileStream readStream = new FileStream(fileResult.OriginalFilePath, FileMode.Open))
                    {
                        using (StreamReader fileReader = new StreamReader(readStream))
                        {
                            while (!fileReader.EndOfStream)
                            {
                                ProcessLineResult lineResult = ProcessLine(fileReader.ReadLine());
                                fileWriter.WriteLine(lineResult.ResultLine);
                                fileResult.LineChangeCounts[lineResult.ActionTaken]++;
                            }
                        }
                    }
                }
            }
        }

        private ProcessLineResult ProcessLine(string line)
        {
            ProcessLineResult result = new ProcessLineResult();

            foreach (LineFiddlerRule rule in Config.Rules)
            {
                if ((rule.PositiveMatchPattern == null || rule.PositiveMatchPattern.IsMatch(line)) && (rule.NegativeMatchPattern == null || !rule.NegativeMatchPattern.IsMatch(line)))
                {
                    result.ActionTaken = rule.Action;
                    switch (rule.Action)
                    {
                        case LineAction.Maintain:
                            result.ResultLine = line;
                            break;
                        case LineAction.Delete:
                            result.ResultLine = null;
                            break;
                        case LineAction.Modify:
                            if (rule.PositiveMatchPattern == null)
                            {
                                // Non-tokenized string
                                result.ResultLine = rule.Replacement;
                            }

                            result.ResultLine = rule.PositiveMatchPattern.Replace(line, rule.Replacement);
                            break;
                        default:
                            throw new ArgumentException("Unrecognized line action: " + rule.Action);
                    }
                }
            }

            // If we've gotten this far, none of the rules match, so keep the line as is
            return result;
        }
        
        private void ValidateDirectoryPath(string dirPath)
        {
            if (String.IsNullOrWhiteSpace(dirPath))
            {
                throw new ArgumentException("Directory path cannot be blank");
            }
            if (!Directory.Exists(dirPath))
            {
                throw new DirectoryNotFoundException(dirPath + " not found");
            }
        }

        private void ValidateInputFilePath(string inputFilePath, bool checkForExistence)
        {
            if (String.IsNullOrWhiteSpace(inputFilePath))
            {
                throw new ArgumentException("File path cannot be blank");
            }
            if (checkForExistence && !File.Exists(inputFilePath))
            {
                throw new FileNotFoundException(inputFilePath + " not found");
            }
        }

        private string GetDestinationFilePath(string inputFilePath)
        {
            string destinationPath = null;
            if (Config.OverwriteOriginalFile)
            {
                destinationPath = inputFilePath;
            }
            else
            {
                if (!Directory.Exists(Config.OutputDirectory))
                {
                    // TODO: Better way to not do this check/work every time
                    // (better way also needs to be thread-safe)
                    Directory.CreateDirectory(Config.OutputDirectory);
                }
                destinationPath = Path.Combine(Config.OutputDirectory, Path.GetFileName(inputFilePath));
            }
            return destinationPath;
        }

        #endregion
    }
}

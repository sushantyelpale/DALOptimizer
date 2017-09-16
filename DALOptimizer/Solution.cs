using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using ICSharpCode.NRefactory.Utils;

namespace DALOptimizer
{
    /// <summary>
    /// Represents a Visual Studio solution (.csProj or .sln files).
    /// </summary>
    public class Solution
    {
        public readonly string directory;
        public readonly List<CSharpProject> Projects = new List<CSharpProject>();

        public IEnumerable<CSharpFile> AllFiles
        {
            get{  return Projects.SelectMany(p => p.Files); }
        }

        public Solution(string fileName)
        {
            this.directory = Path.GetDirectoryName(fileName);
        }

        public void ChooseCSProjFile(string fileName)
        {
            if (fileName.EndsWith(".csproj"))
            {
                Projects.Add(new CSharpProject(this, "SampleProj", Path.Combine(directory, fileName)));
            }
            else if (fileName.EndsWith(".sln"))
            {
                var projectLinePattern = new Regex("Project\\(\"(?<TypeGuid>.*)\"\\)\\s+=\\s+\"(?<Title>.*)\",\\s*\"(?<Location>.*)\",\\s*\"(?<Guid>.*)\"");
                foreach (string line in File.ReadLines(fileName)){
                    Match match = projectLinePattern.Match(line);
                    if (match.Success){
                        string typeGuid = match.Groups["TypeGuid"].Value;
                        string title = match.Groups["Title"].Value;
                        string location = match.Groups["Location"].Value;
                        string guid = match.Groups["Guid"].Value;
                        switch (typeGuid.ToUpperInvariant()){
                            case "{2150E333-8FDC-42A3-9474-1A3956D46DE8}": // Solution Folder
                                // ignore folders
                                break;
                            case "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}": // C# project
                                Projects.Add(new CSharpProject(this, title, Path.Combine(directory, location)));
                                break;
                            default:
                                Console.WriteLine("Project {0} has unsupported type {1}", location, typeGuid);
                                break;
                        }
                    }
                }
            }
            else
                Environment.Exit(0);

            var solutionSnapshot = new DefaultSolutionSnapshot(this.Projects.Select(p => p.ProjectContent));
            foreach (CSharpProject project in this.Projects)
                project.Compilation = solutionSnapshot.GetCompilation(project.ProjectContent);
        }

        ConcurrentDictionary<string, IUnresolvedAssembly> assemblyDict = new ConcurrentDictionary<string, IUnresolvedAssembly>(Platform.FileNameComparer);

        /// <summary>
        /// Loads a referenced assembly from a .dll.
        /// Returns the existing instance if the assembly was already loaded.
        /// </summary>
        public IUnresolvedAssembly LoadAssembly(string assemblyFileName)
        {
            return assemblyDict.GetOrAdd(assemblyFileName, file => new CecilLoader().LoadAssemblyFile(file));
        }
    }
}

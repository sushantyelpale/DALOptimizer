﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace DALOptimizer
{
	/// <summary>
	/// Represents a C# project (.csproj file)
	/// </summary>
	public class CSharpProject
	{
		/// <summary>
		/// Parent solution.
		/// </summary>
		public readonly Solution Solution;
		
		/// <summary>
		/// Title is the project name as specified in the .sln file.
		/// </summary>
		public readonly string title;
		
		/// <summary>
		/// Name of the output assembly.
		/// </summary>
		public readonly string assemblyName;
		
		/// <summary>
		/// Full path to the .csproj file.
		/// </summary>
		public readonly string fileName;
		
		public readonly List<CSharpFile> Files = new List<CSharpFile>();
		
		public readonly CompilerSettings CompilerSettings = new CompilerSettings();
		
		/// <summary>
		/// The unresolved type system for this project.
		/// </summary>
		public readonly IProjectContent ProjectContent;
		
		/// <summary>
		/// The resolved type system for this project.
		/// This field is initialized once all projects have been loaded (in Solution constructor).
		/// </summary>
		public ICompilation Compilation;
		
		public CSharpProject(Solution solution, string title, string fileName)
		{
			// Normalize the file name
			fileName = Path.GetFullPath(fileName);
			
			this.Solution = solution;
			this.title = title;
			this.fileName = fileName;
			
			// Use MSBuild to open the .csproj
			var msbuildProject = new Microsoft.Build.Evaluation.Project(fileName);
			// Figure out some compiler settings
			this.assemblyName = msbuildProject.GetPropertyValue("AssemblyName");
			this.CompilerSettings.AllowUnsafeBlocks = GetBoolProperty(msbuildProject, "AllowUnsafeBlocks") ?? false;
			this.CompilerSettings.CheckForOverflow = GetBoolProperty(msbuildProject, "CheckForOverflowUnderflow") ?? false;
			string defineConstants = msbuildProject.GetPropertyValue("DefineConstants");
			foreach (string symbol in defineConstants.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
				this.CompilerSettings.ConditionalSymbols.Add(symbol.Trim());
			
			// Initialize the unresolved type system
			IProjectContent projectContent = new CSharpProjectContent();
            projectContent = projectContent.SetAssemblyName(this.assemblyName);
            projectContent = projectContent.SetProjectFileName(fileName);
            projectContent = projectContent.SetCompilerSettings(this.CompilerSettings);
			// Parse the C# code files
			foreach (var item in msbuildProject.GetItems("Compile")) {
				var file = new CSharpFile(this, Path.Combine(msbuildProject.DirectoryPath, item.EvaluatedInclude));
				Files.Add(file);
			}
			// Add parsed files to the type system
            projectContent = projectContent.AddOrUpdateFiles(Files.Select(f => f.unresolvedTypeSystemForFile));
			
			// Add referenced assemblies:
			foreach (string assemblyFile in ResolveAssemblyReferences(msbuildProject)) {
				IUnresolvedAssembly assembly = solution.LoadAssembly(assemblyFile);
                projectContent = projectContent.AddAssemblyReferences(new[] { assembly });
			}
			
			// Add project references:
			foreach (var item in msbuildProject.GetItems("ProjectReference")) {
				string referencedFileName = Path.Combine(msbuildProject.DirectoryPath, item.EvaluatedInclude);
				// Normalize the path; this is required to match the name with the referenced project's file name
				referencedFileName = Path.GetFullPath(referencedFileName);
                projectContent = projectContent.AddAssemblyReferences(new[] { new ProjectReference(referencedFileName) });
			}
            this.ProjectContent = projectContent;
		}
		
		IEnumerable<string> ResolveAssemblyReferences(Microsoft.Build.Evaluation.Project project)
		{
			// Use MSBuild to figure out the full path of the referenced assemblies
			var projectInstance = project.CreateProjectInstance();
			projectInstance.SetProperty("BuildingProject", "false");
			project.SetProperty("DesignTimeBuild", "true");
			
			projectInstance.Build("ResolveAssemblyReferences", new [] { new ConsoleLogger(LoggerVerbosity.Minimal) });
			var items = projectInstance.GetItems("_ResolveAssemblyReferenceResolvedFiles");
			string baseDirectory = Path.GetDirectoryName(this.fileName);
			return items.Select(i => Path.Combine(baseDirectory, i.GetMetadataValue("Identity")));
		}
		
		static bool? GetBoolProperty(Microsoft.Build.Evaluation.Project p, string propertyName)
		{
			string val = p.GetPropertyValue(propertyName);
			bool result;
			if (bool.TryParse(val, out result))
				return result;
			else
				return null;
		}
		
		public override string ToString()
		{
			return string.Format("[CSharpProject AssemblyName={0}]", assemblyName);
		}
	}
}

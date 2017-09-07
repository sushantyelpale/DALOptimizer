using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.CSharp.TypeSystem;

namespace DALOptimizer
{
    public class CSharpFile
    {
        public readonly CSharpProject Project;
        public readonly string FileName;
        public readonly string OriginalText;

        public SyntaxTree SyntaxTree;
        public CSharpUnresolvedFile UnresolvedTypeSystemForFile;

        public CSharpFile(CSharpProject project, string fileName)
        {
            this.Project = project;
            this.FileName = fileName;

            CSharpParser p = new CSharpParser(project.CompilerSettings);
            //			using (var stream = File.OpenRead(fileName)) {
            //				this.CompilationUnit = p.Parse(stream, fileName);
            //			}

            // Keep the original text around; we might use it for a refactoring later
            this.OriginalText = File.ReadAllText(fileName);
            this.SyntaxTree = p.Parse(this.OriginalText, fileName);

            if (p.HasErrors)
            {
                Console.WriteLine("Error parsing " + fileName + ":");
                foreach (var error in p.ErrorsAndWarnings)
                {
                    Console.WriteLine("  " + error.Region + " " + error.Message);
                }
            }
            this.UnresolvedTypeSystemForFile = this.SyntaxTree.ToTypeSystem();
        }

        public CSharpAstResolver CreateResolver()
        {
            return new CSharpAstResolver(Project.Compilation, SyntaxTree, UnresolvedTypeSystemForFile);
        }

        public List<FieldDeclaration> IndexOfFieldDecl = new List<FieldDeclaration>();
        public List<PropertyDeclaration> IndexOfPropDecl = new List<PropertyDeclaration>();
        public List<BlockStatement> IndexOfBlockStmt = new List<BlockStatement>();
        public List<AssignmentExpression> IndexOfAssExpr = new List<AssignmentExpression>();
        public List<ExpressionStatement> IndexOfExprStmt = new List<ExpressionStatement>();
        public List<CatchClause> IndexOfCtchClause = new List<CatchClause>();
        public List<MethodDeclaration> IndexOfMthdDecl = new List<MethodDeclaration>();
        public List<VariableDeclarationStatement> IndexOfVarDeclStmt = new List<VariableDeclarationStatement>();
    }
}

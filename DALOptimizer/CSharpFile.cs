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

        public SyntaxTree syntaxTree;
        public CSharpUnresolvedFile UnresolvedTypeSystemForFile;

        public CSharpFile(CSharpProject project, string fileName)
        {
            this.Project = project;
            this.FileName = fileName;

            CSharpParser parsedFile = new CSharpParser(project.CompilerSettings);

            // Keep the original text around; it might get used for refactoring later.
            this.OriginalText = File.ReadAllText(fileName);
            this.syntaxTree = parsedFile.Parse(this.OriginalText, fileName);

            if (parsedFile.HasErrors)
            {
                Console.WriteLine("Error parsing " + fileName + ":");
                foreach (var error in parsedFile.ErrorsAndWarnings)
                    Console.WriteLine("  " + error.Region + " " + error.Message);
            }
            this.UnresolvedTypeSystemForFile = this.syntaxTree.ToTypeSystem();
        }

        public CSharpAstResolver CreateResolver()
        {
            return new CSharpAstResolver(Project.Compilation, syntaxTree, UnresolvedTypeSystemForFile);
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

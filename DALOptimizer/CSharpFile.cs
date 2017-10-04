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
        public readonly CSharpProject project;
        public readonly string fileName;
        public readonly string originalText;

        public SyntaxTree syntaxTree;
        public CSharpUnresolvedFile unresolvedTypeSystemForFile;

        public CSharpFile(CSharpProject project, string fileName)
        {
            this.project = project;
            this.fileName = fileName;

            CSharpParser parsedFile = new CSharpParser(project.CompilerSettings);

            // Keep the original text around; it might get used for refactoring later.
            this.originalText = File.ReadAllText(fileName);
            this.syntaxTree = parsedFile.Parse(this.originalText, fileName);

            if (parsedFile.HasErrors)
            {
                Console.WriteLine("Error parsing " + fileName + ":");
                foreach (var error in parsedFile.ErrorsAndWarnings)
                    Console.WriteLine("  " + error.Region + " " + error.Message);
            }
            this.unresolvedTypeSystemForFile = this.syntaxTree.ToTypeSystem();
        }

        public CSharpAstResolver CreateResolver()
        {
            return new CSharpAstResolver(project.Compilation, syntaxTree, unresolvedTypeSystemForFile);
        }

        public List<FieldDeclaration> IndexOfFieldDecl = new List<FieldDeclaration>();
        public List<PropertyDeclaration> IndexOfPropDecl = new List<PropertyDeclaration>();
        public List<BlockStatement> IndexOfBlockStmt = new List<BlockStatement>();
        public List<AssignmentExpression> IndexOfAssExpr = new List<AssignmentExpression>();
        public List<ExpressionStatement> IndexOfExprStmt = new List<ExpressionStatement>();
        public List<CatchClause> IndexOfCtchClause = new List<CatchClause>();
        public List<MethodDeclaration> IndexOfMthdDecl = new List<MethodDeclaration>();
        public List<VariableDeclarationStatement> IndexOfVarDeclStmt = new List<VariableDeclarationStatement>();
        public List<TypeDeclaration> IndexOfTypeDecl = new List<TypeDeclaration>();
    }
}

using System;
using System.Linq;

using ICSharpCode.NRefactory.CSharp;

namespace DALOptimizer
{
    class PrintFunction
    {
        public void PrintMethod(Solution solution)
        {
            Console.WriteLine("Found {0} places to refactor Field Declaration in {1} files.",
                  solution.AllFiles.Sum(f => f.IndexOfFieldDecl.Count),
                  solution.AllFiles.Count(f => f.IndexOfFieldDecl.Count > 0));
            Console.WriteLine("Found {0} places to refactor Property Declaration in {1} files.",
                  solution.AllFiles.Sum(f => f.IndexOfPropDecl.Count),
                  solution.AllFiles.Count(f => f.IndexOfPropDecl.Count > 0));
            Console.WriteLine("Found {0} places to refactor Finally Block in {1} files.",
                  solution.AllFiles.Sum(f => f.IndexOfBlockStmt.Count),
                  solution.AllFiles.Count(f => f.IndexOfBlockStmt.Count > 0));
            Console.WriteLine("Found {0} places to refactor assignment Expressions in {1} files.",
                  solution.AllFiles.Sum(f => f.IndexOfAssExpr.Count),
                  solution.AllFiles.Count(f => f.IndexOfAssExpr.Count > 0));
            Console.WriteLine("Found {0} places to refactor Expression statement in {1} files.",
                  solution.AllFiles.Sum(f => f.IndexOfExprStmt.Count),
                  solution.AllFiles.Count(f => f.IndexOfExprStmt.Count > 0));
            Console.WriteLine("Found {0} places to refactor Catch clause in {1} files.",
                  solution.AllFiles.Sum(f => f.IndexOfCtchClause.Count),
                  solution.AllFiles.Count(f => f.IndexOfCtchClause.Count > 0));
            Console.WriteLine("Found {0} places to refactor Method Declaration in {1} files.",
                  solution.AllFiles.Sum(f => f.IndexOfMthdDecl.Count),
                  solution.AllFiles.Count(f => f.IndexOfMthdDecl.Count > 0));
            Console.WriteLine("Found {0} places to refactor Variable Declaration in {1} files.",
                  solution.AllFiles.Sum(f => f.IndexOfVarDeclStmt.Count),
                  solution.AllFiles.Count(f => f.IndexOfVarDeclStmt.Count > 0));
        }

        public void PrintInvocation(AstNode invocation)
        {
            Console.WriteLine("new_Line:" + invocation.GetRegion().Begin.Line + "  Invocation: " + invocation.GetText().ToString());
        }
    }
}

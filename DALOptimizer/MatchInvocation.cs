using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;

namespace DALOptimizer
{
    class MatchInvocation
    {
        AllPatterns Pat = new AllPatterns();
        //PrintFunction PrFun = new PrintFunction();
        // For All Global field Declarations
        public void MatchFieldDecl(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            if (invocation.Parent.GetType().Name.ToString() == "TypeDeclaration")
            {
                file.IndexOfFieldDecl.Add((FieldDeclaration)invocation);
                //PrFun.PrintInvocation(invocation);
            }
        }

        // For All Global Property Declarations
        public void MatchPropDecl(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            if (invocation.Parent.GetType().Name.ToString() == "TypeDeclaration")
            {
                file.IndexOfPropDecl.Add((PropertyDeclaration)invocation);
                //PrFun.PrintInvocation(invocation);
            }
        }

        public void MatchCatchClause(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            file.IndexOfCtchClause.Add((CatchClause)invocation);
            //PrFun.PrintInvocation(invocation);
        }

        public void MatchAssExpr(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            var expr7 = Pat.sqlCmdstmt1();
            Match sqlCmdstmt1 = expr7.Match(invocation);
            if (sqlCmdstmt1.Success)
            {
                file.IndexOfAssExpr.Add((AssignmentExpression)invocation);
                //PrFun.PrintInvocation(invocation);
            }
        }

        public void MatchVarDeclStmt(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            var expr6 = Pat.sqlCmdstmt2();
            Match sqlCmdstmt2 = expr6.Match(invocation);
            var expr7 = Pat.SqlDtAdptStmt();
            Match SqlDtAdptStmt = expr7.Match(invocation);
            var expr8 = Pat.varDeclMthd();
            Match varDeclMthd = expr8.Match(invocation);

            if (sqlCmdstmt2.Success)
            {
                file.IndexOfVarDeclStmt.Add((VariableDeclarationStatement)invocation);
                //PrFun.PrintInvocation(invocation);
            }
            if (SqlDtAdptStmt.Success)
            {
                file.IndexOfVarDeclStmt.Add((VariableDeclarationStatement)invocation);
            }
            if (varDeclMthd.Success)
            {
                if (invocation.Parent.GetType().Name == "BlockStatement" && invocation.Parent.Parent.GetType().Name == "MethodDeclaration")
                {
                    file.IndexOfVarDeclStmt.Add((VariableDeclarationStatement)invocation);
                }
            }
        }

        //Match Expression Statement  //da.Fill(dt);
        public void MatchExprStmt(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            var expr1 = Pat.FillExpr();
            var expr2 = Pat.logErr();
            var expr3 = Pat.ExNonQuery();
            var expr4 = Pat.StoredProc();
            var expr5 = Pat.sqlConnstmt();
            var expr6 = Pat.ConnOpenExprStmt();
            var expr7 = Pat.ConnCloseExprStmt();
            var expr8 = Pat.CmdDisposeExprStmt();
            var expr9 = Pat.SqlDataAdapterExprStmt();
            var expr10 = Pat.ExNonQuery1();
            var expr11 = Pat.ConvertToInt32();

            Match FillExpr = expr1.Match(invocation);
            Match logErr = expr2.Match(invocation);
            Match ExNonQuery = expr3.Match(invocation);
            Match StoredProc = expr4.Match(invocation);
            Match sqlConnstmt = expr5.Match(invocation);
            Match ConnOpenExprStmt = expr6.Match(invocation);
            Match ConnCloseExprStmt = expr7.Match(invocation);
            Match CmdDisposeExprStmt = expr8.Match(invocation);
            Match SqlDataAdapterExprStmt = expr9.Match(invocation);
            Match ExNonQuery1 = expr10.Match(invocation);
            Match ConvertToInt32 = expr11.Match(invocation);

            if (FillExpr.Success || logErr.Success || ExNonQuery.Success ||
                CmdDisposeExprStmt.Success || SqlDataAdapterExprStmt.Success ||
                ConnCloseExprStmt.Success || ConvertToInt32.Success || ExNonQuery1.Success ||
                StoredProc.Success || sqlConnstmt.Success || ConnOpenExprStmt.Success)
            {
                file.IndexOfExprStmt.Add((ExpressionStatement)invocation);
                //PrFun.PrintInvocation(invocation);
            }
        }

        public void MatchMethodDecl(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            file.IndexOfMthdDecl.Add((MethodDeclaration)invocation);
            //PrFun.PrintInvocation(invocation);
        }

        public void MatchBlock(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            if (invocation.PrevSibling.GetText() == "finally")
            {
                file.IndexOfBlockStmt.Add((BlockStatement)invocation);
                //PrFun.PrintInvocation(invocation);
            }
        }
    }
}

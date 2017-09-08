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
        public const string EXPRESSIONSTATEMENT = "ExpressionStatement";
        public const string CATCHCLAUSE = "CatchClause";
        public const string FIELDDECLARATION = "FieldDeclaration";
        public const string VARIABLEDECLSTMT = "VariableDeclarationStatement";
        public const string ASSIGNMENTEXPRESSION = "AssignmentExpression";
        public const string PROPERTYDECLARATION = "PropertyDeclaration";
        public const string BLOCKSTATEMENT = "BlockStatement";
        public const string METHODDECLARARION = "MethodDeclaration";
        public const string TYPEDECLARATION = "TypeDeclaration";

        AllPatterns Pat = new AllPatterns();

        public void CheckAllInvocation(CSharpFile file, CSharpAstResolver astResolver) 
        {
            foreach (var invocation in file.syntaxTree.Descendants.OfType<AstNode>())
            {
                //track all expression Statemens
                if (invocation.GetType().Name == EXPRESSIONSTATEMENT)
                {
                    MatchExprStmt(invocation, file, astResolver);
                    continue;
                }
                //catch clause
                if (invocation.GetType().Name == CATCHCLAUSE)
                {
                    MatchCatchClause(invocation, file, astResolver);
                    continue;
                }

                // For All Global Field Declarations
                if (invocation.GetType().Name == FIELDDECLARATION)
                {
                    MatchFieldDecl(invocation, file, astResolver);
                    continue;
                }
                // For variable Decaration of type {SqlCommand cmd = new SqlCommand ("dbo.InboxDeviceReport", con);}
                if (invocation.GetType().Name == VARIABLEDECLSTMT)
                {
                    MatchVarDeclStmt(invocation, file, astResolver);
                    continue;
                }

                if (invocation.GetType().Name == ASSIGNMENTEXPRESSION)
                {
                    MatchAssExpr(invocation, file, astResolver);
                    continue;
                }

                //check Global Property Declaration
                if (invocation.GetType().Name == PROPERTYDECLARATION)
                {
                    MatchPropDecl(invocation, file, astResolver);
                    continue;
                }

                //check Finally Block
                if (invocation.GetType().Name == BLOCKSTATEMENT)
                {
                    MatchBlock(invocation, file, astResolver);
                    continue;
                }

                if (invocation.GetType().Name == METHODDECLARARION)
                {
                    MatchMethodDecl(invocation, file, astResolver);
                    continue;
                }
            }
        }

        // For All Global field Declarations eg. PrintFunction PrFun = new PrintFunction();
        public void MatchFieldDecl(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            if (invocation.Parent.GetType().Name.ToString() == TYPEDECLARATION)
                file.IndexOfFieldDecl.Add((FieldDeclaration)invocation);
        }

        // For All Global Property Declarations
        public void MatchPropDecl(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            if (invocation.Parent.GetType().Name.ToString() == TYPEDECLARATION)
                file.IndexOfPropDecl.Add((PropertyDeclaration)invocation);
        }

        public void MatchCatchClause(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            file.IndexOfCtchClause.Add((CatchClause)invocation);
        }

        public void MatchAssExpr(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            Match sqlCmdstmt1 = Pat.sqlCmdstmt1().Match(invocation);
            if (sqlCmdstmt1.Success)
                file.IndexOfAssExpr.Add((AssignmentExpression)invocation);
        }

        public void MatchVarDeclStmt(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            Match sqlCmdstmt2 = Pat.sqlCmdstmt2().Match(invocation);
            Match SqlDtAdptStmt = Pat.SqlDtAdptStmt().Match(invocation);
            Match varDeclMthd = Pat.varDeclMthd().Match(invocation);

            if (sqlCmdstmt2.Success || SqlDtAdptStmt.Success)
                file.IndexOfVarDeclStmt.Add((VariableDeclarationStatement)invocation);
            else if (varDeclMthd.Success)
            {
                if (invocation.Parent.GetType().Name == BLOCKSTATEMENT && invocation.Parent.Parent.GetType().Name == METHODDECLARARION)
                {
                    file.IndexOfVarDeclStmt.Add((VariableDeclarationStatement)invocation);
                }
            }
        }

        //Match Expression Statement  //da.Fill(dt);
        public void MatchExprStmt(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            if (Pat.FillExpr().Match(invocation).Success) { file.IndexOfExprStmt.Add((ExpressionStatement)invocation); }
            else if (Pat.logErr().Match(invocation).Success) { file.IndexOfExprStmt.Add((ExpressionStatement)invocation); }
            else if (Pat.ExNonQuery().Match(invocation).Success) { file.IndexOfExprStmt.Add((ExpressionStatement)invocation); }
            else if (Pat.StoredProc().Match(invocation).Success) { file.IndexOfExprStmt.Add((ExpressionStatement)invocation); }
            else if (Pat.sqlConnstmt().Match(invocation).Success) { file.IndexOfExprStmt.Add((ExpressionStatement)invocation); }
            else if (Pat.ConnOpenExprStmt().Match(invocation).Success) { file.IndexOfExprStmt.Add((ExpressionStatement)invocation); }
            else if (Pat.ConnCloseExprStmt().Match(invocation).Success) { file.IndexOfExprStmt.Add((ExpressionStatement)invocation); }
            else if (Pat.CmdDisposeExprStmt().Match(invocation).Success) { file.IndexOfExprStmt.Add((ExpressionStatement)invocation); }
            else if (Pat.SqlDataAdapterExprStmt().Match(invocation).Success) { file.IndexOfExprStmt.Add((ExpressionStatement)invocation); }
            else if (Pat.ExNonQuery1().Match(invocation).Success) { file.IndexOfExprStmt.Add((ExpressionStatement)invocation); }
            else if (Pat.ConvertToInt32().Match(invocation).Success) { file.IndexOfExprStmt.Add((ExpressionStatement)invocation); }
        }

        public void MatchMethodDecl(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            file.IndexOfMthdDecl.Add((MethodDeclaration)invocation);
        }

        public void MatchBlock(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            if (invocation.PrevSibling.GetText() == "finally")
                file.IndexOfBlockStmt.Add((BlockStatement)invocation);
        }
    }
}

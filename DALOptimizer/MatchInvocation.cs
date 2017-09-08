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
                var invocationTypeName = invocation.GetType().Name;
                switch (invocationTypeName)
                { 
                    case EXPRESSIONSTATEMENT: MatchExprStmt(invocation, file, astResolver); break; //track all expression Statemens
                    case CATCHCLAUSE: MatchCatchClause(invocation, file, astResolver); break;  //catch clause
                    case FIELDDECLARATION: MatchFieldDecl(invocation, file, astResolver); break; // For All Global Field Declarations
                    case VARIABLEDECLSTMT: MatchVarDeclStmt(invocation, file, astResolver); break; //eg. SqlCommand cmd = new SqlCommand ("dbo.InboxDeviceReport", con);}
                    case ASSIGNMENTEXPRESSION: MatchAssExpr(invocation, file, astResolver); break; 
                    case PROPERTYDECLARATION: MatchPropDecl(invocation, file, astResolver); break;
                    case BLOCKSTATEMENT: MatchBlock(invocation, file, astResolver); break;  // check all finaly blocks
                    case METHODDECLARARION: MatchMethodDecl(invocation, file, astResolver); break; 
                }
            }
        }

        // For All Global field Declarations eg. PrintFunction PrFun = new PrintFunction();
        private void MatchFieldDecl(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            if (invocation.Parent.GetType().Name.ToString() == TYPEDECLARATION)
                file.IndexOfFieldDecl.Add((FieldDeclaration)invocation);
        }

        // For All Global Property Declarations
        private void MatchPropDecl(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            if (invocation.Parent.GetType().Name.ToString() == TYPEDECLARATION)
                file.IndexOfPropDecl.Add((PropertyDeclaration)invocation);
        }

        private void MatchCatchClause(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            file.IndexOfCtchClause.Add((CatchClause)invocation);
        }

        private void MatchAssExpr(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            if (Pat.sqlCmdstmt1().Match(invocation).Success)
                file.IndexOfAssExpr.Add((AssignmentExpression)invocation);
        }

        private void MatchVarDeclStmt(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            if (Pat.sqlCmdstmt2().Match(invocation).Success || Pat.SqlDtAdptStmt().Match(invocation).Success)
                file.IndexOfVarDeclStmt.Add((VariableDeclarationStatement)invocation);
            else if (Pat.varDeclMthd().Match(invocation).Success)
            {
                if (invocation.Parent.GetType().Name == BLOCKSTATEMENT && invocation.Parent.Parent.GetType().Name == METHODDECLARARION)
                {
                    file.IndexOfVarDeclStmt.Add((VariableDeclarationStatement)invocation);
                }
            }
        }

        //Match Expression Statement  //da.Fill(dt);
        private void MatchExprStmt(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            ExpressionStatement[] expressionStatements = new ExpressionStatement[] { 
                Pat.FillExpr(), Pat.logErr(), Pat.ExNonQuery(), Pat.StoredProc(), 
                Pat.sqlConnstmt(), Pat.ConnOpenExprStmt(), Pat.ConnCloseExprStmt(), 
                Pat.CmdDisposeExprStmt(), Pat.SqlDataAdapterExprStmt(), Pat.ExNonQuery1(), Pat.ConvertToInt32() };

            foreach (ExpressionStatement expressionStatement in expressionStatements) {
                if (expressionStatement.Match(invocation).Success) {
                    file.IndexOfExprStmt.Add((ExpressionStatement)invocation);
                    break;
                }
            }
        }

        private void MatchMethodDecl(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            file.IndexOfMthdDecl.Add((MethodDeclaration)invocation);
        }

        private void MatchBlock(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            if (invocation.PrevSibling.GetText() == "finally")
                file.IndexOfBlockStmt.Add((BlockStatement)invocation);
        }
    }
}

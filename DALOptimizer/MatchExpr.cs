using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.TypeSystem;
//using Nrefactory = ICSharpCode.NRefactory.PatternMatching;

namespace DALOptimizer
{
    class MatchExpr
    {
        public StringBuilderDocument CheckAllExpressions(CSharpFile file)
        { 
            file.syntaxTree.Freeze();
            var compilation = file.project.Compilation;
            var astResolver = new CSharpAstResolver(compilation, file.syntaxTree, file.unresolvedTypeSystemForFile);

            // Create a document containing the file content:
            var document = new StringBuilderDocument(file.originalText);
            var formattingOptions = FormattingOptionsFactory.CreateAllman();
            var options = new TextEditorOptions();

            using (var script = new DocumentScript(document, formattingOptions, options))
            {
                GlobalFieldDeclaration(script, file);
                GlobalPropertyDeclaraton(script, file);
                BlockStat(script, file);
                CatchClaus(script,file);
                AssignmentExpr(script, file);
                VarDeclarationStmt(script, file);
                ExprStatement(script, file);
                MethodDecl(script, file);
            }
            return document;
        }

        private void GlobalFieldDeclaration(DocumentScript script, CSharpFile file)
        {
            // for global declarations
            foreach (FieldDeclaration expr in file.IndexOfFieldDecl)
            {
                var copy = (FieldDeclaration)expr.Clone();

                AllPatterns pat = new AllPatterns();
                var pattern = pat.ConnectionClassconnectExpr();
                if (expr.GetText().Contains(pattern.GetText()))   // Replace ConnectionClass.connect() with DatabaseProcessing stmt
                    script.Replace(expr, pat.DbProcessing());
                else                                                // rest of all global declarations are removed
                    script.Remove(expr, true);
            }

        }

        private void GlobalPropertyDeclaraton(DocumentScript script, CSharpFile file)
        {
            // for global property declarations
            foreach (PropertyDeclaration expr in file.IndexOfPropDecl)
            {
                var copy = (PropertyDeclaration)expr.Clone();
                AllPatterns pat = new AllPatterns();
                var pattern = pat.ConnectionClassconnectExpr();
                script.Remove(expr, true);
            }
        }
        private void BlockStat(DocumentScript script, CSharpFile file)
        {
            foreach (var expr in file.IndexOfBlockStmt)
            {
                var copy = (BlockStatement)expr.Clone();
                AllPatterns Pat = new AllPatterns();
                script.Replace(expr, Pat.FinalyBlck());
            }
        }
        private void CatchClaus(DocumentScript script, CSharpFile file) 
        {
            foreach (var expr in file.IndexOfCtchClause)
            {
                var copy = (CatchClause)expr.Clone();
                AllPatterns Pat = new AllPatterns();
                script.Replace(expr, Pat.ctchclause());
            }
        }

        private void AssignmentExpr(DocumentScript script, CSharpFile file)
        {
            foreach (var expr in file.IndexOfAssExpr)
            {
                var copy = (AssignmentExpression)expr.Clone();
                AllPatterns Pat = new AllPatterns();
                if (Pat.sqlCmdstmt().Match(expr).Success)
                {
                    // Removing Conn Object from SP statement
                    int startOffset = script.GetCurrentOffset(expr.LastChild.LastChild.PrevSibling.PrevSibling.StartLocation);
                    int endOffset = script.GetCurrentOffset(expr.LastChild.LastChild.PrevSibling.EndLocation);
                    script.RemoveText(startOffset, endOffset - startOffset);

                    //inserting "dbo." in SP variable
                    var SPName = expr.LastChild.FirstChild.NextSibling.NextSibling.NextSibling;
                    int curOffset = script.GetCurrentOffset(SPName.StartLocation);
                    if (!SPName.GetText().Contains("dbo.") && !SPName.GetText().Contains(" "))
                        script.InsertText(curOffset + 1, "dbo.");

                    // inserting SqlCommand before first declaration for stored procedure in method
                    CheckSqlCmdDecl(expr, script);
                }
                else if (Pat.sqlCmdstmt1().Match(expr).Success)
                {
                    // inserting SqlCommand before first declaration for stored procedure in method
                    CheckSqlCmdDecl(expr,script);
                }
            }
        }

        private void CheckSqlCmdDecl(AssignmentExpression expr, DocumentScript script)
        {
            AllPatterns Pat = new AllPatterns();
            var SqlCmdStmtVarDecl = Pat.SqlCmdStmtVarDecl();
            foreach (var expression in expr.Parent.Parent.Children.OfType<VariableDeclarationStatement>())
            {
                if (SqlCmdStmtVarDecl.Match(expression).Success)
                    return;
            }
            int start = script.GetCurrentOffset(expr.StartLocation);
            script.InsertText(start, "SqlCommand ");
        }

        private void VarDeclarationStmt(DocumentScript script, CSharpFile file)
        {
            foreach (var expr in file.IndexOfVarDeclStmt)
            {
                var copy = (VariableDeclarationStatement)expr.Clone();
                AllPatterns Pat = new AllPatterns();
                
                if (Pat.sqlCmdstmt2().Match(expr).Success)
                {
                    var Tempvar = expr.FirstChild.NextSibling.LastChild.LastChild.PrevSibling;
                    int start = script.GetCurrentOffset(Tempvar.PrevSibling.StartLocation);
                    int end = script.GetCurrentOffset(Tempvar.EndLocation);
                    script.RemoveText(start, end - start);
                }
                else if (Pat.SqlDtAdptStmt().Match(expr).Success)
                {
                    string retType = expr.GetParent<MethodDeclaration>().ReturnType.GetText();
                    if (retType.Contains("DataTable") || retType.Contains("DataSet"))
                    {
                        DataTableSetExpression(expr, script, retType);
                    }
                    else
                    {
                        //set method return type as dummy
                        int Len = expr.GetParent<MethodDeclaration>().ReturnType.ToString().Length;
                        int startOffset = script.GetCurrentOffset(expr.GetParent<MethodDeclaration>().ReturnType.StartLocation);
                        script.RemoveText(startOffset, Len);
                        script.InsertText(startOffset, "DummyText");
                    }
                }
                else if (Pat.varDeclMthd().Match(expr).Success)
                {
                    if (expr.Parent.GetType().Name == "BlockStatement" &&
                        expr.Parent.Parent.GetType().Name == "MethodDeclaration")
                    {
                        var varName = expr.FirstChild.NextSibling.FirstChild.GetText();
                        script.Replace(expr, Pat.varDeclMthd1(varName));
                    }
                }
            }
        }

        
        private void DataTableSetExpression(AstNode expr, DocumentScript script, string retType)
        {
            string varName = expr.GetText().Split("()".ToCharArray())[1];
            AllPatterns Pat = new AllPatterns();
            ExpressionStatement fillExpr = Pat.FillExpr();

            var MatchedExpression = ExpressionStatement.Null;
            foreach (ExpressionStatement expressionStatement in expr.Parent.Descendants.OfType<ExpressionStatement>())
            {
                if (fillExpr.Match(expressionStatement).Success)
                {
                    MatchedExpression = expressionStatement;
                    break;
                }
            }

            if (MatchedExpression != ExpressionStatement.Null)
            {
                string varName1 = MatchedExpression.LastChild.PrevSibling.LastChild.PrevSibling.GetText();

                if (retType.Contains("DataTable"))
                {
                    InsertDtTblSetExpr(expr, script, "DataTable", varName1);
                    script.Replace(expr, Pat.GetDtTbl(varName, varName1));    
                }
                else if (retType.Contains("DataSet")) 
                {
                    //InsertDtTblSetExpr(expr, script, "DataSet");
                    script.Replace(expr, Pat.GetDtSet(varName, varName1));
                }
            }
            else
            {
                script.InsertText(script.GetCurrentOffset(expr.StartLocation), "test.Fill (test); Not found");
            }
        }

        private void InsertDtTblSetExpr(AstNode expr, DocumentScript script, string retType, string dtsVar)
        {
            AllPatterns Pat = new AllPatterns();
            var dataTableSetExpr = Pat.DataTableSetExpr(retType, dtsVar);
            var dataTableSetStmt = Pat.DataTableSetStmt(retType, dtsVar);
            foreach (var expression in expr.GetParent<MethodDeclaration>().LastChild.Children.OfType<ExpressionStatement>())
            {
                if (dataTableSetExpr.Match(expression).Success)
                {
                    script.Replace(expression, dataTableSetStmt);
                    return;
                }
            }
            foreach (var expression in expr.GetParent<MethodDeclaration>().LastChild.Children.OfType<VariableDeclarationStatement>())
            {

                if (dataTableSetStmt.Match(expression).Success)
                {
                    return;
                }
            }
            script.InsertBefore(expr.GetParent<MethodDeclaration>().LastChild.FirstChild.NextSibling, dataTableSetStmt);
        }

        private void ExprStatement(DocumentScript script, CSharpFile file)
        {
            foreach (var expr in file.IndexOfExprStmt)
            {
                var copy = (ExpressionStatement)expr.Clone();
                AllPatterns Pat = new AllPatterns();

                Func<ExpressionStatement>[] functions = {
                    Pat.FillExpr, Pat.StoredProc, Pat.sqlConnstmt, Pat.ConnOpenExprStmt, 
                    Pat.ConnCloseExprStmt, Pat.CmdDisposeExprStmt, Pat.ConvertToInt32, Pat.ConnectionStmt};

                foreach (var function in functions)
                {
                    if (function().Match(expr).Success)
                    {
                        script.Remove(expr, true);
                        break;
                    }
                }
                
                if (Pat.ExNonQuery().Match(expr).Success)
                {
                    string varName = expr.FirstChild.FirstChild.GetText();
                    string objName = expr.FirstChild.LastChild.FirstChild.FirstChild.GetText();
                    var expr1 = Pat.ExeStrdProc(varName, objName);
                    script.Replace(expr, expr1);
                    bool foundVarDecl = false;
                    var parentMtdhDecl = expr.GetParent<MethodDeclaration>();
                    foreach (var VarDeclInMethod in parentMtdhDecl.LastChild.Children.OfType<VariableDeclarationStatement>())
                    {
                        if (VarDeclInMethod.Variables.First().Name == varName)
                        {
                            foundVarDecl = true;
                            break;
                        }
                    }
                    if (foundVarDecl == false)
                        script.InsertBefore(parentMtdhDecl.LastChild.FirstChild.NextSibling, Pat.varDeclMthd1(varName));

                    continue;
                }

                if (Pat.SqlDataAdapterExprStmt().Match(expr).Success)
                {
                    string retType = expr.GetParent<MethodDeclaration>().ReturnType.GetText();
                    if (retType.Contains("DataTable") || retType.Contains("DataSet"))
                    {  
                        DataTableSetExpression(expr, script, retType);
                    }
                    else
                    {
                        //set method return type as dummy
                        int Len = expr.GetParent<MethodDeclaration>().ReturnType.ToString().Length;
                        int startOffset = script.GetCurrentOffset(expr.GetParent<MethodDeclaration>().ReturnType.StartLocation);
                        script.RemoveText(startOffset, Len);
                        script.InsertText(startOffset, "DummyText");
                    }
                    continue;
                }
                if (Pat.ExNonQuery1().Match(expr).Success)
                {
                    var MtchExpr = VariableDeclarationStatement.Null;
                    string Output = "Output";
                    string sqlCmdVar = expr.FirstChild.FirstChild.FirstChild.GetText();
                    foreach (var varDeclStmt in expr.Parent.Descendants.OfType<VariableDeclarationStatement>())
                    {
                        if (Pat.sqlParameter().Match(varDeclStmt).Success)
                        {
                            MtchExpr = varDeclStmt;
                            break;
                        }
                    }
                    if (MtchExpr != VariableDeclarationStatement.Null)
                    {
                        Output = MtchExpr.FirstChild.NextSibling.FirstChild.GetText();
                        script.Replace(expr, Pat.gtOtptParameter(sqlCmdVar, Output));
                    }
                    continue;
                }
            }
        }

        private void MethodDecl(DocumentScript script, CSharpFile file)
        {
            foreach (var expr in file.IndexOfMthdDecl)
            {
                var copy = (MethodDeclaration)expr.Clone();
                var chldOfTypPar = expr.GetChildByRole(Roles.Parameter);
                if (chldOfTypPar != null)
                {
                    string input = Regex.Replace(chldOfTypPar.GetText(), @"\w+\.\b", "");
                    int offset = script.GetCurrentOffset(chldOfTypPar.StartLocation);
                    script.RemoveText(offset, chldOfTypPar.GetText().Length);
                    script.InsertText(offset, input);
                }
            }
        }
    }
}

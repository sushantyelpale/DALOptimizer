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
    class ModifyExpressions
    {
        AllPatterns allPatterns = new AllPatterns();

        public StringBuilderDocument CheckAllExpressions(CSharpFile file, string loggerClassName)
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
                WriteDataBaseProcessingStatement(script, file);
                FinallyBlockStatement(script, file);
                CatchClause(script,file, loggerClassName);
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

                if (expr.GetText().Contains(allPatterns.ConnectionClassconnectExpr().GetText()))   // Remove ConnectionClass.connect() expression
                    script.Remove(expr, true);
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
                if (expr.Match(allPatterns.ConnectionClassconnectExpr()).Success)
                    script.Remove(expr, true);
            }
        }
        private void WriteDataBaseProcessingStatement(DocumentScript script, CSharpFile file)
        {
            foreach (var expr in file.IndexOfTypeDecl)
            {
                var copy = (TypeDeclaration)expr.Clone();
                var startInClass = expr.GetChildByRole(Roles.LBrace).NextSibling;
                script.InsertBefore(startInClass, allPatterns.DbProcessing());
            }
        }

        private void FinallyBlockStatement(DocumentScript script, CSharpFile file)
        {
            foreach (BlockStatement expr in file.IndexOfBlockStmt)
            {
                var copy = (BlockStatement)expr.Clone();
                script.Replace(expr, allPatterns.FinalyBlock());
            }
        }
        private void CatchClause(DocumentScript script, CSharpFile file, string loggerClassName) 
        {
            foreach (CatchClause expr in file.IndexOfCtchClause)
            {
                var copy = (CatchClause)expr.Clone();
                script.Replace(expr, allPatterns.catchclause(loggerClassName));
            }
        }

        private void AssignmentExpr(DocumentScript script, CSharpFile file)
        {
            foreach (var expr in file.IndexOfAssExpr)
            {
                var copy = (AssignmentExpression)expr.Clone();
                if (allPatterns.SqlCmdStmt().Match(expr).Success)
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
                else if (allPatterns.SqlCmdStmtAnyNode().Match(expr).Success)
                {
                    // inserting SqlCommand before first declaration for stored procedure in method
                    CheckSqlCmdDecl(expr,script);
                }
            }
        }

        private void CheckSqlCmdDecl(AssignmentExpression expr, DocumentScript script)
        {
            var SqlCmdStmtVarDecl = allPatterns.SqlCmdStmtVarDecl();
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
                
                if (allPatterns.SqlCmdstmtTwoArgs().Match(expr).Success)
                {
                    var Tempvar = expr.FirstChild.NextSibling.LastChild.LastChild.PrevSibling;
                    int start = script.GetCurrentOffset(Tempvar.PrevSibling.StartLocation);
                    int end = script.GetCurrentOffset(Tempvar.EndLocation);
                    script.RemoveText(start, end - start);
                }
                else if (allPatterns.SqlDtAdptStmt().Match(expr).Success)
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
                else if (allPatterns.varDeclMethodZero().Match(expr).Success)
                {
                    if (expr.Parent.GetType().Name == "BlockStatement" &&
                        expr.Parent.Parent.GetType().Name == "MethodDeclaration")
                    {
                        var varName = expr.FirstChild.NextSibling.FirstChild.GetText();
                        script.Replace(expr, allPatterns.varDeclMethodMinusOne(varName));
                    }
                }
            }
        }

        private void DataTableSetExpression(AstNode expr, DocumentScript script, string retType)
        {
            string varName = expr.GetText().Split("()".ToCharArray())[1];
            ExpressionStatement fillExpr = allPatterns.FillExpr();

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
                    script.Replace(expr, allPatterns.GetDtTbl(varName, varName1));    
                }
                else if (retType.Contains("DataSet")) 
                {
                    //InsertDtTblSetExpr(expr, script, "DataSet");
                    script.Replace(expr, allPatterns.GetDtSet(varName, varName1));
                }
            }
            else
            {
                script.InsertText(script.GetCurrentOffset(expr.StartLocation), "test.Fill (test); Not found");
            }
        }

        private void InsertDtTblSetExpr(AstNode expr, DocumentScript script, string retType, string dtsVar)
        {
            var dataTableSetExpr = allPatterns.DataTableSetExpr(retType, dtsVar);
            var dataTableSetStmt = allPatterns.DataTableSetStmt(retType, dtsVar);
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

                Func<ExpressionStatement>[] functions = {
                    allPatterns.FillExpr, allPatterns.StoredProc, allPatterns.SqlConnStmt, 
                    allPatterns.ConnOpenExprStmt, allPatterns.ConnCloseExprStmt, 
                    allPatterns.CmdDisposeExprStmt, allPatterns.ConvertToInt32, allPatterns.ConnectionStmt};

                foreach (var function in functions)
                {
                    if (function().Match(expr).Success)
                    {
                        script.Remove(expr, true);
                        break;
                    }
                }
                
                if (allPatterns.ExNonQueryVarAssignment().Match(expr).Success)
                {
                    string varName = expr.FirstChild.FirstChild.GetText();
                    string objName = expr.FirstChild.LastChild.FirstChild.FirstChild.GetText();
                    var expr1 = allPatterns.ExeStrdProc(varName, objName);
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
                        script.InsertBefore(parentMtdhDecl.LastChild.FirstChild.NextSibling, allPatterns.varDeclMethodMinusOne(varName));

                    continue;
                }

                if (allPatterns.SqlDataAdapterExprStmt().Match(expr).Success)
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
                if (allPatterns.ExNonQueryDecl().Match(expr).Success)
                {
                    var MtchExpr = VariableDeclarationStatement.Null;
                    string Output = "Output";
                    string sqlCmdVar = expr.FirstChild.FirstChild.FirstChild.GetText();
                    foreach (var varDeclStmt in expr.Parent.Descendants.OfType<VariableDeclarationStatement>())
                    {
                        if (allPatterns.sqlParameter().Match(varDeclStmt).Success)
                        {
                            MtchExpr = varDeclStmt;
                            break;
                        }
                    }
                    if (MtchExpr != VariableDeclarationStatement.Null)
                    {
                        Output = MtchExpr.FirstChild.NextSibling.FirstChild.GetText();
                        script.Replace(expr, allPatterns.gtOtptParameter(sqlCmdVar, Output));
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

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
                    int start = script.GetCurrentOffset(expr.LastChild.LastChild.PrevSibling.PrevSibling.StartLocation);
                    int end = script.GetCurrentOffset(expr.LastChild.LastChild.PrevSibling.EndLocation);
                    script.RemoveText(start, end - start);

                    //inserting "dbo." in SP variable
                    var SPName = expr.LastChild.FirstChild.NextSibling.NextSibling.NextSibling;
                    int curOffset = script.GetCurrentOffset(SPName.StartLocation);
                    if (!SPName.GetText().Contains("dbo.") && !SPName.GetText().Contains(" "))
                        script.InsertText(curOffset + 1, "dbo.");
                }
                if (Pat.sqlCmdstmt1().Match(expr).Success)
                {
                    // inserting SqlCommand before first declaration for stored procedure in method
                    int start = script.GetCurrentOffset(expr.StartLocation);
                    script.InsertText(start, "SqlCommand ");
                }
            }

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
                if (Pat.SqlDtAdptStmt().Match(expr).Success)
                {
                    string MthdretType = expr.GetParent<MethodDeclaration>().ReturnType.GetText();
                    if (MthdretType == "DataTable" || MthdretType == "System.Data.DataTable")
                    {
                        string varName = expr.GetText().Split("()".ToCharArray())[1];
                        var getDtTbl = Pat.GetDtTbl(varName);
                        script.Replace(expr, getDtTbl);
                    }
                    else if (MthdretType == "DataSet" || MthdretType == "System.Data.DataSet")
                    {
                        string varName = expr.GetText().Split("()".ToCharArray())[1];
                        var getDtSet = Pat.GetDtSet(varName);
                        script.Replace(expr, getDtSet);
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
                if (Pat.varDeclMthd().Match(expr).Success)
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
        private void ExprStatement(DocumentScript script, CSharpFile file)
        {
            foreach (var expr in file.IndexOfExprStmt)
            {
                var copy = (ExpressionStatement)expr.Clone();
                AllPatterns Pat = new AllPatterns();
                ExpressionStatement[] expressionStatements = new ExpressionStatement[] { 
                    Pat.FillExpr(), Pat.StoredProc(), Pat.sqlConnstmt(), Pat.ConnOpenExprStmt(), 
                    Pat.ConnCloseExprStmt(), Pat.CmdDisposeExprStmt(), Pat.ConvertToInt32() };

                foreach (ExpressionStatement expressionStatement in expressionStatements)
                {
                    if (expressionStatement.Match(expr).Success)
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
                    continue;
                }

                if (Pat.SqlDataAdapterExprStmt().Match(expr).Success)
                {
                    string retType = expr.GetParent<MethodDeclaration>().ReturnType.GetText();
                    if (retType == "DataTable" || retType == "System.Data.DataTable")  
                    {
                        string varName = expr.GetText().Split("()".ToCharArray())[1];
                        var getDtTbl = Pat.GetDtTbl(varName);
                        script.Replace(expr, getDtTbl);
                    }
                    else if (retType == "DataSet" || retType == "System.Data.DataSet")
                    {
                        string varName = expr.GetText().Split("()".ToCharArray())[1];
                        var getDtSet = Pat.GetDtSet(varName);
                        script.Replace(expr, getDtSet);
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
                    var sqlParameterExpr = Pat.sqlParameter();
                    foreach (var varDeclStmt in expr.Parent.Descendants.OfType<VariableDeclarationStatement>())
                    {
                        ICSharpCode.NRefactory.PatternMatching.Match sqlParameter = sqlParameterExpr.Match(varDeclStmt);
                        if (sqlParameter.Success)
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

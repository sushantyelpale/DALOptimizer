using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using System.Text.RegularExpressions;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.TypeSystem;

namespace DALOptimizer
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {

            OpenFileDialog opnfd = new OpenFileDialog();
            opnfd.ShowDialog();
            string flName = opnfd.FileName;
            
            if (string.IsNullOrWhiteSpace(flName))
            {
                MessageBox.Show("Please Select one file ");
                return;
            }

            MatchInvocation Mr = new MatchInvocation();
            Solution solution = new Solution(flName);
            MatchExpr MtchExp = new MatchExpr();

            // Capture patterns & store it in respective lists of Node patterns

            foreach (var file in solution.AllFiles)
            {

/*                if (file.FileName != "D:\\DALProject\\DAL\\RecharegeReportDAL.cs")
                {
                    continue;
                }
                */

                if (Path.GetFileName(file.FileName) == "DatabaseProcessing.cs")
                {
                    continue;
                }
                var astResolver = new CSharpAstResolver(file.Project.Compilation, file.SyntaxTree, file.UnresolvedTypeSystemForFile);
                foreach (var invocation in file.SyntaxTree.Descendants.OfType<AstNode>())
                {

                    //trak all expression Statemens
                    if (invocation.GetType().Name == "ExpressionStatement")
                    {
                        Mr.MatchExprStmt(invocation, file, astResolver);
                        continue;
                    }
                    //catch clause
                    if (invocation.GetType().Name == "CatchClause")
                    {
                        Mr.MatchCatchClause(invocation, file, astResolver);
                        continue;
                    }

                    // For All Global Field Declarations
                    if (invocation.GetType().Name == "FieldDeclaration")
                    {
                        Mr.MatchFieldDecl(invocation, file, astResolver);
                        continue;
                    }
                    // For variable Decaration of type {SqlCommand cmd = new SqlCommand ("dbo.InboxDeviceReport", con);}
                    if (invocation.GetType().Name == "VariableDeclarationStatement")
                    {
                        Mr.MatchVarDeclStmt(invocation, file, astResolver);
                        continue;
                    }

                    if (invocation.GetType().Name == "AssignmentExpression")
                    {
                        Mr.MatchAssExpr(invocation, file, astResolver);
                        continue;
                    }

                    //check Global Property Declaration
                    if (invocation.GetType().Name == "PropertyDeclaration")
                    {
                        Mr.MatchPropDecl(invocation, file, astResolver);
                        continue;
                    }

                    //check Finally Block
                    if (invocation.GetType().Name == "BlockStatement")
                    {
                        Mr.MatchBlock(invocation, file, astResolver);
                        continue;
                    }

                    if (invocation.GetType().Name == "MethodDeclaration")
                    {
                        Mr.MatchMethodDecl(invocation, file, astResolver);
                        continue;
                    }
                }
            }
            //PrintFunction pr = new PrintFunction();
            //pr.PrintMethod(solution);

            Console.Write("Apply refactorings? ");
            string answer = Console.ReadLine();
            if ("yes".Equals(answer, StringComparison.OrdinalIgnoreCase) || "y".Equals(answer, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var file in solution.AllFiles)
                {

                    if (file.IndexOfFieldDecl.Count == 0 && file.IndexOfPropDecl.Count == 0 &&
                        file.IndexOfAssExpr.Count == 0 && file.IndexOfBlockStmt.Count == 0 &&
                        file.IndexOfCtchClause.Count == 0 && file.IndexOfExprStmt.Count == 0 &&
                        file.IndexOfMthdDecl.Count == 0 && file.IndexOfPropDecl.Count == 0 &&
                        file.IndexOfVarDeclStmt.Count == 0)
                        continue;
                    file.SyntaxTree.Freeze();

                    var compilation = file.Project.Compilation;
                    var astResolver = new CSharpAstResolver(compilation, file.SyntaxTree, file.UnresolvedTypeSystemForFile);

                    // Create a document containing the file content:
                    var document = new StringBuilderDocument(file.OriginalText);
                    var formattingOptions = FormattingOptionsFactory.CreateAllman();
                    var options = new TextEditorOptions();

                    using (var script = new DocumentScript(document, formattingOptions, options))
                    {
                        // for global declarations
                        foreach (FieldDeclaration expr in file.IndexOfFieldDecl)
                        {
                            var copy = (FieldDeclaration)expr.Clone();

                            AllPatterns pat = new AllPatterns();
                            var pattern = pat.ConnectionClassconnectExpr();

                            if (expr.GetText().Contains(pattern.GetText()))   // Replace ConnectionClass.connect() with DatabaseProcessing stmt
                            {
                                script.Replace(expr, pat.DbProcessing());
                            }
                            else                                                // rest of all global declarations are removed
                            {
                                script.Remove(expr, true);
                            }
                        }

                        // for global property declarations
                        foreach (PropertyDeclaration expr in file.IndexOfPropDecl)
                        {
                            var copy = (PropertyDeclaration)expr.Clone();

                            AllPatterns pat = new AllPatterns();
                            var pattern = pat.ConnectionClassconnectExpr();
                            {
                                script.Remove(expr, true);
                            }
                        }

                        foreach (var expr in file.IndexOfBlockStmt)
                        {
                            var copy = (BlockStatement)expr.Clone();
                            AllPatterns Pat = new AllPatterns();
                            script.Replace(expr, Pat.FinalyBlck());
                        }

                        foreach (var expr in file.IndexOfCtchClause)
                        {
                            var copy = (CatchClause)expr.Clone();
                            AllPatterns Pat = new AllPatterns();
                            script.Replace(expr, Pat.ctchclause());
                        }

                        foreach (var expr in file.IndexOfAssExpr)
                        {
                            var copy = (AssignmentExpression)expr.Clone();
                            
                            AllPatterns Pat = new AllPatterns();
                            var expr3 = Pat.sqlCmdstmt();
                            var expr4 = Pat.sqlCmdstmt1();
                            ICSharpCode.NRefactory.PatternMatching.Match sqlCmdstmt = expr3.Match(expr);
                            ICSharpCode.NRefactory.PatternMatching.Match sqlCmdstmt1 = expr4.Match(expr);

                            if (sqlCmdstmt.Success)
                            {
                                // Removing Conn Object from SP statement
                                int start = script.GetCurrentOffset(expr.LastChild.LastChild.PrevSibling.PrevSibling.StartLocation);
                                int end = script.GetCurrentOffset(expr.LastChild.LastChild.PrevSibling.EndLocation);
                                script.RemoveText(start, end - start);

                                //inserting "dbo." in SP variable
                                var SPName = expr.LastChild.FirstChild.NextSibling.NextSibling.NextSibling;
                                int curOffset = script.GetCurrentOffset(SPName.StartLocation);
                                
                                if (!SPName.GetText().Contains("dbo.") && !SPName.GetText().Contains(" "))
                                {
                                    script.InsertText(curOffset+1, "dbo.");
                                }
                            }
                            if (sqlCmdstmt1.Success)
                            {
                                // inserting SqlCommand before first declaration for stored procedure in method
                                int start = script.GetCurrentOffset(expr.StartLocation);
                                script.InsertText(start, "SqlCommand ");
                            }
                        }

                        foreach (var expr in file.IndexOfVarDeclStmt)
                        {
                            var copy = (VariableDeclarationStatement)expr.Clone();

                            AllPatterns Pat = new AllPatterns();
                            var expr1 = Pat.sqlCmdstmt2();
                            var expr2 = Pat.SqlDtAdptStmt();
                            var expr3 = Pat.varDeclMthd();
                            ICSharpCode.NRefactory.PatternMatching.Match sqlCmdstmt2 = expr1.Match(expr);
                            ICSharpCode.NRefactory.PatternMatching.Match SqlDtAdptStmt = expr2.Match(expr);
                            ICSharpCode.NRefactory.PatternMatching.Match varDeclMthd = expr3.Match(expr);
                            if (sqlCmdstmt2.Success)
                            {
                                var Tempvar = expr.FirstChild.NextSibling.LastChild.LastChild.PrevSibling;
                                int start = script.GetCurrentOffset(Tempvar.PrevSibling.StartLocation);
                                int end = script.GetCurrentOffset(Tempvar.EndLocation);
                                script.RemoveText(start, end - start);
                            }
                            if (SqlDtAdptStmt.Success)
                            {
                                string MthdretType = expr.GetParent<MethodDeclaration>().ReturnType.GetText();
                                if (MthdretType == "DataTable" || MthdretType == "System.Data.DataTable")
                                {
                                    string varName = expr.GetText().Split("()".ToCharArray())[1];
                                    var expr10 = Pat.GetDtTbl(varName);
                                    script.Replace(expr, expr10);
                                }
                                else if (MthdretType == "DataSet" || MthdretType == "System.Data.DataSet")
                                {
                                    string varName = expr.GetText().Split("()".ToCharArray())[1];
                                    var expr11 = Pat.GetDtSet(varName);
                                    script.Replace(expr, expr11);
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
                            if (varDeclMthd.Success)
                            {
                                if (expr.Parent.GetType().Name == "BlockStatement" &&
                                    expr.Parent.Parent.GetType().Name == "MethodDeclaration")
                                {
                                    var varName = expr.FirstChild.NextSibling.FirstChild.GetText();
                                    script.Replace(expr,Pat.varDeclMthd1(varName));
                                }
                            }
                        }

                        foreach (var expr in file.IndexOfExprStmt)
                        {
                            var copy = (ExpressionStatement)expr.Clone();
                            int offset = script.GetCurrentOffset(expr.StartLocation);

                            AllPatterns Pat = new AllPatterns();
                            var expr2 = Pat.FillExpr();
                            var expr3 = Pat.StoredProc();
                            var expr4 = Pat.sqlConnstmt();
                            var expr5 = Pat.ExNonQuery();
                            var expr6 = Pat.ConnOpenExprStmt();
                            var expr7 = Pat.ConnCloseExprStmt();
                            var expr8 = Pat.CmdDisposeExprStmt();
                            var expr9 = Pat.SqlDataAdapterExprStmt();

                            var expr12 = Pat.ExNonQuery1();
                            var expr13 = Pat.ConvertToInt32();

                            ICSharpCode.NRefactory.PatternMatching.Match FillExpr = expr2.Match(expr);
                            ICSharpCode.NRefactory.PatternMatching.Match StoredProc = expr3.Match(expr);
                            ICSharpCode.NRefactory.PatternMatching.Match sqlConnstmt = expr4.Match(expr);
                            ICSharpCode.NRefactory.PatternMatching.Match ExNonQuery = expr5.Match(expr);
                            ICSharpCode.NRefactory.PatternMatching.Match ConnOpenExprStmt = expr6.Match(expr);
                            ICSharpCode.NRefactory.PatternMatching.Match ConnCloseExprStmt = expr7.Match(expr);
                            ICSharpCode.NRefactory.PatternMatching.Match CmdDisposeExprStmt = expr8.Match(expr);
                            ICSharpCode.NRefactory.PatternMatching.Match SqlDataAdapterExprStmt = expr9.Match(expr);
                            ICSharpCode.NRefactory.PatternMatching.Match ExNonQuery1 = expr12.Match(expr);
                            ICSharpCode.NRefactory.PatternMatching.Match ConvertToInt32 = expr13.Match(expr);

                            if (StoredProc.Success || sqlConnstmt.Success || 
                                ConnOpenExprStmt.Success || ConnCloseExprStmt.Success || 
                                CmdDisposeExprStmt.Success || FillExpr.Success || ConvertToInt32.Success)
                            {
                                script.Remove(expr, true);
                            }
                            if (ExNonQuery.Success)
                            {
                                string varName = expr.FirstChild.FirstChild.GetText();
                                string objName = expr.FirstChild.LastChild.FirstChild.FirstChild.GetText();
                                var expr1 = Pat.ExeStrdProc(varName, objName);
                                script.Replace(expr, expr1);
                            }

                            if (SqlDataAdapterExprStmt.Success)
                            {
                                string retType = expr.GetParent<MethodDeclaration>().ReturnType.GetText();
                                if (retType == "DataTable" || retType == "System.Data.DataTable")   //
                                {
                                    string varName = expr.GetText().Split("()".ToCharArray())[1];
                                    var expr10 = Pat.GetDtTbl(varName);
                                    script.Replace(expr, expr10);
                                }
                                else if (retType == "DataSet" || retType == "System.Data.DataSet")
                                {
                                    string varName = expr.GetText().Split("()".ToCharArray())[1];
                                    var expr11 = Pat.GetDtSet(varName);
                                    script.Replace(expr, expr11);
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
                            if (ExNonQuery1.Success)
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
                                
                            }
                        }

                        // for changing API.RegistrationAPI objAPI in method declaration to RegistrationAPI objAPI
                        foreach (var expr in file.IndexOfMthdDecl)
                        {

                            var copy = (MethodDeclaration)expr.Clone();
                            //AllPatterns Pat = new AllPatterns();

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
                    File.WriteAllText(Path.ChangeExtension(file.FileName, ".output.cs"), document.Text);
                }
            }
        }
    }
}
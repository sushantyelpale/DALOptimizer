using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
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
            OpenFileDialog openFileDialogDisc = new OpenFileDialog();
            openFileDialogDisc.ShowDialog();
            string fileName = openFileDialogDisc.FileName;
            
            if (string.IsNullOrWhiteSpace(fileName)){
                MessageBox.Show("Please Select one file ");
                return;
            }

            Solution solution = new Solution(fileName);
            solution.ChooseCSProjFile(fileName);

            MatchInvocation matchInvocation = new MatchInvocation();
            MatchExpr matchExpr = new MatchExpr();

            // Capture patterns & store it in respective lists of Node patterns
            foreach (var file in solution.AllFiles)
            {
                if (Path.GetFileName(file.fileName) == "DatabaseProcessing.cs")
                       continue;

//                if (Path.GetFileName(file.fileName) != "TicketDAL.cs")
//                    continue;
                var astResolver = new CSharpAstResolver(file.project.Compilation, file.syntaxTree, file.unresolvedTypeSystemForFile);
                matchInvocation.CheckAllInvocation(file, astResolver);
            }

            PrintFunction pr = new PrintFunction();
            pr.PrintMethod(solution);

            Console.Write("Apply refactorings?  Enter \"y\" for  yes    &   Enter any key for \"no\":");
            string answer = Console.ReadLine();
            if ("y".Equals(answer, StringComparison.OrdinalIgnoreCase))
            { 
                foreach (var file in solution.AllFiles)
                {
                    if (file.IndexOfFieldDecl.Count == 0 && file.IndexOfPropDecl.Count == 0 &&
                        file.IndexOfAssExpr.Count == 0 && file.IndexOfBlockStmt.Count == 0 &&
                        file.IndexOfCtchClause.Count == 0 && file.IndexOfExprStmt.Count == 0 &&
                        file.IndexOfMthdDecl.Count == 0 && file.IndexOfPropDecl.Count == 0 &&
                        file.IndexOfVarDeclStmt.Count == 0)
                        continue;

                    var document = matchExpr.CheckAllExpressions(file);
                    File.WriteAllText(Path.ChangeExtension(file.fileName, ".output.cs"), document.Text);
                    //File.WriteAllText(Path.ChangeExtension(file.fileName, ".cs"), document.Text);
                }
            }
            Console.ReadKey();
        }
    }
}
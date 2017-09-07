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
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;


namespace DALOptimizer
{
    class AllPatterns
    {
        //conn.close() Pattern
        public ExpressionStatement ConnCloseExprStmt()
        {
            var expr = new ExpressionStatement
            {
                Expression = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        Target = new IdentifierExpression("conn"),
                        MemberName = "Close"
                    }
                }
            };
            return expr;
        }

        //cmd.Dispose() Pattern
        public ExpressionStatement CmdDisposeExprStmt()
        {
            var expr = new ExpressionStatement
            {
                Expression = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        Target = new IdentifierExpression("conn"),
                        MemberName = "Dispose"
                    }
                }
            };
            return expr;
        }

        //conn.open()
        public ExpressionStatement ConnOpenExprStmt()
        {
            var expr = new ExpressionStatement
            {
                Expression = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        Target = new IdentifierExpression("conn"),
                        MemberName = "Open"
                    }
                }
            };
            return expr;
        }

        //da.Fill(dt);
        public ExpressionStatement FillExpr()
        {
            var Expr = new ExpressionStatement
            {
                Expression = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        Target = new IdentifierExpression(Pattern.AnyString),
                        MemberName = "Fill"
                    },
                    Arguments = {
                        new IdentifierExpression(Pattern.AnyString)
                    }
                }
            };
            return Expr;
        }

        //ConnectionClass.connect()
        public InvocationExpression ConnectionClassconnectExpr()
        {
            var expr = new InvocationExpression
            {
                Target = new MemberReferenceExpression
                {
                    Target = new IdentifierExpression("ConnectionClass"),
                    MemberName = "connect"
                }
            };
            return expr;
        }

        // Used for Assignment expressions
        //Replaced by cmd = new SqlCommand("InsertTicket", con);
        public AssignmentExpression sqlCmdstmt()
        {
            var expr = new AssignmentExpression
            {
                Left = new IdentifierExpression(Pattern.AnyString),
                Right = new ObjectCreateExpression
                {
                    Type = new SimpleType("SqlCommand"),
                    Arguments = { 
                            new AnyNode("PrimitiveExpression"), 
                            new IdentifierExpression(Pattern.AnyString) 
                        }
                }
            };
            return expr;
        }

        //Replaced by cmd = new SqlCommand("InsertTicket", con);  /argument of any type
        public AssignmentExpression sqlCmdstmt1()
        {
            var expr = new AssignmentExpression
            {
                Left = new IdentifierExpression(Pattern.AnyString),
                Right = new ObjectCreateExpression
                {
                    Type = new SimpleType("SqlCommand"),
                    Arguments = { new Repeat(new AnyNode()) }
                }
            };
            return expr;
        }

        // Pattern of variable Decl Stmt having sqlCommand having 2 arguments
        //Replaced by SqlCommand cmd = new SqlCommand("InsertTicket", con);  /argument of any type
        public VariableDeclarationStatement sqlCmdstmt2()
        {
            var expr = new VariableDeclarationStatement
            {
                Type = new SimpleType("SqlCommand"),
                Variables = { 
                    new VariableInitializer(
                        Pattern.AnyString, 
                        new ObjectCreateExpression {
                            Type = new SimpleType("SqlCommand"),
                            Arguments = { 
                                new AnyNode("PrimitiveExpression"), 
                                new IdentifierExpression(Pattern.AnyString) }
                        })
                }
            };
            return expr;
        }

        //conn = new SqlConnection(constr);
        public ExpressionStatement sqlConnstmt()
        {
            var expr = new ExpressionStatement
            {
                Expression = new AssignmentExpression
                {
                    Left = new IdentifierExpression(Pattern.AnyString),
                    Right = new ObjectCreateExpression
                    {
                        Type = new SimpleType("SqlConnection"),
                        Arguments = { new IdentifierExpression(Pattern.AnyString) }
                    }
                }
            };
            return expr;
        }

        //stored procedure statement inside try block
        //cmd.CommandType=CommandType.StoredProcedure;
        public ExpressionStatement StoredProc()
        {
            var expr = new ExpressionStatement
            {
                Expression = new AssignmentExpression
                {
                    Left = new MemberReferenceExpression
                    {
                        Target = new IdentifierExpression(Pattern.AnyString),
                        MemberName = "CommandType"
                    },
                    Right = new MemberReferenceExpression
                    {
                        Target = new IdentifierExpression("CommandType"),
                        MemberName = "StoredProcedure"
                    }
                }
            };
            return expr;
        }

        public ParameterDeclaration methd()
        {
            var expr = new ParameterDeclaration
            {
                Type = new AnyNode()
            };
            return expr;
        }

        //i = db.ExecuteStoredProcedure(cmd);
        public ExpressionStatement ExeStrdProc(string variable, string objName)
        {
            var expr = new ExpressionStatement
            {
                Expression = new AssignmentExpression
                {
                    Left = new IdentifierExpression(variable),
                    Right = new InvocationExpression
                    {
                        Target = new MemberReferenceExpression
                        {
                            Target = new IdentifierExpression("db"),
                            MemberName = "ExecuteStoredProcedure"
                        },
                        Arguments = { new IdentifierExpression("cmd") }
                    }
                }
            };
            return expr;
        }

        //i = cmd.ExecuteNonQuery();
        public ExpressionStatement ExNonQuery()
        {
            var expr = new ExpressionStatement
            {
                Expression = new AssignmentExpression
                {
                    Left = new IdentifierExpression(Pattern.AnyString),
                    Right = new InvocationExpression
                    {
                        Target = new MemberReferenceExpression
                        {
                            Target = new IdentifierExpression(Pattern.AnyString),
                            MemberName = "ExecuteNonQuery"
                        }
                    }
                }
            };

            return expr;
        }

        //cmd.ExecuteNonQuery();
        public ExpressionStatement ExNonQuery1()
        {
            var expr = new ExpressionStatement
            {
                Expression = new InvocationExpression
                {
                    Target = new MemberReferenceExpression { 
                        Target = new IdentifierExpression(Pattern.AnyString),
                        MemberName = "ExecuteNonQuery"
                    }
                }
            };
            return expr;
        }

        public ExpressionStatement ConvertToInt32()
        {
            var expr = new ExpressionStatement
            {
                Expression = new AssignmentExpression
                {
                    Left = new IdentifierExpression(Pattern.AnyString),
                    Right = new InvocationExpression
                    {
                        Target = new MemberReferenceExpression
                        {
                            Target = new IdentifierExpression("Convert"),
                            MemberName = "ToInt32",
                        },
                        Arguments = { 
                            new MemberReferenceExpression{
                                Target = new IdentifierExpression(Pattern.AnyString),
                                MemberName = "Value"
                            } 
                        }
                    }
                }
            };
            return expr;
        }

        public ExpressionStatement gtOtptParameter(string sqlCmd, string output)
        {
            var expr = new ExpressionStatement
            {
                Expression = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        Target = new IdentifierExpression("db"),
                        MemberName = "GetOutputParameter"
                    },
                    Arguments = { 
                        new IdentifierExpression(sqlCmd),
                        new IdentifierExpression(output)
                    }
                }
            };
            return expr;
        }

        //db.GetDataTable(cmd);
        public ExpressionStatement GetDtTbl(string str)
        {
            var expr = new ExpressionStatement
            {
                Expression = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        MemberName = "GetDataTable",
                        Target = new IdentifierExpression("db")
                    },
                    Arguments = { new IdentifierExpression(str) },
                }
            };
            return expr;
        }

        //db.GetDataSet(cmd);
        public ExpressionStatement GetDtSet(string str)
        {
            var expr = new ExpressionStatement
            {
                Expression = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        MemberName = "GetDataSet",
                        Target = new IdentifierExpression("db")
                    },
                    Arguments = { new IdentifierExpression(str) },
                }
            };
            return expr;
        }

        //sda = new SqlDataAdapter(cmd);
        public ExpressionStatement SqlDataAdapterExprStmt()
        {
            var expr = new ExpressionStatement
            {
                Expression = new AssignmentExpression
                {
                    Left = new IdentifierExpression(Pattern.AnyString),
                    Right = new ObjectCreateExpression
                    {
                        Type = new SimpleType("SqlDataAdapter"),
                        Arguments = { new IdentifierExpression(Pattern.AnyString) }
                    }
                }
            };
            return expr;
        }

        //log.Error("TicketDAL:displayTicket : " + ex.Message);
        public ExpressionStatement logErr()
        {
            var expr = new ExpressionStatement
            {
                Expression = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        Target = new IdentifierExpression("log"),
                        MemberName = "Error"
                    },
                    Arguments = { 
                        new BinaryOperatorExpression{
                            Operator = BinaryOperatorType.Add,
                            Left = new AnyNode("PrimitiveExpression"),
                            Right = new MemberReferenceExpression{
                                    Target = new IdentifierExpression(Pattern.AnyString),
                                    MemberName = Pattern.AnyString
                            }
                        }
                    }
                }
            };
            return expr;
        }

        //catch clause to Replace
        public CatchClause ctchclause()
        {
            var expr = new CatchClause
            {
                Type = new SimpleType("Exception"),
                VariableName = "ex",
                Body = new BlockStatement { 
                    new ExpressionStatement{
                        Expression = new InvocationExpression{
                            Target = new MemberReferenceExpression{
                                Target = new ObjectCreateExpression { 
                                    Type = new SimpleType("LoggerProcessing")
                                },
                                MemberName = "write"
                            },
                            Arguments = { new IdentifierExpression("ex") }
                        }
                    }
                }
            };
            return expr;
        }

        //finally Block
        public BlockStatement FinalyBlck()
        {
            var Expr = new BlockStatement
            {
            };
            return Expr;
        }

        //DatabaseProcessing db = new DatabaseProcessing();
        public FieldDeclaration DbProcessing()
        {
            var Expr = new FieldDeclaration
            {
                ReturnType = new SimpleType("DatabaseProcessing"),
                Variables = {
                    new VariableInitializer("db", new ObjectCreateExpression(type : new SimpleType("DatabaseProcessing")) )
                }
            };
            return Expr;
        }

        public VariableDeclarationStatement varDeclMthd()
        {
            var expr = new VariableDeclarationStatement
            {
                Type = new PrimitiveType("int"),
                Variables = {
                    new VariableInitializer(Pattern.AnyString, new PrimitiveExpression(0))
                }
            };
            return expr;
        }
        public VariableDeclarationStatement varDeclMthd1(string str)
        {
            var expr = new VariableDeclarationStatement
            {
                Type = new PrimitiveType("int"),
                Variables = {
                    new VariableInitializer(str, new PrimitiveExpression(-1))
                }
            };
            return expr;
        }


        public VariableDeclarationStatement SqlDtAdptStmt()
        {
            var expr = new VariableDeclarationStatement
            {
                Type = new SimpleType("SqlDataAdapter"),
                Variables = { new VariableInitializer(
                    Pattern.AnyString, 
                    new ObjectCreateExpression{
                        Type = new SimpleType("SqlDataAdapter"),
                        Arguments = {new IdentifierExpression(Pattern.AnyString)}
                    }
                    )
                }
            };
            return expr;
        }

        public VariableDeclarationStatement sqlParameter()
        {
            var expr = new VariableDeclarationStatement
            {
                Type = new SimpleType("SqlParameter"),
                Variables = { 
                    new VariableInitializer( Pattern.AnyString, 
                        new ObjectCreateExpression{
                            Type = new SimpleType("SqlParameter"),
                            Arguments = {new Repeat (new AnyNode())}
                        })
                }
            };
            return expr;
        }

        public VariableDeclarationStatement newPattern()
        {
            var pattern = new VariableDeclarationStatement
            {
                Type = new AnyNode("type"),
                Variables = { 
                    new VariableInitializer {
                            Name = Pattern.AnyString,
                            Initializer = new ObjectCreateExpression {
                                Type = new Backreference("type"),
                                Arguments = { new Repeat(new AnyNode()) }
                            }
                        }
                    }
            };
            return pattern;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.Semantics;


namespace DALOptimizer
{
    class AllPatterns
    {
        //conn.close() Pattern
        public ExpressionStatement ConnCloseExprStmt()
        {
            var connCloseExprStmt = new ExpressionStatement
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
            return connCloseExprStmt;
        }

        //cmd.Dispose() Pattern
        public ExpressionStatement CmdDisposeExprStmt()
        {
            var cmdDisposeExprStmt = new ExpressionStatement
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
            return cmdDisposeExprStmt;
        }

        //conn.open()
        public ExpressionStatement ConnOpenExprStmt()
        {
            var connOpenExprStmt = new ExpressionStatement
            {
                Expression = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        Target = new IdentifierExpression(Pattern.AnyString),
                        MemberName = "Open"
                    }
                }
            };
            return connOpenExprStmt;
        }

        //da.Fill(dt);
        public ExpressionStatement FillExpr()
        {
            var fillExpr = new ExpressionStatement
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
            return fillExpr;
        }

        //ConnectionClass.connect()
        public InvocationExpression ConnectionClassconnectExpr()
        {
            var connectionClassconnectExpr = new InvocationExpression
            {
                Target = new MemberReferenceExpression
                {
                    Target = new IdentifierExpression("ConnectionClass"),
                    MemberName = "connect"
                }
            };
            return connectionClassconnectExpr;
        }

        // Used for Assignment expressions
        //Replaced by cmd = new SqlCommand("InsertTicket", con);
        public AssignmentExpression sqlCmdstmt()
        {
            var sqlCmdstmt = new AssignmentExpression
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
            return sqlCmdstmt;
        }

        //Replaced by cmd = new SqlCommand("InsertTicket", con);  /argument of any type
        public AssignmentExpression sqlCmdstmt1()
        {
            var sqlCmdstmt1 = new AssignmentExpression
            {
                Left = new IdentifierExpression(Pattern.AnyString),
                Right = new ObjectCreateExpression
                {
                    Type = new SimpleType("SqlCommand"),
                    Arguments = { new Repeat(new AnyNode()) }
                }
            };
            return sqlCmdstmt1;
        }

        // Pattern of variable Decl Stmt having sqlCommand having 2 arguments
        //Replaced by SqlCommand cmd = new SqlCommand("InsertTicket", con);  /argument of any type
        public VariableDeclarationStatement sqlCmdstmt2()
        {
            var sqlCmdstmt2 = new VariableDeclarationStatement
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
            return sqlCmdstmt2;
        }

        //conn = new SqlConnection(constr);
        public ExpressionStatement sqlConnstmt()
        {
            var sqlConnstmt = new ExpressionStatement
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
            return sqlConnstmt;
        }

        //stored procedure statement inside try block
        //cmd.CommandType=CommandType.StoredProcedure;
        public ExpressionStatement StoredProc()
        {
            var storedProc = new ExpressionStatement
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
            return storedProc;
        }

        //i = db.ExecuteStoredProcedure(cmd);
        public ExpressionStatement ExeStrdProc(string variable, string objName)
        {
            var exeStrdProc = new ExpressionStatement
            {
                Expression = new AssignmentExpression
                {
                    Left = new IdentifierExpression(variable),
                    Right = new InvocationExpression
                    {
                        Target = new MemberReferenceExpression
                        {
                            Target = new IdentifierExpression("db"),
                            MemberName = "executeStoredProcedure"
                        },
                        Arguments = { new IdentifierExpression("cmd") }
                    }
                }
            };
            return exeStrdProc;
        }

        //i = cmd.ExecuteNonQuery();
        public ExpressionStatement ExNonQuery()
        {
            var exNonQuery = new ExpressionStatement
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

            return exNonQuery;
        }

        //cmd.ExecuteNonQuery();
        public ExpressionStatement ExNonQuery1()
        {
            var exNonQuery1 = new ExpressionStatement
            {
                Expression = new InvocationExpression
                {
                    Target = new MemberReferenceExpression { 
                        Target = new IdentifierExpression(Pattern.AnyString),
                        MemberName = "ExecuteNonQuery"
                    }
                }
            };
            return exNonQuery1;
        }

        public ExpressionStatement ConvertToInt32()
        {
            var convertToInt32 = new ExpressionStatement
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
            return convertToInt32;
        }

        public ExpressionStatement gtOtptParameter(string sqlCmd, string output)
        {
            var gtOtptParameter = new ExpressionStatement
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
            return gtOtptParameter;
        }

        //db.GetDataTable(cmd);
        public ExpressionStatement GetDtTbl(string cmd, string dt)
        {
            var getDtTbl = new ExpressionStatement
            {
                Expression = new AssignmentExpression{
                    Left = new IdentifierExpression(dt),
                    Right = new InvocationExpression
                    {
                        Target = new MemberReferenceExpression
                        {
                            MemberName = "getDataTable",
                            Target = new IdentifierExpression("db")
                        },
                        Arguments = { new IdentifierExpression(cmd) },
                    }
                }
            };
            return getDtTbl;
        }

        //db.GetDataSet(cmd);
        public ExpressionStatement GetDtSet(string cmd, string dt)
        {
            var getDtSet = new ExpressionStatement{
                Expression = new AssignmentExpression{
                    Left = new IdentifierExpression(dt),
                    Right = new InvocationExpression{
                        Target = new MemberReferenceExpression{
                            MemberName = "getDataSet",
                            Target = new IdentifierExpression("db")
                        },
                        Arguments = { new IdentifierExpression(cmd) },
                    }
                }
            };
            return getDtSet;
        }

        //sda = new SqlDataAdapter(cmd);
        public ExpressionStatement SqlDataAdapterExprStmt()
        {
            var sqlDataAdapterExprStmt = new ExpressionStatement
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
            return sqlDataAdapterExprStmt;
        }

        //log.Error("TicketDAL:displayTicket : " + ex.Message);
        public ExpressionStatement logErr()
        {
            var logErr = new ExpressionStatement
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
            return logErr;
        }

        //cmd.Connection = con;
        public ExpressionStatement ConnectionStmt()
        {
            var connectionStmt = new ExpressionStatement
            {
                Expression = new AssignmentExpression
                {
                    Left = new MemberReferenceExpression
                    {
                        Target = new IdentifierExpression(Pattern.AnyString),
                        MemberName = "Connection"
                    },
                    Right = new IdentifierExpression(Pattern.AnyString)
                }
            };
            return connectionStmt;
        }

        //catch clause to Replace
        public CatchClause ctchclause()
        {
            var ctchclause = new CatchClause
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
            return ctchclause;
        }

        //finally Block
        public BlockStatement FinalyBlck()
        {
            var finalyBlck = new BlockStatement
            {
            };
            return finalyBlck;
        }

        //DatabaseProcessing db = new DatabaseProcessing();
        public FieldDeclaration DbProcessing()
        {
            var dbProcessing = new FieldDeclaration
            {
                ReturnType = new SimpleType("DatabaseProcessing"),
                Variables = {
                    new VariableInitializer("db", 
                    new ObjectCreateExpression(type : new SimpleType("DatabaseProcessing")) )
                }
            };
            return dbProcessing;
        }

        public VariableDeclarationStatement varDeclMthd()
        {
            var varDeclMthd = new VariableDeclarationStatement
            {
                Type = new PrimitiveType("int"),
                Variables = {
                    new VariableInitializer(Pattern.AnyString, new PrimitiveExpression(0))
                }
            };
            return varDeclMthd;
        }

        public VariableDeclarationStatement varDeclMthd1(string str)
        {
            var varDeclMthd1 = new VariableDeclarationStatement
            {
                Type = new PrimitiveType("int"),
                Variables = {
                    new VariableInitializer(str, new PrimitiveExpression(-1))
                }
            };
            return varDeclMthd1;
        }

        public VariableDeclarationStatement SqlDtAdptStmt()
        {
            var sqlDtAdptStmt = new VariableDeclarationStatement
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
            return sqlDtAdptStmt;
        }

        public VariableDeclarationStatement sqlParameter()
        {
            var sqlParameter = new VariableDeclarationStatement
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
            return sqlParameter;
        }
    }
}
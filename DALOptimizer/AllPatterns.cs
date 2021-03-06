﻿using System;
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

        //cmd = new SqlCommand("InsertTicket", con);
        public AssignmentExpression SqlCmdStmt()
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

        //cmd = new SqlCommand();
        //cmd = new SqlCommand("InsertTicket");
        //cmd = new SqlCommand("InsertTicket", con);
        public AssignmentExpression SqlCmdStmtAnyNode()
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

        public VariableDeclarationStatement SqlCmdStmtVarDecl()
        {
            var sqlCmdStmtVarDecl = new VariableDeclarationStatement
            {
                Type = new SimpleType("SqlCommand"),
                Variables = { 
                    new VariableInitializer(
                        Pattern.AnyString, 
                        new ObjectCreateExpression{
                            Type = new SimpleType("SqlCommand")
                        }) 
                     }
            };
            return sqlCmdStmtVarDecl;
        }

        // Pattern of variable Decl Stmt having sqlCommand having 2 arguments
        //Replaced by SqlCommand cmd = new SqlCommand("InsertTicket", con);  /argument of any type
        public VariableDeclarationStatement SqlCmdstmtTwoArgs()
        {
            var sqlCmdstmtTwoArgs = new VariableDeclarationStatement
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
            return sqlCmdstmtTwoArgs;
        }

        //conn = new SqlConnection(constr);
        public ExpressionStatement SqlConnStmt()
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
        public ExpressionStatement ExNonQueryVarAssignment()
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
        public ExpressionStatement ExNonQueryDecl()
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
        public CatchClause catchclause(string loggerClassName)
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
                                    Type = new SimpleType(loggerClassName)
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
        public BlockStatement FinalyBlock()
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

        public VariableDeclarationStatement varDeclMethodZero()
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

        public VariableDeclarationStatement varDeclMethodMinusOne(string str)
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

        public VariableDeclarationStatement DataTableSetStmt(string typeOfStmt, string dtsVar)
        {
            var stmt = new VariableDeclarationStatement
            {
                Type = new SimpleType(typeOfStmt),
                Variables = { 
                    new VariableInitializer(
                        dtsVar,
                        new ObjectCreateExpression(new SimpleType(typeOfStmt)))
                }
            };
            return stmt;
        }

        public ExpressionStatement DataTableSetExpr(string typeOfStmt,string dtsVar)
        {
            var expr = new ExpressionStatement
            {
                Expression = new AssignmentExpression
                {
                    Left = new IdentifierExpression(dtsVar),
                    Right = new ObjectCreateExpression {
                        Type = new SimpleType(typeOfStmt)
                    }
                }
            };
            return expr;
        }
    }
}
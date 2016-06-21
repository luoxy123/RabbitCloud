﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rabbit.Rpc.Ids.Implementation;
using Rabbit.Rpc.ProxyGenerator;
using Rabbit.Rpc.ProxyGenerator.Implementation;
using Rabbit.Rpc.ProxyGenerator.Utilitys;
using Rabbit.Rpc.Server.Implementation.ServiceDiscovery.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Rabbit.Rpc.ClientGenerator
{
    internal class Program
    {
        private static void Main()
        {
            var assemblyFiles =
                Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assemblies"), "*.dll").ToArray();
            var assemblies = assemblyFiles.Select(i => Assembly.Load(File.ReadAllBytes(i))).ToArray();

            IServiceProxyGenerater serviceProxyGenerater = new ServiceProxyGenerater(new DefaultServiceIdGenerator());

            Console.WriteLine("成功加载了以下程序集");
            foreach (var name in assemblies.Select(i => i.GetName().Name))
            {
                Console.WriteLine(name);
            }

            var services = assemblies
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Where(i => i.IsInterface && i.GetCustomAttribute<RpcServiceAttribute>() != null);

            while (true)
            {
                Console.WriteLine("1.生成客户端代理程序集");
                Console.WriteLine("2.生成客户端代理代码");

                var command = Console.ReadLine();

                Func<IEnumerable<SyntaxTree>> getTrees = () =>
                {
                    var trees = new List<SyntaxTree>();
                    foreach (var service in services)
                    {
                        trees.Add(serviceProxyGenerater.GenerateProxyTree(service));
                    }
                    return trees;
                };

                var outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "outputs");
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                switch (command)
                {
                    case "1":
                        var bytes = CompilationUtilitys.CompileClientProxy(getTrees(), Enumerable.Empty<MetadataReference>());
                        {
                            var fileName = Path.Combine(outputDirectory, "Rabbit.Rpc.Proxys.dll");
                            File.WriteAllBytes(fileName, bytes);
                            Console.WriteLine($"生成成功，路径：{fileName}");
                        }
                        break;

                    case "2":
                        foreach (var syntaxTree in getTrees())
                        {
                            var className = ((ClassDeclarationSyntax)((CompilationUnitSyntax)syntaxTree.GetRoot()).Members[0]).Identifier.Value;
                            var code = syntaxTree.ToString();
                            var fileName = Path.Combine(outputDirectory, $"{className}.cs");
                            File.WriteAllText(fileName, code, Encoding.UTF8);
                            Console.WriteLine($"生成成功，路径：{fileName}");
                        }
                        break;

                    default:
                        Console.WriteLine("无效的输入！");
                        continue;
                }
            }
        }

        private static SyntaxTree GetAssemblyInfo()
        {
            return CompilationUnit()
                .WithUsings(
                    List<UsingDirectiveSyntax>(
                        new UsingDirectiveSyntax[]
                        {
                            UsingDirective(
                                QualifiedName(
                                    IdentifierName("System"),
                                    IdentifierName("Reflection"))),
                            UsingDirective(
                                QualifiedName(
                                    QualifiedName(
                                        IdentifierName("System"),
                                        IdentifierName("Runtime")),
                                    IdentifierName("InteropServices")))
                        }))
                .WithAttributeLists(
                    List<AttributeListSyntax>(
                        new AttributeListSyntax[]
                        {
                            AttributeList(
                                SingletonSeparatedList<AttributeSyntax>(
                                    Attribute(
                                        IdentifierName("AssemblyTitle"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList<AttributeArgumentSyntax>(
                                                    AttributeArgument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal("Rabbit.Rpc.Proxys"))))))))
                                .WithTarget(
                                    AttributeTargetSpecifier(
                                        Token(SyntaxKind.AssemblyKeyword))),
                            AttributeList(
                                SingletonSeparatedList<AttributeSyntax>(
                                    Attribute(
                                        IdentifierName("AssemblyProduct"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList<AttributeArgumentSyntax>(
                                                    AttributeArgument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal("Rabbit.Rpc.Proxys"))))))))
                                .WithTarget(
                                    AttributeTargetSpecifier(
                                        Token(SyntaxKind.AssemblyKeyword))),
                            AttributeList(
                                SingletonSeparatedList<AttributeSyntax>(
                                    Attribute(
                                        IdentifierName("AssemblyCopyright"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList<AttributeArgumentSyntax>(
                                                    AttributeArgument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal("Copyright ©  2016"))))))))
                                .WithTarget(
                                    AttributeTargetSpecifier(
                                        Token(SyntaxKind.AssemblyKeyword))),
                            AttributeList(
                                SingletonSeparatedList<AttributeSyntax>(
                                    Attribute(
                                        IdentifierName("ComVisible"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList<AttributeArgumentSyntax>(
                                                    AttributeArgument(
                                                        LiteralExpression(
                                                            SyntaxKind.FalseLiteralExpression)))))))
                                .WithTarget(
                                    AttributeTargetSpecifier(
                                        Token(SyntaxKind.AssemblyKeyword))),
                            AttributeList(
                                SingletonSeparatedList<AttributeSyntax>(
                                    Attribute(
                                        IdentifierName("Guid"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList<AttributeArgumentSyntax>(
                                                    AttributeArgument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal("30e88903-d3ca-4f26-b586-159242840443"))))))))
                                .WithTarget(
                                    AttributeTargetSpecifier(
                                        Token(SyntaxKind.AssemblyKeyword))),
                            AttributeList(
                                SingletonSeparatedList<AttributeSyntax>(
                                    Attribute(
                                        IdentifierName("AssemblyVersion"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList<AttributeArgumentSyntax>(
                                                    AttributeArgument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal("1.0.0.0"))))))))
                                .WithTarget(
                                    AttributeTargetSpecifier(
                                        Token(SyntaxKind.AssemblyKeyword))),
                            AttributeList(
                                SingletonSeparatedList<AttributeSyntax>(
                                    Attribute(
                                        IdentifierName("AssemblyFileVersion"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList<AttributeArgumentSyntax>(
                                                    AttributeArgument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal("1.0.0.0"))))))))
                                .WithTarget(
                                    AttributeTargetSpecifier(
                                        Token(SyntaxKind.AssemblyKeyword)))
                        }))
                .NormalizeWhitespace()
                .SyntaxTree;
        }
    }
}
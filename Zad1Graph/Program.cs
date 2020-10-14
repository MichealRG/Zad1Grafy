﻿using System;
using System.Collections.Generic;
using System.IO;
using Graphviz4Net.Dot;
using Graphviz4Net.Dot.AntlrParser;
//using NUnit.Framework;
using System.Linq;
using Antlr.Runtime;
using System.Text.RegularExpressions;
using Graphviz4Net.Graphs;
using System.Threading;
using System.Diagnostics;

namespace Zad1Graph
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            if (args.Length == 1)
            {
          
                //args[0] = "GraphToTestV4.dt";
                using var sr = new StreamReader(args[0]);
                sw.Start();
                //Console.WriteLine(sr.ReadToEnd());
                var graph = Parse(sr.ReadToEnd());
                //Console.WriteLine(graph+"\n\n");
                List<int> vertices = new List<int>();
                //Dictionary<List<int>,List<int>> edgesConnection= new Dictionary<List<int>, List<int>>();
                Dictionary<int, int> edgesSource = new Dictionary<int, int>();
                Dictionary<int, int> edgesDestination = new Dictionary<int, int>();
                Dictionary<int, List<int>> edgesConnection = new Dictionary<int, List<int>>();
                List<int> vGroup = new List<int>();
                List<int> uGroup = new List<int>();
                Tuple<int, int> transformed;
                bool isInUGroup=false;
                ///System zbierania danych o grafie
                foreach (var item in graph.AllVertices.ToList())
                {
                    vertices.Add(item.Id);
                    edgesDestination.Add(item.Id, 0);
                    edgesSource.Add(item.Id, 0);
                    edgesConnection.Add(item.Id, new List<int>());
                }

                foreach (var item in graph.Edges.ToList())
                {
                    transformed = transformEdgeIdToInt(item, "Destination");
                    edgesSource[transformed.Item1]++;
                    edgesDestination[transformed.Item2]++;
                    edgesConnection[transformed.Item1].Add(transformed.Item2);
                }

                ///System sprawdzania krawedzi

                //foreach (var item in graph.GetAllVertices().ToList())
                //sprawdzenie braku wyjść z wierzchołka -> dodaj do U
                foreach (var item in graph.AllVertices.ToList())
                {
                    if (edgesSource[item.Id] == 0)
                    {
                        uGroup.Add(item.Id);

                    }
                    else
                    {
                        //sprawdzenie braku wejść do wierzchołka dodja do V
                        if (edgesDestination[item.Id] == 0)
                        {
                            vGroup.Add(item.Id);
                        }
                    }

                }
                foreach (var item in graph.AllVertices.ToList())
                {
                    foreach (var connected in edgesConnection[item.Id])
                    {
                        //jezeli cos ma połączenie z wierzcholkiem z U  dodja go do V
                        if (uGroup.Contains(connected))
                        {
                            if (!vGroup.Contains(item.Id))
                                vGroup.Add(item.Id);
                        }
                    }
                }

                foreach (var item in graph.AllVertices.ToList())
                {
                    foreach (var connected in edgesConnection[item.Id])//elementy z którymi jest połączony gość z v groupy
                    {
                        //dodja wierzchołki z którymi łącza się goście z V grupy do U grupy
                       if (vGroup.Contains(item.Id))
                        {
                            if (!uGroup.Contains(connected)&& !vGroup.Contains(connected))
                            {
                                uGroup.Add(connected);
                            }
                        }
                    }
                }

                //sprawdz pozostałe wierzchołki
                foreach (var item in graph.AllVertices.ToList())
                {
                    if (!uGroup.Contains(item.Id) && !vGroup.Contains(item.Id))
                    {
                        foreach (var connected in edgesConnection[item.Id])
                        {
                            if (uGroup.Contains(connected))
                            {
                                isInUGroup = true;
                                if (!vGroup.Contains(item.Id))
                                {
                                    vGroup.Add(item.Id);
                                }
                                break;
                            }
                            else
                                isInUGroup = false;
                        }
                        if (isInUGroup==false)
                        {
                            uGroup.Add(item.Id);
                        }
                    }
                }


                ///Wypisywanie danych
                sw.Stop();
                vGroup.Sort();
                uGroup.Sort();


                Console.WriteLine("\nV Group:");
                foreach (var item in vGroup)
                {
                    Console.Write("{0} ", item);
                }
                Console.WriteLine("\nU Group:");
                foreach (var item in uGroup)
                {
                    Console.Write("{0} ", item);
                
                }

                Console.WriteLine("\nEllapsed time is {0}",sw.Elapsed);
                //var graph = new DotGraph<int>();
                //graph.Attributes.Add("bb", "1.2,3.4,5,6.7");
                //DotVertex<int> dotVertex = new DotVertex<int>(0);
                //graph.AddVertex(dotVertex);
                //DotVertex<int> dotVertex2 = new DotVertex<int>(1);
                //graph.AddVertex(dotVertex2);
                //DotEdge<int> dotEdge = new DotEdge<int>(dotVertex, dotVertex2);
                //graph.AddEdge(dotEdge);
                //Console.WriteLine(graph);
            }
            else
            {
                Console.WriteLine("Podaj arguemnt wejściowy przy uruchamianiu programu.");
            }
        }
        private static Tuple<int, int> transformEdgeIdToInt(IEdge edge, string whichEndOfEdge)
        {
            int charLoc;
            int idS, idD;
            //if (whichEndOfEdge == "Destination")
            //{
            //    Int32.TryParse(edge.Destination.ToString().Trim(new char[] { '{', '}' }), out id);

            //}
            //else
            //    Int32.TryParse(edge.Source.ToString().Trim(new char[] { '{', '}' }), out id);
            charLoc = edge.ToString().IndexOf('-');
            //Console.WriteLine(edge.ToString().Length);
            Int32.TryParse(edge.Source.ToString().Trim(new char[] { '{', '}' }), out idS);
            Int32.TryParse(edge.Destination.ToString().Trim(new char[] { '{', '}' }), out idD);
            return new Tuple<int, int>(idS, idD);
        }

        private static DotGraph<int> Parse(string content)
        {
            var antlrStream = new ANTLRStringStream(content);
            var lexer = new DotGrammarLexer(antlrStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new DotGrammarParser(tokenStream);
            var builder = new IntDotGraphBuilder();
            parser.Builder = builder;
            parser.dot();
            return builder.DotGraph;
        }
    }
}
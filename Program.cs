using System;
using System.IO;

namespace GraphvizTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                Test(args[0]);
            }
            else
            {
                throw new ArgumentException("Expecting command line argument!");
            }

        }

        public static void Test(string applicationBase)
        {
            var graphvizHelper = new GraphVizHelper { ApplicationBaseDir = applicationBase };
            graphvizHelper.LayoutAndRender(null, "digraph { 1 [color=blue] ;1 -> 2 [style=dashed]; 2 -> 3; 3 -> 1 }", "testFromString.png", "dot", "png");
            graphvizHelper.LayoutAndRender(null, "graph { 1 [color=blue]; 1 -- 2 [style=dashed]; 2 -- 3; 3 -- 1 }", "testFromString3.png", "dot", "png");
            graphvizHelper.LayoutAndRender("test.gv", null, "testFromFile3.png", "dot", "png");
        }
    }
}




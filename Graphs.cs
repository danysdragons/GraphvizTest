using System;
using System.IO;
using System.Diagnostics;
using System.Text;


// public static class Tester 
// {
//     public void Test()
// {
// 	var graphSpec = "digraph Test { bgcolor = pink; 1 -> 2; 2 -> 3 [color = purple, style = dotted]; 3 -> 1; }";
// 	var graphResult = new GraphVizHelper().LayoutAndRender(null, graphSpec, null, "neato", "png");
// 	var image = Util.Image(graphResult);
// 	image.Dump();
// }
// }

public class GraphVizHelper
{
    public bool treatWarningsAsErrors { get; set; } = false;
    public int TimeoutInMs { get; set; } = 30_000;

    public string ApplicationBaseDir { get; set; }

    /// <summary>
    ///     Layouts and renders a graph.
    /// </summary>
    /// <param name="graphFilePath">Path to graph file, can be null but then the graph must not be null.</param>
    /// <param name="graph">The graph, can be null but then the graph file path must not be null.</param>
    /// <param name="outputFilePath">Path to output file. If it's null, the return will have the command line output.</param>
    /// <param name="layoutAlgorithm">
    ///     The layout algorithm, if it's null it will default to "dot". See
    ///     https://www.graphviz.org/pdf/dot.1.pdf for more layout algorithms.
    /// </param>
    /// <param name="outputFormat">
    ///     The output format, if it's null it will default to "dot". See
    ///     https://www.graphviz.org/doc/info/output.html for more output formats.
    /// </param>
    /// <param name="extraCommandLineFlags">Any extra command line flags. See https://www.graphviz.org/doc/info/command.html </param>
    /// <returns>The output of the command line.</returns>
    public byte[] LayoutAndRender(string graphFilePath, string graph, string outputFilePath, string layoutAlgorithm, string outputFormat,
        params string[] extraCommandLineFlags)
    {
        if (graphFilePath == null && graph == null)
        {
            throw new ArgumentException($"Arguments {nameof(graphFilePath)} and {nameof(graph)} cannot be null at the same time.");
        }

        string arguments = BuildCommandLineArguments(graphFilePath, outputFilePath, layoutAlgorithm, outputFormat, extraCommandLineFlags);


        Process graphVizProcess = null;
        graphVizProcess = new Process
        {
            /// @"C:\Users\micha\source\repos\GraphVizNet\GraphVizNet\graphviz\dot"
            // @"C:\Users\micha\source\repos\GraphVizNet\GraphVizNet\graphviz"
            StartInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                //FileName = @"/usr/bin/dot",
                FileName = "dot",
                Arguments = arguments,
                WorkingDirectory = @$"{ApplicationBaseDir}/GraphvizOutput",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false
            }
        };


        graphVizProcess.Start();

        if (graph != null)
        {
            graphVizProcess.StandardInput.Write(graph);
            graphVizProcess.StandardInput.Close();
        }

        byte[] result;
        using (Stream baseStream = graphVizProcess.StandardOutput.BaseStream)
        using (var memoryStream = new MemoryStream())
        {
            baseStream.CopyTo(memoryStream);
            result = memoryStream.ToArray();
        }

        string errorAndWarningMessages = graphVizProcess.StandardError.ReadToEnd();

        graphVizProcess.WaitForExit(TimeoutInMs);

        if (graphVizProcess.ExitCode != 0)
        {
            throw new ApplicationException($"dot process exited with code: {graphVizProcess.ExitCode} and with errors: {errorAndWarningMessages}");
        }

        if (treatWarningsAsErrors && !string.IsNullOrEmpty(errorAndWarningMessages))
        {
            throw new ApplicationException($"dot process exited with code: {graphVizProcess.ExitCode} but with warnings: {errorAndWarningMessages}");
        }

        return result;
    }


    // You can define other methods, fields, classes and namespaces here

    /// <summary>
    ///     Utility function to build command line arguments for the dot executable.
    /// </summary>
    /// <param name="inputFilePath"></param>
    /// <param name="outputFilePath"></param>
    /// <param name="layout"></param>
    /// <param name="format"></param>
    /// <param name="extraCommandLineFlags"></param>
    /// <returns></returns>
    public static string BuildCommandLineArguments(string inputFilePath, string outputFilePath, string layout, string format,
        params string[] extraCommandLineFlags)
    {
        var argumentsBuilder = new StringBuilder();

        if (layout != null)
        {
            argumentsBuilder.Append(" -K").Append(layout);
        }

        if (format != null)
        {
            argumentsBuilder.Append(" -T").Append(format);
        }

        if (outputFilePath != null)
        {
            argumentsBuilder.Append(" -o\"").Append(outputFilePath).Append('\"');
        }

        foreach (string extraFlag in extraCommandLineFlags)
        {
            argumentsBuilder.Append(' ').Append(extraFlag);
        }

        if (inputFilePath != null)
        {
            argumentsBuilder.Append(" \"").Append(inputFilePath).Append('\"');
        }

        return argumentsBuilder.ToString();
    }


}


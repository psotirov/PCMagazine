using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class AlgorithmExecutor
{
    public string AlgorithmFilename { get; private set; }
    public string AlgorithmFullPath { get; private set; }
    public string AlgorithmPath { get; private set; }
    public string AlgorithmOwner { get; private set; }
    protected System.Diagnostics.Process algorithm;
    protected System.Diagnostics.ProcessStartInfo algorithmStartInfo;

    private Encoding specialEncoding;
    private bool IsReadLineOperationPending = false;

    public AlgorithmExecutor(string algorithmFolder, Encoding specialEncoding = null)
    {
        this.AlgorithmPath = algorithmFolder;

        //Console.WriteLine(this.AlgorithmPath);
        string algorithmExe = System.IO.Directory.GetFiles(this.AlgorithmPath).First(name => name.Contains(".exe") || name.Contains(".bat") || name.Contains(".class"));

        this.AlgorithmOwner = this.AlgorithmPath.Substring(this.AlgorithmPath.LastIndexOf('\\') + 1);

        this.AlgorithmFullPath = algorithmExe;
        this.AlgorithmFilename = algorithmExe.Substring(algorithmExe.LastIndexOf('\\') + 1, algorithmExe.LastIndexOf('.') - algorithmExe.LastIndexOf('\\'));

        this.algorithmStartInfo = new System.Diagnostics.ProcessStartInfo(algorithmExe);
        algorithmStartInfo.ErrorDialog = false;
        algorithmStartInfo.RedirectStandardInput = true;
        algorithmStartInfo.RedirectStandardOutput = true;
        algorithmStartInfo.RedirectStandardError = true;
        algorithmStartInfo.CreateNoWindow = true;
        algorithmStartInfo.UseShellExecute = false;
        algorithmStartInfo.WorkingDirectory = algorithmFolder;

        this.specialEncoding = specialEncoding;
    }

    public void StartAlgorithm()
    {
        this.algorithm = System.Diagnostics.Process.Start(algorithmStartInfo);
    }

    public void KillAlgorithm()
    {
        try
        {
            this.algorithm.Kill();
        }
        catch (Win32Exception)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    public string GetAnswerToInput(string input)
    {
        this.PushInput(input);

        string algorithmStandartOutput = this.algorithm.StandardOutput.ReadToEnd();
        string algorithmStandartError = this.algorithm.StandardError.ReadToEnd();

        if(algorithmStandartOutput == String.Empty)
        {
            return algorithmStandartError;
        }

        return algorithmStandartOutput;
    }

    public void PushInputAndStoreOutput(string input, StringBuilder output)
    {
        PushInput(input);
        StoreOutput(output);
    }

    public void StoreOutput(object outputStoreStringBuilder)
    {
        StringBuilder outputAsStrBuilder = outputStoreStringBuilder as StringBuilder;
        outputAsStrBuilder.Append(this.algorithm.StandardOutput.ReadToEnd());
    }

    public void StoreOutputLineByLine(object outputStoreStringBuilder)
    {
        var output = outputStoreStringBuilder as StringBuilder;
        while (this.algorithm.StandardOutput.EndOfStream)
        {
            output.Append(this.algorithm.StandardOutput.ReadLine());
        }
    }

    private async void ReadAlgorithmOutputLineAsync(StringBuilder storage)
    {
        this.IsReadLineOperationPending = true;
        string line = await this.algorithm.StandardOutput.ReadLineAsync();
        this.IsReadLineOperationPending = false;
    }

    public string GetMaxOutputLines(CancellationToken cancelToken)
    {
        StringBuilder gatheredOutputLines = new StringBuilder();

        while (!this.algorithm.StandardOutput.EndOfStream)
        {
            this.ReadAlgorithmOutputLineAsync(gatheredOutputLines);

            while (this.IsReadLineOperationPending)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return gatheredOutputLines.ToString();
                }
            }
        }

        return gatheredOutputLines.ToString();
    }

    public void PushInput(string input)
    {
        if (this.specialEncoding != null)
        {
            Console.InputEncoding = this.specialEncoding;
            Console.OutputEncoding = this.specialEncoding;
        }
        
        this.algorithm.StandardInput.Write(input);

        this.algorithm.StandardInput.Close();
    }
}
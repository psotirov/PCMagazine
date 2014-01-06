using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AlgorithmEvaluator
{
    //protected AlgorithmExecutor executor;
    protected ITest[] tests;
    protected IAnswerEvaluator evaluator;
    protected TimeSpan timeLimit;
    protected System.IO.TextWriter logWriter;

    public AlgorithmEvaluator(ICollection<ITest> tests, IAnswerEvaluator answerEvaluator, TimeSpan timeLimit, System.IO.TextWriter logWriter = null)
    {
        if (tests != null)
        {
            this.tests = tests.ToArray();
        }
        else 
        { 
            this.tests = null; 
        }

        this.evaluator = answerEvaluator;
        this.timeLimit = timeLimit;

        this.logWriter = logWriter;
        if (this.logWriter == null)
        {
            this.logWriter = Console.Out;
        }
    }

    //public AlgorithmEvaluator(AlgorithmExecutor executor, ICollection<ITest> tests, IAnswerEvaluator answerEvaluator, TimeSpan timeLimit) : this(tests, answerEvaluator, timeLimit)
    //{
    //    this.executor = executor;
    //}

    //public AlgorithmEvaluator(string algorithmFolder, ICollection<ITest> tests, IAnswerEvaluator answerEvaluator, TimeSpan timeLimit)
    //    : this(new AlgorithmExecutor(algorithmFolder), tests, answerEvaluator, timeLimit)
    //{
    //}

    //public void SetAlgorithm(AlgorithmExecutor newAlg)
    //{
    //    this.executor = newAlg;
    //}

    public int EvaluateWithTest(AlgorithmExecutor algorithm, ITest test, bool allowTimeLimit = false)
    {
        this.logWriter.WriteLine();
        this.logWriter.WriteLine("Evaluating {0} with test {1}", algorithm.AlgorithmOwner, test.Name);

        try
        {
            algorithm.StartAlgorithm();
        }
        catch (Win32Exception)
        {
            this.logWriter.WriteLine("--couldn't start algorithm");
            return 0;
        }

        DateTime start = DateTime.Now;

        string algorithmAnswer = "";
        try
        {
            Task<string> algorithmAnswerTask = Task.Run(() => algorithm.GetAnswerToInput(test.Input));

            bool timeLimitReached = false;
            //Console.WriteLine("press enter");
            //Console.ReadLine();
            System.Threading.Thread.Sleep(timeLimit - TimeSpan.FromMilliseconds(100));
            while (!algorithmAnswerTask.IsCompleted)
            {
                TimeSpan elapsed = DateTime.Now - start;
                if (elapsed > timeLimit)
                {
                    this.logWriter.WriteLine("--time limit reached, killing algorithm");
                    algorithm.KillAlgorithm();
                    timeLimitReached = true;
                    break;
                }
            }

            if (!timeLimitReached || allowTimeLimit)
            {
                this.logWriter.WriteLine("--attempting to retrieve output from algorithm");
                algorithmAnswerTask.Wait();
                algorithmAnswer += algorithmAnswerTask.Result;
            }
            else
            {
                return 0;
            }

            algorithm.KillAlgorithm(); //just in case
        }
        catch (Exception e)
        {
            this.logWriter.WriteLine("--unexpected error while evaluating: {0}", e.Message);
        }

        this.logWriter.Write("--algorithm output:\r\n{0}\r\n", algorithmAnswer);
        return this.evaluator.EvaluateAnswer(algorithmAnswer, test.Input, test.ExpectedOutput);
    }

    public int EvaluateWithAllTests(AlgorithmExecutor algorithm, bool allowTimeLimit = false)
    {
        if (algorithm == null)
        {
            throw new InvalidOperationException("No algorithm is set");
        }

        int total = 0;

        foreach (var currTest in this.tests)
        {
            total += this.EvaluateWithTest(algorithm, currTest, allowTimeLimit);
        }

        return total;
    }
}


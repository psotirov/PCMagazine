using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IAnswerEvaluator
{
    int EvaluateAnswer(string answer, string question, string expected);
    void UseLogWriter(System.IO.TextWriter writer);
}


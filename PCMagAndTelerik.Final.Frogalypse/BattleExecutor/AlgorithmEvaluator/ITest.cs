using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ITest
{
    string Name { get; }
    string Input { get; }
    string ExpectedOutput { get; }
    bool HasStrictOutput { get; }
    void LoadFrom(string filename);
}

public class NoExpectedOutputTest : ITest
{
    public virtual bool HasStrictOutput
    {
        get { return false; }
    }

    public virtual string ExpectedOutput
    {
        get { return ""; }
    }

    public virtual string Input { get; private set; }

    public virtual string Name { get; private set; }

    public NoExpectedOutputTest()
    {
    }

    public NoExpectedOutputTest(string testFilename)
    {
        this.LoadFrom(testFilename);
    }

    public void LoadFrom(string filename)
    {
        this.Name = filename.Substring(filename.LastIndexOf("\\") + 1);
        this.Input = System.IO.File.ReadAllText(filename);
    }
}


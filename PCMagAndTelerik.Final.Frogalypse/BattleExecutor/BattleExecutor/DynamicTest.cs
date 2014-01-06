using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleExecutor
{
    class DynamicTest : ITest
    {
        string content;

        public DynamicTest(string content)
        {
            this.content = content;
        }

        public string Name
        {
            get { return ""; }
        }

        public string Input
        {
            get { return this.content; }
        }

        public string ExpectedOutput
        {
            get { return ""; }
        }

        public bool HasStrictOutput
        {
            get { return false; }
        }

        public void LoadFrom(string filename)
        {
            throw new NotImplementedException();
        }
    }
}

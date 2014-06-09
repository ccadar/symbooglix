using System;
using Microsoft.Boogie;
using System.Collections.Generic;
using System.Diagnostics;

namespace symbooglix
{
    public class TerminationConsoleReporter : ITerminationHandler
    {
        public TerminationConsoleReporter()
        {
        }

        public static void WriteLine(ConsoleColor Colour ,string msg)
        {
            var savedColour = Console.ForegroundColor;
            Console.ForegroundColor = Colour;
            Console.WriteLine(msg);
            Console.ForegroundColor = savedColour;
        }


        public void handleSuccess(ExecutionState s)
        {
            string msg = "State " + s.id + " terminated without error";
            WriteLine(ConsoleColor.Green, msg);
        }

        public void handleFailingAssert(ExecutionState s)
        {
            string msg = "State " + s.id + " terminated with an error";
            WriteLine(ConsoleColor.Red, msg);
            Debug.Assert(s.getCurrentStackFrame().currentInstruction.Current is AssertCmd);
            var failingCmd = (AssertCmd) s.getCurrentStackFrame().currentInstruction.Current;
            msg = "The following assertion failed\n" +
                  failingCmd.tok.filename + ":" + failingCmd.tok.line + ": " +
                  failingCmd.ToString();
            WriteLine(ConsoleColor.DarkRed, msg);


            s.dumpState();
        }

        public void handleUnsatisfiableRequires(ExecutionState s, Microsoft.Boogie.Requires requiresStatement)
        {
            string msg = "State " + s.id + " terminated with an error";
            WriteLine(ConsoleColor.Red, msg);
            msg = "The following requires statement could not be satisfied\n" +
                  requiresStatement.tok.filename + ":" + requiresStatement.tok.line + ": " +
                  requiresStatement.Condition.ToString();
            WriteLine(ConsoleColor.DarkRed, msg);

            s.dumpState();
        }

        public void handleFailingEnsures(ExecutionState s)
        {
            throw new NotImplementedException();
        }

        public void handleUnsatisfiableAssume(ExecutionState s)
        {
            string msg = "State " + s.id + " terminated with an error";
            WriteLine(ConsoleColor.Red, msg);
            Debug.Assert(s.getCurrentStackFrame().currentInstruction.Current is AssumeCmd);
            var failingCmd = (AssumeCmd) s.getCurrentStackFrame().currentInstruction.Current;
            msg = "The following assumption is unsatisfiable\n" +
                  failingCmd.tok.filename + ":" + failingCmd.tok.line + ": " +
                  failingCmd.ToString();
            WriteLine(ConsoleColor.DarkRed, msg);


            s.dumpState();
        }
    }
}


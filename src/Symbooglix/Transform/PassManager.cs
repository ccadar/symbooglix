﻿using Microsoft.Boogie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Symbooglix
{
    namespace Transform
    {
        public class PassManager
        {
            protected List<Tuple<IPass,PassInfo>> Passes;
            public Program TheProgram
            {
                get;
                private set;
            }


            public class PassManagerEventArgs : EventArgs
            {
                public readonly IPass ThePass;
                public readonly Program TheProgram;
                public PassManagerEventArgs(IPass pass, Program program) { ThePass = pass; TheProgram = program; }
            }
            public delegate void PassRunEvent(Object sender, PassManagerEventArgs args);
            public event PassRunEvent BeforePassRun;
            public event PassRunEvent AfterPassRun;
            public event PassRunEvent Finished;


            public PassManager(Program prog)
            {
                Passes = new List<Tuple<IPass,PassInfo>>();
                TheProgram = prog;
            }

            public void Add(IPass pass)
            {
                var passInfo = new PassInfo();

                // Get the dependencies
                pass.SetPassInfo(ref passInfo);
                var tuple = Tuple.Create(pass, passInfo);

                if (passInfo.Dependencies != null)
                {
                    // We could be more sophisticatd here and try to optimise
                    // the list of passes so we don't run redundant analyses
                    // Leave this for now

                    // The pass has Dependencies so Add them

                    // It is not safe to iterate over a dictionary and modify it
                    // at the same time so do it in two stages.
                    // 1. Collect all the KeyValue Pairs in the dictionary
                    // 2. Iterate over the collected KeyValue pairs and modify the dictionary
                    var depList = new List<KeyValuePair<System.Type,IPass>>();
                    foreach (var keyValuePair in passInfo.Dependencies)
                    {
                        depList.Add(keyValuePair);
                    }

                    foreach (var keyValuePair in depList)
                    {
                        // Create dependency. This requires that the pass has a default constructor
                        IPass dependencyOfPass = Activator.CreateInstance(keyValuePair.Key) as IPass;

                        passInfo.Dependencies[keyValuePair.Key] = dependencyOfPass;

                        // Do this recursively so we handle any dependencies of the dependencies (of the...)*
                        Add(dependencyOfPass);
                    }

                }

                Passes.Add(tuple);
            }
                
            public void Run()
            {
                foreach (var pass in Passes.Select( p => p.Item1))
                {
                    if (BeforePassRun != null)
                        BeforePassRun(this, new PassManagerEventArgs(pass, TheProgram));

                    pass.RunOn(TheProgram);

                    if (AfterPassRun != null)
                        AfterPassRun(this, new PassManagerEventArgs(pass, TheProgram));
                }

                if (Finished != null)
                    Finished(this, new PassManagerEventArgs(null, TheProgram));
            }

            public class PassInfo
            {
                // FIXME: This shouldn't be public, only the PassManager should be able to access this
                public Dictionary<System.Type,IPass> Dependencies = null;

                // Passes use this to declare what passes they need run before them in SetPassInfo()
                public void AddDependency<T>() where T : IPass
                {
                    // We don't create the passes here
                    if (Dependencies == null)
                    {
                        Dependencies = new Dictionary<System.Type, IPass>();
                    }

                    // Don't create the passes here. Let the PassManager
                    // do it instead so it can optimise the list of passes to
                    // run.
                    Dependencies.Add(typeof(T), null);
                }

                // Passes can use this to gain access to passes they depend on.
                public T GetDependency<T>() where T : class, IPass
                {
                    return Dependencies[typeof(T)] as T;
                }
            }

        }
    }
}


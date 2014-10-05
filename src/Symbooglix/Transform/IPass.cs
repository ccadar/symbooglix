﻿using System;
using Microsoft.Boogie;
using System.Collections.Generic;

namespace Symbooglix
{
    namespace Transform
    {
        public interface IPass
        {
            string GetName();
            void SetPassInfo(ref PassManager.PassInfo passInfo);
            bool RunOn(Program prog);
        }
    }
}


﻿using System.Collections.Generic;
using System.IO;

namespace Raven.StackOverflow.Etl
{
    public interface ICommand
    {
        string CommandText { get; }
        void Run();

        void LoadArgs(string[] remainingArgs);
        void WriteHelp(TextWriter tw);
    }
}
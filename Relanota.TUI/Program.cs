using System;
using Terminal.Gui;

namespace Relanota.TUI
{
    class Program
    {

        static void Main(string[] args)
        {
            Application.Init();
            Application.UseSystemConsole = true;
            Relanota relanota = new Relanota();
            relanota.ColorScheme = Colors.TopLevel;
            StatusBar status = new StatusBar();
            status.Items = new[] {
                new StatusItem(Key.F | Key.AltMask, "~Alt-F~: Find", relanota.FocusSearch),
                new StatusItem(Key.N | Key.CtrlMask, "~Ctrl-N~: New", relanota.New),
                new StatusItem(Key.S | Key.CtrlMask, "~Ctrl-S~: Save", relanota.Save),
            };
            Application.Current.ColorScheme = Colors.TopLevel;
            Application.Top.Add(relanota, status);
            Application.Run();
        }

        //private ParseArgs ParseArgs()
        //{
        //    ParseArgs parseArgs = new ParseArgs();

        //    for (int i = 0; i < Args.Length; i++)
        //    {
        //        switch (Args[i].ToLower())
        //        {
        //            case "-n":
        //                {
        //                    parseArgs.LoadNote = true;
        //                    if (i + 1 < Args.Length)
        //                    {
        //                        string name = string.Join(' ', Args[i..Args.Length]);
        //                        parseArgs.NoteToLoad = name;
        //                    }
        //                    else
        //                    {
        //                        Error("'-n' flag requires a note to load");
        //                    }
        //                    break;
        //                }
        //        }
        //    }

        //    return parseArgs;
        //}
        //private void Error(string msg)
        //{
        //    Console.Error.WriteLine(msg);
        //    Environment.Exit(1);
        //}

    }
}

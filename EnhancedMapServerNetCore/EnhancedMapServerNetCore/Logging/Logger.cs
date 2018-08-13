using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace EnhancedMapServerNetCore.Logging
{
    public class Logger
    {
        private const int tableWidth = 77;

        public static readonly Dictionary<LogTypes, Tuple<ConsoleColor, string>> LogTypeInfo = new Dictionary<LogTypes, Tuple<ConsoleColor, string>> {{LogTypes.None, Tuple.Create(ConsoleColor.White, "")}, {LogTypes.Info, Tuple.Create(ConsoleColor.Green, "  Info    ")}, {LogTypes.Debug, Tuple.Create(ConsoleColor.DarkGreen, "  Debug   ")}, {LogTypes.Trace, Tuple.Create(ConsoleColor.Green, "  Trace   ")}, {LogTypes.Warning, Tuple.Create(ConsoleColor.Yellow, "  Warning ")}, {LogTypes.Error, Tuple.Create(ConsoleColor.Red, "  Error   ")}, {LogTypes.Panic, Tuple.Create(ConsoleColor.Red, "  Panic   ")}};

        private readonly BlockingCollection<Tuple<LogTypes, string, string>> logQueue;
        private bool isLogging;

        public Logger()
        {
            logQueue = new BlockingCollection<Tuple<LogTypes, string, string>>();
        }

        // No volatile support for properties, let's use a private backing field.
        public LogTypes LogTypes { get; set; }


        private void PrintRow(params string[] columns)
        {
            int width = (tableWidth - columns.Length) / columns.Length;
            string row = "|";

            foreach (string column in columns) row += AlignCentre(column, width) + "|";

            Console.WriteLine(row);
        }

        private string AlignCentre(string text, int width)
        {
            text = text.Length > width ? text.Substring(0, width - 3) + "..." : text;

            if (string.IsNullOrEmpty(text))
                return new string(' ', width);
            return text.PadRight(width - (width - text.Length) / 2).PadLeft(width);
        }


        public void Start(LogFile logFile = null)
        {
            var logThread = new Thread(async () =>
            {
                using (logQueue)
                using (logFile)
                {
                    isLogging = true;

                    while (isLogging)
                    {
                        Thread.Sleep(1);

                        // Do nothing if logging is turned off (LogTypes.None) & the log queue is empty, but continue the loop.
                        if (LogTypes == LogTypes.None || !logQueue.TryTake(out Tuple<LogTypes, string, string> log))
                            continue;

                        if (log.Item1 == LogTypes.Table)
                        {
                            Console.WriteLine(string.Format(log.Item3));
                            continue;
                        }

                        // LogTypes.None is also used for empty/simple log lines (without timestamp, etc.).
                        if (log.Item1 != LogTypes.None)
                        {
                            if (!Core.HeadLess)
                            {
                                Console.ForegroundColor = ConsoleColor.White;

                                Console.Write($"{log.Item2} |");

                                Console.ForegroundColor = LogTypeInfo[log.Item1].Item1;
                                Console.Write(LogTypeInfo[log.Item1].Item2);
                                Console.ForegroundColor = ConsoleColor.White;

                                Console.WriteLine($"| {log.Item3}");
                            }

                            if (logFile != null)
                                await logFile.WriteAsync($"{log.Item2} |{LogTypeInfo[log.Item1].Item2}| {log.Item3}");
                        }
                        else
                        {
                            if (!Core.HeadLess)
                                Console.WriteLine(log.Item3);

                            if (logFile != null)
                                await logFile.WriteAsync(log.Item3);
                        }
                    }
                }
            }) {IsBackground = true};
            logThread.Start();

            isLogging = logThread.ThreadState == ThreadState.Running || logThread.ThreadState == ThreadState.Background;
        }

        public void Stop()
        {
            isLogging = false;
        }

        public void Message(LogTypes logType, string text)
        {
            SetLogger(logType, text);
        }

        public void NewLine()
        {
            SetLogger(LogTypes.None, "");
        }

        public void WaitForKey()
        {
            Console.ReadKey(true);
        }

        public void Clear()
        {
            Console.Clear();
        }

        private void SetLogger(LogTypes type, string text)
        {
            if ((LogTypes & type) == type)
            {
                if (type == LogTypes.None)
                    logQueue.Add(Tuple.Create(type, "", text));
                else
                    logQueue.Add(Tuple.Create(type, DateTime.Now.ToString("T"), text));
            }
        }
    }
}
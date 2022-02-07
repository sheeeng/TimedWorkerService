namespace TimedWorkerService;

using System;
using System.IO;
using System.Text;

/// <summary>
/// Progress bar for c# console applications displayed at the bottom of the console/terminal.
/// https://gist.github.com/pigeonhands/b3b7d935cbc1446d248fd4e6c2d2b0e1
/// </summary>
public class ConsoleProgressBar : IDisposable
{
     public float CurrentProgress => _writer.CurrentProgress;

    private readonly TextWriter _originalWriter;
    private readonly ProgressWriter _writer;

    public ConsoleProgressBar() {
        _originalWriter = Console.Out;
        _writer = new ProgressWriter(_originalWriter);
        Console.SetOut(_writer);
    }

    public void Dispose() {
        Console.SetOut(_originalWriter);
        _writer.ClearProgressBar();
    }

    public void SetProgress(float f) {
        _writer.CurrentProgress = f;
        _writer.RedrawProgress();
    }
    public void SetProgress(int i) {
        SetProgress((float)i);
    }

    private void Increment(float f) {
        _writer.CurrentProgress += f;
        _writer.RedrawProgress();
    }

    public void Increment(int i) {
        Increment((float)i);
    }

    private class ProgressWriter : TextWriter {

        public override Encoding Encoding => Encoding.UTF8;
        public float CurrentProgress {
            get => _currentProgress;
            set {
                _currentProgress = value;
                if(_currentProgress > 100) {
                    _currentProgress = 100;
                }else if(CurrentProgress < 0) {
                    _currentProgress = 0;
                }
            }
        }

        private float _currentProgress = 0;
        private readonly TextWriter _consoleOut;
        private const string ProgressTemplate = "[{0}] {1:n2}%";
        private const int AllocatedTemplateSpace = 70;
        private readonly object _syncLock = new object();
        public ProgressWriter(TextWriter consoleOut) {
            this._consoleOut = consoleOut;
            RedrawProgress();
        }

        private void DrawProgressBar() {
            lock (_syncLock) {
                int availableSpace = Console.BufferWidth - AllocatedTemplateSpace;
                int percentAmount = (int)((float)availableSpace * (CurrentProgress / 100));
                var col = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                string progressBar = string.Concat(new string('=', percentAmount), new string(' ', availableSpace - percentAmount));
                _consoleOut.Write(string.Format(ProgressTemplate, progressBar, CurrentProgress));
                Console.ForegroundColor = col;
            }
        }

        public void RedrawProgress() {
            lock (_syncLock) {
                int lastLineWidth = Console.CursorLeft;
                var consoleH = Console.WindowTop + Console.WindowHeight - 1;
                Console.SetCursorPosition(0, consoleH);
                DrawProgressBar();
                Console.SetCursorPosition(lastLineWidth, consoleH - 1);
            }
        }

        private void ClearLineEnd() {
            lock (_syncLock) {
                int lineEndClear = Console.BufferWidth - Console.CursorLeft - 1;
                _consoleOut.Write(new string(' ', lineEndClear));
            }
        }

        public void ClearProgressBar() {
            lock (_syncLock) {
                int lastLineWidth = Console.CursorLeft;
                var consoleH = Console.WindowTop + Console.WindowHeight - 1;
                Console.SetCursorPosition(0, consoleH);
                ClearLineEnd();
                Console.SetCursorPosition(lastLineWidth, consoleH - 1);
            }
        }

        public override void Write(char value) {
            lock (_syncLock) {
                _consoleOut.Write(value);
            }
        }

        public override void Write(string? value) {
            lock (_syncLock) {
                _consoleOut.Write(value);
            }
        }

        public override void WriteLine(string? value) {
            lock (_syncLock) {
                _consoleOut.Write(value);
                _consoleOut.Write(Environment.NewLine);
                ClearLineEnd();
                _consoleOut.Write(Environment.NewLine);
                RedrawProgress();
            }
        }

        public override void WriteLine(string format, params object?[] arg) {
            WriteLine(string.Format(format, arg));
        }

        public override void WriteLine(int i) {
            WriteLine(i.ToString());
        }

    }
}

using System.Globalization;

namespace TimedWorkerService;

using System;
using System.Text;
using System.Threading;

/// <summary>
/// An ASCII Progress Bar
/// https://gist.github.com/DanielSWolf/0ab6a96899cc5377bf54
/// </summary>
public class ProgressBar : IDisposable, IProgress<double> {
	private const int BlockCount = 66;
	private readonly TimeSpan _animationInterval = TimeSpan.FromSeconds(1.0 / 8);
	// private const string Animation = @"|/-\";
	// private const string Animation = @"✶✸✹✺✹✷";
	// private const string Animation = @"◌ဝ⦾⦿◍◎⦾ဝ◌";
	private const string Animation = @"▌▀▐▄";

	private readonly Timer _timer;

	private double _currentProgress = 0;
	private string _currentText = string.Empty;
	private bool _disposed = false;
	private int _animationIndex = 0;

	public ProgressBar() {
		_timer = new Timer(TimerHandler);

		// A progress bar is only for temporary display in a console window.
		// If the console output is redirected to a file, draw nothing.
		// Otherwise, we'll end up with a lot of garbage in the target file.
		if (!Console.IsOutputRedirected) {
			ResetTimer();
		}
	}

	public void Report(double value) {
		// Make sure value is in [0..1] range
		// Console.WriteLine(value);
		// value = Math.Max(0, Math.Min(1, value));
		Interlocked.Exchange(ref _currentProgress, value);
	}

	private void TimerHandler(object? state) {
		lock (_timer) {
			if (_disposed) return;

			var completedBlockCount = Convert.ToInt32(_currentProgress * BlockCount);
			var percent = Convert.ToDouble(_currentProgress * 100);
			var text =
				$"▕{new string('█', completedBlockCount)}{new string('▁', BlockCount - completedBlockCount)}▏" +
				$" {percent.ToString("0.##", CultureInfo.InvariantCulture).PadLeft(6, ' ')}%" +
				$" [{Animation[_animationIndex++ % Animation.Length]}]";
				// $" {percent.ToString("00.00", CultureInfo.InvariantCulture)} %" +
				// $" {String.Format("{0,6:0.00}", percent)}%" +
			UpdateText(text);

			ResetTimer();
		}
	}

	private void UpdateText(string text) {
		// Get length of common portion
		var commonPrefixLength = 0;
		var commonLength = Math.Min(_currentText.Length, text.Length);
		while (commonPrefixLength < commonLength && text[commonPrefixLength] == _currentText[commonPrefixLength]) {
			commonPrefixLength++;
		}

		// Backtrack to the first differing character
		StringBuilder outputBuilder = new StringBuilder();
		outputBuilder.Append('\b', _currentText.Length - commonPrefixLength);

		// Output new suffix
		outputBuilder.Append(text.Substring(commonPrefixLength));

		// If the new text is shorter than the old one: delete overlapping characters
		var overlapCount = _currentText.Length - text.Length;
		if (overlapCount > 0) {
			outputBuilder.Append(' ', overlapCount);
			outputBuilder.Append('\b', overlapCount);
		}

		Console.Write(outputBuilder);
		_currentText = text;
	}

	private void ResetTimer() {
		_timer.Change(_animationInterval, TimeSpan.FromMilliseconds(-1));
	}

	public void Dispose() {
		lock (_timer) {
			_disposed = true;
			UpdateText(string.Empty);
		}
	}

}

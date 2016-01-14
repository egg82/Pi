using System;
using Util;
using NAudio.Wave;
using System.IO;
using Patterns.Observer;
using System.Collections.Generic;
using ManagedBass;
using ManagedBass.Dynamics;

namespace Speech {
	public class MicrophoneEngine : IDispatchable {
		//structs
		private struct Point {
			public double x;
			public double y;
		}

		//vars
		public static readonly List<Observer> OBSERVERS = new List<Observer>();

		private WaveInEvent winSource;
		private Recording unixSource;

		private MemoryStream _stream;
		private double _threshold;
		private bool _speechStarted;
		private uint _currentWait;
		private uint _waitPeriod;
		private uint _samples;
		private uint _minSamples;

		//constructor
		public MicrophoneEngine(double threshold, uint waitPeriod, uint minSamples) {
			if (SystemUtil.PLATFORM == PlatformID.MacOSX || SystemUtil.PLATFORM == PlatformID.Unix) {
				//unixSource = new Recording(RecordingDevice.DefaultDevice, Resolution.Byte);
				/*unixSource.Info = new ChannelInfo() {
					Channels = 1,
					ChannelType = ChannelType.WAV_FLOAT,
					Frequency = 48000
				};*/
				/*unixSource.Callback += onUnixSourceDataAvailable;
				unixSource.Stopped += onUnixSourceRecordingStopped;*/
			} else {
				winSource = new WaveInEvent();
				winSource.WaveFormat = new WaveFormat(16000, 1);
				winSource.DataAvailable += new EventHandler<WaveInEventArgs>(onWinSourceDataAvailable);
				winSource.RecordingStopped += new EventHandler<StoppedEventArgs>(onWinSourceRecordingStopped);
			}

			_threshold = threshold;
			_waitPeriod = waitPeriod;
			_minSamples = minSamples;
		}

		//public
		public void start() {
			if (_stream != null) {
				try {
					_stream.Dispose();
				} catch (Exception) {

				}
			}

			_stream = new MemoryStream();
			_speechStarted = false;
			_currentWait = 0;
			_samples = 0;

			if (winSource != null) {
				winSource.StartRecording();
			}
			if (unixSource != null) {
				unixSource.Start();
			}
		}
		public void stop() {
			if (winSource != null) {
				winSource.StopRecording();
			}
			if (unixSource != null) {
				unixSource.Stop();
			}
		}

		public void dispatch(string evnt, object data = null) {
			Observer.dispatch(OBSERVERS, this, evnt, data);
		}

		//private
		private void onWinSourceDataAvailable(object sender, WaveInEventArgs e) {
			getData(e.Buffer);
		}
		private void onWinSourceRecordingStopped(object sender, StoppedEventArgs e) {
			stopData();
		}

		private void onUnixSourceDataAvailable(BufferProvider e) {
			byte[] data = new byte[e.ByteLength];
			e.Read(data, 0, e.ByteLength);
			getData(data);
		}
		private void onUnixSourceRecordingStopped() {
			stopData();
		}

		private void getData(byte[] buffer) {
			double amplitude = analyzeData(buffer);

			if (Math.Abs(amplitude) > _threshold) {
				Console.WriteLine(Math.Abs(amplitude));

				_speechStarted = true;
				_currentWait = 0;
				_samples++;

				lock (_stream) {
					_stream.Write(buffer, 0, buffer.Length);
				}
			} else {
				_currentWait++;
				if (_currentWait >= _waitPeriod && _samples >= _minSamples) {
					_samples = 0;
					stopData();
					_stream = new MemoryStream();
				}
			}
		}
		private void stopData() {
			if (_speechStarted) {
				_speechStarted = false;
				dispatch(MicrophoneEngineEvent.SOUND, _stream.ToArray());
				_stream.Close();
			}
		}

		private double analyzeData(byte[] buffer) {
			short[] left = new short[buffer.Length / 2];

			int x = 0;
			for (int s = 0; s < buffer.Length; s = s + 2) {
				left[x] = (short) (buffer[s + 1] * 0x100 + buffer[s]);
				x++;
			}

			return average(left);
		}
		private double average(short[] input) {
			double result = 0;
			for (int i = 0; i < input.Length; i++) {
				result += input[i];
			}
			result = Math.Round(result / input.Length);

			return result;
		}
	}
}
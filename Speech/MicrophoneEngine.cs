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
		//vars
		public static readonly List<Observer> OBSERVERS = new List<Observer>();

		private WaveInEvent winSource;
		private Recording unixSource;

		private MemoryStream _stream;

		//constructor
		public MicrophoneEngine() {
			if (SystemUtil.PLATFORM == PlatformID.MacOSX || SystemUtil.PLATFORM == PlatformID.Unix) {
				unixSource = new Recording(RecordingDevice.DefaultDevice, Resolution.Byte);
				unixSource.Info.Channels = 1;
				unixSource.Info.ChannelType = ChannelType.WAV_FLOAT;
				unixSource.Info.Frequency = 48000;
				unixSource.Callback += onUnixSourceDataAvailable;
				unixSource.Stopped += onUnixSourceRecordingStopped;
			} else {
				winSource = new WaveInEvent();
				winSource.WaveFormat = new WaveFormat(48000, 16, 1);
				winSource.DataAvailable += new EventHandler<WaveInEventArgs>(onWinSourceDataAvailable);
				winSource.RecordingStopped += new EventHandler<StoppedEventArgs>(onWinSourceRecordingStopped);
			}
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

		public MemoryStream stream {
			get {
				return _stream;
			}
		}

		public void dispatch(string evnt, object data = null) {
			Observer.dispatch(OBSERVERS, this, evnt, data);
		}

		//private
		private void onWinSourceDataAvailable(object sender, WaveInEventArgs e) {
			lock (stream) {
				stream.Write(e.Buffer, 0, e.BytesRecorded);
			}
		}
		private void onWinSourceRecordingStopped(object sender, StoppedEventArgs e) {
			
		}

		private void onUnixSourceDataAvailable(BufferProvider e) {
			byte[] data = new byte[e.ByteLength];
			e.Read(data, 0, e.ByteLength);

			lock (stream) {
				stream.Write(data, 0, data.Length);
			}
		}
		private void onUnixSourceRecordingStopped() {

		}
	}
}
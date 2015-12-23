using Crypto;
using NAudio.Wave;
using Patterns.Observer;
using Syn.Speech.Api;
using System;
using System.Collections.Generic;
using System.IO;
using Util;

namespace Speech {
	class SpeechEngine : IDispatchable {
		//vars
		public static readonly List<Observer> OBSERVERS = new List<Observer>();

		private StreamSpeechRecognizer engine;

		private MemoryStream stream = new MemoryStream();
		private WaveInEvent source = new WaveInEvent();
		private WaveOutEvent sOut = new WaveOutEvent();

		//constructor
		public SpeechEngine() {
			string models = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "Models";

			Configuration config = new Configuration();
			config.AcousticModelPath = models;
			config.DictionaryPath = models + Path.DirectorySeparatorChar + "cmudict-en-us.dict";
			config.LanguageModelPath = models + Path.DirectorySeparatorChar + "en-us.lm.dmp";
			config.SampleRate = 48000;

			engine = new StreamSpeechRecognizer(config);

			source.WaveFormat = new WaveFormat(16000, 1);
			source.DataAvailable += new EventHandler<WaveInEventArgs>(onSourceDataAvailable);
			source.RecordingStopped += new EventHandler<StoppedEventArgs>(onSourceRecordingStopped);

			/*sOut.Init(new WaveInProvider(source));
			sOut.Play();*/
		}

		//public
		public void start() {
			source.StartRecording();
		}
		public void stop() {
			source.StopRecording();
		}

		public void dispatch(string evnt, object data = null) {
			Observer.dispatch(OBSERVERS, this, evnt, data);
		}

		//private
		private void onSourceDataAvailable(object sender, WaveInEventArgs e) {
			Console.WriteLine(ByteUtil.toString(ByteUtil.hex(Hash.sha256(e.Buffer))));

			lock (stream) {
				stream.Write(e.Buffer, 0, e.BytesRecorded);

				engine.StartRecognition(stream);
				Console.WriteLine("Speech recognized: " + engine.GetResult().GetHypothesis());
				engine.StopRecognition();

				stream = new MemoryStream();
			}
		}
		private void onSourceRecordingStopped(object sender, StoppedEventArgs e) {
			stream.Dispose();
		}
	}
}

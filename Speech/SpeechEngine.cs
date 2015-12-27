using Syn.Speech.Api;
using Util;
using System.IO;
using System;

namespace Speech {
	class SpeechEngine {
		//vars
		private StreamSpeechRecognizer engine;

		//constructor
		public SpeechEngine() {
			string models = SystemUtil.CWD + SystemUtil.SEPARATOR + "Models";

			Configuration config = new Configuration();
			config.AcousticModelPath = models;
			config.DictionaryPath = models + SystemUtil.SEPARATOR + "cmudict-en-us.dict";
			config.LanguageModelPath = models + SystemUtil.SEPARATOR + "en-us.lm.dmp";
			config.SampleRate = 48000;

			engine = new StreamSpeechRecognizer(config);
		}

		//public
		public string recognize(MemoryStream stream) {
			string output;

			lock (engine) {
				engine.StartRecognition(stream);
				output = engine.GetResult().GetHypothesis();
				engine.StopRecognition();
			}

			Console.WriteLine("Speech recognized: " + output);
			return output;
		}

		//private

	}
}

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
			//config.SampleRate = 16000;
			config.UseGrammar = true;
			config.GrammarPath = SystemUtil.CWD + SystemUtil.SEPARATOR + "mods" + SystemUtil.SEPARATOR + "random";
			config.GrammarName = "number";

			engine = new StreamSpeechRecognizer(config);
		}

		//public
		public string recognize(byte[] data) {
			string output = null;
			SpeechResult result;
			MemoryStream stream = new MemoryStream(data);

			lock (engine) {
				engine.StartRecognition(stream);
				result = engine.GetResult();
				if (result != null) {
					output = result.GetHypothesis();
				}
				engine.StopRecognition();
			}

			Console.WriteLine("Speech recognized: " + output);
			return output;
		}
		public byte[] synthesize(string input) {
			return new byte[0];
		}

		//private

	}
}

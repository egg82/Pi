using NAudio.Wave;
using System;
using Util;

namespace Speech {
	public class AudioEngine {
		//vars
		private WaveOutEvent winOut;
		private dynamic unixOut;

		//constructor
		public AudioEngine() {
			if (SystemUtil.PLATFORM == PlatformID.MacOSX || SystemUtil.PLATFORM == PlatformID.Unix) {

			} else {
				winOut = new WaveOutEvent();
				//winOut.Init(???);
				winOut.Play();
			}
		}

		//public
		public void play(byte[] data) {
			if (winOut != null) {
				
			}
		}

		//private

	}
}
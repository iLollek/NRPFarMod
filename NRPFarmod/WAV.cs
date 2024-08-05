using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRPFarmod {

    [Obsolete("Unused")]
    public class WAV {
        public int ChannelCount { get; private set; }
        public int Frequency { get; private set; }
        public int SampleCount { get; private set; }
        public float[] LeftChannel { get; private set; }

        public WAV(byte[] wav) {
            ChannelCount = BitConverter.ToInt16(wav, 22);
            Frequency = BitConverter.ToInt32(wav, 24);
            int pos = 12;
            while (!(wav[pos] == 'd' && wav[pos + 1] == 'a' && wav[pos + 2] == 't' && wav[pos + 3] == 'a')) {
                pos += 4;
                int chunkSize = BitConverter.ToInt32(wav, pos);
                pos += 4 + chunkSize;
            }
            pos += 8;
            SampleCount = (wav.Length - pos) / 2;
            LeftChannel = new float[SampleCount];
            int i = 0;
            while (pos < wav.Length) {
                LeftChannel[i++] = BitConverter.ToInt16(wav, pos) / 32768.0f;
                pos += 2;
            }
        }
    }
}
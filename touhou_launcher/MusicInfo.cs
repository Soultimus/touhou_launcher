using System;
using System.IO;

namespace touhou_launcher
{
    public class MusicInfo
    {
        private const int FMT_SONG_LENGTH = 52;

        public FileInfo PCMFile { get; }
        public byte[] PCM { get; }
        public MusicFMTData FMT { get; }

        public MusicInfo(FileInfo pcm, FileInfo fmtFile, int index)
        {
            // Load fmt data
            try
            {
                using (FileStream fmtStream = fmtFile.OpenRead())
                {
                    byte[] fmtBytes = new byte[FMT_SONG_LENGTH];
                    fmtStream.Seek(index * FMT_SONG_LENGTH, SeekOrigin.Begin);
                    fmtStream.Read(fmtBytes, 0, FMT_SONG_LENGTH);
                    FMT = new MusicFMTData(fmtBytes);
                }
            }
            catch (IOException e)
            {
                throw new ArgumentException("Invalid FMT file", e);
            }

            // Load music data
            try
            {
                using (FileStream fs = pcm.OpenRead())
                {
                    PCM = new byte[this.FMT.TotalLength];
                    fs.Read(PCM, 0, FMT.TotalLength);
                }
            }
            catch (IOException e)
            {
                throw new ArgumentException("Invalid PCM file", e);
            }
            PCMFile = pcm;
        }
    }
}
using System;
using System.IO;
using System.Runtime;
using NAudio.Wave;
using NAudio.Wave.Compression;
using touhou_launcher;

public class MusicHandler
{
    private WaveFormat wav;
    private MusicInfo mi;
    private WaveOutEvent waveOut;

    public MusicHandler(MusicInfo mi, bool trance)
    {
        this.mi = mi;
        // In Ten Desires, going into Trance mode plays unique songs that have half the sample rate of normal songs
        int sampleRate = (trance) ? 22050 : 44100;
        wav = new WaveFormat(sampleRate, 16, 2);
        waveOut = null;
    }

    public void PlayTrack()
    {
        try
        {
            waveOut = new WaveOutEvent();
            var fileStream = File.OpenRead(mi.PCMFile.FullName);
            var rawStream = new RawSourceWaveStream(fileStream, wav);
            var pcmStream = new RangeWaveStream(rawStream, mi.FMT.StartOffset, (mi.FMT.TotalLength + mi.FMT.StartOffset), mi.FMT.IntroLength);

            waveOut.Init(pcmStream);
            waveOut.Play();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    public void StopTrack()
    {
        waveOut?.Stop();
    }

    public void PauseTrack()
    {
        waveOut?.Pause();
    }

    public void ResumeTrack()
    {
        waveOut?.Play();
    }

    private void OnPlaybackStopped(object sender, StoppedEventArgs e)
    {
        // Handle stop event here
    }

    public class RangeWaveStream : WaveStream
    {
        private readonly WaveStream sourceStream;
        private readonly long startPosition;
        private long position;
        private long introLength;

        public RangeWaveStream(WaveStream sourceStream, long startSample, long endSample, long introLength)
        {
            this.sourceStream = sourceStream;
            this.startPosition = startSample;
            this.position = this.startPosition;
            this.Length = (endSample - startSample);
            this.introLength = introLength;
        }

        public override WaveFormat WaveFormat => sourceStream.WaveFormat;

        public override long Length { get; }

        public override long Position
        {
            get => position;
            set => position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (position >= startPosition + Length)
            {
                position = startPosition + introLength;
            }

            sourceStream.Position = position;
            int bytesRead = sourceStream.Read(buffer, offset, count);
            position += bytesRead;
            return bytesRead;
        }
    }
}

using System;
using System.IO;
using System.Text;

public class MusicFMTData
{
    public string Name { get; private set; }
    public int StartOffset { get; private set; }
    public int IntroLength { get; private set; }
    public int TotalLength { get; private set; }
    public byte[] FmtHeader { get; private set; }

    public MusicFMTData(string name, int startOffset, int introLength, int totalLength, byte[] fmtHeader)
    {
        this.Name = name;
        this.StartOffset = startOffset;
        this.IntroLength = introLength;
        this.TotalLength = totalLength;
        this.FmtHeader = fmtHeader;
    }

    public MusicFMTData(byte[] bytes)
    {
        using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes)))
        {
            this.Name = Encoding.ASCII.GetString(reader.ReadBytes(16)).TrimEnd('\0');
            this.StartOffset = reader.ReadInt32();
            reader.BaseStream.Seek(4, SeekOrigin.Current);
            this.IntroLength = reader.ReadInt32();
            this.TotalLength = reader.ReadInt32();
            this.FmtHeader = reader.ReadBytes(18);
        }
    }
}

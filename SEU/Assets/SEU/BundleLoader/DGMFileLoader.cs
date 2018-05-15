using H3D.Engine;
using System.IO;
using System;
public class DGMFile : IDisposable
{
    private ABFileBufferPool.FileBuffer m_buffer;

    private Stream m_FileStream;

    public DGMFile(Stream fileStream)
    {
        m_FileStream = fileStream;
    }

    public static bool Exist(string path)
    {
        return File.Exists(path);
    }
    public static bool ExistStreamingFile(string path)
    {
        return StreamingAssetsLoader.Exists(path);
    }

    public static DGMFile Open(string path)
    {
        Stream stream = File.OpenRead(path);
        DGMFile file = new DGMFile(stream);
        return file;
    }
    public static DGMFile OpenStreamingFile(string path)
    {
        Stream stream = new ApkAssetStream(path);
        DGMFile file = new DGMFile(stream);
        return file;
    }

    public byte[] Read()
    {
        m_buffer = new ABFileBufferPool.FileBuffer(m_FileStream.Length);
        m_FileStream.Read(m_buffer.data, 0, (int)m_FileStream.Length);
        return m_buffer.data;
    }
    private bool disposedValue = false;
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                m_FileStream.Dispose();
                m_buffer.Dispose();
            }
            disposedValue = true;
        }
    }
    void IDisposable.Dispose()
    {
        Dispose(true);
    }
    public void Close()
    {
        Dispose(true);
    }
}

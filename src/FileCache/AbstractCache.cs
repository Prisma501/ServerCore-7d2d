namespace ServerCore.FileCache
{
    public abstract class AbstractCache
    {
        public abstract byte[] GetFileContent(string _filename);
    }
}
namespace Performance.Playground.Writers
{
    public static class Buffers
    {
        public static byte[] IncrementedBuffer(int length)
        {
            var data = new byte[20000];
            for (int i = 0; i < 20000; i++)
            {
                data[i] = (byte) i;
            }
            return data;
        }
    }
}
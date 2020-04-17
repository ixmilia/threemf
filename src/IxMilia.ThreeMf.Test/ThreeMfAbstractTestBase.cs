using System.IO;
using System.Text;

namespace IxMilia.ThreeMf.Test
{
    public abstract class ThreeMfAbstractTestBase
    {
        public static byte[] StringToBytes(string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        public static string BytesToString(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }

        public static ThreeMfFile RoundTripFile(ThreeMfFile file)
        {
            using (var ms = new MemoryStream())
            {
                file.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var file2 = ThreeMfFile.Load(ms);
                return file2;
            }
        }
    }
}



namespace HZK
{
    public class ChineseHelper
    {

        public int GetOffset(byte gb1, byte gb2)
        {
            return (int)(94 * (gb1 - 1) + (gb2 - 1)) * 32;
        }

        /// <summary>
        /// 获取 gb2312 码的区位对应的在字库文件中的偏移
        /// </summary>
        /// <param name="gb2312"></param>
        /// <returns></returns>
        public int[] GetOffset(byte[] gb2312)
        {
            if (gb2312 is null)
                return new int[0];

            int size = gb2312.Length / 2;
            if (size == 0)
                return new int[0];

            var result = new int[size];
            for (int i = 0; i < gb2312.Length;)
            {
                result[i / 2] = GetOffset(gb2312[i], gb2312[i + 1]);
                i += 2;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public byte[] Utf2Gb2312Byte(byte[] source)
        {
            if (source is null)
                return new byte[0];

            return new byte[0];
        }

    }
}

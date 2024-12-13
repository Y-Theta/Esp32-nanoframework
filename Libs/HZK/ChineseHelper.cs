

namespace HZK
{
    public class ChineseHelper
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public int GetOffset(byte gb1, byte gb2)
        {
            int offset = (int)(94 * (gb1 - 1) + (gb2 - 1)) * 32;
            return offset;
        }




    }
}

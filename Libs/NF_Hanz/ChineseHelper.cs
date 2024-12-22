using nanoFramework.Runtime.Native;

using System;
using System.Collections;

using static NF_Hanz.ChineseHelper;

namespace NF_Hanz
{
    /// <summary>
    /// 
    /// </summary>
    public class ChineseHelper
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="utfbytes"></param>
        /// <returns></returns>
        public static byte[] Utf2Gb2312(byte[] utfbytes)
        {
            if (utfbytes is null || utfbytes.Length == 0)
                return new byte[0];

            ArrayList list = new ArrayList();
            for (int i = 0; i < utfbytes.Length;)
            {
                var b = utfbytes[i];
                if ((b & 0xe0) == 0xe0)
                {
                    var s = Byte2Int(utfbytes[i], utfbytes[i + 1], utfbytes[i + 2]);
                    var final = B_S(0, 7296, s);
                    var chinese = ToBigEndianBytes(final);
                    foreach (var item in chinese)
                    {
                        list.Add(item);
                    }
                    i += 3;
                }
            }

            return (byte[])list.ToArray(typeof(byte));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gb2312bytes"></param>
        /// <returns></returns>
        public static byte[] GetHanzPoint(byte[] gb2312bytes)
        {
            ArrayList list = new ArrayList();
            for (int i = 0; i < gb2312bytes.Length;)
            {
                var bytes = GetHanzPoint(gb2312bytes[0], gb2312bytes[1]);
                foreach (var pt in bytes)
                {
                    list.Add(pt);
                }
                i += 2;
            }
            return (byte[])list.ToArray(typeof(byte));
        }

        private static byte[] GetHanzPoint(byte b1, byte b2)
        {
            b1 -= 0xa0;
            b2 -= 0xa0;
            var offset = ((94 * (b1 - 1)) + (b2 - 1)) * 32;
            var nbyte = (byte[])ResourceUtility.GetObject(Resource1.ResourceManager, Resource1.BinaryResources.hzk16h, offset, 32);
            return nbyte;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        static byte[] ToBigEndianBytes(int num)
        {
            // 将整数转换为字节数组
            byte[] bytes = BitConverter.GetBytes(num);
            return new byte[] { bytes[1], bytes[0] };
        }

        /// <summary>
        /// 翻转位 
        /// </summary>
        /// <param name="old"></param>
        /// <returns></returns>
        public static byte ReverseByte(byte old)
        {
            byte newb = 0;
            for (int x = 0; x < 8; x++)
            {
                newb = (byte)(newb << 1);
                newb = (byte)(newb | ((old >> x) & 0b1));
            }
            return newb;
        }

        /// <summary>
        /// 二分查找
        /// </summary>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        private static int B_S(int low, int high, int m)
        {
            if (low >= 0 && low <= 7296 && high <= 7296 && high >= 0 && low <= high)
            {
                int mid = (low + high) / 2;
                int index = mid * 12;
                byte[] bytsl = (byte[])ResourceUtility.GetObject(Resource1.ResourceManager, Resource1.BinaryResources.utf2gb2312, index, 6);
                var mm = ToNum(bytsl);
                if (mm < m)
                    return B_S(mid + 1, high, m);
                else if (mm > m)
                    return B_S(low, mid - 1, m);
                else
                    return ToNum((byte[])ResourceUtility.GetObject(Resource1.ResourceManager, Resource1.BinaryResources.utf2gb2312, index + 7, 4));
            }

            return 0;
        }

        private static int Byte2Int(params byte[] bytes)
        {
            int s = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                s = s << 8;
                s += bytes[i];
            }
            return s;
        }

        private static int ToNum(byte[] hexarray)
        {
            int hex = 0;
            for (int i = hexarray.Length - 1; i >= 0; i--)
            {
                int cur = 0;
                var single = hexarray[i];
                if (single >= 97)
                {
                    cur = single - 87;
                }
                else
                {
                    cur = single - 48;
                }
                hex += cur * 1 << (4 * (hexarray.Length - 1 - i));
            }
            return hex;
        }

        //public class hzFont 
        //{
        //    public override byte Height => 16;
        //    public override byte Width => 16;

        //    public override byte[] this[char character]
        //    {
        //        get
        //        {
        //            try
        //            {
        //                var bytes = UTF8Encoding.UTF8.GetBytes(character.ToString());
        //                byte[] buffer = new byte[32];
        //                var gb = ChineseHelper.Utf2Gb2312(bytes);
        //                var pts = ChineseHelper.GetHanzPoint(gb);
        //                for (int i = 0; i < 32; i++)
        //                {
        //                    buffer[i] = ChineseHelper.ReverseByte(pts[i]);
        //                }
        //                return buffer;
        //                //var hanz = Resources.GetBytes(BinaryResources.hzk16h);
        //                //for (int i = 0; i < 32; i++)
        //                //{
        //                //    buffer[i] = hanz[offset + i];
        //                //}
        //            }
        //            catch { }

        //            return new byte[0];
        //        }
        //    }
        //}
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HzRes
{
    public class ResourceManager
    {

        public const string RES_MAIN = nameof(Resource);

        public const string HZK16H = nameof(Resource.hzk16h);
        public const string UTF2GB2312 = nameof(Resource.utf2gb2312);


        public static byte[] GetResource(string rescontainer, string resName)
        {
            var types = typeof(ResourceManager).Assembly.GetTypes();
            var container = types.FirstOrDefault(type => type.Name == rescontainer);
            if (container is null)
                return new byte[0];

            var res = container.GetProperty(resName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            if (res != null)
            {
                return res.GetValue(null) as byte[];
            }

            return new byte[0];
        }
    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace NF_Hanz
{
    
    internal partial class Resource1
    {
        private static System.Resources.ResourceManager manager;
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if ((Resource1.manager == null))
                {
                    Resource1.manager = new System.Resources.ResourceManager("NF_Hanz.Resource1", typeof(Resource1).Assembly);
                }
                return Resource1.manager;
            }
        }
        internal static byte[] GetBytes(Resource1.BinaryResources id)
        {
            return ((byte[])(nanoFramework.Runtime.Native.ResourceUtility.GetObject(ResourceManager, id)));
        }
        [System.SerializableAttribute()]
        internal enum BinaryResources : short
        {
            hzk16h = -3840,
            utf2gb2312 = 16692,
        }
    }
}

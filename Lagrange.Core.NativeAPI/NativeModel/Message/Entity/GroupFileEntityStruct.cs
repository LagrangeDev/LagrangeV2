using System.Runtime.InteropServices;
using System.Text;
using Lagrange.Core.Message.Entities;
using Lagrange.Core.NativeAPI.NativeModel.Common;

namespace Lagrange.Core.NativeAPI.NativeModel.Message.Entity
{
    [StructLayout(LayoutKind.Sequential)]
    public struct GroupFileEntityStruct : IEntityStruct
    {
        public GroupFileEntityStruct() { }

        public ByteArrayNative FileId = new();

        public ByteArrayNative FileName = new();

        public long FileSize = 0;

        public ByteArrayNative FileMd5 = new();

        public ByteArrayNative FileUrl = new();

        public static implicit operator GroupFileEntityStruct(GroupFileEntity entity)
        {
            return new GroupFileEntityStruct()
            {
                FileId = Encoding.UTF8.GetBytes(entity.FileId),
                FileName = Encoding.UTF8.GetBytes(entity.FileName),
                FileSize = entity.FileSize,
                FileMd5 = Encoding.UTF8.GetBytes(entity.FileMd5),
                FileUrl = Encoding.UTF8.GetBytes(entity.FileUrl),
            };
        }
    }
}
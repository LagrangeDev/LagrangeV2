// Changelog:
// 2025.07.24 Kamenomi-dev 我舍弃掉了  BotKeystoreStruct，因为我认为这只会使代码复杂，走弯路。

//using System.Text;
//using Lagrange.Core.Common;

//namespace Lagrange.Core.NativeAPI.NativeModel.Common
//{
//    public struct BotKeystoreStruct
//    {
//        public BotKeystoreStruct() { }

//        public long Uin = 0;

//        public ByteArrayNative Uid = new();

//        public ByteArrayNative Guid = new();

//        public ByteArrayNative AndroidId = new();

//        public ByteArrayNative Qimei = new();

//        public ByteArrayNative DeviceName = new();

//        // WLoginSigs

//        public ByteArrayNative A2 = new();

//        public ByteArrayNative A2Key = new byte[16];

//        public ByteArrayNative D2 = new();

//        public ByteArrayNative D2Key = new byte[16];

//        public ByteArrayNative A1 = new();

//        public ByteArrayNative A1Key = new byte[16];

//        public ByteArrayNative NoPicSig = new();

//        public ByteArrayNative TgtgtKey = new();

//        public ByteArrayNative Ksid = new();

//        public ByteArrayNative SuperKey = new();

//        public ByteArrayNative StKey = new();

//        public ByteArrayNative StWeb = new();

//        public ByteArrayNative St = new();

//        public ByteArrayNative WtSessionTicket = new();

//        public ByteArrayNative WtSessionTicketKey = new();

//        public ByteArrayNative RandomKey = new byte[16];

//        public ByteArrayNative SKey = new();

//        public ByteArrayDictNative PsKey = new();

//        public static implicit operator BotKeystore(BotKeystoreStruct keystore)
//        {
//            var psKey = new Dictionary<string, string>();
//            foreach (var kvp in (ByteArrayKVPNative[])keystore.PsKey)
//            {
//                psKey[Encoding.UTF8.GetString(kvp.Key)] = Encoding.UTF8.GetString(kvp.Value);
//            }

//            return new BotKeystore()
//            {
//                Uin = keystore.Uin,
//                Uid = Encoding.UTF8.GetString(keystore.Uid),
//                Guid = keystore.Guid,
//                AndroidId = Encoding.UTF8.GetString(keystore.AndroidId),
//                Qimei = Encoding.UTF8.GetString(keystore.Qimei),
//                DeviceName = Encoding.UTF8.GetString(keystore.DeviceName),
//                WLoginSigs = new WLoginSigs()
//                {
//                    A2 = keystore.A2,
//                    A2Key = keystore.A2Key,
//                    D2 = keystore.D2,
//                    D2Key = keystore.D2Key,
//                    A1 = keystore.A1,
//                    A1Key = keystore.A1Key,
//                    NoPicSig = keystore.NoPicSig,
//                    TgtgtKey = keystore.TgtgtKey,
//                    Ksid = keystore.Ksid,
//                    SuperKey = keystore.SuperKey,
//                    StKey = keystore.StKey,
//                    StWeb = keystore.StWeb,
//                    St = keystore.St,
//                    WtSessionTicket = keystore.WtSessionTicket,
//                    WtSessionTicketKey = keystore.WtSessionTicketKey,
//                    RandomKey = keystore.RandomKey,
//                    SKey = keystore.SKey,
//                    PsKey = psKey
//                }
//            };
//        }

//        public static implicit operator BotKeystoreStruct(BotKeystore keystore)
//        {
//            var bytePsKey = new ByteArrayKVPNative[keystore.WLoginSigs.PsKey.Count];
//            int i = 0;
//            foreach (var kvp in keystore.WLoginSigs.PsKey)
//            {
//                bytePsKey[i++] = new ByteArrayKVPNative
//                {
//                    Key = Encoding.UTF8.GetBytes(kvp.Key), Value = Encoding.UTF8.GetBytes(kvp.Value)
//                };
//            }

//            return new BotKeystoreStruct()
//            {
//                Uin = keystore.Uin,
//                Uid = Encoding.UTF8.GetBytes(keystore.Uid),
//                Guid = keystore.Guid,
//                AndroidId = Encoding.UTF8.GetBytes(keystore.AndroidId),
//                Qimei = Encoding.UTF8.GetBytes(keystore.Qimei),
//                DeviceName = Encoding.UTF8.GetBytes(keystore.DeviceName),
//                A2 = keystore.WLoginSigs.A2,
//                A2Key = keystore.WLoginSigs.A2Key,
//                D2 = keystore.WLoginSigs.D2,
//                D2Key = keystore.WLoginSigs.D2Key,
//                A1 = keystore.WLoginSigs.A1,
//                A1Key = keystore.WLoginSigs.A1Key,
//                NoPicSig = keystore.WLoginSigs.NoPicSig,
//                TgtgtKey = keystore.WLoginSigs.TgtgtKey,
//                Ksid = keystore.WLoginSigs.Ksid,
//                SuperKey = keystore.WLoginSigs.SuperKey,
//                StKey = keystore.WLoginSigs.StKey,
//                StWeb = keystore.WLoginSigs.StWeb,
//                St = keystore.WLoginSigs.St,
//                WtSessionTicket = keystore.WLoginSigs.WtSessionTicket,
//                WtSessionTicketKey = keystore.WLoginSigs.WtSessionTicketKey,
//                RandomKey = keystore.WLoginSigs.RandomKey,
//                SKey = keystore.WLoginSigs.SKey,
//                PsKey = bytePsKey
//            };
//        }

//        public BotKeystore ToKeystoreWithoutFree()
//        {
//            var psKey = new Dictionary<string, string>();
//            foreach (var kvp in (ByteArrayKVPNative[])PsKey)
//            {
//                psKey[Encoding.UTF8.GetString(kvp.Key)] = Encoding.UTF8.GetString(kvp.Value);
//            }

//            return new BotKeystore()
//            {
//                Uin = Uin,
//                Uid = Encoding.UTF8.GetString(Uid.ToByteArrayWithoutFree()),
//                Guid = Guid.ToByteArrayWithoutFree(),
//                AndroidId = Encoding.UTF8.GetString(AndroidId.ToByteArrayWithoutFree()),
//                Qimei = Encoding.UTF8.GetString(Qimei.ToByteArrayWithoutFree()),
//                DeviceName = Encoding.UTF8.GetString(DeviceName.ToByteArrayWithoutFree()),
//                WLoginSigs = new WLoginSigs()
//                {
//                    A2 = A2.ToByteArrayWithoutFree(),
//                    A2Key = A2Key.ToByteArrayWithoutFree(),
//                    D2 = D2.ToByteArrayWithoutFree(),
//                    D2Key = D2Key.ToByteArrayWithoutFree(),
//                    A1 = A1.ToByteArrayWithoutFree(),
//                    A1Key = A1Key.ToByteArrayWithoutFree(),
//                    NoPicSig = NoPicSig.ToByteArrayWithoutFree(),
//                    TgtgtKey = TgtgtKey.ToByteArrayWithoutFree(),
//                    Ksid = Ksid.ToByteArrayWithoutFree(),
//                    SuperKey = SuperKey.ToByteArrayWithoutFree(),
//                    StKey = StKey.ToByteArrayWithoutFree(),
//                    StWeb = StWeb.ToByteArrayWithoutFree(),
//                    St = St.ToByteArrayWithoutFree(),
//                    WtSessionTicket = WtSessionTicket.ToByteArrayWithoutFree(),
//                    WtSessionTicketKey = WtSessionTicketKey.ToByteArrayWithoutFree(),
//                    RandomKey = RandomKey.ToByteArrayWithoutFree(),
//                    SKey = SKey.ToByteArrayWithoutFree(),
//                    PsKey = psKey
//                }
//            };
//        }
//    }
//}

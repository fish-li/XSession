using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace XSession.Modules
{
    internal static class SessionUtils
    {
        internal static bool CheckIdLength(string id, bool throwOnFail)
        {
            if( id.Length <= 80 )
                return true;

            if( throwOnFail )
                throw new HttpException(string.Format("SessionID too long: {0}", id));

            return false;
        }


        internal static SessionStateStoreData CreateLegitStoreData(HttpContext context, ISessionStateItemCollection sessionItems, HttpStaticObjectsCollection staticObjects, int timeout)
        {
            if( sessionItems == null ) {
                sessionItems = new SessionStateItemCollection();
            }
            if( staticObjects == null && context != null ) {
                staticObjects = SessionStateUtility.GetSessionStaticObjects(context);
            }
            return new SessionStateStoreData(sessionItems, staticObjects, timeout);
        }


        internal static void SerializeStoreData(SessionStateStoreData item, int initialStreamSize, out byte[] buf, out int length)
        {
            //using( MemoryStream memoryStream = new MemoryStream(initialStreamSize) ) {
            //    Serialize(item, memoryStream);
            //    buf = memoryStream.GetBuffer();
            //    length = (int)memoryStream.Length;
            //}

            bool compressionEnabled = true;

            using( MemoryStream memoryStream = new MemoryStream(initialStreamSize) ) {
                Serialize(item, memoryStream);
                if( compressionEnabled ) {
                    byte[] buffer = memoryStream.GetBuffer();
                    int count = (int)memoryStream.Length;
                    memoryStream.SetLength(0L);
                    using( DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, leaveOpen: true) ) {
                        deflateStream.Write(buffer, 0, count);
                    }
                    memoryStream.WriteByte(byte.MaxValue);
                }
                buf = memoryStream.GetBuffer();
                length = (int)memoryStream.Length;
            }
        }

        internal static void Serialize(SessionStateStoreData item, Stream stream)
        {
            bool flag = true;
            bool flag2 = true;
            BinaryWriter binaryWriter = new BinaryWriter(stream);
            binaryWriter.Write(item.Timeout);
            if( item.Items == null || item.Items.Count == 0 ) {
                flag = false;
            }
            binaryWriter.Write(flag);
            if( item.StaticObjects == null || item.StaticObjects.NeverAccessed ) {
                flag2 = false;
            }
            binaryWriter.Write(flag2);
            if( flag ) {
                ((SessionStateItemCollection)item.Items).Serialize(binaryWriter);
            }
            if( flag2 ) {
                item.StaticObjects.Serialize(binaryWriter);
            }
            binaryWriter.Write(byte.MaxValue);
        }


        internal static SessionStateStoreData DeserializeStoreData(HttpContext context, Stream stream)
        {
            bool compressionEnabled = true;

            if( compressionEnabled ) {
                using( DeflateStream stream2 = new DeflateStream(stream, CompressionMode.Decompress, leaveOpen: true) ) {
                    return Deserialize(context, stream2);
                }
            }
            return Deserialize(context, stream);
        }


        internal static SessionStateStoreData Deserialize(HttpContext context, Stream stream)
        {
            int timeout;
            SessionStateItemCollection sessionItems;
            HttpStaticObjectsCollection staticObjects;

            BinaryReader binaryReader = new BinaryReader(stream);
            timeout = binaryReader.ReadInt32();
            bool flag = binaryReader.ReadBoolean();
            bool flag2 = binaryReader.ReadBoolean();
            sessionItems = ((!flag) ? new SessionStateItemCollection() : SessionStateItemCollection.Deserialize(binaryReader));
            staticObjects = ((!flag2) ? SessionStateUtility.GetSessionStaticObjects(context) : HttpStaticObjectsCollection.Deserialize(binaryReader));
            byte b = binaryReader.ReadByte();
            if( b != byte.MaxValue ) {
                throw new HttpException("Invalid_session_state");
            }

            return new SessionStateStoreData(sessionItems, staticObjects, timeout);
        }


    }
}

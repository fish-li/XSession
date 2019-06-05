using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace XSession.Modules
{
    internal static class SessionUtils
    {
        public static SessionStateStoreData CreateLegitStoreData(HttpContext context, ISessionStateItemCollection sessionItems, HttpStaticObjectsCollection staticObjects, int timeout)
        {
            if( sessionItems == null ) {
                sessionItems = new SessionStateItemCollection();
            }
            if( staticObjects == null && context != null ) {
                staticObjects = SessionStateUtility.GetSessionStaticObjects(context);
            }
            return new SessionStateStoreData(sessionItems, staticObjects, timeout);
        }


        public static byte[] SerializeStoreData(SessionStateStoreData item, int initialStreamSize)
        {
            using( MemoryStream memoryStream = new MemoryStream(initialStreamSize) ) {
                Serialize(item, memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static void Serialize(SessionStateStoreData item, Stream stream)
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


        public static SessionStateStoreData DeserializeStoreData(HttpContext context, Stream stream)
        {
            return Deserialize(context, stream);
        }


        public static SessionStateStoreData Deserialize(HttpContext context, Stream stream)
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

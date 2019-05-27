using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace XSession.Modules
{
    public sealed class FileSessionStateStore : SessionStateStoreProviderBase
    {
        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            return SessionUtils.CreateLegitStoreData(context, null, null, timeout);
        }


        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            SessionUtils.CheckIdLength(id, true);

            SessionStateStoreData data = CreateNewStoreData(context, timeout);

            SessionUtils.SerializeStoreData(data, 7000, out byte[] bytes, out int length);

            FileHelper.SaveToFile(bytes, context, id);
        }


        public override void Dispose()
        {
        }

        private SessionStateStoreData DoGet(HttpContext context, string id, bool exclusive, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
        {
            locked = false;
            lockId = null;
            lockAge = TimeSpan.Zero;
            actionFlags = SessionStateActions.None;

            byte[] bytes = FileHelper.ReadFile(context, id);
            if( bytes == null || bytes.Length == 0 )
                return null;

            using( MemoryStream memoryStream = new MemoryStream(bytes) ) {

                try {
                    SessionStateStoreData data = SessionUtils.DeserializeStoreData(context, memoryStream);
                    return SessionUtils.CreateLegitStoreData(context, data.Items, data.StaticObjects, data.Timeout);
                }
                catch( EndOfStreamException ex1 ) {
                    throw new EndOfStreamException("无效的会话数据，当前会话文件：" + FileHelper.GetSessionFilePath(context, id), ex1);
                }
                catch( InvalidDataException ex2 ) {
                    throw new InvalidDataException("无效的会话数据，当前会话文件：" + FileHelper.GetSessionFilePath(context, id), ex2);
                }
            }
        }

        public override void EndRequest(HttpContext context)
        {
        }

        public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
        {
            SessionUtils.CheckIdLength(id, true);
            return this.DoGet(context, id, false, out locked, out lockAge, out lockId, out actionFlags);
        }

        public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
        {
            SessionUtils.CheckIdLength(id, true);
            return this.DoGet(context, id, true, out locked, out lockAge, out lockId, out actionFlags);
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            if( string.IsNullOrEmpty(name) )
                name = "File Session State Provider";

            FileHelper.Init();

            base.Initialize(name, config);
        }

        public override void InitializeRequest(HttpContext context)
        {
        }


        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {
        }

        public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            SessionUtils.CheckIdLength(id, true);
            FileHelper.DeleteFile(context, id);
        }

        public override void ResetItemTimeout(HttpContext context, string id)
        {
        }

        public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            SessionUtils.CheckIdLength(id, true);

            SessionUtils.SerializeStoreData(item, 7000, out byte[] bytes, out int length);

            FileHelper.SaveToFile(bytes, context, id);
        }

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            return true;
        }



        


    }
}

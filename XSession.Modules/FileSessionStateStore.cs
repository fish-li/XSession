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
    /// <summary>
    /// 采用文件存储Session数据的StoreProvider
    /// </summary>
    public sealed class FileSessionStateStore : SessionStateStoreProviderBase
    {
        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            return SessionUtils.CreateLegitStoreData(context, null, null, timeout);
        }


        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            CreateUninitializedItem2(context, id, timeout);
        }

        internal SessionStateStoreData CreateUninitializedItem2(HttpContext context, string id, int timeout)
        {
            SessionStateStoreData data = CreateNewStoreData(context, timeout);

            // 将Session数据写入文件
            byte[] bytes = SessionUtils.SerializeStoreData(data, 7000);

            FileStore.SaveToFile(bytes, id);

            return data;
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
            return DoGet(context, id);
        }
        

        internal SessionStateStoreData DoGet(HttpContext context, string id)
        {
            byte[] bytes = FileStore.ReadFile(id);
            if( bytes == null || bytes.Length == 0 )
                return null;


            return LoadSessionState(context, id, bytes);
        }


        private SessionStateStoreData LoadSessionState(HttpContext context, string id, byte[] bytes)
        {
            using( MemoryStream memoryStream = new MemoryStream(bytes) ) {

                try {
                    SessionStateStoreData data = SessionUtils.DeserializeStoreData(context, memoryStream);
                    return SessionUtils.CreateLegitStoreData(context, data.Items, data.StaticObjects, data.Timeout);
                }
                catch( EndOfStreamException ex1 ) {
                    throw new EndOfStreamException("无效的会话数据，当前会话文件：" + FileStore.GetSessionFilePath(id), ex1);
                }
                catch( InvalidDataException ex2 ) {
                    throw new InvalidDataException("无效的会话数据，当前会话文件：" + FileStore.GetSessionFilePath(id), ex2);
                }
            }
        }

        public override void EndRequest(HttpContext context)
        {
        }

        public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
        {
            return this.DoGet(context, id, false, out locked, out lockAge, out lockId, out actionFlags);
        }

        public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
        {
            return this.DoGet(context, id, true, out locked, out lockAge, out lockId, out actionFlags);
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            if( string.IsNullOrEmpty(name) )
                name = "File Session State Provider";

            Initializer.Init();

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
            this.DeleteFile(id);
        }

        internal void DeleteFile(string id)
        {
            FileStore.DeleteFile(id);
        }

        public override void ResetItemTimeout(HttpContext context, string id)
        {
        }

        public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            byte[] bytes = SessionUtils.SerializeStoreData(item, 7000);

            FileStore.SaveToFile(bytes, id);
        }

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            // 这里不主动触发 【Session过期】 回调。
            return true;
        }



        


    }
}

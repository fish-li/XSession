using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;

namespace XSession.Modules
{
    internal class CacheStore
    {
        public static readonly string KeyPrefix = "FastSession_";

        private CacheItemRemovedCallback _callback;
  
        private string GetSessionStateCacheKey(string sessionId)
        {
            return KeyPrefix + sessionId;
        }

        private void InsertCache(string key, InProcSessionState state)
        {
            HttpRuntime.Cache.Insert(key, state, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, state.Timeout, 30), CacheItemPriority.NotRemovable, this._callback);
        }

        public void InsertCache(string id, SessionStateStoreData data)
        {
            InProcSessionState state = new InProcSessionState(data);
            string key = this.GetSessionStateCacheKey(id);
            InsertCache(key, state);
        }

        public void CreateUninitializedItem(string id, int timeout, SessionStateStoreData data)
        {
            string key = this.GetSessionStateCacheKey(id);
            InProcSessionState state = new InProcSessionState(data);
            InsertCache(key, state);
        }


        public SessionStateStoreData DoGet(HttpContext context, string id)
        {
            string key = this.GetSessionStateCacheKey(id);
            InProcSessionState state = HttpRuntime.Cache.Get(key) as InProcSessionState;

            if( state != null )
                return state.ToStoreData(context);
            else
                return null;
        }


        public void Initialize(CacheItemRemovedCallback callback)
        {
            this._callback = callback;
        }

        public void RemoveItem(string id)
        {
            string key = this.GetSessionStateCacheKey(id);
            HttpRuntime.Cache.Remove(key);
        }


        public void SetAndReleaseItemExclusive(string id, SessionStateStoreData item)
        {
            string key = this.GetSessionStateCacheKey(id);
            InProcSessionState state = HttpRuntime.Cache.Get(key) as InProcSessionState;

            if( state == null ) {
                state = new InProcSessionState(item);
                InsertCache(key, state);
            }
            else {
                // 如果缓存中的对象存在，就只做更新
                state.Items = item.Items;
                state.StaticObjects = item.StaticObjects;
                state.Timeout = item.Timeout;
            }
        }


    }
}

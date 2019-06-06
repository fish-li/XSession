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

        private void InsertCacheByKey(string key, SessionStateStoreData state)
        {
            HttpRuntime.Cache.Insert(key, state, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, state.Timeout, 30), CacheItemPriority.NotRemovable, this._callback);
        }

        public void InsertCacheById(string id, SessionStateStoreData data)
        {
            string key = this.GetSessionStateCacheKey(id);
            InsertCacheByKey(key, data);
        }

        public void CreateUninitializedItem(string id, int timeout, SessionStateStoreData data)
        {
            string key = this.GetSessionStateCacheKey(id);
            InsertCacheByKey(key, data);
        }


        public SessionStateStoreData DoGet(HttpContext context, string id)
        {
            string key = this.GetSessionStateCacheKey(id);
            SessionStateStoreData state = HttpRuntime.Cache.Get(key) as SessionStateStoreData;

            return state;
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
            SessionStateStoreData state = HttpRuntime.Cache.Get(key) as SessionStateStoreData;

            if( state == null ) {
                InsertCacheByKey(key, item);
            }

            // 说明：并不需要【更新缓存对象】，因为引用的对象一直没有改变过！
        }


    }
}

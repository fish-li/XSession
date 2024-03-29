﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;

namespace XSession.Modules
{
    /// <summary>
    /// 采用文件和Cache存储Session数据的StoreProvider
    /// 主要优化点在于：
    /// 1、为了防止Session数据丢失，会将Session数据写入文件（参考PHP之类的做法）
    /// 2、为了提升Session数据的加载性能，Session数据在写入文件时，同时存放一份在Cache中，加载时优先从Cache中获取
    /// 3、为了避免32位内存不够用时导致OOM，Cache功能仅当64位时有效
    /// 
    /// 注意：
    /// 这个版本当遇到 ReportViewer 控件时，会在Session保存时遇到阻塞，所以不建议使用！
    /// </summary>
    public sealed partial class FastSessionStateStore : SessionStateStoreProviderBase
    {
        private SessionStateItemExpireCallback _expireCallback;

        private readonly FileSessionStateStore _fileStore = new FileSessionStateStore();
        private readonly CacheStore _cacheStore = new CacheStore();
        


        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            return _fileStore.CreateNewStoreData(context, timeout);
        }
        

        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            // 将Session数据写入文件
            SessionStateStoreData data = _fileStore.CreateUninitializedItem2(context, id, timeout);


            // 将Session放入缓存
            if( Initializer.Is64Bit ) {                
                _cacheStore.CreateUninitializedItem(id, timeout, data);
            }
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

            SessionStateStoreData data = null;

            // 优先从缓存中读取Session数据
            if( Initializer.Is64Bit ) {

                // TODO: 这里可能会有一个缺陷！
                // 假设场景：一个用户有多个并发的请求在服务端同时执行，此时获取到的是同一个Session引用对象，可能会有并发读写问题！
                // 要解决这个问题，需要做对象的克隆，会浪费一些性能，
                // 然则考虑到现实情况，人工点击的操作频率，是不大可能出现线程并发问题的，所以这里暂且不处理。

                data = _cacheStore.DoGet(context, id);
                if( data != null ) {

                    // 即使缓存中存在Session数据，也要更新Session数据文件的【最后访问时间】，供后面文件加载时判断是否过期
                    FileStore.SetLastAccessTime(id, DateTime.Now);

                    return data;
                }
            }


            // 缓存中如果不存在，有可能是AP.NET进程重启了，此时要从文件中读取Session数据
            // 读到结果后，再存入缓存，供后续请求使用
            data = _fileStore.DoGet(context, id, true);

            if( data != null ) 
                _cacheStore.InsertCacheById(id, data);
            
            return data;
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
                name = "Fast Session State Provider";

            Initializer.Init();

            base.Initialize(name, config);


            if( Initializer.Is64Bit ) {
                _cacheStore.Initialize(new CacheItemRemovedCallback(this.OnCacheItemRemoved));
            }
        }

        public override void InitializeRequest(HttpContext context)
        {
        }

        /// <summary>
        /// 缓存过期导致的清理
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="reason"></param>
        public void OnCacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            if( reason == CacheItemRemovedReason.Expired ) {

                // 删除Session数据文件
                string id = key.Substring(CacheStore.KeyPrefix.Length);
                _fileStore.DeleteFile(id);


                if( this._expireCallback != null ) {
                    SessionStateStoreData state = (SessionStateStoreData)value;
                    this._expireCallback(id, SessionUtils.CreateLegitStoreData(null, state.Items, state.StaticObjects, state.Timeout));
                }
            }
        }

        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {
        }

        public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            if( Initializer.Is64Bit ) {
                _cacheStore.RemoveItem(id);
            }

            _fileStore.DeleteFile(id);
        }

        public override void ResetItemTimeout(HttpContext context, string id)
        {
        }

        public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            // 先更新缓存数据，尽量让并发的请求拿到最新的数据
            if( Initializer.Is64Bit ) {
                _cacheStore.SetAndReleaseItemExclusive(id, item);

                // 及时清除修改标记
                item.Items.Dirty = false;
            }

            // 再更新文件
            _fileStore.SetAndReleaseItemExclusive(context, id, item, lockId, newItem);
        }

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            if( Initializer.Is64Bit ) {
                this._expireCallback = expireCallback;
            }

            return true;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XSession.Modules
{
    /// <summary>
    /// 实现一个简单的用户关联锁。
    /// </summary>
    public sealed class UserLock
    {
        private readonly object _lock = new object();
        private Hashtable _hashtable = new Hashtable(10240);


        public static readonly UserLock Instance = new UserLock();

        /// <summary>
        /// 供内部使用的实例
        /// </summary>
        internal static readonly UserLock XInstance = new UserLock();


        private UserLock() { }


        /// <summary>
        /// 根据SessionId获取用户的独占锁。
        /// 获取到锁对象后，配合 lock( .... ) 来使用
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public object GetLock(string sessionId)
        {
            if( string.IsNullOrEmpty(sessionId) )
                throw new ArgumentNullException(nameof(sessionId));


            object value = _hashtable[sessionId];

            if( value == null ) {

                // 只允许一个线程写入
                lock( _lock ) {

                    // 检查【前面】是否有线程已经插入
                    value = _hashtable[sessionId];

                    // 确实没有插入，这里执行插入
                    if( value == null ) {
                        value = new object();
                        _hashtable[sessionId] = value;
                    }
                }
            }

            return value;
        }


        /// <summary>
        /// 删除锁对象，仅当用户Session删除时调用。
        /// </summary>
        /// <param name="sessionId"></param>
        private void RemoveLock(string sessionId)
        {
            lock( _lock ) {
                _hashtable.Remove(sessionId);
            }
        }


        internal static void Remove(string sessionId)
        {
            if( string.IsNullOrEmpty(sessionId) )
                return;

            XInstance.RemoveLock(sessionId);
            Instance.RemoveLock(sessionId);
        }
    }
}

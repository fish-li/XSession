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
    internal sealed class SidLock
    {
        private readonly object _lock = new object();
        private Hashtable _hashtable = new Hashtable(10240);

        /// <summary>
        /// 供内部使用的实例
        /// </summary>
        internal static readonly SidLock Instance = new SidLock();


        private SidLock() { }


        /// <summary>
        /// 根据 Id 获取用户的独占锁。
        /// 获取到锁对象后，配合 lock( .... ) 来使用
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        public object GetLock(string sid)
        {
            if( string.IsNullOrEmpty(sid) )
                throw new ArgumentNullException(nameof(sid));


            object value = _hashtable[sid];

            if( value == null ) {

                // 只允许一个线程写入
                lock( _lock ) {

                    // 检查【前面】是否有线程已经插入
                    value = _hashtable[sid];

                    // 确实没有插入，这里执行插入
                    if( value == null ) {
                        value = new object();
                        _hashtable[sid] = value;
                    }
                }
            }

            return value;
        }


        /// <summary>
        /// 删除锁对象
        /// </summary>
        /// <param name="sid"></param>
        public void RemoveLock(string sid)
        {
            lock( _lock ) {
                _hashtable.Remove(sid);
            }
        }


    }
}

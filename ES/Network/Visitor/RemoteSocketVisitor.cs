﻿using ES.Network.Sockets;
using System;
using System.Collections.Generic;

namespace ES.Network.Visitor
{
    /// <summary>
    /// 远程socket访问器
    /// 异步通信下服务端和客户端均可使用此方案解决，同步无法使用
    /// </summary>
    public class RemoteSocketVisitor : RemoteSocketInvoke
    {
        /// <summary>
        /// 回调委托
        /// </summary>
        public delegate void ReceivedCompleted(RemoteSocketMsg msg);
        /// <summary>
        /// 回调委托列表
        /// </summary>
        private List<KeyValuePair<string, ReceivedCompleted>> commandList = null;
        /// <summary>
        /// 异常回调函数地址
        /// </summary>
        private readonly IRemoteSocketVisitorException catchReceivedException = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public RemoteSocketVisitor(IRemoteSocketVisitorException catchReceivedException)
        {
            commandList = new List<KeyValuePair<string, ReceivedCompleted>>();
            this.catchReceivedException = catchReceivedException;
        }

        /// <summary>
        /// 添加访问函数
        /// </summary>
        /// <param name="main">主指令</param>
        /// <param name="second">副指令</param>
        /// <param name="callback">访问函数</param>
        public void Add(byte main, byte second, ReceivedCompleted callback)
        {
            lock (commandList)
            {
                KeyValuePair<string, ReceivedCompleted> pair = new KeyValuePair<string, ReceivedCompleted>(string.Format("{0}-{1}", main, second), callback);
                commandList.Add(pair);
            }
        }

        /// <summary>
        /// 接受完成回调
        /// </summary>
        /// <param name="msg">数据信息</param>
        void RemoteSocketInvoke.ReceivedCompleted(RemoteSocketMsg msg)
        {
            ReceivedCompleted rc = null;
            string command = string.Format("{0}-{1}", msg.main, msg.second);
            lock (commandList)
            {
                foreach (var item in commandList)
                {
                    if (item.Key == command)
                    {
                        rc = item.Value;
                        break;
                    }
                }
            }
            try
            {
                rc?.Invoke(msg);
            }
            catch (Exception ex)
            {
                if (catchReceivedException != null) catchReceivedException.CatchReceivedException(msg, ex);
                else throw ex;
            }
        }

        /// <summary>
        /// 套接字异常
        /// </summary>
        /// <param name="exception"></param>
        public void SocketException(Exception exception)
        {
            if (catchReceivedException != null) catchReceivedException.CatchReceivedException(null, exception);
            else throw exception;
        }
    }
}

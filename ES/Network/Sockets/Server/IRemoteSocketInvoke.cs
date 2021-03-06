﻿using System;

namespace ES.Network.Sockets.Server
{
    /// <summary>
    /// 远程套接字委托接口回调
    /// </summary>
    public interface IRemoteSocketInvoke
    {
        /// <summary>
        /// 完成接受回调
        /// </summary>
        /// <param name="msg">数据信息体</param>
        void ReceivedCompleted(RemoteSocketMsg msg);

        /// <summary>
        /// 套接字异常捕获
        /// </summary>
        /// <param name="exception">异常对象</param>
        void OnSocketException(Exception exception);
    }
}

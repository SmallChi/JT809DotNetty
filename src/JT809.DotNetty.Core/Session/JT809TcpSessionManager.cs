using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DotNetty.Transport.Channels;
using JT809.DotNetty.Abstractions;
using JT809.DotNetty.Core.Metadata;

namespace JT809.DotNetty.Core
{
    /// <summary>
    /// JT809 Tcp会话管理
    /// </summary>
    public class JT809TcpSessionManager
    {
        private readonly ILogger<JT809TcpSessionManager> logger;

        private readonly IJT809SessionPublishing jT809SessionPublishing;

        public JT809TcpSessionManager(
            IJT809SessionPublishing jT809SessionPublishing,
            ILoggerFactory loggerFactory)
        {
            this.jT809SessionPublishing = jT809SessionPublishing;
            logger = loggerFactory.CreateLogger<JT809TcpSessionManager>();
        }

        private ConcurrentDictionary<uint, JT809TcpSession> SessionIdDict = new ConcurrentDictionary<uint, JT809TcpSession>();

        public int SessionCount
        {
            get
            {
                return SessionIdDict.Count;
            }
        }

        public JT809TcpSession GetSession(uint msgGNSSCENTERID)
        {
            if (SessionIdDict.TryGetValue(msgGNSSCENTERID, out JT809TcpSession targetSession))
            {
                return targetSession;
            }
            else
            {
                return default;
            }
        }

        public void Heartbeat(uint msgGNSSCENTERID)
        {
            if (SessionIdDict.TryGetValue(msgGNSSCENTERID, out JT809TcpSession oldjT808Session))
            {
                oldjT808Session.LastActiveTime = DateTime.Now;
                SessionIdDict.TryUpdate(msgGNSSCENTERID, oldjT808Session, oldjT808Session);
            }
        }

        public void TryAdd(JT809TcpSession appSession)
        {
            if (SessionIdDict.TryAdd(appSession.MsgGNSSCENTERID, appSession))
            {
                jT809SessionPublishing.PublishAsync(JT809Constants.SessionOnline, appSession.MsgGNSSCENTERID.ToString());
            }
        }

        public JT809TcpSession RemoveSession(uint msgGNSSCENTERID)
        {
            //可以使用任意mq的发布订阅
            if (!SessionIdDict.TryGetValue(msgGNSSCENTERID, out JT809TcpSession jT808Session))
            {
                return default;
            }
            if (SessionIdDict.TryRemove(msgGNSSCENTERID, out JT809TcpSession jT808SessionRemove))
            {
                logger.LogInformation($">>>{msgGNSSCENTERID} Session Remove.");
                jT809SessionPublishing.PublishAsync(JT809Constants.SessionOffline, msgGNSSCENTERID.ToString());
                return jT808SessionRemove;
            }
            else
            {
                return default;
            }   
        }

        public void RemoveSessionByChannel(IChannel channel)
        {
            var keys = SessionIdDict.Where(w => w.Value.Channel.Id == channel.Id).Select(s => s.Key).ToList();
            if (keys.Count > 0)
            {
                foreach (var key in keys)
                {
                    SessionIdDict.TryRemove(key, out JT809TcpSession jT808SessionRemove);
                }
                string nos = string.Join(",", keys);
                logger.LogInformation($">>>{nos} Channel Remove.");
                jT809SessionPublishing.PublishAsync(JT809Constants.SessionOffline, nos);
            }      
        }

        public IEnumerable<JT809TcpSession> GetAll()
        {
            return SessionIdDict.Select(s => s.Value).ToList();
        }
    }
}


using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DotNetty.Transport.Channels;
using JT809.DotNetty.Core.Metadata;

namespace JT809.DotNetty.Core.Session
{
    /// <summary>
    /// 上级平台
    /// JT809主链路会话管理
    /// </summary>
    public class JT809SuperiorMainSessionManager
    {
        private readonly ILogger<JT809SuperiorMainSessionManager> logger;

        //private readonly IJT809SessionPublishing jT809SessionPublishing;

        public JT809SuperiorMainSessionManager(
            //IJT809SessionPublishing jT809SessionPublishing,
            ILoggerFactory loggerFactory)
        {
            //this.jT809SessionPublishing = jT809SessionPublishing;
            logger = loggerFactory.CreateLogger<JT809SuperiorMainSessionManager>();
        }

        private ConcurrentDictionary<uint, JT809Session> SessionIdDict = new ConcurrentDictionary<uint, JT809Session>();

        public int SessionCount
        {
            get
            {
                return SessionIdDict.Count;
            }
        }

        public JT809Session GetSession(uint msgGNSSCENTERID)
        {
            if (SessionIdDict.TryGetValue(msgGNSSCENTERID, out JT809Session targetSession))
            {
                return targetSession;
            }
            else
            {
                return default;
            }
        }

        public void TryAdd(IChannel channel, uint msgGNSSCENTERID)
        {
            if(SessionIdDict.TryGetValue(msgGNSSCENTERID,out JT809Session jT809Session))
            {
                jT809Session.LastActiveTime = DateTime.Now;
                jT809Session.Channel = channel;
                SessionIdDict.TryUpdate(msgGNSSCENTERID, jT809Session, jT809Session);
            }
            else
            {
                SessionIdDict.TryAdd(msgGNSSCENTERID, new JT809Session(channel, msgGNSSCENTERID));
                //jT809SessionPublishing.PublishAsync(JT809Constants.SessionOnline, msgGNSSCENTERID.ToString());
            }
        }

        public JT809Session RemoveSession(uint msgGNSSCENTERID)
        {
            //可以使用任意mq的发布订阅
            if (!SessionIdDict.TryGetValue(msgGNSSCENTERID, out JT809Session jT808Session))
            {
                return default;
            }
            if (SessionIdDict.TryRemove(msgGNSSCENTERID, out JT809Session jT808SessionRemove))
            {
                logger.LogInformation($">>>{msgGNSSCENTERID} Session Remove.");
                //jT809SessionPublishing.PublishAsync(JT809Constants.SessionOffline, msgGNSSCENTERID.ToString());
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
                    SessionIdDict.TryRemove(key, out JT809Session jT808SessionRemove);
                }
                string nos = string.Join(",", keys);
                logger.LogInformation($">>>{nos} Channel Remove.");
                //jT809SessionPublishing.PublishAsync(JT809Constants.SessionOffline, nos);
            }      
        }

        public IEnumerable<JT809Session> GetAll()
        {
            return SessionIdDict.Select(s => s.Value).ToList();
        }
    }
}


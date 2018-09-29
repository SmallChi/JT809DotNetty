using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JT809Netty.Core
{
    public class SessionManager:IDisposable
    {
        private readonly ILogger<SessionManager> logger;

        private readonly CancellationTokenSource cancellationTokenSource;

#if DEBUG
        private const int timeout = 1 * 1000 * 60;
#else
        private const int timeout = 5 * 1000 * 60;
#endif
        public SessionManager(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<SessionManager>();
            cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() =>
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    logger.LogInformation($"Online Count>>>{SessionCount}");
                    if (SessionCount > 0)
                    {
                        logger.LogInformation($"SessionIds>>>{string.Join(",", SessionIdDict.Select(s => s.Key))}");
                        logger.LogInformation($"TerminalPhoneNos>>>{string.Join(",", CustomKey_SessionId_Dict.Select(s => $"{s.Key}-{s.Value}"))}");
                    }
                    Thread.Sleep(timeout);
                }
            }, cancellationTokenSource.Token);
        }

        /// <summary>
        /// Netty生成的sessionID和Session的对应关系
        /// key = seession id
        /// value = Session
        /// </summary>
        private ConcurrentDictionary<string, JT809Session> SessionIdDict = new ConcurrentDictionary<string, JT809Session>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// 自定义Key和netty生成的sessionID的对应关系
        /// key = 终端手机号
        /// value = seession id
        /// </summary>
        private ConcurrentDictionary<string, string> CustomKey_SessionId_Dict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public int SessionCount
        {
            get
            {
                return SessionIdDict.Count;
            }
        }

        public void RegisterSession(JT809Session appSession)
        {
            if (CustomKey_SessionId_Dict.ContainsKey(appSession.Key))
            {
                return;
            }
            if (SessionIdDict.TryAdd(appSession.SessionID, appSession) &&
                CustomKey_SessionId_Dict.TryAdd(appSession.Key, appSession.SessionID))
            {
                return;
            }
        }

        public JT809Session GetSessionByID(string sessionID)
        {
            if (string.IsNullOrEmpty(sessionID))
                return default;
            JT809Session targetSession;
            SessionIdDict.TryGetValue(sessionID, out targetSession);
            return targetSession;
        }

        public JT809Session GetSessionByTerminalPhoneNo(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                    return default;
                if (CustomKey_SessionId_Dict.TryGetValue(key, out string sessionId))
                {
                    if (SessionIdDict.TryGetValue(sessionId, out JT809Session targetSession))
                    {
                        return targetSession;
                    }
                    else
                    {
                        return default;
                    }
                }
                else
                {
                    return default;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, key);
                return default;
            }
        }

        public void Heartbeat(string key)
        {
            try
            {
                if(CustomKey_SessionId_Dict.TryGetValue(key, out string sessionId))
                {
                    if (SessionIdDict.TryGetValue(sessionId, out JT809Session oldjT808Session))
                    {
                        if (oldjT808Session.Channel.Active)
                        {
                            oldjT808Session.LastActiveTime = DateTime.Now;
                            if (SessionIdDict.TryUpdate(sessionId, oldjT808Session, oldjT808Session))
                            {

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, key);
            }
        }

        /// <summary>
        /// 通过通道Id和自定义key进行关联
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="key"></param>
        public void UpdateSessionByID(string sessionID, string key)
        {
            try
            {
                if (SessionIdDict.TryGetValue(sessionID, out JT809Session oldjT808Session))
                {
                    oldjT808Session.Key = key;
                    if (SessionIdDict.TryUpdate(sessionID, oldjT808Session, oldjT808Session))
                    {
                        CustomKey_SessionId_Dict.AddOrUpdate(key, sessionID, (tpn, sid) =>
                        {
                            return sessionID;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{sessionID},{key}");
            }
        }

        public void RemoveSessionByID(string sessionID)
        {
            if (sessionID == null) return;
            try
            {
                if (SessionIdDict.TryRemove(sessionID, out JT809Session session))
                {
                    if (session.Key != null)
                    {
                        if(CustomKey_SessionId_Dict.TryRemove(session.Key, out string sessionid))
                        {
                            logger.LogInformation($">>>{sessionID}-{session.Key} Session Remove.");
                        }
                    }
                    else
                    {
                        logger.LogInformation($">>>{sessionID} Session Remove.");
                    }
                    session.Channel.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $">>>{sessionID} Session Remove Exception");
            }
        }

        public void RemoveSessionByKey(string key)
        {
            if (key == null) return;
            try
            {
                if (CustomKey_SessionId_Dict.TryRemove(key, out string sessionid))
                {
                    if (SessionIdDict.TryRemove(sessionid, out JT809Session session))
                    {
                        logger.LogInformation($">>>{key}-{sessionid} Key Remove.");
                    }
                    else
                    {
                        logger.LogInformation($">>>{key} Key Remove.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $">>>{key} Key Remove Exception.");
            }
        }

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
        }
    }
}

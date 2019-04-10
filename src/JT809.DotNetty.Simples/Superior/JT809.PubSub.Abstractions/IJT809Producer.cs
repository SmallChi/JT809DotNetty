using System;
using System.Threading.Tasks;

namespace JT809.PubSub.Abstractions
{
    public interface IJT809Producer: IJT809PubSub, IJT809ProducerOfT<byte[]>
    {
        
    }

    public interface IJT809ProducerOfT<T>: IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msgId">消息Id</param>
        /// <param name="vno_color">车牌号+车牌颜色</param>
        /// <param name="data">hex data</param>
        void ProduceAsync(string msgId, string vno_color, T data);
    }
}

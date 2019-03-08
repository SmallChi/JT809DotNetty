using JT809.DotNetty.Abstractions;
using JT809.DotNetty.Abstractions.Dtos;
using JT809.DotNetty.Core.Handlers;
using JT809.DotNetty.Core.Metadata;
using JT809.DotNetty.Core.Services;
using Newtonsoft.Json;

namespace JT809.DotNetty.Core.Internal
{
    /// <summary>
    /// 默认消息处理业务实现
    /// </summary>
    public class JT809SuperiorWebAPIDefaultHandler : JT809SuperiorWebAPIHandlerBase
    {
        private readonly JT809SimpleSystemCollectService jT809SimpleSystemCollectService;

        public JT809SuperiorWebAPIDefaultHandler(JT809SimpleSystemCollectService jT809SimpleSystemCollectService)
        {
            this.jT809SimpleSystemCollectService = jT809SimpleSystemCollectService;
            InitRoute();
        }

        /// <summary>
        /// 统一下发信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public JT809HttpResponse UnificationSend(JT809HttpRequest request)
        {
            if (string.IsNullOrEmpty(request.Json))
            {
                return EmptyHttpResponse();
            }
            //JT809UnificationSendRequestDto jT808UnificationSendRequestDto = JsonConvert.DeserializeObject<JT809UnificationSendRequestDto>(request.Json);
            //var result = jT808UnificationTcpSendService.Send(jT808UnificationSendRequestDto.TerminalPhoneNo, jT808UnificationSendRequestDto.Data);
            //return CreateJT808HttpResponse(result);
            return null;
        }

        /// <summary>
        /// 获取当前系统进程使用率
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public JT809HttpResponse SystemCollectGet(JT809HttpRequest request)
        {
            JT809ResultDto<JT809SystemCollectInfoDto> jT809ResultDto = new JT809ResultDto<JT809SystemCollectInfoDto>();
            jT809ResultDto.Data = jT809SimpleSystemCollectService.Get();
            return CreateJT809HttpResponse(jT809ResultDto);
        }

        protected virtual void InitRoute()
        {
            CreateRoute(JT809Constants.JT809SuperiorWebApiRouteTable.SystemCollectGet, SystemCollectGet);
        }
    }
}

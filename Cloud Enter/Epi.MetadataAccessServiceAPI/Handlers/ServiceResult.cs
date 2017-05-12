
using Epi.Cloud.MetadataServices.Common.DataTypes;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Epi.MetadataAccessService.Handlers
{
    public class ServiceResult<T> : IHttpActionResult
    {
        HttpStatusCode _statusCode;
        ApiController _controller;
        T _data;


        public ServiceResult(HttpStatusCode statusCode, T data, ApiController controller)
        {
            _statusCode = statusCode;
            _data = data;
            _controller = controller;
        }


        public ServiceResult(T data, ApiController controller)
        {
            _statusCode = HttpStatusCode.OK;
            _data = data;
            _controller = controller;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        public HttpResponseMessage Execute()
        {
            HttpResponseMessage message = null;

            var errorInfo = GetErrorInfo();
            if (errorInfo != null)
            {
                _statusCode = HttpStatusCode.BadRequest;
                message = _controller.Request.CreateResponse(_statusCode, errorInfo);
                //message.ReasonPhrase = CommonResource.CMN_ERR_INVALID_REQ;
            }
            else
            {
                message = _controller.Request.CreateResponse(_statusCode, _data);
            }
            return message;
        }

        public CDTResponse GetErrorInfo()
        {
            var errorResponse = _data as CDTBase;
            CDTResponse error = null;
            if (errorResponse != null)
            {
                error = errorResponse.Response;
            }
            else
            {
                error = _data as CDTResponse;
            }
            //if(error!=null && error.Type != DataType.Constants.ResponseType.Success)
            //{
            //    return error;
            //}
            return null;
        }

    }
}
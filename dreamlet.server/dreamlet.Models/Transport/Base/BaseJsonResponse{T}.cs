namespace dreamlet.Models.Transport.Base
{
	public static class BaseJsonResponse
	{
		public static BaseJsonResponse<TResponse> Create<TResponse>(TResponse data, bool? success = null, string message = null)
		{
			var resp = new BaseJsonResponse<TResponse>(success, message);
			resp.Data = data == null ? default(TResponse) : data;

			return resp;
		}
	}

	public class BaseJsonResponse<TResponse>
	{
		public BaseJsonResponse(bool? success = null, string message = null)
		{
			Success = success;
			Message = message;
		}

		public string Message { get; set; }
		public bool? Success { get; set; }
		public TResponse Data { get; set; }
	}
}
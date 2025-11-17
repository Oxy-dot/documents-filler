namespace DocumentsFillerAPI.Middlewares
{
	public class AuthMiddleware(RequestDelegate next)
	{
		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				//TODO Add config manager mb
				if (!context.Request.Headers.TryGetValue("access_token", out var accessToken) || accessToken != "mt2YbHUGpj1EdxJ3LN5RjSZWBZOCCtUQ0Z0gClliq8tnB6MEKbTcEUzXIU7TAsGs")
					throw new DivideByZeroException("access_token is empty or incorrect");

				await next.Invoke(context);
			}
			catch (DivideByZeroException ex)
			{
				context.Response.StatusCode = 401;
				await context.Response.WriteAsync(ex.Message);
			}
			catch (Exception)
			{
				context.Response.StatusCode = 500;
				await context.Response.WriteAsync("Internal Server Error");
			}
		}
	}
}

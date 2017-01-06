using Elmah;
using System;
using System.Reflection;
using System.Threading;
using System.Web;

namespace dreamlet.Utilities
{
	public static class DreamletLogger
	{
		public static void LogError(Exception ex)
		{
			try
			{
				if (HttpContext.Current != null)
					ErrorSignal.FromCurrentContext().Raise(ex);
				else
				{
					ErrorLog errorLog = ErrorLog.GetDefault(null);
					string sourceName = String.Empty;
					Assembly asm = Assembly.GetEntryAssembly();

					if (asm == null)
						asm = Assembly.GetExecutingAssembly();
					else
						sourceName = asm.GetName().Name;

					errorLog.ApplicationName = sourceName;
					errorLog.Log(new Error(ex));
				}
			}
			catch
			{
			}
		}

		public static string GetFormatedExceptionMessage(Exception ex, bool isInnerException = false, string prependErrorMessage = "")
		{
			prependErrorMessage += isInnerException ? "[INNER EXCEPTION]" : "[BASE EXCEPTION]";
			prependErrorMessage += Environment.NewLine + "****************" + Environment.NewLine;

			prependErrorMessage += String.Format(
				"USER:          {0}" + Environment.NewLine + Environment.NewLine +
				"DATETIME:      {1}" + Environment.NewLine + Environment.NewLine +
				"MESSAGE:       {2}" + Environment.NewLine + Environment.NewLine +
				"STACK TRACE:   {3}" + Environment.NewLine + Environment.NewLine +
				"RAW URL:       {4}",

				Thread.CurrentPrincipal.Identity.Name,
				DateTime.UtcNow.ToString(),
				ex.Message,
				ex.StackTrace,
				HttpContext.Current.Request.RawUrl);

			prependErrorMessage += Environment.NewLine + "__________________" + Environment.NewLine + Environment.NewLine;

			if (ex.InnerException != null)
				return GetFormatedExceptionMessage(ex.InnerException, true, prependErrorMessage);
			else
				return prependErrorMessage;
		}
	}
}

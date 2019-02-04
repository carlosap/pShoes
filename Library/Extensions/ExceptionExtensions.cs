using System;
using System.Diagnostics;
namespace Library.Extensions
{
    public static class ExceptionExtensions
    {
        public static int LineNumber(this Exception error)
        {
            int result;
            try
            {
                var st = new StackTrace(error, true);
                var frame = st.GetFrame(0);
                result = frame.GetFileLineNumber();
            }
            catch (Exception)
            {
                result = -1;
            }
            return result;
        }
    }

}




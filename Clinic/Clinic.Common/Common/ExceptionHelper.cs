using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic.Common.Common
{
    public static class ExceptionHelper
    {
        public static string GetFullText(Exception _Exception, bool _NeedStackTrace)
        {
            if (_Exception == null)
                return "";

            string errorMessage = "";
            Exception exc_cur = _Exception;
            int level = 0;
            for (int i = 0; i < 20; i++)
            {
                string str_cur = "";
                //for (int l = 0; l < level; l++)
                //    str_cur += "\t";
                str_cur += exc_cur.Message + "\r\n";

                errorMessage += str_cur;

                if (exc_cur.InnerException == null)
                    break;

                exc_cur = exc_cur.InnerException;
                level++;
            }

            if (_NeedStackTrace)
                errorMessage += "\r\n" + exc_cur.StackTrace;

            return errorMessage;
        }

        public static string GetStackTrace(Exception _Exception)
        {
            if (_Exception == null)
                return "";

            Exception exc_cur = _Exception;
            for (int i = 0; i < 20; i++)
            {
                if (exc_cur.InnerException == null)
                    break;
                exc_cur = exc_cur.InnerException;
            }

            return exc_cur.StackTrace ?? "";
        }
    }
}
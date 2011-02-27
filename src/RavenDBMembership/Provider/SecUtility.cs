using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenDBMembership.UserStrings;

namespace RavenDBMembership.Provider
{
    public class SecUtility
    {
        internal static bool ValidateParameter(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize)
        {
            if (param == null)
            {
                return !checkForNull;
            }
            param = param.Trim();
            return (((!checkIfEmpty || (param.Length >= 1)) && ((maxSize <= 0) || (param.Length <= maxSize))) && (!checkForCommas || !param.Contains(",")));
        }

        internal static void CheckParameter(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize, string paramName)
        {
            if (param == null)
            {
                if (checkForNull)
                {
                    throw new ArgumentNullException(paramName);
                }
            }
            else
            {
                param = param.Trim();
                if (checkIfEmpty && (param.Length < 1))
                {
                    throw new ArgumentException(SR.Parameter_can_not_be_empty_1.WithParameters(paramName), paramName);
                }
                if ((maxSize > 0) && (param.Length > maxSize))
                {
                    throw new ArgumentException(SR.Parameter_too_long_2.WithParameters(paramName, maxSize), paramName);
                }
                if (checkForCommas && param.Contains(","))
                {
                    throw new ArgumentException(SR.Parameter_can_not_contain_comma_1.WithParameters(paramName), paramName);
                }
            }
        }
    }
}

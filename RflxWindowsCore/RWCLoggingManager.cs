/*
 * @(#)RWFLoggingManager.cs 1.0 12/27/10
 *
 * Use of this code is subject to the terms and conditions of the contract
 * with Reflexis Systems, Inc. Unauthorized reproduction or distribution of
 * any material or programming content is prohibited.
 */


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

/**
 * Helper Class for the Logging related Servlets. 
 * <p>Compatible with Windows 10 : App Version 1.0 and above...</p>
 *
 * @author  Nikhil Menon
 * @version 1.0 7/20/2015
 * @since   7/20/2015
 */


namespace RflxWindowsCore
{
    public class RWCLogManager
    {
        //private static bool isDebugEnabled = false;

        public static void logErrMessage(string urlStr)
        {
            Debug.WriteLine(urlStr);
        }

        public static void logDebugMessage(string urlStr)
        {
            Debug.WriteLine(urlStr);
        }

      
    }
}

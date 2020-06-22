/*
 * @(#)RWFUtilities.cs 1.0 12/27/10
 *
 * Use of this code is subject to the terms and conditions of the contract
 * with Reflexis Systems, Inc. Unauthorized reproduction or distribution of
 * any material or programming content is prohibited.
 */


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.System.Profile;
using static RflxWindowsCore.RWCConstants;
using System.Xml.Serialization;
using System.IO;
using Windows.UI;
using Windows.UI.Popups;
using Windows.Security.Cryptography.DataProtection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Windows.Storage;
using Windows.Media.Capture;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Data.Xml.Dom;

/**
 * Class for Common Utilities.
 * <p>Compatible with Windows 10 :Version 10.0.10586 and above...</p>
 *
 * @author  Nikhil Menon
 * @version 1.0.1 11/25/2015
 * @since   7/20/2015
 */


namespace RflxWindowsCore
{
    public class RWCUtilities
    {
        public static DayOfWeek startOfWeek = DayOfWeek.Sunday;
        public static bool isLogout = false;
        static bool inValidEmail = false;
        /*Check Email Validity*/

        public static bool IsValidEmail(string strIn)
        {

            if (String.IsNullOrEmpty(strIn))
                return false;
            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

            if (inValidEmail)
                return false;

            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
        private static string DomainMapper(Match match)
        {
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                inValidEmail = true;
            }
            return match.Groups[1].Value + domainName;
        }

        /*Check whether String is Valid HTTP URL*/
        public static bool CheckURLValidity(string url)
        {
            Uri uri;
            return Uri.TryCreate(url, UriKind.Absolute, out uri) && (uri.Scheme.Equals("http") || uri.Scheme.Equals("https"));
        }

        public static String getUrlParamStr(Dictionary<String, String> paramMap)
        {
            String paramStr = "";
            foreach (KeyValuePair<String, String> entry in paramMap)
            {
                paramStr += entry.Key + "=" + entry.Value + "&";
            }
            return paramStr.Substring(0, paramStr.Length - 1);
        }

        /*
        *Convert Date to Required Format
        */
        public static String convertToFormat(String _dateTime, String _format)
        {
            String formattedDate;
            DateTime _date = DateTime.Parse(_dateTime);
            //DateTime date = Convert.ToDateTime(_dateTime);
            formattedDate = _date.ToString(_format);
            return formattedDate;
        }

        /*
        *Convert Time to Required Format
        */
        public static String getFormattedTime(TimeSpan _timeSpan, bool isCapital)
        {
            var hours = _timeSpan.Hours;
            var minutes = _timeSpan.Minutes;
            String designator = "";
            if (hours == 0)
            {
                hours = 00;
                if (isCapital) designator = DateAndTime.AM;
                else designator = DateAndTime.am;
            }
            else if (hours == 12)
            {
                if (isCapital) designator = DateAndTime.AM;
                else designator = DateAndTime.am;
            }
            else if (hours > 12)
            {
                hours -= 12;
                if (isCapital) designator = DateAndTime.PM;
                else designator = DateAndTime.pm;
            }
            var formattedTime = String.Format("{0}:{1:00} {2}", hours, minutes, designator);
            return formattedTime;
        }

        public static String getStringFromList(HashSet<String> list)
        {
            StringBuilder str = new StringBuilder("-1,");
            if (list.Count > 0)
            {
                str = new StringBuilder();
                foreach (String flag in list)
                {
                    str = str.Append(flag + ",");
                }
            }
            return str.Remove(str.Length - 1, 1).ToString();
        }

        public static String getStringFromIntList(HashSet<int> list)
        {
            StringBuilder str = new StringBuilder("-1,");


            if (list.Count > 0)
            {
                str = new StringBuilder();

                foreach (int flag in list)
                {
                    str = str.Append(flag + ",");
                }
            }
            return str.Remove(str.Length - 1, 1).ToString();
        }


        //Returns The List of Children of the corresponding UI Elemenet
        public static List<UIElement> GetChildren(DependencyObject parent)
        {
            List<UIElement> listOfControls = new List<UIElement>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var _child = VisualTreeHelper.GetChild(parent, i);
                if (_child is UIElement)
                    listOfControls.Add(_child as UIElement);
                listOfControls.AddRange(GetChildren(_child));
            }
            return listOfControls;
        }
        /*
       *Get Specific Child by Name form Visualtree
       *@author Nikhil Menon
       *@created 09/16/2015
       */
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject control) where T: DependencyObject
        {
            if (control != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(control); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(control, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        public static String GetImagePath(Image _refImage)
        {
            BitmapImage _bmi = _refImage.Source as BitmapImage;
            Uri _uri = _bmi.UriSource;
            String _imagePath = _uri.AbsolutePath;
            return _imagePath;
        }
        /*
        *Get Get Week Number Of Month
        *@author Nikhil Menon
        *@created 11/12/2015
        */
        public static int GetWeekNumberOfMonth(DateTime date)
        {
            date = date.Date;
            DateTime _firstDay = new DateTime(date.Year, date.Month, 1);
            DateTime _firstMonday = _firstDay.AddDays((DayOfWeek.Monday + 7 - _firstDay.DayOfWeek) % 7);
            if (_firstMonday > date)
            {
                _firstDay = _firstDay.AddMonths(-1);
                _firstMonday = _firstDay.AddDays((DayOfWeek.Monday + 7 - _firstDay.DayOfWeek) % 7);
            }
            return (date - _firstMonday).Days / 7 + 1;
        }
        /*
        *Get First Date Of Week From Week Number
        *@author Nikhil Menon
        *@created 05/20/2016
        */
        //Returns First day of week considering week starts on a Sunday.
        public static DateTime FirstDateOfWeekFromWeekNumber(int year, int weekOfYear, System.Globalization.CultureInfo ci)
        {
            ci.DateTimeFormat.FirstDayOfWeek = DayOfWeek.Sunday;
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = (int)ci.DateTimeFormat.FirstDayOfWeek - (int)jan1.DayOfWeek;
            DateTime firstWeekDay = jan1.AddDays(daysOffset);
            int firstWeek = ci.Calendar.GetWeekOfYear(jan1, ci.DateTimeFormat.CalendarWeekRule, ci.DateTimeFormat.FirstDayOfWeek);
            if (firstWeek <= 1 || firstWeek > 50)
            {
                weekOfYear -= 1;
            }
            return firstWeekDay.AddDays(weekOfYear * 7);
        }
        public static int GetLastDate(DateTime _refDate)
        {
            DateTime dt = _refDate;
            while (dt.Month != _refDate.AddMonths(1).Month)
            {
                dt = dt.AddDays(1);
            }
            dt = dt.AddDays(-1);
            return dt.Day;
        }
        public static DateTime GetFirstDateOfWeek(DateTime _refDate)
        {
            CultureInfo cultInfo = CultureInfo.InvariantCulture;
            DayOfWeek _refDay = _refDate.DayOfWeek;
            DayOfWeek _firstDay = cultInfo.DateTimeFormat.FirstDayOfWeek;
            int _diff = _refDay - _firstDay;
            if (_diff == -1)
            {
                _diff = 7;
            }
            for (int i = 0; i < _diff; i++)
            {
                _refDate = _refDate.AddDays(-1);
            }
            return _refDate;
        }


        public static int GetWeekNumberOfYear(DateTime date)
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
            if (day >= DayOfWeek.Monday)
            {
                date = date.AddDays(0);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }

        /*
        *Append elements of one dictionary to another
        *@author Nikhil Menon
        *@created 12/31/2015
        */
        public static Dictionary<string, string> AppendDictionaries(Dictionary<string, string> parent, Dictionary<string, string> child)
        {
            foreach (KeyValuePair<string, string> kvPair in child)
            {
                parent.Add(kvPair.Key, kvPair.Value);
            }
            return parent;
        }

        /*
        *Get Scrollviewer from inside Element
        *@author Nikhil Menon
        *@created 12/31/2015
        */
        public static ScrollViewer GetScrollViewer(DependencyObject depObj)
        {
            if (depObj is ScrollViewer) return depObj as ScrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = GetScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }

        /*
        *Serialize Object to String
        *@author Nikhil Menon
        *@created 12/31/2015
        */
        public static String SerializeToString(Object _object)
        {
            XmlSerializer serializer = new XmlSerializer(_object.GetType());

            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, _object);
                return writer.ToString();
            }
        }

        /*
        *De-serialize Object to String
        *@author Nikhil Menon
        * @created 12/31/2015
        */
        public static Object DeserializeFromString(String _string)
        {
            XmlSerializer serializer = new XmlSerializer(_string.GetType());
            StringReader reader = new StringReader(_string);
            return serializer.Deserialize(reader);
        }

        /*
        Convert HexColour to SolidColourBrush
        *@author Nikhil Menon
        *@created 31/03/2016
        */
        public static SolidColorBrush GetColorFromHex(string hexColor)
        {
            try
            {
                int num;
                if (hexColor.StartsWith("#"))
                    hexColor = hexColor.Remove(0, 1);

                if (int.TryParse(hexColor, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out num))
                {
                    if (!hexColor.StartsWith("#"))
                    {
                        hexColor = "#" + hexColor;
                    }
                    byte r = Convert.ToByte(hexColor.Substring(1, 2), 16);
                    byte g = Convert.ToByte(hexColor.Substring(3, 2), 16);
                    byte b = Convert.ToByte(hexColor.Substring(5, 2), 16);
                    SolidColorBrush solidColorBrush = new SolidColorBrush(Color.FromArgb(0xFF, r, g, b));
                    return solidColorBrush;
                }
                else
                    return new SolidColorBrush(Colors.Black);
            }
            catch (Exception e)
            {
                RWCLogManager.logDebugMessage(e.Message);
                return new SolidColorBrush(Colors.Black);
            }
        }
        public static void ShowToastNotification(string title, string stringContent, int duration)
        {
            ToastNotifier ToastNotifier = ToastNotificationManager.CreateToastNotifier();
            Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText01);
            Windows.Data.Xml.Dom.XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
            toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode(stringContent));
            Windows.Data.Xml.Dom.IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            Windows.Data.Xml.Dom.XmlElement audio = toastXml.CreateElement("audio");
            ToastContent content = new ToastContent()
            {
                Launch = "Alert_Toast_Notification",
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText
                            {
                                Text = title
                            },
                            new AdaptiveText
                            {
                                Text = stringContent
                            }
                        }
                    }
                },

                Audio = new ToastAudio()
                {
                    Src = new Uri("ms-winsoundevent:Notification.SMS")
                },
            };
            audio.SetAttribute("src", "ms-winsoundevent:Notification.SMS");

            ToastNotification toast = new ToastNotification(content.GetXml());
            if (duration != 0) toast.ExpirationTime = DateTime.Now.AddSeconds(duration);
            else
            {
                toast.ExpirationTime = DateTime.Now.AddMinutes(5);
            }
           
            ToastNotifier.Show(toast);
        }
        /*
       *Show Alerts in a MessageDialog Box
       *@author Nikhil Menon
       *@created 1/04/2016
       */
        public async static void ShowMessage(String title, String text, [CallerMemberName]string name = "")
        {
            try
            {
                RWCLogManager.logDebugMessage(name);
                MessageDialog message = new MessageDialog(text);
                message.Title = title;
                message.Content = text;
                await message.ShowAsync();
            }
            catch (Exception e)
            {
                RWCLogManager.logDebugMessage(e.Message);
            }
        }
        /*
       *Show Login Failure Message in a MessageDialog Box
       *@author Nikhil Menon
       *@created 1/04/2016
       */
        public static void ShowLoginFailureAlert()
        {
            ShowMessage("Authentication Failure", "Please enter credentials");
            RflxWindowsCore.RWCLogManager.logDebugMessage("Please enter credentials");
        }

        public async Task<IBuffer> SampleProtectAsync(String strMsg, String strDescriptor, BinaryStringEncoding encoding)
        {
            // Create a DataProtectionProvider object for the specified descriptor.
            DataProtectionProvider Provider = new DataProtectionProvider(strDescriptor);

            // Encode the plaintext input message to a buffer.
            encoding = BinaryStringEncoding.Utf8;
            IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(strMsg, encoding);

            // Encrypt the message.
            IBuffer buffProtected = await Provider.ProtectAsync(buffMsg);

            // Execution of the SampleProtectAsync function resumes here
            // after the awaited task (Provider.ProtectAsync) completes.
            return buffProtected;
        }

        public async Task<String> SampleUnprotectData(IBuffer buffProtected, BinaryStringEncoding encoding)
        {
            // Create a DataProtectionProvider object.
            DataProtectionProvider Provider = new DataProtectionProvider();

            // Decrypt the protected message specified on input.
            IBuffer buffUnprotected = await Provider.UnprotectAsync(buffProtected);

            // Execution of the SampleUnprotectData method resumes here
            // after the awaited task (Provider.UnprotectAsync) completes
            // Convert the unprotected message from an IBuffer object to a string.
            String strClearText = CryptographicBuffer.ConvertBinaryToString(encoding, buffUnprotected);

            // Return the plaintext string.
            return strClearText;
        }
        /*
       *Strip String of basic html tags
       *@author Nikhil Menon
       *@created 25/08/2016
       */
        public static string StripBasicHTMLTags(String _parameter)
        {
            String result = Regex.Replace(_parameter, @"<[^>]*>", String.Empty);
            return result;
        }
        /*
       *Split and append url parameters to string seperated by ","
       *@author Nikhil Menon
       *@created 1/04/2016
       */
        public static string StringifyURLParameters(Dictionary<string, string> _params)
        {
            string urlString = "";
            foreach (KeyValuePair<String, String> param in _params)
            {
                String value = param.Value;
                String key = param.Key;

                if (String.IsNullOrEmpty(value))
                {
                    value = " ";
                }
                urlString += key + "=" + value + "&";
            }
            return urlString;
        }

        public static string CreateURLString(string serverURL, string servletPath, Dictionary<string, string> paramsMap)
        {
            string response = serverURL + servletPath + "?" + StringifyURLParameters(paramsMap);
            return response;
        }
        ////////////Method to get device ID
        private string GetDeviceID()
        {
            HardwareToken token = HardwareIdentification.GetPackageSpecificToken(null);
            IBuffer hardwareId = token.Id;

            HashAlgorithmProvider hasher = HashAlgorithmProvider.OpenAlgorithm("MD5");
            IBuffer hashed = hasher.HashData(hardwareId);

            string hashedString = CryptographicBuffer.EncodeToHexString(hashed);
            return hashedString;
        }


        /*
        *Append url parameters to url separated by /
        *@author Abhishek Pundikar
        *@created 09/09/2016
        */
        public static String getServletUrl(String url, List<String> args)
        {
            String servletPath = "";
            int i = 0;
            string[] str = url.Split('$');

            foreach (String newStr in str)
            {
                servletPath += newStr;
                if (i < args.Count)
                {
                    servletPath += args[i];
                }
                i++;
            }

            return servletPath;
        }


        /* Writing bigger text content
        *@author Abhishek Pundikar
        *@created 09/09/2016
        */
        public static async void writeContentToTextFile(String stringToBeWritten, String fileName)
        {
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(file, stringToBeWritten);
        }

        /* Reading bigger text content
        *@author Abhishek Pundikar
        *@created 09/09/2016
        */
        public static async Task<String> readContentFromTextFile(String fileName)
        {

            try
            {
                StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
                StorageFile file = await folder.GetFileAsync(fileName);
                string fileContent;
                using (StreamReader sRead = new StreamReader(await file.OpenStreamForReadAsync()))
                    fileContent = await sRead.ReadToEndAsync();
                return fileContent;
            }
            catch (Exception)
            {

                throw;
            }
        }


        /* Getting Week Start Date
        *@author Abhishek Pundikar
        *@created 09/09/2016
        */
        public static DateTime StartOfWeek(DateTime date)
        {
            int diff = date.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }
            return date.AddDays(-1 * diff).Date;
        }


        /* Getting Week Start Date
        *@author Abhishek Pundikar
        *@created 09/09/2016
        */
        public static DateTime getWeekStrtDate(DateTime d)
        {
            DateTime date = StartOfWeek(d);
            return date;
        }


        /* Getting Week End Date
        *@author Abhishek Pundikar
        *@created 09/09/2016
        */
        public static DateTime getWeekEndDate(DateTime d)
        {
            DateTime date = StartOfWeek(d).AddDays(6);
            return date;
        }


        /* Getting Month start Date
        *@author Abhishek Pundikar
        *@created 09/09/2016
        */
        public static DateTime getMonthStrtDate(DateTime reqDt)
        {
            DateTime date = reqDt;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            return firstDayOfMonth;
        }


        /* Getting Month End Date
        *@author Abhishek Pundikar
        *@created 09/09/2016
        */
        public static DateTime getMonthEndDate(DateTime reqDt)
        {
            DateTime date = reqDt;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            return lastDayOfMonth;
        }


        /* Getting Week Start for fiscal calendar
        *@author Abhishek Pundikar
        *@created 09/09/2016
        */
        public static int getWeekStartDateFromFiscalCalendar(DateTime fiscalYearStartDate, DateTime currentYearStartDate, DateTime dateForWhichWeekNoIsRequired)
        {
            var culture = CultureInfo.CurrentCulture;
            var weekOfYear = culture.Calendar.GetWeekOfYear(dateForWhichWeekNoIsRequired.Add(currentYearStartDate - fiscalYearStartDate), culture.DateTimeFormat.CalendarWeekRule, culture.DateTimeFormat.FirstDayOfWeek);
            return weekOfYear;
        }


        /* Getting response on alert click
        *@author Abhishek Pundikar
        *@created 13/09/2016
        */
        public async static Task<bool> showInteractiveAlert(String title, String message)
        {
            //messageBox.Commands.Add(new UICommand("No", new UICommandInvokedHandler(CommandInvokedHandler)));
            //messageBox.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(CommandInvokedHandler)));
            //await messageBox.ShowAsync();
            //return isLogout;

            MessageDialog showDialog = new MessageDialog(message, title);
            showDialog.Commands.Add(new UICommand("Yes")
            {
                Id = 0
            });
            showDialog.Commands.Add(new UICommand("No")
            {
                Id = 1
            });

            showDialog.DefaultCommandIndex = 0;
            showDialog.CancelCommandIndex = 1;
            var result = await showDialog.ShowAsync();
            if ((int)result.Id == 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /* Getting response from alert click
        *@author Abhishek Pundikar
        *@created 13/09/2016
        */
        private static void CommandInvokedHandler(IUICommand command)
        {
            if (command.Label.Equals("No"))
            {
                isLogout = false;
            }
            else if (command.Label.Equals("Yes"))
            {
                isLogout = true;
            }
        }

        /* Getting timespan from minutes
        *@author Abhishek Pundikar
        *@created 22/09/2016
        */

        public static TimeSpan getTimeFromMinutes(int _minutes)
        {
            TimeSpan _time = new TimeSpan(0, 0, 0, 0);
            TimeSpan _addedTime = TimeSpan.FromMinutes(_minutes);
            TimeSpan _timeOfDay = _time.Add(_addedTime);
            return _timeOfDay;
        }

        /* Getting duration from minutes
        *@author Abhishek Pundikar
        *@created 22/09/2016
        */
        public static String getDuration(int minutes)
        {
            //Check with Nikhil 
            TimeSpan tempDuration = getTimeFromMinutes(minutes);
            String duration = tempDuration.ToString("hh") + ":" + tempDuration.ToString("mm");
            return duration;
        }

        /* Getting duartion from minutes
          *@author Abhishek Pundikar
          *@created 25/11/2016
        */
        public static String getTimeFromLongDate24Hours(String date)
        {
            if (!date.Equals("0"))
            {
                DateTime returnDate = DateTime.ParseExact(date, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None);
                return returnDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) + " " + returnDate.ToString("HH:mm");
            }
            else
            {
                return "";
            }
        }

        /* Getting duartion from minutes
          *@author Abhishek Pundikar
          *@created 25/11/2016
        */
        public static String getTimeFromLongDate12Hours(String date)
        {
            if (!date.Equals("0"))
            {
                DateTime returnDate = DateTime.ParseExact(date, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None);
                return returnDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) + " " + returnDate.ToString("hh:mm tt").ToLower();
            }
            else
            {
                return "";
            }
        }

        /* Getting duartion from minutes
          *@author Abhishek Pundikar
          *@created 23/01/2017
        */
        public static DateTime getDateTimeFromLongDate(String date)
        {
            if (!date.Equals("0"))
            {
                DateTime returnDate = DateTime.ParseExact(date, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None);
                return returnDate;
            }
            else
            {
                return new DateTime();
            }
        }


        /* Getting duartion from minutes
          *@author Abhishek Pundikar
          *@created 02/12/2016
        */
        public static DateTime EpochTimeConverter(string unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(Convert.ToDouble(unixTimeStamp)).ToLocalTime();
            return dtDateTime;
        }


        /* Capture Photos from the device camera
        *@author Nikhil Menon
        *@created 17/01/2017
        */
        public async static Task<StorageFile> CaptureFromCamera()
        {
            CameraCaptureUI _capture = new CameraCaptureUI();
            StorageFile _capturedImage = await _capture.CaptureFileAsync(CameraCaptureUIMode.Photo);
            return _capturedImage;
        }
    }

}

/*
 * @(#)RWCNetworkReachability.cs 1.0 12/22/15
 *
 * Use of this code is subject to the terms and conditions of the contract
 * with Reflexis Systems, Inc. Unauthorized reproduction or distribution of
 * any material or programming content is prohibited.
 */

using Windows.Networking.Connectivity;

/**
 * Network Reachability Class to check if network is available or not
 * <p>Compatible with Windows 10 : App Version 1.0 and above...</p>
 *
 * @author  Vamsi Koundinya
 * @version 1.0 12/22/2015
 * @since   12/22/2015
 */

namespace RflxWindowsCore
{
    public class RWCNetworkReachability
    {
        public static bool HasNetworkReachability;

        public  RWCNetworkReachability()
        {
            NetworkInformation.NetworkStatusChanged += NetworkInformationOnNetworkStatusChanged;
            CheckNetworkReachability();
        }

        private static  void NetworkInformationOnNetworkStatusChanged(object sender)
        {
            CheckNetworkReachability();
        }

        public static  bool CheckNetworkReachability()
        {
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            HasNetworkReachability = (connectionProfile != null && connectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess);
            return HasNetworkReachability;
        }
    }
}

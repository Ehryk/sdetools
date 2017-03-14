using System;
using log4net;
using ESRI.ArcGIS;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ADF.COMSupport;
using Miner.Interop;

namespace Core.ArcObjects
{
    public static class LicenseHelper
    {
        #region Private Properties

        private static readonly log4net.ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static IAoInitialize _AoInitialize;
        private static IMMAppInitialize _MMAppInit;

        #endregion

        #region Dual License Retrieval

        /// <summary>
        ///  Attempts to checkout an ArcEditor and ArcFM license and returns true if successful.
        /// </summary>
        public static bool GetLicenses()
        {
            RuntimeManager.Bind(ProductCode.Desktop);
            return (GetArcGISLicense() && GetArcFMLicense());
        }

        /// <summary>
        ///  Attempts to checkout an ArcEditor and ArcFM license and returns true if successful.
        /// </summary>
        public static bool GetLicenses(esriLicenseProductCode pESRIProductCode = esriLicenseProductCode.esriLicenseProductCodeAdvanced, mmLicensedProductCode pArcFMProductCode = mmLicensedProductCode.mmLPArcFM)
        {
            RuntimeManager.Bind(ProductCode.Desktop);
            return (GetArcGISLicense(pESRIProductCode) && GetArcFMLicense(pArcFMProductCode));
        }

        #endregion

        #region ESRI License Checkout

        public static bool BindProduct_Engine()
        {
            return BindProduct(ProductCode.Engine);
        }

        public static bool BindProduct_Desktop()
        {
            return BindProduct(ProductCode.Desktop);
        }

        public static bool BindProduct_EngineOrDesktop()
        {
            return BindProduct(ProductCode.EngineOrDesktop);
        }

        public static bool BindProduct(ProductCode pProductCode = ProductCode.Desktop)
        {
            return RuntimeManager.Bind(pProductCode);
        }

        /// <summary>
        ///  Attempts to checkout an ESRI Advanced license and returns true if successful.
        /// </summary>
        public static bool GetArcGISLicense_Advanced()
        {
            return GetArcGISLicense(esriLicenseProductCode.esriLicenseProductCodeAdvanced);
        }

        /// <summary>
        ///  Attempts to checkout an ESRI Standard license and returns true if successful.
        /// </summary>
        public static bool GetArcGISLicense_Standard()
        {
            return GetArcGISLicense(esriLicenseProductCode.esriLicenseProductCodeStandard);
        }

        /// <summary>
        ///  Attempts to checkout an ESRI Basic license and returns true if successful.
        /// </summary>
        public static bool GetArcGISLicense_Basic()
        {
            return GetArcGISLicense(esriLicenseProductCode.esriLicenseProductCodeBasic);
        }

        /// <summary>
        ///  Attempts to checkout an ESRI Engine license and returns true if successful.
        /// </summary>
        public static bool GetArcGISLicense_Engine()
        {
            return GetArcGISLicense(esriLicenseProductCode.esriLicenseProductCodeEngine);
        }

        /// <summary>
        ///  Attempts to checkout a license for the specified ESRI product and returns true if successful.
        /// </summary>
        public static bool GetArcGISLicense(esriLicenseProductCode? pESRIProductCode = esriLicenseProductCode.esriLicenseProductCodeAdvanced)
        {
            //Create a new AoInitialize object
            try
            {
                //The initialization object
                _AoInitialize = new AoInitializeClass();
            }
            catch
            {
                _log.Warn("Unable to initialize Arc Objects. License cannot be checked out.");
                return false;
            }

            if (_AoInitialize == null)
            {
                _log.Warn("Unable to initialize Arc Objects. License cannot be checked out.");
                return false;
            }

            //Determine if the product is available
            esriLicenseStatus licenseStatus = _AoInitialize.IsProductCodeAvailable(pESRIProductCode.Value);

            if (licenseStatus == esriLicenseStatus.esriLicenseAvailable)
            {
                licenseStatus = _AoInitialize.Initialize(pESRIProductCode.Value);

                if (licenseStatus != esriLicenseStatus.esriLicenseCheckedOut)
                {
                    _log.Warn(String.Format("The license checkout for {0} failed, status: {1}.", pESRIProductCode, licenseStatus));
                    return false;
                }

                return true;
            }
            else
            {
                _log.Warn(String.Format("The ArcGIS product {0} is unavailable.", pESRIProductCode));

                return false;
            }
        }

        #endregion

        #region ArcFM License Checkout

        /// <summary>
        ///  Attempts to checkout an ESRI Advanced license and returns true if successful.
        /// </summary>
        public static bool GetArcFMLicense_ArcFM()
        {
            return GetArcFMLicense(mmLicensedProductCode.mmLPArcFM);
        }

        /// <summary>
        ///  Attempts to checkout an ESRI Advanced license and returns true if successful.
        /// </summary>
        public static bool GetArcFMLicense_Designer()
        {
            return GetArcFMLicense(mmLicensedProductCode.mmLPDesigner);
        }

        /// <summary>
        /// Attempts to checkout a license for the specified Miner & Miner product and returns true if successful.
        /// </summary>
        public static bool GetArcFMLicense(mmLicensedProductCode pArcFMProductCode = mmLicensedProductCode.mmLPArcFM)
        {
            try
            {
                _MMAppInit = new MMAppInitializeClass();
            }
            catch
            {
                _log.Warn("Unable to initialize ArcFM. No licenses can be checked out.");
                return false;
            }

            if (_MMAppInit == null)
            {
                _log.Warn("Unable to initialize ArcFM. No licenses can be checked out.");
                return false;
            }

            //Determine if the product license is available or is already checked out
            mmLicenseStatus mmlicenseStatus = _MMAppInit.IsProductCodeAvailable(pArcFMProductCode);
            if (mmlicenseStatus == mmLicenseStatus.mmLicenseCheckedOut)
            {
                return true;
            }

            if (mmlicenseStatus == mmLicenseStatus.mmLicenseAvailable)
            {
                mmlicenseStatus = _MMAppInit.Initialize(pArcFMProductCode);

                if (mmlicenseStatus != mmLicenseStatus.mmLicenseCheckedOut)
                {
                    _log.Warn(String.Format("A license cannot be checked out for M&M product {0}", pArcFMProductCode));
                    return false;
                }

                return true;
            }
            else
            {
                _log.Warn(String.Format("No license is available for M&M product {0}", pArcFMProductCode));

                return false;
            }
        }

        #endregion

        #region License Releasing

        /// <summary>
        /// Releases ArcGIS and ArcFM licenses.
        /// </summary>
        public static void ReleaseLicenses()
        {
            //Release COM objects and shut down the AoInitilaize object
            AOUninitialize.Shutdown();

            _MMAppInit?.Shutdown();
            _AoInitialize?.Shutdown();
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using log4net;
using ESRI.ArcGIS.Geodatabase;

namespace Core.ArcObjects
{
    public static class DomainExtensions
    {
        #region Private Properties

        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region IWorkspace Extensions

        public static IDomain GetDomain(this IWorkspace workspace, string name)
        {
            IWorkspaceDomains workspaceDomains = (IWorkspaceDomains)workspace;
            IDomain domain = workspaceDomains.get_DomainByName(name);
            return domain;
        }

        public static ICodedValueDomain GetCodedValueDomain(this IWorkspace workspace, string name)
        {
            IWorkspaceDomains workspaceDomains = (IWorkspaceDomains)workspace;
            IDomain domain = workspaceDomains.get_DomainByName(name);
            ICodedValueDomain codedValueDomain = (ICodedValueDomain)domain;
            return codedValueDomain;
        }

        public static ICodedValueDomain2 GetCodedValueDomain2(this IWorkspace workspace, string name)
        {
            IWorkspaceDomains2 workspaceDomains = (IWorkspaceDomains2)workspace;
            IDomain domain = workspaceDomains.DomainByName[name];
            ICodedValueDomain2 codedValueDomain = (ICodedValueDomain2)domain;
            return codedValueDomain;
        }

        #endregion

        #region ICodedValueDomain Extensions

        public static bool HasCode(this ICodedValueDomain pDomain, string pCode, bool pIgnoreCase = false)
        {
            if (pDomain is null)
                throw new ArgumentException("domain");
            
            for (int i = 0; i < pDomain.CodeCount; i++)
            {
                if (String.Compare(pDomain.Value[i].ToString(), pCode, pIgnoreCase) == 0)
                    return true;
            }

            //Not Found
            return false;
        }

        public static bool AddCodedValue(this ICodedValueDomain pDomain, string pCode, string pName, bool pLockSchema = true)
        {
            if (pDomain is null)
                throw new ArgumentException("pDomain");

            if (pDomain.HasCode(pCode))
                return false;

            ISchemaLock schemaLock = (ISchemaLock)pDomain;

            if (pLockSchema)
                schemaLock.ChangeSchemaLock(esriSchemaLock.esriExclusiveSchemaLock);

            pDomain.AddCode(pCode, pName);

            if (pLockSchema)
                schemaLock.ChangeSchemaLock(esriSchemaLock.esriSharedSchemaLock);

            return pDomain.HasCode(pCode);
        }

        public static Dictionary<string, string> ListCodedValues(this ICodedValueDomain pDomain)
        {
            if (pDomain is null)
                throw new ArgumentException("pDomain");

            Dictionary<string, string> values = new Dictionary<string, string>();

            for (int i = 0; i < pDomain.CodeCount; i++)
            {
                values.Add(pDomain.Value[i].ToString(), pDomain.Name[i]);
            }

            return values;
        }

        public static bool RemoveCodedValue(this ICodedValueDomain pDomain, string pCode, bool pLockSchema = true)
        {
            if (pDomain is null)
                throw new ArgumentException("pDomain");

            if (!pDomain.HasCode(pCode))
                return false;

            ISchemaLock schemaLock = (ISchemaLock)pDomain;

            if (pLockSchema)
                schemaLock.ChangeSchemaLock(esriSchemaLock.esriExclusiveSchemaLock);

            pDomain.DeleteCode(pCode);

            if (pLockSchema)
                schemaLock.ChangeSchemaLock(esriSchemaLock.esriSharedSchemaLock);

            return !pDomain.HasCode(pCode);
        }

        #endregion

        #region ICodedValueDomain2 Extensions

        public static bool OrderCodedValue(this ICodedValueDomain2 pDomain, bool pByValue = true, bool pDescending = false, bool pLockSchema = true)
        {
            if (pDomain is null)
                throw new ArgumentException("pDomain");
            
            ISchemaLock schemaLock = (ISchemaLock)pDomain;

            if (pLockSchema)
                schemaLock.ChangeSchemaLock(esriSchemaLock.esriExclusiveSchemaLock);

            if (pByValue)
                (pDomain).SortByValue(pDescending);
            else
                (pDomain).SortByName(pDescending);

            if (pLockSchema)
                schemaLock.ChangeSchemaLock(esriSchemaLock.esriSharedSchemaLock);

            return true;
        }

        #endregion
    }
}

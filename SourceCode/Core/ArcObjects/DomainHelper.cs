using System;
using ESRI.ArcGIS.Geodatabase;

namespace Core.ArcObjects
{
    public static class DomainHelper
    {
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

        public static bool AddCodedValue(this ICodedValueDomain pDomain, string pCode, string pName)
        {
            if (pDomain is null)
                throw new ArgumentException("pDomain");

            pDomain.AddCode(pCode, pName);

            return pDomain.HasCode(pCode);
        }

        public static bool RemoveCodedValue(this ICodedValueDomain pDomain, string pCode)
        {
            if (pDomain is null)
                throw new ArgumentException("pDomain");

            pDomain.DeleteCode(pCode);

            return !pDomain.HasCode(pCode);
        }
    }
}

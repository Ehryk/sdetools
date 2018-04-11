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

        public static IWorkspaceDomains GetWorkspaceDomain(this IWorkspace pWorkspace)
        {
            var workspaceDomains = workspace as IWorkspaceDomains;
            if (workspaceDomains == null)
                throw new ArgumentException("Given workspace does not support domains.");
            return workspaceDomains;
        }

        public static IDomain GetDomain(this IWorkspace pWorkspace, string pName, bool pRaiseError = false, bool pCaseInvariant = false)
        {
            IWorkspaceDomains workspaceDomains = pWorkspace.GetWorkspaceDomain();
            IDomain domain = workspaceDomains.DomainByName[pName];

            if (pCaseInvariant && domain == null)
            {
                //We'll do a second sweep of the domains to see if there is a case mismatch
                var domainEnum = workspaceDomains.Domains;
                domainEnum.Reset();
                for (var d = domainEnum.Next(); d != null; domain = d.Next())
                {
                    if (!String.Equals(pName, domain.Name, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    domain = d;
                    break;
                }
            }

            if(pRaiseError && domain == null)
                throw new Exception(String.Format("Given domain does not exist in the geodatabase: {0}", pName));

            return domain;
        }

        public static ICodedValueDomain GetRangeDomain(this IWorkspace pWorkspace, string pName)
        {
            IWorkspaceDomains workspaceDomains = (IWorkspaceDomains)pWorkspace;
            IDomain domain = workspaceDomains.DomainByName[pName];
            IRangeDomain rangeDomain = (IRangeDomain)domain;
            return codedValueDomain;
        }

        public static ICodedValueDomain GetCodedValueDomain(this IWorkspace pWorkspace, string pName)
        {
            IWorkspaceDomains workspaceDomains = (IWorkspaceDomains)pWorkspace;
            IDomain domain = workspaceDomains.DomainByName[pName];
            ICodedValueDomain codedValueDomain = (ICodedValueDomain)domain;
            return codedValueDomain;
        }

        public static ICodedValueDomain2 GetCodedValueDomain2(this IWorkspace pWorkspace, string pName)
        {
            IWorkspaceDomains workspaceDomains = (IWorkspaceDomains)pWorkspace;
            IDomain domain = workspaceDomains.DomainByName[pName];
            ICodedValueDomain2 codedValueDomain2 = (ICodedValueDomain2)domain;
            return codedValueDomain2;
        }

        #endregion

        #region Query Domain

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

        /// <summary>
        /// Generates a dictionary lookup for the given coded value domain from the geodatabase using the aliased value
        /// </summary>
        /// <param name="workspace">ESRI geodatabase</param>
        /// <param name="domainName">Name of the coded value domain</param>
        /// <returns>Dictionary lookup of codes/values</returns>
        public static Dictionary<string, object> GetDomainReverseDictionary(this IWorkspace workspace, string domainName)
        {
            var workspaceDomains = workspace as IWorkspaceDomains;
            if (workspaceDomains == null)
                throw new ArgumentException("Given workspace does not support domains.");

            var givenDomain = workspaceDomains.DomainByName[domainName];
            if (givenDomain == null)
            {
                //We'll do a second sweep of the domains to see if there is an upper case issue
                var domainEnum = workspaceDomains.Domains;
                domainEnum.Reset();
                for (var domain = domainEnum.Next();
                    domain != null;
                    domain = domainEnum.Next())
                {
                    if (!string.Equals(domainName, domain.Name, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    givenDomain = domain;
                    break;
                }
            }

            //Still!
            if(givenDomain == null)
                throw new Exception("Given domain does not exist in the geodatabase: " + domainName);

            var codedDomain = givenDomain as ICodedValueDomain;
            if (codedDomain == null)
                throw new Exception("Given domain is not a coded value domain :" + domainName);

            var codeLookup = new Dictionary<string, object>();

            var codeCount = codedDomain.CodeCount;
            for (var i = 0; i < codeCount; i++)
                codeLookup[codedDomain.Name[i]] = codedDomain.Value[i];

            return codeLookup;
        }

        /// <summary>
        /// Generates a dictionary lookup for the given coded value domain from the geodatabase using the geodatabase value
        /// </summary>
        /// <param name="workspace">ESRI geodatabase</param>
        /// <param name="domainName">Name of the coded value domain</param>
        /// <returns>Dictionary lookup of codes/values</returns>
        public static Dictionary<object, string> GetDomainDictionary(this IWorkspace workspace, string domainName)
        {
            var workspaceDomains = workspace as IWorkspaceDomains;
            if (workspaceDomains == null)
                throw new ArgumentException("Given workspace does not support domains.");

            var givenDomain = workspaceDomains.DomainByName[domainName];
            if (givenDomain == null)
                throw new Exception("Given domain does not exist in the geodatabase: " + domainName);

            var codedDomain = givenDomain as ICodedValueDomain;
            if (codedDomain == null)
                throw new Exception("Given domain is not a coded value domain :" + domainName);

            var codeLookup = new Dictionary<object, string>();

            var codeCount = codedDomain.CodeCount;
            for (var i = 0; i < codeCount; i++)
                codeLookup[codedDomain.Value[i]] = codedDomain.Name[i];

            return codeLookup;
        }

        public static IDictionary<int,IDictionary<int, IDictionary<object, string>>> GetSubtypeFieldDomainDictionary(
            this IObjectClass objectClass, IEnumerable<int> fieldIndices)
        {
            var results = new Dictionary<int, IDictionary<int, IDictionary<object, string>>>()
            {
                {-1, new Dictionary<int, IDictionary<object, string>>()}
            };

            var loadedDomains = new Dictionary<string,IDictionary<object,string>>();
            var objectWorkspace = objectClass.GetWsFromObjectClass();

            var subtypeNames = new Dictionary<object, string>();
            var allSubtypes = new List<int>();
            var classSubtypes = objectClass as ISubtypes;
            var subtypeEnumerable = classSubtypes != null && classSubtypes.HasSubtype ? classSubtypes.Subtypes : null;
            if (subtypeEnumerable != null)
                {
                    int currentSubtype;
                    subtypeEnumerable.Reset();
                    for (var subtypeName= subtypeEnumerable.Next(out currentSubtype);
                        !string.IsNullOrEmpty(subtypeName);
                        subtypeName = subtypeEnumerable.Next(out currentSubtype))
                    {
                        results[currentSubtype] = new Dictionary<int, IDictionary<object, string>>();
                        allSubtypes.Add(currentSubtype);
                        subtypeNames[currentSubtype] = subtypeName;
                    }
                }
            
            var classFields = objectClass.Fields;
            foreach (var fieldIndex in fieldIndices)
            {
                var field = classFields.Field[fieldIndex];
                var fieldDomain = field.Domain;
                string fieldName = field.Name;

                if (fieldDomain != null)
                {
                    var domainName = fieldDomain.Name;
                    IDictionary<object, string> domainDictionary;
                    if (!loadedDomains.TryGetValue(domainName, out domainDictionary))
                        loadedDomains[domainName] = objectWorkspace.GetDomainDictionary(domainName);

                    results[-1][fieldIndex] = domainDictionary;
                }

                if (classSubtypes != null)
                {
                    if (fieldIndex == classSubtypes.SubtypeFieldIndex)
                    {
                        //Let's alias subtypes!
                        foreach (var subtype in allSubtypes)
                            results[subtype][fieldIndex] = subtypeNames;
                    }
                    else
                    {
                        //It's a normal field that may or may not be domained
                        foreach (var subtype in allSubtypes)
                        {
                            var subtypeDomain = classSubtypes.Domain[subtype, fieldName];
                            if (subtypeDomain != null)
                            {
                                var domainName = subtypeDomain.Name;
                                IDictionary<object, string> domainDictionary;
                                if (!loadedDomains.TryGetValue(domainName, out domainDictionary))
                                    domainDictionary = loadedDomains[domainName] = objectWorkspace.GetDomainDictionary(domainName);

                                results[subtype][fieldIndex] = domainDictionary;
                            }
                        }
                    }
                }
            }

            return results;
        }

        public static object GetDomainedValue(this IObject row, int fieldIndex,
            IDictionary<int, IDictionary<int, IDictionary<object, string>>> subtypeFieldDomainDictionary)
        {
            var rawValue = row.Value[fieldIndex];

            int currentSubtype = -1;
            var rowSubtypes = row as IRowSubtypes;
            if (rowSubtypes != null)
                currentSubtype = rowSubtypes.SubtypeCode;

            IDictionary<int, IDictionary<object, string>> fieldDomainDictionary;

            //Check to see if this subtype is valid.  If not, there is no domain (and we've got bigger problems that looking up domains).
            if (!subtypeFieldDomainDictionary.TryGetValue(currentSubtype, out fieldDomainDictionary) ||
                fieldDomainDictionary == null)
                return rawValue;

            //Check to see if this field is domained for the current subtype
            IDictionary<object, string> domainDictionary;
            if (!fieldDomainDictionary.TryGetValue(fieldIndex, out domainDictionary) ||
                domainDictionary == null)
                return rawValue;

            //Check to see if this is a valid domain value
            string valueDescription;
            return domainDictionary.TryGetValue(rawValue, out valueDescription) ? 
                valueDescription: rawValue;
        }

        #endregion

        #region Manage Domain

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

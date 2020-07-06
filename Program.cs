using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace lccADGroupMaintain
{
    class lccADGroupMaintainClass
    {
        static lccSettingsClass lccSCSettings = new lccSettingsClass();
        static void Main(string[] lccParamALArgs)
        {
            try
            {
                lccFLogInfo(0, "[Main] STARTED");
                lccSCSettings.lccALArgs = lccParamALArgs;
                if (lccSCSettings.lccBAbortProgram == false)
                {
                    lccFLogInfo(0, "Loading command line arguments.");
                    lccFLoadArgs();
                    if (lccSCSettings.lccBAbortProgram == false)
                    {
                        lccFLogInfo(0, "Loaded command line arguments.");
                    }
                }
                if (lccSCSettings.lccBAbortProgram == false)
                {
                    lccFLogInfo(0, "Loading Logic File: " + lccSCSettings.lccSLogicPath);
                    lccFLoadLogic(1, lccSCSettings.lccSLogicPath);
                    if (lccSCSettings.lccBAbortProgram == false)
                    {
                        lccFLogInfo(0, "LoadedLogic File: " + lccSCSettings.lccSLogicPath);
                    }
                }
                if (lccSCSettings.lccBAbortProgram == false)
                {
                    lccFLogInfo(0, "Processing Logic.");
                    lccFProcessLogic();
                    if (lccSCSettings.lccBAbortProgram == false)
                    {
                        lccFLogInfo(0, "Processed Logic.");
                    }
                }
                if (lccSCSettings.lccBAbortProgram == false)
                {
                    if (lccSCSettings.lccALLDAPServer.Count == 0)
                    {
                        lccSCSettings.lccBAbortProgram = true;
                        lccFLogInfo(0, "Please provide at least one lcc:LDAPServer");
                    }
                }
                if (lccSCSettings.lccBAbortProgram == false)
                {
                    if (lccSCSettings.lccALRequests.Count == 0)
                    {
                        lccSCSettings.lccBAbortProgram = true;
                        lccFLogInfo(0, "Please provide at least one lcc:requestId");
                    }
                }
                if (lccSCSettings.lccBAbortProgram == false)
                {
                    lccFProcessRequests();
                }
            }
            catch (Exception lccException)
            {
                lccFLogInfo(0, "[Main] ERROR: " + lccException.Message);
            }
            lccFLogInfo(0, "[Main] COMPLETED");
            lccFLogInfo(3, "");
        }
        static public void lccFProcessRequests()
        {
            bool lccBRequestProcessed = false;
            int lccIFunctionReturnVal = 0;
            int lccILDAPServersLoop = 0;
            int lccIRequestsLoop = 0;
            try
            {
                for (lccIRequestsLoop=0; lccIRequestsLoop<lccSCSettings.lccALRequests.Count; lccIRequestsLoop++)
                {
                    lccFLogInfo(0, "[lccFProcessRequests] Request Id [" + lccSCSettings.lccALRequests[lccIRequestsLoop].lccSId + "]");
                    lccFLogInfo(0, "[lccFProcessRequests] Request Skip [" + lccSCSettings.lccALRequests[lccIRequestsLoop].lccBSkip.ToString() + "]");
                    if (lccSCSettings.lccALRequests[lccIRequestsLoop].lccBSkip == false)
                    {
                        lccFLogInfo(0, "[lccFProcessRequests] Request Type [" + lccSCSettings.lccALRequests[lccIRequestsLoop].lccSType + "]");
                        if (lccSCSettings.lccALRequests[lccIRequestsLoop].lccSType.Equals("Add") == false
                        && lccSCSettings.lccALRequests[lccIRequestsLoop].lccSType.Equals("Remove") == false
                        && lccSCSettings.lccALRequests[lccIRequestsLoop].lccSType.Equals("Maintain") == false
                        )
                        {
                            lccFLogInfo(0, "[lccFProcessRequests] Request Type [" + lccSCSettings.lccALRequests[lccIRequestsLoop].lccSType + "] Not Supported, Skipping.");
                        }
                        else
                        {
                            lccFLogInfo(0, "[lccFProcessRequests] Request Start OU [" + lccSCSettings.lccALRequests[lccIRequestsLoop].lccSStartOU + "]");
                            if (lccSCSettings.lccALRequests[lccIRequestsLoop].lccSType.Equals("Add") == true
                                || lccSCSettings.lccALRequests[lccIRequestsLoop].lccSType.Equals("Remove") == true
                                || lccSCSettings.lccALRequests[lccIRequestsLoop].lccSType.Equals("Maintain") == true
                                )
                            {
                                lccFLogInfo(0, "[lccFProcessRequests] Type Supported [" + lccSCSettings.lccALRequests[lccIRequestsLoop].lccSType + "]");
                                lccIFunctionReturnVal = 0;
                                lccBRequestProcessed = false;
                                for (lccILDAPServersLoop = 0; lccILDAPServersLoop < lccSCSettings.lccALLDAPServer.Count && lccBRequestProcessed == false && lccIFunctionReturnVal < 2; lccILDAPServersLoop++)
                                {
                                    lccIFunctionReturnVal = lccFModifyGroup(lccSCSettings.lccALLDAPServer[lccILDAPServersLoop], lccSCSettings.lccALRequests[lccIRequestsLoop].lccSStartOU, lccSCSettings.lccALRequests[lccIRequestsLoop]);
                                    if (lccIFunctionReturnVal == 1)
                                    {
                                        lccBRequestProcessed = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception lccException)
            {
                lccFLogInfo(2, "[lccFProcessRequests] ERROR: " + lccException.Message);
            }
        }
        static lccLDAPObjectClass lccFFindLDAPobject(string lccParamSSearchOU, string lccParamSId, string lccParamSObjectType)
        {
            lccLDAPObjectClass lccReturn = new lccLDAPObjectClass();
            DirectoryEntry lccDESearchOU = null;
            DirectorySearcher lccDSSearcher = null;
            SearchResultCollection lccSRCCollection = null;
            ResultPropertyCollection lccRPCCollection = null;
            try
            {
                lccDESearchOU = new DirectoryEntry(lccParamSSearchOU);
                lccDSSearcher = null;
                lccDSSearcher = new DirectorySearcher(lccDESearchOU);
                lccDSSearcher.PageSize = 1;
                lccDSSearcher.SizeLimit = 1;
                //lccDSSearcher.PropertiesToLoad.Add("sAMAccountName");
                lccDSSearcher.PropertiesToLoad.Add("cn");
                if (lccParamSObjectType.Equals("Group") == true)
                {
                    lccDSSearcher.PropertiesToLoad.Add("member");
                }
                lccDSSearcher.PropertiesToLoad.Add("sn");
                lccDSSearcher.PropertiesToLoad.Add("distinguishedname");
                lccDSSearcher.SearchScope = SearchScope.Subtree;
                lccDSSearcher.Filter = "(samAccountName=" + lccParamSId + ")";
                if (lccDSSearcher.FindOne() != null)
                {
                    lccSRCCollection = lccDSSearcher.FindAll();

                    if (lccSRCCollection.Count == 0)
                    {
                        lccReturn.lccIReturnVal = 3;
                        lccFLogInfo(0, "[lccFFindLDAPobject] No objects returned from Active Directory for [" + lccParamSId + "]");
                    }
                    else
                    {
                        try
                        {
                            foreach (SearchResult aSearchResult in lccSRCCollection)
                            {
                                lccFLogInfo(0, "[lccFFindLDAPobject] Object Path: " + aSearchResult.Path);
                                lccRPCCollection = aSearchResult.Properties;
                                if (lccRPCCollection != null)
                                {
                                    foreach (String aPropertyName in lccRPCCollection.PropertyNames)
                                    {
                                        if (lccParamSObjectType.Equals("Group") == true)
                                        {
                                            if (aPropertyName.CompareTo("member") == 0)
                                            {
                                                foreach (object aCollection in lccRPCCollection[aPropertyName])
                                                {
                                                    lccFLogInfo(0, "[lccFModifyGroup] Found Member [" + aCollection.ToString() + "]");
                                                    lccReturn.lccALMembers.Add(aCollection.ToString());
                                                }
                                            }
                                        }
                                        /*
                                        if (aPropertyName.CompareTo("cn") == 0)
                                        {
                                            foreach (object aCollection in lccRPCCollection[aPropertyName])
                                            {
                                                lccFLogInfo(0, "[lccFModifyGroup] Found Object CN [" + aCollection.ToString() + "]");
                                            }
                                        }
                                        */
                                        if (aPropertyName.CompareTo("distinguishedname") == 0)
                                        {
                                            foreach (object aCollection in lccRPCCollection[aPropertyName])
                                            {
                                                lccFLogInfo(0, "[lccFFindLDAPobject] Found Object DN [" + aCollection.ToString() + "]");
                                                lccReturn.lccSDN = aCollection.ToString();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception lccExceptionCollectionSearchResults)
                        {
                            lccReturn.lccIReturnVal = 3;
                            lccFLogInfo(0, "[lccFFindLDAPobject] Object attributes failed to read.  ERROR: " + lccExceptionCollectionSearchResults.Message);
                        }
                    }
                }
                else
                {
                    lccReturn.lccIReturnVal = 3;
                    lccFLogInfo(0, "[lccFFindLDAPobject] No object found for [" + lccParamSId + "]");
                }
            }
            catch (Exception lccException)
            {
                lccFLogInfo(0, "[lccFFindLDAPobject] ERROR: "+ lccException.Message);
            }
            return lccReturn;
        }
        static int lccFModifyGroup(String lccParamSServer, String lccParamSStartOU, lccRequestClass lccParamRequest)
        {
            // lccIReturnVal
            // 0 - aborted
            // 1 - success
            // 2 - search OU issue, do not retry
            // 3 - user object issue, do not retry
            int lccIReturnVal = 0;
            bool lccBAbortFunction = false;
            bool lccBGroupMemberFound = false;
            int lccMaxRetries = lccSCSettings.lccILDAPMaxRetries;
            int lccRetries = 0;
            int lccIGroupsLoop = 0;
            int lccIGroupMembersLoop = 0;
            int lccIUsersLoop = 0;
            int lccIMaintainUsersLoop = 0;
            string lccSStartOU = "";
            lccLDAPObjectClass lccLDAPGroupObject = new lccLDAPObjectClass();
            lccLDAPObjectClass lccLDAPUserObject = new lccLDAPObjectClass();
            List<string> lccALGroupMembersDN = new List<string>();
            List<string> lccALUsersDN = new List<string>();
            DirectoryEntry lccDEAddToGroup = null;

            try
            {
                for (lccIGroupsLoop=0; lccIGroupsLoop<lccParamRequest.lccALGroups.Count; lccIGroupsLoop++)
                {
                    lccFLogInfo(0, "[lccFModifyGroup] Started Group [" + lccParamRequest.lccALGroups[lccIGroupsLoop] + "]");
                    lccRetries = 0;
                    lccALUsersDN.Clear();

                    do
                    {
                        lccRetries++;
                        try
                        {
                            lccFLogInfo(0, "[lccFModifyGroup] LDAP Server [" + lccParamSServer + "]");
                            lccSStartOU = "LDAP://" + lccParamSServer + "/" + lccParamSStartOU;
                            lccFLogInfo(0, "[lccFModifyGroup] LDAP Root [" + lccSStartOU + "]");

                            if (lccBAbortFunction == false)
                            {
                                try
                                {
                                    if (DirectoryEntry.Exists(lccSStartOU) == false)
                                    {
                                        lccBAbortFunction = true;
                                        lccIReturnVal = 2;
                                        lccFLogInfo(0, "[lccFModifyGroup] lcc:startOU does not exist: " + lccSStartOU);
                                    }
                                }
                                catch (Exception lccExceptionDirectoryEntryExists)
                                {
                                    lccBAbortFunction = true;
                                    lccIReturnVal = 2;
                                    lccFLogInfo(0, "[lccFModifyGroup] OU Check Exists Error.  Incorrect lcc:startOU value?  ERROR: " + lccExceptionDirectoryEntryExists.Message);
                                }
                            }

                            if (lccBAbortFunction == false)
                            {
                                lccLDAPGroupObject = lccFFindLDAPobject(lccSStartOU, lccParamRequest.lccALGroups[lccIGroupsLoop],"Group");
                                if (lccLDAPGroupObject.lccSDN.Length==0)
                                {
                                    lccBAbortFunction = true;
                                    lccIReturnVal = 2;
                                }
                            }

                            if (lccBAbortFunction == false)
                            {
                                lccFLogInfo(0, "[lccFModifyGroup] lcc:startOU exist: " + lccSStartOU);
                                for (lccIUsersLoop=0; lccIUsersLoop<lccParamRequest.lccALUsers.Count; lccIUsersLoop++)
                                {
                                    lccLDAPUserObject.lccFClearValues();
                                    lccFLogInfo(0, "[lccFModifyGroup] Search for user [" + lccParamRequest.lccALUsers[lccIUsersLoop] + "]");
                                    try
                                    {
                                        lccLDAPUserObject = lccFFindLDAPobject(lccSStartOU, lccParamRequest.lccALUsers[lccIUsersLoop],"User");
                                        if (lccLDAPUserObject != null)
                                        {
                                            if (lccLDAPUserObject.lccSDN.Length > 0)
                                            {
                                                lccALUsersDN.Add(lccLDAPUserObject.lccSDN);
                                            }
                                        }
                                    }
                                    catch (Exception lccExceptionSearcher)
                                    {
                                        lccIReturnVal = 3;
                                        lccFLogInfo(0, "[lccFModifyGroup] Searcher ERROR: " + lccExceptionSearcher.Message);
                                    }
                                }
                            }



                            if (lccBAbortFunction == false)
                            {
                                if (lccALUsersDN.Count==0)
                                {
                                    lccBAbortFunction = true;
                                    lccIReturnVal = 3;
                                    lccFLogInfo(0, "[lccFModifyGroup] No User Object DNs available.   Cannot process.");
                                }
                            }
                            if (lccBAbortFunction == false)
                            {
                                lccFLogInfo(0, "[lccFModifyGroup] "+ lccParamRequest.lccSType+" User to Group [Attempt: " + lccRetries.ToString() + " out of " + lccMaxRetries.ToString() + "] [" + lccLDAPGroupObject.lccSDN + "]");
                                if (DirectoryEntry.Exists("LDAP://" + lccLDAPGroupObject.lccSDN) == false)
                                {
                                    lccFLogInfo(0, "[lccADTasksPerform] lcc:searchOU does not exist: " + "LDAP://" + lccLDAPGroupObject.lccSDN);
                                }
                                else
                                {
                                    lccDEAddToGroup = new DirectoryEntry("LDAP://" + lccLDAPGroupObject.lccSDN);

                                    for (lccIUsersLoop=0; lccIUsersLoop<lccALUsersDN.Count; lccIUsersLoop++)
                                    {
                                        if (lccParamRequest.lccSType.Equals("Add") == true)
                                        {
                                            lccFLogInfo(0, "[lccADTasksPerform] " + lccParamRequest.lccSType + " User [" + lccALUsersDN[lccIUsersLoop] + "]");
                                            lccDEAddToGroup.Properties["member"].Add(lccALUsersDN[lccIUsersLoop]);
                                        }
                                        else if (lccParamRequest.lccSType.Equals("Remove") == true)
                                        {
                                            lccFLogInfo(0, "[lccADTasksPerform] " + lccParamRequest.lccSType + " User [" + lccALUsersDN[lccIUsersLoop] + "]");
                                            lccDEAddToGroup.Properties["member"].Remove(lccALUsersDN[lccIUsersLoop]);
                                        }
                                        else if (lccParamRequest.lccSType.Equals("Maintain") == true)
                                        {
                                            lccBGroupMemberFound = false;
                                            for (lccIGroupMembersLoop=0; lccIGroupMembersLoop<lccLDAPGroupObject.lccALMembers.Count && lccBGroupMemberFound==false; lccIGroupMembersLoop++)
                                            {
                                                lccBGroupMemberFound = lccLDAPGroupObject.lccALMembers[lccIGroupMembersLoop].Equals(lccALUsersDN[lccIUsersLoop]);
                                            }
                                            if (lccBGroupMemberFound == false)
                                            {
                                                lccFLogInfo(0, "[lccADTasksPerform] (Maintain) Add User [" + lccALUsersDN[lccIUsersLoop] + "]");
                                                lccDEAddToGroup.Properties["member"].Add(lccALUsersDN[lccIUsersLoop]);
                                            }
                                            else
                                            {
                                                lccFLogInfo(0, "[lccADTasksPerform] (Maintain) User Already Member [" + lccALUsersDN[lccIUsersLoop] + "]");
                                            }
                                            for (lccIGroupMembersLoop = 0; lccIGroupMembersLoop < lccLDAPGroupObject.lccALMembers.Count; lccIGroupMembersLoop++)
                                            {
                                                lccBGroupMemberFound = false;
                                                for (lccIMaintainUsersLoop=0; lccIMaintainUsersLoop< lccALUsersDN.Count; lccIMaintainUsersLoop++)
                                                {
                                                    lccBGroupMemberFound = lccALUsersDN[lccIMaintainUsersLoop].Equals(lccLDAPGroupObject.lccALMembers[lccIGroupMembersLoop]);
                                                }
                                                if (lccBGroupMemberFound == false)
                                                {
                                                    lccFLogInfo(0, "[lccADTasksPerform] (Maintain) Remove  User [" + lccLDAPGroupObject.lccALMembers[lccIGroupMembersLoop] + "]");
                                                    lccDEAddToGroup.Properties["member"].Remove(lccLDAPGroupObject.lccALMembers[lccIGroupMembersLoop]);
                                                }
                                            }
                                        }

                                    }
                                    lccDEAddToGroup.CommitChanges();
                                    lccDEAddToGroup.Close();
                                    lccIReturnVal = 1;
                                }
                                lccFLogInfo(0, "[lccFModifyGroup] Finished");
                            }
                        }
                        catch (Exception exception1)
                        {
                            lccFLogInfo(0, "[lccFModifyGroup] Failed on retry " + lccRetries.ToString() + " out of " + lccMaxRetries.ToString() + ".  ERROR: " + exception1.Message);
                            lccPauseProcess(2);
                        }
                        if (lccIReturnVal == 0)
                        {
                            lccPauseProcess(1);
                        }
                    } while (lccIReturnVal == 0 && lccRetries < lccMaxRetries);
                    if (lccIReturnVal == 0
                        && lccRetries == lccMaxRetries
                        )
                    {
                        lccFLogInfo(0, "[lccFModifyGroup] Failed with the maxium retries.");
                    }
                }
                if (lccSCSettings.lccBDebugMode == true)
                {
                    lccFLogInfo(0, "[lccFModifyGroup] Done");
                }
            }
            catch (Exception lccException)
            {
                lccFLogInfo(0, "[lccFModifyGroup] ERROR: "+lccException.Message);
            }
            return lccIReturnVal;
        }
        static private void lccPauseProcess(int howLong)
        {
            int lccITimeLeft = howLong * 1000;
            if (lccSCSettings.lccBAbortProgram == false)
            {
                System.Threading.Thread.Sleep(lccITimeLeft);
            }
        }
        static public bool lccFLoadArgs()
        {
            bool lccBReturn = false;
            int lccILoop = 0;
            int lccIOnArg = 0;
            try
            {
                for (lccILoop = 0; lccILoop < lccSCSettings.lccALArgs.Length; lccILoop++)
                {
                    if (lccSCSettings.lccBDebugMode == true)
                    {
                        Console.WriteLine("[lccFLoadArgs] Command Line Argument ["+ lccSCSettings.lccALArgs[lccILoop]+"]");
                    }
                    if (lccSCSettings.lccALArgs[lccILoop].Equals("lcc:logicPath") == true)
                    {
                        lccIOnArg = 1;
                    }
                    else if (lccSCSettings.lccALArgs[lccILoop].Equals("lcc:debugMode") == true)
                    {
                        lccIOnArg = 2;
                    }
                    else
                    {
                        switch (lccIOnArg)
                        {
                            case 1:
                                lccSCSettings.lccSLogicPath = lccSCSettings.lccALArgs[lccILoop];
                                if (lccSCSettings.lccBDebugMode == true)
                                {
                                    Console.WriteLine("[lccFLoadArgs] Logic Path Set [" + lccSCSettings.lccSLogicPath+"]");
                                }
                                break;
                            case 2:
                                if (lccSCSettings.lccALArgs[lccILoop].Equals("YES") == true)
                                {
                                    lccSCSettings.lccBDebugMode = true;
                                    Console.WriteLine("Debug Mode Enabled (per command line argument)");
                                }
                                break;
                        }
                        lccIOnArg = 0;
                    }
                }
            }
            catch (Exception lccException)
            {
                lccFLogInfo(2, "[lccFLoadArgs] ERROR: " + lccException.Message);
            }
            return lccBReturn;
        }

        static public bool lccFLoadLogic(int lccParamIFlag, string lccParamSPath)
        {
            // lccParamIFlag
            // 1 - regular
            bool lccBReturnVal = false;
            FileStream lccFSSourceFile = null;
            StreamReader lccSRSourceFile = null;
            String lccSSource = "";
            try
            {
                if (lccParamSPath.Length == 0)
                {
                    lccSCSettings.lccBAbortProgram = true;
                    lccFLogInfo(2, "[lccFLoadLogic] Please provide lcc:logicPath");

                }
                else if (File.Exists(lccParamSPath) == false)
                {
                    lccSCSettings.lccBAbortProgram = true;
                    lccFLogInfo(2, "[lccFLoadLogic] ERROR: lcc:logicPath [" + lccParamSPath + "] could not be read.");
                }
                else
                {
                    lccFSSourceFile = new FileStream(lccParamSPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                    lccSRSourceFile = new StreamReader(lccFSSourceFile);
                    while ((lccSSource = lccSRSourceFile.ReadLine()) != null)
                    {
                        lccSCSettings.lccALLogicRecords.Add(lccSSource.Split('\t'));
                    }
                    lccSRSourceFile.Close();
                    lccFSSourceFile.Close();
                }
                lccBReturnVal = true;
            }
            catch (Exception lccException)
            {
                lccSCSettings.lccBAbortProgram = true;
                lccFLogInfo(0, "[lccFLoadLogic] ERROR: " + lccException.Message);
            }
            return lccBReturnVal;
        }
        static public List<string> lccFLoadUsersFromFile(string lccParamSPath)
        {
            List<string> lccALReturn = new List<string>();
            FileStream lccFSSourceFile = null;
            StreamReader lccSRSourceFile = null;
            String lccSSource = "";
            try
            {
                if (lccParamSPath.Length == 0)
                {
                    lccSCSettings.lccBAbortProgram = true;
                    lccFLogInfo(2, "[lccFLoadUsersFromFile] Path cannot be empty.");

                }
                else if (File.Exists(lccParamSPath) == false)
                {
                    lccSCSettings.lccBAbortProgram = true;
                    lccFLogInfo(2, "[lccFLoadUsersFromFile] Path  [" + lccParamSPath + "] could not be read.");
                }
                else
                {
                    lccFSSourceFile = new FileStream(lccParamSPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    lccSRSourceFile = new StreamReader(lccFSSourceFile);
                    while ((lccSSource = lccSRSourceFile.ReadLine()) != null)
                    {
                        if (lccSSource.Trim().Length > 0)
                        {
                            lccALReturn.Add(lccSSource.Trim());
                        }
                    }
                    lccSRSourceFile.Close();
                    lccFSSourceFile.Close();
                }
            }
            catch (Exception lccException)
            {
                lccSCSettings.lccBAbortProgram = true;
                lccFLogInfo(0, "[lccFLoadUsersFromFile] ERROR: " + lccException.Message);
            }
            return lccALReturn;
        }
        static public bool lccFProcessLogic()
        {

            bool lccBReturnVal = false;
            int lccIRecordsLoop = 0;
            List<string[]> lccALRecords = null;
            List<string> lccALUsersFromFile = new List<string>();
            try
            {
                lccALRecords = lccSCSettings.lccALLogicRecords;
                for (lccIRecordsLoop = 0; lccIRecordsLoop < lccALRecords.Count; lccIRecordsLoop++)
                {
                    if (lccALRecords[lccIRecordsLoop].Length > 1)
                    {
                        if (lccSCSettings.lccBDebugMode == true)
                        {
                            Console.WriteLine("[lccFProcessLogic] Logic Key Name ["+ lccALRecords[lccIRecordsLoop][0]+"] Value ["+ lccALRecords[lccIRecordsLoop][1]+"]");
                        }
                        if (lccALRecords[lccIRecordsLoop][0].CompareTo("lcc:debugMode") == 0)
                        {
                            if (lccALRecords[lccIRecordsLoop][1].Equals("YES") == true)
                            {
                                lccSCSettings.lccBDebugMode = true;
                                Console.WriteLine("Debug Mode Enabled (per Logic File)");

                            }
                        }
                        if (lccALRecords[lccIRecordsLoop][0].CompareTo("lcc:debugLevel") == 0)
                        {
                            lccSCSettings.lccALDebugLevels.Add(lccALRecords[lccIRecordsLoop][1]);
                        }
                        if (lccALRecords[lccIRecordsLoop][0].CompareTo("lcc:logPath") == 0)
                        {
                            lccSCSettings.lccSLogPath = lccALRecords[lccIRecordsLoop][1];
                        }
                        if (lccALRecords[lccIRecordsLoop][0].CompareTo("lcc:LDAPServer") == 0)
                        {
                            lccSCSettings.lccALLDAPServer.Add(lccALRecords[lccIRecordsLoop][1]);
                        }
                        if (lccALRecords[lccIRecordsLoop][0].CompareTo("lcc:LDAPMaxRetries") == 0)
                        {
                            lccSCSettings.lccILDAPMaxRetries = int.Parse(lccALRecords[lccIRecordsLoop][1]);
                        }
                        if (lccALRecords[lccIRecordsLoop][0].CompareTo("lcc:requestId") == 0)
                        {
                            lccSCSettings.lccALRequests.Add(new lccRequestClass());
                            lccSCSettings.lccALRequests[lccSCSettings.lccALRequests.Count - 1].lccSId = lccALRecords[lccIRecordsLoop][1];
                        }
                        if (lccALRecords[lccIRecordsLoop][0].CompareTo("lcc:requestSkip") == 0)
                        {
                            if (lccALRecords[lccIRecordsLoop][1].Equals("YES") == true)
                            {
                                lccSCSettings.lccALRequests[lccSCSettings.lccALRequests.Count - 1].lccBSkip = true;
                            }
                        }
                        if (lccALRecords[lccIRecordsLoop][0].CompareTo("lcc:requestType") == 0)
                        {
                            lccSCSettings.lccALRequests[lccSCSettings.lccALRequests.Count - 1].lccSType = lccALRecords[lccIRecordsLoop][1];
                        }
                        if (lccALRecords[lccIRecordsLoop][0].CompareTo("lcc:requestStartOU") == 0)
                        {
                            lccSCSettings.lccALRequests[lccSCSettings.lccALRequests.Count - 1].lccSStartOU = lccALRecords[lccIRecordsLoop][1];
                        }
                        if (lccALRecords[lccIRecordsLoop][0].CompareTo("lcc:requestGroup") == 0)
                        {
                            lccSCSettings.lccALRequests[lccSCSettings.lccALRequests.Count - 1].lccALGroups.Add(lccALRecords[lccIRecordsLoop][1]);
                        }
                        if (lccALRecords[lccIRecordsLoop][0].CompareTo("lcc:requestUser") == 0)
                        {
                            lccSCSettings.lccALRequests[lccSCSettings.lccALRequests.Count - 1].lccALUsers.Add(lccALRecords[lccIRecordsLoop][1]);
                            //lccFLoadUsersFromFile(string lccParamSPath)
                        }
                        if (lccALRecords[lccIRecordsLoop][0].CompareTo("lcc:requestUserFromFile") == 0)
                        {
                            lccALUsersFromFile = lccFLoadUsersFromFile(lccALRecords[lccIRecordsLoop][1]);
                            if (lccALUsersFromFile.Count > 0)
                            {
                                foreach (string lccSUserLoop in lccALUsersFromFile)
                                {
                                    lccSCSettings.lccALRequests[lccSCSettings.lccALRequests.Count - 1].lccALUsers.Add(lccSUserLoop);
                                }
                            }
                            //
                        }
                    }
                }
                lccBReturnVal = true;
            }
            catch (Exception lccException)
            {
                lccFLogInfo(0, "[lccFProcessLogic] ERROR: " + lccException.Message);
            }
            return lccBReturnVal;
        }
        static public bool lccFLogInfo(int lccIFlag, String logStr)
        {
            // lccIFlag
            // 0 - console and log
            // 1 - only write to log
            // 2 - console only
            // 3 - flush
            bool lccBReturnVal = false;
            string lccSLogFilePath = "";
            StringBuilder lccSBLogAppendYearMonthStr = new StringBuilder();
            StringBuilder lccSBTargetRecord = new StringBuilder();
            StringBuilder lccSBLogConsole = new StringBuilder();
            FileStream lccFSLogFile = null;
            StreamWriter lccSWLogFile = null;
            FileShare lccFSFileShare = FileShare.ReadWrite;
            try
            {
                switch (lccIFlag)
                {
                    case 3:
                        if (lccSCSettings.lccSLogPath.Length > 0)
                        {
                            lccSBLogAppendYearMonthStr.Append(DateTime.Now.Year.ToString());
                            if (DateTime.Now.Month < 10)
                            {
                                lccSBLogAppendYearMonthStr.Append("0");
                            }
                            lccSBLogAppendYearMonthStr.Append(DateTime.Now.Month.ToString());
                            if (DateTime.Now.Day < 10)
                            {
                                lccSBLogAppendYearMonthStr.Append("0");
                            }
                            lccSBLogAppendYearMonthStr.Append(DateTime.Now.Day.ToString());
                            lccSLogFilePath = lccSCSettings.lccSLogPath + "-" + lccSBLogAppendYearMonthStr.ToString() + ".log";
                        }
                        break;
                }

                if (lccIFlag == 3)
                {
                    lccFSLogFile = new FileStream(lccSLogFilePath, FileMode.Append, FileAccess.Write, lccFSFileShare);
                    lccSWLogFile = new StreamWriter(lccFSLogFile);
                }


                else
                {
                    lccSBTargetRecord.Append(lccReturnDateString("YYYYMMDD"));
                    lccSBTargetRecord.Append("\t" + lccReturnDateString("HH:MM:SS.MS"));
                    lccSBLogConsole.Append(lccReturnDateString("HH:MM:SS.MS"));
                    lccSBTargetRecord.Append("\t");
                    lccSBLogConsole.Append("\t");
                    switch (lccIFlag)
                    {
                        case 0:
                        case 1:
                            break;
                    }
                    lccSBTargetRecord.Append(logStr);
                    lccSBLogConsole.Append(logStr);
                    switch (lccIFlag)
                    {
                        case 0:
                        case 2:
                            Console.WriteLine(lccSBLogConsole.ToString());
                            break;
                    }
                    if (lccIFlag == 0
                        || lccIFlag == 1
                        )
                    {
                        lccSCSettings.lccALLogRecords.Add(lccSBTargetRecord.ToString());
                    }
                }
                if (lccIFlag == 3)
                {
                    if (lccSCSettings.lccSLogPath.Length > 0)
                    {
                        foreach (string lccSLogLoop in lccSCSettings.lccALLogRecords)
                        {
                            lccSWLogFile.WriteLine(lccSLogLoop);
                        }
                        lccSCSettings.lccALLogRecords.Clear();
                        lccSWLogFile.Close();
                        lccFSLogFile.Close();
                    }
                }
                lccBReturnVal = true;
            }
            catch
            {
            }
            return lccBReturnVal;
        }
        static public string lccReturnDateString(string lccParamSFlag)
        {
            int lccIFlag = -1;
            // lccIFlag
            // 0 - return YYYYMM
            // 1 - return YYYYMM    [tab]   HH:MM:SS.MS
            // 2 - return YYYYMMDDHHMMSS
            // 3 - return YYYYMMDD
            // 4 - return YYYYMMDDHHMMSSMS
            // 5 - return HHMMSSMS
            // 6 - return YYMMDD
            // 7 - YY
            // 8 - return YYYY-MM-DD
            // 9 - return YYYY-MM-DDZ
            // 10 - return HH:MM:SS.MS
            string lccSReturnVal = "";
            DateTime lccDTNow = DateTime.Now;
            try
            {
                if (lccParamSFlag.Equals("YYYYMM") == true)
                {
                    lccIFlag = 0;
                }
                else if (lccParamSFlag.Equals("YYYYMM[tab]HH:MM:SS:MS") == true)
                {
                    lccIFlag = 1;
                }
                else if (lccParamSFlag.Equals("YYYYMMDDHHMMSS") == true)
                {
                    lccIFlag = 2;
                }
                else if (lccParamSFlag.Equals("YYYYMMDD") == true)
                {
                    lccIFlag = 3;
                }
                else if (lccParamSFlag.Equals("YYYYMMDDHHMMSSMS") == true)
                {
                    lccIFlag = 4;
                }
                else if (lccParamSFlag.Equals("HHMMSSMS") == true)
                {
                    lccIFlag = 5;
                }
                else if (lccParamSFlag.Equals("YYMMDD") == true)
                {
                    lccIFlag = 6;
                }
                else if (lccParamSFlag.Equals("YY") == true)
                {
                    lccIFlag = 7;
                }
                else if (lccParamSFlag.Equals("YYYY-MM-DD") == true)
                {
                    lccIFlag = 8;
                }
                else if (lccParamSFlag.Equals("YYYY-MM-DDZ") == true)
                {
                    lccIFlag = 9;
                }
                else if (lccParamSFlag.Equals("HH:MM:SS.MS") == true)
                {
                    lccIFlag = 10;
                }
                switch (lccIFlag)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                        if (lccIFlag == 6)
                        {
                            lccSReturnVal = lccDTNow.Year.ToString().Substring(2, 2);
                        }
                        else if (lccIFlag == 7)
                        {
                            lccSReturnVal = lccDTNow.Year.ToString().Substring(2, 2);
                        }
                        else
                        {
                            lccSReturnVal = lccDTNow.Year.ToString();
                        }
                        if (lccIFlag == 8
                            || lccIFlag == 9
                            )
                        {
                            lccSReturnVal += "-";
                        }
                        if (lccIFlag != 7)
                        {
                            if (lccDTNow.Month < 10)
                            {
                                lccSReturnVal += "0";
                            }
                            lccSReturnVal += lccDTNow.Month.ToString();
                        }
                        break;
                }
                switch (lccIFlag)
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 6:
                    case 8:
                    case 9:
                        if (lccIFlag == 8
                            || lccIFlag == 9
                            )
                        {
                            lccSReturnVal += "-";
                        }
                        if (lccDTNow.Day < 10)
                        {
                            lccSReturnVal += "0";
                        }
                        lccSReturnVal += lccDTNow.Day.ToString();
                        if (lccIFlag == 8
                            || lccIFlag == 9
                            )
                        {
                            lccSReturnVal += "Z";
                        }
                        break;
                }
                switch (lccIFlag)
                {
                    case 1:
                    case 2:
                    case 4:
                    case 5:
                    case 10:

                        if (lccDTNow.Hour < 10)
                        {
                            lccSReturnVal += "0";
                        }
                        lccSReturnVal += lccDTNow.Hour.ToString();
                        if (lccIFlag == 1
                            || lccIFlag == 10
                            )
                        {
                            lccSReturnVal += ":";
                        }
                        if (lccDTNow.Minute < 10)
                        {
                            lccSReturnVal += "0";
                        }
                        lccSReturnVal += lccDTNow.Minute.ToString();
                        if (lccIFlag == 1
                            || lccIFlag == 10
                            )
                        {
                            lccSReturnVal += ":";
                        }
                        if (lccDTNow.Second < 10)
                        {
                            lccSReturnVal += "0";
                        }
                        lccSReturnVal += lccDTNow.Second.ToString();
                        break;
                }
                switch (lccIFlag)
                {

                    case 4:
                    case 5:
                    case 10:
                        if (lccIFlag == 1
                            || lccIFlag == 10)
                        {
                            lccSReturnVal += ".";
                        }
                        if (lccDTNow.Millisecond < 10)
                        {
                            lccSReturnVal += "0";
                        }
                        lccSReturnVal += lccDTNow.Millisecond.ToString();
                        break;
                }
            }
            catch (Exception lccException)
            {
                lccFLogInfo(0, "[lccReturnDateString] ERROR: Reading File.\r\n");
                lccFLogInfo(0, "[lccReturnDateString]  Message: " + lccException.Message + "\r\n");
            }
            return lccSReturnVal;
        }
        public bool lccFCheckDebugLevel(string lccSParam)
        {
            bool lccBReturnVal = false;
            int lccILoop = 0;
            try
            {
                if (lccSCSettings.lccBDebugMode == true)
                {

                    for (lccILoop = 0; lccILoop < lccSCSettings.lccALDebugLevels.Count && lccBReturnVal == false; lccILoop++)
                    {
                        if (lccSCSettings.lccALDebugLevels[lccILoop].Equals(lccSParam) == true)
                        {
                            lccBReturnVal = true;
                        }
                    }
                }
            }
            catch (Exception lccException)
            {
                lccSCSettings.lccBAbortProgram = true;
                lccFLogInfo(0, "[lccFCheckDebugLevel] ERROR: "+ lccException.Message);
            }
            return lccBReturnVal;
        }

    }
    class lccLDAPObjectClass
    {
        public int lccIReturnVal = 0;
        public string lccSDN = "";
        public List<string> lccALMembers = new List<string>();
        public lccLDAPObjectClass()
        {
            lccFClearValues();
        }
        public void lccFClearValues()
        {
            lccIReturnVal = 0;
            lccSDN = "";
            lccALMembers.Clear();
        }
    }
    class lccRequestClass
    {
        public bool lccBSkip = false;
        public string lccSId = "";
        public string lccSType = "";
        public string lccSStartOU = "";
        public List<string> lccALGroups = new List<string>();
        public List<string> lccALUsers = new List<string>();
        public lccRequestClass()
        {
            lccFClearValues();
        }
        public void lccFClearValues()
        {
            lccBSkip = false;
            lccSId = "";
            lccSType = "";
            lccSStartOU = "";
            lccALGroups.Clear();
            lccALUsers.Clear();
        }
    }
    class lccSettingsClass
    {
        public bool lccBAbortProgram = false;
        public bool lccBDebugMode = false;
        public bool lccBShowLogic = false;
        public int lccILDAPMaxRetries = 0;
        public string lccSLogPath = "";
        public string lccSLogicPath = "";
        public string lccSMode = "";
        public string[] lccALArgs = null;
        public List<string> lccALDebugLevels = new List<string>();
        public List<string> lccALLogRecords = new List<string>();
        public List<string> lccALLDAPServer = new List<string>();
        public List<string[]> lccALLogicRecords = new List<string[]>();
        public List<lccRequestClass> lccALRequests = new List<lccRequestClass>();
        public lccSettingsClass()
        {
            lccFClearValues();
        }
        public void lccFClearValues()
        {
            lccBAbortProgram = false;
            lccBDebugMode = false;
            lccBShowLogic = false;
            lccILDAPMaxRetries = 3;
            lccSLogPath = "";
            lccSLogicPath = "";
            lccSMode = "";
            lccALArgs = null;
            lccALDebugLevels.Clear();
            lccALLogRecords.Clear();
            lccALLDAPServer.Clear();
            lccALLogicRecords.Clear();
            lccALRequests.Clear();
        }
    }
}

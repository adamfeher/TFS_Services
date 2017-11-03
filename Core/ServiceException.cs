using System;

namespace RainforestExcavator.Core
{
    /// <summary>
    /// Custom exception to indicate tool specific errors.
    /// </summary>
    public class ServiceException : Exception
    {
        public override string Message { get; }
        public Error ErrorCode { get; set; }
        public string AffectedId { get; set; }
        public ServiceException(Error errorCode, string affectedId = null, string message = null)
        {
            this.ErrorCode = errorCode;
            this.AffectedId = affectedId;
            this.Message = message ?? GetErrorMessage(errorCode);            
        }

        /// <summary>
        /// Returns the specific message that should be displayed for the given Error.
        /// </summary>
        public static string GetErrorMessage(Error error)
        {
            switch (error)
            {
                // Operation Invalid
                case Error.UpdateToTFSForbidden:
                    return "This test already exists in TFS, updating from Rainforest is forbidden."; 
                case Error.Undetermined:
                    return "An error occured that needs to be investigated by the tools team."; 

                // RF side is unsatisfactory
                case Error.RunNotComplete:
                    return "The results cannot be synced because the latest run is still in progress.";
                case Error.RunCompleted:
                    return "The run has already completed and the action can no longer be performed. Please sync.";
                case Error.RunAborted:
                    return "The run was aborted, results were not saved.";
                case Error.EmptyExpectedResult:
                    return "One or more steps have empty expected results.";
                case Error.RFTestDoesNotExist:
                    return "Rainforest test case with given ID was not found.";
                case Error.RFTestTooManyTags:
                    return "Rainforest test case contains multiple TFSID tags.";
                case Error.DefaultEnvironmentDoesNotExist:
                    return "Operation can not continue because there is no Rainforest environment set as default.";
                case Error.SmartFolderPairNotFound:
                    return "Error: There was no matching Smart Folder.";
                case Error.SmartFolderTooManyFound:
                    return "Error: There were multiple Smart Folders found.";
                case Error.SmartFolderDoesNotExist:
                    return "Error: Previous Smart Folder no longer found.";
                case Error.EnvironmentPairNotFound:
                    return "Error: There was no matching environment.";
                case Error.EnvironmentTooManyFound:
                    return "Error: There were multiple environments found.";
                case Error.SmartFolderAlreadyExists:
                    return "A SmartFolder with the same name already exists.";
                case Error.InvalidSmartFolderTagLogic:
                    return "The tag logic provided for the SmartFolder was invalid.";
                case Error.CustomVariableValueBlank:
                    return "One or more variable of the provided values are blank.";
                case Error.CustomVariableDoesNotExist:
                    return "The custom variable being modified does not exist.";
                case Error.CustomVariableNameDescBlank:
                    return "A custom variable cannot have a blank name or description.";
                case Error.CustomVariableSameName:
                    return "Custom variable has multipl fields with the same name.";
                //case Error.FailedRunStart:  // Was not forgotten, it provides own message
                case Error.FailedDelete:
                    return "Failed to delete test case from Rainforest.";

                // TFS side is unsatisfactory
                case Error.TFSTestDoesNotExist:
                    return "TFS test case with given ID was not found.";
                case Error.TFSSharedTestDoesNotExist:
                    return "TFS shared step with given ID was not found.";
                case Error.TFSTestTooManyTags:
                    return "TFS test case contains multiple RFID tags.";
                case Error.TFSTestPlanDoesNotExist:
                    return "TFS test plan with given ID was not found.";
                case Error.TFSAttachmentDoesNotExist:
                    return "TFS WorkItem attachment was not found.";

                // Tool condition was not met
                case Error.DestinationWasNotSelected:
                    return "There was no TFS suite selected as destination for this operation.";
                case Error.UnhandledProjectName:
                    return "The selected project is not able to be handled, please report to tool developers.";
                case Error.HeaderVarDoesNotExist:
                    return "One or more of the variables used in the dataseed file were not defined in the header.";
                case Error.FileInUse:
                    return "One or more dataseed files have been left open and could not be opened.";

                // DataSeed side is unsatisfactory
                case Error.HeaderTooShort:
                    return "Provided header was too short, must consist of UserType and NumExtraUsers at minimum.";
                case Error.HeaderLengthMismatch:
                    return "Provided number of header names did not match provided number of header values.";
                case Error.InvalidUserType:
                    return "Provided UserType did not match 'generic' or 'specific'.";
                case Error.InvalidExtraUserCount:
                    return "Provided number of extra users is not valid, please enter a number between 0 and 26 inclusive.";
                case Error.TabVarLegthMismatch:
                    return "Provided number of tabular variable names did not match provided number of values.";
                case Error.TabVarValuesMissing:
                    return "Tabular variable values are missing.";
                case Error.InvalidTabVarFormatting:
                    return "Provided tabular variable lines were improperly formatted and could not be parsed.";
                case Error.InvalidSpecificUserValues:
                    return "Provided Specific User tab variable values were out of order or insufficient.";
                case Error.InvalidUserVarUse:
                    return "A seed variable of type ${x.y} was not found or used for a UserType other than Generic.";

                default:
                    return "Error has no matching message";
            }
        }
    }
}

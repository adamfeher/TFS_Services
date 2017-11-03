namespace RainforestExcavator.Core
{
    public enum Error
    {
        // Operation invalid
        UpdateToTFSForbidden,
        Undetermined,

        // RF side is unsatisfactory
        RunNotComplete,
        RunCompleted,
        RunAborted,
        EmptyExpectedResult,
        RFTestDoesNotExist,
        RFTestTooManyTags,
        SmartFolderPairNotFound,
        SmartFolderTooManyFound,
        SmartFolderDoesNotExist,
        EnvironmentPairNotFound,
        EnvironmentTooManyFound,
        DefaultEnvironmentDoesNotExist,
        FailedRunStart,
        FailedDelete,
        SmartFolderAlreadyExists,
        InvalidSmartFolderTagLogic,
        CustomVariableValueBlank,
        CustomVariableDoesNotExist,
        CustomVariableNameDescBlank,
        CustomVariableSameName,

        // TFS side is unsatisfactory
        TFSSuiteDoesNotExist,
        TFSTestDoesNotExist,
        TFSSharedTestDoesNotExist,
        TFSTestTooManyTags,
        TFSTestPlanDoesNotExist,
        TFSAttachmentDoesNotExist,

        // Tool condition was not met
        DestinationWasNotSelected,
        UnhandledProjectName,
        HeaderVarDoesNotExist,
        FileInUse,

        // DataSeed is unsatisfactory
        HeaderTooShort,
        HeaderLengthMismatch,
        InvalidUserType,
        InvalidExtraUserCount,
        TabVarLegthMismatch,
        TabVarValuesMissing,
        InvalidTabVarFormatting,
        InvalidSpecificUserValues,
        InvalidUserVarUse,

        // No Error
        None
    }
}

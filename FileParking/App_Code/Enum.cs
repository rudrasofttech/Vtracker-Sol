namespace FileParking.Models
{
    public enum GeneralStatusType
    {
        Active = 0,
        Inactive = 1,
        Deleted = 2
    }

    public enum ParkedFileStatus
    {
        Uploading = 1,
        Failed = 2,
        Done = 3
    }

    public enum MemberPlanStatus
    {
        Active = 1,
        Inactive = 2,
        Expired = 3
    }
}

namespace FileParking
{
    public enum AlertType
    {
        Success,
        Warning,
        Error,
        Info
    }
}
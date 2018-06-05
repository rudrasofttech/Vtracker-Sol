namespace FileParking.Models
{
    public enum GeneralStatusType
    {
        Active = 0,
        Inactive = 1,
        Deleted = 2
    }

    public enum MemberTypeType
    {
        Admin = 1,
        Author = 2,
        Member = 3,
        Reader = 4
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